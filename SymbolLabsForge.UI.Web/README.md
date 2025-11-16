# SymbolLabsForge UI - Symbol Comparison Tool

**Phase 10.1: Blazor Server Foundation**
**Status:** 🚧 SCAFFOLDING COMPLETE
**Date:** 2025-11-15

---

## Overview

An interactive web application that allows students to **upload or generate musical symbols** (sharp, flat, natural, etc.) and **compare them against SymbolLabsForge's canonical outputs** using production-grade ComparisonUtils (Phase 9.1) and PixelBlender (Phase 9.2).

**Teaching Mission:** Turn symbol comparison into a **teachable, interactive experience** with visual feedback, tolerance configuration, and comprehensive error handling.

---

## For Undergraduate Students

### What You'll Learn
- Web UI development with Blazor Server
- Image upload and processing workflows
- Event handling and data binding
- Service layer architecture and dependency injection
- Input validation and error handling

### Getting Started

#### Prerequisites
- .NET 9.0 SDK
- Visual Studio 2022 or VS Code (with C# extension)
- Web browser (Chrome, Firefox, Edge)

#### Setup Steps

1. **Navigate to UI project:**
   ```bash
   cd /mnt/e/ISP/Programs/SymbolLabsForge/SymbolLabsForge.UI.Web
   ```

2. **Restore packages:**
   ```bash
   dotnet restore
   ```

3. **Build the project:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run
   ```

5. **Open your browser:**
   - Navigate to: `https://localhost:5001`
   - Or: `http://localhost:5000`

6. **Upload or generate a symbol:**
   - **Option 1:** Upload a PNG/JPG image of a musical symbol
   - **Option 2:** Use the Synthetic Symbol Generator to create one

7. **Compare against canonical:**
   - Select symbol type (Sharp, Flat, Natural)
   - Adjust tolerance slider (1% = typical, 5% = lenient)
   - Click "Compare Symbols"

8. **View results:**
   - ✅ PASS or ❌ FAIL status
   - Similarity percentage (e.g., 98.5%)
   - Visual diff image (red = differences, gray = matches)
   - Statistics panel (total pixels, different pixels, max error)

### Key Concepts

**Tolerance:** How much difference is acceptable between your symbol and the canonical.
- `0.0%` = Exact match (pixel-perfect)
- `1.0%` = Typical (99% similar) → **Default**
- `5.0%` = Lenient (95% similar) → For hand-drawn symbols

**Similarity Score:** Percentage of pixels that match the canonical symbol.
- `100%` = Perfect match
- `98.5%` = 1.5% of pixels differ (likely still acceptable)
- `85.0%` = 15% of pixels differ (may fail comparison)

**Diff Image:** 3-panel visualization:
1. **Expected (Canonical):** SymbolLabsForge's reference output
2. **Actual (Your Upload):** Your symbol
3. **Diff (Red Highlights):** Changed pixels highlighted in red

---

## For Graduate Students

### What You'll Learn
- Service layer architecture (ComparisonService, SymbolGenerationService)
- Blazor Server vs. ASP.NET Core MVC trade-offs
- Image processing pipeline integration (Phase 9 utilities)
- Caching strategies for performance (IMemoryCache)
- Dependency injection lifetime management (singleton, scoped, transient)

### Architecture Overview

```
┌─────────────────────────────────────────────┐
│  Student Browser (Blazor Components)        │
│  - Upload symbol image OR generate synthetic│
│  - Configure tolerance slider               │
│  - View comparison results                  │
└─────────────────────────────────────────────┘
                    ↕ SignalR
┌─────────────────────────────────────────────┐
│  Blazor Server (ASP.NET Core)               │
│  ┌─────────────────────────────────────┐   │
│  │  ComparisonService (Scoped)         │   │
│  │  - Validates uploaded images        │   │
│  │  - Loads canonical from cache       │   │
│  │  - Compares via SnapshotComparer    │   │
│  │  - Generates diff via ImageDiffGen  │   │
│  └─────────────────────────────────────┘   │
│  ┌─────────────────────────────────────┐   │
│  │  SymbolGenerationService (Scoped)   │   │
│  │  - Pre-generates canonical symbols  │   │
│  │  - Caches in IMemoryCache           │   │
│  │  - Generates synthetic symbols      │   │
│  └─────────────────────────────────────┘   │
│              ↓                              │
│  ┌─────────────────────────────────────┐   │
│  │  Generators (Singleton)             │   │
│  │  - SharpGenerator                   │   │
│  │  - FlatGenerator                    │   │
│  │  - NaturalGenerator                 │   │
│  └─────────────────────────────────────┘   │
└─────────────────────────────────────────────┘
            ↓
┌─────────────────────────────────────────────┐
│  Phase 9 Utilities (Referenced Libraries)   │
│  - SnapshotComparer.AreSimilar()            │
│  - ImageDiffGenerator.CreateDiffImage()     │
│  - PixelBlender (future enhancements)       │
└─────────────────────────────────────────────┘
```

### Project Structure

```
SymbolLabsForge.UI.Web/
├── Components/               # Blazor components (Pages, Layouts)
├── Services/                 # Service layer (business logic)
│   ├── ComparisonService.cs      # Symbol comparison workflow
│   └── SymbolGenerationService.cs # Canonical symbol management
├── wwwroot/                  # Static files
│   ├── uploads/                  # Temporary uploaded images
│   ├── diffs/                    # Generated diff images
│   └── generated/                # Synthetic symbols
├── Program.cs                # DI configuration, app startup
├── appsettings.json          # Configuration (paths, defaults)
└── README.md                 # This file
```

### Key Design Decisions

**Why Blazor Server (not MVC)?**
- Real-time updates via SignalR (tolerance slider with live feedback)
- Component-based architecture (reusable UI building blocks)
- Modern framework aligned with industry trends
- Higher teaching value (Graduate → PhD)

**Why Pre-Generate Canonical Symbols?**
- **Performance:** Canonical symbols cached in memory (< 1ms retrieval)
- **Consistency:** Same canonical output for all students
- **Versioning:** Can track canonical symbol versions (e.g., `sharp_v1.0.png`)

**Why Project References (not NuGet)?**
- Easier debugging (step into utility code)
- Always uses latest code (no version mismatch)
- Simpler for educational setting

### Experiments

1. **Compare MVC vs. Blazor:**
   - Convert `ComparisonService` to MVC controller
   - Measure response time difference (full page refresh vs. SignalR)

2. **Test Tolerance Calibration:**
   - Upload same symbol 10 times with different tolerances (0%, 1%, 5%, 10%)
   - Observe pass/fail threshold behavior

3. **Caching Performance:**
   - Measure canonical symbol generation time (first request)
   - Measure cache retrieval time (subsequent requests)
   - Expected: 50-100ms generation, <1ms retrieval

4. **Custom Diff Visualization:**
   - Replace ImageDiffGenerator's red highlighting with heatmap mode
   - Use color gradient (blue = small difference, red = large difference)

---

## For PhD Students

### What You'll Learn
- Real-time web communication (SignalR architecture)
- Performance optimization (image caching, async processing, memory management)
- Scalability considerations (multi-user hosting, load balancing)
- Error handling strategies (fail-fast validation, graceful degradation)

### Research Opportunities

#### 1. Performance Benchmarking
**Hypothesis:** Canonical symbol caching reduces comparison latency by 95%+.

**Experiment:**
- Measure comparison time WITHOUT caching (generate + compare)
- Measure comparison time WITH caching (retrieve + compare)
- Simulate 100 concurrent users
- Expected: Uncached ~100ms, Cached ~5ms

**Metrics:**
- P50, P95, P99 latency
- Memory usage (cache size growth)
- Cache hit rate (should be ~100% for canonical symbols)

---

#### 2. Perceptual Hashing
**Hypothesis:** Pixel-by-pixel comparison fails for anti-aliased symbols. Perceptual hashing (pHash) is more robust.

**Experiment:**
- Compare sharp symbol with/without anti-aliasing
- Pixel-by-pixel: Likely 70-80% similar (anti-aliasing introduces grayscale pixels)
- pHash: Should be ~95%+ similar (perceptual similarity)

**Implementation:**
- Replace SnapshotComparer with pHash-based comparison
- Calculate Hamming distance between pHash values
- Tune threshold for acceptable perceptual similarity

---

#### 3. GPU Acceleration
**Hypothesis:** Diff image generation can be parallelized on GPU for 10x+ speedup.

**Experiment:**
- Implement pixel differencing using Compute Shaders (DirectX/Vulkan)
- Benchmark CPU vs. GPU for large images (e.g., 1000×2000 pixels)
- Expected: CPU ~50ms, GPU ~5ms

---

#### 4. Adaptive Tolerance
**Hypothesis:** Optimal tolerance varies by symbol type. ML model can auto-calibrate.

**Experiment:**
- Collect ground truth dataset (100+ symbols per type, labeled "match" or "no match")
- Train binary classifier (XGBoost, Random Forest) on features:
  - Symbol type
  - Difference percentage
  - Max pixel error
  - Mean pixel error
  - Aspect ratio
- Predict optimal tolerance threshold for each symbol type

**Expected Outcomes:**
- Sharp symbols: Lower tolerance (1%) due to geometric precision
- Hand-drawn symbols: Higher tolerance (5-10%) due to natural variation

---

#### 5. SignalR Scaling
**Hypothesis:** SignalR server can handle 1000+ concurrent users with Redis backplane.

**Experiment:**
- Deploy Blazor Server to Azure App Service (2 instances)
- Configure Redis cache as SignalR backplane
- Simulate 1000 concurrent users uploading symbols
- Measure: connection stability, message latency, memory usage

**Metrics:**
- Concurrent connections supported
- Message propagation latency (SignalR → client)
- Memory per connection (target: <100 KB)

---

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "SymbolLabsForge.UI.Web.Services": "Debug"
    }
  },
  "AllowedHosts": "*",
  "CanonicalSymbols": {
    "Sharp": { "Width": 20, "Height": 50 },
    "Flat": { "Width": 12, "Height": 30 },
    "Natural": { "Width": 20, "Height": 50 },
    "DoubleSharp": { "Width": 25, "Height": 55 },
    "Treble": { "Width": 180, "Height": 450 }
  },
  "Comparison": {
    "DefaultTolerance": 0.01,
    "MaxImageSizeMB": 5,
    "AllowedFormats": [ "png", "jpg", "jpeg" ]
  }
}
```

---

## Deployment

### Local Development
```bash
dotnet run
# Open: https://localhost:5001
```

### Azure App Service
```bash
dotnet publish --configuration Release --output ./publish
# Upload ./publish to Azure App Service via VS Code or Azure Portal
```

### Docker (Future)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SymbolLabsForge.UI.Web.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SymbolLabsForge.UI.Web.dll"]
```

