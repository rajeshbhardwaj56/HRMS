//using Amazon;
//using Amazon.S3;
//using Amazon.S3.Model;
//using ImageMagick;
//namespace HRMS.Web.BusinessLayer.S3
//{
//    public interface IS3Service
//    {
//        string UploadFile(IFormFile file, string bucketFolder);
//        bool DeleteFile(string key);
//        string GetFileUrl(string key);
//        string ExtractKeyFromUrl(string fileUrl);
//        void ProcessFileUpload(List<IFormFile> files, string existingKey, out string uploadedKey);
//        public Stream GetFileStream(string key);

//    }

//    public class S3Service : IS3Service
//    {
//        private readonly string _accessKey;
//        private readonly string _secretKey;
//        private readonly string _region;
//        private readonly string _bucketName;
//        private readonly IAmazonS3 _s3Client;

//        public S3Service(IConfiguration configuration)
//        {
//            _accessKey = configuration["AWS:AccessKey"];
//            _secretKey = configuration["AWS:SecretKey"];
//            _region = configuration["AWS:Region"];
//            _bucketName = configuration["AWS:BucketName"];
//            _s3Client = new AmazonS3Client(_accessKey, _secretKey, RegionEndpoint.GetBySystemName(_region));
//        }

//        public Stream GetFileStream(string key)
//        {
//            try
//            {
//                var request = new GetObjectRequest
//                {
//                    BucketName = _bucketName,
//                    Key = key
//                };

//                var response = _s3Client.GetObjectAsync(request).Result;
//                // Returns the file as a stream
//                return response.ResponseStream;
//            }
//            catch (AmazonS3Exception ex)
//            {
//                // Log if file not found or access denied
//                Console.WriteLine($"S3 error: {ex.Message}");
//                return null;
//            }
//        }

//        public string UploadFile(IFormFile file, string fileName)
//        {
//            string extension = Path.GetExtension(fileName);
//            string originalName = Path.GetFileNameWithoutExtension(fileName)
//                                .Replace(" ", "_")
//                                .Replace("/", "_")
//                                .Replace("\\", "_");
//            string uniqueFileName = $"{Guid.NewGuid()}_{originalName}{extension}";
//            using (var stream = file.OpenReadStream())
//            {
//                var request = new PutObjectRequest
//                {
//                    BucketName = _bucketName,
//                    Key = uniqueFileName,
//                    InputStream = stream,
//                    ContentType = file.ContentType
//                };

//                var response = _s3Client.PutObjectAsync(request).GetAwaiter().GetResult();
//                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
//                    throw new Exception($"Upload failed. AWS returned status code: {response.HttpStatusCode}");
//                return uniqueFileName;
//            }
//        }
//        public bool DeleteFile(string key)
//        {
//            var deleteRequest = new DeleteObjectRequest
//            {
//                BucketName = _bucketName,
//                Key = key
//            };
//            var response = _s3Client.DeleteObjectAsync(deleteRequest).GetAwaiter().GetResult();
//            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
//        }
//        public string GetFileUrl(string key)
//        {
//            var request = new GetPreSignedUrlRequest
//            {
//                BucketName = _bucketName,
//                Key = key,
//                Expires = DateTime.UtcNow.AddDays(1)
//            };

//            string url = _s3Client.GetPreSignedURL(request);
//            return url;
//        }
//        public string ExtractKeyFromUrl(string fileUrl)
//        {
//            if (string.IsNullOrEmpty(fileUrl)) return string.Empty;

//            var fileName = fileUrl.Substring(fileUrl.LastIndexOf('/') + 1);
//            return fileName.Split('?')[0];
//        }
//        public void ProcessFileUpload(List<IFormFile> files, string existingKey, out string uploadedKey)
//        {
//            uploadedKey = string.Empty;
//            if (files != null && files.Count > 0)
//            {
//                foreach (var file in files)
//                {
//                    if (file?.Length > 0)
//                    {
//                        var extension = Path.GetExtension(file.FileName)?.ToLower();
//                        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".tiff", ".tif", ".heic", ".heif" };
//                        if (imageExtensions.Contains(extension))
//                        {
//                            var compressedFile = CompressImage(file);
//                            uploadedKey = UploadFile(compressedFile, compressedFile.FileName);
//                        }
//                        else
//                        {
//                            uploadedKey = UploadFile(file, file.FileName);
//                        }
//                        if (!string.IsNullOrEmpty(uploadedKey)) break;
//                    }
//                }
//            }
//        }
//        public static IFormFile CompressImage(IFormFile originalFile)
//        {

//            using var inputStream = originalFile.OpenReadStream();
//            using var image = new MagickImage(inputStream);


//            if (image.Width > 1200)
//            {
//                int newHeight = (int)(image.Height * (1200.0 / image.Width));
//                image.Resize(1200, (uint)newHeight);
//            }


//            byte[] compressedBytes;
//            using (var ms = new MemoryStream())
//            {
//                image.Format = MagickFormat.WebP;
//                image.Settings.SetDefine("webp:lossless", "true");
//                image.Write(ms);
//                compressedBytes = ms.ToArray();
//            }

//            var outputStream = new MemoryStream(compressedBytes);
//            var compressedFile = new FormFile(
//                outputStream,
//                0,
//                outputStream.Length,
//                originalFile.Name,
//                Path.GetFileNameWithoutExtension(originalFile.FileName) + ".webp")
//            {
//                Headers = originalFile.Headers,
//                ContentType = "image/webp"
//            };

