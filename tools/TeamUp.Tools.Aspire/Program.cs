using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<TeamUp_Api>("api");

builder.Build().Run();
