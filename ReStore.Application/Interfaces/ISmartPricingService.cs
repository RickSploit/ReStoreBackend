using System;

namespace ReStore.Application.Interfaces
{
    public interface ISmartPricingService
    {
        decimal CalculateSuggestedPrice(decimal originalPrice, int ageInYears, string condition);
    }
}

