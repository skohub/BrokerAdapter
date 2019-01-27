namespace CloudFactoryTask.Advanced.Domain.KeyGenerators
{
    public interface IWaitListKeyGenerator
    {
        string Generate(BrokerRequest request);
    }
}