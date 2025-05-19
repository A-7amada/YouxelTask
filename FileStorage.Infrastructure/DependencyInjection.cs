using FileStorage.Domain.Repositories;
using HealthChecks.RabbitMQ;
using HealthChecks.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MyArchitechture.Domain.Entities;
using MyArchitechture.Infrastructure.Data;
using MyArchitechture.Infrastructure.Repositories;
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

			var redisHost = configuration["Redis:Host"];
			var redisPort = configuration["Redis:Port"];
			var redisPassword = configuration["Redis:Password"];

			var redisConnectionString = string.IsNullOrEmpty(redisPassword)
				? $"{redisHost}:{redisPort}"
				: $"{redisHost}:{redisPort},password={redisPassword}";

			services.AddHealthChecks()
				.AddCheck("self", () => HealthCheckResult.Healthy("The service is healthy"))
				.AddDbContextCheck<DataDbContext>()
				.AddSqlServer(
					configuration["ConnectionStrings:DefaultConnection"],
					name: "sql",
					healthQuery: "SELECT 1;",
					timeout: TimeSpan.FromSeconds(3))
				.AddRabbitMQ(
					rabbitConnectionString: rabbitUri,
					name: "rabbitmq",
					failureStatus: HealthStatus.Unhealthy,
					tags: new[] { "mq", "rabbit" },
					timeout: TimeSpan.FromSeconds(5)
				)
				.AddRedis(
					redisConnectionString: redisConnectionString,
					name: "redis",
					failureStatus: HealthStatus.Unhealthy,
					tags: new[] { "cache", "redis" },
					timeout: TimeSpan.FromSeconds(5)
				);

			services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

			// Fix for CS7036: Provide the required setupAction parameter
			services.AddStackExchangeRedisCache(options =>
			{
				options.Configuration = redisConnectionString;
				options.InstanceName = "redis";
			});
			services.AddScoped<IRepository<Employee>, GenericRepository<Employee>>();
			services.Decorate(typeof(IRepository<Employee>), typeof(CachingRepositoryDecorator<Employee>));
			return services;
		}
	}
}
