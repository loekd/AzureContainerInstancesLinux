using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System.Threading;
using Newtonsoft.Json;

namespace AzureContainerInstances.JobProcessing
{
	internal static class Program
	{
		private const string LoggingServiceUrlSettingName = "LoggingServiceUrl";
		private const string MicrosoftServicebusConnectionStringSettingName = "Microsoft.ServiceBus.ConnectionString";
		private const string QueueName = "TestQueue";
		private static readonly int ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
		private static string _connectionStringForReading;
		private static Uri _loggingServiceUrl;
		private static HttpClient _httpClient;

		private static void Main()
		{
			_connectionStringForReading = Environment.GetEnvironmentVariable(MicrosoftServicebusConnectionStringSettingName);
			var loggingServiceUrlEnvironmentVariable = Environment.GetEnvironmentVariable(LoggingServiceUrlSettingName);
			
			if (!string.IsNullOrWhiteSpace(loggingServiceUrlEnvironmentVariable))
			{
				_loggingServiceUrl = new Uri(loggingServiceUrlEnvironmentVariable);

				_httpClient = new HttpClient
				{
					BaseAddress = _loggingServiceUrl
				};
				_httpClient.DefaultRequestHeaders.Accept.Clear();
				_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			}
			try
			{
				if (string.IsNullOrWhiteSpace(_connectionStringForReading))
				{
					throw new Exception($"Provide an Environment Variable named '{MicrosoftServicebusConnectionStringSettingName}' that holds a connection string that has read access to your Azure Service Bus.");
				}

				LogMessage("Job Processing is starting.").GetAwaiter().GetResult();
				ProcessQueueMessages();
				LogMessage("Job Processing is started.").GetAwaiter().GetResult();
				LogMessage("Press <enter> to exit.").GetAwaiter().GetResult();
				Console.ReadLine();
			}
			catch (Exception ex)
			{
				LogMessage(ex.ToString()).GetAwaiter().GetResult();
				Console.Error.WriteLine(ex);
			}
			finally
			{
				LogMessage("Job Processing is stopped.").GetAwaiter().GetResult();
			}
		}
		
		private static void ProcessQueueMessages()
		{
			var client = new QueueClient(_connectionStringForReading, QueueName, ReceiveMode.ReceiveAndDelete);

			var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
			{
				MaxConcurrentCalls = 2,
				AutoComplete = true
			};

			client.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
		}

		private static Task ProcessMessagesAsync(Message message, CancellationToken token)
		{
			return LogMessage($"Message processed: {Encoding.Unicode.GetString(message.Body)}.");
		}

		private static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
		{
			return LogMessage(arg.Exception.ToString());
		}

		private static async Task LogMessage(string message)
		{
			if (string.IsNullOrWhiteSpace(message)) return;

			message = $"JobProcessing - {ProcessId:D5} - {DateTime.UtcNow:O} - {message}";
			Console.WriteLine(message);

			if (_loggingServiceUrl == null) return;

			string json = JsonConvert.SerializeObject(new LogMessage
			{
				Message = message
			});

			var content = new StringContent(json, Encoding.Unicode, "application/json");

			try
			{
				await _httpClient.PostAsync(new Uri(_loggingServiceUrl, "api/logs"), content);
			}
			catch
			{
			}
		}
	}
}