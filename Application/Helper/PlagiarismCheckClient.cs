using Application.Enities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Helper
{
    public class PlagiarismCheckClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PlagiarismCheckClient(string baseUrl = "http://localhost:5001")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        public async Task<bool> IsServiceHealthyAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<PlagiarismCheckResult> CheckPlagiarismAsync(string zipFilePath, string submissionId, double threshold = 0.85)
        {
            try
            {
                using var formData = new MultipartFormDataContent();
                
                var fileBytes = await File.ReadAllBytesAsync(zipFilePath);
                var fileContent = new ByteArrayContent(fileBytes);
                formData.Add(fileContent, "file", Path.GetFileName(zipFilePath));
                formData.Add(new StringContent(submissionId), "submission_id");
                formData.Add(new StringContent(threshold.ToString()), "threshold");

                var response = await _httpClient.PostAsync($"{_baseUrl}/check-plagiarism", formData);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Plagiarism check failed: {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                
                var result = JsonSerializer.Deserialize<PlagiarismCheckResult>(jsonResponse, options);
                return result ?? throw new Exception("Failed to deserialize plagiarism check result");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling plagiarism check service: {ex.Message}", ex);
            }
        }

        public async Task<StoreSubmissionResult> StoreSubmissionAsync(string zipFilePath, string submissionId)
        {
            try
            {
                using var formData = new MultipartFormDataContent();
                
                var fileBytes = await File.ReadAllBytesAsync(zipFilePath);
                var fileContent = new ByteArrayContent(fileBytes);
                formData.Add(fileContent, "file", Path.GetFileName(zipFilePath));
                formData.Add(new StringContent(submissionId), "submission_id");

                var response = await _httpClient.PostAsync($"{_baseUrl}/store-submission", formData);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Store submission failed: {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                
                var result = JsonSerializer.Deserialize<StoreSubmissionResult>(jsonResponse, options);
                return result ?? throw new Exception("Failed to deserialize store submission result");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calling store submission service: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteSubmissionAsync(string submissionId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/delete-submission/{submissionId}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
