namespace BookMyCare.Models
{
    public class AdminModel
    {
        public string name { get; set; }
    }

    public class Login
    {
        public string email { get; set; }
        public string password { get; set; }
    }
    public class NurseTabViewModel
    {
        public List<PendingNurses> PendingNurses { get; set; }
        public List<ApprovedNurses> ApprovedNurses { get; set; }
        public List<RejectedNurses> RejectedNurses { get; set; }
    }
   public class PendingNurses
    {
        public int id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string photograph { get; set; }
    }
    public class ApprovedNurses
    {
        public int id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string photograph { get; set; }
    }

    public class RejectedNurses
    {
        public int id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string photograph { get; set; }
    }
    public class PendingNurseDetails
    {
        public int id { get; set; }
        public string photograph { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string address { get; set; }
        public string qualification { get; set; }
        public string specialization { get; set; }
        public string license_number { get; set; }
        public string experience { get; set; }
        public string photo_id { get; set; }
        public string license_photo { get; set; }

    }
}
