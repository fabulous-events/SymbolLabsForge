# Forge UI Tool - Usage Guide

*   **Status**: In Development (Blocked by Build Failure)
*   **AuditTag**: `Phase3.2`

## Overview
The Forge UI Tool provides a graphical interface for generating and inspecting symbol capsules. It is designed to be a contributor-safe way to interact with the core `SymbolLabsForge` library.

## Getting Started
1.  Launch the `SymbolLabsForge.Tool.exe`.
2.  The main window will present the "Symbol Request" form.

## Features

### 1. Symbol Request
*   **Symbol Type**: Select the desired symbol (`Flat`, `Clef`, etc.) from the dropdown.
*   **Dimensions**: Specify the `Width` and `Height` for the output image.
*   **Generate**: Click this button to start the generation process.

### 2. Results Viewer
After generation, the Results Viewer window will appear.
*   **Preview**: An image of the generated symbol.
*   **Validation Log**: A detailed log of each validator that was run, its status (`PASS`/`FAIL`), and any failure messages.
*   **Save Capsule**: Exports the generated symbol image and its metadata `.json` file to a directory of your choice.

## Contributor-Safe Usage
*   **Reset to Standards**: (Planned) This button will load default, known-good dimensions and settings from a configuration file, ensuring reproducible results.
*   **Validation Logs**: Always review the validation logs. A `FAIL` status indicates a problem with the generation process that needs to be investigated.
