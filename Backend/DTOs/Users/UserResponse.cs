using System.Text.Json.Serialization;

namespace Backend.DTOs.Users
{
    public record UserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ActiveRegistrations {get; set;}
        
    }
}