using Microsoft.AspNetCore.Routing;

namespace Deep.Common.Application.Api.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
