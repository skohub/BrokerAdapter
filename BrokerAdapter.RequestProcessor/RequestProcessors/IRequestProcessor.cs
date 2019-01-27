using System.Threading;
using System.Threading.Tasks;

namespace CloudFactoryTask.Advanced.Domain.RequestProcessors
{
    public interface IRequestProcessor
    {
        Task<BrokerResponse> Process(BrokerRequest request, CancellationToken cancellationToken);
    }
}