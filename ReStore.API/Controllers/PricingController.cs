using System;
using Microsoft.AspNetCore.Mvc;
using ReStore.Application.Interfaces;

namespace ReStore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly ISmartPricingService _smartPricingService;

        public PricingController(ISmartPricingService smartPricingService)
        {
            _smartPricingService = smartPricingService;
        }

        [HttpGet("suggest")]
        public IActionResult Suggest(
            [FromQuery] decimal originalPrice,
            [FromQuery] int ageInYears,
            [FromQuery] string condition)
        {
            try
            {
                var suggestedPrice = _smartPricingService.CalculateSuggestedPrice(originalPrice, ageInYears, condition);
                return Ok(new { suggestedPrice });
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
    }
}

