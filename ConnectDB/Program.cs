using ConnectDB.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký Controller và xử lý vòng lặp JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 3. Đăng ký Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- CẤU HÌNH PIPELINE ---

// BẬT SWAGGER CHO MỌI MÔI TRƯỜNG (Cả Local và Render)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ConnectDB v1");
    // Để trống RoutePrefix giúp bạn vào thẳng https://studentshop.onrender.com là thấy Swagger ngay
    c.RoutePrefix = string.Empty;
});

// Chỉ dùng HTTPS Redirection ở local nếu cần, trên Render thường đã có sẵn load balancer xử lý
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();