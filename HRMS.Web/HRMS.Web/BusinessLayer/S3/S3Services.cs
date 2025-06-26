using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using ImageMagick;
namespace HRMS.Web.BusinessLayer.S3
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(IFormFile file, string fileName);
        Task<bool> DeleteFileAsync(string key);
        Task<string> GetFileUrl(string key);
        string ExtractKeyFromUrl(string fileUrl);
        Task<string> ProcessFileUploadAsync(List<IFormFile> files, string existingKey);
       
        
    }

    public class S3Service : IS3Service
    {
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _region;
        private readonly string _bucketName;
        private readonly IAmazonS3 _s3Client;

        public S3Service(IConfiguration configuration)
        {
            _accessKey = configuration["AWS:AccessKey"];
            _secretKey = configuration["AWS:SecretKey"];
            _region = configuration["AWS:Region"];
            _bucketName = configuration["AWS:BucketName"];
            _s3Client = new AmazonS3Client(_accessKey, _secretKey, RegionEndpoint.GetBySystemName(_region));
        }
        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            string extension = Path.GetExtension(fileName);
            string originalName = Path.GetFileNameWithoutExtension(fileName)
                                .Replace(" ", "_")
                                .Replace("/", "_")
                                .Replace("\\", "_");
            string uniqueFileName = $"{Guid.NewGuid()}_{originalName}{extension}";
            using (var stream = file.OpenReadStream())
            {
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = uniqueFileName,
                    InputStream = stream,
                    ContentType = file.ContentType
                };

                var response = await _s3Client.PutObjectAsync(request);
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception($"Upload failed. AWS returned status code: {response.HttpStatusCode}");
                return uniqueFileName;
            }
        }
        public async Task<bool> DeleteFileAsync(string key)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response =await _s3Client.DeleteObjectAsync(deleteRequest);

            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        public async Task<string> GetFileUrl(string key)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            string url = _s3Client.GetPreSignedURL(request);
            return url;
        }
        public string ExtractKeyFromUrl(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return string.Empty;

            var fileName = fileUrl.Substring(fileUrl.LastIndexOf('/') + 1);
            return fileName.Split('?')[0];
        }
        public async Task<string> ProcessFileUploadAsync(List<IFormFile> files, string existingKey)
        {
            if (files == null || !files.Any()) return string.Empty;

            foreach (var file in files)
            {
                if (file?.Length > 0)
                {
                    var extension = Path.GetExtension(file.FileName)?.ToLower();
                    var imageExtensions = new HashSet<string>
                    {
                        ".jpg", ".jpeg", ".png", ".webp", ".gif",
                        ".bmp", ".tiff", ".tif", ".heic", ".heif"
                    };

                    var fileToUpload = imageExtensions.Contains(extension)
                        ? CompressImage(file)
                        : file;

                    return await UploadFileAsync(fileToUpload, fileToUpload.FileName);
                }
            }

            return string.Empty;
        }
        public static IFormFile CompressImage(IFormFile originalFile)
        {
           
            using var inputStream = originalFile.OpenReadStream();
            using var image = new MagickImage(inputStream);

          
            if (image.Width > 1200)
            {
                int newHeight = (int)(image.Height * (1200.0 / image.Width));
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
                Path.GetFileNameWithoutExtension(originalFile.FileName) + ".webp")
            {
                Headers = originalFile.Headers,
                ContentType = "image/webp"
            };

            outputStream.Position = 0;
            return compressedFile;
        }
    }
}
