using Ideageek.Examiner.Core.Helpers;
using QRCoder;

namespace Ideageek.Examiner.Api.Helpers;

public class QrCodeGenerator : IQrCodeGenerator
{
    public string GenerateBase64(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(qrData);
        var bytes = png.GetGraphic(20);
        return Convert.ToBase64String(bytes);
    }
}
