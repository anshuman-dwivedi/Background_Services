using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

// THIS NOW WORKS
builder.Services.AddWindowsService();

var host = builder.Build();
host.Run();

