using System.Security.Cryptography;
using System.Text;

namespace CloudFactoryTask.Advanced.Domain.KeyGenerators
{
    public class RequestKeyGenerator : IRequestKeyGenerator, IWaitListKeyGenerator
    {
        public string Generate(BrokerRequest request)
        {
            return GetMd5Hash(request.Method + request.Path);
        }

        private static string GetMd5Hash(string input)
        {
            using (var md5Hash = MD5.Create())
            {
                var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                var sb = new StringBuilder();

                foreach (var t in data)
                {
                    sb.Append(t.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
