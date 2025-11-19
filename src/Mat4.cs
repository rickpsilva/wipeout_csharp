using System;

namespace WipeoutRewrite
{
    /// <summary>
    /// 4x4 transformation matrix for 3D graphics.
    /// Column-major order (OpenGL style).
    /// </summary>
    public struct Mat4
    {
        public float[] M; // 16 floats in column-major order
        
        public Mat4(float[] m) 
        { 
            if (m.Length != 16)
                throw new ArgumentException("Matrix must have 16 elements");
            M = m; 
        }
        
        /// <summary>
        /// Create identity matrix.
        /// </summary>
        public static Mat4 Identity()
        {
            return new Mat4(new float[] {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            });
        }
        
        /// <summary>
        /// Create translation matrix.
        /// </summary>
        public static Mat4 Translation(Vec3 v)
        {
            return new Mat4(new float[] {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                v.X, v.Y, v.Z, 1
            });
        }
        
        /// <summary>
        /// Create rotation matrix from Euler angles (pitch, yaw, roll).
        /// </summary>
        public static Mat4 FromEulerAngles(Vec3 angles)
        {
            float sx = MathF.Sin(angles.X);
            float cx = MathF.Cos(angles.X);
            float sy = MathF.Sin(angles.Y);
            float cy = MathF.Cos(angles.Y);
            float sz = MathF.Sin(angles.Z);
            float cz = MathF.Cos(angles.Z);
            
            // Combined rotation matrix (Rx * Ry * Rz)
            return new Mat4(new float[] {
                cy * cz,                    // m00
                cy * sz,                    // m01
                -sy,                        // m02
                0,                          // m03
                
                sx * sy * cz - cx * sz,     // m10
                sx * sy * sz + cx * cz,     // m11
                sx * cy,                    // m12
                0,                          // m13
                
                cx * sy * cz + sx * sz,     // m20
                cx * sy * sz - sx * cz,     // m21
                cx * cy,                    // m22
                0,                          // m23
                
                0, 0, 0, 1                  // m30-m33
            });
        }
        
        /// <summary>
        /// Create transformation matrix from position and rotation.
        /// </summary>
        public static Mat4 FromPositionAndAngles(Vec3 position, Vec3 angles)
        {
            Mat4 rotation = FromEulerAngles(angles);
            
            // Set translation in the rotation matrix
            rotation.M[12] = position.X;
            rotation.M[13] = position.Y;
            rotation.M[14] = position.Z;
            
            return rotation;
        }
        
        /// <summary>
        /// Multiply two matrices.
        /// </summary>
        public static Mat4 Multiply(Mat4 a, Mat4 b)
        {
            float[] result = new float[16];
            
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    result[col * 4 + row] = 
                        a.M[0 * 4 + row] * b.M[col * 4 + 0] +
                        a.M[1 * 4 + row] * b.M[col * 4 + 1] +
                        a.M[2 * 4 + row] * b.M[col * 4 + 2] +
                        a.M[3 * 4 + row] * b.M[col * 4 + 3];
                }
            }
            
            return new Mat4(result);
        }
        
        public static Mat4 operator *(Mat4 a, Mat4 b) => Multiply(a, b);
    }
}
