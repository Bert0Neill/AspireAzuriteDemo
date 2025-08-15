var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Azurite_APIs>("azurite-apis");

builder.Build().Run();
