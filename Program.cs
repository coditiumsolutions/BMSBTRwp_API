using BMSBTRwp_API.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── SERVICES ────────────────────────────────────────────────────────────────

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom services
builder.Services.AddSingleton<DbTextFileService>();
builder.Services.AddSingleton<DataFetchService>();

// ─── CORS ────────────────────────────────────────────────────────────────────
// Allow React Native (and any mobile/web client) to call this API.
// In production, replace AllowAnyOrigin with specific origins.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ─── MIDDLEWARE ───────────────────────────────────────────────────────────────

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
