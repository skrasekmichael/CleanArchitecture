using TeamUp.Api.Endpoints.Teams;
using TeamUp.Api.Extensions;

namespace TeamUp.Api.Endpoints;

public sealed class TeamEndpointGroup : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.RequireAuthorization()
			.MapEndpoint<CreateTeamEndpoint>()
			.MapEndpoint<GetTeamEndpoint>()
			.MapEndpoint<DeleteTeamEndpoint>()
			.MapEndpoint<ChangeOwnerShipEndpoint>()
			.MapEndpoint<RemoveTeamMemberEndpoint>()
			.MapEndpoint<UpdateTeamMemberRoleEndpoint>()
			.MapEndpoint<UpdateTeamNameEndpoint>()
			.MapEndpoint<ChangeNicknameEndpoint>();
	}
}
