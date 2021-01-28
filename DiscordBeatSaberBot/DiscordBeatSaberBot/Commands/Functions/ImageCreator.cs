using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBeatSaberBot
{
    class ImageCreator
    {
        private Bitmap _bitmap;
        public ImageCreator(string templatePath)
        {
            string imageFilePath = templatePath;
            _bitmap = (Bitmap)Image.FromFile(imageFilePath);
        }

        public Task Create(string path)
        {
            _bitmap.Save(path);
            return Task.CompletedTask;
        }

        public SizeF AddText(string text, Color color, int fontsize, float x, float y)
        {
            PointF firstLocation = new PointF(x, y);

            using (Font arialFont = new Font("Tourmaline", fontsize))
            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                graphics.DrawString(text, arialFont, new SolidBrush(color), firstLocation);
                return graphics.MeasureString(text, arialFont);
            }
        }

        public SizeF AddTextCenter(string text, Color color, int fontsize, float x, float y)
        {
            

            using (Font arialFont = new Font("Tourmaline", fontsize))
            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                var length = graphics.MeasureString(text, arialFont);
                PointF firstLocation = new PointF(x - (length.Width / 2 ), y);

                graphics.DrawString(text, arialFont, new SolidBrush(color), firstLocation);
                return length;
            }
        }

        public SizeF AddTextFloatRight(string text, Color color, int fontsize, float x, float y)
        {          
            using (Font arialFont = new Font("Tourmaline", fontsize))
            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                PointF firstLocation = new PointF(_bitmap.Width - x - graphics.MeasureString(text, arialFont).Width, y);
                graphics.DrawString(text, arialFont, new SolidBrush(color), firstLocation);
                return graphics.MeasureString(text, arialFont);
            }
        }

        public void AddImage(string path, float x, float y, int width, int height, float opacity = 1)
        {
            Image overlayImage = null;

            var request = WebRequest.Create(path);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                overlayImage = Bitmap.FromStream(stream);
            }

            Graphics g = Graphics.FromImage(_bitmap);
            
            g.DrawImage(SetImageOpacity(overlayImage, opacity), x, y, width, height);
        }

        private Image SetImageOpacity(Image image, float opacity)
        {
            try
            {
                //create a Bitmap the size of the image provided  
                Bitmap bmp = new Bitmap(image.Width, image.Height);

                //create a graphics object from the image  
                using (Graphics gfx = Graphics.FromImage(bmp))
                {

                    //create a color matrix object  
                    ColorMatrix matrix = new ColorMatrix();

                    //set the opacity  
                    matrix.Matrix33 = opacity;

                    //create image attributes  
                    ImageAttributes attributes = new ImageAttributes();

                    //set the color(opacity) of the image  
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    //now draw the image  
                    gfx.DrawImage(image, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attributes);
                }
                return bmp;
            }
            catch (Exception ex)
            {                
                return null;
            }
        }

        public void DrawLineBetweenPoints(Color color, int width, float x1, float y1, float x2, float y2)
        {
            // Create pen.
            Pen Pen = new Pen(color, width);

            // Create points that define line.
            PointF point1 = new PointF(x1, y1);
            PointF point2 = new PointF(x2, y2);

            // Draw line to screen.
            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                graphics.DrawLine(Pen, point1, point2);
            }                
        }

        public void AddImageRounded(string path, float x, float y, int width, int height)
        {
            Image overlayImage = null;

            WebRequest request;
            try
            {
                request = WebRequest.Create(path);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    overlayImage = Bitmap.FromStream(stream);
                }
            }
            catch
            {
                request = WebRequest.Create("https://www.thermaxglobal.com/wp-content/uploads/2020/05/image-not-found.jpg");
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    overlayImage = Bitmap.FromStream(stream);
                }
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
            Image finishedImage = null;
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
