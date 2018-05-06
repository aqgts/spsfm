using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace panorama {
    public static class ProgramForVideo {
        public static void Main(string[] args) {
            try {
                if (args.Length < Config.Angles.Length) throw new ApplicationException($"{Config.Angles.Length}方向分の動画ファイルをドラッグアンドドロップしてください。");
                string[] aviPaths = Enumerable.Range(0, Config.Angles.Length).Select(panoramaPartId => {
                    string aviPath = args.First(arg => File.Exists(arg) && Path.GetFileName(arg) == $"{Config.AngleNames[panoramaPartId]}.avi");
                    if (!File.Exists(aviPath)) throw new ArgumentException($"動画ファイル {Config.AngleNames[panoramaPartId]}.avi が見つかりません。", "args");
                    return aviPath;
                }).ToArray();
                Ffmpeg.CheckExistence();

                if (!Directory.Exists(Config.WorkingDir)) Directory.CreateDirectory(Config.WorkingDir);
                for (int panoramaPartId = 0; panoramaPartId < Config.Angles.Length; panoramaPartId++) {
                    string aviPath = aviPaths[panoramaPartId];
                    string workingSubDir = Config.WorkingSubDirs[panoramaPartId];
                    if (!Directory.Exists(workingSubDir)) {
                        Directory.CreateDirectory(workingSubDir);
                        Ffmpeg.Run(escape => $"-i {escape(aviPath)} -f image2 {escape(Path.Combine(workingSubDir, "%06d.png"))}");
                    }
                }

                int frameCount = Directory.GetFiles(Config.WorkingSubDirs[0], "*.png", SearchOption.TopDirectoryOnly).Length;
                int processedFrameCount = 0;
                Object lockObject = new object();
                Parallel.ForEach(Enumerable.Range(0, frameCount), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, frameNumber => {
                    if (!File.Exists(Path.Combine(Config.WorkingDir, $"{(frameNumber + 1).ToString().PadLeft(6, '0')}.png"))) {
                        double[,,] colorBuffer = new double[Config.OutputWidth, Config.OutputHeight, 3];
                        for (int outputX = 0; outputX < Config.OutputWidth; outputX++) {
                            for (int outputY = 0; outputY < Config.OutputHeight; outputY++) {
                                colorBuffer[outputX, outputY, 0] = Double.NaN;
                                colorBuffer[outputX, outputY, 1] = Double.NaN;
                                colorBuffer[outputX, outputY, 2] = Double.NaN;
                            }
                        }
                        for (int panoramaPartId = 0; panoramaPartId < Config.Angles.Length; panoramaPartId++) {
                            PanoramaPart panoramaPart = new PanoramaPart(Config.Angles[panoramaPartId], Config.AngleOfView);
                            using (var image = new ProcessableImage(Path.Combine(Config.WorkingSubDirs[panoramaPartId], $"{(frameNumber + 1).ToString().PadLeft(6, '0')}.png")))
                            using (var fs1 = new FileStream(Config.OverlapStatePaths[panoramaPartId], FileMode.Open, FileAccess.Read, FileShare.Read))
                            using (var reader1 = new BinaryReader(fs1))
                            using (var fs2 = new FileStream(Config.RatePaths[panoramaPartId], FileMode.Open, FileAccess.Read, FileShare.Read))
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
                        }

                        using (ProcessableImage image = new ProcessableImage(Config.OutputWidth, Config.OutputHeight)) {
                            for (int outputX = 0; outputX < Config.OutputWidth; outputX++) {
                                for (int outputY = 0; outputY < Config.OutputHeight; outputY++) {
                                    if (Double.IsNaN(colorBuffer[outputX, outputY, 0])) {
                                        image.SetColorByXy(outputX, outputY, Color.FromArgb(0, 0, 0));
                                    } else {
                                        image.SetColorByXy(
                                            outputX,
                                            outputY,
                                            Color.FromArgb(
                                                (byte)Math.Round(colorBuffer[outputX, outputY, 0]),
                                                (byte)Math.Round(colorBuffer[outputX, outputY, 1]),
                                                (byte)Math.Round(colorBuffer[outputX, outputY, 2])
                                            )
                                        );
                                    }
                                }
                            }
                            image.Save(Path.Combine(Config.WorkingDir, $"{(frameNumber + 1).ToString().PadLeft(6, '0')}.png"));
                        }
                    }
                    
                    lock (lockObject) {
                        processedFrameCount++;
                        Console.WriteLine($"{processedFrameCount}/{frameCount} フレームのパノラマ合成完了");
                    }
                });

                Ffmpeg.Run(escape => $"-framerate {Config.FrameRate} -i {escape(Path.Combine(Config.WorkingDir, "%06d.png"))} -vcodec libx264 -pix_fmt yuv420p -r {Config.FrameRate} {escape(Path.Combine(Config.ExeDir, "panorama.mp4"))}");
            } catch (Exception e) {
                Console.Error.WriteLine(e.ToString());
            } finally {
                if (Directory.Exists(Config.WorkingDir)) Directory.Delete(Config.WorkingDir, true);
                Console.WriteLine("Enterキーを押すと終了します。");
                Console.ReadLine();
            }
        }
    }
}
