var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bật CORS cho Flutter
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Luôn bật Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Attendance API v1");
    c.RoutePrefix = "swagger"; // Đặt route Swagger chuẩn là /swagger
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// ROUTE CỨU CÁNH: Giúp Render Health Check trả về 200 OK ngay tại trang chủ
app.MapGet("/", () => Results.Ok(new
{
    status = "Online",
    message = "Attendance API dang chay!",
    swagger = "/swagger"
}));

app.MapGet("/healthz", () => Results.Ok("OK"));
app.MapGet("/ping", () => Results.Ok(new { status = "Pong", time = DateTime.UtcNow }));

app.Run();