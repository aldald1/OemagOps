using Microsoft.EntityFrameworkCore;
using OemagOps.Data;
using OemagOps.Models;

namespace OemagOps.Services
{
    public class OfferService
    {
        private readonly OemagOpsDbContext _context;

        public OfferService(OemagOpsDbContext context)
        {
            _context = context;
        }

        public async Task<OfferDraft?> CreateOfferDraftAsync(
            AiRequestAnalysis analysis)
        {
            if (!analysis.Quantity.HasValue ||
                !analysis.PowerWp.HasValue ||
                string.IsNullOrWhiteSpace(analysis.Color))
            {
                return null;
            }

            var product = await _context.Products
                .Where(x =>
                    x.IsActive &&
                    x.Category == "PV_MODULE" &&
                    x.PowerWp == analysis.PowerWp &&
                    x.Color == analysis.Color)
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return null;
            }

            var delivery = await _context.Products
                .Where(x =>
                    x.IsActive &&
                    x.Category == "DELIVERY")
                .FirstOrDefaultAsync();

            decimal deliveryCost =
                delivery?.UnitPrice ?? 0;

            decimal productTotal =
                product.UnitPrice * analysis.Quantity.Value;

            decimal grandTotal =
                productTotal + deliveryCost;

            return new OfferDraft
            {
                CustomerName =
                    analysis.Company ?? "Nicht angegeben",

                ProductName =
                    product.Name,

                Quantity =
                    analysis.Quantity.Value,

                UnitPrice =
                    product.UnitPrice,

                ProductTotal =
                    productTotal,

                DeliveryCost =
                    deliveryCost,

                GrandTotal =
                    grandTotal
            };
        }
    }
}