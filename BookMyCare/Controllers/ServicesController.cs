using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using BookMyCare.Models;


namespace BookMyCare.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public ServicesController(IConfiguration configuration)
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
            if (reader.Read())
            {
                ViewBag.name = reader["name"].ToString();
                ViewBag.photo = reader["profile_pic"].ToString();
            }



        }
        public IActionResult Index()
        {
            GetUserDetails();
            return View();
        }

       

        public IActionResult Nurses()
        {
            GetUserDetails();
            List<NurseList> nurses = new List<NurseList>();
            SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();
            string q = "SELECT ID, name, specialization, address, photograph FROM nurse_details WHERE status = 'approved' ORDER BY ID DESC";
            SqlCommand cmd = new SqlCommand(q, conn);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                nurses.Add(new NurseList
                {
                    name = reader["name"].ToString(),
                    specialization = reader["specialization"].ToString(),
                    address = reader["address"].ToString(),
                    photograph = reader["photograph"].ToString(),
                    id = Convert.ToInt32(reader["ID"]),

                }
                    );
            }






            return View(nurses);
        }
        public IActionResult Nurse(int? id)
        {
            if(id == null)
            {
                return RedirectToAction("Nurses", "Services");
            }
            GetUserDetails(); //for showing user name and photo

            NurseDetails model = new NurseDetails();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM nurse_details WHERE ID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    model.profilepic = reader["photograph"].ToString();
                    model.name = reader["name"].ToString();
                    model.phone = reader["phone"].ToString();
                    model.email = reader["email"].ToString();
                    model.address = reader["address"].ToString();
                    model.speciality = reader["specialization"].ToString();
                    model.license_number = reader["license_number"].ToString();
                    model.qualification = reader["qualification"].ToString();
                    model.experience = reader["experience"].ToString();
                    model.photo_id = reader["photo_id"].ToString();
                    model.license_photo = reader["license_photo"].ToString();
                }
                conn.Close();
            }
            //  Fetch Reviews
            List<Nurse_Review> reviews = new List<Nurse_Review>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT name, rating, review_text, review_date FROM nurse_reviews WHERE nurse_id = @id ORDER BY review_date DESC", conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reviews.Add(new Nurse_Review
                    {
                        name = reader["name"].ToString(),
                        rating = Convert.ToInt32(reader["rating"]),
                        review_text = reader["review_text"].ToString(),
                        review_date = Convert.ToDateTime(reader["review_date"])
                    });
                }
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string q_rating = "SELECT AVG(CAST(rating AS FLOAT)) FROM nurse_reviews WHERE nurse_id = @nurse_id";
                string q_numberofreviews = "SELECT COUNT(*) FROM nurse_reviews WHERE nurse_id = @nurse_id";
                SqlCommand cmd = new SqlCommand(q_rating, conn);
                SqlCommand cmd_reviewsNumber = new SqlCommand(q_numberofreviews, conn);
                cmd.Parameters.AddWithValue("@nurse_id", id);
                cmd_reviewsNumber.Parameters.AddWithValue("@nurse_id", id);
                object result = cmd.ExecuteScalar();
                object result_numberofreviews = cmd_reviewsNumber.ExecuteScalar();
                if (result != DBNull.Value && result_numberofreviews != DBNull.Value)
                {
                    double avgRating = Convert.ToDouble(result);
                    model.rating = Math.Round(avgRating, 1);

                    model.reviewsNumber = Convert.ToInt32(result_numberofreviews);
                }
                
                
            }
                ViewBag.Reviews = reviews;
            ViewBag.NurseId = id;
            return View(model);
        }
        [HttpPost]
        public IActionResult submitReview(Nurse_Review model, int nurseId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"INSERT INTO nurse_reviews (nurse_id, name, rating, review_text) VALUES (@nurse_id, @name, @rating, @review_text)", conn);
                cmd.Parameters.AddWithValue("@nurse_id", nurseId);
                cmd.Parameters.AddWithValue("@name", model.name);
                cmd.Parameters.AddWithValue("@rating", model.rating);
                cmd.Parameters.AddWithValue("@review_text", model.review_text);
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Nurse", new { id = nurseId });
        }
        public IActionResult Booking(int? id)
        {
            GetUserDetails();

            string user_id = HttpContext.Session.GetString("user_id");
            if(string.IsNullOrEmpty(user_id))
                {
                return RedirectToAction("Login", "User");
            }
            if(id == null)
            {
                return RedirectToAction("Nurses", "Services");
            }
            ViewBag.nurseId = id;
            return View();
        }
           [HttpPost]
        public async Task <IActionResult> Booking(NurseBooking model, int id)
        {
            GetUserDetails();
            int nurse_id = id;
            int user_id = Convert.ToInt32(HttpContext.Session.GetString("user_id"));

            if(model.document == null)
            {
                ViewBag.Error = "Document must must have to upload";
                ViewBag.nurseId = id;
                return View();
            }

            if (model.document != null && model.document.Length > 1 * 1024 * 1024)
            {
                ViewBag.Error = "Uploaded document must be 1MB or less";
                ViewBag.nurseId = id;
                return View();

            }
            

            SqlConnection conn = new SqlConnection(_connectionString);
                conn.Open();

            string q_check = "SELECT COUNT(*) FROM nurse_bookings WHERE nurse_id = @nurse_id   AND ( (from_date <= @to_date AND to_date >= @from_date))";
            SqlCommand cmdChk = new SqlCommand(q_check, conn);
            cmdChk.Parameters.AddWithValue("@nurse_id", nurse_id);
            cmdChk.Parameters.AddWithValue("@from_date", model.from_date);
            cmdChk.Parameters.AddWithValue("@to_date", model.to_date);

            cmdChk.Parameters.AddWithValue("@from_time", model.from_time);
            cmdChk.Parameters.AddWithValue("@to_time", model.to_time);
            var exist = Convert.ToInt32(cmdChk.ExecuteScalar());

            if (exist > 0)
            {
                ViewBag.Error = "Nurse is already booked at this time!";
                ViewBag.nurseId = id;
                return View();
            }

            string? filename = null;
            if(model.document != null && model.document.Length >0)
            {
                filename = Guid.NewGuid().ToString() + Path.GetExtension(model.document.FileName);
                string path = Path.Combine("wwwroot/uploads/bookings", filename);
                using var stream = new FileStream(path, FileMode.Create);
                await model.document.CopyToAsync(stream);
            }



            string q = "INSERT INTO nurse_bookings (user_id, nurse_id, from_date, to_date, from_time, to_time, documents, house_no, street, landmark, city, state, pincode) VALUES (@user_id, @nurse_id, @from_date, @to_date, @from_time, @to_time, @document, @house_no, @street, @landmark, @city, @state, @pincode)";
            SqlCommand cmd = new SqlCommand(q, conn);
            cmd.Parameters.AddWithValue("@user_id", user_id);
            cmd.Parameters.AddWithValue("@nurse_id", nurse_id);

            cmd.Parameters.AddWithValue("@from_date", model.from_date);
            cmd.Parameters.AddWithValue("@to_date", model.to_date);

            cmd.Parameters.AddWithValue("@from_time", model.from_time);
            cmd.Parameters.AddWithValue("@to_time", model.to_time);

            cmd.Parameters.AddWithValue("@document", filename);

            cmd.Parameters.AddWithValue("@house_no", model.house_no);
            cmd.Parameters.AddWithValue("@street", model.street);
            cmd.Parameters.AddWithValue("@landmark",
                string.IsNullOrEmpty(model.landmark) ? (object)DBNull.Value : model.landmark);
            cmd.Parameters.AddWithValue("@city", model.city);
            cmd.Parameters.AddWithValue("@state", model.state);
            cmd.Parameters.AddWithValue("@pincode", model.pincode);


            cmd.ExecuteNonQuery();
            conn.Close();

            ViewBag.success = "Booking successfull.";
            ViewBag.nurseId = id;
            return View();
        }

      
 


    }
}
