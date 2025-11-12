# SymbolLabsForge - Test Harness Documentation

This document outlines the structure and purpose of the test harness for the SymbolLabsForge project.

## 1. How to Run Tests

Tests can be run using the .NET CLI or through a test runner in an IDE like Visual Studio.

### Using the .NET CLI

Navigate to the root of the solution (`/mnt/e/ISP/Programs/SymbolLabsForge`) and run:

```bash
dotnet test
```

This command will discover and execute all tests in the `SymbolLabsForge.Tests` project.

## 2. Test Project Structure & Coverage

The test project is organized by the component being tested.

### `Generators/`
*   **Purpose**: Validates the raw image output of each `ISymbolGenerator`.
*   **Test Cases**:
    *   `GenerateRawImage_WithValidDimensions_ReturnsImageOfCorrectSize`: Confirms the output image matches the requested dimensions.
    *   `GenerateRawImage_WithInvalidDimensions_ThrowsException`: Ensures that zero or negative dimensions are handled gracefully.
    *   `GenerateRawImage_IsNotEmptyAndContainsBlackPixels`: A sanity check to ensure the drawing logic executed and the canvas is not blank.
*   **Contributor Notes**: When adding a new generator, create a corresponding test file and replicate these tests. Add new tests specific to the symbol's geometry if necessary. See the **Generator-Specific Test Patterns** section below.

### `Validation/`
*   **Purpose**: Ensures `IValidator` services correctly calculate metrics and apply thresholds.
*   **Test Cases (`DensityValidatorTests`)**:
    *   `Validate_WithDensityBelowThreshold_ReturnsFail`: Checks the lower boundary condition.
    *   `Validate_WithDensityWithinThreshold_ReturnsPass`: Checks a known-good density value.
    *   `Validate_WithDensityAboveThreshold_ReturnsFail`: Checks the upper boundary condition.
*   **Contributor Notes**: When adding a new validator, create a test file with tests for its boundary conditions. Use the `CreateTestImage` helper to produce images with precise metrics. See the **Validator Edge-Case Checklist** below.

### `Export/`
*   **Purpose**: Validates that the `CapsuleExporter` saves files correctly.
*   **Test Cases (`CapsuleExporterTests`)**:
    *   `ExportAsync_CreatesPngAndJsonFiles_WithCorrectNaming`: Confirms that both `.png` and `.json` files are created and that their names follow the `template-form.ext` convention. Also validates a few key fields in the JSON output.
*   **Contributor Notes**: If new export formats are added, extend these tests to validate their output.

### `Registry/`
*   **Purpose**: Validates the logic of the `CapsuleRegistryManager`.
*   **Test Cases (`CapsuleRegistryManagerTests`)**:
    *   `AddEntryAsync_AddsNewEntry_ToEmptyRegistry`: Confirms a new entry can be added.
    *   `AddEntryAsync_DoesNotAddDuplicateEntry`: Ensures that adding a capsule with a pre-existing `CapsuleId` does not create a duplicate entry.
*   **Contributor Notes**: As the registry becomes more complex (e.g., SQLite backend), expand these tests to cover queries and updates.

### `Integration/`
*   **Purpose**: Performs end-to-end tests of the entire `ISymbolForge` pipeline.
*   **Test Cases (`SymbolForgeEndToEndTests`)**:
    *   `Generate_AllSymbolTypes_EndToEnd_Succeeds`: Runs the full pipeline for each symbol type, from request to final validation, ensuring no exceptions are thrown and the final capsule is marked as valid.
*   **Contributor Notes**: When a new `SymbolType` is added, ensure it is included in the `Theory` data for the end-to-end test.

### `TestAssets/`
*   **Purpose**: Contains known-good images and data used for regression testing and validating processing algorithms (e.g., skeletonization).
*   **Contributor Notes**: When fixing a bug caused by a specific input, add that input to this folder and reference it in a new regression test. See `TestAssets/README.md` for more details.

## 3. Visual Snapshot Testing

For a visual generation library, logical tests are not enough. We must also verify the pixel-level output to catch visual regressions. This is achieved through snapshot testing.

### Rationale
A snapshot test compares the output of a generator to a "golden master" or "snapshot" image that has been manually verified and committed to the `TestAssets/Snapshots` directory. This ensures that any code change that unexpectedly alters the visual output will cause a test failure.

### Contributor Workflow
1.  **Add a Snapshot Test**: When adding a new generator or a new visual variant, add a snapshot test case like the one in `FlatGeneratorTests.cs`.
2.  **First Run**: The first time you run the test, it will fail because the snapshot file doesn't exist. The test will automatically create the snapshot (e.g., `TestAssets/Snapshots/Generators/MyNewGenerator_Expected.png`).
3.  **Manual Verification**: **This is the most critical step.** You must manually open the newly created snapshot image and verify that it is 100% correct. If it is, commit the new snapshot file to source control.
4.  **Subsequent Runs**: Now, the test will pass as long as the generator's output matches the verified snapshot.
5.  **Handling Regressions**: If your code change causes a snapshot test to fail, a diff image will be generated in the `TestAssets/Diffs` directory.
    *   **Inspect the Diff**: Open the diff image to see exactly which pixels changed.
    *   **Intentional Change?**: If the change is intentional and correct, delete the old snapshot file from the `TestAssets/Snapshots` directory and re-run the test. It will fail again, but this time it will generate the *new* correct snapshot. Manually verify the new snapshot and commit it.
    *   **Unintentional Change?**: If the change was a bug, fix your code until the test passes against the original snapshot.

