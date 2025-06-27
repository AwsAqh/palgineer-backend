using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using palgineer.models2;

namespace palgineer.DTO
{

    public class EngineerResponseDTO {

  
        public string? _id { get; set; }
        public string name { get; set; }

        public string email { get; set; }

        public string? status { get; set; }

        public string? experience { get; set; }

        public string? summary { get; set; }

        public string? role { get; set; }
        public List<string> skills { get; set; } = new List<string>();
        public Dictionary<string, string>? links { get; set; }
        public string? avatar { get; set; }
        public string? resume { get; set; }



    }
    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public EngineerResponseDTO Engineer { get; set; } = null!;
    }
}
