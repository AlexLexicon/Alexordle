using System.Text;
using System.Web;

namespace Alexordle.Client.Application.Services;
public interface IEncodingService
{
    Task<bool> IsEncodable(string text);
    Task<string> UrlEncodeAsync(string text);
    Task<string> UrlDecodeAsync(string url);
    Task<string> Base64EncodeAsync(byte[] bytes);
    Task<byte[]> Base64DecodeAsync(string base64);
    Task<byte[]> ByteEncodeAsync(string text);
    Task<string> ByteDecodeAsync(byte[] bytes);
}
public class EncodingService : IEncodingService
{
    public async Task<bool> IsEncodable(string text)
    {
        try
        {
            await ByteEncodeAsync(text);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<string> UrlEncodeAsync(string text)
    {
        string url = HttpUtility.UrlEncode(text);

        return Task.FromResult(url);
    }

    public Task<string> UrlDecodeAsync(string url)
    {
        string text = HttpUtility.UrlDecode(url);

        return Task.FromResult(text);
    }

    public Task<string> Base64EncodeAsync(byte[] bytes)
    {
        string base64 = Convert.ToBase64String(bytes);

        return Task.FromResult(base64);
    }

    public Task<byte[]> Base64DecodeAsync(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);

        return Task.FromResult(bytes);
    }

    public Task<byte[]> ByteEncodeAsync(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);

        return Task.FromResult(bytes);
    }

    public Task<string> ByteDecodeAsync(byte[] bytes)
    {
        string text = Encoding.UTF8.GetString(bytes);

        return Task.FromResult(text);
    }
}
