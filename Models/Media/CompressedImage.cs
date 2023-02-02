using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Compression;

namespace NuJournalPro.Models.Media
{
    public class CompressedImage
    {
        public byte[]? CompressedImageData { get; set; }
        public string? ImageMimeType { get; set; }

        [NotMapped]
        public byte[]? ImageData
        {
            get
            {
                if (CompressedImageData != null)
                {
                    return DecompressImageData(CompressedImageData);
                }
                else
                {
                    return null;
                }                    
            }
        }

        [NotMapped]
        public string? DecodedImage
        {
            get
            {
                if (ImageData != null && ImageMimeType != null)
                {
                    return $"data:{ImageMimeType};base64,{Convert.ToBase64String(ImageData)}";
                }
                else
                {
                    return null;
                }
            }
        }
        
        public int ImageSize { get; set; }

        [NotMapped]
        public int CompressedImageSize
        {
            get
            {
                if (CompressedImageData != null)
                {
                    return CompressedImageData.Length;
                }
                else
                {
                    return 0;
                }
            }
        }

        private byte[] DecompressImageData(byte[] compressedImageData)
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
