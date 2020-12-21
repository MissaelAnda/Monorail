using System;
using Monorail.Util;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Matrix2D = OpenTK.Mathematics.Matrix3x2;

namespace Monorail.ECS
{
    public class Transform2D
    {
        [Flags]
        enum DirtyType
        {
            Clean,

            Position,
            Scale,
            Rotation,

            LocalPosition,
            LocalScale,
            LocalRotation,

            WorldToLocal,
            WorldInverse,

            Global = Position | Scale | Rotation,
            Local = LocalPosition | LocalScale | LocalRotation,
        }

        public enum Component
        {
            Position,
            Scale,
            Rotation,
        }

        #region getters and setters

        /// <summary>
		/// the parent Transform of this Transform
		/// </summary>
		/// <value>The parent.</value>
		public Transform2D Parent
        {
            get => _parent;
            set => SetParent(value);
        }

        /// <summary>
		/// total children of this Transform
		/// </summary>
		/// <value>The child count.</value>
		public int ChildCount => _children.Count;

        /// <summary>
        /// World space position
        /// </summary>
        public Vector2 Position
        {
            get
            {
                Update();
                if (_dirty.HasFlag(DirtyType.Position))
                {
                    if (Parent == null)
                    {
                        _position = _localPosition;
                    }
                    else
                    {
                        Parent.Update();
                        _position.Transform(ref _localPosition, ref Parent._worldTransform);
                    }

                    _dirty &= ~DirtyType.Position;
                }

                return _position;
            }
            set => SetPosition(value);
        }

        /// <summary>
        /// World space scale (local + parents)
        /// </summary>
        public Vector2 Scale
        {
            get
            {
                Update();
                return _scale;
            }
            set => SetScale(value);
        }

