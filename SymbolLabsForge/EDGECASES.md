# Edge Cases Specification

This document outlines the edge cases that the SymbolLabsForge generator should handle.

## 1. Clipped

-   **Description:** The symbol is partially cut off at one of the edges of the image.
-   **Parameters:**
    -   `side`: top, bottom, left, right
    -   `percentage`: 0-100

## 2. Rotated

-   **Description:** The symbol is rotated by a certain angle.
-   **Parameters:**
    -   `angle`: -180 to 180 degrees

## 3. Ink Bleed

-   **Description:** The ink has bled, making the lines thicker and less distinct.
-   **Parameters:**
    -   `amount`: 0.0 to 1.0

## 4. Unsupported File Types

-   **Description:** The system should gracefully handle requests for unsupported output file types.
-   **Behavior:** Log an error and return a fallback capsule.

## 5. Read-Permission Errors

-   **Description:** The system should handle cases where it does not have permission to read a template file (if applicable).
-   **Behavior:** Log an error and return a fallback capsule.
