using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CloudFactoryTask.Advanced.Infrastructure;

namespace CloudFactoryTask.Advanced.Domain.BrokerAdapters
{
    public class FileSystemBrokerAdapter : IBrokerAdapter
    {
        private static int FileCheckDelay = 1000;
        private readonly string _brokerPath;
        private readonly ILogger _logger;

        public FileSystemBrokerAdapter(string brokerPath, ILogger logger)
        {
            _brokerPath = brokerPath;
            _logger = logger;
        }

        public void Publish(BrokerRequest request, string requestKey)
        {
            if (!Directory.Exists(_brokerPath))
            {
                throw new Exception("Broker is not available at the moment");
            }

            var requestPath = GetRequestPath(requestKey);
            using (var writer = File.CreateText(Path.Combine(_brokerPath, requestPath)))
            {
                writer.WriteLine(request.Method);
                writer.WriteLine(request.Path);
            }
        }

        public async Task<BrokerResponse> Retrieve(string requestKey, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var responsePath = GetResponsePath(requestKey);
                if (File.Exists(responsePath))
                {
                    try
                    {
                        using (var reader = new StreamReader(responsePath))
                        {
                            if (!int.TryParse(reader.ReadLine(), out var code))
                            {
                                _logger.Error("Wrong response format");
                                return null;
                            }

                            return new BrokerResponse
                            {
                                HttpCode = code,
                                Content = reader.ReadLine()
                            };
                        }
                    }
                    finally
                    {
                        File.Delete(responsePath);
                        File.Delete(GetRequestPath(requestKey));
                    }
                }

                await Task.Delay(FileCheckDelay, cancellationToken);
            }

            _logger.Info("File system monitoring was cancelled");
            return null;
        }

        private string GetRequestPath(string requestKey)
        {

            var fileName = $"{requestKey}.req";
            return Path.Combine(_brokerPath, fileName);
        }

        private string GetResponsePath(string requestKey)
        {
            var fileName = $"{requestKey}.resp";
            return Path.Combine(_brokerPath, fileName);
        }
    }
}
