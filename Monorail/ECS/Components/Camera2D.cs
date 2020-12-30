using Monorail.Math;
using Monorail.Util;
using Monorail.Debug;
using System.Drawing;
using Monorail.Renderer;
using OpenTK.Mathematics;
using Matrix2D = OpenTK.Mathematics.Matrix3x2;

namespace Monorail.ECS
{
    public class Camera2D : ICamera
    {
        private const float DefaultMaxZoom = 2f;
        private const float DefaultMinZoom = 0.1f;

        enum MatrixDirtyType
        {
            Clean,
            View,
            Projection,
        }

        #region Propeties

        public Vector2 Resolution
        {
            get => _resolution;
            set => SetResolution(value);
        }

        /// <summary>
        /// Normalized zoom property in a range from -1 to 1, 0 = no zoom
        /// </summary>
        public float Zoom
        {
            get
            {
                if (_zoom < 1f)
                    return MathExtra.Map(_zoom, MinZoom, 1f, 1f, 0f);

                return MathExtra.Map(_zoom, 1f, MaxZoom, 0f, -1f);
            }
            set => SetZoom(value);
        }

        /// <summary>
        /// The raw zoom of the camera, this is the value used by the matrixes operations
        /// it's in a range of 0.1(zoom in) to 2(zoom out)
        /// </summary>
        public float RawZoom
        {
            get => _zoom;
            set => SetRawZoom(value);
        }

        public float MinZoom
        {
            get => _minZoom;
            set
            {
                if (_zoom != value && value > 0 && value < 1 && value < _maxZoom)
                    _zoom = value;
            }
        }

        public float MaxZoom
        {
            get => _maxZoom;
            set
            {
                if (_maxZoom != value && value > _minZoom && value > 1)
                    _maxZoom = value;
            }
        }

        public RectangleF Bounds
        {
            get
            {
                Update();

                if (_areBoundsDirty)
                {
                    // top-left and bottom-right are needed by either rotated or non-rotated bounds
                    var topLeft = ScreenToWorldPoint(new Vector2(0));
                    var bottomRight = ScreenToWorldPoint(new Vector2(Resolution.X, Resolution.Y));

                    if (Transform.Rotation != 0)
                    {
                        // special care for rotated bounds. we need to find our
                        // absolute min/max values and create the bounds from that
                        var topRight = ScreenToWorldPoint(new Vector2(Resolution.X, 0));
                        var bottomLeft = ScreenToWorldPoint(new Vector2(0, Resolution.Y));

                        var minX = MathExtra.Min(topLeft.X, bottomRight.X, topRight.X, bottomLeft.X);
                        var maxX = MathExtra.Max(topLeft.X, bottomRight.X, topRight.X, bottomLeft.X);
                        var minY = MathExtra.Min(topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y);
                        var maxY = MathExtra.Max(topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y);

                        _bounds.Location = new PointF(minX, minY);
                        _bounds.Width = maxX - minX;
                        _bounds.Height = maxY - minY;
                    }
                    else
                    {
                        _bounds.Location = new PointF(topLeft.X, topLeft.Y);
                        _bounds.Width = bottomRight.X - topLeft.X;
                        _bounds.Height = bottomRight.Y - topLeft.Y;
                    }

                    _areBoundsDirty = false;
                }

                return _bounds;
            }
        }

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

        #endregion

        Matrix4 _projection;
        Matrix4 _view;
        Matrix4 _projectionView;

        Matrix2D _transformMatrix = Matrix2DExt.Identity;
        Matrix2D _inverseTransformMatrix = Matrix2DExt.Identity;

        RectangleF _bounds;

        MatrixDirtyType _dirtyType = MatrixDirtyType.Projection | MatrixDirtyType.View;
        bool _areBoundsDirty = false;

        Vector2 _resolution = new Vector2(App.Width, App.Height);

        float _zoom = 1f;
        float _minZoom = DefaultMinZoom;
        float _maxZoom = DefaultMaxZoom;

