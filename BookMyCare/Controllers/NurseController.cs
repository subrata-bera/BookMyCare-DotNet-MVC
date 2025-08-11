using BookMyCare.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace BookMyCare.Controllers
{
    public class NurseController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public NurseController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public void GetDetails()
        {
            string nurse_id = HttpContext.Session.GetString("id");
            if(string.IsNullOrEmpty(nurse_id))
            {
                return;
            }
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "SELECT photograph, name FROM nurse_details WHERE ID = @id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@id", nurse_id);
            SqlDataReader reader = cmd.ExecuteReader();
            if(reader.Read())
            {
                ViewBag.photo = reader["photograph"].ToString();
                ViewBag.name = reader["name"].ToString();
            }
            conn.Close();

        }
        private IActionResult EnsureNurseAuthenticated()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            var nurse_id = HttpContext.Session.GetString("id");
            if (string.IsNullOrEmpty(nurse_id))
            {
                return RedirectToAction("Login");
            }

            GetDetails(); // Custom method to get profile info
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
        //Hash password
        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create()) {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
                
            }
        }
        [HttpPost]
        public async Task<IActionResult> Register(NurseModel model)
        {
            if (model.photoid != null && model.licensephoto != null && model.photograph != null &&
                model.photoid.Length > 1 * 1024 * 1024 && model.licensephoto.Length > 1 * 1024 * 1024 && model.photograph.Length > 1 * 1024 * 1024)
            {
                ViewBag.Error = "Uploaded file should not exceed 1MB.";
                return View();
            }
            else if (model.photoid == null || model.licensephoto == null || model.photograph == null)
            {
                ViewBag.Error = "Uploaded file cannot be null.";
                return View();
            }
            else
            {
                // Save files to wwwroot/uploads
                string photoidName = Guid.NewGuid().ToString() + Path.GetExtension(model.photoid.FileName);
                string licensephotoName = Guid.NewGuid().ToString() + Path.GetExtension(model.licensephoto.FileName);
                string photographName = Guid.NewGuid().ToString() + Path.GetExtension(model.photograph.FileName);

                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/nurse");

                using (var stream = new FileStream(Path.Combine(uploadPath, photoidName), FileMode.Create))
                {
                    await model.photoid.CopyToAsync(stream);
                }
                using (var stream = new FileStream(Path.Combine(uploadPath, licensephotoName), FileMode.Create))
                {
                    await model.licensephoto.CopyToAsync(stream);
                }
                using (var stream = new FileStream(Path.Combine(uploadPath, photographName), FileMode.Create))
                {
                    await model.photograph.CopyToAsync(stream);
                }

                string hashedPassword = HashPassword(model.Password);

                SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

                // 🔁 Check if email exists and get status
                string checkQuery = "SELECT status FROM nurse_details WHERE email = @Email";
                SqlCommand cmdChk = new SqlCommand(checkQuery, conn);
                cmdChk.Parameters.AddWithValue("@Email", model.email);
                var status = cmdChk.ExecuteScalar()?.ToString();

                if (status == "rejected") // 🔁 Allow update
                {
                    // 🔁 Update existing rejected record
                    string updateQuery = @"
                UPDATE nurse_details SET
                    name = @name, dob = @dob, phone = @phone, address = @address,
                    qualification = @qualification, experience = @experience,
                    license_number = @license, specialization = @specialization,
                    password = @password, photo_id = @photoid,
                    license_photo = @license_photo, photograph = @photograph,
                    status = 'pending'
                WHERE email = @Email";

                    SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn);
                    cmdUpdate.Parameters.AddWithValue("@name", model.fullName);
                    cmdUpdate.Parameters.AddWithValue("@dob", model.dob);
                    cmdUpdate.Parameters.AddWithValue("@phone", model.phone);
                    cmdUpdate.Parameters.AddWithValue("@address", model.address);
                    cmdUpdate.Parameters.AddWithValue("@qualification", model.qualification);
                    cmdUpdate.Parameters.AddWithValue("@experience", model.experience);
                    cmdUpdate.Parameters.AddWithValue("@license", model.license);
                    cmdUpdate.Parameters.AddWithValue("@specialization", model.specialization);
                    cmdUpdate.Parameters.AddWithValue("@password", hashedPassword);
                    cmdUpdate.Parameters.AddWithValue("@photoid", photoidName);
                    cmdUpdate.Parameters.AddWithValue("@license_photo", licensephotoName);
                    cmdUpdate.Parameters.AddWithValue("@photograph", photographName);
                    cmdUpdate.Parameters.AddWithValue("@Email", model.email);

                    cmdUpdate.ExecuteNonQuery();
                    ViewBag.success = "You have resubmitted your registration details. Please wait for approval again.";
                }
                else if (status != null) // 🔁 Already exists, not rejected
                {
                    ViewBag.Error = "This email is already registered.";
                    return View();
                }
                else // 🔁 Insert new
                {
                    SqlCommand cmd = new SqlCommand(@"
                INSERT INTO nurse_details
                (name, dob, email, phone, address, qualification, experience, license_number,
                 specialization, password, photo_id, license_photo, photograph, status)
                VALUES (@name, @dob, @Email, @phone, @address, @qualification, @experience,
                        @license, @specialization, @password, @photoid, @license_photo, @photograph, 'pending')", conn);

                    cmd.Parameters.AddWithValue("@name", model.fullName);
                    cmd.Parameters.AddWithValue("@dob", model.dob);
                    cmd.Parameters.AddWithValue("@Email", model.email);
                    cmd.Parameters.AddWithValue("@phone", model.phone);
                    cmd.Parameters.AddWithValue("@address", model.address);
                    cmd.Parameters.AddWithValue("@qualification", model.qualification);
                    cmd.Parameters.AddWithValue("@experience", model.experience);
                    cmd.Parameters.AddWithValue("@license", model.license);
                    cmd.Parameters.AddWithValue("@specialization", model.specialization);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    cmd.Parameters.AddWithValue("@photoid", photoidName);
                    cmd.Parameters.AddWithValue("@license_photo", licensephotoName);
                    cmd.Parameters.AddWithValue("@photograph", photographName);

                    cmd.ExecuteNonQuery();
                    ViewBag.success = "Your account has been registered successfully.";
                }

                conn.Close();
                return View();
            }
        }

        [HttpPost]
        public IActionResult Login(NurseLoginModel model)
        {
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string password = HashPassword(model.password);

            // Step 1: Check if the email and password match any nurse
            string query = "SELECT ID, status FROM nurse_details WHERE email = @email AND password = @password";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", model.email);
            cmd.Parameters.AddWithValue("@password", password);

            SqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                string status = reader["status"].ToString();

                if (status == "pending")
                {
                    ViewBag.Error = "Your registration is still pending. Please wait for admin approval.";
                    conn.Close();
                    return View();
                }

                if (status == "rejected")
                {
                    ViewBag.Error = "Your registration has been rejected.";
                    conn.Close();
                    return View();
                }

                // Approved nurse
                HttpContext.Session.SetString("id", reader["ID"].ToString());
                conn.Close();
                return RedirectToAction("Home");
            }
            else
            {
                ViewBag.Error = "Invalid login credentials.";
                conn.Close();
                return View();
            }
        }


        public IActionResult Home()
        {
            var result = EnsureNurseAuthenticated();
            if (result != null)
            {
                return result;
            }

            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear all session data
            return RedirectToAction("Index","Home"); // Redirect to the login page
        }

        public IActionResult Bookings()
        {
            var result = EnsureNurseAuthenticated();
            if (result != null)
            {
                return result;
            }
            List<Bookings> bookings = new List<Bookings>();
            string nurse_id = HttpContext.Session.GetString("id");
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "SELECT nurse_bookings.id, user_details.profile_pic, user_details.name, nurse_bookings.from_date, nurse_bookings.from_time, nurse_bookings.city, nurse_bookings.state FROM nurse_bookings JOIN user_details ON nurse_bookings.user_id = user_details.id WHERE nurse_bookings.nurse_id = @nurse_id AND nurse_bookings.status = 'pending' ORDER BY nurse_bookings.ID DESC";
            string q_count = "SELECT COUNT(*) FROM nurse_bookings WHERE nurse_id = @nurse_id AND status = 'pending'";
            SqlCommand cmd = new SqlCommand(q, conn);
            SqlCommand cmd_count = new SqlCommand(q_count, conn);
            cmd.Parameters.AddWithValue("@nurse_id", nurse_id);
            cmd_count.Parameters.AddWithValue("@nurse_id", nurse_id);
            ViewBag.count = Convert.ToInt32(cmd_count.ExecuteScalar());
            
            SqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                bookings.Add(new Bookings
                {
                    id = Convert.ToInt32(reader["id"].ToString()),
                    photograph = reader["profile_pic"].ToString(),
                    name = reader["name"].ToString(),
                    start_date = DateOnly.FromDateTime(Convert.ToDateTime(reader["from_date"])),
                    start_time = TimeOnly.FromTimeSpan((TimeSpan)reader["from_time"]),

                    city = reader["city"].ToString(),
                    state = reader["state"].ToString(),
                    
                });

            }
            conn.Close();
            return View(bookings);
        }
        public IActionResult UpdateProfile()
        {
            var result = EnsureNurseAuthenticated();
            if (result != null)
            {
                return result;
            }
            UpdateProfile updateprofile = new UpdateProfile();
            string nurse_id = HttpContext.Session.GetString("id");
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "SELECT * FROM nurse_details WHERE id = @nurse_id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@nurse_id", nurse_id);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                updateprofile.photograph = reader["photograph"].ToString();
                updateprofile.name = reader["name"].ToString();
                updateprofile.email = reader["email"].ToString();
                updateprofile.phone = reader["phone"].ToString();
                updateprofile.dob = DateOnly.FromDateTime(Convert.ToDateTime(reader["dob"]));
                updateprofile.qualification = reader["qualification"].ToString();
                updateprofile.experience = reader["experience"].ToString();
                updateprofile.license_number = reader["license_number"].ToString();
                updateprofile.specialization = reader["specialization"].ToString();
                updateprofile.address = reader["address"].ToString();

            }
            conn.Close();
            ViewBag.nurseId = nurse_id;

            return View(updateprofile);
        }
        [HttpPost]
        
