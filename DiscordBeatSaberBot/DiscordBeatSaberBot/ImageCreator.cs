using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DiscordBeatSaberBot
{
    class ImageCreator
    {
        private Bitmap _bitmap;
        public ImageCreator()
        {
            string imageFilePath = "../../../Resources/Img/background.png";
            _bitmap = (Bitmap)Image.FromFile(imageFilePath);
        }

        public void Create()
        {
            _bitmap.Save("../../../Resources/Img/scoresaberProfile.png");
        }

        public void AddText(string text, Brush color, float x, float y)
        {
            PointF firstLocation = new PointF(x, y);

            using (Graphics graphics = Graphics.FromImage(_bitmap))
            {
                using (Font arialFont = new Font("Arial", 36))
                {
                    graphics.DrawString(text, arialFont, Brushes.White, firstLocation);
                }
            }
        }
    }
}