---

## Troubleshooting

### Issue: "Project reference not found"
**Solution:** Ensure Phase 9 libraries are built:
```bash
cd /mnt/e/ISP/Programs/SymbolLabsForge
dotnet build
```

### Issue: "wwwroot/uploads directory not writable"
**Solution:** Ensure directory permissions:
```bash
chmod 755 wwwroot/uploads
chmod 755 wwwroot/diffs
chmod 755 wwwroot/generated
```

### Issue: "SignalR connection failed"
**Solution:** Check firewall/antivirus blocking WebSocket connections on ports 5000/5001.

### Issue: "Canonical symbol generation slow"
**Solution:** Check cache configuration in `Program.cs`. Ensure generators are registered as **singletons** (not scoped).

---

## Phase 10 Roadmap

- ✅ **Phase 10.1:** Blazor Server scaffolding + DI configuration ← **YOU ARE HERE**
- ⏳ **Phase 10.2:** Implement ComparisonService integration tests
- ⏳ **Phase 10.3:** Build Comparison.razor component (upload/tolerance/results)
- ⏳ **Phase 10.4:** Add teaching overlays and onboarding guidance
- ⏳ **Phase 10.5:** Implement logging and instructor review features
- ⏳ **Phase 10.6:** Build Synthetic Symbol Generator Form with error handling

---

## Teaching Value Summary

| Audience | Learning Outcomes | Key Concepts |
|----------|------------------|--------------|
| **Undergraduate** | Web UI, image processing, validation | Blazor components, file upload, tolerance |
| **Graduate** | Service architecture, DI, caching | Scoped vs. singleton, IMemoryCache, error handling |
| **PhD** | Performance, scalability, research | SignalR scaling, GPU acceleration, pHash, ML |

---

## Contributing

**Contributor Safety:** All generation logic stays in `SymbolLabsForge.Generation` (core library). UI is just a **thin wrapper** with validation. No direct access to internal pipelines.

**Code Standards:** Follow `docs/standards/SymbolLabs.CSharpStandards.md`.

---

**Author:** Claude (Phase 10.1 - Blazor Server Scaffolding)
**Date:** 2025-11-15
**Status:** 🚧 Foundation Complete, Ready for Phase 10.2
