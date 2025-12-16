using System;

namespace WipeoutRewrite
{
    /// <summary>
    /// 3D vector with basic mathematical operations.
    /// </summary>
    public struct Vec3
    {
        public float X, Y, Z;
        
        public Vec3(float x, float y, float z) 
        { 
            X = x; 
            Y = y; 
            Z = z; 
        }
        
        /// <summary>
        /// Add two vectors.
        /// </summary>
        public readonly Vec3 Add(Vec3 other) => new(X + other.X, Y + other.Y, Z + other.Z);
        
        /// <summary>
        /// Subtract two vectors.
        /// </summary>
        public readonly Vec3 Subtract(Vec3 other) => new(X - other.X, Y - other.Y, Z - other.Z);
        
        /// <summary>
        /// Multiply vector by scalar.
        /// </summary>
        public readonly Vec3 Multiply(float scalar) => new(X * scalar, Y * scalar, Z * scalar);
        
        /// <summary>
        /// Divide vector by scalar.
        /// </summary>
        public readonly Vec3 Divide(float scalar) => new(X / scalar, Y / scalar, Z / scalar);
        
        /// <summary>
        /// Calculate vector length (magnitude).
        /// </summary>
        public readonly float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z);
        
        /// <summary>
        /// Calculate squared length (faster than Length, useful for comparisons).
        /// </summary>
        public readonly float LengthSquared() => X * X + Y * Y + Z * Z;
        
        /// <summary>
        /// Normalize vector (make length = 1).
        /// </summary>
        public Vec3 Normalize()
        {
            float len = Length();
            return len > 0 ? Divide(len) : this;
        }
        
        /// <summary>
        /// Calculate dot product with another vector.
        /// </summary>
        public readonly float Dot(Vec3 other) => X * other.X + Y * other.Y + Z * other.Z;
        
        /// <summary>
        /// Calculate cross product with another vector.
        /// </summary>
        public readonly Vec3 Cross(Vec3 other) => new(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        );
        
        /// <summary>
        /// Calculate distance to another vector.
        /// </summary>
        public float DistanceTo(Vec3 other) => Subtract(other).Length();
        
        /// <summary>
        /// Calculate distance from this point to a plane defined by a point on the plane and its normal.
        /// Returns positive if point is on the side of the normal, negative otherwise.
        /// Based on vec3_distance_to_plane from wipeout-rewrite/src/types.c
        /// </summary>
        public readonly float DistanceToPlane(Vec3 planePoint, Vec3 planeNormal)
        {
            Vec3 diff = this - planePoint;
            return diff.Dot(planeNormal);
        }
        
        /// <summary>
        /// Project this point onto a plane defined by a point and normal.
        /// Returns the closest point on the plane.
        /// Based on ship_draw_shadow projection logic from ship.c
        /// </summary>
        public Vec3 ProjectOntoPlane(Vec3 planePoint, Vec3 planeNormal)
        {
            float distance = DistanceToPlane(planePoint, planeNormal);
            return this - planeNormal * distance;
        }
        
        // Operator overloads for convenience
        public static Vec3 operator +(Vec3 a, Vec3 b) => a.Add(b);
        public static Vec3 operator -(Vec3 a, Vec3 b) => a.Subtract(b);
        public static Vec3 operator *(Vec3 v, float s) => v.Multiply(s);
        public static Vec3 operator *(float s, Vec3 v) => v.Multiply(s);
        public static Vec3 operator /(Vec3 v, float s) => v.Divide(s);
        
        public override readonly string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }
}
