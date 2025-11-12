namespace SymbolLabsForge.Tool
{
    partial class FormResultsViewer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.previewPictureBox = new System.Windows.Forms.PictureBox();
            this.resultsTextBox = new System.Windows.Forms.TextBox();
            this.saveCapsuleButton = new System.Windows.Forms.Button();
            this.viewRegistryButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // previewPictureBox
            // 
            this.previewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.previewPictureBox.Location = new System.Drawing.Point(12, 12);
            this.previewPictureBox.Name = "previewPictureBox";
            this.previewPictureBox.Size = new System.Drawing.Size(150, 300);
            this.previewPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.previewPictureBox.TabIndex = 0;
            this.previewPictureBox.TabStop = false;
            // 
            // resultsTextBox
            // 
            this.resultsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resultsTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.resultsTextBox.Location = new System.Drawing.Point(178, 12);
            this.resultsTextBox.Multiline = true;
            this.resultsTextBox.Name = "resultsTextBox";
            this.resultsTextBox.ReadOnly = true;
            this.resultsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.resultsTextBox.Size = new System.Drawing.Size(394, 268);
            this.resultsTextBox.TabIndex = 1;
            // 
            // saveCapsuleButton
            // 
            this.saveCapsuleButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveCapsuleButton.Enabled = false;
            this.saveCapsuleButton.Location = new System.Drawing.Point(467, 286);
            this.saveCapsuleButton.Name = "saveCapsuleButton";
            this.saveCapsuleButton.Size = new System.Drawing.Size(105, 23);
            this.saveCapsuleButton.TabIndex = 2;
            this.saveCapsuleButton.Text = "Save Capsule...";
            this.saveCapsuleButton.UseVisualStyleBackColor = true;
            this.saveCapsuleButton.Click += new System.EventHandler(this.saveCapsuleButton_Click);
            // 
            // viewRegistryButton
            // 
            this.viewRegistryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.viewRegistryButton.Location = new System.Drawing.Point(356, 286);
            this.viewRegistryButton.Name = "viewRegistryButton";
            this.viewRegistryButton.Size = new System.Drawing.Size(105, 23);
            this.viewRegistryButton.TabIndex = 3;
            this.viewRegistryButton.Text = "View Registry";
            this.viewRegistryButton.UseVisualStyleBackColor = true;
            this.viewRegistryButton.Click += new System.EventHandler(this.viewRegistryButton_Click);
            // 
            // FormResultsViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 321);
            this.Controls.Add(this.viewRegistryButton);
            this.Controls.Add(this.saveCapsuleButton);
            this.Controls.Add(this.resultsTextBox);
            this.Controls.Add(this.previewPictureBox);
            this.Name = "FormResultsViewer";
            this.Text = "FormResultsViewer";
            ((System.ComponentModel.ISupportInitialize)(this.previewPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox previewPictureBox;
        private System.Windows.Forms.TextBox resultsTextBox;
        private System.Windows.Forms.Button saveCapsuleButton;
        private System.Windows.Forms.Button viewRegistryButton;
    }
}
