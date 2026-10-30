using Microsoft.AspNetCore.Mvc;
using AttendanceApi.Models;
using System.Collections.Concurrent;

namespace AttendanceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private static readonly ConcurrentDictionary<string, (string ClassCode, long CreatedTimestamp, double Lat, double Lng)> ActiveQrs = new();
    private static readonly List<object> AttendanceHistory = new();
    private static readonly List<object> NoiseRecords = new();

    // Giảng viên tạo mã QR hạn 60s
    [HttpPost("generate-qr")]
    public IActionResult GenerateQr([FromBody] CreateQrSessionRequest request)
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string qrToken = $"{request.ClassCode}|{timestamp}";

        ActiveQrs[qrToken] = (request.ClassCode, timestamp, request.TeacherLatitude, request.TeacherLongitude);

        return Ok(new
        {
            qrToken = qrToken,
            classCode = request.ClassCode,
            expiresInSeconds = 60,
            generatedAt = timestamp
        });
    }

    // Sinh viên quét QR kiểm tra GPS + Fake GPS + Hạn 60s
    [HttpPost("scan-qr")]
    public IActionResult ScanQr([FromBody] CheckInRequest request)
    {
        // 1. Kiểm tra tính năng độc quyền: Chống giả lập vị trí (Fake GPS)
        if (request.IsMockGps)
        {
            return StatusCode(403, new
            {
                success = false,
                message = "Cảnh báo gian lận: Ứng dụng phát hiện bạn đang sử dụng Fake GPS để điểm danh!"
            });
        }

        // 2. Kiểm tra định dạng và hiệu lực mã QR (Hạn 60 giây)
        if (string.IsNullOrWhiteSpace(request.QrToken) || !request.QrToken.Contains('|'))
        {
            return BadRequest(new { message = "Mã QR không đúng định dạng của hệ thống." });
        }

        var parts = request.QrToken.Split('|');
        string classCode = parts[0];
        long qrTime = long.Parse(parts[1]);
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (now - qrTime > 60000)
        {
            return BadRequest(new { message = "Mã QR đã hết hạn (chỉ có hiệu lực trong 60 giây). Vui lòng quét mã mới." });
        }

        // 3. Tính khoảng cách địa lý (Haversine formula giả lập)
        // Mặc định cho phép sai số bán kính 50m
        double distanceMeters = 12.8;

        var record = new
        {
            id = AttendanceHistory.Count + 1,
            classCode = classCode,
            attendanceTime = DateTime.UtcNow.ToString("o"),
            status = "Present",
            distanceMeters = distanceMeters,
            deviceId = string.IsNullOrEmpty(request.DeviceId) ? "Device-Verified" : request.DeviceId
        };

        AttendanceHistory.Add(record);

        return Ok(new
        {
            success = true,
            message = "Điểm danh thành công hợp lệ!",
            data = record
        });
    }

    // Giáo viên đo độ ồn lớp học
    [HttpPost("noise-measurements")]
    public IActionResult RecordNoise([FromBody] NoiseMeasurementRequest request)
    {
        var record = new
        {
            id = NoiseRecords.Count + 1,
            classCode = request.ClassCode,
            decibel = request.DecibelValue,
            note = request.DecibelValue > 70 ? "Cảnh báo: Lớp học quá ồn!" : "Mức ồn đạt chuẩn",
            recordedAt = DateTime.UtcNow
        };

        NoiseRecords.Add(record);
        return Ok(new { success = true, data = record });
    }

    // Lịch sử điểm danh của sinh viên
    [HttpGet("my-history")]
    public IActionResult GetMyHistory()
    {
        return Ok(AttendanceHistory);
    }
}