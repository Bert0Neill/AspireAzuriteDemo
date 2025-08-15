var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Azurite_APIs>("azurite-APIs");
builder.AddProject<Projects.Azurite_BlazorWasmApp>("azurite-BlazorWasmApp");

builder.Build().Run();
