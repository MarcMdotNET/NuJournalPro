using NuJournalPro.Models.Media;

namespace NuJournalPro.Services.Interfaces
{
    public interface IImageService
    {
        Task<CompressedImage?> CreateCompressedImageAsync(IFormFile file);
        Task<CompressedImage?> CreateCompressedImageAsync(string fileName, string? alternatePath = null);
        Task<byte[]?> EncodeImageDataAsync(IFormFile file, bool? compress = null);
        Task<byte[]?> EncodeImageDataAsync(string fileName, bool? compress = null, string? alternatePath = null);
        string? DecodeImage(byte[] imageData, string mimeType, bool? decompress = null);
        byte[]? CompressDecodedImage(string decodedImage);
        string? DecompressDecodedImage(string decodedImage);
        string? GetImageMimeType(IFormFile file);
        string? GetImageMimeType(string fileName, string? alternatePath = null);
        int GetImageSize(IFormFile file);
        int GetImageSize(string fileName, string? alternatePath = null);
        byte[]? CompressImageData(byte[] uncompressedImageData);
        byte[]? DecompressImageData(byte[] compressedImageData);
    }
}