        internal Transform2D Transform;


        public Camera2D(Transform2D transform)
        {
            Transform = transform;
            Transform.OnChanged += MakeViewDirty;
        }

        #region Fluent setters

        public Camera2D SetResolution(Vector2 resolution)
        {
            if (resolution != _resolution)
            {
                _resolution = resolution;
                _dirtyType |= MatrixDirtyType.Projection;
            }
            return this;
        }

        /// <summary>
        /// Fluent setter for the camera's zoom in a range of -1, 1 being 0 no zoom
        /// </summary>
        /// <param name="zoom"></param>
        /// <returns></returns>
        public Camera2D SetZoom(float zoom)
        {
            var newZoom = MathHelper.Clamp(zoom, -1, 1);

            if (newZoom == 0)
                newZoom = 1f;
            else if (newZoom < 0)
                newZoom = MathExtra.Map(newZoom, -1, 0, MaxZoom, 1);
            else
                newZoom = MathExtra.Map(newZoom, 0, 1, 1, MinZoom);

            if (newZoom != _zoom)
            {
                _zoom = newZoom;
                _dirtyType |= MatrixDirtyType.View;
            }

            return this;
        }

        /// <summary>
        /// Fluent setter for the zoom in a range between <cref>MinZoom</cref> and <cref>MaxZoom</cref>
        /// </summary>
        /// <param name="zoom"></param>
        /// <returns>this</returns>
        public Camera2D SetRawZoom(float zoom)
        {
            var newZoom = MathHelper.Clamp(zoom, MinZoom, MaxZoom);

            if (_zoom != newZoom)
            {
                _zoom = newZoom;
                _dirtyType |= MatrixDirtyType.View;
            }

            return this;
        }

        #endregion

        public Vector2 ScreenToWorldPoint(Vector2 screenPosition)
        {
            var origin = screenPosition - Resolution / 2;
            Update();
            Vector2Ext.Transform(ref origin, ref _inverseTransformMatrix, out screenPosition);
            return screenPosition;
        }

        void Update()
        {
            if (_dirtyType == MatrixDirtyType.Clean)
                return;

            if (_dirtyType.HasFlag(MatrixDirtyType.View))
            {
                _transformMatrix = Matrix2DExt.Identity;

                // Should the transform's scale affect the zoom?
                if (_zoom != 1f)
                {
                    Matrix2D.CreateScale(_zoom, _zoom, out _transformMatrix);
                }

                if (Transform.Rotation != 0f)
                {
                    Matrix2D.CreateRotation(Transform.Rotation, out var rotation); // rotation
                    Matrix2DExt.Mult(_transformMatrix, rotation, out _transformMatrix);
                }

                Matrix2DExt.CreateTranslation(Transform.Position.X, Transform.Position.Y, out var translation); // translate
                Matrix2DExt.Mult(_transformMatrix, translation, out _transformMatrix);

                // calculate our inverse as well
                Matrix2DExt.CreateInvert(_transformMatrix, out _inverseTransformMatrix);

                // View transform
                _view = _inverseTransformMatrix.ToMat4();
            }

            if (_dirtyType.HasFlag(MatrixDirtyType.Projection))
            {
                // Projection transform
                var resolution = Resolution;
                Matrix4.CreateOrthographic(resolution.X, resolution.Y, 0f, 1f, out _projection);
            }

            Matrix4.Mult(_view, _projection, out _projectionView);

            _dirtyType = MatrixDirtyType.Clean;
            _areBoundsDirty = true;
        }

        private void MakeViewDirty()
        {
            _dirtyType |= MatrixDirtyType.View;
        }

        internal void SetTransform(Transform2D transform)
        {
            Insist.AssertNotNull(transform, "Transform can't be null");

            Transform.OnChanged -= MakeViewDirty;
            Transform = transform;
            Transform.OnChanged += MakeViewDirty;
        }
    }
}
