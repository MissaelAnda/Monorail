using Monorail.Layers.ImGUI;
using Monorail.Util;
using OpenTK.Mathematics;

namespace Monorail.ECS
{
    public class Camera2D
    {
        internal Transform2D Transform;

        public Vector2 Resolution
        {
            // Check if Editor Layer is enabled to use viewport resolution, else use window height
            get
            {
                var res = _resolution.HasValue ? _resolution.Value: Editor.Viewport;
                if (res != _oldRes)
                {
                    _dirty = true;
                    _oldRes = res;
                }
                return res;
            }
            set => SetResolution(value);
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

        bool _dirty = true;

        Vector2? _resolution = null;
        Vector2 _oldRes;

        public Camera2D() { }

        public Camera2D(Vector2 resolution)
        {
            _resolution = resolution;
        }

        void Update()
        {
            //if (_dirty)
            {
                Matrix4.CreateTranslation(Transform.Position.ToVector3(), out var trans);
                Matrix4.CreateRotationZ(Transform.Rotation, out var rot);

                Matrix4.Mult(trans, rot, out var mult);
                Matrix4.Invert(mult, out _view);

                var sideWidth = Resolution.X / 2;
                var sideHeight = Resolution.Y / 2;
                _projection = Matrix4.CreateOrthographicOffCenter(-sideWidth, sideWidth, -sideHeight, sideHeight, 0f, 1f);

                Matrix4.Mult(_projection, _view, out _projectionView);

                _dirty = false;
            }
        }

        public Camera2D SetResolution(Vector2 resolution)
        {
            _resolution = resolution;
            _dirty = true;
            return this;
        }

        public void UseDefaultResolution() => _resolution = null;
    }
}
