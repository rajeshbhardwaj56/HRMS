using HRMS.Web.BusinessLayer.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Web.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]

    public class FilesController : Controller
    {
        private readonly IS3Service _fileService;


        public FilesController(
            IS3Service fileService)
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
                    return Content("Stream is NULL");

                using var ms = new MemoryStream();
                stream.CopyTo(ms);

                return File(ms.ToArray(), "application/octet-stream");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }
        
    }
}
