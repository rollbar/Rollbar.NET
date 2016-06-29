using System;
using System.Threading.Tasks;

namespace RollbarDotNet
{
    public interface IRollbarClient
    {
        Guid PostItem(Payload payload);

        Task<Guid> PostItemAsync(Payload payload);
    }
}