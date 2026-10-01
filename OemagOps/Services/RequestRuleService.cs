using OemagOps.Models;

namespace OemagOps.Services
{
    public class RequestRuleService
    {
        public List<string> GetMissingInformation(AiRequestAnalysis analysis)
        {
            var missing = new List<string>();

            if (!analysis.Quantity.HasValue)
            {
                missing.Add("Menge");
            }

            if (!analysis.PowerWp.HasValue)
            {
                missing.Add("Leistung pro Modul");
            }

            if (string.IsNullOrWhiteSpace(analysis.Location))
            {
                missing.Add("Lieferort");
            }

            if (string.IsNullOrWhiteSpace(analysis.DeliveryTime))
            {
                missing.Add("Gewünschter Liefertermin");
            }

            return missing;
        }
    }
}