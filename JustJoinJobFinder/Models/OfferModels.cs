using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace JustJoinJobFinder.Models
{
    public class JjitResponse
    {
        [JsonPropertyName("data")]
        public List<JjitOffer>? Data { get; set; }
    }

    public class JjitOffer
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }


        [JsonPropertyName("slug")]
        public string? Slug { get; set; }
    }
    
}
