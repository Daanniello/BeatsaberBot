using AnimatedGif;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Color = System.Drawing.Color;
using ColorMatrix = System.Drawing.Imaging.ColorMatrix;
using Image = System.Drawing.Image;
using PointF = System.Drawing.PointF;
using Rectangle = System.Drawing.Rectangle;
using SizeF = System.Drawing.SizeF;
using SixLabors.ImageSharp.Formats.Gif;
using GifskiNet;
using Discord;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace DiscordBeatSaberBot
{
    public class ImageCreator
    {
        private Bitmap _bitmap;
        bool isGif = false;
        public ImageCreator(string templatePath, bool isGif = false)
        {
            this.isGif = isGif;
            string imageFilePath = templatePath;
            _bitmap = (Bitmap)Image.FromFile(imageFilePath);
        }

        public Task Create(string path)
        {
            _bitmap.Save(path);
            return Task.CompletedTask;
        }

        public Task CreateAsGifWithShine(string path, Color averageColor)
        {
            //Create a gif with the shine elements on it.
            // 3000ms, each 100ms shine 
            //Create all frames 
            //_bitmap = ApplyOverlayEffect(_bitmap);
            var frame1 = AddImageToFrame((Bitmap)_bitmap.Clone(), (Bitmap)Image.FromFile("../../../Resources/img/frame1.png"));
            var frame2 = AddImageToFrame((Bitmap)_bitmap.Clone(), (Bitmap)Image.FromFile("../../../Resources/img/frame2.png"));
            var frame3 = AddImageToFrame((Bitmap)_bitmap.Clone(), (Bitmap)Image.FromFile("../../../Resources/img/frame3.png"));
            var frame4 = AddImageToFrame((Bitmap)_bitmap.Clone(), (Bitmap)Image.FromFile("../../../Resources/img/frame4.png"));
            var frame5 = AddImageToFrame((Bitmap)_bitmap.Clone(), (Bitmap)Image.FromFile("../../../Resources/img/frame5.png"));
            var frame6 = AddImageToFrame((Bitmap)_bitmap.Clone(), (Bitmap)Image.FromFile("../../../Resources/img/frame6.png"));

            //using (var gif = new AnimatedGifCreator(path, delay: 1000, repeat: 0))
            //{
            //    gif.AddFrame(frame1, 2000);
            //    gif.AddFrame(frame2, 100);
            //    gif.AddFrame(frame3, 100, GifQuality.Bit8);
            //    gif.AddFrame(frame4, 100, GifQuality.Bit8);
            //    gif.AddFrame(frame5, 100, GifQuality.Bit8);
            //    gif.AddFrame(frame6, 100, GifQuality.Bit8);
            //}

            //var list = new List<Image>();
            //list.Add(frame1);
            //list.Add(frame2);
            //list.Add(frame3);
            //list.Add(frame4);
            //list.Add(frame5);
            //list.Add(frame6);
            var guid = Guid.NewGuid();

            frame1.Save($"../../../Resources/TempFiles/frame1-{guid}.png");
            frame2.Save($"../../../Resources/TempFiles/frame2-{guid}.png");
            frame3.Save($"../../../Resources/TempFiles/frame3-{guid}.png");
            frame4.Save($"../../../Resources/TempFiles/frame4-{guid}.png");
            frame5.Save($"../../../Resources/TempFiles/frame5-{guid}.png");
            frame6.Save($"../../../Resources/TempFiles/frame6-{guid}.png");


            using var gifski = Gifski.Create("../../../Resources/DLL/gifski.dll", settings =>
            {
                settings.Quality = 100;
                settings.LossyQuality = 100;
                settings.MotionQuality = 100;
                settings.Width = 735;
                settings.Height = 1211;
                settings.Extra = true;
                settings.Fast = true;
            });

            // Sets the output file of the gif
            gifski.SetFileOutput(path);
            gifski.AddFramePngFile(frameNumber: 0, presentationTimestamp: 2.08, filePath: $"../../../Resources/TempFiles/frame1-{guid}.png");
            gifski.AddFramePngFile(frameNumber: 8, presentationTimestamp: 2.16, filePath: $"../../../Resources/TempFiles/frame2-{guid}.png");
            gifski.AddFramePngFile(frameNumber: 16, presentationTimestamp: 2.24, filePath: $"../../../Resources/TempFiles/frame3-{guid}.png");
            gifski.AddFramePngFile(frameNumber: 24, presentationTimestamp: 2.32, filePath: $"../../../Resources/TempFiles/frame4-{guid}.png");
            gifski.AddFramePngFile(frameNumber: 32, presentationTimestamp: 2.40, filePath: $"../../../Resources/TempFiles/frame5-{guid}.png");
            gifski.AddFramePngFile(frameNumber: 40, presentationTimestamp: 2.48, filePath: $"../../../Resources/TempFiles/frame6-{guid}.png");
            gifski.AddFramePngFile(frameNumber: 48, presentationTimestamp: 2.56, filePath: $"../../../Resources/TempFiles/frame1-{guid}.png");
            gifski.AddFramePngFile(frameNumber: 56, presentationTimestamp: 2.64, filePath: $"../../../Resources/TempFiles/frame1-{guid}.png");
            gifski.Finish();

            File.Delete($"../../../Resources/TempFiles/frame1-{guid}.png");
            File.Delete($"../../../Resources/TempFiles/frame2-{guid}.png");
            File.Delete($"../../../Resources/TempFiles/frame3-{guid}.png");
            File.Delete($"../../../Resources/TempFiles/frame4-{guid}.png");
            File.Delete($"../../../Resources/TempFiles/frame5-{guid}.png");
            File.Delete($"../../../Resources/TempFiles/frame6-{guid}.png");

            return Task.CompletedTask;
        }

        public void ApplyShinyEffect()
        {
            var bottomImage = _bitmap;
            var topImage = (Bitmap)Image.FromFile("../../../Resources/img/shine1.png");
            Bitmap resultImage = new Bitmap(bottomImage.Width, bottomImage.Height);
            for (int x = 0; x < bottomImage.Width; x++)
            {
                for (int y = 0; y < bottomImage.Height; y++)
                {
                    Color bottomColor = bottomImage.GetPixel(x, y);
                    Color topColor = topImage.GetPixel(x, y);

                    int r = bottomColor.R + topColor.R - 255;
                    int g = bottomColor.G + topColor.G - 255;
                    int b = bottomColor.B + topColor.B - 255;

                    if (r > 255) r = 255;
                    if (g > 255) g = 255;
                    if (b > 255) b = 255;

                    if (r < 0) r = 0;
                    if (g < 0) g = 0;
                    if (b < 0) b = 0;

                    int a = bottomColor.A;

                    var color = Color.FromArgb(a, r, g, b);

                    resultImage.SetPixel(x, y, color);
                }
            }
            _bitmap = resultImage;
        }

        public Image AddImageToFrame(Bitmap frame, Bitmap image, int x = 0, int y = 0)
        {
            using (Graphics g = Graphics.FromImage(frame))
            {
                g.DrawImage(image, x, y, image.Width, image.Height);
            }

            return frame;
        }

        public Bitmap GetBitmap()
        {
            return _bitmap;
        }

        public string CreateZoomEffect(string savePath, string path)
        {
            using (var gif = new AnimatedGifCreator($"{savePath}", delay: 1000, repeat: 1))
            {
                var fadeImage = DownloadImage(path, 200, 200);
                gif.AddFrame(ResizeImage(fadeImage, 200, 200));
                gif.AddFrame(ResizeImage(fadeImage, 300, 300));
            }
            return savePath;
        }

        public SizeF AddText(string text, Color color, int fontsize, float x, float y, string fontstyle = "Tourmaline")
        {
            PointF firstLocation = new PointF(x, y);

            using (Font arialFont = new Font(fontstyle, fontsize))
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

        public SizeF AddTextCenter(string text, Color color, int fontsize, float x, float y, string fontstyle = "Tourmaline", Color? textStrokeColor = null, int? maxWidth = null, bool useAntiAlias = false)
        {
            if (maxWidth != null)
            {
                var firstTextHeight = 0f;
                for (var i = fontsize; i > 10; i--)
                {
                    using (Font arialFont = new Font(fontstyle, i))
                    using (Graphics graphics = Graphics.FromImage(_bitmap))
                    {
                        var length = graphics.MeasureString(text, arialFont);
                        if (i == fontsize) firstTextHeight = length.Height;
                        if (length.Width > maxWidth) continue;
                        else
                        {
                            fontsize = i;
                            y += (firstTextHeight - length.Height) / 2;
                            break;
                        }
                    }
                }
            }

            using (Font arialFont = new Font(fontstyle, fontsize))
            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                if (useAntiAlias) graphics.SmoothingMode = SmoothingMode.AntiAlias;

                var length = graphics.MeasureString(text, arialFont);
                PointF firstLocation = new PointF(x - (length.Width / 2), y);

                if (textStrokeColor != null) graphics.DrawString(text, arialFont, new SolidBrush((Color)textStrokeColor), new PointF { X = firstLocation.X - 1, Y = firstLocation.Y - 1 });
                graphics.DrawString(text, arialFont, new SolidBrush(color), firstLocation);
                return length;
            }
        }

        public SizeF GetTextSize(string text, string fontstyle, int fontsize)
        {
            using (Font arialFont = new Font(fontstyle, fontsize))
            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                var length = graphics.MeasureString(text, arialFont);
                return length;
            }
        }

        public SizeF AddTextFloatRight(string text, Color color, int fontsize, float x, float y, string fontstyle = "Tourmaline")
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
                    AddTextFloatRight(Math.Round(100 - ((dataPointyMax * 100) /* times 100 for the acc graph... */ - (precisionPointXJump * t)), 2).ToString() + "%", Color.White, 12, x - 350, (y + height - precisionLineJump * t) - 11);
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

        public Bitmap AddImage(string path, float x, float y, int width, int height, float opacity = 1, bool isLocalFile = false, int blurItensity = 0, int cornerRadius = 0)
        {
            Image overlayImage = null;
            if (isLocalFile)
            {
                overlayImage = Image.FromFile(path);
            }
            else
            {
                WebRequest request;
                try
                {
                    request = WebRequest.Create(path);
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        var bitmap = Bitmap.FromStream(stream);
                        if (blurItensity != 0)
                        {
                            var streamBitmap = new Bitmap(bitmap);
                            bitmap = AddBlur(streamBitmap, blurItensity);
                        }
                        overlayImage = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    // An error occurred while trying to download the image
                    Console.WriteLine("An error occurred while trying to download the image: {0}", ex.Message);

                    request = WebRequest.Create("https://www.thermaxglobal.com/wp-content/uploads/2020/05/image-not-found.jpg");
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        overlayImage = Bitmap.FromStream(stream);
                    }
                }
            }

            Graphics g = Graphics.FromImage(_bitmap);

            if (cornerRadius > 0) overlayImage = RoundCorners(overlayImage, cornerRadius, Color.FromArgb(0, 32, 32, 32));

            overlayImage = SetImageOpacity(overlayImage, opacity);

            g.DrawImage(overlayImage, x, y, width, height);

            return (Bitmap)overlayImage;
        }

        public void AddMask(string path, bool isLocalFile = false)
        {
            //Get image
            Image overlayImage = null;
            if (isLocalFile)
            {
                overlayImage = Image.FromFile(path);
            }
            else
            {
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
            }


            //Remove pixels in image from mask
            Bitmap OrgImg = (Bitmap)overlayImage;
            Bitmap NewImg = _bitmap;
            for (int yy = 0; yy <= OrgImg.Height - 1; yy++)
            {
                for (int xx = 0; xx <= OrgImg.Width - 1; xx++)
                {
                    if (OrgImg.GetPixel(xx, yy).A == 255)
                    {
                        NewImg.SetPixel(xx, yy, Color.FromArgb(255 - OrgImg.GetPixel(xx, yy).A, 255, 0, 0));
                    }
                }
            }

            _bitmap = NewImg;
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

        public void DrawRectangle(int x, int y, int with, int height, Color? fillColor = null, Color? outerColor = null, int opacity = 255, float cornerRadius = 0)
        {
            // Create pen.
            SolidBrush brush = new SolidBrush(Color.White);
            if (fillColor != null)
            {
                var filling = Color.FromArgb(fillColor.Value.A == null || opacity != 255 ? opacity : fillColor.Value.A, (Color)fillColor);
                brush = new SolidBrush(filling);
            }

            Pen pen = new Pen(Color.Gray, 3);
            if (outerColor != null)
            {
                pen = new Pen((Color)outerColor, 3);
            }

            // Create rectangle.
            Rectangle rect = new Rectangle(x, y, with, height);

            if (cornerRadius != 0)
            {

            }

            // Draw rectangle to screen.
            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                graphics.FillRectangle(brush, rect);
                if (outerColor != null) graphics.DrawRectangle(pen, rect);
            }
        }

        public void AddImageRounded(string path, float x, float y, int width, int height, float opacity = 1, int blurSize = 0, int cornerRadius = 25)
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

            if (blurSize > 0) overlayImage = Blur((Bitmap)overlayImage, blurSize);

            var imageRounded = RoundCorners(overlayImage, cornerRadius, Color.FromArgb(0, 32, 32, 32));
            g.DrawImage(SetImageOpacity(imageRounded, opacity), x, y, width, height);

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


        public static Bitmap Blur(Bitmap image, Int32 blurSize)
        {
            return Blur(image, new Rectangle(0, 0, image.Width, image.Height), blurSize);
        }

        private static Bitmap Blur(Bitmap image, Rectangle rectangle, Int32 blurSize)
        {
            Bitmap blurred = new Bitmap(image.Width, image.Height);

            // make an exact copy of the bitmap provided
            using (Graphics graphics = Graphics.FromImage(blurred))
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

            // look at every pixel in the blur rectangle
            for (int xx = rectangle.X; xx < rectangle.X + rectangle.Width; xx++)
            {
                for (int yy = rectangle.Y; yy < rectangle.Y + rectangle.Height; yy++)
                {
                    int avgR = 0, avgG = 0, avgB = 0;
                    int blurPixelCount = 0;

                    // average the color of the red, green and blue for each pixel in the
                    // blur size while making sure you don't go outside the image bounds
                    for (int x = xx; (x < xx + blurSize && x < image.Width); x++)
                    {
                        for (int y = yy; (y < yy + blurSize && y < image.Height); y++)
                        {
                            Color pixel = blurred.GetPixel(x, y);

                            avgR += pixel.R;
                            avgG += pixel.G;
                            avgB += pixel.B;

                            blurPixelCount++;
                        }
                    }

                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;

                    // now that we know the average for the blur size, set each pixel to that color
                    for (int x = xx; x < xx + blurSize && x < image.Width && x < rectangle.Width; x++)
                        for (int y = yy; y < yy + blurSize && y < image.Height && y < rectangle.Height; y++)
                            blurred.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
                }
            }

            return blurred;
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

        Image DownloadImage(string fromUrl, int width, int height)
        {
            using (System.Net.WebClient webClient = new System.Net.WebClient())
            {
                using (Stream stream = webClient.OpenRead(fromUrl))
                {
                    return ResizeImage(Image.FromStream(stream), width, height);
                }
            }
        }

        public void ResizeImage(int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(_bitmap.HorizontalResolution, _bitmap.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(_bitmap, destRect, 0, 0, _bitmap.Width, _bitmap.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            _bitmap = destImage;
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap AddBlur(Bitmap bitmap, int itensity = 5)
        {

            using (SixLabors.ImageSharp.Image<Rgba32> image = new SixLabors.ImageSharp.Image<Rgba32>(bitmap.Width, bitmap.Height))
            {
                // Copy the pixel data from the bitmap into the ImageSharp image
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        Color pixel = bitmap.GetPixel(x, y);
                        image[x, y] = new Rgba32(pixel.R, pixel.G, pixel.B, pixel.A);
                    }
                }

                // Apply a Gaussian blur filter to the image
                image.Mutate(x => x.GaussianBlur(itensity));
                using (MemoryStream stream = new MemoryStream())
                {
                    image.SaveAsBmp(stream);
                    Bitmap output = new Bitmap(stream);
                    return output;
                }
            }
        }

        public Color GetAverageImagePixel(Bitmap image)
        {
            int totalRed = 0;
            int totalGreen = 0;
            int totalBlue = 0;
            int pixelCount = 0;

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixelColor = image.GetPixel(x, y);
                    totalRed += pixelColor.R;
                    totalGreen += pixelColor.G;
                    totalBlue += pixelColor.B;
                    pixelCount++;
                }
            }

            int averageRed = totalRed / pixelCount;
            int averageGreen = totalGreen / pixelCount;
            int averageBlue = totalBlue / pixelCount;

            return Color.FromArgb(averageRed, averageGreen, averageBlue);
        }

        public Color GetBackgroundContrast(Color color)
        {
            var temp = new HSV();
            temp.h = color.GetHue();
            double contrastingHue = temp.h - 40;
            temp.h = (float)contrastingHue;
            temp.s = color.GetSaturation();
            temp.v = getBrightness(color);

            if (temp.s == 1)
            {
                temp.s = 0.5f;
            }
            if (temp.v > 0.1)
            {
                var contrastingBrightness = 0.0f;
                if (temp.v <= 0.3) contrastingBrightness = temp.v - 0.2f;
                else if (temp.v > 0.3 && temp.v <= 0.5) contrastingBrightness = temp.v - 0.25f;
                else if (temp.v > 0.5 && temp.v <= 0.8) contrastingBrightness = temp.v - 0.3f;
                else if (temp.v > 0.8) contrastingBrightness = temp.v - 0.4f;

                if (contrastingBrightness < 0) contrastingBrightness = 0.0f;
                temp.v = contrastingBrightness;
            }
            else
            {
                temp.v += 0.2f;
            }

            //If color is black
            if(color.R < 10 && color.G < 10 && color.B < 10)
            {
                temp.h = color.GetHue();
                temp.s = 0.05f;
                temp.v = 0.8f;
            }

            return ColorFromHSL(temp);
        }

        public Color GetHighlightColor(Color color)
        {
            var temp = new HSV();
            temp.h = color.GetHue();
            temp.s = color.GetSaturation();
            temp.v = getBrightness(color);
            temp.v = temp.v + 0.3f;
            if (temp.v > 1) temp.v = 1;

            //If color is black
            if (color.R < 10 && color.G < 10 && color.B < 10)
            {
                temp.h = color.GetHue();
                temp.s = 0.05f;
                temp.v = 0.8f;
            }

            return ColorFromHSL(temp);
        }

        // A common triple float struct for both HSL & HSV
        // Actually this should be immutable and have a nice constructor!!
        public struct HSV { public float h; public float s; public float v; }

        // the Color Converter
        static public Color ColorFromHSL(HSV hsl)
        {
            if (hsl.s == 0)
            { int L = (int)hsl.v; return Color.FromArgb(255, L, L, L); }

            double min, max, h;
            h = hsl.h / 360d;

            max = hsl.v < 0.5d ? hsl.v * (1 + hsl.s) : (hsl.v + hsl.s) - (hsl.v * hsl.s);
            min = (hsl.v * 2d) - max;

            Color c = Color.FromArgb(255, (int)(255 * RGBChannelFromHue(min, max, h + 1 / 3d)),
                                          (int)(255 * RGBChannelFromHue(min, max, h)),
                                          (int)(255 * RGBChannelFromHue(min, max, h - 1 / 3d)));
            return c;
        }

        // color brightness as perceived:
        float getBrightness(Color c)
        { return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f; }
        static double RGBChannelFromHue(double m1, double m2, double h)
        {
            h = (h + 1d) % 1d;
            if (h < 0) h += 1;
            if (h * 6 < 1) return m1 + (m2 - m1) * 6 * h;
            else if (h * 2 < 1) return m2;
            else if (h * 3 < 2) return m1 + (m2 - m1) * 6 * (2d / 3d - h);
            else return m1;

        }
    }
}
