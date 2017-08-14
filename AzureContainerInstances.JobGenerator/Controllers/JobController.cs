using System;
using System.Net.Http;
using System.Threading.Tasks;
using AzureContainerInstances.JobGenerator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace AzureContainerInstances.JobGenerator.Controllers
{
	[Route("api/jobs")]
	public class JobController : Controller
	{
		private const string LoggingServiceUrlSettingName = "LoggingServiceUrl";
		private const string MicrosoftServicebusConnectionStringSettingName = "Microsoft.ServiceBus.ConnectionString";
		private const string QueueName = "TestQueue";
		private readonly string _connectionStringForWriting;
		private readonly Uri _loggingServiceUrl;
		private readonly HttpClient _httpClient;
		private static readonly int ProcessId = System.Diagnostics.Process.GetCurrentProcess().Id;
		
		public JobController()
		{
			_connectionStringForWriting = Environment.GetEnvironmentVariable(MicrosoftServicebusConnectionStringSettingName);
			var loggingServiceUrlEnvironmentVariable = Environment.GetEnvironmentVariable(LoggingServiceUrlSettingName);
			//loggingServiceUrlEnvironmentVariable = @"http://localhost:20337";
			if (!string.IsNullOrWhiteSpace(loggingServiceUrlEnvironmentVariable))
			{
				_loggingServiceUrl = new Uri(loggingServiceUrlEnvironmentVariable);

				_httpClient = new HttpClient()
				{
					BaseAddress = _loggingServiceUrl,
				};
				_httpClient.DefaultRequestHeaders.Accept.Clear();
				_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			}
		}

		// GET api/job/?jobDescription={some value}
		[HttpGet]
		[Route("")]
		public async Task<IActionResult> Get([FromQuery]string jobDescription)
		{
			if (string.IsNullOrWhiteSpace(_connectionStringForWriting))
			{
				string error = $"Provide an Environment Variable named '{MicrosoftServicebusConnectionStringSettingName}' that holds a connection string that has write access to your Azure Service Bus.";
				await LogMessageAsync($"Bad request received: '{error}'");
				return BadRequest(error);
			}

			if (string.IsNullOrWhiteSpace(jobDescription))
			{
				var error = $"A value for '{jobDescription}' is required.";
				await LogMessageAsync($"Bad request received: '{error}'");
				return BadRequest(error);
			}

			await LogMessageAsync($"Enqueueing job: {jobDescription}");
			await EnqueueJobAsync(jobDescription);

			return Ok(jobDescription);
		}

		// POST api/job
		[HttpPost]
		[Route("")]
		public async Task<IActionResult> Post([FromBody]JobMessage job)
		{
			if (string.IsNullOrWhiteSpace(_connectionStringForWriting))
			{
				string error = $"Provide an Environment Variable named '{MicrosoftServicebusConnectionStringSettingName}' that holds a connection string that has write access to your Azure Service Bus.";
				await LogMessageAsync($"Bad request received: '{error}'");
				return BadRequest(error);
			}

			if (string.IsNullOrWhiteSpace(job.JobDescription))
			{
				var error = $"A value for '{job.JobDescription}' is required.";
				await LogMessageAsync($"Bad request received: '{error}'");
				return BadRequest(error);
			}

			await LogMessageAsync($"Enqueueing job: {job.JobDescription}");

			await EnqueueJobAsync(job.JobDescription);
			return Accepted();
		}

		private async Task EnqueueJobAsync(string jobDescription)
		{
			try
			{
				var client = new QueueClient(_connectionStringForWriting, QueueName);
				var message = new Message(System.Text.Encoding.Unicode.GetBytes(jobDescription));
				await client.SendAsync(message).ConfigureAwait(true);
			}
			catch (Exception ex)
			{
				await LogMessageAsync($"An exception has occurred: '{ex}'");
				throw;
			}
		}

		private Task LogMessageAsync(string message)
		{
			if (_loggingServiceUrl == null || string.IsNullOrWhiteSpace(message)) return Task.CompletedTask;

			string json = JsonConvert.SerializeObject(new LogMessage
			{
				Message = $"JobGenerator - {ProcessId:D5} - {DateTime.UtcNow:O} - {message}"
			});

			var content = new StringContent(json, Encoding.Unicode, "application/json");
			return  _httpClient.PostAsync(new Uri(_loggingServiceUrl, "api/logs"), content);
		}
	}
}
