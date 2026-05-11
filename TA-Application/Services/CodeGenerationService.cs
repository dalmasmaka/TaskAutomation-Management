using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Octokit;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using TA_Application.Configurations;
using TA_Application.DTOs;
using TA_Application.Interfaces.Services;
using static Azure.Core.HttpHeader;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TA_Application.Services
{
    public class CodeGenerationService : ICodeGenerationService
    {
        private readonly HttpClient _httpClient;
        private readonly GroqSettings _settings;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public CodeGenerationService(
            HttpClient httpClient,
            IOptions<GroqSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<GeneratedFolderDTO> GenerateAsync(string description)
        {
            await _semaphore.WaitAsync();
            try
            {
                var prompt = BuildPrompt(description);

                // Use _settings.Model and _settings.ApiKey from your appsettings
                var requestBody = new
                {
                    model = _settings.Model,
                    messages = new[]
                    {
                        new { role = "system", content = "You are a senior .NET developer that outputs only raw JSON." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.1, // Lower temperature ensures more stable JSON structure
                    response_format = new { type = "json_object" } // Forces Groq to return valid JSON
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

                request.Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Groq API Error: {response.StatusCode} - {errorDetails}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseContent);

                // Groq follows the OpenAI response structure
                var jsonResult = doc
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var generated = JsonSerializer.Deserialize<GeneratedResponse>(jsonResult ?? "{}", options);

                return new GeneratedFolderDTO
                {
                    FolderName = "Generated_Code",
                    Files = generated?.Files ?? new List<GeneratedFileDTO>()
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public string BuildPrompt(string description)
        {
            return $@"
            Generate code for the following task: {description}

            Return a JSON object with the following structure:
            {{
              ""files"": [
                {{
                  ""fileName"": ""Services/ProductService.cs"",
                  ""content"": ""The source code""
                }}
              ]
            }}

            Requirements:
            1. Provide ONLY the JSON.
            2. Do not use markdown code blocks (no ```).
            3. Ensure all quotes inside the 'content' field are properly escaped for a JSON string
            STRICT RULES:
            -Every file MUST include full relative path
            - Use proper layered architecture when applicable:
            - Controllers
            - Services
            - Repositories
            - DTOs
            - Models
          - Do NOT output flat file names.";

        }
    }

    // internal helper to match your JSON structure
    internal class GeneratedResponse
    {
        public List<GeneratedFileDTO> Files { get; set; } = new();
    }
}