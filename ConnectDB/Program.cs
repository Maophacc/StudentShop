using ConnectDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
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

// 3. Đăng ký Swagger với JWT Auth
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StudentShop API", Version = "v1" });
    
    // Thêm chức năng chọn Token cho Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header sử dụng Bearer scheme. Vui lòng nhập: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Cấu hình JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// 4. Cho phép Frontend ở mọi nơi gọi tới (CORS)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

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

app.UseCors("AllowAll"); // Rất quan trọng: Phải đặt trước Authorization

app.UseAuthentication(); // Phải gọi Authentication trước Authorization
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