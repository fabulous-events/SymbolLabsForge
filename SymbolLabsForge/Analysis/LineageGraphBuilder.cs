using SymbolLabsForge.Contracts;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System;

namespace SymbolLabsForge.Analysis
{
    public class LineageGraphBuilder
    {
        public List<LineageNode> Nodes { get; } = new();
        public List<LineageEdge> Edges { get; } = new();

        public void AddCapsule(SymbolCapsule capsule)
        {
            var node = new LineageNode(
                capsule.Metadata.CapsuleId,
                capsule.Metadata.SymbolType,
                "Generated", // Default style for now
                null, // No interpolation factor for now
                "System", // Default contributor
                DateTime.UtcNow
            );
            Nodes.Add(node);

            // For now, we don't have parent tracking, so all nodes are roots.
            // This can be expanded later.
        }

        public void Link(string fromId, string toId, string transitionType, string auditTag)
        {
            Edges.Add(new LineageEdge(fromId, toId, transitionType, auditTag));
        }

        public string ExportAsDot()
        {
            var sb = new StringBuilder("digraph CapsuleLineage {\\n");
            sb.AppendLine("  rankdir=LR;");
            sb.AppendLine("  node [shape=box, style=rounded];");
            foreach (var node in Nodes)
            {
                var factorLabel = node.InterpolationFactor.HasValue ? $"Factor: {node.InterpolationFactor.Value}" : "Factor: N/A";
                sb.AppendLine($"  \\\"{node.CapsuleId}\\\" [label=\\\"{node.Type}\\\\n({node.Style})\\\\n{factorLabel}\\\"];");
            }

            foreach (var edge in Edges)
            {
                sb.AppendLine($"  \\\"{edge.FromCapsuleId}\\\" -> \\\"{edge.ToCapsuleId}\\\" [label=\\\"{edge.TransitionType}\\\\n{edge.AuditTag}\\\"];");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