        /// <summary>
        /// World space rotation in radians
        /// </summary>
        public float Rotation
        {
            get
            {
                Update();
                return _rotation;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetRotation(value);
        }

        /// <summary>
        /// World space rotation in degrees
        /// </summary>
        public float RotationDegrees
        {
            get => MathHelper.RadiansToDegrees(Rotation);
            set => SetRotationDegrees(value);
        }


        /// <summary>
        /// Local position relative to parent
        /// </summary>
        public Vector2 LocalPosition
        {
            get
            {
                Update();
                return _localPosition;
            }
            set => SetLocalPosition(value);
        }

        /// <summary>
        /// Local scale relative to parent
        /// </summary>
        public Vector2 LocalScale
        {
            get
            {
                Update();
                return _localScale;
            }
            set => SetLocalScale(value);
        }

        /// <summary>
        /// Local rotation relative to parent in radians
        /// </summary>
        public float LocalRotation
        {
            get
            {
                Update();
                return _localRotation;
            }
            set => SetLocalRotation(value);
        }

        /// <summary>
        /// Local rotation relative to parent in degrees
        /// </summary>
        public float LocalRotationDegrees
        {
            get => MathHelper.RadiansToDegrees(LocalRotation);
            set => SetLocalRotationDegrees(value);
        }

        public Matrix2D WorldInverseTransform
        {
            get
            {
                Update();
                if (_dirty.HasFlag(DirtyType.WorldInverse))
                {
                    _worldTransform.CreateInvert(out _worldInverseTransform);
                    _dirty &= ~DirtyType.WorldInverse;
                }

                return _worldInverseTransform;
            }
        }

        public Matrix2D LocalToWorldTransform
        {
            get
            {
                Update();
                return _worldTransform;
            }
        }


        public Matrix2D WorldToLocalTransform
        {
            get
            {
                if (_dirty.HasFlag(DirtyType.WorldToLocal))
                {
                    if (Parent == null)
                    {
                        _worldToLocalTransform = Matrix2DExt.Identity;
                    }
                    else
                    {
                        Parent.Update();
                        Parent._worldTransform.CreateInvert(out _worldToLocalTransform);
                    }

                    _dirty &= ~DirtyType.WorldToLocal;
                }

                return _worldToLocalTransform;
            }
        }

        #endregion

        Transform2D _parent;
        DirtyType _dirty;

        // value is automatically recomputed from the position, rotation and scale
        Matrix2D _localTransform;

        // value is automatically recomputed from the local and the parent matrices.
        Matrix2D _worldTransform = Matrix2DExt.Identity;
        Matrix2D _worldToLocalTransform = Matrix2DExt.Identity;
        Matrix2D _worldInverseTransform = Matrix2DExt.Identity;

        Matrix2D _rotationMatrix;
        Matrix2D _translationMatrix;
        Matrix2D _scaleMatrix;

        Vector2 _position = Vector2.Zero;
        Vector2 _scale = Vector2.One;
        float _rotation = 0f;

        Vector2 _localPosition = Vector2.Zero;
        Vector2 _localScale = Vector2.One;
        float _localRotation = 0f;

        List<Transform2D> _children = new List<Transform2D>();

        /// <summary>
		/// returns the Transform child at index
		/// </summary>
		/// <returns>The child.</returns>
		/// <param name="index">Index.</param>
		public Transform2D GetChild(int index)
        {
            return _children[index];
        }

        public void Update()
        {
            // Has any local variable dirty
            if (_dirty != DirtyType.Clean)
            {
                Parent?.Update();

                if ((int)(_dirty & DirtyType.Local) > 0)
                {
                    if (_dirty.HasFlag(DirtyType.LocalPosition))
                    {
                        Matrix2DExt.CreateTranslation(_localPosition.X, _localPosition.Y, out _translationMatrix);
                        _dirty &= ~DirtyType.LocalPosition;
                    }

                    if (_dirty.HasFlag(DirtyType.LocalRotation))
                    {
                        Matrix2D.CreateRotation(_localRotation, out _rotationMatrix);
                        _dirty &= ~DirtyType.LocalRotation;
                    }

                    if (_dirty.HasFlag(DirtyType.LocalScale))
                    {
                        Matrix2D.CreateScale(_localScale.X, _localScale.Y, out _scaleMatrix);
                        _dirty &= ~DirtyType.LocalScale;
                    }

                    Matrix2DExt.Mult(_scaleMatrix, _rotationMatrix, out _localTransform);
                    Matrix2DExt.Mult(_localTransform, _translationMatrix, out _localTransform);

                    if (Parent == null)
                    {
                        _worldTransform = _localTransform;
                        _rotation = _localRotation;
                        _scale = _localScale;
                        _dirty |= DirtyType.WorldInverse;
                    }
                }

                if (Parent != null)
                {
                    Matrix2DExt.Mult(_localTransform, Parent._worldTransform, out _worldTransform);

                    _rotation = _localRotation + Parent._rotation;
                    _scale = Parent._scale * _localScale;
                    _dirty |= DirtyType.WorldInverse;
                }

                _dirty |= DirtyType.Position | DirtyType.WorldToLocal;
            }
        }

        #region Fluent setters

        /// <summary>
		/// sets the parent Transform of this Transform
		/// </summary>
		/// <returns>The parent.</returns>
		/// <param name="parent">Parent.</param>
		public Transform2D SetParent(Transform2D parent)
        {
            if (_parent == parent)
                return this;

            if (_parent != null)
                _parent._children.Remove(this);

            if (parent != null)
                parent._children.Add(this);

            _parent = parent;
            SetDirty(DirtyType.Position);

            return this;
        }

        /// <summary>
		/// sets the position of the transform in world space
		/// </summary>
		/// <returns>The position.</returns>
		/// <param name="position">Position.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform2D SetPosition(Vector2 position)
        {
            if (position == _position)
                return this;

            _position = position;
            if (Parent != null)
                LocalPosition = Vector2Ext.Transform(_position, WorldToLocalTransform);
            else
                LocalPosition = position;

            _dirty |= DirtyType.Position;
            return this;
        }

        /// <summary>
		/// sets the global scale of the transform
		/// </summary>
		/// <returns>The scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform2D SetScale(Vector2 scale)
        {
            _scale = scale;
            if (Parent != null)
                LocalScale = Vector2.Divide(scale, Parent._scale);
            else
                LocalScale = scale;

            return this;
        }

        /// <summary>
		/// sets the global scale of the transform
		/// </summary>
		/// <returns>The scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform2D SetScale(float scale)
        {
            return SetScale(new Vector2(scale));
        }

        /// <summary>
		/// sets the rotation of the transform in world space in radians
		/// </summary>
		/// <returns>The rotation.</returns>
		/// <param name="radians">Radians.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform2D SetRotation(float radians)
        {
            _rotation = radians;
            if (Parent != null)
                LocalRotation = Parent.Rotation + radians;
            else
                LocalRotation = radians;

            return this;
        }

        /// <summary>
		/// sets the rotation of the transform in world space in degrees
		/// </summary>
		/// <returns>The rotation.</returns>
		/// <param name="radians">Radians.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform2D SetRotationDegrees(float degrees)
        {
            return SetRotation(MathHelper.DegreesToRadians(degrees));
        }


        /// <summary>
		/// sets the position of the transform relative to the parent transform. If the transform has no parent, it is the same
		/// as Transform.position
		/// </summary>
		/// <returns>The local position.</returns>
		/// <param name="localPosition">Local position.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform2D SetLocalPosition(Vector2 position)
        {
            if (position == _localPosition)
                return this;

            _localPosition = position;
            _dirty |= DirtyType.LocalPosition | DirtyType.LocalScale | DirtyType.LocalRotation;
            SetDirty(DirtyType.Position);

            return this;
        }

        /// <summary>
		/// sets the scale of the transform relative to the parent. If the transform has no parent, it is the same as Transform.scale
		/// </summary>
		/// <returns>The local scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform2D SetLocalScale(Vector2 scale)
        {
            _localScale = scale;
            SetDirty(DirtyType.Scale);
            SetDirty(DirtyType.Position);
            _dirty |= DirtyType.LocalScale;

            return this;
        }

        /// <summary>
		/// sets the scale of the transform relative to the parent. If the transform has no parent, it is the same as Transform.scale
		/// </summary>
		/// <returns>The local scale.</returns>
		/// <param name="scale">Scale.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform2D SetLocalScale(float scale)
        {
            return SetLocalScale(new Vector2(scale));
        }

        /// <summary>
		/// sets the the rotation of the transform relative to the parent transform's rotation. If the transform has no parent, it is the
		/// same as Transform.rotation
		/// </summary>
		/// <returns>The local rotation.</returns>
		/// <param name="radians">Radians.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform2D SetLocalRotation(float radians)
        {
            if (_localRotation == radians)
                return this;

            _localRotation = radians;

            SetDirty(DirtyType.Position);
            _dirty |= DirtyType.LocalPosition | DirtyType.LocalScale | DirtyType.LocalRotation;

            return this;
        }

        /// <summary>
		/// sets the the rotation of the transform relative to the parent transform's rotation. If the transform has no parent, it is the
		/// same as Transform.rotation
		/// </summary>
		/// <returns>The local rotation.</returns>
		/// <param name="radians">Radians.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform2D SetLocalRotationDegrees(float degree)
        {
            return SetLocalRotation(MathHelper.DegreesToRadians(degree));
        }

        #endregion

        /// <summary>
		/// rounds the position of the Transform
		/// </summary>
		public void RoundPosition()
        {
            Position = Vector2Ext.Round(_position);
        }

        public void CopyFrom(Transform2D transform)
        {
            _position = transform.Position;
            _localPosition = transform._localPosition;
            _rotation = transform._rotation;
            _localRotation = transform._localRotation;
            _scale = transform._scale;
            _localScale = transform._localScale;

            SetDirty(DirtyType.LocalPosition);
            SetDirty(DirtyType.LocalRotation);
            SetDirty(DirtyType.LocalScale);
        }

        /// <summary>
		/// sets the dirty flag on the enum and passes it down to our children
		/// </summary>
		/// <param name="dirtyFlagType">Dirty flag type.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        void SetDirty(DirtyType dirtyFlagType)
        {
            if ((_dirty & dirtyFlagType) == 0)
            {
                _dirty |= dirtyFlagType;

                // dirty our children as well so they know of the changes
                for (var i = 0; i < _children.Count; i++)
                    _children[i].SetDirty(dirtyFlagType);
            }
        }

        public override string ToString()
        {
            return string.Format(
                "[Transform: parent: {0}, position: {1}, rotation: {2}, scale: {3}, localPosition: {4}, localRotation: {5}, localScale: {6}]",
                Parent != null, Position, Rotation, Scale, LocalPosition, LocalRotation, LocalScale);
        }
    }
}
