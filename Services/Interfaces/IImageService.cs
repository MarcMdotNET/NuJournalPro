using NuJournalPro.Models.Database;
using NuJournalPro.Models.Media;

namespace NuJournalPro.Services.Interfaces
{
    public interface IImageService
    {
        Task<CompressedImage?> CreateCompressedImageAsync(IFormFile file);
        Task<CompressedImage?> CreateCompressedImageAsync(string fileName, string filePath);
        Task<byte[]?> EncodeImageDataAsync(IFormFile file, bool? compress = null);
        Task<byte[]?> EncodeImageDataAsync(string fileName, string filePath, bool? compress = null);
        string? DecodeImage(byte[] imageData, string mimeType, bool? decompress = null);
        byte[]? CompressDecodedImage(string decodedImage);
        string? DecompressDecodedImage(byte[] compressedDecodedImage);
        string CompressDecodedImageB64(string decodedImage);
        string DecompressDecodedImageB64(string compressedDecodedImageB64);
        string? GetImageMimeType(IFormFile file);
        string? GetImageMimeType(string fileName, string filePath);
        int GetImageSize(IFormFile file);
        int GetImageSize(string fileName, string filePath);
        byte[]? CompressImageData(byte[] uncompressedImageData);
        byte[]? DecompressImageData(byte[] compressedImageData);
    }
}