using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;

namespace TeamUp.Application.Abstractions;

public interface IAppQueryContext
{
	public IQueryable<User> Users { get; }
	public IQueryable<TeamMember> TeamMembers { get; }
	public IQueryable<Invitation> Invitations { get; }
	public IQueryable<Event> Events { get; }
}
