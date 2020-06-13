using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBeatSaberBot.Api.ScoreberAPI
{
    class PpCalculator { 
 
        public static double GetMaxPpByCurrentPpAndAcc(double acc, double pp)
        {
            var accFake = acc;
            var ppFake = pp;
            var currentPp = pp;
            if (accFake < 45 && accFake >= 0)
            {
                var ppIndex = 0.015;
                var currentPpIndex = (accFake * ppIndex) / 45;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 45;
                ppFake = pp * thisMaxPpIndex;

            }
            if (accFake < 50 && accFake >= 45)
            {
                var ppIndex = 0.03;
                var currentPpIndex = (accFake * ppIndex) / 50;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 50;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 55 && accFake >= 50)
            {
                var ppIndex = 0.06;
                var currentPpIndex = (accFake * ppIndex) / 55;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 55;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 60 && accFake >= 55)
            {
                var ppIndex = 0.105;
                var currentPpIndex = (accFake * ppIndex) / 60;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 60;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 65 && accFake >= 60)
            {
                var ppIndex = 0.16;
                var currentPpIndex = (accFake * ppIndex) / 65;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 65;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 68 && accFake >= 65)
            {
                var ppIndex = 0.24;
                var currentPpIndex = (accFake * ppIndex) / 68;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 68;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 70 && accFake >= 68)
            {
                var ppIndex = 0.285;
                var currentPpIndex = (accFake * ppIndex) / 70;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 70;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 80 && accFake >= 70)
            {
                var ppIndex = 0.563;
                var currentPpIndex = (accFake * ppIndex) / 80;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 80;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 84 && accFake >= 80)
            {
                var ppIndex = 0.695;
                var currentPpIndex = (accFake * ppIndex) / 84;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 84;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 88 && accFake >= 84)
            {
                var ppIndex = 0.826;
                var currentPpIndex = (accFake * ppIndex) / 88;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 88;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 94.5 && accFake >= 88)
            {
                var ppIndex = 1.015;
                var currentPpIndex = (accFake * ppIndex) / 94.5;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 94.5;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 95 && accFake >= 94.5)
            {
                var ppIndex = 1.046;
                var currentPpIndex = (accFake * ppIndex) / 95;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 95;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake <= 100 && accFake >= 95)
            {
                var ppIndex = 1.12;
                var currentPpIndex = (accFake * ppIndex) / 100;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 100;
                ppFake = pp * thisMaxPpIndex;
            }

            var maxPp = ppFake;

            return maxPp;
        }

        public static double GetPpFromWishedAccByCurrentPpAndAcc(double acc, double pp, double wishedAcc)
        {
            var accFake = acc;
            var ppFake = pp;
            var currentPp = pp;
            if (accFake < 45 && accFake >= 0 && acc < wishedAcc)
            {
                var ppIndex = 0.015;
                var currentPpIndex = (accFake * ppIndex) / 45;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 45;
                ppFake = pp * thisMaxPpIndex;

            }
            if (accFake < 50 && accFake >= 45 && acc < wishedAcc)
            {
                var ppIndex = 0.03;
                var currentPpIndex = (accFake * ppIndex) / 50;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 50;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 55 && accFake >= 50 && acc < wishedAcc)
            {
                var ppIndex = 0.06;
                var currentPpIndex = (accFake * ppIndex) / 55;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 55;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 60 && accFake >= 55 && acc < wishedAcc)
            {
                var ppIndex = 0.105;
                var currentPpIndex = (accFake * ppIndex) / 60;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 60;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 65 && accFake >= 60 && acc < wishedAcc)
            {
                var ppIndex = 0.16;
                var currentPpIndex = (accFake * ppIndex) / 65;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 65;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 68 && accFake >= 65 && acc < wishedAcc)
            {
                var ppIndex = 0.24;
                var currentPpIndex = (accFake * ppIndex) / 68;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 68;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 70 && accFake >= 68 && acc < wishedAcc)
            {
                var ppIndex = 0.285;
                var currentPpIndex = (accFake * ppIndex) / 70;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 70;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 80 && accFake >= 70 && acc < wishedAcc)
            {
                var ppIndex = 0.563;
                var currentPpIndex = (accFake * ppIndex) / 80;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 80;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 84 && accFake >= 80 && acc < wishedAcc)
            {
                var ppIndex = 0.695;
                var currentPpIndex = (accFake * ppIndex) / 84;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 84;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 88 && accFake >= 84 && acc < wishedAcc)
            {
                var ppIndex = 0.826;
                var currentPpIndex = (accFake * ppIndex) / 88;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 88;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 94.5 && accFake >= 88 && acc < wishedAcc)
            {
                var ppIndex = 1.015;
                var currentPpIndex = (accFake * ppIndex) / 94.5;
                var thisMaxPpIndex = (currentPp * ppIndex) / ppFake;

                accFake = 94.5;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake < 95 && accFake >= 94.5 && acc < wishedAcc)
            {
                //1.046
                var ppIndex = 1.021;
                var currentPpIndex = (accFake * ppIndex) / 95;
                var thisMaxPpIndex = (currentPp * ppIndex) / currentPp;

                accFake = 95;
                ppFake = pp * thisMaxPpIndex;
            }
            if (accFake <= 100 && accFake >= 95 && acc < wishedAcc)
            {
                //1.15
                //1.0002208 per 0.01%
                var f = 100 - wishedAcc;
                var maxPp = (f * 100) * 1.0002208;

                var ppIndex = 1.104;
                var currentPpIndex = (accFake * ppIndex) / 100;
                var wishedPp = (wishedAcc * maxPp) / 100;
            }

            var ppFromWishedAcc = ppFake;

            return ppFromWishedAcc;
        }
    }
}
