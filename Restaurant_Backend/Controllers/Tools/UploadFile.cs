using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant_Backend.Controllers.Tools
{
    public class UploadFile
    {
        public static async Task<string> UploadImage(IFormFile? image, IWebHostEnvironment _hostingEnvironment,string fileName)
        {
            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var uniqueFileName =fileName + ".jpg";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                return $"Uploads/{uniqueFileName}";
            }
            return string.Empty;

        }
        public static void DeleteImage(IWebHostEnvironment _hostingEnvironment,string fileName)
        {
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath,fileName);
            if (File.Exists(filePath))
            { 
                File.Delete(filePath);
            }

        }
    }
}
