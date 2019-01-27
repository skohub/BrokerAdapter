using System;

namespace CloudFactoryTask.Advanced.Infrastructure
{
    public class ConsoleLogger : ILogger
    {
        public void Error(string message)
        {
            Console.WriteLine($"ERROR: {message}");
        }

        public void Info(string message)
        {
            Console.WriteLine($"INFO: {message}");
        }
    }
}
