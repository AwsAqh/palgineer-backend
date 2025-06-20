namespace palgineer.DTO
{
    public class EngineerFormDataDto
    {
        public string name { get; set; }
        public string email { get; set; }
        
        public string password { get; set; }

        public string? status { get; set; }

        public string? experience { get; set; }
        public string? summary { get; set; }

        public string[]? skills { get; set; }
        public Dictionary<string,string>? links { get; set; }

        public IFormFile? avatar { get; set; }
        public IFormFile? resume { get; set; }
    }

}
