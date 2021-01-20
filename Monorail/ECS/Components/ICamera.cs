using OpenTK.Mathematics;

namespace Monorail.ECS
{
    public interface ICamera
    {
        public Matrix4 Projection { get; }

        public Matrix4 View { get; }

        public Matrix4 ProjectionView { get; }

        public Vector2 Resolution { get; set; }

        public float Zoom { get; set; }

        public float RawZoom { get; set; }
    }
}
