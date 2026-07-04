using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<TaskHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Apply pending migrations on startup; data is preserved across restarts.
// If the database doesn't exist yet it is created automatically by Migrate().
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskHubDbContext>();

    // If the database exists but was created with EnsureCreated (no __EFMigrationsHistory table),
    // drop it cleanly before Migrate() runs so no SQL errors are logged.
    if (dbContext.Database.CanConnect())
    {
        var connection = dbContext.Database.GetDbConnection();
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '__EFMigrationsHistory'";
        var hasMigrationsTable = (int)(cmd.ExecuteScalar() ?? 0) > 0;
        connection.Close();

        if (!hasMigrationsTable)
            dbContext.Database.EnsureDeleted();
    }

    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseAuthorization();
app.MapControllers();
app.Run();
