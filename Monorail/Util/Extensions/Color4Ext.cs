using OpenTK.Mathematics;
using ImVec3 = System.Numerics.Vector3;
using ImVec4 = System.Numerics.Vector4;

namespace Monorail.Util
{
    public static class Color4Ext
    {
        public static float[] ToArray(this Color4 color)
        {
            return new float[4] { color.R, color.G, color.B, color.A };
        }

        public static ImVec4 ToImVec4(in this Color4 color)
        {
            return new ImVec4(color.R, color.G, color.B, color.A);
        }

        public static ImVec3 ToImVec3(in this Color4 color)
        {
            return new ImVec3(color.R, color.G, color.B);
        }
    }
}
