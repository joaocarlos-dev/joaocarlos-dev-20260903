using EmployeeManagmentSystem.API.Configurations.DependencyInjection;
using EmployeeManagmentSystem.API.Configurations.Pipeline;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiDependencies(builder.Configuration);

var app = builder.Build();
await app.UseApiConfigurationAsync(builder.Configuration);
app.Run();
