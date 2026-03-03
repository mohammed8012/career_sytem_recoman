using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using career_sytem_recoman.Models.DTOs.CV;
using career_sytem_recoman.Services.Interfaces;

namespace career_sytem_recoman.Services
{
    public class AiCvService : IAiCvService
    {
        private readonly HttpClient _httpClient;
        private readonly string _aiServiceUrl;
        private readonly ILogger<AiCvService> _logger;

        public AiCvService(HttpClient httpClient, IConfiguration configuration, ILogger<AiCvService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _aiServiceUrl = configuration["AiService:Url"]
                ?? throw new InvalidOperationException("AiService:Url is missing in configuration.");
        }

        public async Task<CvAnalysisResultDto> GetFullAnalysisAsync(Stream fileStream, string fileName)
        {
            try
            {
                using var formData = new MultipartFormDataContent();
                using var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                formData.Add(fileContent, "file", fileName);

                var response = await _httpClient.PostAsync(_aiServiceUrl, formData);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AnalysisResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var analysisText = result?.Analysis ?? "No analysis returned.";
                var skills = ExtractSkillsFromAnalysis(analysisText);

                return new CvAnalysisResultDto
                {
                    Analysis = analysisText,
                    Skills = skills
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling AI service for CV: {FileName}", fileName);
                throw;
            }
        }

        private List<string> ExtractSkillsFromAnalysis(string analysis)
        {
            var skills = new List<string>();

            // البحث عن قسم "Skills:" في النص
            var match = Regex.Match(analysis, @"\*\*Skills:\*\*(.*?)(?=\n\n|\Z)", RegexOptions.Singleline);
            if (match.Success)
            {
                var skillsText = match.Groups[1].Value;
                // استخراج العناصر النقطية
                var items = Regex.Matches(skillsText, @"[-*]\s*(.*?)(?=\n|$)")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
                skills.AddRange(items);
            }

            // إذا لم نجد قسم Skills، نبحث عن المهارات المذكورة بشكل مباشر
            if (skills.Count == 0)
            {
                // استخراج كل الأسطر التي تحتوي على نقاط أو أرقام
                var lines = analysis.Split('\n');
                foreach (var line in lines)
                {
                    if (line.TrimStart().StartsWith("-") || line.TrimStart().StartsWith("*") || Regex.IsMatch(line, @"^\d+\."))
                    {
                        var skill = Regex.Replace(line, @"^[-*\d.]\s*", "").Trim();
                        if (!string.IsNullOrWhiteSpace(skill) && skill.Length < 100) // تجنب الفقرات الطويلة
                        {
                            skills.Add(skill);
                        }
                    }
                }
            }

            return skills.Distinct().ToList();
        }

        private class AnalysisResponse
        {
            public string Analysis { get; set; } = string.Empty;
        }
    }
}