//
// MainPage.xaml.cs
//
// work on Silverlight by ch3cooh, http://ch3cooh.jp/
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
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using GenericVideoFilter;
using Microsoft.Phone.Controls;

namespace AutoColor
{
    public partial class MainPage : PhoneApplicationPage
    {
        // コンストラクター
        public MainPage()
        {
            InitializeComponent();
        }

        WriteableBitmap _bitmap = null;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            DisplayOriginalPicture();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            // using default curves data.
            image.Source = GiCoCuLoader.Effect(_bitmap);

            //// using application's curves data.
            // TODO: gimp format only!!
            //var res = Application.GetResourceStream(new Uri("hosei.cur", UriKind.Relative));
            //image.Source = GiCoCuLoader.Effect(_bitmap, new Curve(res.Stream, CurveTypes.Gimp));
        }

        private void DisplayOriginalPicture()
        {
            // display test picture.
            var res = App.GetResourceStream(new Uri("Lenna.jpg", UriKind.Relative));
            var bmp = new BitmapImage();
            bmp.SetSource(res.Stream);
            _bitmap = new WriteableBitmap(bmp);
            image.Source = _bitmap;
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            DisplayOriginalPicture();
        }
    }
}