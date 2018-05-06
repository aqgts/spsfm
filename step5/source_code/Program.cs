using System;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace panorama {
    public static class Program {
        public static void Main(string[] args) {
            try {
                if (args.Length < Config.Angles.Length) throw new ApplicationException("動画ファイルをドラッグアンドドロップしてください。");
                if (!File.Exists(args[0])) throw new FileNotFoundException("ファイルが見つかりません。", args[0]);
                string aviPath = args[0];
                Ffmpeg.CheckExistence();

                if (!Directory.Exists(Config.WorkingDir)) Directory.CreateDirectory(Config.WorkingDir);
                Ffmpeg.Run(escape => $"-i {escape(aviPath)} -f image2 {escape(Path.Combine(Config.WorkingDir, "%06d.png"))}");

                double[,,] colorBuffer = new double[Config.OutputWidth, Config.OutputHeight, 3];
                for (int outputX = 0; outputX < Config.OutputWidth; outputX++) {
                    for (int outputY = 0; outputY < Config.OutputHeight; outputY++) {
                        colorBuffer[outputX, outputY, 0] = Double.NaN;
                        colorBuffer[outputX, outputY, 1] = Double.NaN;
                        colorBuffer[outputX, outputY, 2] = Double.NaN;
                    }
                }
                for (int panoramaPartId = 0; panoramaPartId < Config.Angles.Length; panoramaPartId++) {
                    Console.WriteLine(((double)panoramaPartId / Config.Angles.Length).ToString("P1", CultureInfo.InvariantCulture) + " 完了");

                    PanoramaPart panoramaPart = new PanoramaPart(Config.Angles[panoramaPartId], Config.AngleOfView);
                    using (var image = new ProcessableImage(Path.Combine(Config.WorkingDir, $"{(panoramaPartId + 1).ToString().PadLeft(6, '0')}.png")))
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
                        image.Save(Path.Combine(Config.ExeDir, "panorama.png"));
                    }
                }

                Console.WriteLine(1.0.ToString("P1", CultureInfo.InvariantCulture) + " 完了");
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
