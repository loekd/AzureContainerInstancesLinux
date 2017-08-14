using System.Collections;
using System.Collections.Generic;

namespace AzureContainerInstances.Logging.Logging
{
    public interface IMessageLogger
    {
	    void Log(string message);

	    IEnumerable<string> GetAllMessages();
    }
}
