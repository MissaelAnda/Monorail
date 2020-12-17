using System.Runtime.CompilerServices;

namespace Monorail.Util
{
    public static class Flags
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(int val, int pos)
        {
            return (val & (1 << pos)) != 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(ref int val, int pos, bool on)
        {
            if (on) val |= 1 << pos;
            else    val &= ~(1 << pos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Toggle(ref int val, int pos)
        {
            SetBit(ref val, pos, !GetBit(val, pos));
        }
    }
}
