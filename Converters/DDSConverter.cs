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

            try
            {
                using (var image = Pfim.Pfim.FromFile(filePath))
                {
                    PixelFormat format = PixelFormat(image);

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
            } catch (Exception e) {
                return null;
            }
        }

        private static PixelFormat PixelFormat(IImage image)
        {
            switch (image.Format)
            {
                case ImageFormat.Rgb24:
                    return PixelFormats.Bgr24;
                case ImageFormat.Rgba32:
                    return PixelFormats.Bgra32;
                case ImageFormat.Rgb8:
                    return PixelFormats.Gray8;
                case ImageFormat.R5g5b5a1:
                    return PixelFormats.Bgr555;
                case ImageFormat.R5g5b5:
                    return PixelFormats.Bgr555;
                case ImageFormat.R5g6b5:
                    return PixelFormats.Bgr565;
                default:
                    throw new Exception($"Unable to convert {image.Format} to WPF PixelFormat");
            }
        }
    }
}
