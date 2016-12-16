namespace FYP
{
    partial class TestGround
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
            this.components = new System.ComponentModel.Container();
            this.videoFeed = new System.Windows.Forms.PictureBox();
            this.progTimer = new System.Windows.Forms.Timer(this.components);
            this.debugLabel = new System.Windows.Forms.Label();
            this.fpsLabel = new System.Windows.Forms.Label();
            this.fpsTimer = new System.Windows.Forms.Timer(this.components);
            this.region1 = new System.Windows.Forms.PictureBox();
            this.region2 = new System.Windows.Forms.PictureBox();
            this.region3 = new System.Windows.Forms.PictureBox();
            this.region4 = new System.Windows.Forms.PictureBox();
            this.expressionLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.videoFeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.region1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.region2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.region3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.region4)).BeginInit();
            this.SuspendLayout();
            // 
            // videoFeed
            // 
            this.videoFeed.Location = new System.Drawing.Point(10, 10);
            this.videoFeed.Name = "videoFeed";
            this.videoFeed.Size = new System.Drawing.Size(640, 480);
            this.videoFeed.TabIndex = 0;
            this.videoFeed.TabStop = false;
            // 
            // progTimer
            // 
            this.progTimer.Enabled = true;
            this.progTimer.Interval = 20;
            this.progTimer.Tick += new System.EventHandler(this.progTimer_Tick);
            // 
            // debugLabel
            // 
            this.debugLabel.AutoSize = true;
            this.debugLabel.Location = new System.Drawing.Point(72, 495);
            this.debugLabel.Name = "debugLabel";
            this.debugLabel.Size = new System.Drawing.Size(39, 13);
            this.debugLabel.TabIndex = 1;
            this.debugLabel.Text = "Debug";
            // 
            // fpsLabel
            // 
            this.fpsLabel.AutoSize = true;
            this.fpsLabel.Location = new System.Drawing.Point(12, 495);
            this.fpsLabel.Name = "fpsLabel";
            this.fpsLabel.Size = new System.Drawing.Size(13, 13);
            this.fpsLabel.TabIndex = 2;
            this.fpsLabel.Text = "0";
            // 
            // fpsTimer
            // 
            this.fpsTimer.Enabled = true;
            this.fpsTimer.Interval = 1000;
            this.fpsTimer.Tick += new System.EventHandler(this.fpsTimer_Tick);
            // 
            // region1
            // 
            this.region1.Location = new System.Drawing.Point(657, 10);
            this.region1.Name = "region1";
            this.region1.Size = new System.Drawing.Size(182, 113);
            this.region1.TabIndex = 3;
            this.region1.TabStop = false;
            // 
            // region2
            // 
            this.region2.Location = new System.Drawing.Point(656, 129);
            this.region2.Name = "region2";
            this.region2.Size = new System.Drawing.Size(182, 113);
            this.region2.TabIndex = 4;
            this.region2.TabStop = false;
            // 
            // region3
            // 
            this.region3.Location = new System.Drawing.Point(657, 248);
            this.region3.Name = "region3";
            this.region3.Size = new System.Drawing.Size(182, 113);
            this.region3.TabIndex = 9;
            this.region3.TabStop = false;
            // 
            // region4
            // 
            this.region4.Location = new System.Drawing.Point(657, 367);
            this.region4.Name = "region4";
            this.region4.Size = new System.Drawing.Size(182, 113);
            this.region4.TabIndex = 10;
            this.region4.TabStop = false;
            // 
            // expressionLabel
            // 
            this.expressionLabel.AutoSize = true;
            this.expressionLabel.Location = new System.Drawing.Point(31, 495);
            this.expressionLabel.Name = "expressionLabel";
            this.expressionLabel.Size = new System.Drawing.Size(33, 13);
            this.expressionLabel.TabIndex = 13;
            this.expressionLabel.Text = "None";
            // 
            // TestGround
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(863, 528);
            this.Controls.Add(this.expressionLabel);
            this.Controls.Add(this.region4);
            this.Controls.Add(this.region3);
            this.Controls.Add(this.region2);
            this.Controls.Add(this.region1);
            this.Controls.Add(this.fpsLabel);
            this.Controls.Add(this.debugLabel);
            this.Controls.Add(this.videoFeed);
            this.Name = "TestGround";
            this.Text = "Testing Ground";
            ((System.ComponentModel.ISupportInitialize)(this.videoFeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.region1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.region2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.region3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.region4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox videoFeed;
        private System.Windows.Forms.Timer progTimer;
        private System.Windows.Forms.Label debugLabel;
        private System.Windows.Forms.Label fpsLabel;
        private System.Windows.Forms.Timer fpsTimer;
        private System.Windows.Forms.PictureBox region1;
        private System.Windows.Forms.PictureBox region2;
        private System.Windows.Forms.PictureBox region3;
        private System.Windows.Forms.PictureBox region4;
        private System.Windows.Forms.Label expressionLabel;
    }
}

