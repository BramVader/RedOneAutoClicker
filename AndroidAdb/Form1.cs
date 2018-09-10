using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AndroidAdb
{
    public partial class Form1 : Form
    {
        private Random rnd;
        bool initialized = false;
        private AdbClient client;
        private DeviceData device;
        private System.Windows.Forms.Timer timer1;
        private Image image;
        private Point? lastPoint, prevPoint, newPoint;
        private float imageScale = 1.0f;
        private CancellationTokenSource cts = null;
        private List<BonusMask> bonusMasks;
        private DateTime? muteTime1 = null;
        private DateTime? muteTime2 = null;
        private DateTime? muteTime3 = null;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.Paint += OnPaint;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!initialized) Initialize();
        }

        protected void OnPaint(Object sender, PaintEventArgs e)
        {
            void DrawCross(Pen pen, Point p)
            {
                e.Graphics.DrawLine(pen, p.X - 40, p.Y - 40, p.X + 40, p.Y + 40);
                e.Graphics.DrawLine(pen, p.X - 40, p.Y + 40, p.X + 40, p.Y - 40);
            }

            base.OnPaint(e);
            if (image != null)
            {
                float scX = (float)image.Width / this.ClientSize.Width;
                float scY = (float)image.Height / this.ClientSize.Height;
                imageScale = Math.Max(scX, scY);
                e.Graphics.ScaleTransform(1f/imageScale, 1f/imageScale);
                e.Graphics.DrawImageUnscaled(image, 0, 0);
                if (newPoint.HasValue)
                {
                    DrawCross(Pens.Red, newPoint.Value);
                    if (lastPoint.HasValue) DrawCross(Pens.Yellow, newPoint.Value);
                    if (prevPoint.HasValue) DrawCross(Pens.Green, prevPoint.Value);
                }
            }
        }

        protected void ReadBonusImages()
        {
            var rectNormal = new Rectangle(188, 580, 180, 180);
            var rectExtra = new Rectangle(188, 470, 180, 180);
            var files = new[]
            {
                new { Type = BonusType.Bpc, FileName = "BPC Bonus.png", IsExtra = false },
                new { Type = BonusType.Bps, FileName = "BPS Bonus.png", IsExtra = false },
                new { Type = BonusType.FasterResearch, FileName = "Faster Research.png", IsExtra = false },
                new { Type = BonusType.FreeBacteria, FileName = "Free Bacteria.png", IsExtra = false },
                new { Type = BonusType.FreeDiamond, FileName = "Free Diamond.png", IsExtra = false },
                new { Type = BonusType.BpcExtra, FileName = "BPC Bonus Extra.png", IsExtra = true },
                new { Type = BonusType.BpsExtra, FileName = "BPS Bonus Extra.png", IsExtra = true },
                new { Type = BonusType.FasterResearchExtra, FileName = "Faster Research Extra.png", IsExtra = true },
                new { Type = BonusType.FreeBacteriaExtra, FileName = "Free Bacteria Extra.png", IsExtra = true },
                new { Type = BonusType.FreeDiamondExtra, FileName = "Free Diamond Extra.png", IsExtra = true }
            };
            bonusMasks = new List<BonusMask>();
            foreach (var file in files)
            {
                var image = Image.FromFile("Images//" + file.FileName) as Bitmap;
                var mask = new BonusMask(file.IsExtra ? rectExtra : rectNormal)
                {
                    ClickNormalPos = file.IsExtra ? new Point(550, 1400) : new Point(550, 1270),
                    ClickExtraPos = file.IsExtra ? new Point(550, 1230) : (Point?)null,
                    Type = file.Type
                };
                for (int n = 0; n < BonusMask.PixelCount; n++)
                {
                    var p = mask.GetCoord(n);
                    mask.Pixels[n] = image.GetPixel(p.X, p.Y);
                }
                bonusMasks.Add(mask);
            }
        }

        private void Initialize()
        {
            AdbServer server = new AdbServer();
            var result = server.StartServer(@"D:\Android\Sdk\platform-tools\adb.exe", restartServerIfNewer: false);
            client = (AdbClient)AdbClient.Instance;
            device = client.GetDevices().Single();

            //timer1 = new System.Windows.Forms.Timer { Interval = 50, Enabled = true };
            //timer1.Tick += Timer1_Tick;
            //sw.Start();

            ReadBonusImages();
            ImageUpdateLoop();
            initialized = true;
        }

        int v = 0;
        int[] pv = new int[] { 0, 4, 2, 6, -1, 1, 5, 3, 7, -2 };

        Stopwatch sw = new Stopwatch();

        Point fireLaser = new Point(675, 1660);
        Point p1 = new Point(240, 200);
        Point p2 = new Point(860, 200);
        Point p3 = new Point(240, 1320);
        Point p4 = new Point(860, 1320);

        private void Timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            //Point p;
            //if (pv[v] == -1) p = new Point(529, 1069);           // "Next Level"
            //else if (pv[v] == -2) p = new Point(874, 1120);      // "No" (Watch one video)
            //else p = new Point(194 + (910 - 194) * pv[v] / 8, 265);
            //client.ExecuteRemoteCommand($"input tap {p.X} {p.Y}", device, null);
            //v = (v + 1) % pv.Length;

            if (sw.ElapsedMilliseconds > (1 * 30) * 1000)
            {
                client.ExecuteRemoteCommand($"input tap 890 1120", device, null);  // "No" (Watch one video)
                client.ExecuteRemoteCommand($"input tap 220 1450", device, null);  // Retry level
                sw.Restart();
            }
            else
            {
                client.ExecuteRemoteCommand($"input tap {fireLaser.X} {fireLaser.Y}", device, null);
                if (v == 0)
                    client.ExecuteRemoteCommand($"input swipe {p1.X} {p1.Y} {p4.X} {p4.Y} 500", device, null);
                else
                    client.ExecuteRemoteCommand($"input swipe {p2.X} {p2.Y} {p3.X} {p3.Y} 500", device, null);
                v = (v + 1) % 2;
            }

            timer1.Enabled = true;
        }

        private async void ImageUpdateLoop()
        {
            if (cts != null)
            {
                // Stop a pending loop
                cts.Cancel();
                cts = null;
            }
            else
            {
                // Start a new rendering
                var updater = new ImageUpdater(client, device, bonusMasks);
                var progress = new Progress<Tuple<Framebuffer, Point?, BonusMask>>(it =>
                {
                    image = it.Item1.ToImage();

                    if (muteTime1.HasValue)
                    {
                        Text = ((int)(muteTime1.Value - DateTime.UtcNow).TotalSeconds).ToString();
                        if (DateTime.UtcNow >= muteTime1.Value)
                        {
                            muteTime1 = null;
                            client.ExecuteRemoteCommand($"input keyevent 4", device, null);
                            LoggingTextbox.AppendText($"{DateTime.Now}: Muting done. Pressed Back\r\n");
                        }
                    }
                    else
                    {
                        var mask = it.Item3;
                        var curPoint = it.Item2;

                        if (muteTime3.HasValue && DateTime.UtcNow >= muteTime3.Value) muteTime3 = null;

                        if (mask != null && muteTime3 == null)
                        {
                            LoggingTextbox.AppendText($"{DateTime.Now}: Bonus {mask.Type}\r\n");
                            if (mask.Type == BonusType.FasterResearchExtra)
                            {
                                client.ExecuteRemoteCommand($"input tap {mask.ClickExtraPos.Value.X} {mask.ClickExtraPos.Value.Y}", device, null);
                                LoggingTextbox.AppendText($"{DateTime.Now}: Click Extra - Mute time initiated\r\n");
                                muteTime1 = DateTime.UtcNow.AddSeconds(40.0);
                            }
                            else
                            {
                                client.ExecuteRemoteCommand($"input tap {mask.ClickNormalPos.X} {mask.ClickNormalPos.Y}", device, null);
                                LoggingTextbox.AppendText($"{DateTime.Now}: Click Normal\r\n");
                            }
                            muteTime3 = DateTime.UtcNow.AddSeconds(4);
                        }

                        if (muteTime2.HasValue && DateTime.UtcNow >= muteTime2.Value)
                        {
                            muteTime2 = null;
                            newPoint = null;
                        }

                        if (curPoint != null && prevPoint != null && muteTime2 == null)
                        {
                            int dx = curPoint.Value.X - prevPoint.Value.X;
                            int dy = curPoint.Value.Y - prevPoint.Value.Y;
                            Point testPoint = new Point(curPoint.Value.X + dx, curPoint.Value.Y + dy);
                            if (ImageUpdater.Clickable(testPoint))
                            {
                                newPoint = testPoint;
                                LoggingTextbox.AppendText($"{DateTime.Now}: Red one at ({curPoint.Value.X}, {curPoint.Value.Y}), ({dx}, {dy})\r\n");
                                client.ExecuteRemoteCommand($"input tap {testPoint.X} {testPoint.Y}", device, null);
                                muteTime2 = DateTime.UtcNow.AddSeconds(0.5);
                                System.Media.SystemSounds.Beep.Play();
                            }
                        }
                        lastPoint = prevPoint;
                        prevPoint = curPoint;
                        if (!collapsed) pictureBox1.Invalidate();
                    }
                });
                cts = new CancellationTokenSource();
                try
                {
                    await Task.Run(() => updater.Run(progress, cts.Token));
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string fileBase = "Screendump";
            var files = Directory.GetFiles(".", fileBase + "*.png").Select(it => Path.GetFileName(it)).ToList();
            for (int n = 0; true; n++)
            {
                string newFileName = fileBase + n.ToString("0000") + ".png";
                if (!files.Contains(newFileName))
                {
                    image.Save(newFileName);
                    return;
                }
            }
        }

        private int expandedWidth = 0;
        private bool collapsed = true;

        private void ShowHideButton_Click(object sender, EventArgs e)
        {
            int collapsedWidth = ShowHideButton.Right + 16;
            if (expandedWidth == 0) expandedWidth = collapsedWidth * 2;

            collapsed = Width == collapsedWidth;
            Width = collapsed ? expandedWidth : collapsedWidth;
            collapsed = !collapsed;
            ShowHideButton.Text = collapsed ? "Show" : "Hide";
            pictureBox1.Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            client.ExecuteRemoteCommand($"input tap {(int)(e.X * imageScale)} {(int)(e.Y * imageScale)}", device, null);
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            client.ExecuteRemoteCommand($"input keyevent 4", device, null);
        }
    }
}
