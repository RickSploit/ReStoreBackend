using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReStore.API.DTOs;
using ReStore.Core.Entities;

namespace ReStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public AccountController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            // 1. إنشاء كائن المستخدم الجديد
            var user = new User
            {
                Name = registerDto.Name,
                UserName = registerDto.Email, // الـ Identity بيستخدم الـ UserName كمعرف أساسي
                Email = registerDto.Email
            };

            // 2. استخدام الـ UserManager لحفظ اليوزر وتشفير الباسورد
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                // لو في مشكلة (باسورد ضعيف مثلاً)، نرجع الأخطاء للفرونت إند
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }

            return Ok("User registered successfully");
        }
    }
}