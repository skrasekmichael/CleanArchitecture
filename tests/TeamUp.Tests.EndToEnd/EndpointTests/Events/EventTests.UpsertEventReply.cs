using Microsoft.EntityFrameworkCore;

using TeamUp.Infrastructure.Core;

using EventResponse = TeamUp.Domain.Aggregates.Events.EventResponse;

namespace TeamUp.Tests.EndToEnd.EndpointTests.Events;

public sealed class UpsertEventReplyTests(AppFixture app) : EventTests(app)
{
	public static string GetUrl(TeamId teamId, EventId eventId) => GetUrl(teamId.Value, eventId.Value);
	public static string GetUrl(Guid teamId, Guid eventId) => $"/api/v1/teams/{teamId}/events/{eventId}";

	[Fact]
	public async Task UpsertEventReply_Should_AddEventResponseToDatabase()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var initiatorMemberId = team.Members.Single(member => member.UserId == initiatorUser.Id).Id;
		var targetEvent = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithStatus(EventStatus.Open)
			.WithRandomEventResponses(team.Members.Where(member => member.Id != initiatorMemberId))
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.Add(targetEvent);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidUpsertEventReplyRequest.Generate();

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetEvent.Id), request);

		//assert
		response.Should().Be200Ok();

		var eventResponse = await UseDbContextAsync(dbContext => dbContext.Set<EventResponse>().SingleOrDefaultAsync(er => er.TeamMemberId == initiatorMemberId));
		eventResponse.ShouldNotBeNull();
		eventResponse.ReplyType.Should().Be(request.ReplyType);
		eventResponse.Message.Should().Be(request.Message);
	}

	[Fact]
	public async Task UpsertEventReply_WhenUpdating_Should_UpdateEventResponseToDatabase()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var initiatorMemberId = team.Members.Single(member => member.UserId == initiatorUser.Id).Id;
		var targetEvent = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithStatus(EventStatus.Open)
			.WithRandomEventResponses(team.Members)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.Add(targetEvent);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidUpsertEventReplyRequest.Generate();

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetEvent.Id), request);

		//assert
		response.Should().Be200Ok();

		var eventResponse = await UseDbContextAsync(dbContext => dbContext.Set<EventResponse>().SingleOrDefaultAsync(er => er.TeamMemberId == initiatorMemberId));
		eventResponse.ShouldNotBeNull();
		eventResponse.ReplyType.Should().Be(request.ReplyType);
		eventResponse.Message.Should().Be(request.Message);
	}

	[Fact]
	public async Task UpsertEventReply_WhenReplyingCloseToReplyCloseTime_Should_AddEventResponseToDatabase()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var initiatorMemberId = team.Members.Single(member => member.UserId == initiatorUser.Id).Id;
		var targetEvent = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithStatus(EventStatus.Open)
			.WithRandomEventResponses(team.Members.Where(member => member.Id != initiatorMemberId))
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.Add(targetEvent);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidUpsertEventReplyRequest.Generate();

		DateTimeProvider.ExactTime = targetEvent.FromUtc.AddMinutes(-1) - targetEvent.MeetTime - targetEvent.ReplyClosingTimeBeforeMeetTime;

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetEvent.Id), request);

		//assert
		response.Should().Be200Ok();

		var eventResponse = await UseDbContextAsync(dbContext => dbContext.Set<EventResponse>().SingleOrDefaultAsync(er => er.TeamMemberId == initiatorMemberId));
		eventResponse.ShouldNotBeNull();
		eventResponse.ReplyType.Should().Be(request.ReplyType);
		eventResponse.Message.Should().Be(request.Message);
	}

	[Fact]
	public async Task UpsertEventReply_ToUnExistingEvent_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var initiatorMemberId = team.Members.Single(member => member.UserId == initiatorUser.Id).Id;
		var targetEventId = Guid.NewGuid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidUpsertEventReplyRequest.Generate();

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id.Value, targetEventId), request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(EventErrors.EventNotFound);
	}

	[Fact]
	public async Task UpsertEventReply_InUnExistingTeam_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var teamId = Guid.NewGuid();
		var targetEventId = Guid.NewGuid();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidUpsertEventReplyRequest.Generate();

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(teamId, targetEventId), request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.TeamNotFound);
	}

	[Fact]
	public async Task UpsertEventReply_WhenNotMemberOfTeam_Should_ResultInForbidden()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var owner = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(owner, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var targetEvent = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithStatus(EventStatus.Open)
			.WithRandomEventResponses(team.Members)
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.AddRange([owner, initiatorUser]);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.Add(targetEvent);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidUpsertEventReplyRequest.Generate();

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetEvent.Id), request);

		//assert
		response.Should().Be403Forbidden();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(TeamErrors.NotMemberOfTeam);
	}

	[Fact]
	public async Task UpsertEventReply_ToEventOfAnotherTeam_Should_ResultInNotFound()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var teams = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate(2);
		var team1 = teams.First();
		var team2 = teams.Last();
		var event1 = EventGenerators.Event
			.ForTeam(team1.Id)
			.WithEventType(team1.EventTypes[0].Id)
			.WithStatus(EventStatus.Open)
			.WithRandomEventResponses(team1.Members.Where(member => member.UserId != initiatorUser.Id))
			.Generate();
		var event2 = EventGenerators.Event
			.ForTeam(team2.Id)
			.WithEventType(team2.EventTypes[0].Id)
			.WithStatus(EventStatus.Open)
			.WithRandomEventResponses(team2.Members.Where(member => member.UserId != initiatorUser.Id))
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.AddRange(teams);
			dbContext.Events.AddRange([event1, event2]);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidUpsertEventReplyRequest.Generate();

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team1.Id, event2.Id), request);

		//assert
		response.Should().Be404NotFound();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(EventErrors.EventNotFound);
	}

	[Theory]
	[InlineData(EventStatus.Closed)]
	[InlineData(EventStatus.Canceled)]
	public async Task UpsertEventReply_WhenEventIsClosedForResponses_Should_ResultInBadRequest_DomainError(EventStatus status)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var initiatorMemberId = team.Members.Single(member => member.UserId == initiatorUser.Id).Id;
		var targetEvent = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithStatus(status)
			.WithRandomEventResponses(team.Members.Where(member => member.Id != initiatorMemberId))
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.Add(targetEvent);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidUpsertEventReplyRequest.Generate();

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetEvent.Id), request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(EventErrors.NotOpenForResponses);
	}

	[Fact]
	public async Task UpsertEventReply_WhenTimeForResponsesExpired_Should_ResultInBadRequest_DomainError()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var initiatorMemberId = team.Members.Single(member => member.UserId == initiatorUser.Id).Id;
		var targetEvent = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithStatus(EventStatus.Open)
			.WithRandomEventResponses(team.Members.Where(member => member.Id != initiatorMemberId))
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.Add(targetEvent);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidUpsertEventReplyRequest.Generate();

		DateTimeProvider.ExactTime = targetEvent.FromUtc.AddMinutes(10) - targetEvent.MeetTime - targetEvent.ReplyClosingTimeBeforeMeetTime;

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetEvent.Id), request);

		//assert
		response.Should().Be400BadRequest();

		var problemDetails = await response.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(EventErrors.TimeForResponsesExpired);
	}

	[Theory]
	[ClassData(typeof(EventGenerators.InvalidUpsertEventReplyRequest))]
	public async Task UpsertEventReply_WithInvalidRequest_Should_ResultInBadRequest_ValidationError(InvalidRequest<UpsertEventReplyRequest> request)
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var initiatorMemberId = team.Members.Single(member => member.UserId == initiatorUser.Id).Id;
		var targetEvent = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithStatus(EventStatus.Open)
			.WithRandomEventResponses(team.Members.Where(member => member.Id != initiatorMemberId))
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.Add(targetEvent);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		//act
		var response = await Client.PutAsJsonAsync(GetUrl(team.Id, targetEvent.Id), request.Request);

		//assert
		response.Should().Be400BadRequest();

		var validationProblemDetails = await response.ReadValidationProblemDetailsAsync();
		validationProblemDetails.ShouldContainValidationErrorFor(request.InvalidProperty);
	}

	[Fact]
	public async Task UpsertEventReply_WhenConcurrentUpsertFromSameMemberCompletes_Should_ResultInConflict()
	{
		//arrange
		var initiatorUser = UserGenerators.User.Generate();
		var members = UserGenerators.User.Generate(19);
		var team = TeamGenerators.Team
			.WithMembers(initiatorUser, members)
			.WithEventTypes(5)
			.Generate();
		var eventType = team.EventTypes[0];
		var initiatorMemberId = team.Members.Single(member => member.UserId == initiatorUser.Id).Id;
		var targetEvent = EventGenerators.Event
			.ForTeam(team.Id)
			.WithEventType(eventType.Id)
			.WithStatus(EventStatus.Open)
			.WithRandomEventResponses(team.Members.Where(member => member.Id != initiatorMemberId))
			.Generate();

		await UseDbContextAsync(dbContext =>
		{
			dbContext.Users.Add(initiatorUser);
			dbContext.Users.AddRange(members);
			dbContext.Teams.Add(team);
			dbContext.Events.Add(targetEvent);
			return dbContext.SaveChangesAsync();
		});

		Authenticate(initiatorUser);

		var request = EventGenerators.ValidUpsertEventReplyRequest.Generate();

		//act
		var (responseA, responseB) = await RunConcurrentRequestsAsync(
			() => Client.PutAsJsonAsync(GetUrl(team.Id, targetEvent.Id), request),
			() => Client.PutAsJsonAsync(GetUrl(team.Id, targetEvent.Id), request)
		);

		//assert
		responseA.Should().Be200Ok();
		responseB.Should().Be409Conflict();

		var eventResponse = await UseDbContextAsync(dbContext => dbContext.Set<EventResponse>().SingleAsync(er => er.TeamMemberId == initiatorMemberId));
		eventResponse.ReplyType.Should().Be(request.ReplyType);
		eventResponse.Message.Should().Be(request.Message);

		var problemDetails = await responseB.ReadProblemDetailsAsync();
		problemDetails.ShouldContainError(UnitOfWork.UniqueConstraintError);
	}
}
