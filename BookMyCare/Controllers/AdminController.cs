using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using BookMyCare.Models;
using System.Reflection.Metadata.Ecma335;

namespace BookMyCare.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        private string GetAdminName()
        {
            string id = HttpContext.Session.GetString("id");
            if (string.IsNullOrEmpty(id)) return null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string q = "SELECT name FROM admin_details WHERE id = @id";
                SqlCommand cmd = new SqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return reader["name"].ToString();
                }
            }
            return null;
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(Login model)
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "SELECT id, name FROM admin_details WHERE email = @email AND password = @password";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@email", model.email);
            cmd.Parameters.AddWithValue("@password", model.password);
            SqlDataReader reader = cmd.ExecuteReader();
            if(reader.Read())
            {
                HttpContext.Session.SetString("id", reader["id"].ToString());
                return RedirectToAction("Home", "Admin");
            }
            else
            {
                ViewBag.Error = "Invalid login credentials.";
                return View();
            }
        }
        public IActionResult Home()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            string id = HttpContext.Session.GetString("id");
            if(id == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            ViewBag.AdminName = GetAdminName();
            return View();

        }
        public IActionResult Nurses()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            string id = HttpContext.Session.GetString("id");
            if(id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var pendingNurses = new List<PendingNurses>();
            var approvedNurses = new List<ApprovedNurses>();
            var rejectedNurses = new List<RejectedNurses>();
            SqlConnection conn = new SqlConnection(_connectionString);

            //Pending


            conn.Open();
            string q_pending = "SELECT ID, name, address, photograph FROM nurse_details WHERE status = 'pending' ORDER BY ID DESC";
            SqlCommand cmdPending = new SqlCommand(q_pending, conn);
            SqlDataReader readerPending = cmdPending.ExecuteReader();
            while(readerPending.Read())
            {
                pendingNurses.Add(new PendingNurses
                {
                    name = readerPending["name"].ToString(),
                    address = readerPending["address"].ToString(),
                    photograph = readerPending["photograph"].ToString(),
                    id = Convert.ToInt32(readerPending["id"])
                });
            }
            conn.Close();


            //Approved
            conn.Open();
            string q_approved = "SELECT ID, name, address, photograph FROM nurse_details WHERE status = 'approved' ORDER BY ID DESC";
            SqlCommand cmdApproved = new SqlCommand(q_approved, conn);

            SqlDataReader readerApproved = cmdApproved.ExecuteReader();

            while(readerApproved.Read())
            {
                

                approvedNurses.Add(new ApprovedNurses
                {
                    name = readerApproved["name"].ToString(),
                    address = readerApproved["address"].ToString(),
                    photograph = readerApproved["photograph"].ToString(),
                    id = Convert.ToInt32(readerApproved["id"])
                });

            }
            conn.Close();
            //Rejected nurses

            conn.Open();
            string q_rejected = "SELECT ID, name, address, photograph FROM nurse_details WHERE status = 'rejected' ORDER BY ID DESC";
            SqlCommand cmdRejected = new SqlCommand(q_rejected, conn);
            SqlDataReader readerRejected = cmdRejected.ExecuteReader();

            while (readerRejected.Read())
            {
                rejectedNurses.Add(new RejectedNurses { 
                    name = readerRejected["name"].ToString(),
                    address = readerRejected["address"].ToString(),
                    photograph = readerRejected["photograph"].ToString(),
                    id = Convert.ToInt32(readerRejected["id"])
                });
            }

            var model = new NurseTabViewModel
            {
                PendingNurses = pendingNurses,
                ApprovedNurses = approvedNurses,
                RejectedNurses = rejectedNurses
            };
            conn.Close();
            ViewBag.AdminName = GetAdminName();



            return View(model);
        }
        public IActionResult PendingNurse(int id)
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            string nurseid = HttpContext.Session.GetString("id");
            if (nurseid == null)
            {
                return RedirectToAction("Index", "Home");
            }


            PendingNurseDetails nurse = new PendingNurseDetails();
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "SELECT * FROM nurse_details WHERE ID = @id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@id", id);
            SqlDataReader reader = cmd.ExecuteReader();
            if(reader.Read())
            {
                nurse.photograph = reader["photograph"].ToString();
                nurse.name = reader["name"].ToString();
                nurse.email = reader["email"].ToString();
                nurse.mobile = reader["phone"].ToString();
                nurse.address = reader["address"].ToString();
                nurse.qualification = reader["qualification"].ToString();
                nurse.license_number = reader["license_number"].ToString();
                nurse.specialization = reader["specialization"].ToString();
                nurse.experience = reader["experience"].ToString();
                nurse.photo_id = reader["photo_id"].ToString();
                nurse.license_photo = reader["license_photo"].ToString();
            }
            ViewBag.NurseId = id;
            return View(nurse);
        }
        [HttpPost]
        public IActionResult ApproveNurse(int id)
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "UPDATE nurse_details SET status = 'approved' WHERE ID = @id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            conn.Close();

            return RedirectToAction("Nurses", "Admin");
           
        }
        public IActionResult CancelNurse(int id)
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "SELECT name FROM nurse_details WHERE ID = @id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@id", id);
            SqlDataReader reader = cmd.ExecuteReader();
            if(reader.Read())
            {
                ViewBag.name = reader["name"].ToString();
            }
            ViewBag.nurseId = id;
            return View();
        }
        [HttpPost]
        public IActionResult cancelNurse(int id)
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "UPDATE nurse_details SET status = 'rejected' WHERE ID = @id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            conn.Close();
            return RedirectToAction("Nurses", "Admin");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
