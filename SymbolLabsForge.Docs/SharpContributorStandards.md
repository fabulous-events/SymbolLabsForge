# 📘 SymbolLabs C#A Standards & AI Code Output Compliance Guide
Version: 2025.09

## 🛡 Purpose

This document defines contributor-safe standards for all C# code authored, audited, or validated by Gemini AI. These standards ensure audit traceability, onboarding clarity, and legacy-grade governance across all validator logic, test suites, and capsule generators.

### Audience Segments
* 🧑‍🎓 Students learning C# and architectural discipline
* 👩‍💻 Contributors maintaining production grade systems
* 🤖 AI assisting with code generation and review

---

## 🔍 Method and Parameter Hygiene

- All methods must use explicit, narratable names
- Parameters must be typed, ordered, and documented
- No silent overloads or ambiguous signatures
- Optional parameters must be justified with onboarding rationale

---

## 🧪 Test Suite Standards

- Use **xUnit** with explicit assertions
- Avoid framework-dependent attributes (e.g., `[ExpectedException]`)
- Each test must narrate its purpose and expected outcome
- Fallback logic must be tested and documented

---

## 🧭 Validator Structure

- Validators must be modular and narratable
- Each validator must log rationale, fallback, and override triggers
- Arbitration logic must be traceable and override-safe
- Replay bundles must validate consensus and log discrepancies

---

## 📦 Artifact Expectations

- Every fix, override, and validator outcome must be codified into:
  - `OnboardingSimulationLog.md`
  - `ReplayBundleManifest.md`
  - `ValidatorRationale.md`
  - `SessionComplianceLog.md`

---

## 🛡 Governance Hooks

- Gemini must apply these standards at session start
- Deviations must be logged with rationale
- Overrides require explicit governance proposal

---

## 🔗 Integration

This file is referenced by:
- `GEMINI.md` (Session Bootstrap and Preprompt)
- `SessionComplianceLog.md` (Adherence tracking)
- `ValidatorRationale.md` (Narration source)

---

## 🧭 Section 1: File Structure & Asset Governance
* **Canonical Asset Folder**: `E:\ISP\Programs\Assets\`
* **Rule**: Never store outputs inside project folders.
    * ❌ Wrong: `bin\Debug\` or source directories
    * ✅ Right: `E:\ISP\Programs\Assets\Output\`

## 🧭 Section 2: Logging Standards
* Use `ILogger<T>` via DI.
* Replace all `Console.WriteLine` calls.
* **Severity levels**:
    * `LogTrace` → iteration details
    * `LogDebug` → template/config overrides
    * `LogInformation` → status updates
    * `LogWarning` → fallback logic, missing assets
    * `LogError` → exceptions, critical failures
* **Artifacts**:
    * `LoggingStandard.md`
    * `ConsoleWriteAudit.cs` (CI enforcement)
    * `ILoggerMigrationChecklist.md`
    * `SweepRunLog.md`

## 🧭 Section 3: LINQ Learning Roadmap
* **Level 0 (Training Wheels)**: explicit loops only.
* **Level 1 (Smooth Pedals)**: safe LINQ (`.Where()`, `.Any()`, `.Count()`)
* **Level 2 (Advanced Tricks)**: only in versioned, benchmarked components with rationale + approval.

## 🧭 Section 4: C# Coding Standards
* **Naming**: PascalCase for classes/methods, camelCase for privates, `I` prefix for interfaces.
* **Structure**: one public class per file, filename = class name.
* **Formatting**: 4 space indent, Allman braces, max line length 120, `#nullable enable`.
* **Docs**: XML comments for all public members.

## 🧭 Section 5: AI Code Output Standards
* **Full Code Definition**: Only return the exact contents of the requested file.
* **Scope**: If partial, return only that file’s content.
* **Prohibited**:
    * Speculative completions
    * Placeholder code (`throw new NotImplementedException()`)
    * Silent signature changes
    * Formatting drift
    * Overwriting unrelated files
