# Code Review & Refactoring Log

*   **AuditTag**: `PostPhase.Refactor`

This document logs code smells, silent code, and other issues found during the final integration review, along with the actions taken.

---
### 1. Missing Code / Incomplete Implementation
*   **Location**: `SymbolLabsForge.CLI/Program.cs` (multiple command handlers)
*   **Issue**: Many CLI commands execute but have no effect beyond printing to the console. For example, `deploy-replay`, `propose-governance-shift`, and `archive-capsule` are placeholders.
*   **Analysis**: "This executes, but nothing happens — what was your intent here?" The intent was to establish the full CLI surface area and governance workflow conceptually.
*   **Next Step**: "This section is missing the actual implementation — what’s the next step?" The next step is to implement the backend logic for each of these commands (e.g., connecting to a real deployment target, running the actual governance proposal engine).

---
### 2. Code Smell: Magic Numbers
*   **Location**: `SymbolLabsForge/Validation/DensityValidator.cs`
*   **Issue**: The validator uses hardcoded `0.05f` and `0.12f` for density thresholds. This works, but it’s brittle. If these values need to be changed, a developer has to hunt them down in the code.
*   **Refactoring Action**: I will refactor these magic numbers into named constants to improve readability and maintainability.

---
### 3. Code Smell: Brittle Asset Path
*   **Location**: `SymbolLabsForge/Generation/PixelBlendMorphEngine.cs`
*   **Issue**: The morphing engine hardcodes the path to the `TestAssets/Snapshots` directory. This works, but it’s brittle. It tightly couples the engine to a specific directory structure.
*   **Refactoring Action**: I will refactor the engine to receive the root asset path via its constructor, allowing it to be configured through Dependency Injection.

---
### 4. Wrong Namespace
*   **Location**: `SymbolLabsForge.CLI/Program.cs`
*   **Issue**: The CLI project has a `using SymbolLabsForge.Tests.Utils;` statement to access `SnapshotComparer`. Test utilities should not be referenced by a production CLI.
*   **Analysis**: "Check the method signature — are you passing the right type/number of parameters?" In this case, the project is referencing the wrong assembly entirely.
*   **Refactoring Action**: I will move `SnapshotComparer` and `ImageDiffGenerator` from the `.Tests` project into the core `SymbolLabsForge` library under a `Utils` namespace to make them properly accessible.
