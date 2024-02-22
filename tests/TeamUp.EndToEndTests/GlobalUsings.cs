global using System.Net.Http.Json;

global using Bogus;

global using FluentAssertions;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.DependencyInjection;

global using TeamUp.Domain.Aggregates.Events;
global using TeamUp.Domain.Aggregates.Invitations;
global using TeamUp.Domain.Aggregates.Teams;
global using TeamUp.Domain.Aggregates.Users;
global using TeamUp.EndToEndTests.DataGenerators;
global using TeamUp.EndToEndTests.Extensions;
global using TeamUp.Infrastructure.Persistence;
global using TeamUp.TestsCommon;

global using Xunit;

global using FExt = TeamUp.TestsCommon.FakerExtensions;
