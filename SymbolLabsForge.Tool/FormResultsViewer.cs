//===============================================================
// File: FormResultsViewer.cs
// Author: Gemini (Original), Claude (Phase 6.1 - Student-Friendly UI)
// Date: 2025-11-14
// Purpose: Results viewer form with narratable quality metrics and provenance display.
//
// PHASE 6.1: STUDENT-FRIENDLY UI REFACTOR
//   - Replaced obsolete Density property with DensityPercent/DensityFraction
//   - Added "What it means" explanations for density and aspect ratio
//   - Improved formatting with section headers and indentation
//   - Added visual indicators (✓/✗) for validation results
//
// PHASE 6.2: PROVENANCE METADATA PANEL
//   - Added full provenance tracking display (source image, preprocessing, validation)
//   - Shows template name, generator version, capsule ID, SHA256 hash
//   - Explains preprocessing method (Raw/Binarized/Skeletonized/Custom)
//   - Displays validation date and validator version
//   - Handles legacy capsules without provenance gracefully
//
// PHASE 6.3: NARRATABLE VALIDATOR RESULTS
//   - Added AppendValidatorGuidance() method for educational explanations
//   - Each validator shows "What it checks", "Why it matters", "How to fix"
//   - Provides actionable guidance for DensityValidator, ContrastValidator, StructureValidator
//   - Students learn validation criteria instead of just seeing PASS/FAIL
//   - Converts cryptic failures into teachable moments
//
// PHASE 6.5: HASH VERIFICATION PANEL (Graduate/PhD)
//   - Added AppendHashVerification() method for integrity verification
//   - Computes SHA256 hash from image bytes and compares with metadata hash
//   - Shows MATCH/MISMATCH status with clear explanations
//   - Provides troubleshooting guidance for hash mismatches
//   - Educates students about cryptographic hash functions and integrity verification
//
// DEFECT HISTORY:
//   - Original Implementation: Used obsolete Density property (Phase 1B deprecation)
//   - Root Cause: UI not updated after QualityMetrics refactor
//   - Impact: Students saw cryptic percentage without understanding what it means
//   - Fix: Use DensityPercent/DensityFraction with educational context
//
// VALIDATION STRATEGY:
//   - Display both percentage (0-100) and fraction (0.0-1.0) for clarity
//   - Explain valid range (3-8% for skeletonized symbols)
//   - Provide "What it means" context for each metric
//   - Use visual indicators (✓/✗) instead of cryptic PASS/FAIL
//
// STUDENT LEARNING OBJECTIVES:
//   - Understand what pixel density means (% of ink pixels)
//   - Recognize valid density range for skeletonized symbols
//   - Interpret aspect ratio (Height/Width) for symbol shape
//   - Connect validation results to actionable fixes
//   - Trace template provenance from source image to export
//   - Understand preprocessing methods (Raw/Binarized/Skeletonized)
//   - Recognize importance of SHA256 hash for integrity verification
//   - Learn about metadata completeness for reproducibility
//
// AUDIENCE: Undergraduate / Graduate (UI design, educational tooling)
//===============================================================
using SixLabors.ImageSharp;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Services;
using System.Diagnostics;
using System.Text;

namespace SymbolLabsForge.Tool
{
    public partial class FormResultsViewer : Form
    {
        private readonly CapsuleExporter _exporter;
        private readonly CapsuleRegistryManager _registryManager;
        private SymbolCapsule? _currentCapsule;
        private const string RegistryFileName = "CapsuleRegistry.json";

        public FormResultsViewer(CapsuleExporter exporter, CapsuleRegistryManager registryManager)
        {
            _exporter = exporter;
            _registryManager = registryManager;
            InitializeComponent();
            this.Text = "SymbolLabsForge - Results";
        }

