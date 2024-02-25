using TeamUp.Api.Endpoints.Invitations;
using TeamUp.Api.Extensions;

namespace TeamUp.Api.Endpoints;

public sealed class InvitationsEndpointGroup : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.RequireAuthorization()
			.MapEndpoint<InviteUserEndpoint>()
			.MapEndpoint<GetTeamInvitationsEndpoint>()
			.MapEndpoint<AcceptInvitationEndpoint>()
			.MapEndpoint<GetMyInvitationsEndpoint>();
	}
}
