using WorkflowFormEngine.Components;
using WorkflowFormEngine.Providers;
using WorkflowFormEngine.Services;
using WorkflowFormEngine.WorkflowEngine;

var builder = WebApplication.CreateBuilder(args);

// ── Blazor ────────────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── Workflow Engine ───────────────────────────────────────────────────────────
// WorkflowLoader now reads files via IWebHostEnvironment (injected automatically).
// No circular HTTP call — reads directly from the wwwroot folder on disk.
builder.Services.AddScoped<WorkflowLoader>();

// ── Validation ────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<ValidationService>();

// ── Data Source Providers ─────────────────────────────────────────────────────
// All IDataSourceProvider registrations are collected by OptionService via DI.
// To add a new data source: add one line here — nothing else changes.
builder.Services.AddScoped<IDataSourceProvider, CountryProvider>();
builder.Services.AddScoped<IDataSourceProvider, CityProvider>();
builder.Services.AddScoped<IDataSourceProvider, ContactMethodsProvider>();
builder.Services.AddScoped<IDataSourceProvider, InterestsProvider>();
builder.Services.AddScoped<IDataSourceProvider, PrimaryInterestProvider>();
builder.Services.AddScoped<IDataSourceProvider, ContactTimeProvider>();
builder.Services.AddScoped<IDataSourceProvider, NotificationFrequencyProvider>();
builder.Services.AddScoped<IDataSourceProvider, NewsletterTopicsProvider>();

// ── Option Service ────────────────────────────────────────────────────────────
builder.Services.AddScoped<OptionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
