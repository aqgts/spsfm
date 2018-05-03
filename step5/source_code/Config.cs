using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace panorama {
    public static class Config {
        public static int OutputHeight { get; } = 2048;
        public static int OutputWidth { get; } = OutputHeight * 2;
        public static int PitchDivisionCount { get; } = 6;
        public static int YawDivisionCount { get; } = PitchDivisionCount * 2;
        public static double UnitRadian { get; } = Math.PI / PitchDivisionCount;
        public static double AngleOfView { get; }
        public static string ExeDir { get; } = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        public static string PreComputedDir { get; } = Path.Combine(ExeDir, "pre_computed");
        public static string PanoramaImageDir { get; } = Path.Combine(ExeDir, "panorama_images");
        public static string OutputFile { get; } = Path.Combine(ExeDir, "panorama.png");

        static Config() {
            Func<double, double, double> calcAngleOfView = null;
            calcAngleOfView = (min, max) => {
                double x = (min + max) / 2;
                if (x == min || x == max) return x;
                double value = Math.Sin(x - UnitRadian) * Math.Cos(x / 2) - 0.3 * Math.Cos(x / 2 - UnitRadian) * Math.Sin(x);
                if (value < 0) return calcAngleOfView(x, max);
                if (value > 0) return calcAngleOfView(min, x);
                return x;
            };
            AngleOfView = Math.Round(calcAngleOfView(0, Math.PI) / Math.PI * 180) * Math.PI / 180;
        }
    }
}
