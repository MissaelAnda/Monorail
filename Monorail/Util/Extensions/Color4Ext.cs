using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Monorail.Util
{
    public static class Color4Ext
    {
        public static float[] ToArray(this Color4 color)
        {
            return new float[4] { color.R, color.G, color.B, color.A };
        }
    }
}
