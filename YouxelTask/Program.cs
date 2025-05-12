using FileStorage.Application.Interfaces;
using FileStorage.Application.Services;
using FileStorage.Domain.Repositories;
using FileStorage.Domain.Services;
using FileStorage.Infrastructure.Data;
using FileStorage.Infrastructure.Repositories;
using FileStorage.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using YouxelTask.FileStorage.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new() { Title = "Your API", Version = "v1" });

	// 🔒 Add JWT authentication support
	c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		In = Microsoft.OpenApi.Models.ParameterLocation.Header,
		Description = "Enter 'Bearer' [space] and then your valid JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6..."
	});

	c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
			new string[] {}
		}
	});
});

builder.Services.AddDbContext<FileStorageDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<FileRepository, FileRepository>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddSingleton<IStorageService, FileSystemStorageService>();
builder.Services.AddSingleton<IMessageService, RabbitMQService>();

builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowSpecificOrigins",
		policy => policy
			.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>())
			.AllowAnyMethod()
			.AllowAnyHeader());
});
var secretKey = builder.Configuration["Jwt:SecretKey"]
				?? throw new InvalidOperationException("JWT Secret Key not configured");
var key = Encoding.ASCII.GetBytes(secretKey);
builder.Services.AddAuthentication(x =>
{
	x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
	x.RequireHttpsMetadata = false;
	x.SaveToken = true;
	x.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(key),
		
	};
	x.IncludeErrorDetails = true;

});
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<FileStorageDbContext>();
	dbContext.Database.EnsureCreated();
}
app.Run();
