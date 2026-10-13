 using System.ComponentModel.DataAnnotations;

 namespace Backend.DTOs.Users
 {
     public class CreateUserRequest
     {
        [Required, StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;
        [Required, StringLength(200, MinimumLength = 2)]
        public string Surname { get; set; } = string.Empty;
     }
 }