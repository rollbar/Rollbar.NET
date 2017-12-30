#if NETCOREAPP2_0

namespace Microsoft.AspNetCore.Builder
{
    using Rollbar.AspNetCore;


    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRollbarMiddleware(
            this IApplicationBuilder builder
            )
        {
            return builder.UseMiddleware<RollbarMiddleware>();
        }
    }
}

#endif