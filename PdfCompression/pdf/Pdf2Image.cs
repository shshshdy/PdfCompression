using PDFtoImage.PdfiumViewer;
using SkiaSharp;
using System;
using System.IO;
using static PdfSharpCore.Pdf.PdfDictionary;

namespace PdfCompression.pdf
{
    public class Pdf2Image : IDisposable
    {
        PdfDocument _pdf;
        public Pdf2Image(Stream pdf)
        {
            _pdf = PdfDocument.Load(pdf, null, false);
        }
        public Stream ToPng(int page)
        {
            return Conver(page, true, 40);
        }

        public Stream ToJpeg(int page, int quality)
        {
            return Conver(page, false, quality);
        }

        private Stream Conver(int page, bool isPng, int quality)
        {
            var size = _pdf.PageSizes[page];
            var bitmap = _pdf.Render(page, (int)size.Width, (int)size.Height, 300, 300, PdfRenderFlags.None, true);
            var img = new MemoryStream();
            using (var image = SKImage.FromBitmap(bitmap))
                image.Encode(isPng ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg, quality).SaveTo(img);
            img.Seek(0, SeekOrigin.Begin);

            return img;
        }

        public void Dispose()
        {
            _pdf.Dispose();
        }
    }

}

