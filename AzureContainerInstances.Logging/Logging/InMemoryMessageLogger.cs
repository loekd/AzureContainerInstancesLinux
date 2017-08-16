using System;
using System.Collections.Generic;
using System.IO;

namespace AzureContainerInstances.Logging.Logging
{
	public class InMemoryMessageLogger : IMessageLogger
	{
		private readonly List<string> _messages = new List<string>();

		public InMemoryMessageLogger()
		{
			Log("using memory based logging");
			//try
			//{
			//	var folders = string.Join(" | ", Directory.EnumerateDirectories("/", "*", SearchOption.TopDirectoryOnly));
			//	Log($"All root folders:{folders}");
			//}
			//catch (Exception ex)
			//{
			//	Log(ex.Message);
			//}
		}

		public void Log(string message)
		{
			_messages.Add(message);	
		}

		public IEnumerable<string> GetAllMessages()
		{
			return _messages.ToArray();
		}
	}
}