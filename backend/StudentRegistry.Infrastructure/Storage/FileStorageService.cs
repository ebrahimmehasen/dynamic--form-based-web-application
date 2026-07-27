using Microsoft.AspNetCore.Hosting;
using StudentRegistry.Application.Interfaces;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudentRegistry.Infrastructure.Storage
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _env;
        private const long MaxFileSizeInBytes = 2 * 1024 * 1024; // 2MB

        public FileStorageService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveBase64ImageAsync(string base64String, string nationalId)
        {
            if (string.IsNullOrEmpty(base64String))
                throw new ArgumentException("بيانات الصورة الشخصية مفقودة.");

            // 1. Parse base64 header and extract extension
            var match = Regex.Match(base64String, @"^data:image/(?<type>\w+);base64,(?<data>.+)$");
            if (!match.Success)
                throw new ArgumentException("تنسيق الصورة غير صالح.");

            string fileType = match.Groups["type"].Value.ToLower();
            string base64Data = match.Groups["data"].Value;

            if (fileType == "jpeg") fileType = "jpg";

            // 2. Validate MIME type extension
            if (fileType != "jpg" && fileType != "png" && fileType != "webp")
                throw new ArgumentException("صيغة الصورة غير مدعومة. يجب أن تكون JPG أو PNG أو WebP.");

            // 3. Decode base64 to byte array
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(base64Data);
            }
            catch
            {
                throw new ArgumentException("فشل فك تشفير الصورة الشخصية.");
            }

            // 4. Validate image file signature (magic bytes)
            if (!IsValidImageSignature(imageBytes, fileType))
                throw new ArgumentException("محتوى الصورة غير صالح أو لا يتطابق مع الصيغة المحددة.");

            // 5. Validate image file size (max 2MB)
            if (imageBytes.Length > MaxFileSizeInBytes)
                throw new ArgumentException("حجم الصورة كبير جداً. الحد الأقصى هو 2 ميجابايت.");

            // 6. Generate secure unique filename
            // Clean national ID for filesystem safety
            string safeNationalId = Regex.Replace(nationalId, @"[^a-zA-Z0-9_\-]", "");
            string uniqueFilename = $"{safeNationalId}_{Guid.NewGuid():N}.{fileType}";

            // 7. Resolve absolute upload path in wwwroot
            string uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string absolutePath = Path.Combine(uploadsFolder, uniqueFilename);

            // 8. Write files asynchronously to disk
            await File.WriteAllBytesAsync(absolutePath, imageBytes);

            // 9. Return relative path for database storage
            return $"uploads/{uniqueFilename}";
        }

        private bool IsValidImageSignature(byte[] bytes, string extension)
        {
            if (bytes == null || bytes.Length < 12) return false;

            if (extension == "jpg" || extension == "jpeg")
            {
                // JPEG starts with FF D8 FF
                return bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
            }
            else if (extension == "png")
            {
                // PNG starts with 89 50 4E 47 0D 0A 1A 0A
                return bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                       bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
            }
            else if (extension == "webp")
            {
                // WebP starts with "RIFF" (offset 0) and has "WEBP" (offset 8)
                return bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                       bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50;
            }

            return false;
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            string absolutePath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), relativePath);
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }
    }
}
