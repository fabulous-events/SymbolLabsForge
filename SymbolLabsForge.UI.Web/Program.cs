//===============================================================
// File: Program.cs
// Author: Claude (Phase 10.1 - Blazor Server Scaffolding)
// Date: 2025-11-15
// Purpose: Blazor Server application entry point with DI configuration.
//
// PHASE 10.1: DEPENDENCY INJECTION CONFIGURATION
//   - Registers Phase 9 utilities (SnapshotComparer, ImageDiffGenerator)
//   - Registers generators (SharpGenerator, FlatGenerator, etc.)
//   - Registers UI services (ComparisonService, SymbolGenerationService)
//   - Configures memory cache for canonical symbol caching
//
// WHY THIS MATTERS:
//   - Demonstrates clean DI architecture for web applications
//   - Students learn service lifetime management (singleton, scoped, transient)
//   - Shows integration of library utilities with UI services
//
// TEACHING VALUE:
//   - Undergraduate: DI basics, service registration patterns
//   - Graduate: Service lifetime strategies, cache configuration
//   - PhD: Performance optimization via DI, memory management
//
// AUDIENCE: Undergraduate / Graduate (DI architecture)
//===============================================================

using SymbolLabsForge.UI.Web.Components;
using SymbolLabsForge.UI.Web.Services;
using SymbolLabsForge.Generation;  // Correct namespace for most generators
using SymbolLabsForge.Generators;   // FlatGenerator outlier namespace

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//==============================================================
// PHASE 10.1: DI Configuration
//==============================================================

// 1. Memory cache for canonical symbol caching
builder.Services.AddMemoryCache();

// 2. Register generators as singletons (stateless, expensive initialization)
builder.Services.AddSingleton<SharpGenerator>();
builder.Services.AddSingleton<FlatGenerator>();
builder.Services.AddSingleton<NaturalGenerator>();
builder.Services.AddSingleton<DoubleSharpGenerator>();
builder.Services.AddSingleton<TrebleGenerator>();

// 3. Register UI services as scoped (per-request state)
builder.Services.AddScoped<SymbolGenerationService>();
builder.Services.AddScoped<ComparisonService>();
builder.Services.AddScoped<BatchComparisonService>(); // Phase 10.5: Parallel batch comparison

// 4. Register state management services (Phase 10.4)
builder.Services.AddScoped<GeneratedSymbolState>(); // State transfer: Generator → Comparison

// 5. Register post-processing services (Phase 10.4 Priority 2)
builder.Services.AddSingleton<SymbolStyleProcessor>(); // Style customization (stroke thickness, background)

// 6. Register PDF export service (Phase 10.5)
builder.Services.AddSingleton<PdfExportService>(); // PDF generation for comparison reports

// 7. Register health checks (Phase 11 - Production Deployment)
builder.Services.AddHealthChecks(); // Health endpoint for Azure App Service monitoring

//==============================================================
// End Phase 10.1 DI Configuration (Updated Phase 10.5)
//==============================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Health check endpoint (Phase 11 - Production Deployment)
// Azure App Service pings /health to verify application is running
app.MapHealthChecks("/health");

app.Run();
