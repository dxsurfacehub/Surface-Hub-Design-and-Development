using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Contoso_Jobs.Common
{
    public class SoftwareBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ImageSource source = null;

            SoftwareBitmap softwareBitmap = value as SoftwareBitmap;
            if (null != softwareBitmap)
            {
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                        softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap,
                        BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                SoftwareBitmapSource swBmpSource = new SoftwareBitmapSource();
                swBmpSource.SetBitmapAsync(softwareBitmap);
                source = swBmpSource;
            }


            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
