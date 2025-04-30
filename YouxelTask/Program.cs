using FileStorage.Application.Interfaces;
using FileStorage.Application.Services;
using FileStorage.Domain.Repositories;
using FileStorage.Domain.Services;
using FileStorage.Infrastructure.Data;
using FileStorage.Infrastructure.Repositories;
using FileStorage.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using YouxelTask.FileStorage.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
app.UseAuthorization();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<FileStorageDbContext>();
	dbContext.Database.EnsureCreated();
}
app.Run();
