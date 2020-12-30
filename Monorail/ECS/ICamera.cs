using OpenTK.Mathematics;

namespace Monorail.ECS
{
    public interface ICamera
    {
        public Vector2 Resolution { get; set; }

        public float Zoom { get; set; }

        public float RawZoom { get; set; }
    }
}
