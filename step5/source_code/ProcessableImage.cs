using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace panorama {
    public class ProcessableImage : IDisposable {
        private readonly Bitmap bitmap;
        private readonly BitmapData bitmapData;
        private readonly byte[] pixels;
        private readonly int channelCount;

        public ProcessableImage(int width, int height) {
            this.bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            try {
                this.bitmapData = this.bitmap.LockBits(new Rectangle(0, 0, this.bitmap.Width, this.bitmap.Height), ImageLockMode.WriteOnly, this.bitmap.PixelFormat);
                this.pixels = new byte[this.bitmapData.Stride * this.bitmapData.Height];
                this.channelCount = 3;
            } catch {
                this.bitmap.Dispose();
                throw;
            }
        }
        public ProcessableImage(string filePath) {
            this.bitmap = (Bitmap)Image.FromFile(filePath);
            try {
                this.bitmapData = this.bitmap.LockBits(new Rectangle(0, 0, this.bitmap.Width, this.bitmap.Height), ImageLockMode.WriteOnly, this.bitmap.PixelFormat);
                this.pixels = new byte[this.bitmapData.Stride * this.bitmapData.Height];
                Marshal.Copy(this.bitmapData.Scan0, this.pixels, 0, this.pixels.Length);
                switch (this.bitmap.PixelFormat) {
                    case PixelFormat.Format24bppRgb:
                        this.channelCount = 3;
                        break;
                    case PixelFormat.Format32bppArgb:
                        this.channelCount = 4;
                        break;
                    default:
                        throw new NotSupportedException($"Unknown format: {this.bitmap.PixelFormat.ToString()}");
                }
            } catch {
                this.bitmap.Dispose();
                throw;
            }
        }

        public int Width {
            get { return this.bitmapData.Width; }
        }

        public int Height {
            get { return this.bitmapData.Height; }
        }
        private int GetPosition(int x, int y) {
            return x * this.channelCount + this.bitmapData.Stride * y;
        }

        public void Save(string filePath) {
            Marshal.Copy(this.pixels, 0, this.bitmapData.Scan0, this.pixels.Length);
            this.bitmap.Save(filePath, ImageFormat.Png);
        }

        public Color GetColorByXy(int x, int y) {
            int position = this.GetPosition(x, y);
            return Color.FromArgb(this.pixels[position + 2], this.pixels[position + 1], this.pixels[position]);
        }

        public Color GetColorByUv(double u, double v) {
            double x = (u % 1) * this.Width - 0.5;
            double y = (v % 1) * this.Height - 0.5;
            int baseX = (int)Math.Floor(x);
            int baseY = (int)Math.Floor(y);
            double fractionX = x % 1;
            double fractionY = y % 1;
            Func<int, int, Color> getPixelSafely = (originalX, originalY) => {
                int fixedX = originalX < 0 ? 0 : originalX >= this.Width ? this.Width - 1 : originalX;
                int fixedY = originalY < 0 ? 0 : originalY >= this.Height ? this.Height - 1 : originalY;
                return this.GetColorByXy(fixedX, fixedY);
            };
            double blendRate1 = (1 - fractionX) * (1 - fractionY);
            Color pixel1 = getPixelSafely(baseX, baseY);
            double blendRate2 = fractionX * (1 - fractionY);
            Color pixel2 = getPixelSafely(baseX + 1, baseY);
            double blendRate3 = (1 - fractionX) * fractionY;
            Color pixel3 = getPixelSafely(baseX, baseY + 1);
            double blendRate4 = fractionX * fractionY;
            Color pixel4 = getPixelSafely(baseX + 1, baseY + 1);
            return Color.FromArgb(
                (byte)Math.Round(blendRate1 * pixel1.R + blendRate2 * pixel2.R + blendRate3 * pixel3.R + blendRate4 * pixel4.R),
                (byte)Math.Round(blendRate1 * pixel1.G + blendRate2 * pixel2.G + blendRate3 * pixel3.G + blendRate4 * pixel4.G),
                (byte)Math.Round(blendRate1 * pixel1.B + blendRate2 * pixel2.B + blendRate3 * pixel3.B + blendRate4 * pixel4.B)
            );
        }

        public void SetColorByXy(int x, int y, Color color) {
            int position = this.GetPosition(x, y);
            this.pixels[position] = color.B;
            this.pixels[position + 1] = color.G;
            this.pixels[position + 2] = color.R;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing) {
            if (!this.disposedValue) {
                if (disposing) {
                    this.bitmap.UnlockBits(this.bitmapData);
                    this.bitmap.Dispose();
                }
                this.disposedValue = true;
            }
        }

        public void Dispose() {
            this.Dispose(true);
        }
        #endregion

    }
}
