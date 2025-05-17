using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Abstractions;
using TeamUp.Domain.Aggregates.Events;
using TeamUp.Domain.Aggregates.Invitations;
using TeamUp.Domain.Aggregates.Teams;
using TeamUp.Domain.Aggregates.Users;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.Infrastructure.Core;

internal sealed class ApplicationDbContextQueryFacade : IAppQueryContext
{
	private readonly ApplicationDbContext _context;

	public ApplicationDbContextQueryFacade(ApplicationDbContext context)
	{
		_context = context;
	}

	public IQueryable<User> Users => _context.Users.AsNoTracking();
	public IQueryable<Team> Teams => _context.Teams.AsNoTracking();
	public IQueryable<Invitation> Invitations => _context.Invitations.AsNoTracking();
	public IQueryable<Event> Events => _context.Events.AsNoTracking();
}
