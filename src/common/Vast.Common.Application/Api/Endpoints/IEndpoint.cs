using Microsoft.AspNetCore.Routing;

namespace Vast.Common.Application.Api.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
