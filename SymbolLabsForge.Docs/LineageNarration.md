# Lineage Narration

*   **Status**: 📝 Draft
*   **Timestamp**: 2025-11-12
*   **AuditTag**: `PostPhase.Lineage.Review`

This document provides a narration of the capsule ancestry based on the `LineageGraph.dot` and `LineageMap.md` files.

## 1. Current State of the Lineage Graph

The current lineage graph is a simple, three-node graph that demonstrates a morph between a "Handwritten" and a "Typeset" version of a "Flat" symbol. While this is a useful starting point, it does not represent the full complexity of a real-world lineage with multiple generations, branches, and merges.

## 2. Anomalies and Gaps

*   **Incomplete Representation**: The current graph is a sample and does not reflect the full history of the capsules in the system.
*   **Missing SVG**: The `LineageMap.md` file refers to a `LineageGraph.svg` file that is not present in the repository. This visual aid should be generated and included.
*   **Lack of Validator Information**: The lineage graph does not currently incorporate any information about the validation process. It is not possible to see which validators were applied at each stage of the lineage or what the results of those validations were.

## 3. Future Work

*   **Expand the Graph**: The lineage graph should be expanded to include all capsules in the system.
*   **Integrate Validation Data**: The validation results for each capsule should be integrated into the lineage graph. This will allow for a more complete understanding of the capsule's history.
*   **Automate Graph Generation**: The process of generating the lineage graph should be automated to ensure that it is always up-to-date.
