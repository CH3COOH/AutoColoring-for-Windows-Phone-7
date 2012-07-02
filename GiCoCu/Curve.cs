//
// Curves.cs
//
// original work is E-Male.
// http://avisynth.org.ru/docs/english/externalfilters/gicocu.htm
// http://avisynth.org/warpenterprises/files/gicocu_25_dll_20050620.zip
//
// Modified for work on Silverlight by ch3cooh, http://ch3cooh.jp/
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files 
// (the "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Diagnostics;
using System.IO;

namespace GenericVideoFilter
{

    /// <summary>
    /// 
    /// </summary>
    class CRMatrix
    {
        public float[,] data;

        public CRMatrix()
        {
            this.data = new float[4, 4];
        }

        public CRMatrix(float[,] data)
        {
            this.data = data;
        }

        public CRMatrix compose(CRMatrix b)
        {
            CRMatrix result = new CRMatrix();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    result.data[i, j] = this.data[i, 0] * b.data[0, j] +
                                        this.data[i, 1] * b.data[1, j] +
                                        this.data[i, 2] * b.data[2, j] +
                                        this.data[i, 3] * b.data[3, j];
                }
            }
            return result;
        }
    }

    public enum CurveTypes
    {
        Auto,
        PhotoShop,
        Gimp
    }


    /// <summary>
    /// 
    /// </summary>
    public class Curve
    {

        #region Constructor

        public Curve() { }

        public Curve(Stream strm)
            : this(strm, CurveTypes.Auto)
        {
            throw new NotImplementedException();
        }

        public Curve(Stream strm, CurveTypes type)
        {

            Curve temp = null;
            if (type == CurveTypes.PhotoShop)
            {
                temp = Curve.GetPhotoShopCurve(strm);
            }
            else if (type == CurveTypes.Gimp)
            {
                temp = Curve.GetGimpCurve(strm);
            }
            else
            {
                throw new ArgumentException("invalid is CurveTypes.");
            }

            Data = temp.Data;
            Points = temp.Points;
        }
        #endregion

        private static Curve GetPhotoShopCurve(Stream strm)
        {
            throw new NotImplementedException();
        }

        private static Curve GetGimpCurve(Stream strm)
        {
            var curves = new Curve();

            int[,] index = new int[5, 17];
            int[,] value = new int[5, 17];

            var reader = new StreamReader(strm);

            // ヘッダーがGIMP形式になっているかチェック
            var header = reader.ReadLine();
            if ("# GIMP Curves File" != header)
            {
                throw new IOException("not gimp curves file");
            }

            for (int i = 0; i < 5; i++)
            {
                string line = reader.ReadLine();
                var values = line.Split(' ');

                for (int j = 0, k = 0; j < 17; j++)
                {
                    index[i, j] = int.Parse(values[k]);
                    k++;
                    value[i, j] = int.Parse(values[k]);
                    k++;

                    Debug.WriteLine("index: {0}, value: {0}", index[i, j], value[i, j]);
                }
            }

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 17; j++)
                {
                    curves.Points[i, j, 0] = index[i, j];
                    curves.Points[i, j, 1] = value[i, j];
                }
            }

            // make LUTs
            for (int i = 0; i < 5; i++)
            {
                curves.Calculate(i);
            }

            return curves;
        }

        private int[, ,] _points = new int[5, 17, 2];
        public int[, ,] Points
        {
            get
            {
                return _points;
            }
            set
            {
                _points = value;
            }
        }

        private int[,] _data = new int[5, 256];
        public int[,] Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        private readonly int MaxPoint = 17;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        public void Calculate(int channel)
        {
            var points = new int[MaxPoint];

            int num_pts = 0;
            for (int i = 0; i < MaxPoint; i++)
            {
                if (Points[channel, i, 0] != -1)
                {
                    points[num_pts++] = i;
                }
            }

            if (num_pts != 0)
            {
                for (int i = 0; i < Points[channel, points[0], 0]; i++)
                {
                    Data[channel, i] = Points[channel, points[0], 1];
                }
                for (int i = Points[channel, points[num_pts - 1], 0]; i < 256; i++)
                {
                    Data[channel, i] = Points[channel, points[num_pts - 1], 1];
                }
            }

            for (int i = 0; i < num_pts - 1; i++)
            {
                int p1 = (i == 0) ? points[i] : points[(i - 1)];
                int p2 = points[i];
                int p3 = points[(i + 1)];
                int p4 = (i == (num_pts - 2)) ? points[(num_pts - 1)] : points[(i + 2)];

                plotCurve(channel, p1, p2, p3, p4);
            }

            for (int i = 0; i < num_pts; i++)
            {
                int x = Points[channel, points[i], 0];
                int y = Points[channel, points[i], 1];
                Data[channel, x] = y;
            }
        }

        private void plotCurve(int channel, int p1, int p2, int p3, int p4)
        {
            // construct the geometry matrix from the segment
            CRMatrix geometry = new CRMatrix();
            for (int i = 0; i < 4; i++)
            {
                geometry.data[i, 2] = 0;
                geometry.data[i, 3] = 0;
            }
            for (int i = 0; i < 2; i++)
            {
                geometry.data[0, i] = Points[channel, p1, i];
                geometry.data[1, i] = Points[channel, p2, i];
                geometry.data[2, i] = Points[channel, p3, i];
                geometry.data[3, i] = Points[channel, p4, i];
            }

            // subdivide the curve 1000 times
            // n can be adjusted to give a finer or coarser curve
            float d = 1.0f / 1000;
            float d2 = d * d;
            float d3 = d * d * d;

            // construct a temporary matrix for determining the forward differencing
            // deltas
            CRMatrix tmp2 = new CRMatrix();
            tmp2.data[0, 0] = 0;
            tmp2.data[0, 1] = 0;
            tmp2.data[0, 2] = 0;
            tmp2.data[0, 3] = 1;
            tmp2.data[1, 0] = d3;
            tmp2.data[1, 1] = d2;
            tmp2.data[1, 2] = d;
            tmp2.data[1, 3] = 0;
            tmp2.data[2, 0] = 6 * d3;
            tmp2.data[2, 1] = 2 * d2;
            tmp2.data[2, 2] = 0;
            tmp2.data[2, 3] = 0;
            tmp2.data[3, 0] = 6 * d3;
            tmp2.data[3, 1] = 0;
            tmp2.data[3, 2] = 0;
            tmp2.data[3, 3] = 0;

            CRMatrix basis = new CRMatrix(new float[,] {
                { -0.5f, 1.5f, -1.5f, 0.5f },
                { 1.0f, -2.5f, 2.0f, -0.5f },
                { -0.5f, 0.0f, 0.5f, 0.0f },
                { 0.0f, 1.0f, 0.0f, 0.0f }
            });

            // compose the basis and geometry matrices
            CRMatrix tmp1 = basis.compose(geometry);

            // compose the above results to get the deltas matrix
            CRMatrix deltas = tmp2.compose(tmp1);

            // extract the x deltas
            float x = deltas.data[0, 0];
            float dx = deltas.data[1, 0];
            float dx2 = deltas.data[2, 0];
            float dx3 = deltas.data[3, 0];

            // extract the y deltas
            float y = deltas.data[0, 1];
            float dy = deltas.data[1, 1];
            float dy2 = deltas.data[2, 1];
            float dy3 = deltas.data[3, 1];

            int lastx = Math.Max(0, Math.Min(255, (int)Math.Round(x)));
            int lasty = Math.Max(0, Math.Min(255, (int)Math.Round(y)));

            Data[channel, lastx] = lasty;

            // loop over the curve
            for (int i = 0; i < 1000; i++)
            {
                // increment the x values
                x += dx;
                dx += dx2;
                dx2 += dx3;

                // increment the y values
                y += dy;
                dy += dy2;
                dy2 += dy3;

                int newx = Math.Max(0, Math.Min(255, (int)Math.Round(x)));
                int newy = Math.Max(0, Math.Min(255, (int)Math.Round(y)));

                // if this point is different than the last one...then draw it
                if ((lastx != newx) || (lasty != newy))
                {
                    Data[channel, newx] = newy;
                }

                lastx = newx;
                lasty = newy;
            }
        }

        void resetChannel(int channel)
        {
            for (int j = 0; j < 256; j++)
            {
                Data[channel, j] = j;
            }

            for (int j = 0; j < 17; j++)
            {
                Points[channel, j, 0] = -1;
                Points[channel, j, 1] = -1;
            }

            Points[channel, 0, 0] = 0;
            Points[channel, 0, 1] = 0;
            Points[channel, 16, 0] = 255;
            Points[channel, 16, 1] = 255;
        }

        void applyCurveHsv(int[] r, int[] g, int[] b)
        {
            int x, y;
            // RGB to HSV (x=H y=S z=V)
            int cmin = Math.Min(r[0], g[0]);
            cmin = Math.Min(b[0], cmin);
            int z = Math.Max(r[0], g[0]);
            z = Math.Max(b[0], z);
            int cdelta = z - cmin;
            if (cdelta != 0)
            {
                y = (cdelta << 8) / z;
                if (y > 255)
                    y = 255;
                if (r[0] == z)
                {
                    x = ((g[0] - b[0]) << 8) / cdelta;
                }
                else if (g[0] == z)
                {
                    x = 512 + (((b[0] - r[0]) << 8) / cdelta);
                }
                else
                {
                    x = 1024 + (((r[0] - g[0]) << 8) / cdelta);
                }
                if (x < 0)
                {
                    x = x + 1536;
                }
                x = x / 6;
            }
            else
            {
                y = 0;
                x = 0;
            }

            // Applying the curves
            x = Data[1, x];
            y = Data[2, y];
            z = Data[3, z];

            // HSV to RGB
            if (y == 0)
            {
                r[0] = z;
                g[0] = z;
                b[0] = z;
            }
            else
            {
                int chi = (x * 6) >> 8;
                int ch = (x * 6 - (chi << 8));
                int rd = (z * (256 - y)) >> 8;
                int gd = (z * (256 - ((y * ch) >> 8))) >> 8;
                int bd = (z * (256 - (y * (256 - ch) >> 8))) >> 8;
                if (chi == 0)
                {
                    r[0] = z;
                    g[0] = bd;
                    b[0] = rd;
                }
                else if (chi == 1)
                {
                    r[0] = gd;
                    g[0] = z;
                    b[0] = rd;
                }
                else if (chi == 2)
                {
                    r[0] = rd;
                    g[0] = z;
                    b[0] = bd;
                }
                else if (chi == 3)
                {
                    r[0] = rd;
                    g[0] = gd;
                    b[0] = z;
                }
                else if (chi == 4)
                {
                    r[0] = bd;
                    g[0] = rd;
                    b[0] = z;
                }
                else
                {
                    r[0] = z;
                    g[0] = rd;
                    b[0] = gd;
                }
            }
        }
    }

}
