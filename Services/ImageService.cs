using NuJournalPro.Models.Media;
using NuJournalPro.Services.Interfaces;
using System.IO.Compression;

namespace NuJournalPro.Services
{
    public class ImageService : IImageService
    {
        public async Task<CompressedImage?> CreateCompressedImageAsync(IFormFile file)
        {
            if (file is null) return null;
            else
            {
                return new CompressedImage()
                {
                    CompressedImageData = await EncodeImageDataAsync(file, true),
                    ImageMimeType = GetImageMimeType(file),
                    ImageSize = GetImageSize(file)
                };
            }
        }

        public async Task<CompressedImage?> CreateCompressedImageAsync(string fileName)
        {
            if (fileName is null) return null;
            else
            {
                return new CompressedImage()
                {
                    CompressedImageData = await EncodeImageDataAsync(fileName, true),
                    ImageMimeType = GetImageMimeType(fileName),
                    ImageSize = GetImageSize(fileName)
                };
            }
        }

        public async Task<byte[]?> EncodeImageDataAsync(IFormFile file, bool? compress = null)
        {
            if (file is null) return null;
            else
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                if (compress is not null && compress is true) return CompressImageData(memoryStream.ToArray());
                else return memoryStream.ToArray();
            }
        }

        public async Task<byte[]?> EncodeImageDataAsync(string fileName, bool? compress = null)
        {
            if (fileName is null) return null;
            else
            {
                var file = $"{Directory.GetCurrentDirectory()}/wwwresources/images/{fileName}";
                var fileContents = await File.ReadAllBytesAsync(file);
                if (compress is not null && compress is true) return CompressImageData(fileContents);
                else return fileContents;
            }
        }

        public string? DecodeImage(byte[]? imageData, string? mimeType, bool? decompress = null)
        {
            if (imageData is null || mimeType is null) return null;
            else
            {
                if (decompress is not null && decompress is true) return $"data:{mimeType};base64,{Convert.ToBase64String(DecompressImageData(imageData))}";
                else return $"data:{mimeType};base64,{Convert.ToBase64String(imageData)}";
            }
        }

        public byte[]? CompressDecodedImage(string decodedImage)
        {
            if (decodedImage is null) return null;
            else
            {
                var imageData = Convert.FromBase64String(decodedImage);
                return CompressImageData(imageData);
            }
        }

        public string? DecompressDecodedImage(string decodedImage)
        {
            if (decodedImage is null) return null;
            else
            {
                var imageData = Convert.FromBase64String(decodedImage);
                return Convert.ToBase64String(DecompressImageData(imageData));
            }
        }

        public int GetImageSize(IFormFile file)
        {
            if (file is null) return 0;
            else return Convert.ToInt32(file?.Length);
        }

        public int GetImageSize(string fileName)
        {
            if (fileName is null) return 0;
            else return Convert.ToInt32(new FileInfo($"{Directory.GetCurrentDirectory()}/wwwresources/images/{fileName}").Length);
        }

        public string? GetImageMimeType(IFormFile file)
        {
            if (file is null) return null;
            else return file.ContentType;
        }

        public string? GetImageMimeType(string fileName)
        {
            if (fileName is null) return null;
            else
            {
                var file = $"{Directory.GetCurrentDirectory()}/wwwresources/images/{fileName}";
                var fileExtension = Path.GetExtension(file);
                if (fileExtension == string.Empty)
                {
                    return null;
                }
                else
                {
                    if (fileExtension == ".svg")
                    {
                        return "image/svg+xml";
                    }
                    else if (fileExtension == ".png")
                    {
                        return "image/png";
                    }
                    else if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                    {
                        return "image/jpeg";
                    }
                    else if (fileExtension == ".gif")
                    {
                        return "image/gif";
                    }
                    else if (fileExtension == ".bmp")
                    {
                        return "image/bmp";
                    }
                    else return null;
                }
            }
        }

        public byte[] CompressImageData(byte[] uncompressedImageData)
        {
            using (MemoryStream input = new MemoryStream(uncompressedImageData))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (DeflateStream deflate = new DeflateStream(output, CompressionLevel.Optimal))
                    {
                        input.CopyTo(deflate);
                    }
                    return output.ToArray();
                }
            }
        }

        public byte[] DecompressImageData(byte[] compressedImageData)
        {
            using (MemoryStream input = new MemoryStream(compressedImageData))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (DeflateStream deflate = new DeflateStream(input, CompressionMode.Decompress))
                    {
                        deflate.CopyTo(output);
                    }
                    return output.ToArray();
                }
            }
        }
    }
}
