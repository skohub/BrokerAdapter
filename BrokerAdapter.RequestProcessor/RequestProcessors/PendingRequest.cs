using System.Threading;
using System.Threading.Tasks;

namespace CloudFactoryTask.Advanced.Domain.RequestProcessors
{
    public class PendingRequest
    {
        public Task<BrokerResponse> Task { get; set; }
        public int QueueLength { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}
