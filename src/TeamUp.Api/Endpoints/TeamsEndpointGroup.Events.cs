using TeamUp.Api.Endpoints.Events;
using TeamUp.Api.Extensions;

namespace TeamUp.Api.Endpoints;

public sealed class EventsEndpointGroup : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapEndpoint<CreateEventEndpoint>()
			.MapEndpoint<GetEventEndpoint>();
	}
}
