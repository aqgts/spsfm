using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;

namespace panorama {
    public static class Program {
        public static void Main(string[] args) {
            try {
                if (args.Length != 1) throw new ApplicationException("パノラマ画像化したい動画ファイルをドラッグアンドドロップしてください。");
                if (!File.Exists(args[0])) throw new ArgumentException("引数がファイルではありません。", "args");
                string aviFile = args[0];
                if (!File.Exists(Path.Combine(Config.ExeDir, "ffmpeg.exe"))) throw new FileNotFoundException("ffmpeg.exeが見つかりません。");

                string[] overlapStatePaths = Enumerable.Range(1, Config.YawDivisionCount * Config.PitchDivisionCount).Select(i => Path.Combine(Config.PreComputedDir, $"overlap_state_{i.ToString().PadLeft(2, '0')}")).ToArray();
                string[] ratePaths = Enumerable.Range(1, Config.YawDivisionCount * Config.PitchDivisionCount).Select(i => Path.Combine(Config.PreComputedDir, $"rate_{i.ToString().PadLeft(2, '0')}")).ToArray();
                string[] panoramaImagePaths = Enumerable.Range(1, Config.YawDivisionCount * Config.PitchDivisionCount).Select(i => Path.Combine(Config.PanoramaImageDir, $"{i.ToString().PadLeft(2, '0')}.png")).ToArray();

                if (!Directory.Exists(Config.PanoramaImageDir)) Directory.CreateDirectory(Config.PanoramaImageDir);
                // https://stackoverflow.com/questions/5510343/escape-command-line-arguments-in-c-sharp
                Func<string, string> escapeParameter = input => {
                    string s = Regex.Replace(input, @"(\\*)" + "\"", @"$1$1\" + "\"");
                    return "\"" + Regex.Replace(s, @"(\\+)$", @"$1$1") + "\"";
                };
                using (var process = new Process()) {
                    process.StartInfo = new ProcessStartInfo {
                        FileName = Path.Combine(Config.ExeDir, "ffmpeg.exe"),
                        Arguments = $"-i {escapeParameter(aviFile)} -f image2 {escapeParameter(Path.Combine(Config.PanoramaImageDir, "%02d.png"))}",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                    };
                    process.Start();
                    process.WaitForExit();
                    if (process.ExitCode != 0) throw new ApplicationException($"ffmpegがコード{process.ExitCode}で終了しました。");
                }

                double[,,] colorBuffer = new double[Config.OutputWidth, Config.OutputHeight, 3];
                for (int outputX = 0; outputX < Config.OutputWidth; outputX++) {
                    for (int outputY = 0; outputY < Config.OutputHeight; outputY++) {
                        colorBuffer[outputX, outputY, 0] = Double.NaN;
                        colorBuffer[outputX, outputY, 1] = Double.NaN;
                        colorBuffer[outputX, outputY, 2] = Double.NaN;
                    }
                }
                for (int pitchIndex = 0; pitchIndex < Config.PitchDivisionCount; pitchIndex++) {
                    for (int yawIndex = 0; yawIndex < Config.YawDivisionCount; yawIndex++) {
                        int panoramaPartId = pitchIndex * Config.YawDivisionCount + yawIndex;
                        Console.WriteLine(((double)panoramaPartId / (Config.YawDivisionCount * Config.PitchDivisionCount)).ToString("P1", CultureInfo.InvariantCulture) + " 完了");

                        PanoramaPart panoramaPart = new PanoramaPart(yawIndex * -Config.UnitRadian, pitchIndex * Config.UnitRadian - (Math.PI / 2 - Config.UnitRadian / 2), Config.AngleOfView);
                        using (var image = new ProcessableImage(panoramaImagePaths[panoramaPartId]))
                        using (var fs1 = new FileStream(overlapStatePaths[panoramaPartId], FileMode.Open))
                        using (var reader1 = new BinaryReader(fs1))
                        using (var fs2 = new FileStream(ratePaths[panoramaPartId], FileMode.Open))
                        using (var reader2 = new BinaryReader(fs2)) {
                            for (int outputX = 0; outputX < Config.OutputWidth; outputX++) {
                                for (int outputY = 0; outputY < Config.OutputHeight; outputY++) {
                                    Vector2 uv;
                                    Color color;
                                    double blendRate;
                                    switch ((OverlapState)Enum.ToObject(typeof(OverlapState), reader1.ReadByte())) {
                                        case OverlapState.Overlapped:
                                            uv = panoramaPart.GetUV(((outputX + 0.5) / Config.OutputWidth - 0.5) * 2 * Math.PI, ((outputY + 0.5) / Config.OutputHeight - 0.5) * Math.PI);
                                            color = image.GetColorByUv(uv.X, uv.Y);
                                            blendRate = reader2.ReadDouble();
                                            colorBuffer[outputX, outputY, 0] = colorBuffer[outputX, outputY, 0] * blendRate + color.R * (1 - blendRate);
                                            colorBuffer[outputX, outputY, 1] = colorBuffer[outputX, outputY, 1] * blendRate + color.G * (1 - blendRate);
                                            colorBuffer[outputX, outputY, 2] = colorBuffer[outputX, outputY, 2] * blendRate + color.B * (1 - blendRate);
                                            break;
                                        case OverlapState.OnlyOther:
                                            uv = panoramaPart.GetUV(((outputX + 0.5) / Config.OutputWidth - 0.5) * 2 * Math.PI, ((outputY + 0.5) / Config.OutputHeight - 0.5) * Math.PI);
                                            color = image.GetColorByUv(uv.X, uv.Y);
                                            colorBuffer[outputX, outputY, 0] = color.R;
                                            colorBuffer[outputX, outputY, 1] = color.G;
                                            colorBuffer[outputX, outputY, 2] = color.B;
                                            break;
                                    }
                                }
                            }
                        }

                        using (ProcessableImage image = new ProcessableImage(Config.OutputWidth, Config.OutputHeight)) {
                            for (int outputX = 0; outputX < Config.OutputWidth; outputX++) {
                                int inputX = ((outputX - Config.OutputWidth / 4) % Config.OutputWidth + Config.OutputWidth) % Config.OutputWidth;
                                for (int outputY = 0; outputY < Config.OutputHeight; outputY++) {
                                    int inputY = outputY;
                                    if (Double.IsNaN(colorBuffer[inputX, inputY, 0])) {
                                        image.SetColorByXy(outputX, outputY, Color.FromArgb(0, 0, 0));
                                    } else {
                                        image.SetColorByXy(
                                            outputX,
                                            outputY,
                                            Color.FromArgb(
                                                (byte)Math.Round(colorBuffer[inputX, inputY, 0]),
                                                (byte)Math.Round(colorBuffer[inputX, inputY, 1]),
                                                (byte)Math.Round(colorBuffer[inputX, inputY, 2])
                                            )
                                        );
                                    }
                                }
                            }
                            image.Save(Config.OutputFile);
                        }
                    }
                }

                Directory.Delete(Config.PanoramaImageDir, true);

                Console.WriteLine(1.0.ToString("P1", CultureInfo.InvariantCulture) + " 完了");
            } catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
            } finally {
                Console.WriteLine("何かキーを押すと終了します。");
                Console.ReadLine();
            }
        }
    }
}
