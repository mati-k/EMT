using Pfim;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EMT.Converters
{
    public class DDSConverter : IValueConverter
    {
        private static readonly DDSConverter defaultInstace = new DDSConverter();

        public static DDSConverter Default
        {
            get { return DDSConverter.defaultInstace; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            else if (value is string)
            {
                return DDSConverter.Convert((string)value);
            }

            else
            {
                throw new NotSupportedException(string.Format("{0} cannot convert from {1}.", this.GetType().FullName, value.GetType().FullName));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(string.Format("{0} does not support converting back.", this.GetType().FullName));
        }

        public static ImageSource Convert(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            using (var image = Pfim.Pfim.FromFile(filePath))
            {
                PixelFormat format = PixelFormats.Bgra32;
                if (image.Format == ImageFormat.Rgb24)
                    format = PixelFormats.Rgb24;

                try
                {
                    var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    return BitmapSource.Create(image.Width, image.Height, 0, 0, format, null, data, image.Data.Length, image.Stride);
                }

                catch (Exception e)
                {
                    return null;
                }
            }
        }
    }
}
