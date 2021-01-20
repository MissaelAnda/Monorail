using OpenTK.Mathematics;
using ImVec4 = System.Numerics.Vector4;

namespace Monorail.Util
{
    public static class ImVec4Ext
    {
        public static Color4 ToColor4(in this ImVec4 vec)
        {
            return new Color4(vec.X, vec.Y, vec.Z, vec.W);
        }
    }
}
