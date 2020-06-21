using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordBeatSaberBot.Api.ScoreberAPI
{
    class PpCalculator
    {

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

        public static double GetPpFromWishedAccByCurrentPpAndAcc(double currentAcc, double currentScore, double maxScore, double wishedAcc)
        {
            var currentAccFake = currentAcc;
            double ppGain = 0;
            if (currentAccFake < 45 && currentAccFake >= 0 && currentAcc < wishedAcc)
            {


            }
            if (currentAccFake < 50 && currentAccFake >= 45 && currentAcc < wishedAcc)
            {

            }
            if (currentAccFake < 55 && currentAccFake >= 50 && currentAcc < wishedAcc)
            {

            }
            if (currentAccFake < 60 && currentAccFake >= 55 && currentAcc < wishedAcc)
            {

            }
            if (currentAccFake < 65 && currentAccFake >= 60 && currentAcc < wishedAcc)
            {

            }
            if (currentAccFake < 68 && currentAccFake >= 65 && currentAcc < wishedAcc)
            {

            }
            if (currentAccFake < 70 && currentAccFake >= 68 && currentAcc < wishedAcc)
            {

            }
            if (currentAccFake < 80 && currentAccFake >= 70 && currentAcc < wishedAcc)
            {
                getPpGain(80, StaticGraphData.GetSeventyToEightyIncrement(), StaticGraphData.SeventyToEightyMaxScore);
            }
            if (currentAccFake < 84 && currentAccFake >= 80 && currentAcc < wishedAcc)
            {
                getPpGain(84, StaticGraphData.GetEightyToEightyFourIncrement(), StaticGraphData.EightyToEightyFourMaxScore);
            }
            if (currentAccFake < 88 && currentAccFake >= 84 && currentAcc < wishedAcc)
            {
                getPpGain(88, StaticGraphData.GetEightyFourToEightyEightIncrement(), StaticGraphData.EightyFourToEightyEightMaxScore);
            }
            if (currentAccFake < 94.5 && currentAccFake >= 88 && currentAcc < wishedAcc)
            {
                getPpGain(94.5, StaticGraphData.GetEightyEightToNinetyFourIncrement(), StaticGraphData.EightyEightToNinetyFourMaxScore);
            }
            if (currentAccFake < 95 && currentAccFake >= 94.5 && currentAcc < wishedAcc)
            {
                getPpGain(95, StaticGraphData.GetNinetyfourToNinetyfiveIncrement(), StaticGraphData.NinetyfourToNinetyfiveMaxScore);                               
            }
            if (currentAccFake <= 100 && currentAccFake >= 95 && currentAcc < wishedAcc)
            {
                var staticIncrementValue = StaticGraphData.GetNinetyfiveToHonderdIncrement();
                var staticMaxScore = StaticGraphData.NinetyfiveToHonderdMaxScore;

                var incrementValue = (maxScore * staticIncrementValue) / staticMaxScore;

                var wishedScore = maxScore * (wishedAcc / 100);
                var scoreLeft = wishedScore - currentScore;
                ppGain += scoreLeft * incrementValue;
            }

            void getPpGain(double maxAcc, double staticIncrementValue, double staticMaxScore)
            {
                var incrementValue = (staticMaxScore / (maxScore * (maxAcc / 100)) * staticIncrementValue);

                //var incrementValue = ((maxScore * (maxAcc / 100)) * staticIncrementValue) / staticMaxScore;

                double wishedScore = 0;
                double scoreLeft = 0;
                if (wishedAcc > maxAcc)
                {
                    wishedScore = maxScore * (maxAcc / 100);
                    currentAccFake = maxAcc;
                    scoreLeft = wishedScore - currentScore;
                    currentScore = wishedScore;
                }
                else
                {
                    wishedScore = maxScore * (wishedAcc / 100);
                    scoreLeft = wishedScore - currentScore;
                }

                ppGain += scoreLeft * incrementValue;
            }

            return ppGain;
        }
    }

    //Glitched Character data
    public static class StaticGraphData
    {
        //95 to 100
        public static double NinetyfiveToHonderdMaxScore = 2402235;
        private static Dictionary<double, double> NinetyfiveToHonderdValues = new Dictionary<double, double>()
            {
                { 2313578, 416.11},
                { 2308130, 414.80},
                { 2304946, 414.03},
                { 2303154, 413.60},
                { 2297492, 412.24},
                { 2296701, 412.05},
                { 2295666, 411.80},
                { 2295258, 411.70},
                { 2292753, 411.10}
            };
        //94.5 to 95
        public static double NinetyfourToNinetyfiveMaxScore = 2402235 * 0.9500;
        private static Dictionary<double, double> NinetyfourToNinetyfiveValues = new Dictionary<double, double>()
            {
                { 2280138, 406.54},
                { 2279171, 405.57},
                { 2278097, 404.48},
                { 2277923, 404.31},
                { 2277478, 403.86},
                { 2276487, 402.86},
                { 2275855, 402.22},
                { 2273016, 399.36},
                { 2271782, 398.12}
            };

        //88 to 94.5
        public static double EightyEightToNinetyFourMaxScore = 2402235 * 0.9450;
        private static Dictionary<double, double> EightyEightToNinetyFourValues = new Dictionary<double, double>()
            {
                { 2265760, 394.38},
                { 2258448, 390.92},
                { 2256627, 390.06},
                { 2240135, 382.26},
                { 2227971, 376.51},
                { 2206510, 366.37},
                { 2179999, 353.83},
                { 2149618, 339.47},
                { 2135226 , 332.67}
            };

        //84 to 88
        public static double EightyFourToEightyEightMaxScore = 2402235 * 0.8800;
        private static Dictionary<double, double> EightyFourToEightyEightValues = new Dictionary<double, double>()
            {
                {2114084, 322.67},
                {2108093, 319.49},
                {2096520, 313.33},
                {2083355, 306.32},
                {2055815, 291.65},
                {2049109, 288.08},
                {2037510, 281.90},
                {2018148, 271.59}
            };

        //80 to 84
        public static double EightyToEightyFourMaxScore = 2402235 * 0.8400;
        private static Dictionary<double, double> EightyToEightyFourValues = new Dictionary<double, double>()
            {
                {2015926, 270.40},
                {2008882, 266.62},
                {1999721, 261.71},
                {1995298, 259.34},
                {1988657, 255.77},
                {1963401, 242.22},
                {1951715, 235.95},
                {1930117, 224.36}
            };

        //70 to 80
        public static double SeventyToEightyMaxScore = 2402235 * 0.8000;
        private static Dictionary<double, double> SeventyToEightyValues = new Dictionary<double, double>()
            {
                {1920140, 219.15},
                {1876986, 199.64},
                {1855768, 190.05},
                {1802910, 166.16},
                {1772875, 152.59},
                {1759808, 146.68},
                {1744634, 139.82},
                {1705475, 122.12}
            };



        public static double GetNinetyfiveToHonderdIncrement()
        {
            return IncrementCalculator(NinetyfiveToHonderdValues);
        }

        public static double GetNinetyfourToNinetyfiveIncrement()
        {
            return IncrementCalculator(NinetyfourToNinetyfiveValues);
        }

        public static double GetEightyEightToNinetyFourIncrement()
        {
            return IncrementCalculator(EightyEightToNinetyFourValues);
        }

        public static double GetEightyFourToEightyEightIncrement()
        {
            return IncrementCalculator(EightyFourToEightyEightValues);
        }

        public static double GetEightyToEightyFourIncrement()
        {
            return IncrementCalculator(EightyToEightyFourValues);
        }

        public static double GetSeventyToEightyIncrement()
        {
            return IncrementCalculator(SeventyToEightyValues);
        }

        private static double IncrementCalculator(Dictionary<double, double> scoreAndPp)
        {

            var incrementList = new List<double>();

            for (var x = 0; x < scoreAndPp.Count() - 1; x++)
            {

                var scoreDiff = scoreAndPp.Keys.ToArray()[x] - scoreAndPp.Keys.ToArray()[x + 1];
                var ppDiff = scoreAndPp.Values.ToArray()[x] - scoreAndPp.Values.ToArray()[x + 1];
                incrementList.Add(ppDiff / scoreDiff);
            }

            double total = 0;
            foreach (var inc in incrementList)
            {
                total += inc;
            }

            return total / scoreAndPp.Count();
        }
    }

}
