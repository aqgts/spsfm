using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace panorama {
    public readonly struct Vector2 {
        public double X { get; }
        public double Y { get; }
        public Vector2(double x, double y) {
            this.X = x;
            this.Y = y;
        }
        public double Norm {
            get { return Math.Sqrt(this.SquaredNorm); }
        }
        public double SquaredNorm {
            get { return this.X * this.X + this.Y * this.Y; }
        }
        public static Vector2 operator +(Vector2 lhs, Vector2 rhs) {
            return new Vector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }
        public static Vector2 operator -(Vector2 self) {
            return new Vector2(-self.X, -self.Y);
        }
        public static Vector2 operator *(Vector2 self, double scale) {
            return new Vector2(self.X * scale, self.Y * scale);
        }
        public static Vector2 operator *(double scale, Vector2 self) {
            return new Vector2(self.X * scale, self.Y * scale);
        }
        public static Vector2 operator /(Vector2 self, double factor) {
            return new Vector2(self.X / factor, self.Y / factor);
        }
        public double InnerProduct(Vector2 other) {
            return this.X * other.X + this.Y * other.Y;
        }
        public Vector2 Normalize() {
            double squaredNorm = this.SquaredNorm;
            if (squaredNorm == 1) {
                return this;
            } else {
                return this / Math.Sqrt(squaredNorm);
            }
        }
    }
}
