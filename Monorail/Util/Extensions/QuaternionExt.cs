
using OpenTK.Mathematics;

namespace Monorail.Util
{
    public static class QuaternionExt
    {
        public static void Divide(Quaternion left, Quaternion right, out Quaternion quaternion)
        {
            quaternion = new Quaternion();

            float q1x = left.X;
            float q1y = left.Y;
            float q1z = left.Z;
            float q1w = left.W;

            //-------------------------------------
            // Inverse part.
            float ls = right.X * right.X + right.Y * right.Y +
                       right.Z * right.Z + right.W * right.W;
            float invNorm = 1.0f / ls;

            float q2x = -right.X * invNorm;
            float q2y = -right.Y * invNorm;
            float q2z = -right.Z * invNorm;
            float q2w = right.W * invNorm;

            //-------------------------------------
            // Multiply part.

            // cross(av, bv)
            float cx = q1y * q2z - q1z * q2y;
            float cy = q1z * q2x - q1x * q2z;
            float cz = q1x * q2y - q1y * q2x;

            float dot = q1x * q2x + q1y * q2y + q1z * q2z;

            quaternion.X = q1x * q2w + q2x * q1w + cx;
            quaternion.Y = q1y * q2w + q2y * q1w + cy;
            quaternion.Z = q1z * q2w + q2z * q1w + cz;
            quaternion.W = q1w * q2w - dot;
        }

        public static Quaternion Divide(Quaternion left, Quaternion right)
        {
            var quaternion = new Quaternion();

            float q1x = left.X;
            float q1y = left.Y;
            float q1z = left.Z;
            float q1w = left.W;

            //-------------------------------------
            // Inverse part.
            float ls = right.X * right.X + right.Y * right.Y +
                       right.Z * right.Z + right.W * right.W;
            float invNorm = 1.0f / ls;

            float q2x = -right.X * invNorm;
            float q2y = -right.Y * invNorm;
            float q2z = -right.Z * invNorm;
            float q2w = right.W * invNorm;

            //-------------------------------------
            // Multiply part.

            // cross(av, bv)
            float cx = q1y * q2z - q1z * q2y;
            float cy = q1z * q2x - q1x * q2z;
            float cz = q1x * q2y - q1y * q2x;

            float dot = q1x * q2x + q1y * q2y + q1z * q2z;

            quaternion.X = q1x * q2w + q2x * q1w + cx;
            quaternion.Y = q1y * q2w + q2y * q1w + cy;
            quaternion.Z = q1z * q2w + q2z * q1w + cz;
            quaternion.W = q1w * q2w - dot;

            return quaternion;
        }

        public static void Divide(in this Quaternion left, Quaternion right, out Quaternion quaternion)
        {
            quaternion = new Quaternion();

            float q1x = left.X;
            float q1y = left.Y;
            float q1z = left.Z;
            float q1w = left.W;

            //-------------------------------------
            // Inverse part.
            float ls = right.X * right.X + right.Y * right.Y +
                       right.Z * right.Z + right.W * right.W;
            float invNorm = 1.0f / ls;

            float q2x = -right.X * invNorm;
            float q2y = -right.Y * invNorm;
            float q2z = -right.Z * invNorm;
            float q2w = right.W * invNorm;

            //-------------------------------------
            // Multiply part.

            // cross(av, bv)
            float cx = q1y * q2z - q1z * q2y;
            float cy = q1z * q2x - q1x * q2z;
            float cz = q1x * q2y - q1y * q2x;

            float dot = q1x * q2x + q1y * q2y + q1z * q2z;

            quaternion.X = q1x * q2w + q2x * q1w + cx;
            quaternion.Y = q1y * q2w + q2y * q1w + cy;
            quaternion.Z = q1z * q2w + q2z * q1w + cz;
            quaternion.W = q1w * q2w - dot;
        }

        public static Quaternion Divide(in this Quaternion left, Quaternion right)
        {
            var quaternion = new Quaternion();

            float q1x = left.X;
            float q1y = left.Y;
            float q1z = left.Z;
            float q1w = left.W;

            //-------------------------------------
            // Inverse part.
            float ls = right.X * right.X + right.Y * right.Y +
                       right.Z * right.Z + right.W * right.W;
            float invNorm = 1.0f / ls;

            float q2x = -right.X * invNorm;
            float q2y = -right.Y * invNorm;
            float q2z = -right.Z * invNorm;
            float q2w = right.W * invNorm;

            //-------------------------------------
            // Multiply part.

            // cross(av, bv)
            float cx = q1y * q2z - q1z * q2y;
            float cy = q1z * q2x - q1x * q2z;
            float cz = q1x * q2y - q1y * q2x;

            float dot = q1x * q2x + q1y * q2y + q1z * q2z;

            quaternion.X = q1x * q2w + q2x * q1w + cx;
            quaternion.Y = q1y * q2w + q2y * q1w + cy;
            quaternion.Z = q1z * q2w + q2z * q1w + cz;
            quaternion.W = q1w * q2w - dot;

            return quaternion;
        }

        /// <summary>
        /// Calculates the direction the quaternion is facing
        /// </summary>
        /// <param name="quat"></param>
        /// <returns>A vector pointing at the facing direction</returns>
        public static Vector3 Direction(in this Quaternion quat)
        {
            return new Vector3(
                2 * (quat.X * quat.Z - quat.W * quat.Y),
                2 * (quat.Y * quat.Z + quat.W * quat.X),
                1 - 2 * (quat.X * quat.X + quat.Y * quat.Y)
            );
        }
    }
}
