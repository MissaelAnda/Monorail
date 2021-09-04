using System;
using Monorail.Util;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using Necs;
using System.Collections.Generic;

namespace Monorail.ECS
{
    public class Transform
    {
        #region Properties

        public readonly Entity Entity;

        /// <summary>
        /// An action called whenever the Transform is modified
        /// </summary>
        public event Action OnChanged;

        /// <summary>
        /// Gets or Sets the Transform's Parent
        /// Modifying this does not change the World position of the Transform
        /// </summary>
        public Transform Parent
        {
            get => _parent;
            set => SetParent(value);
        }

        /// <summary>
        /// Gets or Sets the World Position of the Transform
        /// </summary>
        public Vector3 Position
        {
            get
            {
                if (_dirty)
                    Update();

                return _position;
            }
            set => SetPosition(value);
        }

        /// <summary>
        /// Gets or Sets the X Component of the World Position of the Transform
        /// </summary>
        public float X
        {
            get => Position.X;
            set => Position = new Vector3(value, Position.Y, Position.Z);
        }

        /// <summary>
        /// Gets or Sets the Y Component of the World Position of the Transform
        /// </summary>
        public float Y
        {
            get => Position.Y;
            set => Position = new Vector3(Position.X, value, Position.Z);
        }

        /// <summary>
        /// Gets or Sets the Z Component of the World Position of the Transform
        /// </summary>
        public float Z
        {
            get => Position.Z;
            set => Position = new Vector3(Position.X, Position.Y, value);
        }

        /// <summary>
        /// Gets or Sets the Local Position of the Transform
        /// </summary>
        public Vector3 LocalPosition
        {
            get => _localPosition;
            set => SetLocalPosition(value);
        }

        /// <summary>
        /// Gets or Sets the X Component of the World Position of the Transform
        /// </summary>
        public float LocalX
        {
            get => LocalPosition.X;
            set => LocalPosition = new Vector3(value, LocalPosition.Y, LocalPosition.Z);
        }

        /// <summary>
        /// Gets or Sets the Y Component of the World Position of the Transform
        /// </summary>
        public float LocalY
        {
            get => LocalPosition.Y;
            set => LocalPosition = new Vector3(LocalPosition.X, value, LocalPosition.Z);
        }

        /// <summary>
        /// Gets or Sets the Z Component of the World Position of the Transform
        /// </summary>
        public float LocalZ
        {
            get => LocalPosition.Z;
            set => LocalPosition = new Vector3(LocalPosition.X, LocalPosition.Y, value);
        }

        /// <summary>
        /// Gets or Sets the World Scale of the Transform
        /// </summary>
        public Vector3 Scale
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
        public Vector3 LocalScale
        {
            get => _localScale;
            set => SetLocalScale(value);
        }

        /// <summary>
        /// Gets or Sets the World Rotation of the Transform
        /// </summary>
        public Quaternion Rotation
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
        /// Gets or Sets the Local Rotation of the Transform
        /// </summary>
        public Quaternion LocalRotation
        {
            get => _localRotation;
            set => SetLocalRotation(value);
        }

        /// <summary>
        /// Gets the Local Matrix of the Transform
        /// </summary>
        public Matrix4 LocalMatrix
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
        public Matrix4 WorldMatrix
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
        public Matrix4 WorldToLocalMatrix
        {
            get
            {
                if (_dirty)
                    Update();
                return _worldToLocalMatrix;
            }
        }

        public List<Transform> Children { get; private set; } = new List<Transform>();

        #endregion
 
        private Transform _parent = null;
        private bool _dirty = true;

        private Vector3 _position = Vector3.Zero;
        private Vector3 _localPosition = Vector3.Zero;
        private Vector3 _scale = Vector3.Zero;
        private Vector3 _localScale = Vector3.One;
        private Quaternion _rotation = Quaternion.Identity;
        private Quaternion _localRotation = Quaternion.Identity;

        private Matrix4 _localMatrix = Matrix4.Identity;
        private Matrix4 _worldMatrix = Matrix4.Identity;
        private Matrix4 _worldToLocalMatrix = Matrix4.Identity;

        public Transform(Entity entity) { Entity = entity; }

        public Transform(Entity entity, Transform parent)
        {
            Entity = entity;
            SetParent(parent);
        }

        #region Fluent Setters

