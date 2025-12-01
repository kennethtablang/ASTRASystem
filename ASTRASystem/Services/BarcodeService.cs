using ASTRASystem.Interfaces;
using QRCoder;
using SkiaSharp;

namespace ASTRASystem.Services
{
    public class BarcodeService : IBarcodeService
    {
        private readonly ILogger<BarcodeService> _logger;

        public BarcodeService(ILogger<BarcodeService> logger)
        {
            _logger = logger;
        }

        public byte[] GenerateQRCode(string content, int width = 300, int height = 300)
        {
            try
            {
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);

                // Resize to requested dimensions using SkiaSharp
                using var inputStream = new MemoryStream(qrCodeImage);
                using var inputBitmap = SKBitmap.Decode(inputStream);

                var imageInfo = new SKImageInfo(width, height);
                using var resizedBitmap = inputBitmap.Resize(imageInfo, SKFilterQuality.High);

                if (resizedBitmap == null)
                {
                    // If resize fails, return original
                    return qrCodeImage;
                }

                using var image = SKImage.FromBitmap(resizedBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                return data.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                throw;
            }
        }

        public byte[] GenerateBarcode(string content, int width = 300, int height = 100)
        {
            try
            {
                // Create a simple barcode visualization
                var imageInfo = new SKImageInfo(width, height);
                using var surface = SKSurface.Create(imageInfo);
                var canvas = surface.Canvas;

                // White background
                canvas.Clear(SKColors.White);

                // Draw barcode bars (simplified representation)
                var paint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                // Simple barcode pattern based on content
                var barCount = Math.Min(content.Length * 6, 50);
                var barWidth = (float)width / barCount;

                for (int i = 0; i < barCount; i++)
                {
                    // Alternate black/white bars based on content hash
                    if ((content.GetHashCode() + i) % 2 == 0)
                    {
                        var rect = new SKRect(
                            i * barWidth,
                            10,
                            (i + 1) * barWidth,
                            height - 30);
                        canvas.DrawRect(rect, paint);
                    }
                }

                // Draw text below barcode
                var textPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextSize = 14,
                    TextAlign = SKTextAlign.Center,
                    Typeface = SKTypeface.FromFamilyName("Arial")
                };

                canvas.DrawText(content, width / 2, height - 8, textPaint);

                // Convert to byte array
                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                return data.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating barcode");
                throw;
            }
        }
    }
}
