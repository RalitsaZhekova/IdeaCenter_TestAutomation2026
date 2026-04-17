using System;
using System.Text.Json.Serialization;

namespace ExamPrepIdeaCenter.Models
{
    internal class IdeaDTO
    {
        [JsonPropertyName("title")]
        public required string Title { get; set; }

        [JsonPropertyName("description")]
        public required string Description { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