public async Task<IActionResult> submitUpdate(NurseModel model)
        {
            string nurse_id = HttpContext.Session.GetString("id");
            var result = EnsureNurseAuthenticated();
            if (result != null)
            {
                return result;
            }

            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            // Step 1: Get existing file names from DB
            string getQuery = "SELECT photo_id, license_photo, photograph FROM nurse_details WHERE ID = @id";
            SqlCommand getCmd = new SqlCommand(getQuery, conn);
            getCmd.Parameters.AddWithValue("@id", nurse_id);
            SqlDataReader reader = getCmd.ExecuteReader();
            string oldPhotoId = "", oldLicensePhoto = "", oldPhotograph = "";
            if (reader.Read())
            {
                oldPhotoId = reader["photo_id"]?.ToString();
                oldLicensePhoto = reader["license_photo"]?.ToString();
                oldPhotograph = reader["photograph"]?.ToString();
            }
            reader.Close();

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/nurse");
            string photoIdName = oldPhotoId;
            string licensePhotoName = oldLicensePhoto;
            string photographName = oldPhotograph;

            // Step 2: Save new files and delete old ones (if uploaded)
            if (model.photoid != null)
            {
                photoIdName = Guid.NewGuid().ToString() + Path.GetExtension(model.photoid.FileName);
                string fullPath = Path.Combine(uploadPath, photoIdName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.photoid.CopyToAsync(stream);
                }
                if (!string.IsNullOrEmpty(oldPhotoId))
                {
                    var oldPath = Path.Combine(uploadPath, oldPhotoId);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
            }

            if (model.licensephoto != null)
            {
                licensePhotoName = Guid.NewGuid().ToString() + Path.GetExtension(model.licensephoto.FileName);
                string fullPath = Path.Combine(uploadPath, licensePhotoName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.licensephoto.CopyToAsync(stream);
                }
                if (!string.IsNullOrEmpty(oldLicensePhoto))
                {
                    var oldPath = Path.Combine(uploadPath, oldLicensePhoto);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
            }

            if (model.photograph != null)
            {
                photographName = Guid.NewGuid().ToString() + Path.GetExtension(model.photograph.FileName);
                string fullPath = Path.Combine(uploadPath, photographName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.photograph.CopyToAsync(stream);
                }
                if (!string.IsNullOrEmpty(oldPhotograph))
                {
                    var oldPath = Path.Combine(uploadPath, oldPhotograph);
                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                }
            }

            // Step 3: Update database
            string updateQuery = @"
        UPDATE nurse_details SET
            name = @name,
            dob = @dob,
email = @email,
            phone = @phone,
            address = @address,
            qualification = @qualification,
            experience = @experience,
            license_number = @license,
            specialization = @specialization,
            photo_id = @photoid,
            license_photo = @licensephoto,
            photograph = @photograph,
            status = 'pending'
        WHERE ID = @id";

            SqlCommand cmd = new SqlCommand(updateQuery, conn);
            cmd.Parameters.AddWithValue("@name", model.fullName);
            cmd.Parameters.AddWithValue("@dob", model.dob);
            cmd.Parameters.AddWithValue("@phone", model.phone);
            cmd.Parameters.AddWithValue("@email", model.email);
            cmd.Parameters.AddWithValue("@address", model.address);
            cmd.Parameters.AddWithValue("@qualification", model.qualification);
            cmd.Parameters.AddWithValue("@experience", model.experience);
            cmd.Parameters.AddWithValue("@license", model.license);
            cmd.Parameters.AddWithValue("@specialization", model.specialization);
            cmd.Parameters.AddWithValue("@photoid", photoIdName);
            cmd.Parameters.AddWithValue("@licensephoto", licensePhotoName);
            cmd.Parameters.AddWithValue("@photograph", photographName);
            cmd.Parameters.AddWithValue("@id", nurse_id);

            cmd.ExecuteNonQuery();
            conn.Close();
            HttpContext.Session.Clear();
            TempData["success"] = "Profile updated successfully. Awaiting admin approval.";
            return RedirectToAction("Index", "Home");
        }

        
        public IActionResult BookingHistory()
        {
            
            var result = EnsureNurseAuthenticated();
            if (result != null)
            {
                return result;
            }
            List<BookingHistory> bookingHistory = new List<BookingHistory>();
            string nurse_id = HttpContext.Session.GetString("id");
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "SELECT u.profile_pic, u.name, b.from_date, b.to_date, b.from_time, b.to_time, b.status FROM user_details as u JOIN nurse_bookings as b ON u.ID = b.user_id WHERE b.nurse_id = @nurse_id AND b.status != 'pending'";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@nurse_id", nurse_id);
            SqlDataReader reader = cmd.ExecuteReader();
            while(reader.Read())
            {
                bookingHistory.Add(new BookingHistory
                {
                    photograph = reader["profile_pic"].ToString(),
                    name = reader["name"].ToString(),
                    from_date = DateOnly.FromDateTime(Convert.ToDateTime(reader["from_date"])),
                    to_date = DateOnly.FromDateTime(Convert.ToDateTime(reader["from_date"])),
                    from_time = TimeOnly.FromTimeSpan((TimeSpan)reader["from_time"]),
                    to_time = TimeOnly.FromTimeSpan((TimeSpan)reader["to_time"]),
                    status = reader["status"].ToString()

                });
            }
            return View(bookingHistory); 

        }
        public IActionResult Support()
        {
            var result = EnsureNurseAuthenticated();
            if (result != null)
            {
                return result;
            }
            Support model = new Support();
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string nurse_id = HttpContext.Session.GetString("id");
            string q = "SELECT name, email FROM nurse_details WHERE ID = @nurse_id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@nurse_id", nurse_id);
            SqlDataReader reader = cmd.ExecuteReader();
            if(reader.Read())
            {
                model.name = reader["name"].ToString();
                model.email = reader["email"].ToString();
            }
            string phoneNumber = _configuration["ContactInfo:PhoneNumber"];
            ViewBag.PhoneNumber = phoneNumber;
            
            return View(model);
        }
        [HttpPost]
        public IActionResult submitTicket(Support model)
        {
            var result = EnsureNurseAuthenticated();
            if (result != null)
            {
                return result;
            }
             SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "INSERT INTO support (name, email, message) VALUES (@name, @email, @message)";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@name", model.name);
            cmd.Parameters.AddWithValue("@email", model.email);
            cmd.Parameters.AddWithValue("@message", model.message);
            cmd.ExecuteNonQuery();
            conn.Close();
            TempData["success"] = "Thank you for reaching out. Our support team will review your issue and get back to you shortly.";
            return RedirectToAction("Support");


        }

        public IActionResult BookingDetails(int id)
        {
            int booking_id = id;
            var result = EnsureNurseAuthenticated();
            string nurse_id = HttpContext.Session.GetString("id");
            if (result != null)
            {
                return result;
            }
            if(booking_id <=0)
            {
                return RedirectToAction("Bookings");
            }
            Bookings bookings = new Bookings();
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "select u.profile_pic,u.name,b.documents, b.from_date, b.from_time, b.to_date, b.to_time, b.house_no, b.street, b.landmark, b.city, b.state, b.pincode from user_details as u JOIN nurse_bookings as b ON u.ID = b.user_id WHERE b.ID = @booking_id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@booking_id", booking_id);
            SqlDataReader reader = cmd.ExecuteReader();
            if(reader.Read())
            {
                bookings.photograph = reader["profile_pic"].ToString();
                bookings.name = reader["name"].ToString();
                bookings.start_date = DateOnly.FromDateTime(Convert.ToDateTime(reader["from_date"]));
                bookings.end_date = DateOnly.FromDateTime(Convert.ToDateTime(reader["to_date"]));
                bookings.start_time = TimeOnly.FromTimeSpan((TimeSpan)reader["from_time"]);
                bookings.end_time = TimeOnly.FromTimeSpan((TimeSpan)reader["to_time"]);
                bookings.house_no = reader["house_no"].ToString();
                bookings.street = reader["street"].ToString();
                bookings.landmark = reader["landmark"].ToString();
                bookings.city = reader["city"].ToString();
                bookings.state = reader["state"].ToString();
                bookings.pincode = reader["pincode"].ToString();
                bookings.documents = reader["documents"].ToString();
            }
            conn.Close();
            ViewBag.id = booking_id;
            return View(bookings);
        }
        [HttpPost]
       public IActionResult completeBooking(int id)
        {
            int booking_id = id;
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "UPDATE nurse_bookings SET status = 'completed' WHERE id = @id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@id", booking_id);
            cmd.ExecuteNonQuery();
            conn.Close();
            TempData["Success"] = "Booking marked as completed successfully.";
            return RedirectToAction("Bookings");

        }
        [HttpPost]
        public IActionResult RejectBooking(int id)
        {
            int booking_id = id;
            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            string q = "UPDATE nurse_bookings SET status = 'rejected' WHERE id = @id";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@id", booking_id);
            cmd.ExecuteNonQuery();
            conn.Close();
            TempData["reject"] = "Booking is successfully Rejected.";
            return RedirectToAction("Bookings");


        }
        

    }
}
