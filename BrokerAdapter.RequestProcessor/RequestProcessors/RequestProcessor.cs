using System;
using System.Threading;
using System.Threading.Tasks;
using CloudFactoryTask.Advanced.Domain.BrokerAdapters;
using CloudFactoryTask.Advanced.Domain.KeyGenerators;
using CloudFactoryTask.Advanced.Infrastructure;

namespace CloudFactoryTask.Advanced.Domain.RequestProcessors
{
    public class RequestProcessor : IRequestProcessor
    {
        private readonly IBrokerAdapter _brokerAdapter;
        private readonly IRequestKeyGenerator _requestKeyGenerator;
        private readonly ILogger _logger;

        public RequestProcessor(IBrokerAdapter brokerAdapter, IRequestKeyGenerator requestKeyGenerator, ILogger logger)
        {
            _brokerAdapter = brokerAdapter;
            _requestKeyGenerator = requestKeyGenerator;
            _logger = logger;
        }

        public async Task<BrokerResponse> Process(BrokerRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var requestKey = _requestKeyGenerator.Generate(request);
                _brokerAdapter.Publish(request, requestKey);
                return await _brokerAdapter.Retrieve(requestKey, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                return null;
            }
        }
    }
}
