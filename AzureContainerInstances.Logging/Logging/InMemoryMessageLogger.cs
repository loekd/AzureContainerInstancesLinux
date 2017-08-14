using System.Collections.Generic;

namespace AzureContainerInstances.Logging.Logging
{
	public class InMemoryMessageLogger : IMessageLogger
	{
		private readonly List<string> _messages = new List<string>();

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