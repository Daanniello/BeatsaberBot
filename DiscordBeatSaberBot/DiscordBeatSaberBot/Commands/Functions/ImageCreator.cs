using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;
using System.Text;

namespace DiscordBeatSaberBot
{
    class ImageCreator
    {
        private Bitmap _bitmap;
        private string _fontFamily;
        public ImageCreator(string templatePath, string fontFamily = "Tourmaline")
        {
            _bitmap = (Bitmap)Image.FromFile(templatePath);
            _fontFamily = fontFamily;
        }

        public void Create(string path)
        {
            _bitmap.Save(path);
        }

        public SizeF AddText(string text, Color color, int fontsize, float x, float y)
        {
            PointF firstLocation = new PointF(x, y);

            using (Font font = new Font(_fontFamily, fontsize))
            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                graphics.DrawString(text, font, new SolidBrush(color), firstLocation);
                return graphics.MeasureString(text, font);
            }
        }

        public SizeF AddTextFloatRight(string text, Color color, int fontsize, float x, float y)
        {          
            using (Font font = new Font(_fontFamily, fontsize))
            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                PointF firstLocation = new PointF(_bitmap.Width - x - graphics.MeasureString(text, font).Width, y);
                graphics.DrawString(text, font, new SolidBrush(color), firstLocation);
                return graphics.MeasureString(text, font);
            }
        }

        public void AddImage(string path, float x, float y, int width, int height)
        {
            Image overlayImage = null;

            var request = WebRequest.Create(path);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                overlayImage = Bitmap.FromStream(stream);
            }

            Graphics g = Graphics.FromImage(_bitmap);
            g.DrawImage(overlayImage, x, y, width, height);
        }

        public void AddImageRounded(string path, float x, float y, int width, int height)
        {
            Image overlayImage = null;

            WebRequest request;
            try
            {
                request = WebRequest.Create(path);
            }
            catch
            {
                request = WebRequest.Create("https://www.thermaxglobal.com/wp-content/uploads/2020/05/image-not-found.jpg"); 
            }

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                overlayImage = Bitmap.FromStream(stream);
            }

            Graphics g = Graphics.FromImage(_bitmap);

            g.DrawImage(RoundCorners(overlayImage, 25, Color.FromArgb(0, 32, 32, 32)), x, y, width, height);

        }

        public void AddNoteSlashEffect(string path, float x, float y, int width, int height)
        {
            Image overlayImage = null;

            var request = WebRequest.Create(path);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                overlayImage = Bitmap.FromStream(stream);
            }

            Graphics g = Graphics.FromImage(_bitmap);

            var tuple = AddNoteSlashEffect(overlayImage, 25, Color.FromArgb(32, 32, 32));

            g.DrawImage(tuple.Item1, x / 2 + 100, y + 30, width / 2, height);
            g.DrawImage(tuple.Item2, x * 2 + 220, y - 30, width / 2, height);

        }

        private Image RoundCorners(Image StartImage, int CornerRadius, Color BackgroundColor)
        {
            CornerRadius *= 2;
            Bitmap RoundedImage = new Bitmap(StartImage.Width, StartImage.Height);
            using (Graphics g = Graphics.FromImage(RoundedImage))
            {
                g.Clear(BackgroundColor);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Brush brush = new TextureBrush(StartImage);
                GraphicsPath gp = new GraphicsPath();
                gp.AddArc(0, 0, CornerRadius, CornerRadius, 180, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0, CornerRadius, CornerRadius, 270, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
                gp.AddArc(0, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
                g.FillPath(brush, gp);
                return RoundedImage;
            }
        }

        private Tuple<Image, Image> AddNoteSlashEffect(Image StartImage, int CornerRadius, Color BackgroundColor)
        {
            CornerRadius *= 2;
            Bitmap RoundedImage = new Bitmap(StartImage.Width, StartImage.Height);
            using (Graphics g = Graphics.FromImage(RoundedImage))
            {
                g.Clear(BackgroundColor);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Brush brush = new TextureBrush(StartImage);
                GraphicsPath gp = new GraphicsPath();
                gp.AddArc(0, 0, CornerRadius, CornerRadius, 180, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0, CornerRadius, CornerRadius, 270, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
                gp.AddArc(0, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
                g.FillPath(brush, gp);

                Rectangle rect = new Rectangle(0, 0, RoundedImage.Width / 2, RoundedImage.Height);
                Bitmap leftSide = RoundedImage.Clone(rect, RoundedImage.PixelFormat);

                rect = new Rectangle(RoundedImage.Width / 2, 0, RoundedImage.Width / 2, RoundedImage.Height);
                Bitmap rightSide = RoundedImage.Clone(rect, RoundedImage.PixelFormat);

                return new Tuple<Image, Image>(leftSide, rightSide);
            }


        }
    }
}
