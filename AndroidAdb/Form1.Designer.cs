namespace AndroidAdb
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
            this.button1 = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.LoggingTextbox = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ShowHideButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // BackButton
            // 
            this.BackButton.Location = new System.Drawing.Point(13, 46);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(36, 40);
            this.BackButton.TabIndex = 1;
            this.BackButton.Text = "<";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // LoggingTextbox
            // 
            this.LoggingTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LoggingTextbox.Location = new System.Drawing.Point(12, 105);
            this.LoggingTextbox.Multiline = true;
            this.LoggingTextbox.Name = "LoggingTextbox";
            this.LoggingTextbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LoggingTextbox.Size = new System.Drawing.Size(276, 435);
            this.LoggingTextbox.TabIndex = 2;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(301, 9);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(367, 531);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // ShowHideButton
            // 
            this.ShowHideButton.Location = new System.Drawing.Point(245, 9);
            this.ShowHideButton.Name = "ShowHideButton";
            this.ShowHideButton.Size = new System.Drawing.Size(50, 40);
            this.ShowHideButton.TabIndex = 1;
            this.ShowHideButton.Text = "Show";
            this.ShowHideButton.UseVisualStyleBackColor = true;
            this.ShowHideButton.Click += new System.EventHandler(this.ShowHideButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 552);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.LoggingTextbox);
            this.Controls.Add(this.ShowHideButton);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.TextBox LoggingTextbox;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button ShowHideButton;
    }
}

