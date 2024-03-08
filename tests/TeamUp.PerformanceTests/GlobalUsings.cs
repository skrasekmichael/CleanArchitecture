global using System.Net.Http.Json;

global using Bogus;

global using FluentAssertions;
global using FluentAssertions.Extensions;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;

global using TeamUp.Contracts.Events;
global using TeamUp.Contracts.Invitations;
global using TeamUp.Contracts.Teams;
global using TeamUp.Contracts.Users;
global using TeamUp.Domain.Aggregates.Events;
global using TeamUp.Domain.Aggregates.Invitations;
global using TeamUp.Domain.Aggregates.Teams;
global using TeamUp.Domain.Aggregates.Users;
global using TeamUp.TestsCommon.DataGenerators;
global using TeamUp.TestsCommon.Extensions;

global using Xunit;
