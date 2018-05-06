using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace panorama {
    public static class Ffmpeg {
        public static void CheckExistence() {
            if (!File.Exists(Config.FfmpegPath)) throw new FileNotFoundException("ffmpeg.exeが見つかりません。");
        }
        private static string EscapeParameter(string input) {
            // https://stackoverflow.com/questions/5510343/escape-command-line-arguments-in-c-sharp
            string s = Regex.Replace(input, @"(\\*)" + "\"", @"$1$1\" + "\"");
            return "\"" + Regex.Replace(s, @"(\\+)$", @"$1$1") + "\"";
        }
        public static void Run(Func<Func<string, string>, string> command) {
            using (var process = new Process()) {
                process.StartInfo = new ProcessStartInfo {
                    FileName = Config.FfmpegPath,
                    Arguments = command(Ffmpeg.EscapeParameter),
                    CreateNoWindow = false,
                    UseShellExecute = false,
                };
                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0) throw new ApplicationException($"ffmpegがコード{process.ExitCode}で終了しました。");
            }
        }
    }
}
