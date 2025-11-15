//===============================================================
// File: FormSymbolRequest.cs
// Author: Gemini (Original), Claude (Phase 6.4 - Guided Workflow)
// Date: 2025-11-14
// Purpose: Symbol generation request form with student-friendly guided workflow.
//
// PHASE 6.4: GUIDED WORKFLOW
//   - Added inline aspect ratio validation and guidance
//   - Set recommended defaults based on symbol type (180x450 for clefs)
//   - Added real-time aspect ratio calculation and feedback
//   - Provides student-friendly error messages and recommendations
//
// STUDENT LEARNING OBJECTIVES:
//   - Understand aspect ratio requirements for different symbol types
//   - Learn recommended dimensions for template generation
//   - Recognize invalid configurations before generation
//   - Build mental model of dimension constraints
//
// VALIDATION STRATEGY:
//   - Real-time aspect ratio calculation on dimension change
//   - Warn if aspect ratio outside valid range (1.5-3.5 for clefs)
//   - Recommend adjustments with specific guidance
//   - Prevent generation with clearly invalid dimensions
//
// AUDIENCE: Undergraduate / Graduate (UI design, validation workflows)
//===============================================================
using SymbolLabsForge.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace SymbolLabsForge.Tool
{
    public partial class FormSymbolRequest : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FormSymbolRequest> _logger;

        public FormSymbolRequest(IServiceProvider serviceProvider, ILogger<FormSymbolRequest> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            InitializeComponent();
            this.Text = "SymbolLabsForge - Symbol Generator";

            // PHASE 6.4: Set recommended defaults for clefs
            SetRecommendedDefaults();

            // PHASE 6.4: Wire up value change handlers for inline validation
            widthNumericUpDown.ValueChanged += DimensionValueChanged;
            heightNumericUpDown.ValueChanged += DimensionValueChanged;
            symbolTypeComboBox.SelectedIndexChanged += SymbolTypeChanged;
        }

        /// <summary>
        /// Sets recommended default dimensions based on best practices for clef generation.
        /// PHASE 6.4: Student-friendly defaults
        /// </summary>
        private void SetRecommendedDefaults()
        {
            // Recommended dimensions for clefs: 180x450 (aspect ratio ~2.5)
            widthNumericUpDown.Value = 180;
            widthNumericUpDown.Minimum = 50;
            widthNumericUpDown.Maximum = 500;

            heightNumericUpDown.Value = 450;
            heightNumericUpDown.Minimum = 50;
            heightNumericUpDown.Maximum = 1000;

            // Update labels with guidance
            label1.Text = "STEP 1: Select Symbol Type";
            label2.Text = "STEP 2: Width (px):";
            label3.Text = "Height (px):";
            generateButton.Text = "STEP 3: Generate Symbol";
        }

        /// <summary>
        /// Handles symbol type selection change to provide type-specific guidance.
        /// PHASE 6.4: Context-sensitive recommendations
        /// </summary>
        private void SymbolTypeChanged(object? sender, EventArgs e)
        {
            if (symbolTypeComboBox.SelectedItem == null) return;

            var symbolType = (SymbolType)symbolTypeComboBox.SelectedItem;

            // Provide symbol-specific dimension recommendations
            switch (symbolType)
            {
                case SymbolType.Clef:
                    widthNumericUpDown.Value = 180;
                    heightNumericUpDown.Value = 450;
                    MessageBox.Show(
                        "Recommended dimensions for clefs:\n" +
                        "Width: 180px, Height: 450px (ratio ~2.5)\n\n" +
                        "Clefs are typically tall, narrow symbols.\n" +
                        "Valid aspect ratio range: 2.0-3.0",
                        "Clef Recommendations",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;

                case SymbolType.Sharp:
                case SymbolType.Flat:
                case SymbolType.Natural:
                    widthNumericUpDown.Value = 150;
                    heightNumericUpDown.Value = 300;
                    MessageBox.Show(
                        "Recommended dimensions for accidentals:\n" +
                        "Width: 150px, Height: 300px (ratio ~2.0)\n\n" +
                        "Accidentals are moderately tall symbols.\n" +
                        "Valid aspect ratio range: 1.5-2.5",
                        "Accidental Recommendations",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;

                default:
                    widthNumericUpDown.Value = 200;
                    heightNumericUpDown.Value = 200;
                    break;
            }

            ValidateDimensions();
        }

        /// <summary>
        /// Handles dimension value changes with real-time aspect ratio validation.
        /// PHASE 6.4: Inline validation and feedback
        /// </summary>
        private void DimensionValueChanged(object? sender, EventArgs e)
        {
            ValidateDimensions();
        }

        /// <summary>
        /// Validates dimensions and provides real-time feedback on aspect ratio.
        /// PHASE 6.4: Student-friendly validation messaging
        /// </summary>
        private void ValidateDimensions()
        {
            int width = (int)widthNumericUpDown.Value;
            int height = (int)heightNumericUpDown.Value;

            if (width == 0 || height == 0) return;

            double aspectRatio = (double)height / width;

            // Update form title with real-time aspect ratio
            this.Text = $"SymbolLabsForge - Generator (Ratio: {aspectRatio:F2})";

            // Validate aspect ratio based on symbol type
            if (symbolTypeComboBox.SelectedItem != null)
            {
                var symbolType = (SymbolType)symbolTypeComboBox.SelectedItem;

                bool isValid = symbolType switch
                {
                    SymbolType.Clef => aspectRatio >= 2.0 && aspectRatio <= 3.0,
                    SymbolType.Sharp or SymbolType.Flat or SymbolType.Natural => aspectRatio >= 1.5 && aspectRatio <= 2.5,
                    _ => aspectRatio >= 0.5 && aspectRatio <= 4.0
                };

                if (!isValid)
                {
                    generateButton.Enabled = false;
                    generateButton.Text = "⚠️ Fix Aspect Ratio First";
                }
                else
                {
                    generateButton.Enabled = true;
                    generateButton.Text = "STEP 3: Generate Symbol ✓";
                }
            }
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            // PHASE 6.4: Enhanced validation with student-friendly messaging
            if (symbolTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please select a symbol type from STEP 1.\n\n" +
                    "Symbol types available:\n" +
                    "- Clef (treble, bass, alto, tenor)\n" +
                    "- Sharp, Flat, Natural (accidentals)\n" +
                    "- Other musical symbols",
                    "Input Required - Step 1",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Validate dimensions one more time before generation
            int width = (int)widthNumericUpDown.Value;
            int height = (int)heightNumericUpDown.Value;
            double aspectRatio = (double)height / width;
            var symbolType = (SymbolType)symbolTypeComboBox.SelectedItem;

            bool isValid = symbolType switch
            {
                SymbolType.Clef => aspectRatio >= 2.0 && aspectRatio <= 3.0,
                SymbolType.Sharp or SymbolType.Flat or SymbolType.Natural => aspectRatio >= 1.5 && aspectRatio <= 2.5,
                _ => aspectRatio >= 0.5 && aspectRatio <= 4.0
            };

            if (!isValid)
            {
                MessageBox.Show(
                    $"Invalid aspect ratio: {aspectRatio:F2}\n\n" +
                    $"For {symbolType} symbols, valid range is:\n" +
                    (symbolType == SymbolType.Clef ? "2.0 - 3.0 (tall, narrow)" :
                     symbolType == SymbolType.Sharp || symbolType == SymbolType.Flat || symbolType == SymbolType.Natural ? "1.5 - 2.5 (moderately tall)" :
                     "0.5 - 4.0") + "\n\n" +
                    "Adjust width or height in STEP 2 to fix this.",
                    "Validation Failed - Step 2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _logger.LogInformation("Creating generation request for {SymbolType} at {Width}x{Height} (ratio: {AspectRatio:F2})",
                    symbolType, width, height, aspectRatio);

                // 1. Gather selections from UI controls
                var request = new SymbolRequest(
                    symbolType,
                    new List<SixLabors.ImageSharp.Size> { new SixLabors.ImageSharp.Size(width, height) },
                    new List<OutputForm> { OutputForm.Skeletonized } // Skeletonized recommended for templates
                );

                // 2. Generate the symbol (provenance metadata automatically populated in SymbolForge)
                var symbolForge = _serviceProvider.GetRequiredService<ISymbolForge>();
                using (var capsuleSet = symbolForge.Generate(request))
                {
                    _logger.LogInformation("Generation complete. Launching results viewer.");

                    // 3. Launch the results viewer form
                    using (var resultsForm = _serviceProvider.GetRequiredService<FormResultsViewer>())
                    {
                        // The resultsForm takes ownership of the primary capsule's display,
                        // but the using block on capsuleSet ensures it's disposed of eventually.
                        resultsForm.LoadCapsule(capsuleSet.Primary);
                        resultsForm.ShowDialog();
                    }
                }

                _logger.LogInformation("Symbol generation workflow complete.");
            }
            catch (InvalidOperationException ex)
            {
                // PHASE 6.4: Catch provenance validation errors with educational messaging
                _logger.LogError(ex, "Validation error during symbol generation.");
                MessageBox.Show(
                    "Symbol generation failed validation:\n\n" +
                    $"{ex.Message}\n\n" +
                    "This error indicates incomplete metadata or provenance tracking.\n" +
                    "Please ensure all required fields are populated.",
                    "Generation Failed - Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during symbol generation.");
                MessageBox.Show(
                    "An unexpected error occurred:\n\n" +
                    $"{ex.Message}\n\n" +
                    "Check the log for detailed error information.",
                    "Generation Failed - Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
