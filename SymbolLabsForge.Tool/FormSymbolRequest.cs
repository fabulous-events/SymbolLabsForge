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
            this.Text = "SymbolLabsForge - Request";
            // Populate ComboBoxes, etc. in the designer or a separate method.
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            if (symbolTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a symbol type.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 1. Gather selections from UI controls
                var request = new SymbolRequest(
                    (SymbolType)symbolTypeComboBox.SelectedItem,
                    new List<SixLabors.ImageSharp.Size> { new SixLabors.ImageSharp.Size((int)widthNumericUpDown.Value, (int)heightNumericUpDown.Value) },
                    new List<OutputForm> { OutputForm.Skeletonized } // Placeholder
                );

                _logger.LogInformation("Creating generation request.");

                // 2. Generate the symbol
                var symbolForge = _serviceProvider.GetRequiredService<ISymbolForge>();
                var capsuleSet = symbolForge.Generate(request);

                _logger.LogInformation("Generation complete. Launching results viewer.");

                // 3. Launch the results viewer form
                using (var resultsForm = _serviceProvider.GetRequiredService<FormResultsViewer>())
                {
                    resultsForm.LoadCapsule(capsuleSet.Primary);
                    resultsForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during symbol generation.");
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
