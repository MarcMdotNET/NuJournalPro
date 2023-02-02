using NuJournalPro.Models.Database;
using NuJournalPro.Models.Media;
using NuJournalPro.Services.Interfaces;
using System.IO.Compression;
using System.Text;

namespace NuJournalPro.Services
{
    public class ImageService : IImageService
    {
        public async Task<CompressedImage?> CreateCompressedImageAsync(IFormFile file)
        {
            if (file == null)
            {
                return null;
            }                
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

        public async Task<CompressedImage?> CreateCompressedImageAsync(string fileName, string filePath)
        {
            if (fileName == null || filePath == null)
            {
                return null;
            }            
            else
            {                
                if (File.Exists($"{Directory.GetCurrentDirectory()}{filePath}{fileName}")) {
                    return new CompressedImage()
                    {
                        CompressedImageData = await EncodeImageDataAsync(fileName, filePath, true),
                        ImageMimeType = GetImageMimeType(fileName, filePath),
                        ImageSize = GetImageSize(fileName, filePath)
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<byte[]?> EncodeImageDataAsync(IFormFile file, bool? compress = null)
        {
            if (file == null)
            {
                return null;
            }
            else
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                if (compress != null && compress == true)
                {
                    return CompressImageData(memoryStream.ToArray());
                }
                else
                {
                    return memoryStream.ToArray();
                }
            }
        }

        public async Task<byte[]?> EncodeImageDataAsync(string fileName, string filePath, bool? compress = null)
        {
            if (fileName == null || filePath == null)
            {
                return null;
            }
            else
            {
                var file = $"{Directory.GetCurrentDirectory()}{filePath}{fileName}";

                if (File.Exists(file))
                {
                    var fileContents = await File.ReadAllBytesAsync(file);
                    if (compress != null && compress == true)
                    {
                        return CompressImageData(fileContents);
                    }
                    else
                    {
                        return fileContents;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public string? DecodeImage(byte[]? imageData, string? mimeType, bool? decompress = null)
        {
            if (imageData == null || mimeType == null)
            {
                return null;
            }
            else
            {
                if (decompress != null && decompress == true)
                {
                    return $"data:{mimeType};base64,{Convert.ToBase64String(DecompressImageData(imageData))}";
                }
                else
                {
                    return $"data:{mimeType};base64,{Convert.ToBase64String(imageData)}";
                }
            }
        }

        public byte[] CompressDecodedImage(string decodedImage)
        {
            if (decodedImage == null)
            {
                throw new ArgumentNullException(nameof(decodedImage));
            }
            else
            {                
                var byteData = Encoding.ASCII.GetBytes(decodedImage);
                return CompressImageData(byteData);
            }
        }

        public string DecompressDecodedImage(byte[] compressedDecodedImage)
        {
            if (compressedDecodedImage == null)
            {
                throw new ArgumentNullException(nameof(compressedDecodedImage));
            }
            else
            {
                var byteData = DecompressImageData(compressedDecodedImage);
                return Encoding.ASCII.GetString(byteData);
            }
        }

        public string CompressDecodedImageB64(string decodedImage)
        {
            if (decodedImage == null)
            {
                throw new ArgumentNullException(nameof(decodedImage));
            }
            else
            {
                return Convert.ToBase64String(CompressDecodedImage(decodedImage));
            }
        }

        public string DecompressDecodedImageB64(string compressedDecodedImageB64)
        {
            if (compressedDecodedImageB64 == null)
            {
                throw new ArgumentNullException(nameof(compressedDecodedImageB64));
            }
            else
            {
                return DecompressDecodedImage(Convert.FromBase64String(compressedDecodedImageB64));
            }
        }

        public int GetImageSize(IFormFile file)
        {
            if (file == null)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(file?.Length);
            }
        }

        public int GetImageSize(string fileName, string filePath)
        {
            if (fileName == null || filePath == null)
            {
                return 0;
            }
            else
            {
                if (File.Exists($"{Directory.GetCurrentDirectory()}{filePath}{fileName}"))
                {
                    return Convert.ToInt32(new FileInfo($"{Directory.GetCurrentDirectory()}{filePath}{fileName}").Length);
                }
                else
                {
                    return 0;
                }                
            }
        }

        public string? GetImageMimeType(IFormFile file)
        {
            if (file == null)
            {
                return null;
            }
            else
            {
                return file.ContentType;
            }
        }

        public string? GetImageMimeType(string fileName, string filePath)
        {
            if (fileName == null || filePath == null)
            {
                return null;
            }
            else
            {
                var file = $"{Directory.GetCurrentDirectory()}{filePath}{fileName}";
                
                if (File.Exists(file))
                {
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
                else
                {
                    return null; 
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
