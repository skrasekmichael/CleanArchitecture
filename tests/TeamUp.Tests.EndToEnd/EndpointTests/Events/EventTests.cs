﻿namespace TeamUp.Tests.EndToEnd.EndpointTests.Events;

public abstract class EventTests(AppFixture app) : BaseEndpointTests(app)
{
	protected static bool ResponseHasCorrectReply(EventResponseResponse err, Event @event)
	{
		var response = @event.EventResponses.Single(er => er.TeamMemberId == err.TeamMemberId);
		return response.ReplyType == err.Type && response.Message == err.Message;
	}

	protected static bool ResponseIsFromMemberWithCorrectNickname(EventResponseResponse err, Team team) => err.TeamMemberNickname == team.Members.Single(m => m.Id == err.TeamMemberId).Nickname;

	protected static void EventShouldContainCorrectReplyCount(EventSlimResponse @event, List<Event> expectedEvents)
	{
		var expectedEvent = expectedEvents.First(e => e.Id == @event.Id);
		@event.ReplyCount.ShouldOnlyContain(reply => expectedEvent.EventResponses.Sum(r => r.ReplyType == reply.Type ? 1 : 0) == reply.Count);
	}
}
