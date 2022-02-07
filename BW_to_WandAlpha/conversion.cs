using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BW_to_WandAlpha
{
    static public class conversion
    {
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource ToImageSource(this Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public static ImageSource ToImageSource(this OpenCvSharp.Mat mat)
        {
            try
            {
                return ToImageSource(mat.ToBitmap());
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}