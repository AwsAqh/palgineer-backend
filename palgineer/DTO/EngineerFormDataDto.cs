namespace palgineer.DTO
{
    public class EngineerFormDataDto
    {
        public string name { get; set; }
        public string email { get; set; }
        
        public string password { get; set; }

        public string? status { get; set; }

        public string? experience { get; set; }

        public string? role { get; set; }
        public string? summary { get; set; }

        public List<string> skills { get; set; } = new List<string>();
        public Dictionary<string,string>? links { get; set; }

        public IFormFile? avatar { get; set; }
        public IFormFile? resume { get; set; }
    }

}
