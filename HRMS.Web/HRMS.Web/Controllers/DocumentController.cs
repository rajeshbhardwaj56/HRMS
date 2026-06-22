using HRMS.Web.BusinessLayer.S3;
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

        [HttpGet]
        public IActionResult GetFile(string key)
        {
            try
            {
                var stream = _fileService.GetFileStream(key);

                if (stream == null)
                    return NotFound("File not found.");

                using var ms = new MemoryStream();
                stream.CopyTo(ms);

                return File(ms.ToArray(), "application/octet-stream");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
