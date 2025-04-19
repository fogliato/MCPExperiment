using MCPEcommerce.Core.Interfaces;
using MCPEcommerce.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Configure Gemini API key
var geminiApiKey = builder.Configuration["GeminiApi:ApiKey"];
if (string.IsNullOrEmpty(geminiApiKey))
{
    throw new InvalidOperationException("Gemini API key is not configured");
}

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string is not configured");
}

// Register services
builder.Services.AddSingleton<IDatabaseService>(sp => 
    new DatabaseService(connectionString));
builder.Services.AddSingleton<IDatabaseSchemaService>(sp => 
    new DatabaseSchemaService(connectionString));
builder.Services.AddSingleton<IAIService>(sp => 
    new AIService(geminiApiKey, sp.GetRequiredService<IDatabaseSchemaService>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
