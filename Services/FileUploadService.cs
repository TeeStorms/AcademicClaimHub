using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace ClaimManagementHub.Services
{
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        // Enhanced file type restrictions - made static readonly
        private static readonly Dictionary<string, string[]> AllowedFileTypes = new()
        {
            [".pdf"] = new[] { "application/pdf" },
            [".docx"] = new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            [".xlsx"] = new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            [".jpg"] = new[] { "image/jpeg" },
            [".jpeg"] = new[] { "image/jpeg" },
            [".png"] = new[] { "image/png" }
        };

        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<FileUploadResult> UploadFileAsync(IFormFile file, string uploadSubDirectory = "claims")
        {
            var result = new FileUploadResult();

            try
            {
                // Validate file size
                if (file.Length > MaxFileSize)
                {
                    result.Error = $"File size exceeds the maximum limit of {MaxFileSize / 1024 / 1024}MB";
                    return result;
                }

                // Validate file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                // Use TryGetValue instead of ContainsKey + indexer
                if (!AllowedFileTypes.TryGetValue(fileExtension, out var allowedMimeTypes))
                {
                    result.Error = "Invalid file type. Allowed types: PDF, DOCX, XLSX, JPG, JPEG, PNG";
                    return result;
                }

                // Validate MIME type
                if (!allowedMimeTypes.Contains(file.ContentType))
                {
                    result.Error = "File content type doesn't match the file extension";
                    return result;
                }

                // Create upload directory
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", uploadSubDirectory);
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                // Generate secure filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                result.Success = true;
                result.FileName = file.FileName;
                result.StoredFileName = fileName;
                result.FilePath = $"/uploads/{uploadSubDirectory}/{fileName}";
                result.FileSize = file.Length;

                _logger.LogInformation("File uploaded successfully: {FileName} -> {StoredFileName}", file.FileName, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                result.Error = "An error occurred while uploading the file";
            }

            return result;
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("File deleted: {FilePath}", filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return false;
            }
        }
    }

    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string? FileName { get; set; }
        public string? StoredFileName { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
        public string? Error { get; set; }
    }
}