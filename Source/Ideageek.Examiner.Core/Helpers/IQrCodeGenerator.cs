namespace Ideageek.Examiner.Core.Helpers;

public interface IQrCodeGenerator
{
    string GenerateBase64(string payload);
}