        public void LoadCapsule(SymbolCapsule capsule)
        {
            // Dispose of the previous capsule before loading the new one
            _currentCapsule?.Dispose();
            _currentCapsule = capsule;

            // Display the image
            SetImage(previewPictureBox, capsule.TemplateImage);

            // Display the results
            var sb = new StringBuilder();
            sb.AppendLine($"Overall Result: {(capsule.IsValid ? "PASS" : "FAIL")}");
            sb.AppendLine();
            sb.AppendLine("=== QUALITY METRICS ===");
            sb.AppendLine($"Dimensions: {capsule.Metrics.Width}x{capsule.Metrics.Height} pixels");
            sb.AppendLine();

            // PHASE 6.1: Student-friendly density display (replaces obsolete Density property)
            sb.AppendLine($"Pixel Density: {capsule.Metrics.DensityPercent:F2}% (fraction: {capsule.Metrics.DensityFraction:F4})");
            sb.AppendLine($"  What it means: {capsule.Metrics.DensityPercent:F2}% of pixels are ink (black)");
            sb.AppendLine($"  Valid range: 3-8% for skeletonized symbols");
            sb.AppendLine($"  Status: {capsule.Metrics.DensityStatus}");
            sb.AppendLine();

            sb.AppendLine($"Aspect Ratio: {capsule.Metrics.AspectRatio:F2}");
            sb.AppendLine($"  What it means: Height/Width ratio (tall symbols ~2.0-3.0)");
            sb.AppendLine();

            // PHASE 6.2: Provenance metadata panel for full traceability
            sb.AppendLine("=== PROVENANCE & METADATA ===");
            sb.AppendLine($"Template Name: {capsule.Metadata.TemplateName}");
            sb.AppendLine($"Generated By: {capsule.Metadata.GeneratedBy}");
            sb.AppendLine($"Generated On: {capsule.Metadata.GeneratedOn}");
            sb.AppendLine($"Capsule ID: {capsule.Metadata.CapsuleId}");
            sb.AppendLine();

            sb.AppendLine($"Template Hash: {capsule.Metadata.TemplateHash}");
            sb.AppendLine($"  (SHA256 for integrity verification)");
            sb.AppendLine();

            sb.AppendLine($"Symbol Type: {capsule.Metadata.SymbolType}");
            sb.AppendLine();

            // Display provenance tracking info
            if (capsule.Metadata.Provenance != null)
            {
                sb.AppendLine("--- Provenance Tracking ---");
                sb.AppendLine($"Source Image: {capsule.Metadata.Provenance.SourceImage}");
                sb.AppendLine($"Preprocessing Method: {capsule.Metadata.Provenance.Method}");

                // Add explanation of preprocessing method
                string methodExplanation = capsule.Metadata.Provenance.Method switch
                {
                    PreprocessingMethod.Raw => "No preprocessing applied",
                    PreprocessingMethod.Binarized => "Adaptive binarization (threshold-based)",
                    PreprocessingMethod.Skeletonized => "Zhang-Suen thinning algorithm",
                    PreprocessingMethod.Custom => "Custom preprocessing (see notes)",
                    _ => "Unknown method"
                };
                sb.AppendLine($"  What it means: {methodExplanation}");
                sb.AppendLine();

                sb.AppendLine($"Validated By: {capsule.Metadata.Provenance.ValidatedBy}");
                sb.AppendLine($"Validation Date: {capsule.Metadata.Provenance.ValidationDate:yyyy-MM-dd HH:mm} UTC");

                if (!string.IsNullOrWhiteSpace(capsule.Metadata.Provenance.Notes))
                {
                    sb.AppendLine($"Notes: {capsule.Metadata.Provenance.Notes}");
                }
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("--- Provenance Tracking ---");
                sb.AppendLine("⚠️ No provenance metadata available");
                sb.AppendLine("  This capsule may be from an older version.");
                sb.AppendLine();
            }

            // PHASE 6.3: Narratable validator results with educational explanations
            sb.AppendLine("=== VALIDATION RESULTS ===");
            foreach (var result in capsule.ValidationResults)
            {
                string status = result.IsValid ? "PASS ✓" : "FAIL ✗";
                sb.AppendLine($"[{status}] {result.ValidatorName}");

                // Add validator-specific educational guidance
                AppendValidatorGuidance(sb, result.ValidatorName, result.IsValid, result.FailureMessage);
                sb.AppendLine();
            }

            // PHASE 6.5: Hash verification panel for Graduate/PhD students
            sb.AppendLine("=== INTEGRITY VERIFICATION (Advanced) ===");
            AppendHashVerification(sb, capsule);

            resultsTextBox.Text = sb.ToString();
            saveCapsuleButton.Enabled = true;
        }

