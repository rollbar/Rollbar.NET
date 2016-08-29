using System;
#if NETFX_45
using System.Threading.Tasks;
#endif

namespace RollbarDotNet
{
    public interface IRollbarClient
    {
        Guid PostItem(Payload payload);
#if NETFX_45
        Task<Guid> PostItemAsync(Payload payload);
#endif
    }
}