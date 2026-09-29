using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Web.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IS3Service _fileService;

        public DocumentController(IS3Service fileService)
        {
            _fileService = fileService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetFile(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return NotFound("File key is required.");

            try
            {
                var stream = _fileService.GetFileStream(key);

                if (stream == null)
                    return NotFound("File not found.");

                string extension =
                    Path.GetExtension(key)?.ToLowerInvariant();

                string contentType = extension switch
                {
                    ".pdf" => "application/pdf",

                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",

                    ".doc" => "application/msword",
                    ".docx" =>
                        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                    ".xls" => "application/vnd.ms-excel",
                    ".xlsx" =>
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",

                    ".txt" => "text/plain",

                    _ => "application/octet-stream"
                };

                using var ms = new MemoryStream();

                stream.CopyTo(ms);

                return File(
                    ms.ToArray(),
                    contentType);
            }
            catch (FileNotFoundException)
            {
                return NotFound("File not found.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}