        /// <summary>
        /// Appends SHA256 hash verification information for advanced students.
        /// Demonstrates cryptographic integrity verification and hash computation.
        ///
        /// PHASE 6.5: Graduate/PhD level integrity verification
        /// </summary>
        private void AppendHashVerification(StringBuilder sb, SymbolCapsule capsule)
        {
            sb.AppendLine("Template Hash (from metadata):");
            sb.AppendLine($"  {capsule.Metadata.TemplateHash}");
            sb.AppendLine();

            // Compute hash from current image for verification
            using (var memoryStream = new MemoryStream())
            {
                capsule.TemplateImage.SaveAsPng(memoryStream);
                memoryStream.Position = 0;

                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(memoryStream);
                    string computedHash = Convert.ToHexString(hashBytes).ToLowerInvariant();

                    sb.AppendLine("Computed Hash (from image bytes):");
                    sb.AppendLine($"  {computedHash}");
                    sb.AppendLine();

                    bool hashesMatch = capsule.Metadata.TemplateHash.Equals(computedHash, StringComparison.OrdinalIgnoreCase);

                    if (hashesMatch)
                    {
                        sb.AppendLine("Status: ✓ MATCH - Template integrity verified");
                        sb.AppendLine();
                        sb.AppendLine("What this means:");
                        sb.AppendLine("  The template hash matches the computed hash from image bytes.");
                        sb.AppendLine("  This confirms no corruption or tampering has occurred.");
                        sb.AppendLine("  The template is identical to when it was generated.");
                    }
                    else
                    {
                        sb.AppendLine("Status: ✗ MISMATCH - Template integrity check failed");
                        sb.AppendLine();
                        sb.AppendLine("What this means:");
                        sb.AppendLine("  The template hash does NOT match the computed hash.");
                        sb.AppendLine("  This indicates possible corruption, tampering, or metadata error.");
                        sb.AppendLine("  DO NOT use this template for production.");
                        sb.AppendLine();
                        sb.AppendLine("Troubleshooting:");
                        sb.AppendLine("  1. Re-generate the template from source");
                        sb.AppendLine("  2. Verify source image hasn't been modified");
                        sb.AppendLine("  3. Check for file system corruption");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("About SHA256:");
            sb.AppendLine("  SHA256 is a cryptographic hash function that produces a unique");
            sb.AppendLine("  64-character hexadecimal \"fingerprint\" for any data.");
            sb.AppendLine("  Even a single pixel change produces a completely different hash.");
            sb.AppendLine("  This enables integrity verification and deduplication.");
        }

        /// <summary>
        /// Appends educational guidance for each validator to help students understand
        /// what each validator checks, why it matters, and how to fix failures.
        ///
        /// PHASE 6.3: Student-friendly validator explanations
        /// </summary>
        private void AppendValidatorGuidance(StringBuilder sb, string validatorName, bool isValid, string? failureMessage)
        {
            switch (validatorName)
            {
                case "DensityValidator":
                    sb.AppendLine("  What it checks: Pixel density (% of ink pixels)");
                    sb.AppendLine("  Why it matters: Too few pixels = thin/incomplete symbol");
                    sb.AppendLine("                  Too many pixels = thick/contaminated symbol");
                    if (!isValid && !string.IsNullOrEmpty(failureMessage))
                    {
                        sb.AppendLine($"  Why it failed: {failureMessage}");
                        sb.AppendLine("  How to fix: Adjust skeletonization settings or check for contamination");
                    }
                    else if (isValid)
                    {
                        sb.AppendLine("  Result: Density is within valid range (3-8%)");
                    }
                    break;

                case "ContrastValidator":
                    sb.AppendLine("  What it checks: Sufficient contrast between ink and background");
                    sb.AppendLine("  Why it matters: Low contrast = hard to detect symbol boundaries");
                    if (!isValid && !string.IsNullOrEmpty(failureMessage))
                    {
                        sb.AppendLine($"  Why it failed: {failureMessage}");
                        sb.AppendLine("  How to fix: Increase contrast in source image or adjust binarization threshold");
                    }
                    else if (isValid)
                    {
                        sb.AppendLine("  Result: At least 10% dark pixels detected");
                    }
                    break;

                case "StructureValidator":
                    sb.AppendLine("  What it checks: Symbol has connected components (not fragmented)");
                    sb.AppendLine("  Why it matters: Fragmented symbols indicate processing errors");
                    if (!isValid && !string.IsNullOrEmpty(failureMessage))
                    {
                        sb.AppendLine($"  Why it failed: {failureMessage}");
                        sb.AppendLine("  How to fix: Check for over-skeletonization or noise in source image");
                    }
                    else if (isValid)
                    {
                        sb.AppendLine("  Result: Symbol has valid connected components");
                    }
                    break;

                case "TemplateValidator":
                    sb.AppendLine("  What it checks: Template metadata completeness and validity");
                    sb.AppendLine("  Why it matters: Incomplete metadata breaks traceability");
                    if (!isValid && !string.IsNullOrEmpty(failureMessage))
                    {
                        sb.AppendLine($"  Why it failed: {failureMessage}");
                        sb.AppendLine("  How to fix: Provide complete provenance metadata before export");
                    }
                    else if (isValid)
                    {
                        sb.AppendLine("  Result: All required metadata fields are complete");
                    }
                    break;

                default:
                    // Generic fallback for unknown validators
                    if (!isValid && !string.IsNullOrEmpty(failureMessage))
                    {
                        sb.AppendLine($"  Why it failed: {failureMessage}");
                    }
                    else if (isValid)
                    {
                        sb.AppendLine("  Result: Validation passed");
                    }
                    break;
            }
        }

        private async void saveCapsuleButton_Click(object sender, EventArgs e)
        {
            if (_currentCapsule == null) return;

            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select a folder to save the capsule files";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        resultsTextBox.AppendText("\r\n--- EXPORTING ---\r\n");
                        await _exporter.ExportAsync(_currentCapsule, dialog.SelectedPath, "skeleton"); // Assuming skeleton for now
                        resultsTextBox.AppendText($"Capsule saved to: {dialog.SelectedPath}\r\n");

                        await _registryManager.AddEntryAsync(_currentCapsule);
                        resultsTextBox.AppendText("Registry updated successfully.\r\n");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save capsule: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void viewRegistryButton_Click(object sender, EventArgs e)
        {
            var registryPath = Path.Combine(Application.StartupPath, RegistryFileName);
            if (File.Exists(registryPath))
            {
                try
                {
                    // Use Process.Start to open the file with the default editor
                    new Process
                    {
                        StartInfo = new ProcessStartInfo(registryPath)
                        {
                            UseShellExecute = true
                        }
                    }.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open registry file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Registry file not found. Save a capsule first to create it.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Helper to safely display an ImageSharp image in a PictureBox
        private void SetImage(PictureBox pictureBox, SixLabors.ImageSharp.Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.SaveAsBmp(ms);
                pictureBox.Image = new Bitmap(ms);
            }
        }
    }
}
