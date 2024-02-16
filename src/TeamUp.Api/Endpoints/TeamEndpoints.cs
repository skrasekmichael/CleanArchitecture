using MediatR;

using Microsoft.AspNetCore.Mvc;

using TeamUp.Api.Endpoints.Teams;
using TeamUp.Api.Extensions;
using TeamUp.Application.Teams.ChangeOwnership;
using TeamUp.Application.Teams.DeleteTeam;
using TeamUp.Application.Teams.GetTeam;
using TeamUp.Application.Teams.RemoveTeamMember;
using TeamUp.Application.Teams.SetMemberNickname;
using TeamUp.Application.Teams.SetMemberRole;
using TeamUp.Application.Teams.SetTeamName;
using TeamUp.Contracts.Teams;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Api.Endpoints;

public sealed class TeamEndpoints : IEndpointGroup
{
	public void MapEndpoints(RouteGroupBuilder group)
	{
		group.RequireAuthorization();

		group.MapPost("/", CreateTeamAsync)
			.Produces<TeamId>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesValidationProblem()
			.WithName(nameof(CreateTeamAsync))
			.MapToApiVersion(1);

		group.MapGet("/{teamId:guid}", GetTeamAsync)
			.Produces<TeamResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(GetTeamAsync))
			.MapToApiVersion(1);

		group.MapDelete("/{teamId:guid}", DeleteTeamAsync)
			.Produces<TeamResponse>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(DeleteTeamAsync))
			.MapToApiVersion(1);

		group.MapPatch("/{teamId:guid}", UpdateTeamNameAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(UpdateTeamNameAsync))
			.MapToApiVersion(1);

		group.MapPut("/{teamId:guid}/owner", ChangeOwnerShipAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(ChangeOwnerShipAsync))
			.MapToApiVersion(1);

		group.MapDelete("/{teamId:guid}/{teamMemberId:guid}", RemoveTeamMemberAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithName(nameof(RemoveTeamMemberAsync))
			.MapToApiVersion(1);

		group.MapPut("/{teamId:guid}/{teamMemberId:guid}/role", UpdateTeamRoleAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(UpdateTeamRoleAsync))
			.MapToApiVersion(1);

		group.MapPut("/{teamId:guid}/nickname", ChangeNicknameAsync)
			.Produces(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status403Forbidden)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.ProducesValidationProblem()
			.WithName(nameof(ChangeNicknameAsync))
			.MapToApiVersion(1);
	}

	private async Task<IResult> CreateTeamAsync(
		[FromBody] CreateTeamRequest request,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		[FromServices] LinkGenerator linkGenerator,
		CancellationToken ct)
	{
		var command = new CreateTeamCommand(httpContextAccessor.GetLoggedUserId(), request.Name);
		var result = await sender.Send(command, ct);
		return result.Match(teamId => TypedResults.Created(
			uri: linkGenerator.GetPathByName(nameof(GetTeamAsync)),
			value: teamId
		));
	}

	private async Task<IResult> GetTeamAsync(
		[FromRoute] Guid teamId,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var query = new GetTeamQuery(httpContextAccessor.GetLoggedUserId(), TeamId.FromGuid(teamId));
		var result = await sender.Send(query, ct);
		return result.Match(TypedResults.Ok);
	}

	private async Task<IResult> DeleteTeamAsync(
		[FromRoute] Guid teamId,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var command = new DeleteTeamCommand(httpContextAccessor.GetLoggedUserId(), TeamId.FromGuid(teamId));
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}

	private async Task<IResult> UpdateTeamNameAsync(
		[FromRoute] Guid teamId,
		[FromBody] UpdateTeamNameRequest request,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var command = new SetTeamNameCommand(
			httpContextAccessor.GetLoggedUserId(),
			TeamId.FromGuid(teamId),
			request.Name
		);
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}

	private async Task<IResult> ChangeOwnerShipAsync(
		[FromRoute] Guid teamId,
		[FromBody] Guid teamMemberId,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var command = new ChangeOwnershipCommand(
			httpContextAccessor.GetLoggedUserId(),
			TeamId.FromGuid(teamId),
			TeamMemberId.FromGuid(teamMemberId)
		);
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}

	private async Task<IResult> RemoveTeamMemberAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid teamMemberId,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var command = new RemoveTeamMemberCommand(
			httpContextAccessor.GetLoggedUserId(),
			TeamId.FromGuid(teamId),
			TeamMemberId.FromGuid(teamMemberId)
		);
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}

	private async Task<IResult> UpdateTeamRoleAsync(
		[FromRoute] Guid teamId,
		[FromRoute] Guid teamMemberId,
		[FromBody] UpdateTeamRoleRequest request,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var command = new SetMemberRoleCommand(
			httpContextAccessor.GetLoggedUserId(),
			TeamId.FromGuid(teamId),
			TeamMemberId.FromGuid(teamMemberId),
			request.Role
		);
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}

	private async Task<IResult> ChangeNicknameAsync(
		[FromRoute] Guid teamId,
		[FromBody] ChangeNicknameRequest request,
		[FromServices] ISender sender,
		[FromServices] IHttpContextAccessor httpContextAccessor,
		CancellationToken ct)
	{
		var command = new ChangeNicknameCommand(
			httpContextAccessor.GetLoggedUserId(),
			TeamId.FromGuid(teamId),
			request.Nickname
		);
		var result = await sender.Send(command, ct);
		return result.Match(TypedResults.Ok);
	}
}
