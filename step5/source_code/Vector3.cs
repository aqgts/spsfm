using System;

namespace panorama {
    public readonly struct Vector3 {
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public Vector3(double x, double y, double z) {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public double Norm {
            get { return Math.Sqrt(this.SquaredNorm); }
        }
        public double SquaredNorm {
            get { return this.X * this.X + this.Y * this.Y + this.Z * this.Z; }
        }
        public static Vector3 operator +(Vector3 lhs, Vector3 rhs) {
            return new Vector3(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        }
        public static Vector3 operator -(Vector3 self) {
            return new Vector3(-self.X, -self.Y, -self.Z);
        }
        public static Vector3 operator *(Vector3 self, double scale) {
            return new Vector3(self.X * scale, self.Y * scale, self.Z * scale);
        }
        public static Vector3 operator *(double scale, Vector3 self) {
            return new Vector3(self.X * scale, self.Y * scale, self.Z * scale);
        }
        public static Vector3 operator /(Vector3 self, double factor) {
            return new Vector3(self.X / factor, self.Y / factor, self.Z / factor);
        }
        public double InnerProduct(Vector3 other) {
            return this.X * other.X + this.Y * other.Y + this.Z * other.Z;
        }
        public Vector3 CrossProduct(Vector3 other) {
            return new Vector3(
                this.Y * other.Z - this.Z * other.Y,
                this.Z * other.X - this.X * other.Z,
                this.X * other.Y - this.Y * other.X
            );
        }
        public Vector3 Normalize() {
            double squaredNorm = this.SquaredNorm;
            if (squaredNorm == 1) {
                return this;
            } else {
                return this / Math.Sqrt(squaredNorm);
            }
        }
    }
}
