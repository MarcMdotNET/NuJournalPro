using NuJournalPro.Models.Media;

namespace NuJournalPro.Services.Interfaces
{
    public interface IImageService
    {
        Task<CompressedImage?> CreateCompressedImageAsync(IFormFile file);
        Task<CompressedImage?> CreateCompressedImageAsync(string fileName);
        Task<byte[]?> EncodeImageDataAsync(IFormFile file, bool? compress = null);
        Task<byte[]?> EncodeImageDataAsync(string fileName, bool? compress = null);
        string? DecodeImage(byte[] imageData, string mimeType, bool? decompress = null);
        byte[]? CompressDecodedImage(string decodedImage);
        string? DecompressDecodedImage(string decodedImage);
        string? GetImageMimeType(IFormFile file);
        string? GetImageMimeType(string fileName);
        int GetImageSize(IFormFile file);
        int GetImageSize(string fileName);
        byte[]? CompressImageData(byte[] uncompressedImageData);
        byte[]? DecompressImageData(byte[] compressedImageData);
    }
}