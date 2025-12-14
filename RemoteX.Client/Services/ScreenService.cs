using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace RemoteX.Client.Services
{
    public static class ScreenService
    {
        //Chụp màn hình hiện tại
        public static Bitmap CaptureScreen()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            var bmp = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            }

            return bmp;
        }

        //Nén ảnh
        public static byte[] CompressToJpeg(Bitmap bmp, long quality)
        {
            using (var ms = new MemoryStream())
            {
                var encoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);

                bmp.Save(ms, encoder, encoderParams);
                return ms.ToArray();
            }
        }

        public static BitmapImage ConvertToBitmapImage(byte[] dataBuffer, int length)
        {
            //dùng MemoryStream để đọc byte[]
            using var ms = new MemoryStream(dataBuffer, 0, length, writable: false); //chỉ đọc đến length
            var bmpImage = new BitmapImage();
            bmpImage.BeginInit();
            bmpImage.CacheOption = BitmapCacheOption.OnLoad;
            bmpImage.StreamSource = ms;
            bmpImage.EndInit();
            bmpImage.Freeze(); //thread-safe
            return bmpImage;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return Array.Find(ImageCodecInfo.GetImageDecoders(), c => c.FormatID == format.Guid);
        }


    }
}
