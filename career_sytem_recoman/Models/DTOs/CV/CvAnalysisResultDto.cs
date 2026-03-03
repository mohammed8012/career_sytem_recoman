using System.Collections.Generic;

namespace career_sytem_recoman.Models.DTOs.CV
{
    public class CvAnalysisResultDto
    {
        public string Analysis { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
    }
}