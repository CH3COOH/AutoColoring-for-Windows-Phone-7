//
// GiCoCu.cs
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
using System.Windows.Media.Imaging;

namespace GenericVideoFilter
{
    /// <summary>
    /// 疑似的に着色をおこなうクラス
    /// </summary>
    public class GiCoCu
    {
        private Curve _curve = null;

        public GiCoCu(Curve curve)
        {
            _curve = curve;
        }

        public WriteableBitmap Process(WriteableBitmap src)
        {
            int width = src.PixelWidth;
            int height = src.PixelHeight;

            var dst = new WriteableBitmap(width, height);

            // 画処理のためビットマップからbyte配列にコピーする
            var bytes = new int[width * height];
            src.Pixels.CopyTo(bytes, 0);

            // マッピングされたカーブテーブルを元に疑似着色を実施する
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    int argb = bytes[x + y * width];
                    int a = _curve.Data[4, (argb & 0xff000000) >> 24];
                    int r = _curve.Data[0, _curve.Data[1, (argb & 0x00ff0000) >> 16]];
                    int g = _curve.Data[0, _curve.Data[2, (argb & 0x0000ff00) >> 8]];
                    int b = _curve.Data[0, _curve.Data[3, (argb & 0x000000ff)]];
                    bytes[x + y * width] = a << 24 | r << 16 | g << 8 | b;
                }

            // 画処理後のbyte配列を出力側のビットマップへコピーする
            bytes.CopyTo(dst.Pixels, 0);

            return dst;
        }
    }
}
