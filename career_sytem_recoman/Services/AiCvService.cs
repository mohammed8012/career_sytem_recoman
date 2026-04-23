using System.Net.Http.Headers;
using System.Text;
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

        // قائمة المهارات المعروفة (يمكنك إضافة المزيد حسب الحاجة)
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
            { "Agile", "Methodology" },
            { "Scrum", "Methodology" }
        };

        public AiCvService(HttpClient httpClient, IConfiguration configuration, ILogger<AiCvService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _aiServiceUrl = configuration["AiService:Url"]
                ?? "https://ahmhnmh-resumeanalyzer.hf.space/analyze";
        }

        public async Task<CvAnalysisResultDto> GetFullAnalysisAsync(Stream fileStream, string fileName, string jobDescription)
        {
            try
            {
                // إرسال الطلب كـ multipart/form-data
                using var formData = new MultipartFormDataContent();
                using var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                formData.Add(fileContent, "file", fileName);
                formData.Add(new StringContent(jobDescription ?? ""), "job_description");

                var response = await _httpClient.PostAsync(_aiServiceUrl, formData);
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("AI Response: {Response}", responseBody);

                response.EnsureSuccessStatusCode();

                // استخراج حقل "analysis" من الاستجابة
                string analysisText = "No analysis returned.";
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                if (root.TryGetProperty("analysis", out var analysisProp))
                {
                    analysisText = analysisProp.GetString() ?? "No analysis returned.";
                }
                else if (root.TryGetProperty("output", out var outputProp))
                {
                    analysisText = outputProp.GetString() ?? "No analysis returned.";
                }
                else if (root.TryGetProperty("data", out var dataArray) && dataArray.ValueKind == JsonValueKind.Array && dataArray.GetArrayLength() > 0)
                {
                    analysisText = dataArray[0].GetString() ?? "No analysis returned.";
                }

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
                // في حالة الفشل، نعيد تحليلًا وهميًا لتجنب ظهور خطأ 500 للمستخدم
                return new CvAnalysisResultDto
                {
                    Analysis = "Analysis temporarily unavailable. Please try again later.",
                    Skills = new List<string>()
                };
            }
        }

        /// <summary>
        /// استخراج المهارات من نص التحليل باستخدام عدة طرق لضمان الاستقرار والشمولية.
        /// </summary>
        private List<string> ExtractSkillsFromAnalysis(string analysis)
        {
            var skills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(analysis))
                return new List<string>();

            // 1. البحث المباشر عن مهارات معروفة في النص بأكمله
            var commonTech = KnownSkills.Keys.ToList();
            foreach (var tech in commonTech)
            {
                if (analysis.Contains(tech, StringComparison.OrdinalIgnoreCase))
                    skills.Add(tech);
            }

            // 2. البحث عن أقسام المهارات المحددة (بأكثر من صيغة)
            var sectionPatterns = new[]
            {
                @"(?i)SKILLS?\s*:?\s*(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)TECHNICAL SKILLS?\s*:?\s*(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)PROGRAMMING LANGUAGES?\s*:?\s*(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)FRONT END TECHNOLOGIES?\s*:?\s*(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)BACK END TECHNOLOGIES?\s*:?\s*(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)DATABASES?\s*:?\s*(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)TOOLS?\s*:?\s*(.*?)(?=\n\n|\n[A-Z]|\Z)",
                @"(?i)FRAMEWORKS?\s*:?\s*(.*?)(?=\n\n|\n[A-Z]|\Z)"
            };

            string allSkillsText = "";
            foreach (var pattern in sectionPatterns)
            {
                var match = Regex.Match(analysis, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (match.Success)
                    allSkillsText += " " + match.Groups[1].Value;
            }

            if (!string.IsNullOrWhiteSpace(allSkillsText))
            {
                // تقسيم النص على المسافات والفواصل والأسطر الجديدة
                var words = Regex.Split(allSkillsText, @"[\s,;:\n\r]+")
                                 .Select(w => w.Trim())
                                 .Where(w => w.Length > 1 && w.Length < 50 && !IsNonSkillPhrase(w));
                foreach (var word in words)
                {
                    if (KnownSkills.ContainsKey(word))
                        skills.Add(word);
                    else
                    {
                        var match = KnownSkills.Keys.FirstOrDefault(k => string.Equals(k, word, StringComparison.OrdinalIgnoreCase));
                        if (match != null)
                            skills.Add(match);
                    }
                }
            }

            // 3. البحث عن عناوين نقطية (إذا كانت موجودة) كخطوة إضافية
            var bulletMatches = Regex.Matches(analysis, @"(?:^|\n)\s*[•\-\*•\d\.]\s*([A-Za-z0-9\s#\+\.]+?)(?=\n\s*[•\-\*•\d\.]|\n\n|\Z)", RegexOptions.Multiline);
            foreach (Match match in bulletMatches)
            {
                var skill = match.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(skill) && skill.Length < 50 && !IsNonSkillPhrase(skill))
                {
                    var matched = KnownSkills.Keys.FirstOrDefault(k => skill.Contains(k, StringComparison.OrdinalIgnoreCase));
                    if (matched != null)
                        skills.Add(matched);
                    else
                        skills.Add(skill);
                }
            }

            return skills.OrderBy(s => s).ToList();
        }

        /// <summary>
        /// تطبيع العبارة (إزالة علامات الترقيم، تحويل الأحرف).
        /// </summary>
        private string NormalizeSkill(string skill)
        {
            return Regex.Replace(skill, @"[^\w\s\.#\+\-]", "").Trim();
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
                "available", "upon", "request", "reference", "references", "resume", "cv", "curriculum",
                "and", "the", "for", "with", "this", "that", "are", "was", "were", "been", "not"
            };
            var lowerPhrase = phrase.ToLower();
            return nonSkillKeywords.Any(k => lowerPhrase.Contains(k)) || phrase.Length > 80;
        }
    }
}