## 7. CLI Test Automation and Scaffolding



To streamline developer workflows, the Forge includes a command-line interface (CLI) for running targeted tests and scaffolding new test files.



### Running Tests (`forge test`)

The `test` command is a wrapper around `dotnet test` that uses the test traits (`Category`, `SymbolType`, `AuditTag`) to filter which tests are run.



**Usage:**

```bash

# Run all tests for the Validator category

dotnet run --project SymbolLabsForge.CLI -- test --component Validator



# Run all tests for the Flat symbol type

dotnet run --project SymbolLabsForge.CLI -- test --variant Flat



# Run all tests from a specific phase

dotnet run --project SymbolLabsForge.CLI -- test --audit-tag Phase2.12



# Combine filters

dotnet run --project SymbolLabsForge.CLI -- test --component Generator --variant Clef

```



### Scaffolding New Tests (`forge scaffold-test`)

The `scaffold-test` command creates a new test file from a template, ensuring it includes the required audit metadata.



**Usage:**

```bash

# Scaffold a new unit test for a component named "MyGenerator"

dotnet run --project SymbolLabsForge.CLI -- scaffold-test --component MyGenerator --type unit

```

This will create a new file `SymbolLabsForge.Tests/Generators/MyGeneratorTests.cs` with a placeholder test and the required metadata header, ready for you to implement.



## 8. Audit Trace Protocols





To ensure the Forge is production-ready, we validate its performance and stability under load. These tests are integrated into our CI pipeline.



### Performance Benchmarking

*   **Purpose**: To measure and track the execution time and memory allocation of critical components. This helps identify performance regressions.

*   **Tooling**: `BenchmarkDotNet`.

*   **Location**: `SymbolLabsForge.Benchmarks/` project.

*   **Execution**: Benchmarks are not run as part of the standard `dotnet test` command. They must be run explicitly:

    ```bash

    dotnet run --project SymbolLabsForge.Benchmarks --configuration Release

    ```

*   **Contributor Notes**: When modifying a performance-critical path (e.g., a core drawing or processing algorithm), please run the benchmarks and be prepared to discuss any significant changes in the results.



### Stress Testing

*   **Purpose**: To verify the stability of the Forge when handling a high volume of concurrent operations.

*   **Location**: `SymbolLabsForge.Tests/Integration/StressTests.cs`.

*   **Scenarios**:

    *   `Generate_100SymbolsInParallel_DoesNotCrash`: Simulates many simultaneous requests to the `ISymbolForge` service.

*   **Contributor Notes**: If you suspect a change might introduce a race condition or excessive memory usage, add a new parallel scenario to this test file.



### CI Integration

*   **Purpose**: To automatically build and test every commit and pull request.

*   **Workflow File**: `.github/workflows/ci.yml`.

*   **Process**:

    1.  The workflow checks out the code.

    2.  It builds the entire solution.

    3.  It runs all tests via `dotnet test`.

    4.  (Future) It will run benchmarks and upload test/benchmark reports as artifacts.

*   **Contributor Feedback**: If your pull request fails the CI check, review the logs in the "Actions" tab on GitHub. Failures will include standard test output and error messages.



## 11. Test Replay and Failure Reproduction







To ensure regressions can be debugged efficiently and traceably, the Forge provides a framework for replaying failed tests from their generated artifacts.







### Test Replay via CLI (`forge replay-test`)



*   **Purpose**: To allow any contributor (or a CI process) to reproduce a test failure using the exact metadata from a failed run.



*   **Workflow**:



    1.  A test fails, producing a `.json` capsule and a `.png` image.



    2.  The contributor runs the replay command, pointing to the failed `.json` file.



        ```bash



        dotnet run --project SymbolLabsForge.CLI -- replay-test --capsule "path/to/failed-capsule.json"



        ```



    3.  The CLI will:



        a. Load the capsule and reconstruct the original `SymbolRequest`.



        b. Re-run the generation.



        c. Compare the newly generated image with the original image.



        d. Report `PASS` if they match (indicating a flaky test or environmental issue) or `FAIL` if the bug is reproducible. A `diff.png` will be generated on failure.







### Programmatic Failure Reproduction



*   **Purpose**: To write specific regression tests that codify a bug and prevent it from recurring.



*   **Pattern**:



    1.  **Arrange**: Place the known-bad `.json` and `.png` artifacts into the `TestAssets/Regressions` folder.



    2.  **Act**: Use the `CapsuleLoader.LoadFromFileAsync()` utility to load the capsule in a test.



    3.  **Assert**: Run the capsule through the part of the pipeline that *should have failed* and assert that it now passes due to the bug fix.



*   **Location**: `SymbolLabsForge.Tests/RegressionTests.cs`.







### Regression Trace (`RegressionTrace.md`)



*   **Purpose**: To provide a high-level, auditable log of significant bugs, tracking when they were introduced and when they were resolved.



*   **Process**: When a significant regression is fixed, a manual entry should be added to `SymbolLabsForge.Docs/RegressionTrace.md` as part of the pull request.




















