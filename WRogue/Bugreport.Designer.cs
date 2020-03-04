namespace djack.RogueSurvivor
{
    partial class Bugreport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.m_OkButton = new System.Windows.Forms.Button();
            this.m_HeaderTextBox = new System.Windows.Forms.TextBox();
            this.m_LogTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // m_OkButton
            // 
            this.m_OkButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.m_OkButton.Location = new System.Drawing.Point(0, 343);
            this.m_OkButton.Name = "m_OkButton";
            this.m_OkButton.Size = new System.Drawing.Size(592, 23);
            this.m_OkButton.TabIndex = 0;
            this.m_OkButton.Text = "OK";
            this.m_OkButton.UseVisualStyleBackColor = true;
            this.m_OkButton.Click += new System.EventHandler(this.m_OkButton_Click);
            // 
            // m_HeaderTextBox
            // 
            this.m_HeaderTextBox.AcceptsReturn = true;
            this.m_HeaderTextBox.AcceptsTab = true;
            this.m_HeaderTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_HeaderTextBox.Location = new System.Drawing.Point(0, 0);
            this.m_HeaderTextBox.Multiline = true;
            this.m_HeaderTextBox.Name = "m_HeaderTextBox";
            this.m_HeaderTextBox.ReadOnly = true;
            this.m_HeaderTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.m_HeaderTextBox.Size = new System.Drawing.Size(592, 97);
            this.m_HeaderTextBox.TabIndex = 1;
            // 
            // m_LogTextBox
            // 
            this.m_LogTextBox.AcceptsReturn = true;
            this.m_LogTextBox.AcceptsTab = true;
            this.m_LogTextBox.Location = new System.Drawing.Point(12, 103);
            this.m_LogTextBox.Multiline = true;
            this.m_LogTextBox.Name = "m_LogTextBox";
            this.m_LogTextBox.ReadOnly = true;
            this.m_LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.m_LogTextBox.Size = new System.Drawing.Size(568, 234);
            this.m_LogTextBox.TabIndex = 2;
            // 
            // Bugreport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 366);
            this.Controls.Add(this.m_LogTextBox);
            this.Controls.Add(this.m_HeaderTextBox);
            this.Controls.Add(this.m_OkButton);
            this.Name = "Bugreport";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Rogue Survivor Error Report";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Bugreport_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_OkButton;
        private System.Windows.Forms.TextBox m_HeaderTextBox;
        private System.Windows.Forms.TextBox m_LogTextBox;
    }
}