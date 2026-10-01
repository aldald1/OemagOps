using Microsoft.AspNetCore.Mvc;
using OemagOps.Data;
using OemagOps.Models;
using OemagOps.Services;

namespace OemagOps.Controllers
{
    public class RequestsController : Controller
    {
        private readonly OemagOpsDbContext _context;
        private readonly OpenAiAnalysisService _aiService;
        private readonly RequestRuleService _ruleService;
        private readonly OfferService _offerService;
        private readonly ILogger<RequestsController> _logger;

        public RequestsController(
            OemagOpsDbContext context,
            OpenAiAnalysisService aiService,
            RequestRuleService ruleService,
            OfferService offerService,
            ILogger<RequestsController> logger)
        {
            _context = context;
            _aiService = aiService;
            _ruleService = ruleService;
            _offerService = offerService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IncomingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            request.Status = "NEW";
            request.CreatedAt = DateTime.Now;

            _context.IncomingRequests.Add(request);
            await _context.SaveChangesAsync();

            try
            {
                // 1. Anfrage mit KI analysieren
                var aiAnalysis = await _aiService.AnalyzeAsync(
                    request.Sender,
                    request.Subject,
                    request.Body);

                // 2. Firmenregeln prüfen
                var missingInformation =
                    _ruleService.GetMissingInformation(aiAnalysis);

                string? replyDraft = null;
                OfferDraft? offerDraft = null;

                // 3. Je nach Vollständigkeit weiterarbeiten
                if (missingInformation.Count > 0)
                {
                    request.Status = "MISSING_INFORMATION";

                    replyDraft =
                        await _aiService.GenerateReplyDraftAsync(
                            aiAnalysis,
                            missingInformation);
                }
                else
                {
                    request.Status = "READY_FOR_OFFER";

                    offerDraft =
                        await _offerService.CreateOfferDraftAsync(
                            aiAnalysis);

                    if (offerDraft == null)
                    {
                        request.Status = "PRODUCT_NOT_FOUND";
                    }
                }

                await _context.SaveChangesAsync();

                // 4. Daten an View übergeben
                ViewBag.AiAnalysis = aiAnalysis;
                ViewBag.MissingInformation = missingInformation;
                ViewBag.ReplyDraft = replyDraft;
                ViewBag.OfferDraft = offerDraft;

                return View("Result", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Fehler bei der Verarbeitung der Anfrage {RequestId}",
                    request.Id);

                request.Status = "AI_ERROR";
                await _context.SaveChangesAsync();

                ViewBag.ErrorMessage =
                    "Die Anfrage wurde gespeichert, konnte aber nicht vollständig verarbeitet werden.";

                return View("Result", request);
            }
        }
    }
}