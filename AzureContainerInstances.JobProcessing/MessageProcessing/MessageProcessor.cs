using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace AzureContainerInstances.JobProcessing.MessageProcessing
{
	public interface IMessageProcessor : IDisposable
	{
		void Start();
	}

	public sealed class ServiceBusMessageProcessor : IMessageProcessor
	{
		private static readonly int ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;

		private readonly ILogCLient _logClient;
		private readonly string _connectionStringForReading;
		private readonly string _queueName;
		private QueueClient _queueClient;


		public ServiceBusMessageProcessor(ILogCLient logClient, string connectionString = null, string queueName = "TestQueue")
		{
			if (string.IsNullOrWhiteSpace(connectionString))
				connectionString = Environment.GetEnvironmentVariable(Startup.MicrosoftServicebusConnectionStringSettingName);
			if (string.IsNullOrWhiteSpace(connectionString))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));
			if (string.IsNullOrWhiteSpace(queueName))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(queueName));

			_logClient = logClient ?? throw new ArgumentNullException(nameof(logClient));
			_connectionStringForReading = connectionString;
			_queueName = queueName;
		}

		public void Start()
		{
			ProcessQueueMessages();
		}

		public void Dispose()
		{
			_queueClient?.CloseAsync().GetAwaiter().GetResult();
		}

		private void ProcessQueueMessages()
		{
			if (_queueClient != null) return;

			_queueClient = new QueueClient(_connectionStringForReading, _queueName, ReceiveMode.ReceiveAndDelete);

			var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
			{
				MaxConcurrentCalls = 2,
				AutoComplete = true
			};

			_queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
		}

		private Task ProcessMessagesAsync(Message message, CancellationToken token)
		{
			return LogMessage($"Message processed: {Encoding.Unicode.GetString(message.Body)}.");
		}

		private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
		{
			return LogMessage(arg.Exception.ToString());
		}

		private async Task LogMessage(string message)
		{
			if (string.IsNullOrWhiteSpace(message)) return;

			message = $"JobProcessing - {ProcessId:D5} - {DateTime.UtcNow:O} - {message}";
			Console.WriteLine(message);

			if (_logClient == null) return;

			await _logClient.LogAsync(message);
		}
	}
}
