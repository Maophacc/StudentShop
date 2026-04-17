using ConnectDB.Data;

using Microsoft.EntityFrameworkCore;
// Không cần thêm using Npgsql ở đây, nhưng UseNpgsql sẽ tự nhận diện sau khi cài package thành công
var builder = WebApplication.CreateBuilder(args);

// Đổi UseSqlServer thành UseNpgsql
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
    c.RoutePrefix = "swagger"; // Vào thẳng link gốc là thấy Swagger
});

// CHỈ dùng HttpsRedirection khi chạy ở LOCAL (Development)
// Trên Render, họ đã tự xử lý HTTPS rồi, mình dùng cái này sẽ bị lỗi Redirect
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

// ... (Các phần code builder và app.Use... giữ nguyên)

// --- ĐOẠN CODE TỰ ĐỘNG TẠO BẢNG ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Tương đương với lệnh dotnet ef database update
        context.Database.Migrate();
        Console.WriteLine(">>> DATABASE: Đã kết nối và cập nhật bảng thành công!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($">>> DATABASE ERROR: {ex.Message}");
    }
}
// ----------------------------------

app.Run();