using System.ComponentModel.DataAnnotations;

namespace BookMyCare.Models
{
    public class NurseModel
    {
        [Required]
        public string fullName { get; set; }
        [Required]
        public string dob { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string phone { get; set; }
        [Required]
        public string address { get; set; }
        [Required]  
        public string qualification { get; set; }
        [Required]
        public string experience { get; set; }    
        [Required]
        public string license { get; set; }
        [Required]
        public string specialization { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public IFormFile photoid { get; set; }
        [Required]
        public IFormFile licensephoto { get; set; }
        [Required]
        public IFormFile photograph { get; set;}
    }
    public class NurseLoginModel
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
    }
    public class NurseHomeModel
    {
        [Required]
        public string name { get; set; }
        [Required]
        public string profilePic { get; set; }
    }
    public class NurseList
    {
        [Required]
        public int id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string specialization { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public string photograph { get; set; }
    }
    public class NurseDetails
    {
        [Required]
        public string profilepic { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string phone { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public string speciality { get; set; }
        [Required]
        public string qualification { get; set; }
        [Required]
        public string license_number { get; set; }
        [Required]
        public string experience { get; set; }
        [Required]
        public string photo_id { get; set; }
        [Required]
        public string license_photo { get; set; }
        public double rating { get; set; }
        public int reviewsNumber { get; set; }
    }
    public class Nurse_Review
    {
        [Required]
        public int rating { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string review_text { get; set; }
        public DateTime review_date { get; set; }

    }
    public class NurseBooking
    {
        [Required]
        public DateOnly from_date { get; set; }
        [Required]
        public DateOnly to_date { get; set; }
        [Required]
        public TimeOnly from_time { get; set; }
        [Required]
        public TimeOnly to_time { get; set; }
        
        [Required]
        public IFormFile document { get; set; }
        [Required]
        public string house_no { get; set; }
        [Required]
        public string street { get; set; }
        [Required]
        public string landmark { get; set; }
        [Required]
        public string city { get; set; }
        [Required]
        public string state { get; set; }
        [Required]
        public string pincode { get; set; }
    }
    public class Bookings
    { 
        public int id { get; set; }
        public string photograph { get; set; }
        public string name { get; set; }
        public DateOnly start_date { get; set; }
        public TimeOnly start_time { get; set; }
        public DateOnly end_date { get; set; }
        public TimeOnly end_time { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string house_no { get; set; }
        public string street { get; set; }
        public string landmark { get; set; }
        public string pincode { get; set; }
        public string documents { get; set; }

    }
    public class UpdateProfile
    {
        public string photograph { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public DateOnly dob { get; set; }
        public string qualification { get; set; }
        public string experience { get; set; }
        public string license_number { get; set; }
        public string specialization { get; set; }
        public string address { get; set; }
        public IFormFile photoId { get; set; }
        public IFormFile license_photo { get; set; }
    }
    public class BookingHistory
    {
        public string photograph { get; set; }
        public string name { get; set; }
        public string service { get; set; }
        public DateOnly from_date { get; set; }
        public DateOnly to_date { get; set; }
        public TimeOnly from_time { get; set; }
        public TimeOnly to_time { get; set; }
        public string status { get; set; }

    }
    public class Support
    {
        public string name { get; set; }
        public string email { get; set; }
        public string message { get; set; }
    }
}
