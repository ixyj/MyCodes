using System.Drawing;

namespace Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using MyDrawing;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var lines =
                    File.ReadAllLines(@"C:\Users\yajxu\Desktop\CCR Gap\[Threshold QF] Suggestion Group Metrics_[Win10] Top Hit CCR.tsv")
                        .Select(x => x.Split("\t".ToArray()));

                var scenarioCount = lines.First().Length / 2 - 1;
                var scenarios = lines.First().Where((x, i) => i > 1 && i <= 1 + scenarioCount).Select(x => x.Substring(0, x.IndexOf(':')));
                var dates = lines.Where((x, i) => i > 0).Select(x => x[0]).Reverse();

                var folder = @"C:\Users\yajxu\Desktop\CCR Gap\test";
                //if (Directory.Exists(folder))
                //{
                //    return;
                //}

                Directory.CreateDirectory(folder);
                var index = 0;

                var images = new List<Bitmap>();
                foreach (var scenario in scenarios)
                {
                    var dat = lines.Where((x, i) => i > 0).Select(x => double.Parse(x[2 + scenarioCount + index]) - double.Parse(x[2 + index])).Reverse();

                    var data = new List<KeyValuePair<string, double?[]>>
                    {
                        new KeyValuePair<string, double?[]>(scenario, dat.Select(x => (double?)x).ToArray())
                    };

                    var graph = new DrawGraph(scenario);
                    graph.InitData("Date", "CCR Gap (%)", dates.ToArray(), data, new[] { "AllImpressions" });

                    images.Add(graph.DrawImage(string.Format("{0}\\{1}.png", folder, scenario)));
                    ++index;
                }

                DrawGraph.MergeImg(images, string.Format("{0}\\merged.png", folder));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

