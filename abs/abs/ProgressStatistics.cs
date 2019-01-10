using System;
using System.Linq;
using System.Collections.Generic;
using OxyPlot.Pdf;

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
        public double Lin_Slope => points == null ? 0 : (Lin_M * Unit_Y / Unit_X.TotalDays);
        public double Lin_Significance => Math.Max(0, Math.Min(10, (points == null) ? 0 : (points.Count - 2))) * 0.1; //0 if there are no data points

        public void MakePdf(string title, System.IO.Stream output) {
            const double width = 750, height = 500;

            OxyPlot.PdfRenderContext ctx = new OxyPlot.PdfRenderContext(width, height, OxyPlot.OxyColor.FromRgb(255, 255, 255));

            if (points.Count == 0) {
                ctx.DrawText(new OxyPlot.ScreenPoint(250, 250), "test", OxyPlot.OxyColor.FromRgb(0, 0, 0), "Consolas", 100, 10.0, 0.0, OxyPlot.HorizontalAlignment.Center, OxyPlot.VerticalAlignment.Middle, null);
            } else {
                const double bX = 50, bY = 25;
                const double mX = width - 2 * bX, mY = height - 2 * bY;

                ctx.DrawLine(new List<OxyPlot.ScreenPoint> {
                    new OxyPlot.ScreenPoint(bX, bY),
                    new OxyPlot.ScreenPoint(mX + bX, bY),
                    new OxyPlot.ScreenPoint(mX + bX, mY + bY),
                    new OxyPlot.ScreenPoint(bX, mY + bY),
                    new OxyPlot.ScreenPoint(bX, bY)
                }, OxyPlot.OxyColor.FromRgb(0, 0, 0), 2, null, OxyPlot.LineJoin.Miter, false);

                ctx.DrawLine(points.Select(point => new OxyPlot.ScreenPoint(point.x * mX + width / 2, point.y * mY + height / 2)).ToList(), OxyPlot.OxyColor.FromRgb(0, 0, 0), 2.0, null, OxyPlot.LineJoin.Round, false);

                ctx.DrawText(new OxyPlot.ScreenPoint(width / 2, bY / 2), title, OxyPlot.OxyColor.FromRgb(0, 0, 0), "Consolas", 20, 10.0, 0.0, OxyPlot.HorizontalAlignment.Center, OxyPlot.VerticalAlignment.Middle, null);

                const int sCount = 3;
                for (int i = 0; i < sCount; i++) {
                    double xNormalized = i / (double)(sCount - 1);
                    double yNormalized = i / (double)(sCount - 1);
                    double xScreen = bX + xNormalized * mX;
                    double yScreen = bY + (1 - yNormalized) * mY;
                    DateTime xlabel = Origin_X + Unit_X.Multiply(xNormalized);
                    double ylabel = Origin_Y + Unit_Y * yNormalized;
                    ctx.DrawText(new OxyPlot.ScreenPoint(xScreen, mY + 3 * bY / 2), xlabel.ToShortDateString(), OxyPlot.OxyColor.FromRgb(0, 0, 0), "Consolas", 15, 10.0, 0.0, OxyPlot.HorizontalAlignment.Center, OxyPlot.VerticalAlignment.Middle, null);
                    ctx.DrawText(new OxyPlot.ScreenPoint(bX / 2, yScreen), ylabel.ToString("0.0"), OxyPlot.OxyColor.FromRgb(0, 0, 0), "Consolas", 15, 10.0, 0.0, OxyPlot.HorizontalAlignment.Center, OxyPlot.VerticalAlignment.Middle, null);
                }
            }

            ctx.Save(output);
        }
  

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
            Unit_X = max - min;
            Origin_Y = (ymin + ymax) / 2.0;
            Unit_Y = ymax - ymin;

            points = new List<Vec2>(rawData.Count);
            for(int i = 0; i < rawData.Count; i++) {
                points.Add(new Vec2((rawData[i].Key - Origin_X).Ticks / (double) Unit_X.Ticks, (rawData[i].Value - Origin_Y) / Unit_Y));
            }
        }

        public void AddDataPoint(DateTime time, double oneRM) {
            if (done) throw new Exception("You can't add data once you call Finish()");
            rawData.Add(new KeyValuePair<DateTime, double>(time, oneRM));
        }
    }
}
