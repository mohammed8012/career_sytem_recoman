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
            var skills = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(analysis))
                return new List<string>();

            var skillsSectionPattern = @"(?i)(?:SKILLS|TECHNICAL SKILLS|المهارات)\s*:?\s*\n?(.*?)(?=\n\n|\n[A-Z]|\Z)";
            var sectionMatch = Regex.Match(analysis, skillsSectionPattern, RegexOptions.Singleline);
            if (sectionMatch.Success)
            {
                var skillsText = sectionMatch.Groups[1].Value;
                var itemMatches = Regex.Matches(skillsText, @"(?:^|\n)\s*[•\-\*•]\s*(.*?)(?=\n\s*[•\-\*•]|\n\n|\Z)", RegexOptions.Multiline);
                if (itemMatches.Count > 0)
                {
                    foreach (Match match in itemMatches)
                    {
                        var skill = match.Groups[1].Value.Trim();
                        if (!string.IsNullOrWhiteSpace(skill) && skill.Length < 100)
                            skills.Add(skill);
                    }
                }
                else
                {
                    var lines = skillsText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var cleanLine = line.Trim().TrimStart('•', '-', '*', '•').Trim();
                        if (!string.IsNullOrWhiteSpace(cleanLine) && cleanLine.Length < 100 && !cleanLine.Contains(':'))
                            skills.Add(cleanLine);
                    }
                }
            }

            if (skills.Count == 0)
            {
                var commonSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "C#", "Java", "Python", "JavaScript", "TypeScript", "PHP", "Ruby", "Swift", "Kotlin",
                    "ASP.NET", ".NET Core", "Spring Boot", "Django", "Flask", "React", "Angular", "Vue",
                    "Node.js", "Express", "HTML", "CSS", "Bootstrap", "Tailwind",
                    "SQL Server", "MySQL", "PostgreSQL", "MongoDB", "Oracle",
                    "Git", "Docker", "Kubernetes", "Jenkins", "Azure", "AWS", "GCP",
                    "Entity Framework", "Hibernate", "RESTful", "GraphQL"
                };

                var words = Regex.Split(analysis, @"\W+")
                                 .Where(w => w.Length > 1)
                                 .Select(w => w.Trim());

                foreach (var word in words)
                {
                    if (commonSkills.Contains(word))
                        skills.Add(word);
                }
            }

            return skills.ToList();
        }

        private class AnalysisResponse
        {
            public string Analysis { get; set; } = string.Empty;
        }
    }
}