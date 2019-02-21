using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;
using ZXing.Rendering;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 二维码生成帮助类
    /// </summary>
    public class QRCodeHelper
    {
        #region 生成二维码

        /// <summary>
        /// 生成二维码，默认边长为250px
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <returns></returns>
        public static Image BuildQRCode(string content)
        {
            return BuildQRCode(content, 250, Color.White, Color.Black);
        }

        /// <summary>
        /// 生成二维码,自定义边长
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="imgSize">二维码边长px</param>
        /// <returns></returns>
        public static Image BuildQRCode(string content, int imgSize)
        {
            return BuildQRCode(content, imgSize, Color.White, Color.Black);
        }

        /// <summary>
        /// 生成二维码
        /// 注：自定义边长以及颜色
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="imgSize">二维码边长px</param>
        /// <param name="background">二维码底色</param>
        /// <param name="foreground">二维码前景色</param>
        /// <returns></returns>
        public static Image BuildQRCode(string content, int imgSize, Color background, Color foreground)
        {
            BarcodeWriter writer = new BarcodeWriter
            {
                Renderer = new BitmapRenderer { Background = background, Foreground = foreground },
                Format = BarcodeFormat.QR_CODE
            };
            writer.Options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            writer.Options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);

            writer.Options.Height = writer.Options.Width = imgSize;
            writer.Options.Margin = 0;
            Bitmap img = writer.Write(content);
            return img;
        }

        /// <summary>
        /// 生成二维码并添加Logo
        /// 注：默认生成边长为250px的二维码
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="logo">logo图片</param>
        /// <returns></returns>
        public static Image BuildQRCode_Logo(string content, Image logo)
        {
            return BuildQRCode_Logo(content, 250, Color.White, Color.Black, logo);
        }

        /// <summary>
        /// 生成二维码并添加Logo
        /// 注：自定义边长
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="imgSize">二维码边长px</param>
        /// <param name="logo">logo图片</param>
        /// <returns></returns>
        public static Image BuildQRCode_Logo(string content, int imgSize, Image logo)
        {
            return BuildQRCode_Logo(content, imgSize, Color.White, Color.Black, logo);
        }

        /// <summary>
        /// 生成二维码并添加Logo
        /// 注：自定义边长以及颜色
        /// </summary>
        /// <param name="content">二维码内容</param>
        /// <param name="imgSize">二维码边长px</param>
        /// <param name="background">二维码底色</param>
        /// <param name="foreground">二维码前景色</param>
        /// <param name="logo">logo图片</param>
        /// <returns></returns>
        public static Image BuildQRCode_Logo(string content, int imgSize, Color background, Color foreground, Image logo)
        {
            //构造二维码写码器
            MultiFormatWriter writer = new MultiFormatWriter();
            Dictionary<EncodeHintType, object> hint = new Dictionary<EncodeHintType, object>
            {
                { EncodeHintType.CHARACTER_SET, "UTF-8" },
                { EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H }
            };

            //生成二维码 
            BitMatrix bm = writer.encode(content, BarcodeFormat.QR_CODE, imgSize, imgSize, hint);
            BarcodeWriter barcodeWriter = new BarcodeWriter()
            {
                Renderer = new BitmapRenderer { Background = background, Foreground = foreground },
                Format = BarcodeFormat.QR_CODE
            };
            Bitmap map = barcodeWriter.Write(bm);

            //获取二维码实际尺寸（去掉二维码两边空白后的实际尺寸）
            int[] rectangle = bm.getEnclosingRectangle();

            //计算插入图片的大小和位置
            int middleW = Math.Min((int)(rectangle[2] / 3.5), logo.Width);
            int middleH = Math.Min((int)(rectangle[3] / 3.5), logo.Height);
            int middleL = (map.Width - middleW) / 2;
            int middleT = (map.Height - middleH) / 2;

            //将img转换成bmp格式，否则后面无法创建Graphics对象
            Bitmap bmpimg = new Bitmap(map.Width, map.Height, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmpimg))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.DrawImage(map, 0, 0);
            }
            //将二维码插入图片
            Graphics myGraphic = Graphics.FromImage(bmpimg);
            //白底
            myGraphic.FillRectangle(Brushes.White, middleL, middleT, middleW, middleH);
            myGraphic.DrawImage(logo, middleL, middleT, middleW, middleH);

            return bmpimg;
        }

        #endregion

        #region 生成条形码

        /// <summary>
        /// 生成条形码
        /// 注：默认宽150px,高50px
        /// </summary>
        /// <param name="content">条形码内容</param>
        /// <returns></returns>
        public static Image BuildBarCode(string content)
        {
            return BuildBarCode(content, 150, 50);
        }

        /// <summary>
        /// 生成条形码
        /// 注：自定义尺寸
        /// </summary>
        /// <param name="content">条形码内容</param>
        /// <param name="width">宽度px</param>
        /// <param name="height">高度px</param>
        /// <returns></returns>
        public static Image BuildBarCode(string content, int width, int height)
        {
            BarcodeWriter writer = new BarcodeWriter
            {
                //使用ITF 格式，不能被现在常用的支付宝、微信扫出来
                //如果想生成可识别的可以使用 CODE_128 格式
                //writer.Format = BarcodeFormat.ITF;
                Format = BarcodeFormat.CODE_128
            };
            EncodingOptions options = new EncodingOptions()
            {
                Width = width,
                Height = height,
                Margin = 2
            };
            writer.Options = options;
            Bitmap map = writer.Write(content);

            return map;
        }

        #endregion

        #region 读取码内容

        /// <summary>
        /// 从二维码读取内容
        /// </summary>
        /// <param name="image">二维码</param>
        /// <returns></returns>
        public static string ReadContent(Bitmap image)
        {
            BarcodeReader reader = new BarcodeReader();
            reader.Options.CharacterSet = "UTF-8";
            Result result = reader.Decode(image);
            return result == null ? "" : result.Text;
        }

        #endregion
    }
}
