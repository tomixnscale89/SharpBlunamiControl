namespace SharpBlunamiControl.GUI
{
    partial class SharpBlunamiControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SharpBlunamiControl));
            this.knobControl1 = new KnobControl.KnobControl();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.cOMPortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cVEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.foundBlunamiMenuStrip = new System.Windows.Forms.ComboBox();
            this.bellButton = new System.Windows.Forms.Button();
            this.whistleButton = new System.Windows.Forms.Button();
            this.shortWhistleButton = new System.Windows.Forms.Button();
            this.cylinderCocksButton = new System.Windows.Forms.Button();
            this.gradeCrossingWhistleButton = new System.Windows.Forms.Button();
            this.blowdownButton = new System.Windows.Forms.Button();
            this.brakeButton = new System.Windows.Forms.Button();
            this.brakeSelectButton = new System.Windows.Forms.Button();
            this.dimmerButton = new System.Windows.Forms.Button();
            this.cutoffPlusButton = new System.Windows.Forms.Button();
            this.cutoffMinusButton = new System.Windows.Forms.Button();
            this.muteButton = new System.Windows.Forms.Button();
            this.blunamiSearchStartButton = new System.Windows.Forms.Button();
            this.ashDumpButton = new System.Windows.Forms.Button();
            this.fuelLoadButton = new System.Windows.Forms.Button();
            this.waterStopButton = new System.Windows.Forms.Button();
            this.wheelChainButton = new System.Windows.Forms.Button();
            this.cabChatterButton = new System.Windows.Forms.Button();
            this.sanderValveButton = new System.Windows.Forms.Button();
            this.injectorButton = new System.Windows.Forms.Button();
            this.wheelSlipButton = new System.Windows.Forms.Button();
            this.fx3Button = new System.Windows.Forms.Button();
            this.switchingModeButton = new System.Windows.Forms.Button();
            this.coupleButton = new System.Windows.Forms.Button();
            this.allAboardButton = new System.Windows.Forms.Button();
            this.fx4Button = new System.Windows.Forms.Button();
            this.fx5Button = new System.Windows.Forms.Button();
            this.fx6Button = new System.Windows.Forms.Button();
            this.fx28Button = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // knobControl1
            // 
            this.knobControl1.EndAngle = 405F;
            this.knobControl1.ImeMode = System.Windows.Forms.ImeMode.On;
            this.knobControl1.KnobBackColor = System.Drawing.Color.White;
            this.knobControl1.KnobPointerStyle = KnobControl.KnobControl.KnobPointerStyles.line;
            this.knobControl1.LargeChange = 5;
            this.knobControl1.Location = new System.Drawing.Point(356, 59);
            this.knobControl1.Margin = new System.Windows.Forms.Padding(2);
            this.knobControl1.Maximum = 100;
            this.knobControl1.Minimum = 0;
            this.knobControl1.Name = "knobControl1";
            this.knobControl1.PointerColor = System.Drawing.Color.SlateBlue;
            this.knobControl1.ScaleColor = System.Drawing.Color.Black;
            this.knobControl1.ScaleDivisions = 11;
            this.knobControl1.ScaleFont = new System.Drawing.Font("Microsoft Sans Serif", 7.875F);
            this.knobControl1.ScaleSubDivisions = 4;
            this.knobControl1.ShowLargeScale = true;
            this.knobControl1.ShowSmallScale = false;
            this.knobControl1.Size = new System.Drawing.Size(237, 237);
            this.knobControl1.SmallChange = 1;
            this.knobControl1.StartAngle = 135F;
            this.knobControl1.TabIndex = 0;
            this.knobControl1.Value = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cOMPortToolStripMenuItem,
            this.toolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(3, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(638, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // cOMPortToolStripMenuItem
            // 
            this.cOMPortToolStripMenuItem.Name = "cOMPortToolStripMenuItem";
            this.cOMPortToolStripMenuItem.Size = new System.Drawing.Size(72, 22);
            this.cOMPortToolStripMenuItem.Text = "COM Port";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cVEditorToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 22);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // cVEditorToolStripMenuItem
            // 
            this.cVEditorToolStripMenuItem.Name = "cVEditorToolStripMenuItem";
            this.cVEditorToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.cVEditorToolStripMenuItem.Text = "CV Editor";
            // 
            // foundBlunamiMenuStrip
            // 
            this.foundBlunamiMenuStrip.FormattingEnabled = true;
            this.foundBlunamiMenuStrip.Location = new System.Drawing.Point(12, 33);
            this.foundBlunamiMenuStrip.Margin = new System.Windows.Forms.Padding(2);
            this.foundBlunamiMenuStrip.Name = "foundBlunamiMenuStrip";
            this.foundBlunamiMenuStrip.Size = new System.Drawing.Size(237, 21);
            this.foundBlunamiMenuStrip.TabIndex = 2;
            // 
            // bellButton
            // 
            this.bellButton.Location = new System.Drawing.Point(12, 59);
            this.bellButton.Name = "bellButton";
            this.bellButton.Size = new System.Drawing.Size(75, 48);
            this.bellButton.TabIndex = 3;
            this.bellButton.Text = "Bell";
            this.bellButton.UseVisualStyleBackColor = true;
            this.bellButton.Click += new System.EventHandler(this.bellButton_Click);
            // 
            // whistleButton
            // 
            this.whistleButton.Location = new System.Drawing.Point(93, 59);
            this.whistleButton.Name = "whistleButton";
            this.whistleButton.Size = new System.Drawing.Size(75, 48);
            this.whistleButton.TabIndex = 4;
            this.whistleButton.Text = "Whistle";
            this.whistleButton.UseVisualStyleBackColor = true;
            // 
            // shortWhistleButton
            // 
            this.shortWhistleButton.Location = new System.Drawing.Point(174, 59);
            this.shortWhistleButton.Name = "shortWhistleButton";
            this.shortWhistleButton.Size = new System.Drawing.Size(75, 48);
            this.shortWhistleButton.TabIndex = 6;
            this.shortWhistleButton.Text = "Short Whistle";
            this.shortWhistleButton.UseVisualStyleBackColor = true;
            // 
            // cylinderCocksButton
            // 
            this.cylinderCocksButton.Location = new System.Drawing.Point(12, 113);
            this.cylinderCocksButton.Name = "cylinderCocksButton";
            this.cylinderCocksButton.Size = new System.Drawing.Size(75, 48);
            this.cylinderCocksButton.TabIndex = 5;
            this.cylinderCocksButton.Text = "Cylinder Cocks";
            this.cylinderCocksButton.UseVisualStyleBackColor = true;
            // 
            // gradeCrossingWhistleButton
            // 
            this.gradeCrossingWhistleButton.Location = new System.Drawing.Point(93, 113);
            this.gradeCrossingWhistleButton.Name = "gradeCrossingWhistleButton";
            this.gradeCrossingWhistleButton.Size = new System.Drawing.Size(75, 48);
            this.gradeCrossingWhistleButton.TabIndex = 7;
            this.gradeCrossingWhistleButton.Text = "Grade Crossing Whistle";
            this.gradeCrossingWhistleButton.UseVisualStyleBackColor = true;
            // 
            // blowdownButton
            // 
            this.blowdownButton.Location = new System.Drawing.Point(174, 113);
            this.blowdownButton.Name = "blowdownButton";
            this.blowdownButton.Size = new System.Drawing.Size(75, 48);
            this.blowdownButton.TabIndex = 8;
            this.blowdownButton.Text = "Blowdown";
            this.blowdownButton.UseVisualStyleBackColor = true;
            // 
            // brakeButton
            // 
            this.brakeButton.Location = new System.Drawing.Point(12, 167);
            this.brakeButton.Name = "brakeButton";
            this.brakeButton.Size = new System.Drawing.Size(75, 48);
            this.brakeButton.TabIndex = 11;
            this.brakeButton.Text = "Brake";
            this.brakeButton.UseVisualStyleBackColor = true;
            // 
            // brakeSelectButton
            // 
            this.brakeSelectButton.Location = new System.Drawing.Point(93, 167);
            this.brakeSelectButton.Name = "brakeSelectButton";
            this.brakeSelectButton.Size = new System.Drawing.Size(75, 48);
            this.brakeSelectButton.TabIndex = 10;
            this.brakeSelectButton.Text = "Brake Select";
            this.brakeSelectButton.UseVisualStyleBackColor = true;
            // 
            // dimmerButton
            // 
            this.dimmerButton.Location = new System.Drawing.Point(174, 167);
            this.dimmerButton.Name = "dimmerButton";
            this.dimmerButton.Size = new System.Drawing.Size(75, 48);
            this.dimmerButton.TabIndex = 9;
            this.dimmerButton.Text = "Dimmer";
            this.dimmerButton.UseVisualStyleBackColor = true;
            // 
            // cutoffPlusButton
            // 
            this.cutoffPlusButton.Location = new System.Drawing.Point(12, 221);
            this.cutoffPlusButton.Name = "cutoffPlusButton";
            this.cutoffPlusButton.Size = new System.Drawing.Size(75, 48);
            this.cutoffPlusButton.TabIndex = 12;
            this.cutoffPlusButton.Text = "Cutoff +";
            this.cutoffPlusButton.UseVisualStyleBackColor = true;
            // 
            // cutoffMinusButton
            // 
            this.cutoffMinusButton.Location = new System.Drawing.Point(93, 221);
            this.cutoffMinusButton.Name = "cutoffMinusButton";
            this.cutoffMinusButton.Size = new System.Drawing.Size(75, 48);
            this.cutoffMinusButton.TabIndex = 14;
            this.cutoffMinusButton.Text = "Cutoff -";
            this.cutoffMinusButton.UseVisualStyleBackColor = true;
            // 
            // muteButton
            // 
            this.muteButton.Location = new System.Drawing.Point(174, 221);
            this.muteButton.Name = "muteButton";
            this.muteButton.Size = new System.Drawing.Size(75, 48);
            this.muteButton.TabIndex = 13;
            this.muteButton.Text = "Mute";
            this.muteButton.UseVisualStyleBackColor = true;
            // 
            // blunamiSearchStartButton
            // 
            this.blunamiSearchStartButton.Location = new System.Drawing.Point(254, 33);
            this.blunamiSearchStartButton.Name = "blunamiSearchStartButton";
            this.blunamiSearchStartButton.Size = new System.Drawing.Size(75, 21);
            this.blunamiSearchStartButton.TabIndex = 15;
            this.blunamiSearchStartButton.Text = "Search";
            this.blunamiSearchStartButton.UseVisualStyleBackColor = true;
            this.blunamiSearchStartButton.Click += new System.EventHandler(this.blunamiSearchButton_Click);
            // 
            // ashDumpButton
            // 
            this.ashDumpButton.Location = new System.Drawing.Point(12, 437);
            this.ashDumpButton.Name = "ashDumpButton";
            this.ashDumpButton.Size = new System.Drawing.Size(75, 48);
            this.ashDumpButton.TabIndex = 19;
            this.ashDumpButton.Text = "Ash Dump";
            this.ashDumpButton.UseVisualStyleBackColor = true;
            // 
            // fuelLoadButton
            // 
            this.fuelLoadButton.Location = new System.Drawing.Point(12, 383);
            this.fuelLoadButton.Name = "fuelLoadButton";
            this.fuelLoadButton.Size = new System.Drawing.Size(75, 48);
            this.fuelLoadButton.TabIndex = 18;
            this.fuelLoadButton.Text = "Fuel Loading";
            this.fuelLoadButton.UseVisualStyleBackColor = true;
            // 
            // waterStopButton
            // 
            this.waterStopButton.Location = new System.Drawing.Point(12, 329);
            this.waterStopButton.Name = "waterStopButton";
            this.waterStopButton.Size = new System.Drawing.Size(75, 48);
            this.waterStopButton.TabIndex = 17;
            this.waterStopButton.Text = "Water Stop";
            this.waterStopButton.UseVisualStyleBackColor = true;
            // 
            // wheelChainButton
            // 
            this.wheelChainButton.Location = new System.Drawing.Point(12, 275);
            this.wheelChainButton.Name = "wheelChainButton";
            this.wheelChainButton.Size = new System.Drawing.Size(75, 48);
            this.wheelChainButton.TabIndex = 16;
            this.wheelChainButton.Text = "Wheel Chains";
            this.wheelChainButton.UseVisualStyleBackColor = true;
            // 
            // cabChatterButton
            // 
            this.cabChatterButton.Location = new System.Drawing.Point(93, 437);
            this.cabChatterButton.Name = "cabChatterButton";
            this.cabChatterButton.Size = new System.Drawing.Size(75, 48);
            this.cabChatterButton.TabIndex = 23;
            this.cabChatterButton.Text = "Cab Chatter";
            this.cabChatterButton.UseVisualStyleBackColor = true;
            // 
            // sanderValveButton
            // 
            this.sanderValveButton.Location = new System.Drawing.Point(93, 383);
            this.sanderValveButton.Name = "sanderValveButton";
            this.sanderValveButton.Size = new System.Drawing.Size(75, 48);
            this.sanderValveButton.TabIndex = 22;
            this.sanderValveButton.Text = "Sander Valve";
            this.sanderValveButton.UseVisualStyleBackColor = true;
            // 
            // injectorButton
            // 
            this.injectorButton.Location = new System.Drawing.Point(93, 329);
            this.injectorButton.Name = "injectorButton";
            this.injectorButton.Size = new System.Drawing.Size(75, 48);
            this.injectorButton.TabIndex = 21;
            this.injectorButton.Text = "Injector";
            this.injectorButton.UseVisualStyleBackColor = true;
            // 
            // wheelSlipButton
            // 
            this.wheelSlipButton.Location = new System.Drawing.Point(93, 275);
            this.wheelSlipButton.Name = "wheelSlipButton";
            this.wheelSlipButton.Size = new System.Drawing.Size(75, 48);
            this.wheelSlipButton.TabIndex = 20;
            this.wheelSlipButton.Text = "Wheel Slip";
            this.wheelSlipButton.UseVisualStyleBackColor = true;
            // 
            // fx3Button
            // 
            this.fx3Button.Location = new System.Drawing.Point(174, 437);
            this.fx3Button.Name = "fx3Button";
            this.fx3Button.Size = new System.Drawing.Size(75, 48);
            this.fx3Button.TabIndex = 27;
            this.fx3Button.Text = "FX3";
            this.fx3Button.UseVisualStyleBackColor = true;
            // 
            // switchingModeButton
            // 
            this.switchingModeButton.Location = new System.Drawing.Point(174, 383);
            this.switchingModeButton.Name = "switchingModeButton";
            this.switchingModeButton.Size = new System.Drawing.Size(75, 48);
            this.switchingModeButton.TabIndex = 26;
            this.switchingModeButton.Text = "Switching Mode";
            this.switchingModeButton.UseVisualStyleBackColor = true;
            // 
            // coupleButton
            // 
            this.coupleButton.Location = new System.Drawing.Point(174, 329);
            this.coupleButton.Name = "coupleButton";
            this.coupleButton.Size = new System.Drawing.Size(75, 48);
            this.coupleButton.TabIndex = 25;
            this.coupleButton.Text = "Coupler";
            this.coupleButton.UseVisualStyleBackColor = true;
            // 
            // allAboardButton
            // 
            this.allAboardButton.Location = new System.Drawing.Point(174, 275);
            this.allAboardButton.Name = "allAboardButton";
            this.allAboardButton.Size = new System.Drawing.Size(75, 48);
            this.allAboardButton.TabIndex = 24;
            this.allAboardButton.Text = "All Aboard";
            this.allAboardButton.UseVisualStyleBackColor = true;
            // 
            // fx4Button
            // 
            this.fx4Button.Location = new System.Drawing.Point(255, 60);
            this.fx4Button.Name = "fx4Button";
            this.fx4Button.Size = new System.Drawing.Size(75, 48);
            this.fx4Button.TabIndex = 28;
            this.fx4Button.Text = "FX4";
            this.fx4Button.UseVisualStyleBackColor = true;
            // 
            // fx5Button
            // 
            this.fx5Button.Location = new System.Drawing.Point(255, 113);
            this.fx5Button.Name = "fx5Button";
            this.fx5Button.Size = new System.Drawing.Size(75, 48);
            this.fx5Button.TabIndex = 29;
            this.fx5Button.Text = "FX5";
            this.fx5Button.UseVisualStyleBackColor = true;
            // 
            // fx6Button
            // 
            this.fx6Button.Location = new System.Drawing.Point(255, 167);
            this.fx6Button.Name = "fx6Button";
            this.fx6Button.Size = new System.Drawing.Size(75, 48);
            this.fx6Button.TabIndex = 30;
            this.fx6Button.Text = "FX6";
            this.fx6Button.UseVisualStyleBackColor = true;
            // 
            // fx28Button
            // 
            this.fx28Button.Location = new System.Drawing.Point(254, 221);
            this.fx28Button.Name = "fx28Button";
            this.fx28Button.Size = new System.Drawing.Size(75, 48);
            this.fx28Button.TabIndex = 31;
            this.fx28Button.Text = "FX28";
            this.fx28Button.UseVisualStyleBackColor = true;
            // 
            // SharpBlunamiControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(638, 522);
            this.Controls.Add(this.fx28Button);
            this.Controls.Add(this.fx6Button);
            this.Controls.Add(this.fx5Button);
            this.Controls.Add(this.fx4Button);
            this.Controls.Add(this.fx3Button);
            this.Controls.Add(this.switchingModeButton);
            this.Controls.Add(this.coupleButton);
            this.Controls.Add(this.allAboardButton);
            this.Controls.Add(this.cabChatterButton);
            this.Controls.Add(this.sanderValveButton);
            this.Controls.Add(this.injectorButton);
            this.Controls.Add(this.wheelSlipButton);
            this.Controls.Add(this.ashDumpButton);
            this.Controls.Add(this.fuelLoadButton);
            this.Controls.Add(this.waterStopButton);
            this.Controls.Add(this.wheelChainButton);
            this.Controls.Add(this.blunamiSearchStartButton);
            this.Controls.Add(this.cutoffMinusButton);
            this.Controls.Add(this.muteButton);
            this.Controls.Add(this.cutoffPlusButton);
            this.Controls.Add(this.brakeButton);
            this.Controls.Add(this.brakeSelectButton);
            this.Controls.Add(this.dimmerButton);
            this.Controls.Add(this.blowdownButton);
            this.Controls.Add(this.gradeCrossingWhistleButton);
            this.Controls.Add(this.shortWhistleButton);
            this.Controls.Add(this.cylinderCocksButton);
            this.Controls.Add(this.whistleButton);
            this.Controls.Add(this.bellButton);
            this.Controls.Add(this.foundBlunamiMenuStrip);
            this.Controls.Add(this.knobControl1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SharpBlunamiControl";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private KnobControl.KnobControl knobControl1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem cOMPortToolStripMenuItem;
        private System.Windows.Forms.ComboBox foundBlunamiMenuStrip;
        private System.Windows.Forms.Button bellButton;
        private System.Windows.Forms.Button whistleButton;
        private System.Windows.Forms.Button shortWhistleButton;
        private System.Windows.Forms.Button cylinderCocksButton;
        private System.Windows.Forms.Button gradeCrossingWhistleButton;
        private System.Windows.Forms.Button blowdownButton;
        private System.Windows.Forms.Button brakeButton;
        private System.Windows.Forms.Button brakeSelectButton;
        private System.Windows.Forms.Button dimmerButton;
        private System.Windows.Forms.Button cutoffPlusButton;
        private System.Windows.Forms.Button cutoffMinusButton;
        private System.Windows.Forms.Button muteButton;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cVEditorToolStripMenuItem;
        private System.Windows.Forms.Button blunamiSearchStartButton;
        private System.Windows.Forms.Button ashDumpButton;
        private System.Windows.Forms.Button fuelLoadButton;
        private System.Windows.Forms.Button waterStopButton;
        private System.Windows.Forms.Button wheelChainButton;
        private System.Windows.Forms.Button cabChatterButton;
        private System.Windows.Forms.Button sanderValveButton;
        private System.Windows.Forms.Button injectorButton;
        private System.Windows.Forms.Button wheelSlipButton;
        private System.Windows.Forms.Button fx3Button;
        private System.Windows.Forms.Button switchingModeButton;
        private System.Windows.Forms.Button coupleButton;
        private System.Windows.Forms.Button allAboardButton;
        private System.Windows.Forms.Button fx4Button;
        private System.Windows.Forms.Button fx5Button;
        private System.Windows.Forms.Button fx6Button;
        private System.Windows.Forms.Button fx28Button;
    }
}