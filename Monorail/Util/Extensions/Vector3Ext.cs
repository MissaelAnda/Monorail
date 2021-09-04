using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using ImVec3 = System.Numerics.Vector3;
using Matrix2D = OpenTK.Mathematics.Matrix3x2;

namespace Monorail.Util
{
    public static class Vector3Ext
    {
		/// <summary>
		/// rounds the x and y values
		/// </summary>
		/// <param name="vec">Vec.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Round(ref this Vector3 vec)
		{
			vec.X = (float)MathHelper.Round(vec.X);
			vec.Y = (float)MathHelper.Round(vec.Y);
			vec.Z = (float)MathHelper.Round(vec.Z);
		}

		/// <summary>
		/// rounds the x and y values
		/// </summary>
		/// <param name="vec">Vec.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Round(Vector3 vec)
		{
			return new Vector3(
				(float)MathHelper.Round(vec.X),
				(float)MathHelper.Round(vec.Y),
				(float)MathHelper.Round(vec.Z)
			);
		}

		/// <summary>
		/// Divides the vector a / b
		/// </summary>
		/// <param name="a">dividend</param>
		/// <param name="b">divider</param>
		public static void Divide(ref this Vector3 a, in Vector3 b)
        {
			a.X /= b.X;
			a.Y /= b.Y;
			a.Z /= b.Z;
        }

		/// <summary>
		/// Divides the vector a / b
		/// </summary>
		/// <param name="a">dividend</param>
		/// <param name="b">divider</param>
		public static void Divide(ref this Vector3 a, Vector3 b)
		{
			a.X /= b.X;
			a.Y /= b.Y;
			a.Z /= b.Z;
		}

		public static Color4 ToColor(ref this Vector3 vec, float a = 1.0f)
        {
			return new Color4(vec.X, vec.Y, vec.Z, a);
        }

		public static ImVec3 ToImVec3(this Vector3 vec)
			=> new ImVec3(vec.X, vec.Y, vec.Z);
	}
}
