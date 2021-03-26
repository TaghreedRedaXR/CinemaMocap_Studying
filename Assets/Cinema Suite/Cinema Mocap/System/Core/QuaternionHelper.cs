using UnityEngine;

namespace CinemaSuite.CinemaMocap.System.Core
{
    public static class QuaternionHelper
    {
        /// <summary>
        /// Convert a Quaternion to Vector3 containing information about Euler Angles, in order of X, Y, and Z.
        /// </summary>
        /// <param name="q">The quaternion to be converted.</param>
        /// <returns>Vector3 containing information about Euler Angles, in order of X, Y, and Z.</returns>
        public static Vector3 ToEulerAnglesXYZ(Quaternion q)
        {
            Vector3 right = new Vector3();
            right = q * Vector3.right;

            Vector3 up = new Vector3();
            up = q * Vector3.up;

            Vector3 forward = new Vector3();
            forward = q * Vector3.forward;

            float yawAngle = 0f;
            float pitchAngle = Mathf.Rad2Deg * Mathf.Asin(-right.z);
            float rollAngle = 0f;

            if (pitchAngle < 90f)
            {
                if (pitchAngle > -90f)
                {
                    yawAngle = Mathf.Rad2Deg * Mathf.Atan2(up.z, forward.z);
                    rollAngle = Mathf.Rad2Deg * Mathf.Atan2(right.y, right.x);
                }
                else
                {
                    rollAngle = 0f;
                    yawAngle = rollAngle - Mathf.Rad2Deg * Mathf.Atan2(-up.x, up.y);
                }
            }
            else
            {
                rollAngle = 0f;
                yawAngle = Mathf.Rad2Deg * Mathf.Atan2(-up.x, up.y) - rollAngle;
            }

            return new Vector3(yawAngle, pitchAngle, rollAngle);
        }

        public static Quaternion FromEulerAnglesXYZ(Vector3 v)
        {
            float[,] yawMatrix = new float[3, 3];
            yawMatrix[0, 0] = 1;

            float[,] pitchMatrix = new float[3, 3];
            pitchMatrix[1, 1] = 1;

            float[,] rollMatrix = new float[3, 3];
            rollMatrix[2, 2] = 1;

            yawMatrix[1, 1] = Mathf.Cos(Mathf.Deg2Rad * v.x);
            yawMatrix[2, 2] = yawMatrix[1, 1];
            yawMatrix[1, 2] = Mathf.Sin(Mathf.Deg2Rad * v.x);
            yawMatrix[2, 1] = -yawMatrix[1, 2];

            pitchMatrix[2, 2] = Mathf.Cos(Mathf.Deg2Rad * v.y);
            pitchMatrix[0, 0] = pitchMatrix[2, 2];
            pitchMatrix[2, 0] = Mathf.Sin(Mathf.Deg2Rad * v.y);
            pitchMatrix[0, 2] = -pitchMatrix[2, 0];

            rollMatrix[0, 0] = Mathf.Cos(Mathf.Deg2Rad * v.z);
            rollMatrix[1, 1] = rollMatrix[0, 0];
            rollMatrix[0, 1] = Mathf.Sin(Mathf.Deg2Rad * v.z);
            rollMatrix[1, 0] = -rollMatrix[0, 1];

            float[,] m = MatrixMultiply(MatrixMultiply(yawMatrix, pitchMatrix), rollMatrix);

            float t = trace(m);
            float root = 0f;
            Vector4 components = Vector4.zero;

            if (t > 0f)
            {
                root = Mathf.Sqrt(t + 1f);
                components.w = 0.5f * root;
                root = 0.5f / root;
                components.x = (m[2, 1] - m[1, 2]) * root;
                components.y = (m[0, 2] - m[2, 0]) * root;
                components.z = (m[1, 0] - m[0, 1]) * root;
            }
            else
            {
                int[] iNext = new int[3] { 1, 2, 0 };
                int i = 0;
                if (m[1, 1] > m[0, 0]) i = 1;
                if (m[2, 2] > m[i, i]) i = 2;
                int j = iNext[i];
                int k = iNext[j];

                root = Mathf.Sqrt(m[i, i] - m[j, j] - m[k, k] + 1f);
                components[i] = 0.5f * root;
                root = 0.5f / root;
                components[3] = (m[k, j] - m[j, k]) * root;
                components[j] = (m[j, i] + m[i, j]) * root;
                components[k] = (m[k, i] + m[i, k]) * root;
            }

            return new Quaternion(components.x, components.y, components.z, -components.w);
        }

        /// <summary>
        /// 4x4 Matrix Mulitply
        /// </summary>
        /// <param name="m1">The first matrix.</param>
        /// <param name="m2">The second matrix.</param>
        /// <returns>The result of the 4x4 matrix multiply.</returns>
        private static float[,] MatrixMultiply(float[,] m1, float[,] m2)
        {
            float[,] s = new float[3, 3];
            for (int m1Row = 0; m1Row < 3; m1Row++)
                for (int m2Col = 0; m2Col < 3; m2Col++)
                    for (int m1Col = 0; m1Col < 3; m1Col++)
                        s[m1Row, m2Col] += m1[m1Row, m1Col] * m2[m1Col, m2Col];
            return s;
        }

        /// <summary>
        /// Retrieve the trace value of a 4x4 Matrix.
        /// </summary>
        /// <param name="matrix">The matrix to discover the trace value of.</param>
        /// <returns>The trace value of the matrix.</returns>
        private static float trace(float[,] matrix)
        {
            float t = 0f;
            for (int i = 0; i < 3; i++) t += matrix[i, i];
            return t;
        }

        /// <summary>
        /// Convert a Right-handed Euler rotation to a Left-handed Quaternion.
        /// </summary>
        /// <param name="rotation">The right-handed euler rotation.</param>
        /// <returns>A left-handed quaternion.</returns>
        public static Quaternion RHStoLHS(Vector3 rotation)
        {
            Vector3 flippedRotation = new Vector3(rotation.x, -rotation.y, -rotation.z);
            Quaternion qx = Quaternion.AngleAxis(flippedRotation.x, Vector3.right);
            Quaternion qy = Quaternion.AngleAxis(flippedRotation.y, Vector3.up);
            Quaternion qz = Quaternion.AngleAxis(flippedRotation.z, Vector3.forward);
            return (qz * qy * qx);
        }
    }
}