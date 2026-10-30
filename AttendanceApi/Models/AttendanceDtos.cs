using System.ComponentModel.DataAnnotations;

namespace AttendanceApi.Models;

// --- 1 & 2. Auth DTOs ---
public sealed class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string FullName { get; set; } = string.Empty;
    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Student"; // Student, Teacher, Admin
}

public sealed class LoginRequest
{
    [Required]
    public string Identifier { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed class VerifyOtpRequest
{
    [Required]
    public long ChallengeId { get; set; }
    [Required]
    public string Code { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

// --- 5. Điểm danh QR & Vị trí GPS & Chống Fake GPS ---
public sealed class CreateQrSessionRequest
{
    [Required]
    public string ClassCode { get; set; } = string.Empty;
    public double TeacherLatitude { get; set; }
    public double TeacherLongitude { get; set; }
}

public sealed class CheckInRequest
{
    [Required]
    public string QrToken { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsMockGps { get; set; } // Tính năng mới: Phát hiện giả lập GPS
    public string DeviceId { get; set; } = string.Empty; // Tính năng mới: Khóa thiết bị
}

// --- 5d. Đo tiếng ồn ---
public sealed class NoiseMeasurementRequest
{
    [Required]
    public string ClassCode { get; set; } = string.Empty;
    public double DecibelValue { get; set; }
    public string Note { get; set; } = string.Empty;
}

// --- 4. CRUD Sinh viên / Quản lý ---
public sealed class StudentDto
{
    public string Id { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = "Công nghệ thông tin";
}