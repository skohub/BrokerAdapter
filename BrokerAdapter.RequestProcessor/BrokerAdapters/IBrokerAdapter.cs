using System.Threading;
using System.Threading.Tasks;

namespace CloudFactoryTask.Advanced.Domain.BrokerAdapters
{
    public interface IBrokerAdapter
    {
        void Publish(BrokerRequest request, string requestKey);
        Task<BrokerResponse> Retrieve(string key, CancellationToken cancellationToken);
    }
}