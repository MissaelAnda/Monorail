using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using Matrix2D = OpenTK.Mathematics.Matrix3x2;

namespace Monorail.Util
{
    public static class Matrix2DExt
    {
        public static Matrix2D Identity => _identity;
        static Matrix2D _identity = new Matrix2D(1f, 0f, 0f, 1f, 0f, 0f);

		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="xPosition">X coordinate of translation.</param>
		/// <param name="yPosition">Y coordinate of translation.</param>
		/// <returns>The translation <see cref="Matrix2D"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D CreateTranslation(float xPosition, float yPosition)
		{
			Matrix2D result;
			CreateTranslation(xPosition, yPosition, out result);
			return result;
		}


		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="position">X,Y and Z coordinates of translation.</param>
		/// <param name="result">The translation <see cref="Matrix2D"/> as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CreateTranslation(in Vector2 position, out Matrix2D result)
		{
			result = new Matrix2D(1, 0, 0, 1, position.X, position.Y);
		}


		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="position">X,Y and Z coordinates of translation.</param>
		/// <returns>The translation <see cref="Matrix2D"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D CreateTranslation(in Vector2 position)
		{
			Matrix2D result;
			CreateTranslation(position, out result);
			return result;
		}


		/// <summary>
		/// Creates a new translation <see cref="Matrix2D"/>.
		/// </summary>
		/// <param name="xPosition">X coordinate of translation.</param>
		/// <param name="yPosition">Y coordinate of translation.</param>
		/// <param name="result">The translation <see cref="Matrix2D"/> as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CreateTranslation(float xPosition, float yPosition, out Matrix2D result)
		{
			result = new Matrix2D(1, 0, 0, 1, xPosition, yPosition);
		}

		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains a multiplication of two matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/>.</param>
		/// <returns>Result of the matrix multiplication.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D Mult(in Matrix2D matrix1, in Matrix2D matrix2)
		{
			var m11 = (matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21);
			var m12 = (matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22);

			var m21 = (matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21);
			var m22 = (matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22);

			var m31 = (matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21) + matrix2.M31;
			var m32 = (matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22) + matrix2.M32;

			return new Matrix2D(m11, m12, m21, m22, m31, m32);
		}


		/// <summary>
		/// Creates a new <see cref="Matrix2D"/> that contains a multiplication of two matrix.
		/// </summary>
		/// <param name="matrix1">Source <see cref="Matrix2D"/>.</param>
		/// <param name="matrix2">Source <see cref="Matrix2D"/>.</param>
		/// <param name="result">Result of the matrix multiplication as an output parameter.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Mult(in Matrix2D matrix1, in Matrix2D matrix2, out Matrix2D result)
		{
			var m11 = (matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21);
			var m12 = (matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22);

			var m21 = (matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21);
			var m22 = (matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22);

			var m31 = (matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21) + matrix2.M31;
			var m32 = (matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22) + matrix2.M32;

			result = new Matrix2D(m11, m12, m21, m22, m31, m32);
		}

		/// <summary>
		/// Creates an inverted the matrix from the input
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="result"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CreateInvert(in this Matrix2D matrix, out Matrix2D result)
		{
			var det = 1 / matrix.Determinant();

			result = new Matrix2D();
			result.M11 = matrix.M22 * det;
			result.M12 = -matrix.M12 * det;

			result.M21 = -matrix.M21 * det;
			result.M22 = matrix.M11 * det;

			result.M31 = (matrix.M32 * matrix.M21 - matrix.M31 * matrix.M22) * det;
			result.M32 = -(matrix.M32 * matrix.M11 - matrix.M31 * matrix.M12) * det;
		}

		/// <summary>
		/// Creates an inverted the matrix from the input
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="result"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Matrix2D CreateInvert(in this Matrix2D matrix)
		{
			Matrix2D result = new Matrix2D();
			var det = 1 / matrix.Determinant();

			result.M11 = matrix.M22 * det;
			result.M12 = -matrix.M12 * det;

			result.M21 = -matrix.M21 * det;
			result.M22 = matrix.M11 * det;

			result.M31 = (matrix.M32 * matrix.M21 - matrix.M31 * matrix.M22) * det;
			result.M32 = -(matrix.M32 * matrix.M11 - matrix.M31 * matrix.M12) * det;

			return result;
		}

		/// <summary>
		/// Creates an inverted the matrix from the input
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="result"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Invert(ref this Matrix2D matrix)
		{
			Matrix2D result = new Matrix2D();
			var det = 1 / matrix.Determinant();

			result.M11 = matrix.M22 * det;
			result.M12 = -matrix.M12 * det;

			result.M21 = -matrix.M21 * det;
			result.M22 = matrix.M11 * det;

			result.M31 = (matrix.M32 * matrix.M21 - matrix.M31 * matrix.M22) * det;
			result.M32 = -(matrix.M32 * matrix.M11 - matrix.M31 * matrix.M12) * det;

			matrix = result;
		}

		/// <summary>
		/// Gets the matrix determinant
		/// </summary>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Determinant(in this Matrix2D matrix)
		{
			return matrix.M11 * matrix.M22 - matrix.M12 * matrix.M21;
		}

		public static Matrix4 ToMat4(in this Matrix2D matrix)
        {
			return new Matrix4(
				matrix.M11, matrix.M12, 0, 0,
				matrix.M21, matrix.M22, 0, 0,
				0,			0,			1, 0,
				matrix.M31, matrix.M32, 0, 1);
        }
	}
}
