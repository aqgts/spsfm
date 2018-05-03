using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace panorama {
    public static class PreCompute {
        public static void Main(string[] args) {
            try {
                if (!Directory.Exists(Config.PreComputedDir)) Directory.CreateDirectory(Config.PreComputedDir);

                var getNeighbor = new Func<Coordinate, (Coordinate, int)>[] {
                origin => (new Coordinate(origin.X == 0 ? Config.OutputWidth - 1 : origin.X - 1, origin.Y), 0),
                origin => (new Coordinate(origin.X == Config.OutputWidth - 1 ? 0 : origin.X + 1, origin.Y), 1),
                origin => origin.Y == 0 ? (new Coordinate((origin.X + Config.OutputWidth / 2) % Config.OutputWidth, 0), 3) : (new Coordinate(origin.X, origin.Y - 1), 2),
                origin => origin.Y == Config.OutputHeight - 1 ? (new Coordinate((origin.X + Config.OutputWidth / 2) % Config.OutputWidth, Config.OutputHeight - 1), 2) : (new Coordinate(origin.X, origin.Y + 1), 3),
            };
                int[] getReverse = { 1, 0, 3, 2 };
                bool[,] existenceBuffer = new bool[Config.OutputWidth, Config.OutputHeight];
                for (int outputX = 0; outputX < Config.OutputWidth; outputX++) {
                    for (int outputY = 0; outputY < Config.OutputHeight; outputY++) {
                        existenceBuffer[outputX, outputY] = false;
                    }
                }

                for (int pitchIndex = 0; pitchIndex < Config.PitchDivisionCount; pitchIndex++) {
                    for (int yawIndex = 0; yawIndex < Config.YawDivisionCount; yawIndex++) {
                        int panoramaPartId = pitchIndex * Config.YawDivisionCount + yawIndex;
                        Console.WriteLine(((double)panoramaPartId / (Config.YawDivisionCount * Config.PitchDivisionCount)).ToString("P1", CultureInfo.InvariantCulture) + " 完了");

                        OverlapState[,] overlapStateBuffer = new OverlapState[Config.OutputWidth, Config.OutputHeight];
                        // MMD合わせでyawは左回転にする
                        PanoramaPart panoramaPart = new PanoramaPart(yawIndex * -Config.UnitRadian, pitchIndex * Config.UnitRadian - (Math.PI / 2 - Config.UnitRadian / 2), Config.AngleOfView);
                        Parallel.ForEach(Enumerable.Range(0, Config.OutputWidth), outputX => {
                            for (int outputY = 0; outputY < Config.OutputHeight; outputY++) {
                                Vector2 uv = panoramaPart.GetUV(((outputX + 0.5) / Config.OutputWidth - 0.5) * 2 * Math.PI, ((outputY + 0.5) / Config.OutputHeight - 0.5) * Math.PI);
                                double u = uv.X;
                                double v = uv.Y;
                                bool hasValidColor = !Double.IsNaN(u) && 0 <= u && u <= 1 && 0 <= v && v <= 1;
                                if (existenceBuffer[outputX, outputY]) {
                                    overlapStateBuffer[outputX, outputY] = hasValidColor ? OverlapState.Overlapped : OverlapState.OnlyThis;
                                } else {
                                    overlapStateBuffer[outputX, outputY] = hasValidColor ? OverlapState.OnlyOther : OverlapState.NotAvailable;
                                }
                                if (hasValidColor) {
                                    existenceBuffer[outputX, outputY] = true;
                                }
                            }
                        });

                        var variableMap = new Dictionary<Coordinate, int>();
                        for (int outputX = 0; outputX < Config.OutputWidth; outputX++) {
                            for (int outputY = 0; outputY < Config.OutputHeight; outputY++) {
                                if (overlapStateBuffer[outputX, outputY] == OverlapState.Overlapped) {
                                    variableMap.Add(new Coordinate(outputX, outputY), variableMap.Count);
                                }
                            }
                        }

                        int[,] distanceBuffer = new int[variableMap.Count, 4];
                        int[,] heightBuffer = new int[variableMap.Count, 4];
                        for (int variableId = 0; variableId < variableMap.Count; variableId++) {
                            distanceBuffer[variableId, 0] = -1;
                            distanceBuffer[variableId, 1] = -1;
                            distanceBuffer[variableId, 2] = -1;
                            distanceBuffer[variableId, 3] = -1;
                            heightBuffer[variableId, 0] = -1;
                            heightBuffer[variableId, 1] = -1;
                            heightBuffer[variableId, 2] = -1;
                            heightBuffer[variableId, 3] = -1;
                        }
                        foreach (Coordinate origin in variableMap.Keys) {
                            int variableId = variableMap[origin];
                            for (int directionIndex = 0; directionIndex < getNeighbor.Length; directionIndex++) {
                                if (distanceBuffer[variableId, directionIndex] != -1) continue;
                                (Coordinate currentCoordinate, int currentDirectionIndex) = getNeighbor[directionIndex](origin);
                                while (variableMap.ContainsKey(currentCoordinate) && distanceBuffer[variableMap[currentCoordinate], currentDirectionIndex] == -1 && currentCoordinate != origin) {
                                    (currentCoordinate, currentDirectionIndex) = getNeighbor[currentDirectionIndex](currentCoordinate);
                                }
                                if (currentCoordinate == origin || (variableMap.ContainsKey(currentCoordinate) && distanceBuffer[variableMap[currentCoordinate], currentDirectionIndex] == -2)) {
                                    (currentCoordinate, currentDirectionIndex) = getNeighbor[getReverse[currentDirectionIndex]](currentCoordinate);
                                    while (currentCoordinate != origin) {
                                        distanceBuffer[variableMap[currentCoordinate], currentDirectionIndex] = -2;
                                        distanceBuffer[variableMap[currentCoordinate], getReverse[currentDirectionIndex]] = -2;
                                        heightBuffer[variableMap[currentCoordinate], currentDirectionIndex] = -2;
                                        heightBuffer[variableMap[currentCoordinate], getReverse[currentDirectionIndex]] = -2;
                                        (currentCoordinate, currentDirectionIndex) = getNeighbor[currentDirectionIndex](currentCoordinate);
                                    }
                                    distanceBuffer[variableMap[currentCoordinate], currentDirectionIndex] = -2;
                                    distanceBuffer[variableMap[currentCoordinate], getReverse[currentDirectionIndex]] = -2;
                                    heightBuffer[variableMap[currentCoordinate], currentDirectionIndex] = -2;
                                    heightBuffer[variableMap[currentCoordinate], getReverse[currentDirectionIndex]] = -2;
                                    continue;
                                }
                                int distance;
                                int height;
                                if (!variableMap.ContainsKey(currentCoordinate)) {
                                    distance = 0;
                                    if (overlapStateBuffer[currentCoordinate.X, currentCoordinate.Y] == OverlapState.OnlyThis) {
                                        height = 1;
                                    } else {
                                        height = 0;
                                    }
                                } else {
                                    distance = distanceBuffer[variableMap[currentCoordinate], currentDirectionIndex];
                                    height = heightBuffer[variableMap[currentCoordinate], currentDirectionIndex];
                                }
                                currentDirectionIndex = getReverse[currentDirectionIndex];
                                (currentCoordinate, currentDirectionIndex) = getNeighbor[currentDirectionIndex](currentCoordinate);
                                distance++;
                                while (currentCoordinate != origin) {
                                    distanceBuffer[variableMap[currentCoordinate], getReverse[currentDirectionIndex]] = distance;
                                    heightBuffer[variableMap[currentCoordinate], getReverse[currentDirectionIndex]] = height;
                                    (currentCoordinate, currentDirectionIndex) = getNeighbor[currentDirectionIndex](currentCoordinate);
                                    distance++;
                                }
                                distanceBuffer[variableMap[currentCoordinate], getReverse[currentDirectionIndex]] = distance;
                                heightBuffer[variableMap[currentCoordinate], getReverse[currentDirectionIndex]] = height;
                            }
                        }

                        double[] answerBuffer = new double[variableMap.Count];
                        Parallel.ForEach(variableMap.Values, variableId => {
                            Func<double, double, double> calcAnswer = null;
                            calcAnswer = (min, max) => {
                                double x = (min + max) / 2;
                                if (x == min || x == max) return x;
                                double value = Enumerable.Range(0, 4)
                                    .Where(directionIndex => distanceBuffer[variableId, directionIndex] >= 1)
                                    .Select(directionIndex => (x - heightBuffer[variableId, directionIndex]) / Math.Sqrt(Math.Pow(distanceBuffer[variableId, directionIndex], 2) + Math.Pow(x - heightBuffer[variableId, directionIndex], 2)))
                                    .Sum();
                                if (value < 0) return calcAnswer(x, max);
                                if (value > 0) return calcAnswer(min, x);
                                return x;
                            };
                            answerBuffer[variableId] = calcAnswer(0, 1);
                        });

                        using (var fs1 = new FileStream(Path.Combine(Config.PreComputedDir, $"overlap_state_{(panoramaPartId + 1).ToString().PadLeft(2, '0')}"), FileMode.Create))
                        using (var writer1 = new BinaryWriter(fs1))
                        using (var fs2 = new FileStream(Path.Combine(Config.PreComputedDir, $"rate_{(panoramaPartId + 1).ToString().PadLeft(2, '0')}"), FileMode.Create))
                        using (var writer2 = new BinaryWriter(fs2)) {
                            for (int outputX = 0; outputX < Config.OutputWidth; outputX++) {
                                for (int outputY = 0; outputY < Config.OutputHeight; outputY++) {
                                    writer1.Write((byte)overlapStateBuffer[outputX, outputY]);
                                    if (overlapStateBuffer[outputX, outputY] == OverlapState.Overlapped) {
                                        writer2.Write(answerBuffer[variableMap[new Coordinate(outputX, outputY)]]);
                                    }
                                }
                            }
                        }
                    }
                }
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
