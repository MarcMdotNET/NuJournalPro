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
                if (CompressedImageData is not null)
                {
                    return DecompressImageData(CompressedImageData);
                }
                else return null;
            }
        }

        [NotMapped]
        public string? DecodedImage
        {
            get
            {
                if (ImageData is not null && ImageMimeType is not null)
                {
                    return $"data:{ImageMimeType};base64,{Convert.ToBase64String(ImageData)}";
                }
                else return null;
            }
        }
        public int ImageSize { get; set; }

        [NotMapped]
        public int CompressedImageSize
        {
            get
            {
                if (CompressedImageData is not null && ImageMimeType is not null) return CompressedImageData.Length + ImageMimeType.ToArray().Length;
                else return 0;
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
