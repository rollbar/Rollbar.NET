namespace Sample.WindowsFormsApp
{
    partial class Form1
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
            this._btnLogHandledException = new System.Windows.Forms.Button();
            this._btnLogUnhandledException = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _btnLogHandledException
            // 
            this._btnLogHandledException.Location = new System.Drawing.Point(12, 35);
            this._btnLogHandledException.Name = "_btnLogHandledException";
            this._btnLogHandledException.Size = new System.Drawing.Size(248, 23);
            this._btnLogHandledException.TabIndex = 0;
            this._btnLogHandledException.Text = "Log Handled Exception";
            this._btnLogHandledException.UseVisualStyleBackColor = true;
            this._btnLogHandledException.Click += new System.EventHandler(this._btnLogHandledException_Click);
            // 
            // _btnLogUnhandledException
            // 
            this._btnLogUnhandledException.Location = new System.Drawing.Point(12, 64);
            this._btnLogUnhandledException.Name = "_btnLogUnhandledException";
            this._btnLogUnhandledException.Size = new System.Drawing.Size(248, 23);
            this._btnLogUnhandledException.TabIndex = 1;
            this._btnLogUnhandledException.Text = "Log Unhandled Exception";
            this._btnLogUnhandledException.UseVisualStyleBackColor = true;
            this._btnLogUnhandledException.Click += new System.EventHandler(this._btnLogUnhandledException_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this._btnLogUnhandledException);
            this.Controls.Add(this._btnLogHandledException);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _btnLogHandledException;
        private System.Windows.Forms.Button _btnLogUnhandledException;
    }
}

