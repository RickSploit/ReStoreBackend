using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ReStore.Infrastructure.Services;
public class ImageService
{
    private readonly IWebHostEnvironment _env;
    public ImageService(IWebHostEnvironment env) => _env = env;

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0) return null;

        // 1. تكوين اسم فريد للصورة عشان مفيش صورتين يتصادموا
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        
        // 2. تحديد المسار اللي هتسيف فيه
        var path = Path.Combine(_env.WebRootPath, "images/appliances", fileName);

        // 3. حفظ الملف فعلياً
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        // 4. نرجع اللينك اللي هيتحفظ في الداتا بيز
        return $"/images/appliances/{fileName}";
    }
}