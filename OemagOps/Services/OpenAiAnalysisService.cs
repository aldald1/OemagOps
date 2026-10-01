using OpenAI.Chat;
using OemagOps.Models;
using System.Text.Json;

namespace OemagOps.Services
{
    public class OpenAiAnalysisService
    {
        private readonly ChatClient _client;

        public OpenAiAnalysisService(IConfiguration configuration)
        {
            var apiKey = configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API-Key wurde nicht gefunden.");
            }

            _client = new ChatClient(
                model: "gpt-5.6-luna",
                apiKey: apiKey
            );
        }

        public async Task<AiRequestAnalysis> AnalyzeAsync(
            string sender,
            string subject,
            string body)
        {
            List<ChatMessage> messages =
            [
                new SystemChatMessage("""
                    Du analysierst eingehende Geschäftsanfragen.

                    Deine Aufgabe ist ausschließlich,
                    Informationen aus der Nachricht zu extrahieren.

                    WICHTIGE REGELN:

                    - Erfinde niemals Informationen.
                    - Leite keine Preise ab.
                    - Leite keine Mengen ab, die nicht genannt wurden.
                    - Erfinde keine Firmennamen.
                    - Erfinde keine technischen Anforderungen.
                    - Wenn eine Information nicht vorhanden ist,
                      gib null zurück.
                    - Bewerte nicht, welche Informationen noch fehlen.
                    - Verwende nur Informationen, die tatsächlich
                      in Absender, Betreff oder Nachricht stehen.
                    """),

                new UserChatMessage($"""
                    Absender:
                    {sender}

                    Betreff:
                    {subject}

                    Nachricht:
                    {body}
                    """)
            ];

            ChatCompletionOptions options = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "request_analysis",

                    jsonSchema: BinaryData.FromBytes("""
                    {
                      "type": "object",

                      "properties": {

                        "requestType": {
                          "type": ["string", "null"]
                        },

                        "company": {
                          "type": ["string", "null"]
                        },

                        "contactPerson": {
                          "type": ["string", "null"]
                        },

                        "product": {
                          "type": ["string", "null"]
                        },

                        "quantity": {
                          "type": ["integer", "null"]
                        },

                        "powerWp": {
                          "type": ["integer", "null"]
                        },

                        "color": {
                          "type": ["string", "null"]
                        },

                        "location": {
                          "type": ["string", "null"]
                        },

                        "deliveryTime": {
                          "type": ["string", "null"]
                        },

                        "summary": {
                          "type": ["string", "null"]
                        }
                      },

                      "required": [
                        "requestType",
                        "company",
                        "contactPerson",
                        "product",
                        "quantity",
                        "powerWp",
                        "color",
                        "location",
                        "deliveryTime",
                        "summary"
                      ],

                      "additionalProperties": false
                    }
                    """u8.ToArray()),

                    jsonSchemaIsStrict: true
                )
            };

            ChatCompletion completion =
                await _client.CompleteChatAsync(messages, options);

            string json = completion.Content[0].Text;

            var analysis = JsonSerializer.Deserialize<AiRequestAnalysis>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (analysis == null)
            {
                throw new InvalidOperationException(
                    "Die KI-Analyse konnte nicht verarbeitet werden.");
            }

            return analysis;
        }

        public async Task<string> GenerateReplyDraftAsync(
    AiRequestAnalysis analysis,
    List<string> missingInformation)
        {
            if (missingInformation == null || missingInformation.Count == 0)
            {
                return string.Empty;
            }

            string missing = string.Join(", ", missingInformation);

            List<ChatMessage> messages =
            [
                new SystemChatMessage("""
            Du formulierst professionelle Antwortentwürfe für ein Unternehmen.

            WICHTIGE REGELN:
            - Frage ausschließlich nach den Informationen, die dir ausdrücklich als fehlend genannt werden.
            - Erfinde keine zusätzlichen Anforderungen.
            - Erfinde keine Preise.
            - Erfinde keine Termine.
            - Antworte kurz, freundlich und professionell.
            - Verwende die Sie-Form.
            - Schreibe nur den eigentlichen E-Mail-Text.
            """),

        new UserChatMessage($"""
            Kunde/Firma:
            {analysis.Company ?? "Nicht angegeben"}

            Ansprechpartner:
            {analysis.ContactPerson ?? "Nicht angegeben"}

            Produkt:
            {analysis.Product ?? "Nicht angegeben"}

            Bereits bekannte Menge:
            {analysis.Quantity?.ToString() ?? "Nicht angegeben"}

            Fehlende Informationen:
            {missing}

            Formuliere eine kurze Rückfrage an den Kunden.
            """)
            ];

            ChatCompletion completion =
                await _client.CompleteChatAsync(messages);

            if (completion.Content == null || completion.Content.Count == 0)
            {
                return "Es konnte kein Antwortentwurf erstellt werden.";
            }

            return completion.Content[0].Text;
        }
    }
}