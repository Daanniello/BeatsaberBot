using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
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

        public SizeF AddTextWithBackGround(string text, Color color, int fontsize, Color backgroundColor, float x, float y)
        {
            PointF firstLocation = new PointF(x, y);

            using (Font arialFont = new Font("Tourmaline", fontsize))
            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                var textSize = graphics.MeasureString(text, arialFont);
                DrawRectangle((int)x, (int)y, (int)textSize.Width, (int)textSize.Height, backgroundColor);
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
                PointF firstLocation = new PointF(x - (length.Width / 2), y);

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

        public void AddAccGraph(int x, int y, int width, int height, Dictionary<float, float> dataPoints, float dataPointxMax, float dataPointyMax, Color color, float zoomIn = 1)
        {
            if (zoomIn != 1)
            {
                dataPointxMax = dataPointxMax / zoomIn;
                dataPointyMax = dataPointyMax / zoomIn;
                dataPoints = dataPoints.ToDictionary(x => x.Key / zoomIn, x => x.Value / zoomIn);
            }

            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                //Drawing the background of the graph
                var precisionLineJump = height / 5;
                var precisionPointXJump = (dataPointyMax * 100) / 5; // times 100 for the acc graph...
                for (var t = 0; t <= 5; t++)
                {
                    AddTextFloatRight(Math.Round(100 - ((dataPointyMax * 100) /* times 100 for the acc graph... */ - (precisionPointXJump * t)), 2).ToString() + "%", Color.White, 12, x - 400, (y + height - precisionLineJump * t) - 11);
                    //graphics.DrawString(Math.Round(100 - ((dataPointyMax * 100) /* times 100 for the acc graph... */ - (precisionPointXJump * t)), 2).ToString() + "%", new Font("Tourmaline", 12), new SolidBrush(color), new PointF(x - 60, (y + height - precisionLineJump * t) - 15));
                    graphics.DrawLine(new Pen(Color.Gray, 3), x, y + height - precisionLineJump * t, x + width, y + height - precisionLineJump * t);
                }

                //Drawing the graph points
                for (var i = 0; i < dataPoints.Count; i++)
                {
                    if (i == dataPoints.Count - 1) break;
                    var x1 = x + (dataPoints.ElementAt(i).Key * width / dataPointxMax);
                    var y1 = y + (height * zoomIn) - (dataPoints.ElementAt(i).Value * (height * zoomIn) / dataPointyMax);
                    var x2 = x + (dataPoints.ElementAt(i + 1).Key * width / dataPointxMax);
                    var y2 = y + (height * zoomIn) - (dataPoints.ElementAt(i + 1).Value * (height * zoomIn) / dataPointyMax);

                    var pointOne = new PointF(x1, y1);
                    var pointTwo = new PointF(x2, y2);

                    graphics.DrawLine(new Pen(color, 2), pointOne, pointTwo);

                }
            }

        }

        public void AddImage(string path, float x, float y, int width, int height, float opacity = 1)
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

        public void DrawRectangle(int x, int y, int with, int height, Color? fillColor = null, Color? outerColor = null)
        {
            // Create pen.
            SolidBrush brush = new SolidBrush(Color.White);
            if (fillColor != null)
            {
                brush = new SolidBrush((Color)fillColor);
            }

            Pen pen = new Pen(Color.Gray, 3);
            if (outerColor != null)
            {
                pen = new Pen((Color)outerColor, 3);
            }

            // Create rectangle.
            Rectangle rect = new Rectangle(x, y, with, height);

            // Draw rectangle to screen.
            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                graphics.FillRectangle(brush, rect);
                graphics.DrawRectangle(pen, rect);
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
