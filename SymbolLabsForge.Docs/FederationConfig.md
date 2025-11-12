# SymbolLabsForge Federation Configuration

*   **Status**: ✅ Active
*   **AuditTag**: `Phase11.1`

This document defines the nodes, trust levels, and synchronization policies for the SymbolLabsForge federated network.

```yaml
# List of trusted nodes in the federation.
nodes:
  - id: forge-west
    url: https://forge-west.symbollabs.org/api
    trustLevel: high # Can propose and vote on governance changes.
    syncsWith:
      - forge-east

  - id: forge-east
    url: https://forge-east.symbollabs.org/api
    trustLevel: high # Can propose and vote on governance changes.
    syncsWith:
      - forge-west

  - id: community-node-alpha
    url: https://community.symbollabs.org/forge/alpha
    trustLevel: moderate # Can submit capsules and receive governance updates, but cannot vote.
    syncsWith:
      - forge-west

# Default synchronization policies.
syncPolicy:
  # How often nodes should attempt to synchronize with each other.
  syncInterval: "6h"

  # What data to synchronize.
  capsuleSync: true
  lineageSync: true
  governanceSync: true
```
