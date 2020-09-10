using System;
using System.Drawing;

namespace NewFeatures
{
    class Program
    {
        static void Main(string[] args)
        {
            var rankingCardCreator = new ImageCreator();
            //Add player main info

            rankingCardCreator.AddText("SILVERHAZE", Color.White, 100, 1100, 800);
            rankingCardCreator.AddText("NL", Color.White, 100, 1250, 950);
            rankingCardCreator.AddText("#10", Color.White, 100, 1100, 1200);
            rankingCardCreator.AddText("#1", Color.FromArgb(176, 176, 176), 60, 1350, 1250);
            rankingCardCreator.AddText("12,875.10 PP", Color.White, 100, 1100, 1350);

            rankingCardCreator.AddImage("https://scoresaber.com/imports/images/flags/nl.png", 1120, 1000, 120, 100);
            rankingCardCreator.AddNoteSlashEffect("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/47/471cc0bb0dcb03e2bdcc8c7ce1fe48bc2aff3e57_full.jpg", 200, 800, 800, 800);

            //Add Date
            rankingCardCreator.AddText(DateTime.UtcNow.ToString("dd MMM. yyyy"), Color.White, 100, 3600, 650);
            rankingCardCreator.AddText(DateTime.UtcNow.ToString("hh:mm"), Color.FromArgb(176, 176, 176), 70, 3950, 800);

            //Add scorestats 
            rankingCardCreator.AddText("ACC.", Color.FromArgb(251, 211, 64), 90, 3350, 1140);
            rankingCardCreator.AddText("94.7%", Color.FromArgb(251, 211, 64), 100, 3750, 1125);

            rankingCardCreator.AddText("SCORE", Color.FromArgb(251, 211, 64), 90, 3350, 1300);
            rankingCardCreator.AddText("2,583,348,727", Color.FromArgb(251, 211, 64), 110, 3750, 1280);

            rankingCardCreator.AddText("RANKED", Color.FromArgb(251, 211, 64), 40, 3370, 1470);
            rankingCardCreator.AddText("504,263,641", Color.FromArgb(251, 211, 64), 60, 3760, 1450);

            rankingCardCreator.AddText("PLAYS", Color.FromArgb(251, 211, 64), 90, 3350, 1560);
            rankingCardCreator.AddText("2,592", Color.FromArgb(251, 211, 64), 110, 3750, 1540);

            rankingCardCreator.AddText("RANKED", Color.FromArgb(251, 211, 64), 40, 3370, 1720);
            rankingCardCreator.AddText("461", Color.FromArgb(251, 211, 64), 60, 3760, 1700);

            //Add best score
            rankingCardCreator.AddText("VILLAIN VIRUS", Color.White, 130, 100, 2300);
            rankingCardCreator.AddText("EXPERT", Color.FromArgb(176, 176, 176), 110, 100, 2500);
            rankingCardCreator.AddText("503.96PP", Color.White, 160, 2950, 2270);
            rankingCardCreator.AddText("94.50%", Color.FromArgb(176, 176, 176), 110, 3320, 2500);
            rankingCardCreator.AddImageRounded("https://new.scoresaber.com/api/static/covers/92C7490D903F3E676069B92B7DE9E56B03A9677A.png", 3950, 2020, 800, 800);

            rankingCardCreator.Create();
        }
    }
}
