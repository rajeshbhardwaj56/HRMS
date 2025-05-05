using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace HRMS.Web.BusinessLayer.S3
{
    public interface IS3Service
    {
        string UploadFile(IFormFile file, string bucketFolder);
        bool DeleteFile(string key);
        Stream DownloadFile(string key);
        string GetFileUrl(string key);
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
        public string UploadFile(IFormFile file, string fileName)
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

                var response = _s3Client.PutObjectAsync(request).GetAwaiter().GetResult();
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception($"Upload failed. AWS returned status code: {response.HttpStatusCode}");
                return uniqueFileName;
            }
        }

        public bool DeleteFile(string key)
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = _s3Client.DeleteObjectAsync(deleteRequest).GetAwaiter().GetResult();

            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }

        public Stream DownloadFile(string key)
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };
            var response = _s3Client.GetObjectAsync(request).GetAwaiter().GetResult();
            return response.ResponseStream;
        }
        public string GetFileUrl(string key)
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

    }
}