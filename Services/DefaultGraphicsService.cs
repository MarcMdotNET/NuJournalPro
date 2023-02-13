using Microsoft.Extensions.Options;
using NuJournalPro.Models.Settings;
using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Services
{
    public class DefaultGraphicsService : IDefaultGraphicsService
    {
        private readonly IImageService _imageService;
        private readonly DefaultGraphics _defaultGraphics;

        public DefaultGraphicsService(IImageService imageService,
                                      IOptions<DefaultGraphics> defaultGraphics)
        {
            _imageService = imageService;
            _defaultGraphics = defaultGraphics.Value;
        }

        public async Task<string> GetDefaultTopNavLogo()
        {
            var compressedTopNavLogoDecodedImageB64 = "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";
            var imagePath = _defaultGraphics.ImagePath ?? Environment.GetEnvironmentVariable("ImagePath");
            var topNavLogo = _defaultGraphics.TopNavLogo ?? Environment.GetEnvironmentVariable("TopNavLogo");

            if (imagePath != null && topNavLogo != null && File.Exists($"{Directory.GetCurrentDirectory()}{imagePath}{topNavLogo}"))
            {
                var defaultEncodedImageData = await _imageService.EncodeImageDataAsync(topNavLogo, imagePath);
                var defaultImageMimeType = _imageService.GetImageMimeType(topNavLogo, imagePath);
                if (defaultEncodedImageData != null && defaultImageMimeType != null)
                {
                    var defaultDecodedImage = _imageService.DecodeImage(defaultEncodedImageData, defaultImageMimeType);
                    if (defaultDecodedImage != null)
                    {
                        return defaultDecodedImage;
                    }
                }
            }

            return _imageService.DecompressDecodedImageB64(compressedTopNavLogoDecodedImageB64);
        }
    }
}
