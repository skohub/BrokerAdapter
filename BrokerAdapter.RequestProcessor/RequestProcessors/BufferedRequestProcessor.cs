using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CloudFactoryTask.Advanced.Domain.KeyGenerators;
using CloudFactoryTask.Advanced.Infrastructure;

namespace CloudFactoryTask.Advanced.Domain.RequestProcessors
{
    public class BufferedRequestProcessor : IRequestProcessor
    {
        private const int RequestTimeout = 10000;
        private readonly IRequestProcessor _requestProcessor;
        private readonly IWaitListKeyGenerator _waitListKeyGenerator;
        private readonly ILogger _logger;
        private readonly object _locker = new object();
        private readonly Dictionary<string, PendingRequest> _waitList = new Dictionary<string, PendingRequest>();

        public BufferedRequestProcessor(IRequestProcessor requestProcessor, IWaitListKeyGenerator waitListKeyGenerator, ILogger logger)
        {
            _requestProcessor = requestProcessor;
            _waitListKeyGenerator = waitListKeyGenerator;
            _logger = logger;
        }

        public async Task<BrokerResponse> Process(BrokerRequest request, CancellationToken cancellationToken)
        {
            var waitListKey = _waitListKeyGenerator.Generate(request);
            var pendingRequest = PutRequestIntoWaitList(request, waitListKey);

            BrokerResponse brokerResponse;
            if (await Task.WhenAny(pendingRequest.Task, Task.Delay(RequestTimeout, cancellationToken)) == pendingRequest.Task)
            {
                brokerResponse = await pendingRequest.Task;
            }
            else
            {
                _logger.Error($"Timeout occured for the request {request.Path}");
                brokerResponse = null;
            }

            RemoveFromWaitListIfNecessary(pendingRequest, waitListKey);
            
            return brokerResponse;
        }

        private PendingRequest PutRequestIntoWaitList(BrokerRequest request, string waitListKey)
        {
            PendingRequest pendingRequest;
            lock (_locker)
            {
                if (_waitList.TryGetValue(waitListKey, out pendingRequest))
                {
                    pendingRequest.QueueLength += 1;
                }
                else
                {
                    pendingRequest = MakeRequest(request);
                    _waitList.Add(waitListKey, pendingRequest);
                }
            }

            return pendingRequest;
        }

        private void RemoveFromWaitListIfNecessary(PendingRequest pendingRequest, string waitListKey)
        {
            lock (_locker)
            {
                if (--pendingRequest.QueueLength == 0)
                {
                    pendingRequest.CancellationTokenSource.Cancel();
                    _waitList.Remove(waitListKey);
                }
            }
        }

        private PendingRequest MakeRequest(BrokerRequest request)
        {
            var source = new CancellationTokenSource();
            return new PendingRequest
            {
                Task = _requestProcessor.Process(request, source.Token),
                QueueLength = 1,
                CancellationTokenSource = source
            };
        }
    }
}
