namespace palgineer.models2;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

    public class Engineer
    {
        [BsonId]

    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id { get; set; }
    public string name { get; set; }

    [BsonElement("email")]
        public string email { get; set; }

    [BsonElement("passwordHash")]
    public string passwordHash { get; set; }

    public string? status { get; set; }

    public string? experience { get; set; }
    
        public string? summary {  get; set; }

    public string? role { get; set; }
    public List<string> skills { get; set; } = new List<string>();
    public Dictionary<string,string>? links { get; set; }
        public string? avatar { get; set; }
        public string? resume { get; set; }
    public string ? resumeName { get; set; }


    
}
