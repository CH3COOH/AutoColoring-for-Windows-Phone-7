//
// GiCoCuLoader.cs
//
// original work is E-Male.
// http://avisynth.org.ru/docs/english/externalfilters/gicocu.htm
// http://avisynth.org/warpenterprises/files/gicocu_25_dll_20050620.zip
//
// Modified for work on Silverlight by Kenji Wada, http://ch3cooh.jp/
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
using System;
using System.Reflection;

namespace GenericVideoFilter
{
    public class GiCoCuLoader
    {
        /// <summary>
        /// デフォルトのカーブ情報を使用して疑似着色をおこなう
        /// </summary>
        /// <param name="srcBmp">加工元の画像</param>
        /// <returns>加工後の画像</returns>
        public static WriteableBitmap Effect(WriteableBitmap srcBmp)
        {
            return Effect(srcBmp, null);
        }

        /// <summary>
        /// アプリ側で保持しているカーブ情報を使用して疑似着色をおこなう
        /// カーブ情報にnullが設定されている場合は、デフォルトのカーブ情報を使用する
        /// </summary>
        /// <param name="srcBmp">加工元の画像</param>
        /// <param name="curve">カーブ情報</param>
        /// <returns>加工後の画像</returns>
        public static WriteableBitmap Effect(WriteableBitmap srcBmp, Curve curve)
        {
            Curve applyCurve = null;
            if (curve != null)
            {
                applyCurve = curve;
            }
            else
            {
                var curAssembly = Assembly.GetExecutingAssembly();
                var strm = curAssembly.GetManifestResourceStream("GenericVideoFilter.default_hosei.cur");
                applyCurve = new Curve(strm, CurveTypes.Gimp);
            }

            var filter = new GiCoCu(applyCurve);
            return filter.Process(srcBmp);
        }
    }
}
