namespace FYP
{
    partial class TestHarness
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
            this.fpsLabel = new System.Windows.Forms.Label();
            this.fpsTimer = new System.Windows.Forms.Timer(this.components);
            this.expressionLabel = new System.Windows.Forms.Label();
            this.testInstructions = new System.Windows.Forms.Label();
            this.testButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.videoFeed)).BeginInit();
            this.SuspendLayout();
            // 
            // videoFeed
            // 
            this.videoFeed.Location = new System.Drawing.Point(8, 51);
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
            // fpsLabel
            // 
            this.fpsLabel.AutoSize = true;
            this.fpsLabel.Location = new System.Drawing.Point(7, 534);
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
            // expressionLabel
            // 
            this.expressionLabel.AutoSize = true;
            this.expressionLabel.Location = new System.Drawing.Point(26, 534);
            this.expressionLabel.Name = "expressionLabel";
            this.expressionLabel.Size = new System.Drawing.Size(31, 13);
            this.expressionLabel.TabIndex = 3;
            this.expressionLabel.Text = "none";
            // 
            // testInstructions
            // 
            this.testInstructions.AutoSize = true;
            this.testInstructions.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.testInstructions.ForeColor = System.Drawing.Color.Red;
            this.testInstructions.Location = new System.Drawing.Point(13, 13);
            this.testInstructions.Name = "testInstructions";
            this.testInstructions.Size = new System.Drawing.Size(162, 24);
            this.testInstructions.TabIndex = 4;
            this.testInstructions.Text = "Test Instructions";
            // 
            // testButton
            // 
            this.testButton.Location = new System.Drawing.Point(572, 13);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 23);
            this.testButton.TabIndex = 5;
            this.testButton.Text = "Start Test";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // TestHarness
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(660, 556);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.testInstructions);
            this.Controls.Add(this.expressionLabel);
            this.Controls.Add(this.fpsLabel);
            this.Controls.Add(this.videoFeed);
            this.Name = "TestHarness";
            this.Text = "Test Harness";
            ((System.ComponentModel.ISupportInitialize)(this.videoFeed)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox videoFeed;
        private System.Windows.Forms.Timer progTimer;
        private System.Windows.Forms.Label fpsLabel;
        private System.Windows.Forms.Timer fpsTimer;
        private System.Windows.Forms.Label expressionLabel;
        private System.Windows.Forms.Label testInstructions;
        private System.Windows.Forms.Button testButton;
    }
}

