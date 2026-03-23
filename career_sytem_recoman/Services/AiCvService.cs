using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using career_sytem_recoman.Models.DTOs.CV;
using career_sytem_recoman.Services.Interfaces;
using FuzzySharp;

namespace career_sytem_recoman.Services
{
    public class AiCvService : IAiCvService
    {
        private readonly HttpClient _httpClient;
        private readonly string _aiServiceUrl;
        private readonly ILogger<AiCvService> _logger;

        // قائمة المهارات المعروفة مع تصنيفها
        private static readonly Dictionary<string, string> KnownSkills = new(StringComparer.OrdinalIgnoreCase)
        {
            // لغات البرمجة
            { "C#", "Programming Language" },
            { "C++", "Programming Language" },
            { "Java", "Programming Language" },
            { "Python", "Programming Language" },
            { "JavaScript", "Programming Language" },
            { "TypeScript", "Programming Language" },
            { "PHP", "Programming Language" },
            { "Ruby", "Programming Language" },
            { "Swift", "Programming Language" },
            { "Kotlin", "Programming Language" },
            { "Go", "Programming Language" },
            { "Rust", "Programming Language" },
            // أطر العمل والمكتبات
            { "ASP.NET", "Framework" },
            { ".NET Core", "Framework" },
            { "Spring Boot", "Framework" },
            { "Django", "Framework" },
            { "Flask", "Framework" },
            { "React", "Framework" },
            { "Angular", "Framework" },
            { "Vue", "Framework" },
            { "Node.js", "Framework" },
            { "Express", "Framework" },
            // قواعد البيانات
            { "SQL Server", "Database" },
            { "MySQL", "Database" },
            { "PostgreSQL", "Database" },
            { "MongoDB", "Database" },
            { "Oracle", "Database" },
            // أدوات وتقنيات
            { "Git", "Tool" },
            { "Docker", "Tool" },
            { "Kubernetes", "Tool" },
            { "Jenkins", "Tool" },
            { "Azure", "Cloud" },
            { "AWS", "Cloud" },
            { "GCP", "Cloud" },
            { "Entity Framework", "ORM" },
            { "Hibernate", "ORM" },
            { "RESTful", "Architecture" },
            { "GraphQL", "Architecture" },
            // مهارات عامة
            { "HTML", "Frontend" },
            { "CSS", "Frontend" },
            { "Bootstrap", "Frontend" },
            { "Tailwind", "Frontend" },
        };

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
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("AI Response: {Response}", responseBody);

                response.EnsureSuccessStatusCode();

                var result = JsonSerializer.Deserialize<AnalysisResponse>(responseBody,
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

        /// <summary>
        /// استخراج المهارات من نص التحليل مع مطابقة ضبابية وتصنيف.
        /// </summary>
        private List<string> ExtractSkillsFromAnalysis(string analysis)
        {
            if (string.IsNullOrWhiteSpace(analysis))
                return new List<string>();

            var candidateSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. البحث عن أقسام المهارات بأسماء مختلفة
            var sectionPatterns = new[]
            {
                @"(?i)SKILLS?\s*:?\s*\n(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)TECHNICAL SKILLS?\s*:?\s*\n(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)PROGRAMMING LANGUAGES?\s*:?\s*\n(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)TOOLS?\s*:?\s*\n(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)FRAMEWORKS?\s*:?\s*\n(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)DATABASES?\s*:?\s*\n(.*?)(?=\n\n|\n[A-Z]|\Z)"
            };

            string allSkillsText = "";
            foreach (var pattern in sectionPatterns)
            {
                var match = Regex.Match(analysis, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (match.Success)
                    allSkillsText += " " + match.Groups[1].Value;
            }

            if (string.IsNullOrWhiteSpace(allSkillsText))
                allSkillsText = analysis;

            // 2. استخراج العناصر النقطية (bullets)
            var bulletMatches = Regex.Matches(allSkillsText, @"(?:^|\n)\s*[•\-\*•\d\.]\s*(.*?)(?=\n\s*[•\-\*•\d\.]|\n\n|\Z)", RegexOptions.Multiline);
            if (bulletMatches.Count > 0)
            {
                foreach (Match match in bulletMatches)
                {
                    var skill = match.Groups[1].Value.Trim().TrimEnd(':');
                    if (!string.IsNullOrWhiteSpace(skill) && skill.Length < 100 && !IsNonSkillPhrase(skill))
                        candidateSkills.Add(NormalizeSkill(skill));
                }
            }
            else
            {
                // إذا لم توجد نقاط، جرب تقسيم النص على الأسطر
                var lines = allSkillsText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var cleanLine = line.Trim().TrimStart('•', '-', '*', '•').Trim();
                    if (!string.IsNullOrWhiteSpace(cleanLine) && cleanLine.Length < 100 && !IsNonSkillPhrase(cleanLine))
                        candidateSkills.Add(NormalizeSkill(cleanLine));
                }
            }

            // 3. مطابقة ضبابية (Fuzzy Matching) مع قائمة المهارات المعروفة
            var finalSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var candidate in candidateSkills)
            {
                // تجاهل العبارات الطويلة جداً
                if (candidate.Length > 50) continue;

                // البحث عن أفضل تطابق مع مهارة معروفة
                var matches = KnownSkills.Keys
                    .Select(known => new { Known = known, Score = Fuzz.PartialRatio(candidate, known) })
                    .Where(m => m.Score > 70) // عتبة 70%
                    .OrderByDescending(m => m.Score)
                    .ToList();

                if (matches.Any())
                {
                    // أضف أفضل تطابق (أو يمكن إضافة الكل إذا كانت النتائج متقاربة)
                    finalSkills.Add(matches.First().Known);
                }
                else
                {
                    // إذا لم نجد تطابقاً، أضف المرشح نفسه (ربما مهارة غير معروفة)
                    finalSkills.Add(candidate);
                }
            }

            // 4. إضافة مهارات تقنية شائعة موجودة في النص (كخطوة احتياطية)
            var commonTech = KnownSkills.Keys.ToList();
            foreach (var tech in commonTech)
            {
                if (analysis.Contains(tech, StringComparison.OrdinalIgnoreCase))
                    finalSkills.Add(tech);
            }

            // 5. إزالة التكرارات والترتيب الأبجدي
            return finalSkills.OrderBy(s => s).ToList();
        }

        /// <summary>
        /// تطبيع العبارة (إزالة علامات الترقيم، تحويل الأحرف).
        /// </summary>
        private string NormalizeSkill(string skill)
        {
            // إزالة الرموز غير المرغوب فيها
            var normalized = Regex.Replace(skill, @"[^\w\s\.#\+\-]", "").Trim();
            return normalized;
        }

        /// <summary>
        /// التحقق مما إذا كانت العبارة لا تمثل مهارة.
        /// </summary>
        private bool IsNonSkillPhrase(string phrase)
        {
            var nonSkillKeywords = new[]
            {
                "experience", "recommend", "suggest", "improvement", "formatting", "readability",
                "education", "contact", "certification", "summary", "objective", "analysis",
                "section", "review", "provide", "include", "consider", "would", "should", "could",
                "using", "adding", "adding a", "based on", "according to", "please", "here", "step",
                "area", "evaluate", "present", "improve", "address", "enhance", "effectively",
                "available", "upon", "request", "reference", "references", "resume", "cv", "curriculum"
            };
            var lowerPhrase = phrase.ToLower();
            return nonSkillKeywords.Any(k => lowerPhrase.Contains(k)) || phrase.Length > 80;
        }

        private class AnalysisResponse
        {
            public string Analysis { get; set; } = string.Empty;
        }
    }
}