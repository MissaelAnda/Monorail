using System.Runtime.CompilerServices;

namespace Monorail.Math
{
    public static class MathExtra
    {
        #region Min

        public static byte Min(params byte[] values)
        {
            byte min = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];
            return min;
        }

        public static sbyte Min(params sbyte[] values)
        {
            sbyte min = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];
            return min;
        }

        public static short Min(params short[] values)
        {
            short min = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];
            return min;
        }

        public static ushort Min(params ushort[] values)
        {
            ushort min = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];
            return min;
        }

        public static int Min(params int[] values)
        {
            int min = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];
            return min;
        }

        public static uint Min(params uint[] values)
        {
            uint min = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];
            return min;
        }

        public static long Min(params long[] values)
        {
            long min = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];
            return min;
        }

        public static ulong Min(params ulong[] values)
        {
            ulong min = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];
            return min;
        }

        public static float Min(params float[] values)
        {
            float min = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];
            return min;
        }

        public static double Min(params double[] values)
        {
            double min = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < min)
                    min = values[i];
            return min;
        }

        #endregion

        #region Max

        public static byte Max(params byte[] values)
        {
            byte max = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];
            return max;
        }

        public static sbyte Max(params sbyte[] values)
        {
            sbyte max = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];
            return max;
        }

        public static short Max(params short[] values)
        {
            short max = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];
            return max;
        }

        public static ushort Max(params ushort[] values)
        {
            ushort max = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];
            return max;
        }

        public static int Max(params int[] values)
        {
            int max = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];
            return max;
        }

        public static uint Max(params uint[] values)
        {
            uint max = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];
            return max;
        }

        public static long Max(params long[] values)
        {
            long max = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];
            return max;
        }

        public static ulong Max(params ulong[] values)
        {
            ulong max = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];
            return max;
        }

        public static float Max(params float[] values)
        {
            float max = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];
            return max;
        }

        public static double Max(params double[] values)
        {
            double max = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] > max)
                    max = values[i];
            return max;
        }

        #endregion

        /// <summary>
		/// mapps value (which is in the range leftMin - leftMax) to a value in the range rightMin - rightMax
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="leftMin">Left minimum.</param>
		/// <param name="leftMax">Left max.</param>
		/// <param name="rightMin">Right minimum.</param>
		/// <param name="rightMax">Right max.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Map(double value, double leftMin, double leftMax, double rightMin, double rightMax)
        {
            return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
        }

        /// <summary>
		/// mapps value (which is in the range leftMin - leftMax) to a value in the range rightMin - rightMax
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="leftMin">Left minimum.</param>
		/// <param name="leftMax">Left max.</param>
		/// <param name="rightMin">Right minimum.</param>
		/// <param name="rightMax">Right max.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Map(float value, float leftMin, float leftMax, float rightMin, float rightMax)
        {
            return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
        }
    }
}
