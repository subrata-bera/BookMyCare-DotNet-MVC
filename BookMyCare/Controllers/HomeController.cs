using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;


namespace BookMyCare.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }
        private readonly ILogger<HomeController> _logger;

       

        public void GetUserDetails()
        {
            string user_id = HttpContext.Session.GetString("user_id");
            if (string.IsNullOrEmpty(user_id))
            {
                ViewBag.name = null;
                ViewBag.photo = null;
                return; // Stop if session is not set
            }
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "SELECT name, profile_pic FROM user_details WHERE ID = @user_id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@user_id", user_id);

            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                ViewBag.name = reader["name"].ToString();
                ViewBag.photo = reader["profile_pic"].ToString();
            }



        }
        public IActionResult Index()
        {
            string user_id = HttpContext.Session.GetString("user_id");
            if (!string.IsNullOrEmpty(user_id)) { 
            GetUserDetails();
        }
            return View();
        }
       

       
    }
}
