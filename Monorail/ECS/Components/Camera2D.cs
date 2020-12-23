using Monorail.Util;
using System.Drawing;
using OpenTK.Mathematics;
using Monorail.Layers.ImGUI;
using Matrix2D = OpenTK.Mathematics.Matrix3x2;
using Monorail.Renderer;
using Monorail.Math;

namespace Monorail.ECS
{
    public class Camera2D
    {
        internal Transform2D Transform;

        enum MatrixDirtyType
        {
            Clean,
            View,
            Projection,
        }

        public Vector2 Resolution
        {
            // Check if Editor Layer is enabled to use viewport resolution, else use window height
            get
            {
                var res = _resolution.HasValue ? _resolution.Value: Editor.Viewport;
                if (res != _oldRes)
                {
                    _dirtyType |= MatrixDirtyType.Projection;
                    _oldRes = res;
                }
                return res;
            }
            set => SetResolution(value);
        }

        public RectangleF Bounds
        {
            get
            {
                Update();

                if (_areBoundsDirty)
                {
                    // top-left and bottom-right are needed by either rotated or non-rotated bounds
                    // TODO: Change screen size for Scene viewport
                    var topLeft = ScreenToWorldPoint(new Vector2(RenderCommand.Viewport.X, RenderCommand.Viewport.Y));
                    var bottomRight = ScreenToWorldPoint(new Vector2(
                        RenderCommand.Viewport.X + RenderCommand.Viewport.Width,
                        RenderCommand.Viewport.Y + RenderCommand.Viewport.Height));

                    if (Transform.Rotation != 0)
                    {
                        // special care for rotated bounds. we need to find our absolute min/max values and create the bounds from that
                        var topRight = ScreenToWorldPoint(new Vector2(
                            RenderCommand.Viewport.X + RenderCommand.Viewport.Width,
                            RenderCommand.Viewport.Y));
                        var bottomLeft = ScreenToWorldPoint(new Vector2(RenderCommand.Viewport.X,
                            RenderCommand.Viewport.Y + RenderCommand.Viewport.Height));

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

        public Camera2D() { }

        public Camera2D(Vector2 resolution)
        {
            _resolution = resolution;
        }

        void Update()
        {
            if (_dirtyType == MatrixDirtyType.Clean)
                return;

            if (_dirtyType.HasFlag(MatrixDirtyType.View))
            {
                Matrix2D tempMat = Matrix2DExt.Identity;
                _transformMatrix =
                    Matrix2DExt.CreateTranslation(-Transform.Position.X, -Transform.Position.Y); // position

                //if (_zoom != 1f)
                //{
                //    Matrix2D.CreateScale(_zoom, _zoom, out tempMat); // scale ->
                //    Matrix2D.Multiply(ref _transformMatrix, ref tempMat, out _transformMatrix);
                //}

                if (Transform.Rotation != 0f)
                {
                    Matrix2D.CreateRotation(Transform.Rotation, out tempMat); // rotation
                    Matrix2DExt.Mult(_transformMatrix, tempMat, out _transformMatrix);
                }

                //Matrix2DExt.CreateTranslation((int)_origin.X, (int)_origin.Y, out tempMat); // translate -origin
                Matrix2DExt.Mult(_transformMatrix, tempMat, out _transformMatrix);

                // calculate our inverse as well
                Matrix2DExt.CreateInvert(_transformMatrix, out _inverseTransformMatrix);

                // View transform
                Matrix4.CreateTranslation(Transform.Position.ToVector3(), out var trans);
                Matrix4.CreateRotationZ(Transform.Rotation, out var rot);

                Matrix4.Mult(trans, rot, out var mult);
                Matrix4.Invert(mult, out _view);
            }

            if (_dirtyType.HasFlag(MatrixDirtyType.Projection))
            {
                // Projection transform
                var sideWidth = Resolution.X / 2;
                var sideHeight = Resolution.Y / 2;
                _projection = Matrix4.CreateOrthographicOffCenter(-sideWidth, sideWidth, -sideHeight, sideHeight, 0f, 1f);
            }

            Matrix4.Mult(_projection, _view, out _projectionView);

            _dirtyType = MatrixDirtyType.Clean;
            _areBoundsDirty = true;
        }

        public Vector2 ScreenToWorldPoint(Vector2 screenPosition)
        {
            Update();
            var origin = screenPosition - Resolution / 2;
            Vector2Ext.Transform(ref origin, ref _inverseTransformMatrix, out screenPosition);
            return screenPosition;
        }

        public Camera2D SetResolution(Vector2 resolution)
        {
            _resolution = resolution;
            _dirtyType |= MatrixDirtyType.Projection;
            return this;
        }

        public void UseDefaultResolution()
        {
            _dirtyType |= MatrixDirtyType.Projection;
            _resolution = null;
        }
    }
}
