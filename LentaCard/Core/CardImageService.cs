using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using ZXing.SkiaSharp;
using ZXing.PDF417;
using ZXing;
using SkiaSharp;
using ZXing.Common;
using System.Text;


namespace LentaCard.Core
{
    public class CardImageService : ICardImageService
    {
        public byte[] GenerateCardImage(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length != 12)
            {
                throw new ArgumentException("A 12-digit code is required.");
            }

            var pdf417Barcode = GeneratePdf417Barcode(code);
            var templateImage = GenerateTemplateImage(code, pdf417Barcode);

            using (var ms = new MemoryStream())
            {
                templateImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private Bitmap GeneratePdf417Barcode(string code)
        {
            var barcodeWriter = new ZXing.SkiaSharp.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.PDF_417,
                Options = new ZXing.PDF417.PDF417EncodingOptions
                {
                    Width = 1580,
                    Height = 450
                }
            };

            var barcodeBitmap = barcodeWriter.Write(code);

            using (MemoryStream stream = new MemoryStream())
            {
                barcodeBitmap.Encode(stream, SkiaSharp.SKEncodedImageFormat.Png, 100);
                stream.Seek(0, SeekOrigin.Begin);
                return new Bitmap(stream);
            }
        }

        private Bitmap GenerateTemplateImage(string code, Bitmap barcode)
        {
            Bitmap template = new Bitmap("wwwroot/images/TemplateCard.jpg");
            using (Graphics graphics = Graphics.FromImage(template))
            {
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                barcode.RotateFlip(RotateFlipType.Rotate90FlipNone);
                graphics.DrawImage(barcode, new PointF(370, 480));

                Font cardFont = new Font("Qanelas Medium", 32);
                SolidBrush cardBrush = new SolidBrush(ColorTranslator.FromHtml("#003B95"));

                StringBuilder formattedCode = new StringBuilder();
                for (int i = 0; i < code.Length; i++)
                {
                    if (i % 3 == 0 && i != 0)
                    {
                        formattedCode.Append(' ');
                    }
                    formattedCode.Append(code[i]);
                }

                graphics.DrawString(formattedCode.ToString(), cardFont, cardBrush, new PointF(460, 2150));

                Font timeFont = new Font("Avenir Next Rounded Std Medium", 27);
                SolidBrush timeBrush = new SolidBrush(ColorTranslator.FromHtml("#E4ECF9"));

                DateTime utcNow = DateTime.UtcNow;
                TimeZoneInfo moscowTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time");
                DateTime moscowTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, moscowTimeZone);
                string currentTime = moscowTime.ToString("HH:mm");
                graphics.DrawString(currentTime, timeFont, timeBrush, new PointF(1034, 66));
            }
            return template;
        }
    }
}