//            outputStream.Position = 0;
//            return compressedFile;
//        }

//    }
//}

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using HRMS.API.BusinessLayer.ITF;
using HRMS.Models.Common;
using ImageMagick;
using System.Net;
namespace HRMS.Web.BusinessLayer.S3
{
    public interface IS3Service
    {
        string UploadFile(IFormFile file, string bucketFolder);
        bool DeleteFile(string key);
        string GetFileUrl(string key);
        string ExtractKeyFromUrl(string fileUrl);
        void ProcessFileUpload(List<IFormFile> files, string existingKey, out string uploadedKey);
        public Stream GetFileStream(string key);

    }

    public class S3Service : IS3Service
    {

        private readonly string _rootPath;
        private readonly string _username;
        private readonly string _password;
        private readonly string _baseFolder;
        private readonly string _baseApiUrl;


        public S3Service(IConfiguration configuration)
        {

            _rootPath = configuration["FileStorage:RootPath"];
            _username = configuration["FileStorage:Username"];
            _password = configuration["FileStorage:Password"];
            _baseFolder = configuration["FileStorage:BaseFolder"];
            _baseApiUrl = configuration["AppSettings:BaseAPIUrl"];



        }


        public Stream GetFileStream(string key)
        {
            try
            {
                using (new NetworkConnection(
                    _rootPath,
                    new NetworkCredential(_username, _password)))
                {
                    string filePath = Path.Combine(_rootPath, _baseFolder, key);

                    if (!File.Exists(filePath))
                        throw new Exception("File not found: " + filePath);

                    byte[] bytes = File.ReadAllBytes(filePath);

                    return new MemoryStream(bytes);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"RootPath={_rootPath}, BaseFolder={_baseFolder}, Key={key}",
                    ex);
            }
        }
        public string UploadFile(IFormFile file, string fileName)
        {
            using (new NetworkConnection(
                _rootPath,
                new NetworkCredential(_username, _password)))
            {
                string extension = Path.GetExtension(fileName);

                string originalName = Path.GetFileNameWithoutExtension(fileName)
                    .Replace(" ", "_")
                    .Replace("/", "_")
                    .Replace("\\", "_");

                string uniqueFileName =
                    $"{Guid.NewGuid()}_{originalName}{extension}";

                string folderPath = Path.Combine(_rootPath, _baseFolder);

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return uniqueFileName; // ONLY filename stored
            }
        }

        public bool DeleteFile(string key)
        {
            using (new NetworkConnection(
                _rootPath,
                new NetworkCredential(_username, _password)))
            {
                string filePath = Path.Combine(_rootPath, _baseFolder, key);

                if (!File.Exists(filePath))
                    return false;

                File.Delete(filePath);
                return true;
            }
        }

        public string GetFileUrl(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "";

            return $"/Document/GetFile?key={Uri.EscapeDataString(key)}";
        }
        public string ExtractKeyFromUrl(string fileUrl)
        {
            return fileUrl;
        }

        public void ProcessFileUpload(
    List<IFormFile> files,
    string existingKey,
    out string uploadedKey)
        {
            uploadedKey = string.Empty;

            if (files == null || files.Count == 0)
                return;

            // ✅ DELETE OLD FILE FIRST
            if (!string.IsNullOrEmpty(existingKey))
            {
                try
                {
                    DeleteFile(existingKey);
                }
                catch (Exception ex)
                {
                    // optional: log error, don't block upload
                }
            }

            foreach (var file in files)
            {
                if (file == null || file.Length == 0)
                    continue;

                var extension = Path.GetExtension(file.FileName)?.ToLower();

                var imageExtensions = new[]
                {
            ".jpg", ".jpeg", ".png", ".webp", ".gif",
            ".bmp", ".tiff", ".tif", ".heic", ".heif"
        };

                if (imageExtensions.Contains(extension))
                {
                    var compressedFile = CompressImage(file);
                    uploadedKey = UploadFile(compressedFile, compressedFile.FileName);
                }
                else
                {
                    uploadedKey = UploadFile(file, file.FileName);
                }

                if (!string.IsNullOrEmpty(uploadedKey))
                    break;
            }
        }

        public static IFormFile CompressImage(IFormFile originalFile)
        {
            using var inputStream = originalFile.OpenReadStream();
            using var image = new MagickImage(inputStream);

            if (image.Width > 1200)
            {
                int newHeight =
                    (int)(image.Height * (1200.0 / image.Width));

                image.Resize(1200, (uint)newHeight);
            }

            byte[] compressedBytes;

            using (var ms = new MemoryStream())
            {
                image.Format = MagickFormat.WebP;
                image.Settings.SetDefine("webp:lossless", "true");
                image.Write(ms);
                compressedBytes = ms.ToArray();
            }

            var outputStream = new MemoryStream(compressedBytes);

            var compressedFile = new FormFile(
                outputStream,
                0,
                outputStream.Length,
                originalFile.Name,
                Path.GetFileNameWithoutExtension(
                    originalFile.FileName) + ".webp")
            {
                Headers = originalFile.Headers,
                ContentType = "image/webp"
            };

            outputStream.Position = 0;

            return compressedFile;
        }
    }
}
