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
        public static int FrameRate { get; } = 30;
        /*
        public static Quaternion[] Angles { get; } = new[] {
            Quaternion.AngleAxis(-Math.PI / 2, new Vector3(-1, 0, 0)),
            Quaternion.AngleAxis(Math.PI / 2, new Vector3(-1, 0, 0)),
            Quaternion.Identity,
            Quaternion.AngleAxis(Math.PI / 2, new Vector3(0, 1, 0)),
            Quaternion.AngleAxis(Math.PI, new Vector3(0, 1, 0)),
            Quaternion.AngleAxis(-Math.PI / 2, new Vector3(0, 1, 0)),
        };
        */
        public static Quaternion[] Angles { get; } = new[] {
            Quaternion.AngleAxis(-Math.PI / 3, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(-Math.PI / 3, new Vector3(-1, 0, 0)),
            Quaternion.AngleAxis(Math.PI, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(-Math.PI / 3, new Vector3(-1, 0, 0)),
            Quaternion.AngleAxis(Math.PI / 3, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(-Math.PI / 3, new Vector3(-1, 0, 0)),
            Quaternion.AngleAxis(-Math.PI / 3, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(Math.PI / 3, new Vector3(-1, 0, 0)),
            Quaternion.AngleAxis(Math.PI, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(Math.PI / 3, new Vector3(-1, 0, 0)),
            Quaternion.AngleAxis(Math.PI / 3, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(Math.PI / 3, new Vector3(-1, 0, 0)),
            Quaternion.Identity,
            Quaternion.AngleAxis(-Math.PI / 3, new Vector3(0, 1, 0)),
            Quaternion.AngleAxis(-Math.PI * 2 / 3, new Vector3(0, 1, 0)),
            Quaternion.AngleAxis(Math.PI, new Vector3(0, 1, 0)),
            Quaternion.AngleAxis(Math.PI * 2 / 3, new Vector3(0, 1, 0)),
            Quaternion.AngleAxis(Math.PI / 3, new Vector3(0, 1, 0)),
        };
        public static string[] AngleNames { get; } = Enumerable.Range(1, Angles.Length)
            .Select(i => i.ToString().PadLeft(2, '0'))
            .ToArray();
        public static double AngleOfView { get; }
        public static string ExeDir { get; } = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
        public static string FfmpegPath { get; } = Path.Combine(ExeDir, "ffmpeg.exe");
        public static string PreComputedDir { get; } = Path.Combine(ExeDir, "pre_computed");
        public static string WorkingDir { get; } = Path.Combine(ExeDir, "work");
        public static string[] WorkingSubDirs { get; } = AngleNames.Select(angleName => Path.Combine(WorkingDir, angleName)).ToArray();
        public static string[] OverlapStatePaths { get; } = Enumerable.Range(1, Angles.Length)
            .Select(i => Path.Combine(Config.PreComputedDir, $"overlap_state_{i.ToString().PadLeft(2, '0')}"))
            .ToArray();
        public static string[] RatePaths { get; } = Enumerable.Range(1, Angles.Length)
            .Select(i => Path.Combine(Config.PreComputedDir, $"rate_{i.ToString().PadLeft(2, '0')}"))
            .ToArray();

        static Config() {
            double unitRadian = Math.PI / 3;
            Func<double, double, double> calcAngleOfView = null;
            calcAngleOfView = (min, max) => {
                double x = (min + max) / 2;
                if (x == min || x == max) return x;
                double value = Math.Sin(x - unitRadian) * Math.Cos(x / 2) - 0.3 * Math.Cos(x / 2 - unitRadian) * Math.Sin(x);
                if (value < 0) return calcAngleOfView(x, max);
                if (value > 0) return calcAngleOfView(min, x);
                return x;
            };
            AngleOfView = Math.Round(calcAngleOfView(0, Math.PI) / Math.PI * 180) * Math.PI / 180;
        }
    }
}
