using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.Advanced;
using PdfSharpCore.Pdf.IO;

namespace PdfCompression.pdf
{
    public class PdfHelper
    {
        #region 页面中的所有jpg图片压缩
        public static void Convert(string path, string target, Action<float> callBack)
        {
            PdfDocument document = PdfReader.Open(path);
            document.Options.UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages.Always;
            int imageCount = 0;
            // Iterate pages
            var pageCount = document.Pages.Count;
            for (var i = 0; i < pageCount; i++)
            {
                callBack?.Invoke((i + 1)* 1f / pageCount);
                var page = document.Pages[i];
                //if (page.Width.Value > page.Height.Value)
                //    page.Orientation = PdfSharpCore.PageOrientation.Portrait;
                //page.Size = PdfSharpCore.PageSize.A4;
                // Get resources dictionary
                PdfDictionary resources = page.Elements.GetDictionary("/Resources");
                if (resources != null)
                {
                    // Get external objects dictionary
                    PdfDictionary xObjects = resources.Elements.GetDictionary("/XObject");
                    if (xObjects != null)
                    {
                        ICollection<PdfItem> items = xObjects.Elements.Values;
                        // Iterate references to external objects
                        foreach (PdfItem item in items)
                        {
                            PdfReference reference = item as PdfReference;
                            if (reference != null)
                            {
                                PdfDictionary xObject = reference.Value as PdfDictionary;
                                // Is external object an image?
                                if (xObject != null && xObject.Elements.GetString("/Subtype") == "/Image")
                                {
                                    ExportImage(xObject, ref imageCount);
                                }
                            }
                        }
                    }
                }
            }

            document.Save(target);
        }

        static void ExportImage(PdfDictionary image, ref int count)
        {
            string filter = image.Elements.GetName("/Filter");
            switch (filter)
            {
                case "/DCTDecode":
                    ExportJpegImage(image, ref count);
                    break;

                case "/FlateDecode":
                    ExportAsPngImage(image, ref count);
                    break;
            }
        }

        static void ExportJpegImage(PdfDictionary image, ref int count)
        {
            // Fortunately JPEG has native support in PDF and exporting an image is just writing the stream to a file.
            byte[] stream = image.Stream.Value;
            using (var newImage = new MemoryStream())
            {
                var old = new MemoryStream(stream);
                old.Seek(0, SeekOrigin.Begin);
                int width = image.Elements.GetInteger(PdfImage.Keys.Width);
                ImageHelper.Compress(old, newImage, width, 40);
                image.Stream.Value = newImage.ToArray();
            }

        }

        static void ExportAsPngImage(PdfDictionary image, ref int count)
        {
            int width = image.Elements.GetInteger(PdfImage.Keys.Width);
            int height = image.Elements.GetInteger(PdfImage.Keys.Height);
            int bitsPerComponent = image.Elements.GetInteger(PdfImage.Keys.BitsPerComponent);

            // TODO: You can put the code here that converts vom PDF internal image format to a Windows bitmap
            // and use GDI+ to save it in PNG format.
            // It is the work of a day or two for the most important formats. Take a look at the file
            // PdfSharpCore.Pdf.Advanced/PdfImage.cs to see how we create the PDF image formats.
            // We don't need that feature at the moment and therefore will not implement it.
            // If you write the code for exporting images I would be pleased to publish it in a future release
            // of PdfSharpCore.
        }
        #endregion

        /// <summary>
        /// 整页存为图片压缩
        /// </summary>
        /// <param name="path"></param>
        /// <param name="target"></param>
        /// <param name="callBack"></param>
        public static void Convert2(string path, string target, Action<float> callBack)
        {
            var fileName = Path.GetFileName(path);
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var pdf2image = new Pdf2Image(stream))
            using (var newPdf = new PdfDocument())
            {
                newPdf.Options.UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages.Always;
                PdfDocument document = PdfReader.Open(stream);
                var pageCount = document.PageCount;
                for (var i = 0; i < pageCount; i++)
                {
                    callBack?.Invoke((i + 1) * 1f / pageCount);
                    var page = document.Pages[i];
                    var newPage = newPdf.AddPage();
                    newPage.Size = PdfSharpCore.PageSize.A4;
                    if (page.Width.Value > page.Height.Value)
                        newPage.Orientation = PdfSharpCore.PageOrientation.Portrait;
                    var gfx = XGraphics.FromPdfPage(newPage);
                    using (var tStream = pdf2image.ToPng(i))
                    using (var outStream = new MemoryStream())
                    {
                        ImageHelper.Compress2(tStream, outStream, (decimal)newPage.Width.Value, 40);
                        var img = XImage.FromStream(() => outStream);
                        var width = img.PointWidth;
                        var height = img.PointHeight;
                        var x = width > newPage.Width.Value ? 0 : (newPage.Width.Value - width) / 2;
                        var y = height > newPage.Height.Value ? 0 : (newPage.Height.Value - height) / 2;

                        newPage.Width = width;
                        newPage.Height = height;

                        gfx.DrawImage(img, new XPoint(0, 0));
                    }
                    //break;
                }
                newPdf.Save(target);
            }
        }


        /// <summary>
        /// 调整页面为A4尺寸大小
        /// </summary>
        /// <param name="path"></param>
        /// <param name="target"></param>
        /// <param name="callBack"></param>
        public static void Convert2A4(string path, string target, Action<float> callBack)
        {
            using (var newPdf = new PdfDocument())
            using (XPdfForm form = XPdfForm.FromFile(path))
            {
                newPdf.Options.UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages.Always;
                var pageCount = form.PageCount;
                for (var i = 0; i < pageCount; i++)
                {
                    callBack?.Invoke((i + 1) * 1f / pageCount);
                    form.PageNumber = i + 1;

                    var page = newPdf.AddPage();
                    page.Size = PageSize.A4;
                    if (form.PixelHeight > form.PixelWidth)
                    {
                        page.Orientation = PageOrientation.Landscape;
                    }
                    var gfx = XGraphics.FromPdfPage(page);
                    var width = form.PixelWidth;
                    var height = form.PixelHeight;

                    if (width > page.Width)
                    {
                        var scale = page.Width / width;
                        var hScale = page.Height / height;
                        if (hScale < scale) scale = hScale;
                        width = (int)(width * scale);
                        height = (int)(height * scale);
                    }

                    page.Width = width;
                    page.Height = height;

                    gfx.DrawImage(form, new XRect(0, 0, width, height));

                    gfx.Save();
                }

                newPdf.Save(target);
            }
        }
    }

}

