using Microsoft.EntityFrameworkCore;

using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Events;
using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Teams;

using EventResponse = TeamUp.Contracts.Events.EventResponse;

namespace TeamUp.Application.Events.GetEvent;

internal sealed class GetEventQueryHandler : IQueryHandler<GetEventQuery, Result<EventResponse>>
{
	private readonly IAppQueryContext _appQueryContext;

	public GetEventQueryHandler(IAppQueryContext appQueryContext)
	{
		_appQueryContext = appQueryContext;
	}

	public async Task<Result<EventResponse>> Handle(GetEventQuery query, CancellationToken ct)
	{
		var team = await _appQueryContext.Teams
			.Include(team => team.Members)
			.Select(team => new
			{
				team.Id,
				Event = _appQueryContext.Events
					.Where(e => e.Id == query.EventId && e.TeamId == team.Id)
					.Select(e => new EventResponse
					{
						Description = e.Description,
						EventTypeId = e.EventTypeId,
						EventType = team.EventTypes.First(et => et.Id == e.EventTypeId).Name,
						FromUtc = e.FromUtc,
						ToUtc = e.ToUtc,
						MeetTime = e.MeetTime,
						ReplyClosingTimeBeforeMeetTime = e.ReplyClosingTimeBeforeMeetTime,
						Status = e.Status,
						EventResponses = e.EventResponses.Select(er => new EventResponseResponse
						{
							Message = er.Message,
							TeamMemberId = er.TeamMemberId,
							TeamMemberNickname = team.Members.First(member => member.Id == er.TeamMemberId).Nickname,
							TimeStampUtc = er.TimeStampUtc,
							Type = er.ReplyType
						}).ToList()
					})
					.FirstOrDefault(),
				Initiator = team.Members
					.Select(member => member.UserId)
					.FirstOrDefault(id => id == query.InitiatorId)
			})
			.FirstOrDefaultAsync(team => team.Id == query.TeamId, ct);

		return team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.EnsureNotNull(team => team.Initiator, TeamErrors.NotMemberOfTeam)
			.EnsureNotNull(team => team.Event, EventErrors.EventNotFound)
			.Then(team => team.Event!);
	}
}
