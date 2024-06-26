﻿using TeamUp.Api.Endpoints.UserAccess;
using TeamUp.Api.Extensions;

namespace TeamUp.Api.Endpoints;

public sealed class UserAccessEndpointGroup : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.MapEndpoint<RegisterUserEndpoint>()
			.MapEndpoint<ActivateAccountEndpoint>()
			.MapEndpoint<CompleteRegistrationEndpoint>()
			.MapEndpoint<LoginUserEndpoint>()
			.MapEndpoint<GetMyAccountDetailsEndpoint>()
			.MapEndpoint<DeleteAccountEndpoint>();
	}
}
