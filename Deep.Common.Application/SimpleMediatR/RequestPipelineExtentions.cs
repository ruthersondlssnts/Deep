// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace Deep.Common.Application.SimpleMediatR;

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
