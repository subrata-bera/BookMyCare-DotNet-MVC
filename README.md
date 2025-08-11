# BookMyCare

## Project Overview
BookMyCare is a nurse booking and profile management platform that allows hospitals, clinics, and individuals to easily book professional nurses for healthcare services. Nurses can register, update their profiles, manage bookings, upload documents, and communicate with support. The system includes an admin approval mechanism for nurse profiles to ensure authenticity and quality of service.

## Features

### 1. User Roles
- **Admin**:  
  - Approves nurse registrations and profile updates.  
  - Manages bookings, user accounts, and platform settings.  

- **Nurse**:  
  - Registers and uploads profile details/documents.  
  - Manages availability, accepts bookings, and tracks booking history.  

- **Client (Patient / Organization)**:  
  - Searches and books available nurses for required services.  
  - Views booking history and communicates via support.  

### 2. Core Functionalities
- **Secure Authentication**: Login and registration for all user roles with session management.  
- **Nurse Profile Management**:  
  - Upload profile picture and documents (ID, License, Certificates).  
  - Edit personal and professional details.  
  - Admin approval required after profile updates.  
- **Booking System**:  
  - Clients can request nurse services.  
  - Nurses can view and accept/reject bookings.  
- **Booking History**: View all completed bookings with key details.  
- **Document Handling**: Files stored in `wwwroot/uploads/nurse`. Old files automatically removed upon updates.  
- **Support System**:  
  - Contact form for queries.  
  - Email and phone support.  
  - FAQ section for quick help.  
- **Responsive UI**: Mobile-friendly design with Bootstrap 5 and icons.

## Database Schema

### 1. AdminDetails
- Email (Primary Key)  
- Password  
- Name  

### 2. NurseDetails
- NurseID (Primary Key)  
- Name  
- Email (Unique)  
- Contact  
- Address  
- Gender  
- DOB  
- Qualification  
- Experience  
- Specialization  
- ProfilePic  
- DocumentFile  
- Status (Pending/Approved/Rejected)  
- ApprovedBy  
- ApprovedDate  

### 3. BookingDetails
- BookingID (Primary Key, Auto-Increment)  
- NurseID (Foreign Key from NurseDetails)  
- ClientName  
- ClientContact  
- ServiceType  
- BookingDate  
- Status (Pending/Approved/Completed/Cancelled)  
- ApprovedBy  
- ResponseDate  

### 4. SupportMessages
- MessageID (Primary Key)  
- UserEmail  
- Subject  
- MessageBody  
- SubmittedDate  

## Technologies Used
- **Frontend**: HTML, CSS, Bootstrap 5, JavaScript, Font Awesome, SweetAlert2  
- **Backend**: ASP.NET Core MVC (.NET 6+)  
- **Database**: Microsoft SQL Server  
- **Data Access**: ADO.NET (SqlConnection, SqlDataReader, SqlCommand)  

## Installation & Setup

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/your-repo/BookMyCare.git
cd BookMyCare
