using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace AndroidAdb
{
    class ImageUpdater
    {
        private readonly AdbClient client;
        private readonly DeviceData device;
        private readonly Framebuffer buffer;
        private readonly IEnumerable<BonusMask> bonusMasks;
        private byte[] prevBuffer, currBuffer;

        private Rectangle AppMarkRect = new Rectangle(821, 1542, 192, 147);

        public ImageUpdater(AdbClient client, DeviceData device, IEnumerable<BonusMask> bonusMasks)
        {
            this.client = client;
            this.device = device;
            this.buffer = client.CreateRefreshableFramebuffer(device);
            this.bonusMasks = bonusMasks;
        }

        private bool CheckInApp(Framebuffer buffer)
        {
            var points = new[] {
                new { Point = new Point(855, 1562), Color = Color.FromArgb(126, 198, 241) },
                new { Point = new Point(890, 1582), Color = Color.FromArgb( 56, 141, 203) },
                new { Point = new Point(916, 1618), Color = Color.FromArgb( 35, 97, 142) }
            };

            int redOffset = (int)buffer.Header.Red.Offset / 8;
            int greenOffset = (int)buffer.Header.Green.Offset / 8;
            int blueOffset = (int)buffer.Header.Blue.Offset / 8;
            int pixelSize = (int)buffer.Header.Size / (int)buffer.Header.Width / (int)buffer.Header.Height;

            return points.All(it =>
            {
                var adr = (it.Point.Y * (int)buffer.Header.Width + it.Point.X) * pixelSize;
                return Math.Abs(buffer.Data[adr + redOffset] - it.Color.R) +
                    Math.Abs(buffer.Data[adr + greenOffset] - it.Color.G) +
                    Math.Abs(buffer.Data[adr + blueOffset] - it.Color.B) < 20;
            });
        }

        private BonusMask FindBonus(Framebuffer buffer)
        {
            int redOffset = (int)buffer.Header.Red.Offset / 8;
            int greenOffset = (int)buffer.Header.Green.Offset / 8;
            int blueOffset = (int)buffer.Header.Blue.Offset / 8;
            int pixelSize = (int)buffer.Header.Size / (int)buffer.Header.Width / (int)buffer.Header.Height;

            foreach (var mask in bonusMasks)
            {
                bool fnd = true;
                for (int n = 0; n < BonusMask.PixelCount; n++)
                {
                    Point p = mask.GetCoord(n);
                    var adr = (p.Y * (int)buffer.Header.Width + p.X) * pixelSize;
                    if (Math.Abs(buffer.Data[adr + redOffset] - mask.Pixels[n].R) > 20 ||
                        Math.Abs(buffer.Data[adr + greenOffset] - mask.Pixels[n].G) > 20 ||
                        Math.Abs(buffer.Data[adr + blueOffset] - mask.Pixels[n].B) > 20
                    )
                    {
                        fnd = false;
                        break;
                    }
                }
                if (fnd) return mask;
            }
            return null;
        }

        private Point? FindRedOne(Framebuffer buffer)
        {
            var rects = new[]
            {
                new Rectangle(0, 316, 661, 223),
                new Rectangle(0, 539, 869, 775),
                new Rectangle(346, 1003, 734, 459)
            };

            int step = 8;
            int halfStep = step / 4;
            int redOffset = (int)buffer.Header.Red.Offset / 8;
            int greenOffset = (int)buffer.Header.Green.Offset / 8;
            int blueOffset = (int)buffer.Header.Blue.Offset / 8;

            int pixelSize = (int)buffer.Header.Size / (int)buffer.Header.Width / (int)buffer.Header.Height;

            int y1 = rects.Min(r => r.Top);
            int y2 = rects.Max(r => r.Bottom);
            for (int y = y1; y < y2; y += step)
            {
                int adr = (y * (int)buffer.Header.Width + halfStep) * pixelSize;
                for (int x = halfStep; x < buffer.Header.Width; x += step)
                {
                    if (buffer.Data[adr + redOffset] > 128 &&
                        buffer.Data[adr + greenOffset] < 10 &&
                        buffer.Data[adr + blueOffset] < 10 &&
                        rects.Any(r => x >= r.Left && x < r.Right && y >= r.Top && y < r.Bottom))
                    {
                        return new Point(x, y);
                    }
                    adr += step * pixelSize;
                }
            }
            return null;
        }

        public async void Run(IProgress<Tuple<Framebuffer, Point?, BonusMask>> progress, CancellationToken ct)
        {
            while (true)
            {
                await buffer.RefreshAsync(ct);
                if (prevBuffer == null)
                {
                    prevBuffer = new byte[buffer.Data.Length];
                    currBuffer = new byte[buffer.Data.Length];
                }
                Array.Copy(buffer.Data, currBuffer, buffer.Data.Length);

                Point? point = null;
                BonusMask bonus = null;
                if (CheckInApp(buffer))
                {
                    point = FindRedOne(buffer);
                }
                bonus = FindBonus(buffer);

                progress.Report(Tuple.Create(buffer, point, bonus));

                Array.Copy(currBuffer, prevBuffer, currBuffer.Length);
                ct.ThrowIfCancellationRequested();
            }
        }
    }
}
