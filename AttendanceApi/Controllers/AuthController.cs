using Microsoft.AspNetCore.Mvc;
using AttendanceApi.Models;
using System.Collections.Concurrent;

namespace AttendanceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Lưu trữ OTP tạm thời trong bộ nhớ
    private static readonly ConcurrentDictionary<long, (string Code, string Email, string Role, string FullName, DateTime ExpireAt)> OtpChallenges = new();
    private static long _challengeCounter = 1000;

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        return Ok(new
        {
            success = true,
            message = "Đăng ký tài khoản thành công!",
            email = request.Email
        });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Hỗ trợ đăng nhập với mật khẩu chuẩn Demo@12345 hoặc mọi tài khoản demo
        if (string.IsNullOrWhiteSpace(request.Identifier) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Vui lòng nhập tài khoản và mật khẩu." });
        }

        string role = "Student";
        string fullName = "Sinh viên mẫu";

        if (request.Identifier.Contains("admin"))
        {
            role = "Admin";
            fullName = "Quản trị viên";
        }
        else if (request.Identifier.Contains("teacher"))
        {
            role = "Teacher";
            fullName = "Giảng viên";
        }

        long challengeId = Interlocked.Increment(ref _challengeCounter);
        string otpCode = "123456"; // Mã OTP cố định tiện demo hoặc kiểm tra
        DateTime expireAt = DateTime.UtcNow.AddMinutes(10);

        OtpChallenges[challengeId] = (otpCode, request.Identifier, role, fullName, expireAt);

        return Ok(new
        {
            success = true,
            challengeId = challengeId,
            message = "Mã xác thực OTP đã được gửi.",
            demoOtpCode = otpCode // Hỗ trợ app/giảng viên xem trực tiếp để pass nhanh
        });
    }

    [HttpPost("verify-otp")]
    public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (!OtpChallenges.TryGetValue(request.ChallengeId, out var challenge))
        {
            // Dự phòng cho mã thử nghiệm 123456 hoặc 111111
            if (request.Code == "123456" || request.Code == "111111")
            {
                return Ok(new LoginResponse
                {
                    AccessToken = "mock-jwt-token-attendance-secret-key-2026",
                    UserId = Guid.NewGuid().ToString(),
                    Email = "demo.user@baconca.local",
                    FullName = "Người dùng xác thực",
                    Roles = new List<string> { "Student" }
                });
            }
            return BadRequest(new { message = "Yêu cầu OTP không tồn tại hoặc đã hết hạn." });
        }

        if (challenge.ExpireAt < DateTime.UtcNow)
        {
            OtpChallenges.TryRemove(request.ChallengeId, out _);
            return BadRequest(new { message = "Mã OTP đã hết thời hạn 10 phút." });
        }

        if (challenge.Code != request.Code && request.Code != "123456")
        {
            return BadRequest(new { message = "Mã OTP không chính xác." });
        }

        OtpChallenges.TryRemove(request.ChallengeId, out _);

        return Ok(new LoginResponse
        {
            AccessToken = "mock-jwt-token-attendance-secret-key-2026",
            UserId = Guid.NewGuid().ToString(),
            Email = challenge.Email,
            FullName = challenge.FullName,
            Roles = new List<string> { challenge.Role }
        });
    }
}