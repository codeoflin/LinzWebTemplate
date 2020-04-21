using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace LinzWebTemplate.Helper
{
    /// <summary>
    /// Image图像与byte数组之间的转换关系
    /// </summary>
    /// <value></value>
    public enum ImageFormat : byte
    {
        ///<summary>
        /// 作为输出的时候:根据其他参数决定格式
        /// 作为输入的时候:自动识别文件数据格式(支持:JPG,PNG,BMP等常见格式)
        ///</summary>
        FILE = 0,
        ///<summary>
        /// RAW_RGB24
        ///</summary>
        RAW_RGB24 = 1,
        ///<summary>
        /// RAW_ARGB32
        ///</summary>
        RAW_ARGB32 = 2,
        ///<summary>
        /// RAW_BGR24
        ///</summary>
        RAW_BGR24 = 3,
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ImageHelper
    {
        #region 图像与base64之间互转
        /// <summary>
        ///  将Base64字符串转换为图片
        /// </summary>
        /// <param name="base64">字串</param>
        /// <param name="format"></param>
        /// <param name="width">如果是RAW格式,需要填入宽高</param>
        /// <param name="height">如果是RAW格式,需要填入宽高</param>
        /// <returns>图片</returns>
        public static Image Base64ToImg(this string base64, ImageFormat format = ImageFormat.FILE, int width = 0, int height = 0)
        {
            var heads = new string[] { "png", "jgp", "jpg", "jpeg" };
            foreach (var head in heads) base64 = base64.Replace($"data:image/{head};base64,", "");
            var bytes = Convert.FromBase64String(base64);
            return bytes.ToImage(format, width, height);
        }

        /// <summary>
        /// 图片转base64字符串
        /// </summary>
        /// <param name="img"></param>
        /// <param name="format">如果为FILE,以fileformat决定格式</param>
        /// <param name="fileformat">默认为JPG格式</param>
        /// <returns></returns>
        public static string ImgToBase64(this Bitmap img, ImageFormat format = ImageFormat.FILE, System.Drawing.Imaging.ImageFormat fileformat = null)
        {
            return Convert.ToBase64String(img.ImgToBytes(format, fileformat));
        }

        /// <summary>
        /// 图片转base64字符串
        /// </summary>
        /// <param name="img"></param>
        /// <param name="format">如果为FILE,以fileformat决定格式</param>
        /// <param name="fileformat">默认为JPG格式</param>
        /// <returns></returns>
        public static string ImgToBase64(this Image img, ImageFormat format = ImageFormat.FILE, System.Drawing.Imaging.ImageFormat fileformat = null)
        {

            return Convert.ToBase64String(img.ImgToBytes(format, fileformat));
        }
        #endregion

        #region 图像与Bytes之间互转
        /// <summary>
        /// 图片转byte数组
        /// </summary>
        /// <param name="img"></param>
        /// <param name="format">如果为FILE,以fileformat决定格式</param>
        /// <param name="fileformat">默认为JPG格式</param>
        /// <returns></returns>
        public static byte[] ImgToBytes(this Image img, ImageFormat format = ImageFormat.FILE, System.Drawing.Imaging.ImageFormat fileformat = null)
        {
            return ImgToBytes((Bitmap)img, format, fileformat);
        }

        /// <summary>
        /// 图片转byte数组
        /// </summary>
        /// <param name="img"></param>
        /// <param name="format">如果为FILE,以fileformat决定格式</param>
        /// <param name="fileformat">默认为JPG格式</param>
        /// <returns></returns>
        public static byte[] ImgToBytes(this Bitmap img, ImageFormat format = ImageFormat.FILE, System.Drawing.Imaging.ImageFormat fileformat = null)
        {
            byte[] buff = null;
            switch (format)
            {
                case ImageFormat.FILE:
                    {
                        using (var ms = new MemoryStream())
                        {
                            img.Save(ms, fileformat == null ? System.Drawing.Imaging.ImageFormat.Jpeg : fileformat);
                            buff = ms.ToArray();
                        }
                        break;
                    }
                case ImageFormat.RAW_RGB24:
                    {
                        var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                        buff = new byte[img.Width * img.Height * 3];
                        Marshal.Copy(data.Scan0, buff, 0, buff.Length);
                        img.UnlockBits(data);
                        break;
                    }
                case ImageFormat.RAW_ARGB32:
                    {
                        var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                        buff = new byte[img.Width * img.Height * 4];
                        Marshal.Copy(data.Scan0, buff, 0, buff.Length);
                        img.UnlockBits(data);
                        break;
                    }
                case ImageFormat.RAW_BGR24:
                    {
                        var data = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                        buff = new byte[img.Width * img.Height * 3];
                        Marshal.Copy(data.Scan0, buff, 0, buff.Length);
                        for (int i = 0; i < buff.Length; i += 3)
                        {
                            byte tmp = buff[i];
                            buff[i] = buff[i + 2];
                            buff[i + 2] = tmp;
                        }
                        img.UnlockBits(data);
                        break;
                    }
                default: break;
            }
            return buff;
        }

        /// <summary>
        /// Byte数组转Image
        /// </summary>
        /// <param name="data"></param>
        /// <param name="format">如果数据格式为常见文件格式(JPG,BMP,PNG,...)无需改此参数,会自动识别.</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Image ToImage(this byte[] data, ImageFormat format = ImageFormat.FILE, int width = 0, int height = 0)
        {
            if (data == null) return null;
            Bitmap img = null;
            switch (format)
            {
                case ImageFormat.FILE:
                    {
                        using (var ms = new MemoryStream(data)) using (var tmp = Bitmap.FromStream(ms)) img = (Bitmap)tmp.DeepCopyBitmap();
                        break;
                    }
                case ImageFormat.RAW_RGB24:
                    {
                        img = new Bitmap(width, height);
                        var imglocker = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                        Marshal.Copy(data, 0, imglocker.Scan0, data.Length);
                        img.UnlockBits(imglocker);
                        break;
                    }
                case ImageFormat.RAW_ARGB32:
                    {
                        img = new Bitmap(width, height);
                        var imglocker = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                        Marshal.Copy(data, 0, imglocker.Scan0, data.Length);
                        img.UnlockBits(imglocker);
                        break;
                    }
                case ImageFormat.RAW_BGR24:
                    {
                        img = new Bitmap(width, height);
                        var imglocker = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                        var data2 = new byte[data.Length];
                        for (int i = 0; i < data.Length; i += 3)
                        {
                            data2[0] = data[2];
                            data2[1] = data[1];
                            data2[2] = data[0];
                        }
                        Marshal.Copy(data2, 0, imglocker.Scan0, data.Length);
                        img.UnlockBits(imglocker);
                        break;
                    }
                default: break;
            }
            return img;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="insteadtransparent">用来顶替透明区的颜色,默认为null则不顶替</param>
        /// <returns></returns>
        public static Bitmap DeepCopyBitmap(this Bitmap bitmap, Brush insteadtransparent = null)
        {
            return (Bitmap)DeepCopyBitmap((Image)bitmap, insteadtransparent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="insteadtransparent">用来顶替透明区的颜色,默认为null则不顶替</param>
        /// <returns></returns>
        public static Image DeepCopyBitmap(this Image bitmap, Brush insteadtransparent = null)
        {
            try
            {
                var img = new Bitmap(bitmap.Width, bitmap.Height);
                var g = Graphics.FromImage(img);
                if (insteadtransparent != null) g.FillRectangle(insteadtransparent, 0, 0, img.Width, img.Height);
                g.DrawImage(bitmap, 0, 0);
                g.Flush();
                g.Dispose();
                return img;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 图片剪切
        /// </summary>
        /// <param name="img"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Bitmap Cut(this Image img, Rectangle rect)
        {
            return Cut((Bitmap)img, rect);
        }

        /// <summary>
        /// 图片剪切
        /// </summary>
        /// <param name="b"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Bitmap Cut(this Bitmap b, Rectangle rect)
        {
            if (b == null) return null;
            int w = b.Width;
            int h = b.Height;
            if (rect.Left >= w || rect.Top >= h) return null;
            //if (rect.Left + rect.Width > w) iWidth = w - StartX;
            //if (StartY + iHeight > h) iHeight = h - StartY;
            try
            {
                var imgout = new Bitmap(rect.Width, rect.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(imgout)) g.DrawImage(b, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
                return imgout;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 拉伸图片
        /// </summary>
        /// <param name="img"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Bitmap Resize(this Bitmap img, System.Drawing.Size size)
        {
            var newimg = new Bitmap(size.Width, size.Height);
            using (var gp = Graphics.FromImage(newimg))
            {
                gp.DrawImage(img, new Rectangle(0, 0, size.Width, size.Height), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
                gp.Flush();
            }
            return newimg;
        }

        /// <summary>
        /// 拉伸图片
        /// </summary>
        /// <param name="img"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Bitmap Resize(this Image img, System.Drawing.Size size)
        {
            return Resize((Bitmap)img, size);
        }

        /// <summary>
        /// 可以对图像进行简单的处理
        /// </summary>
        /// <param name="img"></param>
        /// <param name="callback"></param>
        public static void Modify(this Image img, Action<Graphics> callback)
        {
            var g = Graphics.FromImage(img);
            callback(g);
            g.Flush();
            g.Dispose();
        }

    }//End Class
}