using Monorail.Math;
using Monorail.Util;
using Monorail.Debug;
using System.Drawing;
using Monorail.Layers;
using Monorail.Renderer;
using OpenTK.Mathematics;
using Matrix2D = OpenTK.Mathematics.Matrix3x2;
using System.ComponentModel.DataAnnotations;

namespace Monorail.ECS
{
    public class Camera2D
    {
        private const float MaxZoom = 40f;
        private const float MinZoom = 0.000000001f;

        enum MatrixDirtyType
        {
            Clean,
            View,
            Projection,
        }

        #region Propeties

        public Vector2 Resolution
        {
            get
            {
                // if the camera doesn't have a custom resolution it should use the viewport one
                var res = _resolution.HasValue ? _resolution.Value : new Vector2(RenderCommand.Viewport.Width, RenderCommand.Viewport.Height);
                if (res != _oldRes)
                {
                    _dirtyType |= MatrixDirtyType.Projection;
                    _origin = res / 2;
                    _oldRes = res;
                }
                return res;
            }
            set => SetResolution(value);
        }

        [Range(-1f, 1f)]
        public float Zoom
        {
            get => (_zoom / MaxZoom) * 2 - 1;
            set => Zoom = ((value + 1) / 2) * MaxZoom;
        }

        [Range(MinZoom, MaxZoom)]
        public float RawZoom
        {
            get => _zoom;
            set => _zoom = value;
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

        Vector2? _resolution = null;
        Vector2 _oldRes;
        Vector2 _origin;

        float _zoom = 1f;

        internal Transform2D Transform;


        public Camera2D(Transform2D transform)
        {
            Transform = transform;
            Transform.OnChanged += MakeViewDirty;
            _origin = Resolution / 2;
        }

        #region Fluent setters

        public Camera2D SetResolution(Vector2 resolution)
        {
            _resolution = resolution;
            _dirtyType |= MatrixDirtyType.Projection;
            _origin = resolution / 2;
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

        public void UseDefaultResolution()
        {
            _dirtyType |= MatrixDirtyType.Projection;
            _resolution = null;
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
