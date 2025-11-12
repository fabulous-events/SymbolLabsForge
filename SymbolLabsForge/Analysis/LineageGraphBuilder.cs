using SymbolLabsForge.Contracts;
using System.Collections.Generic;
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
                CapsuleId: capsule.Metadata.CapsuleId,
                Type: capsule.Metadata.SymbolType,
                Style: capsule.Metadata.TemplateName,
                InterpolationFactor: capsule.Metadata.InterpolationFactor,
                Contributor: capsule.Metadata.GeneratedBy,
                GeneratedOn: DateTime.Parse(capsule.Metadata.GeneratedOn)
            );
            Nodes.Add(node);
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
                sb.AppendLine($"  \\\"{node.CapsuleId}\\\" [label=\\\"{node.Type}\\\\n({node.Style})\\\\nFactor: {node.InterpolationFactor ?? 0.0f}\\\"];");
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
