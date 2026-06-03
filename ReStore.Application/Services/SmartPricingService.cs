using System;
using ReStore.Application.Interfaces;

namespace ReStore.Application.Services
{
    public class SmartPricingService : ISmartPricingService
    {
        public decimal CalculateSuggestedPrice(decimal originalPrice, int ageInYears, string condition)
        {
            if (originalPrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(originalPrice), "Original price must be greater than 0.");

            if (ageInYears < 0)
                throw new ArgumentOutOfRangeException(nameof(ageInYears), "Age in years cannot be negative.");

            if (string.IsNullOrWhiteSpace(condition))
                throw new ArgumentException("Condition is required.", nameof(condition));

            var normalizedCondition = condition.Trim();
            decimal discountPercent;

            switch (normalizedCondition)
            {
                case "LikeNew":
                    discountPercent = 15m + (5m * ageInYears);
                    break;
                case "Good":
                    discountPercent = 30m + (5m * ageInYears);
                    break;
                case "Fair":
                    discountPercent = 50m + (5m * ageInYears);
                    break;
                default:
                    throw new ArgumentException("Condition must be one of: LikeNew, Good, Fair.", nameof(condition));
            }

            // Convert percent discount into multiplier: price * (1 - discount/100)
            var discountedPrice = originalPrice * (1m - discountPercent / 100m);

            // Final price never drops below 20% of original price.
            var minPrice = originalPrice * 0.20m;

            return discountedPrice < minPrice ? minPrice : discountedPrice;
        }
    }
}

