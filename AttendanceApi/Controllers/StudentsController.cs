using Microsoft.AspNetCore.Mvc;
using AttendanceApi.Models;

namespace AttendanceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private static readonly List<StudentDto> Students = new()
    {
        new() { Id = "1", StudentCode = "SV-DEMO-001", FullName = "Lê Minh Anh", Email = "demo.student1@baconca.local" },
        new() { Id = "2", StudentCode = "SV-DEMO-002", FullName = "Phạm Quốc Bảo", Email = "demo.student2@baconca.local" },
        new() { Id = "3", StudentCode = "SV-DEMO-003", FullName = "Võ Ngọc Chi", Email = "demo.student3@baconca.local" },
        new() { Id = "4", StudentCode = "SV-DEMO-004", FullName = "Đặng Hoàng Duy", Email = "demo.student4@baconca.local" }
    };

    // GET: /api/students (Read All)
    [HttpGet]
    public IActionResult GetAll() => Ok(Students);

    // GET: /api/students/{id} (Read One)
    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var st = Students.FirstOrDefault(s => s.Id == id);
        return st != null ? Ok(st) : NotFound(new { message = "Không tìm thấy sinh viên." });
    }

    // POST: /api/students (Create)
    [HttpPost]
    public IActionResult Create([FromBody] StudentDto student)
    {
        student.Id = (Students.Count + 1).ToString();
        Students.Add(student);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    // PUT: /api/students/{id} (Update)
    [HttpPut("{id}")]
    public IActionResult Update(string id, [FromBody] StudentDto updated)
    {
        var st = Students.FirstOrDefault(s => s.Id == id);
        if (st == null) return NotFound(new { message = "Không tìm thấy sinh viên." });

        st.FullName = updated.FullName;
        st.Email = updated.Email;
        st.Department = updated.Department;
        return Ok(st);
    }

    // DELETE: /api/students/{id} (Delete)
    [HttpDelete("{id}")]
    public IActionResult Delete(string id)
    {
        var st = Students.FirstOrDefault(s => s.Id == id);
        if (st == null) return NotFound(new { message = "Không tìm thấy sinh viên." });

        Students.Remove(st);
        return Ok(new { success = true, message = "Đã xóa sinh viên thành công." });
    }
}