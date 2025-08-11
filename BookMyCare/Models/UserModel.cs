using System.ComponentModel.DataAnnotations;

namespace BookMyCare.Models
{
    public class UserModel
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public IFormFile ProfilePic { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }

        public string photograph { get; set; }
    }

    public class UserLoginModel
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
    }

    public class UserHomeModel
    {
        public string Name { get; set; }
        public string ProfilePic { get; set; }
    }

   


}
