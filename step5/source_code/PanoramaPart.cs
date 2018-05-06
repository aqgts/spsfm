using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace panorama {
    public class PanoramaPart {
        private readonly Quaternion rotation;
        private readonly double angleOfView;

        public PanoramaPart(Quaternion rotation, double angleOfView) {
            this.rotation = rotation;
            this.angleOfView = angleOfView;
        }
        public PanoramaPart(double yaw, double pitch, double angleOfView)
            : this(Quaternion.AngleAxis(yaw, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(pitch, new Vector3(-1, 0, 0)), angleOfView) {
        }

        public Vector2 GetUV(double yaw, double pitch) {
            Vector3 xyz = this.rotation.Inverse.Rotate(new Vector3(Math.Sin(yaw) * Math.Cos(pitch), Math.Sin(pitch), Math.Cos(yaw) * Math.Cos(pitch)));
            if (xyz.Z <= 0 || xyz.Y >= 1 || xyz.Y <= -1) return new Vector2(Double.NaN, Double.NaN);

            double tanRelativeYaw = xyz.X / xyz.Z;
            double tanRelativePitch = xyz.Y / Math.Sqrt(1 - xyz.Y * xyz.Y);
            double u = tanRelativeYaw / (2 * Math.Tan(this.angleOfView / 2)) + 0.5;
            double v = (tanRelativePitch * Math.Sqrt(tanRelativeYaw * tanRelativeYaw + 1)) / (2 * Math.Abs(Math.Tan(this.angleOfView / 2))) +0.5;
            return new Vector2(u, v);
        }
    }
}
