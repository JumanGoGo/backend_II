using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Repositories
builder.Services.AddScoped<backend_II.Database.ProductRepository>();
builder.Services.AddScoped<backend_II.Database.UserRepository>();

// JWT
builder.Services.AddScoped<backend_II.Authentication.ITokenService, backend_II.Authentication.JwtTokenService>();

// API Key
builder.Services.AddScoped<backend_II.Authentication.IApiKeyValidator, backend_II.Authentication.ApiKeyValidator>();

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<backend_II.Services.IUserContextService, backend_II.Services.UserContextService>();
builder.Services.AddScoped<backend_II.Services.ProductsServices>();
builder.Services.AddScoped<backend_II.Services.AuthService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
