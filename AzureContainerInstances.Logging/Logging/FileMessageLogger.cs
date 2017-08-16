using System;
using System.Collections.Generic;
using System.IO;

namespace AzureContainerInstances.Logging.Logging
{
    public class FileMessageLogger : IMessageLogger
    {
	    private readonly string _folder;
		private readonly object _lockMe = new object();

	    public FileMessageLogger(string folder)
	    {
		    if (string.IsNullOrWhiteSpace(folder))
			    throw new ArgumentException("Value cannot be null or whitespace.", nameof(folder));

			_folder = folder;
			Log("using file based logging");
	    }

	    public void Log(string message)
	    {
		    try
		    {
			    string file = Path.Combine(_folder, $"{DateTime.UtcNow.Date:yyyyMMddHH}.txt");

				lock (_lockMe)
			    {
				    using (var sw = File.AppendText(file))
				    {
					    sw.WriteLine(message);
				    }
			    }
			}
		    catch (Exception ex)
		    {
			    Console.WriteLine(ex);
		    }
	    }

	    public IEnumerable<string> GetAllMessages()
	    {
		    string[] contents = new string[0];
		    try
			{
			    string file = Path.Combine(_folder, $"{DateTime.UtcNow.Date:yyyyMMddHH}.txt");
				lock (_lockMe)
			    {
				    contents = File.ReadAllLines(file);
				}
			}
		    catch (Exception ex)
		    {
			    Console.WriteLine(ex);
		    }

		    return contents;
	    }
    }
}