        /// <summary>
        /// Sets the parent of this transform
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>this transform</returns>
        public Transform SetParent(Transform parent)
        {
            if (_parent != parent)
            {
                // Remove our OnChanged listener from the existing parent
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
                _parent = parent;
                _dirty = true;

                // retain state
                Position = position;
                Scale = scale;
                Rotation = rotation;

                // Add our OnChanged listener to the new parent
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
        /// Sets the global position
        /// </summary>
        /// <param name="position"></param>
        /// <returns>this transform</returns>
        public Transform SetPosition(Vector3 position)
        {
            if (_parent == null)
                LocalPosition = position;
            else
                LocalPosition = Vector3.TransformPosition(position, WorldToLocalMatrix);

            return this;
        }

        /// <summary>
        /// Sets the global position individualy
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>this transform</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetPosition(float x, float y, float z)
            => SetPosition(new Vector3(x, y, z));

        /// <summary>
        /// Sets the global scale
        /// </summary>
        /// <param name="scale"></param>
        /// <returns>this transform</returns>
        public Transform SetScale(Vector3 scale)
        {
            if (_parent == null)
            {
                LocalScale = scale;
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

                if (_parent.Scale.Z == 0)
                    scale.Z = 0;
                else
                    scale.Z /= _parent.Scale.Z;

                LocalScale = scale;
            }

            return this;
        }

        /// <summary>
        /// Sets the global scale individually
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>this transform</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetScale(float x, float y, float z)
            => SetScale(new Vector3(x, y, z));

        /// <summary>
        /// Sets the uniform global scale
        /// </summary>
        /// <param name="scale"></param>
        /// <returns>this transform</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetScale(float scale)
            => SetScale(new Vector3(scale));

        /// <summary>
        /// Sets the global rotation
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns>this transform</returns>
        public Transform SetRotation(Quaternion rotation)
        {
            if (_parent == null)
                LocalRotation = rotation;
            else
                LocalRotation = QuaternionExt.Divide(rotation, _parent.Rotation);
            return this;
        }

        /// <summary>
        /// Sets the local position, relative to parent if has
        /// </summary>
        /// <param name="position"></param>
        /// <returns>this transform</returns>
        public Transform SetLocalPosition(Vector3 position)
        {
            if (_localPosition != position)
            {
                _localPosition = position;
                MakeDirty();
            }
            return this;
        }

        /// <summary>
        /// Sets the local position individually
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>this transform</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetLocalPosition(float x, float y, float z)
            => SetLocalPosition(new Vector3(x, y, z));

        /// <summary>
        /// Sets the local scale, relative to parent if has
        /// </summary>
        /// <param name="scale"></param>
        /// <returns>this transform</returns>
        public Transform SetLocalScale(Vector3 scale)
        {
            if (_localScale != scale)
            {
                _localScale = scale;
                MakeDirty();
            }
            return this;
        }

        /// <summary>
        /// Sets the local scale individually
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>this transform</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetLocalScale(float x, float y, float z)
            => SetLocalScale(new Vector3(x, y, z));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Transform SetLocalScale(float scale)
            => SetLocalScale(new Vector3(scale));

        /// <summary>
        /// Sets the local rotation, relative to parent if has
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns>this transform</returns>
        public Transform SetLocalRotation(Quaternion rotation)
        {
            if (_localRotation != rotation)
            {
                _localRotation = rotation;
                MakeDirty();
            }
            return this;
        }

        #endregion

        private void Update()
        {
            _dirty = false;

            _localMatrix =
                Matrix4.CreateScale(_localScale) *
                Matrix4.CreateFromQuaternion(_localRotation) *
                Matrix4.CreateTranslation(_localPosition);

            if (_parent == null)
            {
                _worldMatrix = _localMatrix;
                _worldToLocalMatrix = Matrix4.Identity;
                _position = _localPosition;
                _scale = _localScale;
                _rotation = _localRotation;
            }
            else
            {
                _worldMatrix = _localMatrix * _parent.WorldMatrix;
                Matrix4.Invert(_parent.WorldMatrix, out _worldToLocalMatrix);
                _position = Vector3.TransformPosition(_localPosition, _parent.WorldMatrix);
                _scale = _localScale * _parent.Scale;
                _rotation = _localRotation * _parent.Rotation;
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

    }
}
