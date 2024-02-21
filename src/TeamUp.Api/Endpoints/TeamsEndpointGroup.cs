using TeamUp.Api.Endpoints.Teams;
using TeamUp.Api.Extensions;

namespace TeamUp.Api.Endpoints;

public sealed class TeamsEndpointGroup : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.RequireAuthorization()
			.MapEndpoint<CreateTeamEndpoint>()
			.MapEndpoint<GetTeamEndpoint>()
			.MapEndpoint<DeleteTeamEndpoint>()
			.MapEndpoint<ChangeOwnershipEndpoint>()
			.MapEndpoint<RemoveTeamMemberEndpoint>()
			.MapEndpoint<UpdateTeamMemberRoleEndpoint>()
			.MapEndpoint<UpdateTeamNameEndpoint>()
			.MapEndpoint<ChangeNicknameEndpoint>()
			.MapEndpoint<CreateEventTypeEndpoint>();
	}
}
