using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace panorama {
    public readonly struct Quaternion {
        public double W { get; }
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public Quaternion(double w, double x, double y, double z) {
            this.W = w;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public double Abs {
            get { return Math.Sqrt(this.Abs2); }
        }
        public double Abs2 {
            get { return this.W * this.W + this.X * this.X + this.Y * this.Y + this.Z * this.Z; }
        }
        public double Real {
            get { return this.W; }
        }
        public Vector3 Imag {
            get { return new Vector3(this.X, this.Y, this.Z); }
        }
        public Quaternion Conjugate {
            get { return new Quaternion(this.W, -this.X, -this.Y, -this.Z); }
        }
        public Quaternion Inverse {
            get {
                Quaternion conj = this.Conjugate;
                double abs2 = this.Abs2;
                return new Quaternion(conj.W / abs2, conj.X / abs2, conj.Y / abs2, conj.Z / abs2);
            }
        }
        public static Quaternion operator *(Quaternion lhs, Quaternion rhs) {
            double real = lhs.Real * rhs.Real - lhs.Imag.InnerProduct(rhs.Imag);
            Vector3 imag = rhs.Imag * lhs.Real + lhs.Imag * rhs.Real + lhs.Imag.CrossProduct(rhs.Imag);
            return new Quaternion(real, imag.X, imag.Y, imag.Z);
        }
        public Vector3 Rotate(Vector3 vector) {
            return (this * new Quaternion(0, vector.X, vector.Y, vector.Z) * this.Conjugate).Imag;
        }
        public static Quaternion AngleAxis(double angle, Vector3 axis) {
            double real = Math.Cos(angle / 2);
            Vector3 imag = axis.Normalize() * Math.Sin(angle / 2);
            return new Quaternion(real, imag.X, imag.Y, imag.Z);
        }
    }
}
