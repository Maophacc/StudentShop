using ConnectDB.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký Controller
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 3. Đăng ký Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- CẤU HÌNH PIPELINE ---

// Bật Swagger cho mọi môi trường
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConnectDB v1");
    c.RoutePrefix = string.Empty; // Vào thẳng link gốc là thấy Swagger
});

// CHỈ dùng HttpsRedirection khi chạy ở LOCAL (Development)
// Trên Render, họ đã tự xử lý HTTPS rồi, mình dùng cái này sẽ bị lỗi Redirect
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();