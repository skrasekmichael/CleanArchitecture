using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Abstractions;
using TeamUp.Common.Abstractions;
using TeamUp.Contracts.Events;
using TeamUp.Domain.Aggregates.Teams;

namespace TeamUp.Application.Events.GetEvents;

internal sealed class GetEventsQueryHandler : IQueryHandler<GetEventsQuery, Result<List<EventSlimResponse>>>
{
	private readonly IAppQueryContext _appQueryContext;
	private readonly IDateTimeProvider _dateTimeProvider;

	public GetEventsQueryHandler(IAppQueryContext appQueryContext, IDateTimeProvider dateTimeProvider)
	{
		_appQueryContext = appQueryContext;
		_dateTimeProvider = dateTimeProvider;
	}

	public async Task<Result<List<EventSlimResponse>>> HandleAsync(GetEventsQuery query, CancellationToken ct)
	{
		var from = query.FromUtc ?? _dateTimeProvider.UtcNow;
		var team = await _appQueryContext.Teams
			.Where(team => team.Id == query.TeamId)
			.Select(team => new
			{
				team.Id,
				team.EventTypes,
				Initiator = team.Members
					.Where(member => member.UserId == query.InitiatorId)
					.Select(member => new
					{
						member.Id,
						member.Nickname,
					})
					.FirstOrDefault()
			})
			.Select(team => new
			{
				team.Id,
				team.Initiator,
				Events = _appQueryContext.Events
					.AsSplitQuery()
					.Where(e => e.TeamId == team.Id && e.ToUtc > from)
					.Include(e => e.EventResponses)
					.Select(e => new EventSlimResponse
					{
						Id = e.Id,
						Description = e.Description,
						FromUtc = e.FromUtc,
						ToUtc = e.ToUtc,
						Status = e.Status,
						MeetTime = e.MeetTime,
						ReplyClosingTimeBeforeMeetTime = e.ReplyClosingTimeBeforeMeetTime,
						InitiatorResponse = e.EventResponses
							.Where(er => er.TeamMemberId == team.Initiator!.Id)
							.Select(er => new EventResponseResponse
							{
								TeamMemberId = team.Initiator!.Id,
								Message = er.Message,
								TeamMemberNickname = team.Initiator.Nickname,
								TimeStampUtc = er.TimeStampUtc,
								Type = er.ReplyType,
							})
							.FirstOrDefault(),
						ReplyCount = e.EventResponses
							.GroupBy(er => er.ReplyType)
							.Select(x => new ReplyCountResponse
							{
								Type = x.Key,
								Count = x.Count()
							})
							.ToList(),
						EventType = team.EventTypes.First(et => et.Id == e.EventTypeId).Name
					})
					.OrderBy(e => e.FromUtc)
					.ToList()
			})
			.FirstOrDefaultAsync(ct);

		return team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.EnsureNotNull(team => team.Initiator, TeamErrors.NotMemberOfTeam)
			.Then(team => team.Events);
	}
}
