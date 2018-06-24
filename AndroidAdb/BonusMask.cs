using System.Drawing;

namespace AndroidAdb
{
    class BonusMask
    {
        public static int PixelCount = 300;

        public BonusType Type { get; set; }
        public Color[] Pixels { get; set; }
        public Rectangle Rectangle { get; }
        public Point ClickNormalPos { get; set; }
        public Point? ClickExtraPos { get; set; }

        public Point GetCoord(int n)
        {
            int offset = n * (Rectangle.Width * Rectangle.Height * 3 / 4 - 31) % (Rectangle.Width * Rectangle.Height);
            return new Point(
                offset % Rectangle.Width + Rectangle.Left, 
                offset / Rectangle.Width + Rectangle.Top);
        }

        public BonusMask(Rectangle rect)
        {
            Pixels = new Color[PixelCount];
            Rectangle = rect;
        }
    }
}
