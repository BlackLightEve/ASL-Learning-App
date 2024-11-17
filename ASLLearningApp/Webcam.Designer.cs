namespace ASLLearningApp
{
    partial class Webcam
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pbWebcam = new System.Windows.Forms.PictureBox();
            this.btHide = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbWebcam)).BeginInit();
            this.SuspendLayout();
            // 
            // pbWebcam
            // 
            this.pbWebcam.Location = new System.Drawing.Point(20, 20);
            this.pbWebcam.Name = "pbWebcam";
            this.pbWebcam.Size = new System.Drawing.Size(230, 250);
            this.pbWebcam.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbWebcam.TabIndex = 0;
            this.pbWebcam.TabStop = false;
            // 
            // btHide
            // 
            this.btHide.Location = new System.Drawing.Point(98, 260);
            this.btHide.Name = "btHide";
            this.btHide.Size = new System.Drawing.Size(75, 23);
            this.btHide.TabIndex = 1;
            this.btHide.Text = "HIde";
            this.btHide.UseVisualStyleBackColor = true;
            // 
            // Webcam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.btHide);
            this.Controls.Add(this.pbWebcam);
            this.Name = "Webcam";
            this.Size = new System.Drawing.Size(268, 288);
            this.Load += new System.EventHandler(this.Webcam_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbWebcam)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private PictureBox pbWebcam;
        private Button btHide;
    }
}
