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
            _currentCapsule = capsule;

            // Display the image
            SetImage(previewPictureBox, capsule.TemplateImage);

            // Display the results
            var sb = new StringBuilder();
            sb.AppendLine($"Overall Result: {(capsule.IsValid ? "PASS" : "FAIL")}");
            sb.AppendLine("--- Quality Metrics ---");
            sb.AppendLine($"Dimensions: {capsule.Metrics.Width}x{capsule.Metrics.Height}");
            sb.AppendLine($"Density: {capsule.Metrics.Density:F2}% ({capsule.Metrics.DensityStatus})");
            sb.AppendLine($"Aspect Ratio: {capsule.Metrics.AspectRatio:F2}");
            sb.AppendLine();
            sb.AppendLine("--- Validation ---");
            foreach (var result in capsule.ValidationResults)
            {
                sb.AppendLine($"[{ (result.IsValid ? "PASS" : "FAIL") }] {result.ValidatorName}: {result.FailureMessage ?? "OK"}");
            }

            resultsTextBox.Text = sb.ToString();
            saveCapsuleButton.Enabled = true;
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
