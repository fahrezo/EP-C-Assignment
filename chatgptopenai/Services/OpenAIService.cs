﻿using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using chatgptopenai.Model;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Reflection.Metadata;

namespace chatgptopenai.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAIService(HttpClient httpClient, IConfiguration Configuration)
        {
            _httpClient = httpClient;
            _configuration = Configuration;
        }

        public async Task<string> GenerateCompletion(string prompt)
        {

            var requestData = new
            {
                model = "gpt-3.5-turbo",
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 2000,
                temperature = 0
            };

            var jsonRequest = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration.GetValue<string>("ApiKey")}");

            var response = await _httpClient.PostAsync(_configuration.GetValue<string>("ApiUrl"), content);

            if (response == null || !response.IsSuccessStatusCode)
            {
                return $"failed openai api call with this status code -{response?.StatusCode} {response}";
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            // Extract the generated response
            ChatGptAPIResponse responseData = JsonSerializer.Deserialize<ChatGptAPIResponse>(responseBody);
            // string generatedText = responseData.choices[0].text;
            string generatedText = responseData.choices[0].message.content;

            // Console.WriteLine(responseData.choices[0].message.content);
            return generatedText;
        }
    }
}
