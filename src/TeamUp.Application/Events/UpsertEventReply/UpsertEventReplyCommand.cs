using TeamUp.Application.Abstractions;
using TeamUp.Common;
using TeamUp.Contracts.Events;
using TeamUp.Contracts.Teams;
using TeamUp.Contracts.Users;

namespace TeamUp.Application.Events.UpsertEventReply;

public sealed record UpsertEventReplyCommand(UserId InitiatorId, TeamId TeamId, EventId EventId, ReplyType ReplyType, string Message) : ICommand<Result>;
