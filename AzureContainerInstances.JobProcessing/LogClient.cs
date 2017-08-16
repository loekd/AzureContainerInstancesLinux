using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureContainerInstances.JobProcessing
{
	public interface ILogCLient
	{
		void Log(string message);

		Task LogAsync(string message);
	}

	public class LogClient : ILogCLient
    {
	    private readonly Uri _loggingServiceUrl;
	    private readonly HttpClient _httpClient;

		public LogClient(string loggingServiceUrl)
		{
			if (string.IsNullOrWhiteSpace(loggingServiceUrl))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(loggingServiceUrl));

			_loggingServiceUrl = new Uri(loggingServiceUrl);
			_httpClient = new HttpClient();
			_httpClient = new HttpClient
			{
				BaseAddress = _loggingServiceUrl
			};
			_httpClient.DefaultRequestHeaders.Accept.Clear();
			_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		}

		public void Log(string message)
	    {
		    LogAsync(message).GetAwaiter().GetResult();
	    }

	    public async Task LogAsync(string message)
	    {
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
