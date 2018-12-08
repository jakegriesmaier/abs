using System;
using System.Collections.Generic;

namespace abs {
    public class ProgressStatistics {
        private DateTime Origin_X; //when x = 0, the date is this
        private TimeSpan Unit_X;   //one unit of x is this length of time
        private double Origin_Y;   //when y = 0, the one rep max is this
        private double Unit_Y;     //one unit of y is this amount of weight

        private bool done = false;
        private List<KeyValuePair<DateTime, double>> rawData = new List<KeyValuePair<DateTime, double>>();
        private List<Vec2> points = null;

        //linear regression
        private double Lin_M, Lin_B;
        public double Lin_Slope => Lin_M * Unit_Y / Unit_X.TotalDays;
        public double Lin_Significance => Math.Max(0, Math.Min(10, (points == null) ? 0 : (points.Count - 2))) * 0.1; //0 if there are no data points



        public void Finish() {
            LinearRegression();
        }

        private void LinearRegression() {
            NormalizeData();
            if (points == null) {
                Lin_M = 0.0;
                Lin_B = 0.0;
            } else {

                double xmean = 0;
                for (int i = 0; i < points.Count; i++) {
                    xmean += points[i].x;
                }
                xmean /= points.Count;

                double ymean = 0;
                for (int i = 0; i < points.Count; i++) {
                    ymean += points[i].y;
                }
                ymean /= points.Count;

                double num = 0;
                for (int i = 0; i < points.Count; i++) {
                    num += (points[i].x - xmean) * (points[i].y - ymean);
                }

                double denom = 0;
                for (int i = 0; i < points.Count; i++) {
                    double dif = (points[i].x - xmean);
                    denom += dif * dif;
                }

                Lin_M = num / denom;
                Lin_B = ymean - Lin_M * xmean;
            }
        }

        private void NormalizeData() {
            if (done) return;
            done = true;

            if (rawData.Count <= 1) return;

            DateTime min = DateTime.MaxValue, max = DateTime.MinValue;
            double ymin = double.MaxValue, ymax = double.MinValue;
            foreach(var kvp in rawData) {
                if (kvp.Key < min) min = kvp.Key;
                if (kvp.Key > max) max = kvp.Key;
                if (kvp.Value < ymin) ymin = kvp.Value;
                if (kvp.Value > ymax) ymax = kvp.Value;
            }

            if (min == max) return;

            Origin_X = new DateTime((max.Ticks + min.Ticks) / 2);
            Unit_X = max - Origin_X;
            Origin_Y = (ymin + ymax) / 2.0;
            Unit_Y = ymax - Origin_Y;

            points = new List<Vec2>(rawData.Count);
            for(int i = 0; i < rawData.Count; i++) {
                points[i] = new Vec2((rawData[i].Key - Origin_X).Ticks / (double) Unit_X.Ticks, (rawData[i].Value - Origin_Y) / Unit_Y);
            }
        }

        public void AddDataPoint(DateTime time, double oneRM) {
            if (done) throw new Exception("You can't add data once you call Finish()");
            rawData.Add(new KeyValuePair<DateTime, double>(time, oneRM));
        }
    }
}
