//using PhotoSauce.MagicScaler;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;

namespace PdfCompression.pdf
{
    public static class ImageHelper
    {
        /// <summary>
        /// 有损压缩图片
        /// </summary>
        /// <param name="source">原文件位置</param>
        /// <param name="target">生成目标文件位置</param>
        /// <param name="maxWidth">最大宽度，根据此宽度计算是否需要缩放，计算新高度</param>
        /// <param name="quality">图片质量，范围0-100</param>
        public static void Compress(Stream file, Stream target, decimal maxWidth, int quality)
        {
            using (var fileStream = new SKManagedStream(file))
            using (var bitmap = SKBitmap.Decode(fileStream))
            {
                var width = (decimal)bitmap.Width;
                var height = (decimal)bitmap.Height;
                var newWidth = width;
                var newHeight = height;
                if (width > maxWidth)
                {
                    newWidth = maxWidth;
                    newHeight = height / width * maxWidth;
                }
                using (var resized = bitmap.Resize(new SKImageInfo((int)newWidth, (int)newHeight), SKFilterQuality.Medium))
                {
                    if (resized != null)
                    {
                        using (var image = SKImage.FromBitmap(resized))
                            image.Encode(SKEncodedImageFormat.Jpeg, quality).SaveTo(target);
                    }
                }
            }
        }
        /// <summary>
        /// 无损压缩图片
        /// </summary>
        /// <param name="file"></param>
        /// <param name="target"></param>
        /// <param name="maxWidth"></param>
        /// <param name="quality"></param>
        public static void Compress2(Stream file, Stream target, decimal maxWidth, int quality)
        {
            using var image = Image.Load<Rgba32>(file);
            {
                image.Mutate(x => x.Resize((int)maxWidth, 0));
                image.SaveAsPng(target, new PngEncoder { CompressionLevel = PngCompressionLevel.Level4 });
            }
            target.Seek(0, SeekOrigin.Begin);
        }


        /// <summary>
        /// 有损压缩图片
        /// </summary>
        /// <param name="file"></param>
        /// <param name="target"></param>
        /// <param name="maxWidth"></param>
        /// <param name="quality"></param>
        public static void Compress3(Stream file, Stream target, decimal maxWidth, int quality)
        {
            using var image = Image.Load<Rgba32>(file);
            {
                image.Mutate(x => x.Resize((int)maxWidth, 0));
                image.SaveAsJpeg(target, new JpegEncoder { Quality = quality });
            }
            target.Seek(0, SeekOrigin.Begin);

        }
    }
}