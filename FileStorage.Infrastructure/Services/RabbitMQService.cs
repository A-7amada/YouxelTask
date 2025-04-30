using FileStorage.Domain.Services;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using File = FileStorage.Domain.Entities.File;
using IModel = RabbitMQ.Client.IModel;
namespace FileStorage.Infrastructure.Services
{
	public class RabbitMQService : IMessageService, IDisposable
	{
		private readonly IConnection _connection;
		private readonly IModel _channel;
		private readonly ILogger<RabbitMQService> _logger;
		private const string ExchangeName = "file_storage_events";

		public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
		{
			_logger = logger;

			try
			{
				var factory = new ConnectionFactory
				{
					HostName = configuration["RabbitMQ:Host"],
					Port = int.Parse(configuration["RabbitMQ:Port"]),
					UserName = configuration["RabbitMQ:Username"],
					Password = configuration["RabbitMQ:Password"],
					VirtualHost = configuration["RabbitMQ:VirtualHost"]
				};
				
				_connection = factory.CreateConnection();
				_channel = _connection.CreateModel();

				// Declare exchange
				_channel.ExchangeDeclare(
					exchange: ExchangeName,
					type: "topic",
					durable: true,
					autoDelete: false);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to connect to RabbitMQ");
				throw;
			}
		}

		public void PublishFileCreated(File file)
		{
			PublishMessage("file.created", new
			{
				FileId = file.Id,
				FileName = file.Name,
				FileSize = file.Size,
				ContentType = file.ContentType,
				Timestamp = DateTime.UtcNow
			});
		}

		public void PublishFileAccessed(File file)
		{
			PublishMessage("file.accessed", new
			{
				FileId = file.Id,
				AccessCount = file.AccessCount,
				Timestamp = DateTime.UtcNow
			});
		}

		public void PublishFileDeleted(File file)
		{
			PublishMessage("file.deleted", new
			{
				FileId = file.Id,
				FileName = file.Name,
				Timestamp = DateTime.UtcNow
			});
		}

		private void PublishMessage(string routingKey, object message)
		{
			try
			{
				var messageJson = JsonSerializer.Serialize(message);
				var body = Encoding.UTF8.GetBytes(messageJson);

				_channel.BasicPublish(
					exchange: ExchangeName,
					routingKey: routingKey,
					basicProperties: null,
					body: body);

				_logger.LogInformation("Published message to {RoutingKey}: {Message}", routingKey, messageJson);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to publish message to RabbitMQ");
			}
		}

		public void Dispose()
		{
			_channel?.Close();
			_connection?.Close();
		}
	}
}
