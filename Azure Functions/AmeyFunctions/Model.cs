using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeyFunctions
{
    public class Response
    {
        public List<Candidate> candidates { get; set; }
    }

    public class Content
    {
        public List<Part> parts { get; set; }
        public string role { get; set; }
    }

    public class Candidate
    {
        public Content content { get; set; }
        public string finishReason { get; set; }
        public int index { get; set; }
        public List<SafetyRating> safetyRatings { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }

    public class SafetyRating
    {
        public string category { get; set; }
        public string probability { get; set; }
    }

    public class Recommendation
    {
       public Guid id { get; set; }
       public string created_at { get; set; }
       public string user_id { get; set;}
       public string mood_recommendation { get; set;}
       public string journal_recommendation { get; set; }
    }

    public class Articles
    {
        public string title { get; set; }
        public string url { get; set; }
        public string description { get; set; }

    }

    public class ArticlesRoot
    {
        public List<Articles> articles { get; set; }
    }
}
