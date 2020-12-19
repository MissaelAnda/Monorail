using OpenTK.Mathematics;
using ImVec2 = System.Numerics.Vector2;

namespace Monorail.Util
{
    public static class ImVec2Ext
    {
        public static Vector2 ToVector2(this ImVec2 vec2)
        {
            return new Vector2(vec2.X, vec2.Y);
        }

        public static Vector2i ToVector2i(this ImVec2 vec2)
        {
            return new Vector2i((int)vec2.X, (int)vec2.Y);
        }
    }
}
