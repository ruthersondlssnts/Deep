using Microsoft.Extensions.DependencyInjection;

namespace Deep.Common.SimpleMediatR
{
    public static class RequestPipelineExtensions
    {
        public static IServiceCollection AddRequestPipelines(
            this IServiceCollection services,
            params Type[] pipelineTypes)
        {
            foreach (var pipelineType in pipelineTypes)
            {
                services.Decorate(typeof(IRequestHandler<,>), pipelineType);
            }

            return services;
        }
    }
}
