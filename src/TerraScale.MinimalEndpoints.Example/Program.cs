using TerraScale.MinimalEndpoints;
// Use the generated registration helpers for this assembly
using TerraScale.MinimalEndpoints.Generated_TerraScale_MinimalEndpoints_Example;
using TerraScale.MinimalEndpoints.Example.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

// Add Auth services
builder.Services.AddAuthentication("Test")
    .AddCookie("Test", options => {
        options.Events.OnRedirectToLogin = context => {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context => {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

// Register application services
builder.Services.AddSingleton<IGreetingService, GreetingService>();
builder.Services.AddSingleton<IUserService, UserService>();

// Register Minimal Endpoints (generated)
builder.Services.AddGeneratedMinimalEndpoints();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Map Minimal Endpoints (generated)
app.MapGeneratedMinimalEndpoints();

app.Run();
