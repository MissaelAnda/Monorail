using Monorail.Debug;
using Monorail.Util;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monorail.ECS
{
    public enum ProjectionType
    {
        Perspective,
        Orthographic
    }

    public class Camera3D : ICamera
    {
        private enum DirtyType
        {
            Clean,
            Projection,
            View,
        }

        internal Transform Transform;

        #region Properties
        public Matrix4 Projection
        {
            get
            {
                Update();
                return _projection;
            }
        }

        public Matrix4 View
        {
            get
            {
                Update();
                return _view;
            }
        }

        public Matrix4 ProjectionView
        {
            get
            {
                Update();
                return _projectionView;
            }
        }

        public Vector2 Resolution { get => _resolution; set => SetResolution(value); }

        public float Zoom { get => _zoom; set => SetZoom(value); }

        public float RawZoom { get => _zoom; set => SetRawZoom(value); }

        #endregion

        private Vector2 _resolution;
        private float _zoom;
        private Matrix4 _projection;
        private Matrix4 _view;
        private Matrix4 _projectionView;
        private ProjectionType _type = ProjectionType.Perspective;
        private DirtyType _dirtyType = DirtyType.Projection | DirtyType.View;
        private bool _dirty = true;
        private float _fov = MathF.PI / 8f;
        private float _near = 0.1f;
        private float _far = 100f;

        public Camera3D(Transform transform)
        {
            Transform = transform;
            transform.OnChanged += MakeViewDirty;
        }

        #region Fluent setters

        public Camera3D SetResolution(Vector2 resolution)
        {
            if (resolution != _resolution)
            {
                _resolution = resolution;
                _dirtyType |= DirtyType.Projection;
            }

            return this;
        }

        public Camera3D SetZoom(float zoom)
        {
            if (zoom != _zoom)
            {
                _zoom = zoom;
                _dirtyType |= DirtyType.Projection;
            }

            return this;
        }

        public Camera3D SetRawZoom(float zoom)
        {
            if (_zoom != zoom)
            {
                // TODO: Raw zoom
                _zoom = zoom;
                _dirtyType |= DirtyType.Projection;
            }

            return this;
        }

        #endregion

        public void Update()
        {
            if (_dirtyType != DirtyType.Clean || _dirty)
            {
                if (_dirtyType.HasFlag(DirtyType.Projection))
                {
                    if (_type == ProjectionType.Perspective)
                        Matrix4.CreatePerspectiveFieldOfView(_fov, Resolution.X / Resolution.Y, _near, _far, out _projection);
                    else
                    {
                        float halfX = Resolution.X / 2;
                        float halfY = Resolution.Y / 2;
                        Matrix4.CreateOrthographicOffCenter(-halfX, halfX, halfY, -halfY, _near, _far, out _projection);
                    }
                }

                if (_dirtyType.HasFlag(DirtyType.View))
                {
                    // TODO: Change unit z to quaternion rotation
                    _view = Matrix4.LookAt(Transform.Position, Transform.Position + Transform.Rotation.Direction(), Vector3.UnitY);
                }

                Matrix4.Mult(_view, _projection, out _projectionView);
                _dirty = false;
                _dirtyType = DirtyType.Clean;
            }
        }

        private void MakeViewDirty() => _dirtyType |= DirtyType.View;

        public void MakeDirty() => _dirty = true;

        internal void SetTransform(Transform transform)
        {
            Insist.AssertNotNull(transform, "Transform can't be null");

            Transform.OnChanged -= MakeViewDirty;
            Transform = transform;
            Transform.OnChanged += MakeViewDirty;
        }
    }
}
