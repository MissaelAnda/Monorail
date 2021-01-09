using Necs;
using System;
using Monorail.Util;
using OpenTK.Mathematics;
using System.Collections.Generic;
using Matrix2D = OpenTK.Mathematics.Matrix3x2;

namespace Monorail.ECS
{
    public class Transform2D
    {
        #region Public Properties

        public readonly Entity Entity;

        /// <summary>
        /// An action called whenever the Transform is modified
        /// </summary>
        public event Action OnChanged;

        /// <summary>
        /// Gets or Sets the Transform's Parent
        /// Modifying this does not change the World position of the Transform
        /// </summary>
        public Transform2D Parent
        {
            get => _parent;
            set => SetParent(value, true);
        }

        /// <summary>
        /// Gets or Sets the World Position of the Transform
        /// </summary>
        public Vector2 Position
        {
            get
            {
                if (_dirty)
                    Update();

                return new Vector2(_worldMatrix.M31, _worldMatrix.M32);
            }
            set => SetPosition(value);
        }

        /// <summary>
        /// Gets or Sets the X Component of the World Position of the Transform
        /// </summary>
        public float X
        {
            get => Position.X;
            set => SetPosition(value, Position.Y);
        }

        /// <summary>
        /// Gets or Sets the Y Component of the World Position of the Transform
        /// </summary>
        public float Y
        {
            get => Position.Y;
            set => SetPosition(Position.X, value);
        }

        /// <summary>
        /// Gets or Sets the Local Position of the Transform
        /// </summary>
        public Vector2 LocalPosition
        {
            get => new Vector2(_localMatrix.M31, _localMatrix.M32);
            set => SetLocalPosition(value);
        }

        /// <summary>
        /// Gets or Sets the World Scale of the Transform
        /// </summary>
        public Vector2 Scale
        {
            get
            {
                if (_dirty)
                    Update();

                return _scale;
            }
            set => SetScale(value);
        }

        /// <summary>
        /// Gets or Sets the Local Scale of the Transform
        /// </summary>
        public Vector2 LocalScale
        {
            get => _localScale;
            set => SetLocalScale(value);
        }

        /// <summary>
        /// Gets or Sets the World Rotation of the Transform in radians
        /// </summary>
        public float Rotation
        {
            get
            {
                if (_dirty)
                    Update();

                return _rotation;
            }
            set => SetRotation(value);
        }

        /// <summary>
        /// Gets or Sets the World Rotation of the Transform in degrees
        /// </summary>
        public float RotationDegrees
        {
            get => MathHelper.RadiansToDegrees(Rotation);
            set => SetRotationDegrees(value);
        }

        /// <summary>
        /// Gets or Sets the Local Rotation of the Transform in radians
        /// </summary>
        public float LocalRotation
        {
            get => _localRotation;
            set => SetLocalRotation(value);
        }

        /// <summary>
        /// Gets or Sets the Local Rotation of the Transform in degrees
        /// </summary>
        public float LocalRotationDegrees
        {
            get => MathHelper.RadiansToDegrees(LocalRotation);
            set => SetLocalRotationDegrees(value);
        }

        /// <summary>
        /// Gets the Local Matrix of the Transform
        /// </summary>
        public Matrix2D LocalMatrix
        {
            get
            {
                if (_dirty)
                    Update();
                return _localMatrix;
            }
        }

        /// <summary>
        /// Gets the World Matrix of the Transform
        /// </summary>
        public Matrix2D WorldMatrix
        {
            get
            {
                if (_dirty)
                    Update();
                return _worldMatrix;
            }
        }

        /// <summary>
        /// Gets the World-to-Local Matrix of the Transform
        /// </summary>
        public Matrix2D WorldToLocalMatrix
        {
            get
            {
                if (_dirty)
                    Update();
                return _worldToLocalMatrix;
            }
        }

        public List<Transform2D> Children { get; private set; } = new List<Transform2D>();

        #endregion

        private Transform2D _parent = null;
        private bool _dirty = true;

        //private Vector2 position = Vector2.Zero;
        //private Vector2 localPosition = Vector2.Zero;

        private Vector2 _scale = Vector2.One;
        private Vector2 _localScale = Vector2.One;

        private float _rotation = 0f;
        private float _localRotation = 0f;

        private Matrix2D _localMatrix = Matrix2DExt.Identity;
        private Matrix2D _worldMatrix = Matrix2DExt.Identity;
        private Matrix2D _worldToLocalMatrix = Matrix2DExt.Identity;

        public Transform2D(in Entity entity) => Entity = entity;

        #region fluent setters

        /// <summary>
        /// Sets the Parent of this Transform
        /// </summary>
        /// <param name="value">The new Parent</param>
        /// <param name="retainWorldPosition">Whether this Transform should retain its world position when it is transfered to the new parent</param>
        public Transform2D SetParent(Transform2D value, bool retainWorldPosition = false)
        {
            if (_parent != value)
            {
                // Circular Hierarchy isn't allowed
                if (value != null && value.Parent == this)
                    throw new Exception("Circular Transform Heritage is not allowed");

                // Remove our OnChanged listener from the existing parent
                // and remove this from the parent's children list
                if (_parent != null)
                {
                    _parent.OnChanged -= MakeDirty;
                    _parent.Children.Remove(this);
                }

                // store state
                var position = Position;
                var scale = Scale;
                var rotation = Rotation;

                // update parent
                _parent = value;
                _dirty = true;

                // retain state
                if (retainWorldPosition)
                {
                    Position = position;
                    Scale = scale;
                    Rotation = rotation;
                }

                // Add our OnChanged listener to the new parent
                // And add this to the parent's children list
                if (_parent != null)
                {
                    _parent.OnChanged += MakeDirty;
                    _parent.Children.Add(this);
                }

                // we have changed
                OnChanged?.Invoke();
            }

            return this;
        }