namespace MyDrawing
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;

    class DrawGraph
    {
        private readonly int _height = 0;
        private readonly int _width = 0;
        private readonly int _xEdgeDistance = 0;
        private readonly int _yEdgeDistance = 0;
        private string _xName;  // X-axis name
        private string _yName;  // Y-axis
        private string[] _xValue; // X-axis value
        private readonly string _tableName;
        private double yMin;
        private string _precision;
        private string[] _dataNames;

        private List<KeyValuePair<string, double?[]>> data;

        public DrawGraph(string tableName, int width = 800, int height = 500, string precision = "F2", int xEdgeDistance = 40, int yEdgeDistance = 40)
        {
            this._width = width;
            this._height = height;
            this._tableName = tableName;
            this._precision = precision;
            this._xEdgeDistance = xEdgeDistance;
            this._yEdgeDistance = yEdgeDistance;
        }

        public void InitData(string xName, string yName, string[] xValue, List<KeyValuePair<string, double?[]>> data, string[] dataNames = null)
        {
            if (data.Any(x => x.Value.Length != xValue.Length))
            {
                throw new Exception("the length for data/yName is not euqal!");
            }

            if (data.SelectMany(x => x.Value).All(x => x == null))
            {
                throw new Exception("null data!");
            }

            if (dataNames != null && data.Count != dataNames.Length)
            {
                throw new Exception("data.Count != dataNames.Length!");
            }

            this._xName = xName;
            this._yName = yName;
            this._xValue = xValue;
            this.data = data;
            this._dataNames = dataNames;
        }

        public static void MergeImg(IEnumerable<Bitmap> imgs, string file)
        {
            var width = imgs.Select(x => x.Width).Max();
            var height = imgs.Sum(x => x.Height);

            var image = new Bitmap(width, height);
            var g = Graphics.FromImage(image);

            g.Clear(System.Drawing.Color.White);

            int y = 0;
            foreach (var img in imgs)
            {
                g.DrawImage(img, 0, y, width, img.Height);
                y += img.Height;
            }

            g.Dispose();
            image.Save(file, ImageFormat.Png);
        }

        public Bitmap DrawImage(string imgFile, bool showTextInGraph = true)
        {
            var image = new Bitmap(_width, _height);
            var g = Graphics.FromImage(image);
            var font = new Font("Arial", 9, FontStyle.Regular);
            var pen = new Pen(new SolidBrush(Color.Blue), 1);
            g.Clear(Color.White);
            g.FillRectangle(Brushes.WhiteSmoke, 0, 0, _width, _height);

            var xTag = _xEdgeDistance;
            var yTag = _height - _yEdgeDistance;
            var xLength = (int)(_width - 2.5 * _xEdgeDistance);
            var yLength = (int)(_height - 2.5 * _yEdgeDistance);

            var yMax = data.SelectMany(x => x.Value).Max() ?? 0;
            yMin = data.SelectMany(x => x.Value).Min() ?? 0;
            var yRat = yLength / (yMax - yMin);
            var xRat = ((double)xLength) / _xValue.Length;

            // X-axis, Y-axis
            g.DrawLine(pen, xTag, yTag, xTag + xLength, yTag);
            g.DrawLine(pen, xTag, yTag, xTag, yTag - yLength);
            // X-axis
            g.DrawLine(pen, xTag + xLength, yTag, xTag + xLength - 5, yTag - 5);
            g.DrawLine(pen, xTag + xLength, yTag, xTag + xLength - 5, yTag + 5);
            // Y-axis
            g.DrawString(yMin.ToString(_precision), font, Brushes.Black, xTag - _xEdgeDistance + 1, yTag - 15);
            g.DrawLine(pen, xTag, yTag, xTag + 5, yTag);
            g.DrawString(yMax.ToString(_precision), font, Brushes.Black, xTag - _xEdgeDistance + 1, yTag - yLength - 5);
            g.DrawLine(pen, xTag, yTag - yLength, xTag + 5, yTag - yLength + 5);
            g.DrawLine(pen, xTag, yTag - yLength, xTag - 5, yTag - yLength + 5);

            if (yMin < 0)
            {
                // Y - zero
                var yZero = yTag + yMin / (yMax - yMin) * yLength;
                g.DrawLine(new Pen(new SolidBrush(Color.DarkGray)), xTag, (int)yZero, xTag + xLength, (int)yZero);
            }

            // Drawing tags
            g.DrawString(_xName, font, Brushes.Black, xTag + xLength + 5, yTag - 7);
            g.DrawString(_yName, font, Brushes.Black, xTag - 5, yTag - yLength - _xEdgeDistance + 10);
            g.DrawString(_tableName, new Font("Arial", 12, FontStyle.Bold), Brushes.Red, xTag + xLength / 2, yTag - yLength - _yEdgeDistance);

            Drawing(g, xTag, yTag, xRat, yRat, pen, showTextInGraph);

            g.Dispose();

            image.Save(imgFile, ImageFormat.Png);

            return image;
        }

        private void Drawing(Graphics g, int x, int y, double xRat, double yRat, Pen pen, bool showTextInGraph)
        {
            var font = new Font("Arial", 9, FontStyle.Regular);
            double? lastY = null;
            float lastX = x;

            for (var dataIndex = 0; dataIndex < data.Count; dataIndex++)
            {
                var item = data[dataIndex];
                if (item.Value[0] != null)
                {
                    g.DrawString((item.Value[0] ?? 0).ToString(_precision), font, Brushes.Black, x + 5, (int)(y - (item.Value[0] - yMin) * yRat));
                }

                for (var i = 1; i < _xValue.Length; i++)
                {
                    var nowX = (float)(x + xRat * i);
                    var nowY = item.Value[i] == null ? null : y - (item.Value[i] - yMin) * yRat;

                    g.DrawLine(pen, (int)lastX, y, (int)lastX, y - 3);

                    if (i % 4 == 1)
                    {
                        // X-axis
                        g.DrawLine(pen, (int)nowX, y, (int)nowX, y + 3);
                        var xValueLength = g.MeasureString(_xValue[i], font).Width;
                        g.DrawString(_xValue[i], font, Brushes.Black, nowX - xValueLength / 2, (float)(y + 10));
                    }

                    if (nowY != null && lastY != null)
                    {
                        g.DrawLine(pen, (int)lastX, (int)lastY, (int)nowX, (int)nowY);
                    }

                    if (i + 1 == _xValue.Length)
                    {
                        g.DrawString(_dataNames[dataIndex], font, Brushes.Black, nowX + 3, (float)nowY-10);
                    }

                    if (showTextInGraph && item.Value[i] != null)
                    {
                        var y3 = i + 1 == _xValue.Length || item.Value[i] == null
                            ? null
                            : y - (item.Value[i + 1] - yMin)*yRat;
                        var text = (item.Value[i] ?? 0).ToString(_precision);
                        DrawTextInGraph(g, (int) lastX, lastY, (int) nowX, nowY, (int) (nowX + xRat), y3, pen, font, text);
                    }

                    lastX = nowX;
                    lastY = nowY;
                }
            }
        }

        private void DrawTextInGraph(Graphics g, int x1, double? y1, int x2, double? y2, int x3, double? y3, Pen pen, Font font, string text)
        {
            if (y2 == null)
            {
                return;
            }

            var size = g.MeasureString(text, font);

            if (y1 == null)
            {
                g.DrawString(text, font, Brushes.Black, x2 - size.Width, (int)y2);
                return;
            }
            else if (y3 == null)
            {
                g.DrawString(text, font, Brushes.Black, x2, (int)y2);
                return;
            }

            if (y1 >= y2)
            {
                if (y2 >= y3)
                {
                    g.DrawString(text, font, Brushes.Black, x2, (int)y2 + size.Height);
                }
                else
                {
                    g.DrawString(text, font, Brushes.Black, x2 - size.Width / 2, (int)y2 - size.Height);
                }
            }
            else
            {
                if (y2 >= y3)
                {
                    g.DrawString(text, font, Brushes.Black, x2 - size.Width / 2, (int)y2);
                }
                else
                {
                    g.DrawString(text, font, Brushes.Black, x2, (int)y2 - size.Height);
                }
            }
        }
    }
}