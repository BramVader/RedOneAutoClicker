using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AndroidAdb
{
    public partial class Form1 : Form
    {
        bool initialized = false;
        private AdbClient client;
        private DeviceData device;
        private Image image;
        private Point? lastPoint, prevPoint;
        private float imageScale = 1.0f;
        private CancellationTokenSource cts = null;
        private List<BonusMask> bonusMasks;
        private DateTime? muteTime1 = null;
        private DateTime? muteTime2 = null;
        private DateTime? muteTime3 = null;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!initialized) Initialize();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (image != null)
            {
                float scX = (float)image.Width / this.ClientSize.Width;
                float scY = (float)image.Height / this.ClientSize.Height;
                imageScale = Math.Max(scX, scY);
                e.Graphics.DrawImage(image, 0f, 0f, (float)image.Width / imageScale, (float)image.Height / imageScale);
                //e.Graphics.DrawImageUnscaled(image, 0, -900);
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
            ReadBonusImages();
            ImageUpdateLoop();
            initialized = true;
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

                        if (muteTime2.HasValue && DateTime.UtcNow >= muteTime2.Value) muteTime2 = null;

                        if (curPoint != null && prevPoint != null && lastPoint == null && muteTime2 == null)
                        {
                            float dx = curPoint.Value.X - prevPoint.Value.X;
                            float dy = curPoint.Value.Y - prevPoint.Value.Y;
                            LoggingTextbox.AppendText($"{DateTime.Now}: Red one at ({curPoint.Value.X}, {curPoint.Value.Y}), ({dx}, {dy})\r\n");
                            client.ExecuteRemoteCommand($"input tap {it.Item2.Value.X + dx} {it.Item2.Value.Y + dy}", device, null);
                            muteTime2 = DateTime.UtcNow.AddSeconds(4);
                            //System.Media.SystemSounds.Beep.Play();
                        }
                        lastPoint = prevPoint;
                        prevPoint = it.Item2;
                        Invalidate();
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
