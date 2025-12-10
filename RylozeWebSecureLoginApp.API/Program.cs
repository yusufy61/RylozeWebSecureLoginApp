using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RylozeWebSecureLoginApp.API.Data;
using RylozeWebSecureLoginApp.API.Entities;
using RylozeWebSecureLoginApp.API.Interfaces;
using RylozeWebSecureLoginApp.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// DbContext configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


// Identity configuration
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


// JWT Authentication configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = false; // Token � sunucuda saklam�yoruz
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Token �n ge�erli bir issuer (yay�nc�) taraf�ndan olu�turuldu�unu do�rula
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true, // Token �n ge�erli bir audience (hedef kitle) i�in olu�turuldu�unu do�rula
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true, // Token �n s�resinin dolup dolmad���n� do�rula
            ClockSkew = TimeSpan.FromSeconds(30), // Saat fark� tolerans� yani token s�resi dolduktan sonra 30 saniye daha ge�erli say�lacak
            ValidateIssuerSigningKey = true, // �mza anahtar�n� do�rula
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
        };

    });

// JWT ile Rollere Authorization ekleme
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AntrenorOrAdmin", policy => policy.RequireRole("Antrenor", "Admin"));
    options.AddPolicy("OgrenciOrAbove", policy => policy.RequireRole("Ogrenci"));
});

// Dependency Injection for TokenService
builder.Services.AddScoped<ITokenService, TokenService>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Ryloze Web Secure Login API",
        Version = "v1",
        Description = "JWT Authentication & Authorization API"
    });

    // JWT Bearer token yapılandırması
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header kullanarak Bearer token giriniz. Örnek: 'Bearer {token}'"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    // Rolleri ve varsay�lan admin kullan�c�s�n� olu�tur
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // authentication her zaman authorization'dan önce gelir giriş yapmadan yetki alamazsın.
app.UseAuthorization();

app.MapControllers();

app.Run();