        /// <summary>
        /// Fluent setter for world position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Transform2D SetPosition(Vector2 position)
        {
            if (_parent == null)
                LocalPosition = position;
            else
                LocalPosition = Vector2Ext.Transform(position, WorldToLocalMatrix);

            return this;
        }

        /// <summary>
        /// Flient setter for worl position with x and y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Transform2D SetPosition(float x, float y)
            => SetPosition(new Vector2(x, y));

        /// <summary>
        /// Fluent setter for local position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Transform2D SetLocalPosition(Vector2 position)
        {
            if (LocalPosition != position)
            {
                _localMatrix.M31 = position.X;
                _localMatrix.M32 = position.Y;
                MakeDirty();
            }

            return this;
        }

        /// <summary>
        /// Fluent setter for local position with x and y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Transform2D SetLocalPosition(float x, float y)
            => SetLocalPosition(new Vector2(x, y));

        /// <summary>
        /// Fluent setter for world scale
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public Transform2D SetScale(Vector2 scale)
        {
            if (_parent == null)
            {
                SetLocalScale(scale);
            }
            else
            {
                if (_parent.Scale.X == 0)
                    scale.X = 0;
                else
                    scale.X /= _parent.Scale.X;

                if (_parent.Scale.Y == 0)
                    scale.Y = 0;
                else
                    scale.Y /= _parent.Scale.Y;

                SetLocalScale(scale);
            }

            return this;
        }

        /// <summary>
        /// Fluent setter for world scale with x and y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Transform2D SetScale(float x, float y)
            => SetScale(new Vector2(x, y));

        /// <summary>
        /// Fluent setter for uniform world scale
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public Transform2D SetScale(float scale)
            => SetScale(new Vector2(scale));

        /// <summary>
        /// Fluent setter for local scale
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public Transform2D SetLocalScale(Vector2 scale)
        {
            if (_localScale != scale)
            {
                _localScale = scale;
                MakeDirty();
            }

            return this;
        }

        /// <summary>
        /// Fluent setter for local scale with x and y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Transform2D SetLocalScale(float x, float y)
            => SetLocalScale(new Vector2(x, y));

        /// <summary>
        /// Fluent setter for uniform local scale
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public Transform2D SetLocalScale(float scale)
            => SetLocalScale(new Vector2(scale));

        /// <summary>
        /// Fluent setter for world rotation in radians
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public Transform2D SetRotation(float radians)
        {
            if (_parent == null)
                LocalRotation = radians;
            else
                LocalRotation = radians - _parent.Rotation;

            return this;
        }

        /// <summary>
        /// Fluent setter for world rotation in degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public Transform2D SetRotationDegrees(float degrees)
            => SetRotation(MathHelper.DegreesToRadians(degrees));

        /// <summary>
        /// Fluent setter for local rotation in radians
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public Transform2D SetLocalRotation(float radians)
        {
            if (_localRotation != radians)
            {
                _localRotation = radians;
                MakeDirty();
            }

            return this;
        }

        /// <summary>
        /// Fluent setter for local rotation in degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public Transform2D SetLocalRotationDegrees(float degrees)
            => SetLocalRotation(MathHelper.DegreesToRadians(degrees));

        #endregion

        private void Update()
        {
            _dirty = false;

            Matrix2D matrix = Matrix2DExt.Identity;

            if (_localScale != Vector2.One)
                Matrix2D.CreateScale(_scale.X, _scale.Y, out matrix);

            if (_localRotation != 0)
                Matrix2DExt.Mult(matrix, Matrix2D.CreateRotation(_rotation), out matrix);

            var localPosition = LocalPosition;
            if (localPosition != Vector2.Zero)
                Matrix2DExt.Mult(matrix, Matrix2DExt.CreateTranslation(localPosition.X, localPosition.Y), out matrix);

            _localMatrix = matrix;

            if (_parent == null)
            {
                _worldMatrix = _localMatrix;
                _worldToLocalMatrix = Matrix2DExt.Identity;
                //Position = localPosition;
                _scale = _localScale;
                _rotation = _localRotation;
            }
            else
            {
                Matrix2DExt.Mult(_localMatrix, _parent.WorldMatrix, out _worldMatrix);
                Matrix2DExt.CreateInvert(_parent._worldMatrix, out _worldToLocalMatrix);
                //Position = ImVec2.Transform(localPosition.ToImVec2(), parent.WorldMatrix).ToVector2();
                _scale = _localScale * _parent.Scale;
                _rotation = _localRotation + _parent.Rotation;
            }
        }

        private void MakeDirty()
        {
            if (!_dirty)
            {
                _dirty = true;
                OnChanged?.Invoke();
            }
        }


        /// <summary>
        /// Copies the other transform world transfrom and sets it to local
        /// </summary>
        /// <param name="transform">the transform to be copied from</param>
        public void CopyFrom(Transform2D transform)
        {
            Position = transform.Position;
            _rotation = transform._rotation;
            _localRotation = transform._localRotation;
            _scale = transform._scale;
            _localScale = transform._localScale;
        }

        /// <summary>
        /// Clones the transform without a parent and children
        /// </summary>
        /// <returns>The cloned transform</returns>
        public Transform2D Clone()
        {
            var trans = new Transform2D(Entity);

            trans.LocalPosition = Position;
            trans._scale = trans._localScale = Scale;
            trans._rotation = trans._localRotation = Rotation;
            trans._dirty = true;

            return trans;
        }
    }
}
