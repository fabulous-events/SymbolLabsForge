# Program.cs Fix Checklist

*   **AuditTag**: `PostPhase.Refactor`

This checklist tracks the resolution of build errors in `SymbolLabsForge.CLI/Program.cs`.

| Task                                  | Status      | Notes                                                              |
|---------------------------------------|-------------|--------------------------------------------------------------------|
| Rename duplicate `targetOption`       | ✅ Complete | Use unique names like `targetOptionDeploy` and `targetOptionSync`. |
| Correct `Option<T>` types             | ✅ Complete | Ensure all options use explicit generic types (e.g., `Option<bool>`). |
| Correct `SetHandler` delegate signatures | ✅ Complete | Match lambda parameters to the number and types of options.        |
| Correct default value lambdas         | ✅ Complete | Use `() => value` syntax for default values.                       |
| Resolve missing `using` statements    | ✅ Complete | Add `using SymbolLabsForge.Services;` for Exporter/Manager.        |
