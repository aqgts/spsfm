using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace panorama {
    public readonly struct Coordinate {
        public int X { get; }
        public int Y { get; }
        public Coordinate(int x, int y) {
            this.X = x;
            this.Y = y;
        }
        public static Coordinate operator +(Coordinate lhs, Coordinate rhs) {
            return new Coordinate(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }
        public static bool operator ==(Coordinate lhs, Coordinate rhs) {
            return lhs.X == rhs.X && lhs.Y == rhs.Y;
        }
        public static bool operator !=(Coordinate lhs, Coordinate rhs) {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj) {
            if (!(obj is Coordinate)) {
                return false;
            }

            Coordinate other = (Coordinate)obj;
            return this == other;
        }

        public override int GetHashCode() {
            int hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + this.X.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Y.GetHashCode();
            return hashCode;
        }
    }
}
