using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using ImVec2 = System.Numerics.Vector2;
using Matrix2D = OpenTK.Mathematics.Matrix3x2;

namespace Monorail.Util
{
    public static class Vector2Ext
    {
		/// <summary>
		/// rounds the x and y values
		/// </summary>
		/// <param name="vec">Vec.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Round(ref this Vector2 vec)
		{
			vec.X = (float)MathHelper.Round(vec.X);
			vec.Y = (float)MathHelper.Round(vec.Y);
		}

		/// <summary>
		/// rounds the x and y values
		/// </summary>
		/// <param name="vec">Vec.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 Round(Vector2 vec)
		{
			return new Vector2((float)MathHelper.Round(vec.X), (float)MathHelper.Round(vec.Y));
		}


		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix"/>.
		/// </summary>
		/// <param name="position">Source <see cref="Vector2"/>.</param>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <returns>Transformed <see cref="Vector2"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 Transform(Vector2 position, Matrix2D matrix)
		{
			return new Vector2((position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M31,
				(position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M32);
		}


		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix"/>.
		/// </summary>
		/// <param name="position">Source <see cref="Vector2"/>.</param>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <param name="result">Transformed <see cref="Vector2"/> as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Transform(ref Vector2 position, ref Matrix2D matrix, out Vector2 result)
		{
			var x = (position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M31;
			var y = (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M32;
			result.X = x;
			result.Y = y;
		}


		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix"/>.
		/// </summary>
		/// <param name="position">Source <see cref="Vector2"/>.</param>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Transform(ref this Vector2 vector, ref Vector2 position, ref Matrix2D matrix)
		{
			var x = (position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M31;
			var y = (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M32;
			vector.X = x;
			vector.Y = y;
		}


		/// <summary>
		/// Apply transformation on vectors within array of <see cref="Vector2"/> by the specified <see cref="Matrix"/> and places the results in an another array.
		/// </summary>
		/// <param name="sourceArray">Source array.</param>
		/// <param name="sourceIndex">The starting index of transformation in the source array.</param>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <param name="destinationArray">Destination array.</param>
		/// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector2"/> should be written.</param>
		/// <param name="length">The number of vectors to be transformed.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Transform(Vector2[] sourceArray, int sourceIndex, ref Matrix2D matrix,
									 Vector2[] destinationArray, int destinationIndex, int length)
		{
			for (var i = 0; i < length; i++)
			{
				var position = sourceArray[sourceIndex + i];
				var destination = destinationArray[destinationIndex + i];
				destination.X = (position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M31;
				destination.Y = (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M32;
				destinationArray[destinationIndex + i] = destination;
			}
		}


		/// <summary>
		/// Apply transformation on all vectors within array of <see cref="Vector2"/> by the specified <see cref="Matrix"/> and places the results in an another array.
		/// </summary>
		/// <param name="sourceArray">Source array.</param>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <param name="destinationArray">Destination array.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Transform(Vector2[] sourceArray, ref Matrix2D matrix, Vector2[] destinationArray)
		{
			Transform(sourceArray, 0, ref matrix, destinationArray, 0, sourceArray.Length);
		}

		/// <summary>
		/// Divides the vector a / b
		/// </summary>
		/// <param name="a">dividend</param>
		/// <param name="b">divider</param>
		public static void Divide(ref this Vector2 a, in Vector2 b)
        {
			a.X /= b.X;
			a.Y /= b.Y;
        }

		/// <summary>
		/// Divides the vector a / b
		/// </summary>
		/// <param name="a">dividend</param>
		/// <param name="b">divider</param>
		public static void Divide(ref this Vector2 a, Vector2 b)
		{
			a.X /= b.X;
			a.Y /= b.Y;
		}

		/// <summary>
		/// Creates a new vector 3 with 0 in Z
		/// </summary>
		/// <param name="vec"></param>
		/// <returns>The new vector</returns>
		public static Vector3 ToVector3(this Vector2 vec)
		{
			return new Vector3(vec.X, vec.Y, 0);
		}

		public static ImVec2 ToImVec2(this Vector2 vec)
			=> new ImVec2(vec.X, vec.Y);
	}
}
