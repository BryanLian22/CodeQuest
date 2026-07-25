// Purpose: Calls the configured generative-AI endpoint and normalizes service errors for the UI.
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using CodeQuest.Models;

namespace CodeQuest.Data
{
    /// <summary>
    /// Server-side Google Gemini client for the Premium learning assistant.
    /// The API key is read only from Web.config and is never sent to the browser.
    /// The filename is retained for simple upgrades from the earlier provider.
    /// </summary>
    public sealed class GoogleAiClient
    {
        private const string DefaultEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";
        private const string DefaultModel = "gemini-3.5-flash";

        public string Ask(string context, IList<AiChatMessage> conversation)
        {
            string apiKey = ConfigurationManager.AppSettings["CodeQuestGoogleAiApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Add CodeQuestGoogleAiApiKey to Web.config before sending an AI prompt.");
            }

            string model = ConfigurationManager.AppSettings["CodeQuestGoogleAiModel"] ?? DefaultModel;
            List<Dictionary<string, object>> contents = new List<Dictionary<string, object>>();
            int start = Math.Max(0, conversation.Count - 8);
            for (int index = start; index < conversation.Count; index++)
            {
                AiChatMessage message = conversation[index];
                if (message == null || string.IsNullOrWhiteSpace(message.Content))
                {
                    continue;
                }

                contents.Add(new Dictionary<string, object>
                {
                    { "role", message.Role == "assistant" ? "model" : "user" },
                    { "parts", new List<Dictionary<string, string>>
                        {
                            new Dictionary<string, string> { { "text", message.Content } }
                        }
                    }
                });
            }

            Dictionary<string, object> payload = new Dictionary<string, object>
            {
                { "systemInstruction", new Dictionary<string, object>
                    {
                        { "parts", new List<Dictionary<string, string>>
                            {
                                new Dictionary<string, string> { { "text", BuildSystemPrompt(context) } }
                            }
                        }
                    }
                },
                { "contents", contents },
                { "generationConfig", new Dictionary<string, object>
                    {
                        { "temperature", 0.4 },
                        { "maxOutputTokens", 1200 }
                    }
                }
            };

            string endpointTemplate = ConfigurationManager.AppSettings["CodeQuestGoogleAiEndpoint"] ?? DefaultEndpoint;
            string endpoint = BuildEndpoint(endpointTemplate, model, apiKey);
            string body = new JavaScriptSerializer().Serialize(payload);
            byte[] bytes = Encoding.UTF8.GetBytes(body);
            for (int attempt = 0; attempt < 3; attempt++)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Accept = "application/json";
                request.Timeout = 30000;
                request.ReadWriteTimeout = 30000;
                request.ContentLength = bytes.Length;

