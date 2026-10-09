using Lab02.UsersApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.PropertyNamingPolicy = null);
builder.Services.Configure<RouteHandlerOptions>(options =>
    options.ThrowOnBadRequest = true);
var dbPath = Path.GetFullPath(builder.Configuration["DatabasePath"]
    ?? "artifacts/users.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseSqlite(new SqliteConnectionStringBuilder
        { DataSource = dbPath }.ToString()));
var app = builder.Build();
using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<UsersDbContext>()
        .Database.EnsureCreatedAsync();

// Binding and routing errors use JSON as well as application errors.
app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (BadHttpRequestException error)
    {
        await Results.Json(new ErrorResponse("Invalid JSON request or content type"),
            statusCode: error.StatusCode).ExecuteAsync(context);
    }
});
app.UseStatusCodePages(async status =>
{
    await Results.Json(new ErrorResponse("HTTP request failed"),
        statusCode: status.HttpContext.Response.StatusCode)
        .ExecuteAsync(status.HttpContext);
});

app.MapGet("/", () => Results.Ok(new
    { Name = "Lab02 Users API", Routes = new[] { "/user", "/user/{id}" } }));

app.MapPost("/user", async Task<IResult> (
    UserWriteRequest request, UsersDbContext db, CancellationToken ct) =>
{
    if (UserValidation.Validate(request) is { } error)
        return Results.BadRequest(new ErrorResponse(error));
    if (await db.Users.AnyAsync(user => user.Login == request.Login, ct))
        return Results.Conflict(new ErrorResponse("Login already exists"));
    var user = new User { Login = request.Login!,
        PassHash = request.PassHash!.ToLowerInvariant() };
    db.Users.Add(user);
    try { await db.SaveChangesAsync(ct); }
    catch (DbUpdateException ex) when (IsUniqueViolation(ex))
    { return Results.Conflict(new ErrorResponse("Login already exists")); }
    return Results.Created($"/user/{user.Id}", new UserResponse(user.Id, user.Login));
});

app.MapGet("/user/{id:int}", async Task<IResult> (
    int id, UsersDbContext db, CancellationToken ct) =>
{
    if (id <= 0) return Results.BadRequest(new ErrorResponse("Id must be positive"));
    var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(
        user => user.Id == id, ct);
    return user is null
        ? Results.NotFound(new ErrorResponse("User not found"))
        : Results.Ok(new UserResponse(user.Id, user.Login));
});

app.MapPut("/user/{id:int}", async Task<IResult> (
    int id, UserWriteRequest request, UsersDbContext db, CancellationToken ct) =>
{
    if (id <= 0) return Results.BadRequest(new ErrorResponse("Id must be positive"));
    if (UserValidation.Validate(request) is { } error)
        return Results.BadRequest(new ErrorResponse(error));
    var user = await db.Users.SingleOrDefaultAsync(user => user.Id == id, ct);
    if (user is null) return Results.NotFound(new ErrorResponse("User not found"));
    if (await db.Users.AnyAsync(other => other.Id != id &&
        other.Login == request.Login, ct))
        return Results.Conflict(new ErrorResponse("Login already exists"));
    user.Login = request.Login!;
    user.PassHash = request.PassHash!.ToLowerInvariant();
    try { await db.SaveChangesAsync(ct); }
    catch (DbUpdateException ex) when (IsUniqueViolation(ex))
    { return Results.Conflict(new ErrorResponse("Login already exists")); }
    return Results.Ok(new UserResponse(user.Id, user.Login));
});

app.MapDelete("/user/{id:int}", async Task<IResult> (
    int id, UsersDbContext db, CancellationToken ct) =>
{
    if (id <= 0) return Results.BadRequest(new ErrorResponse("Id must be positive"));
    var user = await db.Users.SingleOrDefaultAsync(user => user.Id == id, ct);
    if (user is null) return Results.NotFound(new ErrorResponse("User not found"));
    db.Users.Remove(user);
    await db.SaveChangesAsync(ct);
    return Results.Ok(new DeleteResponse(id, "User deleted"));
});

app.Run();

static bool IsUniqueViolation(DbUpdateException error) =>
    error.InnerException is SqliteException
        { SqliteErrorCode: 19, SqliteExtendedErrorCode: 2067 };