* **Compliance Checklist (AI must confirm before output)**:
    * [ ] Exact file/class only
    * [ ] No merging from other partials
    * [ ] No speculative code
    * [ ] Formatting/comments preserved
    * [ ] API/binding integrity maintained
    * [ ] Null checks enforced
    * [ ] No missing config keys/folders
    * [ ] No output inside project folders

## 🧭 Section 6: AI Interaction Rules
* **Ask before coding**: If unsure about a signature, request the definition.
* **No invention**: Do not create new DTOs, async methods, or abstractions unless explicitly instructed.
* **Phase discipline**: Stick to current phase scope (e.g., Phase 1 = validation, logging, single staff).
* **Research first**: Verify existing contracts (StaffMetrics, ClefSettings, etc.) before generating.
* **Output discipline**:
    * Avoid full classes unless explicitly requested.
    * Small samples (≤10 lines) are fine.
    * Keep answers under 500 words unless extended.

## 🧭 Section 7: Error Prevention Rules
* Check for duplicate members across partials.
* Preserve method/property types.
* Maintain UI binding names.
* Ask before guessing.

## 🧭 Section 8: Cross Check Tips
### For Students:
* Confirm method names/parameters match exactly.
* Don’t duplicate or rename shared enums/DTOs.
* Test dependent classes together.
* Verify ViewModel bindings.
### For AI:
* Do not rename methods/properties unless explicitly requested.
* Do not add parameters to shared methods unless approved.
* Do not merge logic from other classes unless asked.
* Always preserve binding contracts and shared types.
* If unsure, ask — never guess.

## 🧭 Section 9: Specific Prohibitions
❌ No placeholder citations (`[cite]`)
❌ No stub code (`throw new NotImplementedException()`)
❌ No speculative completions
❌ No silent signature changes
❌ No formatting drift
❌ No overwriting unrelated files

✅ **Why This Matters**
Cross checking prevents architectural drift and keeps our code narratable. Every class is part of a handshake — make sure yours is clean.

## Additions to SymbolLabs C#A Standards
1.  **Naming & Spelling**
    *   **Rule**: All identifiers must use complete, correctly spelled words. Abbreviations like `Dist` are prohibited unless they are universally recognized (e.g., `Id`, `Xml`).
    *   **Rationale**: Prevents unclear naming and reduces onboarding confusion.
2.  **Method Signatures & Overloads**
    *   **Rule**: Do not define duplicate methods with identical signatures.
    *   **Rule**: If overloads are required, parameter lists must differ in type or arity.
    *   **Rationale**: Prevents CS0102/CS0229 ambiguity errors and ensures clear API contracts.
3.  **Async/Await Discipline**
    *   **Rule**: All `async` methods must contain at least one `await`.
    *   **Rule**: If no asynchronous work is performed, remove the `async` modifier.
    *   **Rule**: For CPU bound work, explicitly wrap in `await Task.Run(...)`.
    *   **Rationale**: Prevents misleading `async` signatures and ensures correct threading behavior.
4.  **Explicit Typing**
    *   **Rule**: Use explicit types instead of `var` in all public members, DTOs, and governance critical code.
    *   **Rule**: `var` is permitted only in local, obvious assignments (e.g., `var x = 5;`).
    *   **Rationale**: Improves readability, auditability, and onboarding clarity.
5.  **Collection Initialization**
    *   **Rule**: Use collection initializer syntax where possible.
        ```csharp
        // ✅ Preferred
        var list = new List<int> { 1, 2, 3 };

        // ❌ Avoid
        var list = new List<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);
        ```
    *   **Rationale**: Reduces boilerplate and improves clarity.
6.  **Enum Validation**
    *   **Rule**: Always use the generic overload `Enum.IsDefined<TEnum>(value)` instead of `Enum.IsDefined(typeof(TEnum), value)`.
    *   **Rationale**: Improves type safety and eliminates boxing.
7.  **Logging Consistency**
    *   **Rule**: Logging message templates must be consistent across calls.
    *   **Rule**: Do not vary the template string between invocations of the same log event.
    *   **Rationale**: Ensures structured logging tools can parse and aggregate correctly.
