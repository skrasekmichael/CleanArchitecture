using TeamUp.Contracts.Events;

namespace TeamUp.EndToEndTests.EndpointTests.Events;

public abstract class BaseEventTests : BaseEndpointTests
{
	protected BaseEventTests(TeamApiWebApplicationFactory appFactory) : base(appFactory) { }

	protected static bool ResponseHasCorrectReply(EventResponseResponse err, Event @event)
	{
		var reply = @event.EventResponses.Single(er => er.TeamMemberId == err.TeamMemberId).Reply;
		return reply.Type == err.Type && reply.Message == err.Message;
	}

	protected static bool ResponseIsFromMemberWithCorrectNickname(EventResponseResponse err, Team team) => err.TeamMemberNickname == team.Members.Single(m => m.Id == err.TeamMemberId).Nickname;
}
