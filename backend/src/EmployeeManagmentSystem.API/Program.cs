using EmployeeManagmentSystem.API.Configurations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiDependencies(builder.Configuration);

var app = builder.Build();
app.UseApiConfiguration();
app.Run();