                try
                {
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        return ReadAnswer(reader.ReadToEnd());
                    }
                }
                catch (WebException exception)
                {
                    string responseBody = string.Empty;
                    HttpWebResponse errorResponse = exception.Response as HttpWebResponse;
                    if (errorResponse != null)
                    {
                        using (StreamReader reader = new StreamReader(errorResponse.GetResponseStream()))
                        {
                            responseBody = reader.ReadToEnd();
                        }
                    }

                    if (IsTransient(errorResponse) && attempt < 2)
                    {
                        if (errorResponse != null)
                        {
                            errorResponse.Close();
                        }

                        // Retry transient overload/quota errors after 1s, then 2s.
                        Thread.Sleep(1000 * (1 << attempt));
                        continue;
                    }

                    throw new InvalidOperationException(BuildFriendlyError(errorResponse, responseBody), exception);
                }
            }

            throw new InvalidOperationException("The Google AI service could not be reached. Try again later.");
        }

        private static bool IsTransient(HttpWebResponse response)
        {
            if (response == null)
            {
                return true;
            }

            int statusCode = (int)response.StatusCode;
            return statusCode == 408 || statusCode == 429 || statusCode == 500
                || statusCode == 502 || statusCode == 503 || statusCode == 504;
        }

        private static string BuildEndpoint(string endpointTemplate, string model, string apiKey)
        {
            string endpoint = endpointTemplate;
            if (endpoint.IndexOf("{model}", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                endpoint = endpoint.Replace("{model}", Uri.EscapeDataString(model));
            }
            else if (endpoint.IndexOf(":generateContent", StringComparison.OrdinalIgnoreCase) < 0)
            {
                endpoint = endpoint.TrimEnd('/') + "/models/" + Uri.EscapeDataString(model) + ":generateContent";
            }

            return endpoint + (endpoint.IndexOf("?", StringComparison.Ordinal) >= 0 ? "&" : "?")
                + "key=" + Uri.EscapeDataString(apiKey);
        }

        private static string BuildSystemPrompt(string context)
        {
            return "You are CodeQuest AI, a patient web-development tutor. "
                + "Give beginner-friendly explanations, small examples and hints. "
                + "Do not complete graded quiz answers for the learner; guide them to reason it out. "
                + "Stay focused on HTML, CSS, JavaScript and the learner's current lesson. "
                + "Current learning context: " + (context ?? "General web-development help") + ".";
        }

        private static string ReadAnswer(string json)
        {
            IDictionary<string, object> root = new JavaScriptSerializer().DeserializeObject(json) as IDictionary<string, object>;
            object candidatesValue;
            object[] candidates = root != null && root.TryGetValue("candidates", out candidatesValue)
                ? candidatesValue as object[]
                : null;
            if (candidates == null || candidates.Length == 0)
            {
                throw new InvalidOperationException("The AI service returned no answer. Try again in a moment.");
            }

            IDictionary<string, object> candidate = candidates[0] as IDictionary<string, object>;
            object contentValue;
            IDictionary<string, object> content = candidate != null && candidate.TryGetValue("content", out contentValue)
                ? contentValue as IDictionary<string, object>
                : null;
            object partsValue;
            object[] parts = content != null && content.TryGetValue("parts", out partsValue)
                ? partsValue as object[]
                : null;
            if (parts == null || parts.Length == 0)
            {
                throw new InvalidOperationException("The AI service returned an empty answer. Try a different prompt.");
            }

            StringBuilder answer = new StringBuilder();
            for (int index = 0; index < parts.Length; index++)
            {
                IDictionary<string, object> part = parts[index] as IDictionary<string, object>;
                object textValue;
                if (part != null && part.TryGetValue("text", out textValue))
                {
                    answer.Append(Convert.ToString(textValue));
                }
            }

            if (answer.Length == 0)
            {
                throw new InvalidOperationException("The AI service returned an empty answer. Try a different prompt.");
            }

            return answer.ToString().Trim();
        }

        private static string BuildFriendlyError(HttpWebResponse response, string responseBody)
        {
            string providerMessage = ExtractProviderMessage(responseBody);
            if (response != null)
            {
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return "Google AI rejected the request (400). "
                        + (providerMessage ?? "Check the Gemini model and request settings.");
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return "The Google AI API key was rejected (" + (int)response.StatusCode + "). "
                        + (providerMessage ?? "Check CodeQuestGoogleAiApiKey in Web.config.");
                }

                if ((int)response.StatusCode == 429)
                {
                    return "The Google AI usage limit was reached (429). "
                        + (providerMessage ?? "Wait a moment or check your AI Studio quota.");
                }

                if ((int)response.StatusCode == 500 || (int)response.StatusCode == 503)
                {
                    return "Google AI is temporarily unavailable (" + (int)response.StatusCode + "). "
                        + (providerMessage ?? "Try again in a moment.");
                }

                if (providerMessage != null)
                {
                    return "Google AI returned HTTP " + (int)response.StatusCode + ": " + providerMessage;
                }
            }

            return string.IsNullOrWhiteSpace(responseBody)
                ? "The Google AI service could not be reached. Check the endpoint and try again."
                : "The Google AI service returned an error. Check the Gemini configuration and try again.";
        }

        private static string ExtractProviderMessage(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
            {
                return null;
            }

            try
            {
                IDictionary<string, object> root = new JavaScriptSerializer().DeserializeObject(responseBody)
                    as IDictionary<string, object>;
                object errorValue;
                IDictionary<string, object> error = root != null && root.TryGetValue("error", out errorValue)
                    ? errorValue as IDictionary<string, object>
                    : null;
                object messageValue;
                string message = error != null && error.TryGetValue("message", out messageValue)
                    ? Convert.ToString(messageValue)
                    : null;
                return string.IsNullOrWhiteSpace(message) ? null : message.Trim();
            }
            catch (Exception)
            {
                // Never let error-message parsing hide the original API error.
                return null;
            }
        }
    }

}
