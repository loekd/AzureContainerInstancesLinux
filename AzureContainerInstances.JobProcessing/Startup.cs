using System;
using AzureContainerInstances.JobProcessing.MessageProcessing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureContainerInstances.JobProcessing
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

			//add log client
	        string loggingServiceUrlEnvironmentVariable = Environment.GetEnvironmentVariable(LoggingServiceUrlSettingName);
#if DEBUG
	        if (string.IsNullOrWhiteSpace(loggingServiceUrlEnvironmentVariable))
	        {
		        loggingServiceUrlEnvironmentVariable = "http://localhost:8080";
	        }
#endif
			if (!string.IsNullOrWhiteSpace(loggingServiceUrlEnvironmentVariable))
	        {
		        services.AddSingleton<ILogCLient>(_ => new LogClient(loggingServiceUrlEnvironmentVariable));
	        }
	        else
	        {
		        Console.WriteLine($"To enable the log client, provide an Environment Variable named '{loggingServiceUrlEnvironmentVariable}' that holds its root url.");
			}

			//add message processing
			string connectionStringForReading = Environment.GetEnvironmentVariable(MicrosoftServicebusConnectionStringSettingName);
	        if (!string.IsNullOrWhiteSpace(connectionStringForReading))
	        {
		        services.AddSingleton<IMessageProcessor, ServiceBusMessageProcessor>();
	        }
			else
			{ 
				Console.WriteLine($"Provide an Environment Variable named '{MicrosoftServicebusConnectionStringSettingName}' that holds a connection string that has read access to your Azure Service Bus.");
	        }
		}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

	        applicationLifetime.ApplicationStarted.Register(StartQueueuMessageProcessing, app.ApplicationServices.GetService<IMessageProcessor>(), false);
	        applicationLifetime.ApplicationStopping.Register(StopQueueuMessageProcessing, app.ApplicationServices.GetService<IMessageProcessor>(), false);
		}

	   

	    public const string LoggingServiceUrlSettingName = "LoggingServiceUrl";
	    public const string MicrosoftServicebusConnectionStringSettingName = "ServiceBusConnectionString";

		private void StartQueueuMessageProcessing(object state)
		{
			var processor = (IMessageProcessor) state;
			processor.Start();
		}

		private void StopQueueuMessageProcessing(object state)
	    {
			var processor = (IMessageProcessor)state;
		    processor.Dispose();
		}

	}
}
