using OpenTK.Mathematics;
using ImVec3 = System.Numerics.Vector3;

namespace Monorail.Util
{
    public static class ImVec3Ext
    {
        public static Vector3 ToVector3(this ImVec3 vec)
        {
            return new Vector3(vec.X, vec.Y, vec.Z);
        }

        public static Vector3i ToVector3i(this ImVec3 vec)
        {
            return new Vector3i((int)vec.X, (int)vec.Y, (int)vec.Z);
        }
    }
}
