using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.Models;

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

// Password hashing utility (from Microsoft.AspNetCore.Identity) used by
// AuthController/UsersController/ProfileController. We only use the hasher
// itself, not the full Identity membership system.
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

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

    // One-time (idempotent) data fix: hash any plaintext passwords still left
    // over from before password hashing was introduced. See PasswordMigration
    // for full details on why this is safe to run on every startup.
    PasswordMigration.HashPlaintextPasswords(dbContext);
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
