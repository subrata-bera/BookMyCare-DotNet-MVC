using Microsoft.AspNetCore.Mvc;
using BookMyCare.Models;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Connections.Features;


namespace BookMyCare.Controllers
{
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public UserController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
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
            if(reader.Read())
            {
                ViewBag.name = reader["name"].ToString();
                ViewBag.photo = reader["profile_pic"].ToString();
            }



        }
        private IActionResult EnsureUserAuthenticated()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            var user_id = HttpContext.Session.GetString("user_id");
            if (string.IsNullOrEmpty(user_id))
            {
                return RedirectToAction("Login");
            }

            GetUserDetails(); // Custom method to get profile info
            return null;
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(UserModel model)
        {

            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Password not matched.";
                return View(model);
            }


            // Check profile pic size
            if (model.ProfilePic != null && model.ProfilePic.Length > 1 * 1024 * 1024)
            {
                ViewBag.Error = "Profile picture must be less than 1 MB.";

                return View(model);
            }


            SqlConnection conn = new SqlConnection(_connectionString);

            conn.Open();
            SqlCommand checkCmd = new SqlCommand("SELECT email FROM user_details WHERE email = @Email", conn);
            checkCmd.Parameters.AddWithValue("@Email", model.Email);
            var exists = checkCmd.ExecuteScalar();
            if (exists != null)
            {
                ViewBag.Error = "This email is already registered.";

                conn.Close();
                return View(model);
            }

            // Save image to wwwroot/uploads/nurse
            string? fileName = null;
            if (model.ProfilePic != null && model.ProfilePic.Length > 0)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePic.FileName);
                string path = Path.Combine("wwwroot/uploads/user", fileName);
                using var stream = new FileStream(path, FileMode.Create);
                await model.ProfilePic.CopyToAsync(stream);
            }

            // Hash password
            string hashedPassword = HashPassword(model.Password);

            // Insert into DB
            SqlCommand insertCmd = new SqlCommand("INSERT INTO user_details (name, email, phone, password, profile_pic) VALUES (@name, @email, @phone, @password, @profilepic)", conn);
            insertCmd.Parameters.AddWithValue("@name", model.FullName);
            insertCmd.Parameters.AddWithValue("@email", model.Email);
            insertCmd.Parameters.AddWithValue("@phone", model.Phone);
            insertCmd.Parameters.AddWithValue("@password", hashedPassword);
            insertCmd.Parameters.AddWithValue("@profilepic", (object?)fileName ?? DBNull.Value);
            insertCmd.ExecuteNonQuery();
            conn.Close();

            ViewBag.success = "Your account has been created successfully.";
            //return RedirectToAction("Register");
            return View();
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
        [HttpPost]
        public IActionResult Login(UserLoginModel model)
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string hashedPassword = HashPassword(model.password);
            SqlCommand cmd = new SqlCommand("SELECT ID, name FROM user_details WHERE email = @Email AND password = @Password", conn);
            cmd.Parameters.AddWithValue("@Email", model.email);
            cmd.Parameters.AddWithValue("@Password", hashedPassword);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                // ✅ Store both ID and Name in session
                HttpContext.Session.SetString("user_id", reader["ID"].ToString());
                HttpContext.Session.SetString("user_name", reader["name"].ToString());

                conn.Close(); // ✅ Close here before return
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Invalid login credentials.";
                conn.Close(); // ✅ Close here too
                return View();
            }
        }
        
        public IActionResult Home()
        {
            var result = EnsureUserAuthenticated();
            if (result != null)
            {
                return result;
            }

           
            return View();
        }

        public IActionResult UpdateProfile()
        {
            var result = EnsureUserAuthenticated();
            if (result != null)
            {
                return result;
            }
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            UserModel model = new UserModel();
            string user_id = HttpContext.Session.GetString("user_id");
            string q = "SELECT name, email, phone, profile_pic FROM user_details WHERE ID = @user_id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@user_id", user_id);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                model.FullName = reader["name"].ToString();
                model.Email = reader["email"].ToString();
                model.Phone = reader["phone"].ToString();
                model.photograph = reader["profile_pic"].ToString();
            }
            conn.Close();
            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> submitUpdate(UserModel model)
        {
            var result = EnsureUserAuthenticated();
            if (result != null)
            {
                return result;
            }
            string user_id = HttpContext.Session.GetString("user_id");

            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string getQuery = "SELECT profile_pic FROM user_details WHERE ID = @id";
            SqlCommand getCmd = new SqlCommand(getQuery, conn);
            getCmd.Parameters.AddWithValue("@id", user_id);
            SqlDataReader reader = getCmd.ExecuteReader();
            string oldPhotograph = "";
            if (reader.Read())
            {
               
                oldPhotograph = reader["profile_pic"]?.ToString();
            }
            reader.Close();
            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/user");
            
            string photographName = oldPhotograph;

            if (model.ProfilePic != null)
            {
                photographName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePic.FileName);
                string fullPath = Path.Combine(uploadPath, photographName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ProfilePic.CopyToAsync(stream);
                }
                if (!string.IsNullOrEmpty(oldPhotograph))
                {
                    var oldPath = Path.Combine(uploadPath, oldPhotograph);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
            }



           
            string q = "UPDATE user_details set name = @name, email = @email, profile_pic = @profile_pic WHERE ID = @user_id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@name", model.FullName);
            cmd.Parameters.AddWithValue("@email", model.Email);
            cmd.Parameters.AddWithValue("@phone", model.Phone);
            cmd.Parameters.AddWithValue("profile_pic", photographName);
            cmd.Parameters.AddWithValue("@user_id", user_id);   
            cmd.ExecuteNonQuery();
            conn.Close();
            TempData["success"] = "Details updated successfully.";
            return RedirectToAction("Home");

        }
        public IActionResult BookingHistory()
        {
            var result = EnsureUserAuthenticated();
            if (result != null)
            {
                return result;
            }
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            List<BookingHistory> history = new List<BookingHistory>();
            string user_id = HttpContext.Session.GetString("user_id");
            string q = "SELECT n.photograph, n.name, b.from_date, b.to_date, b.from_time, b.to_time, b.status FROM nurse_details as n JOIN nurse_bookings as b ON n.ID = b.nurse_id WHERE b.user_id = @user_id ORDER BY b.id DESC";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@user_id", user_id);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                history.Add(new BookingHistory
                {
                    photograph = reader["photograph"].ToString(),
                    name = reader["name"].ToString(),
                    from_date = DateOnly.FromDateTime(Convert.ToDateTime(reader["from_date"])),
                    to_date = DateOnly.FromDateTime(Convert.ToDateTime(reader["from_date"])),
                    from_time = TimeOnly.FromTimeSpan((TimeSpan)reader["from_time"]),
                    to_time = TimeOnly.FromTimeSpan((TimeSpan)reader["to_time"]),
                    status = reader["status"].ToString()
                });
            }
            return View(history);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    
    }

}
