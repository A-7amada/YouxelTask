using FileStorage.Infrastructure.Data;
using HealthChecks.RabbitMQ;
using HealthChecks.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouxelTask.FileStorage.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			var rabbitUri =
			$"amqp://{configuration["RabbitMQ:Username"]}:" +
			$"{configuration["RabbitMQ:Password"]}@" +
			$"{configuration["RabbitMQ:Host"]}:" +
			$"{configuration["RabbitMQ:Port"]}" +
			$"{configuration["RabbitMQ:VirtualHost"]}";
			services.AddSingleton<IConnection>(sp =>
			{
				var section = configuration.GetSection("RabbitMQ");                                     // {RabbitMQ} in appsettings.json :contentReference[oaicite:0]{index=0}
				var uri = new Uri($"amqp://{section["Username"]}:{section["Password"]}" +
								  $"@{section["Host"]}:{section["Port"]}{section["VirtualHost"]}");
				var factory = new ConnectionFactory { Uri = uri, AutomaticRecoveryEnabled = true };     // reuse & auto-recover per RabbitMQ best practice :contentReference[oaicite:1]{index=1}
				return factory.CreateConnection();
			});
			services.AddHealthChecks()
				.AddCheck("self", () => HealthCheckResult.Healthy("The service is healthy"))
				.AddDbContextCheck<FileStorageDbContext>()
				.AddSqlServer(
					configuration["ConnectionStrings:DefaultConnection"],
					name: "sql",
					healthQuery: "SELECT 1;",
					timeout: TimeSpan.FromSeconds(3))
				// RabbitMQ check reusing our singleton IConnection
				
			.AddRabbitMQ(
				rabbitConnectionString: rabbitUri,           
				name: "rabbitmq",                     
				failureStatus: HealthStatus.Unhealthy,        
				tags: new[] { "mq", "rabbit" },     
				timeout: TimeSpan.FromSeconds(5) 
			);

			return services;


		}
	}
}
