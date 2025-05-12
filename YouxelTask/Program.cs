using FileStorage.Application.Interfaces;
using FileStorage.Application.Services;
using FileStorage.Domain.Repositories;
using FileStorage.Domain.Services;
using FileStorage.Infrastructure.Data;
using FileStorage.Infrastructure.Repositories;
using FileStorage.Infrastructure.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using YouxelTask.FileStorage.Infrastructure;
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

builder.Services.AddRateLimiter(options =>
{
	// Define a “fixed” window policy: 10 requests per minute per client
	options.AddFixedWindowLimiter("fixed", opts =>
	{
		opts.PermitLimit = 10;                    // max requests per window
		opts.Window = TimeSpan.FromMinutes(1);
		opts.QueueLimit = 2;                     // queue up to 2 extra requests
		opts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
	});

	// (Optionally) set a global default policy:
	//options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
	//	RateLimitPartition.GetHeaderLimiter(
	//		partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString()!,
	//		factory: _ => new FixedWindowRateLimiterOptions
	//		{
	//			PermitLimit = 100,
	//			Window = TimeSpan.FromMinutes(5),
	//			QueueLimit = 0
	//		}));
});

builder.Services.AddDbContext<FileStorageDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddHealthChecks()
//	.AddDbContextCheck<FileStorageDbContext>()
//	.AddCheck("Custom Health Check", () => HealthCheckResult.Healthy("The service is healthy"))
//	/*.AddRabbitMQ(builder.Configuration["RabbitMQ:ConnectionString"])*/;

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
builder.Services
	.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseRateLimiter();
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
app.UseHealthChecksUI(options =>
{
	options.UIPath = "/health-ui";         // dashboard at /health-ui
});
app.MapHealthChecks("/health", new HealthCheckOptions
{
	ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}); 
app.MapControllers().RequireRateLimiting("fixed");
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<FileStorageDbContext>();
	dbContext.Database.EnsureCreated();
}
app.Run();
