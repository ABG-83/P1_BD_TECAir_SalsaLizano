using TECAir.Data.Connection;
// using TECAir.Data.Interfaces;
// using TECAir.Data.Repositories;
// using TECAir.Core.Interfaces;
// using TECAir.Core.Services;
// using TECAir.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar el Factory como Singleton
builder.Services.AddSingleton<DbConnectionFactory>();

// Repositories
// builder.Services.AddScoped<IUserRepository, UserRepository>();
// builder.Services.AddScoped<IFlightRepository, FlightRepository>();

// Services
// builder.Services.AddScoped<IUserService, UserService>();
// builder.Services.AddScoped<IFlightService, FlightService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TECAir API v1");
        c.RoutePrefix = "swagger";
    });
}

// Middleware Global Exceptions
// app.UseMiddleware<ExceptionMiddleware>();

// app.UseHttpsRedirection();

// CORS
app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

app.UseAuthorization();

// Map Controllers
app.MapControllers();

app.Run();
