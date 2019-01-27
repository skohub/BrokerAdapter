namespace CloudFactoryTask.Advanced.Domain.KeyGenerators
{
    public interface IRequestKeyGenerator
    {
        string Generate(BrokerRequest request);
    }
}