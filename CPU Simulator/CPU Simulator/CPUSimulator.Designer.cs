namespace CPU_Simulator
{
    partial class MainForm
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
            this.pnlRAM = new System.Windows.Forms.Panel();
            this.lblRAMContents = new System.Windows.Forms.Label();
            this.lblAddr = new System.Windows.Forms.Label();
            this.lblRAM = new System.Windows.Forms.Label();
            this.pnlCU = new System.Windows.Forms.Panel();
            this.lblCU = new System.Windows.Forms.Label();
            this.pnlFlags = new System.Windows.Forms.Panel();
            this.lblFlags = new System.Windows.Forms.Label();
            this.pnlALU = new System.Windows.Forms.Panel();
            this.lblALU = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.RichTextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblClock = new System.Windows.Forms.Label();
            this.btnIncrease = new System.Windows.Forms.Button();
            this.btnDecrease = new System.Windows.Forms.Button();
            this.pnlRAM.SuspendLayout();
            this.pnlCU.SuspendLayout();
            this.pnlFlags.SuspendLayout();
            this.pnlALU.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlRAM
            // 
            this.pnlRAM.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRAM.Controls.Add(this.lblRAMContents);
            this.pnlRAM.Controls.Add(this.lblAddr);
            this.pnlRAM.Controls.Add(this.lblRAM);
            this.pnlRAM.Location = new System.Drawing.Point(495, 10);
            this.pnlRAM.Name = "pnlRAM";
            this.pnlRAM.Size = new System.Drawing.Size(270, 50);
            this.pnlRAM.TabIndex = 11;
            // 
            // lblRAMContents
            // 
            this.lblRAMContents.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblRAMContents.Location = new System.Drawing.Point(164, 15);
            this.lblRAMContents.Name = "lblRAMContents";
            this.lblRAMContents.Size = new System.Drawing.Size(60, 20);
            this.lblRAMContents.TabIndex = 6;
            this.lblRAMContents.Text = "00000000";
            this.lblRAMContents.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAddr
            // 
            this.lblAddr.AutoSize = true;
            this.lblAddr.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAddr.Location = new System.Drawing.Point(107, 16);
            this.lblAddr.Name = "lblAddr";
            this.lblAddr.Size = new System.Drawing.Size(54, 18);
            this.lblAddr.TabIndex = 5;
            this.lblAddr.Text = "Addr 0:";
            this.lblAddr.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblRAM
            // 
            this.lblRAM.AutoSize = true;
            this.lblRAM.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRAM.Location = new System.Drawing.Point(38, 16);
            this.lblRAM.Name = "lblRAM";
            this.lblRAM.Size = new System.Drawing.Size(41, 18);
            this.lblRAM.TabIndex = 4;
            this.lblRAM.Text = "RAM";
            this.lblRAM.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlCU
            // 
            this.pnlCU.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCU.Controls.Add(this.lblCU);
            this.pnlCU.Location = new System.Drawing.Point(275, 175);
            this.pnlCU.Name = "pnlCU";
            this.pnlCU.Size = new System.Drawing.Size(350, 200);
            this.pnlCU.TabIndex = 12;
            // 
            // lblCU
            // 
            this.lblCU.AutoSize = true;
            this.lblCU.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCU.Location = new System.Drawing.Point(130, 90);
            this.lblCU.MinimumSize = new System.Drawing.Size(90, 20);
            this.lblCU.Name = "lblCU";
            this.lblCU.Size = new System.Drawing.Size(90, 20);
            this.lblCU.TabIndex = 6;
            this.lblCU.Text = "Control Unit";
            this.lblCU.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlFlags
            // 
            this.pnlFlags.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlFlags.Controls.Add(this.lblFlags);
            this.pnlFlags.Location = new System.Drawing.Point(170, 300);
            this.pnlFlags.Name = "pnlFlags";
            this.pnlFlags.Size = new System.Drawing.Size(75, 75);
            this.pnlFlags.TabIndex = 13;
            // 
            // lblFlags
            // 
            this.lblFlags.AutoSize = true;
            this.lblFlags.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFlags.Location = new System.Drawing.Point(15, 27);
            this.lblFlags.MinimumSize = new System.Drawing.Size(45, 20);
            this.lblFlags.Name = "lblFlags";
            this.lblFlags.Size = new System.Drawing.Size(45, 20);
            this.lblFlags.TabIndex = 4;
            this.lblFlags.Text = "Flags";
            this.lblFlags.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlALU
            // 
            this.pnlALU.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlALU.Controls.Add(this.lblALU);
            this.pnlALU.Location = new System.Drawing.Point(40, 175);
            this.pnlALU.Name = "pnlALU";
            this.pnlALU.Size = new System.Drawing.Size(100, 200);
            this.pnlALU.TabIndex = 13;
            // 
            // lblALU
            // 
            this.lblALU.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.lblALU.AutoSize = true;
            this.lblALU.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblALU.Location = new System.Drawing.Point(30, 90);
            this.lblALU.MinimumSize = new System.Drawing.Size(40, 20);
            this.lblALU.Name = "lblALU";
            this.lblALU.Size = new System.Drawing.Size(40, 20);
            this.lblALU.TabIndex = 6;
            this.lblALU.Text = "ALU";
            this.lblALU.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtInput
            // 
            this.txtInput.BackColor = System.Drawing.SystemColors.HighlightText;
            this.txtInput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtInput.Location = new System.Drawing.Point(784, 10);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(347, 473);
            this.txtInput.TabIndex = 16;
            this.txtInput.Text = "";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(795, 450);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 25);
            this.btnStart.TabIndex = 17;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblClock
            // 
            this.lblClock.AutoSize = true;
            this.lblClock.Location = new System.Drawing.Point(966, 455);
            this.lblClock.Name = "lblClock";
            this.lblClock.Size = new System.Drawing.Size(51, 13);
            this.lblClock.TabIndex = 18;
            this.lblClock.Text = "Real-time";
            // 
            // btnIncrease
            // 
            this.btnIncrease.Location = new System.Drawing.Point(1023, 449);
            this.btnIncrease.Name = "btnIncrease";
            this.btnIncrease.Size = new System.Drawing.Size(30, 25);
            this.btnIncrease.TabIndex = 19;
            this.btnIncrease.Text = "+";
            this.btnIncrease.UseVisualStyleBackColor = true;
            this.btnIncrease.Click += new System.EventHandler(this.btnIncrease_Click);
            // 
            // btnDecrease
            // 
            this.btnDecrease.Location = new System.Drawing.Point(930, 450);
            this.btnDecrease.Name = "btnDecrease";
            this.btnDecrease.Size = new System.Drawing.Size(30, 25);
            this.btnDecrease.TabIndex = 20;
            this.btnDecrease.Text = "-";
            this.btnDecrease.UseVisualStyleBackColor = true;
            this.btnDecrease.Click += new System.EventHandler(this.btnDecrease_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1137, 489);
            this.Controls.Add(this.btnDecrease);
            this.Controls.Add(this.btnIncrease);
            this.Controls.Add(this.lblClock);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.pnlALU);
            this.Controls.Add(this.pnlFlags);
            this.Controls.Add(this.pnlCU);
            this.Controls.Add(this.pnlRAM);
            this.Name = "MainForm";
            this.Text = "CPU Simulator";
            this.pnlRAM.ResumeLayout(false);
            this.pnlRAM.PerformLayout();
            this.pnlCU.ResumeLayout(false);
            this.pnlCU.PerformLayout();
            this.pnlFlags.ResumeLayout(false);
            this.pnlFlags.PerformLayout();
            this.pnlALU.ResumeLayout(false);
            this.pnlALU.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel pnlRAM;
        private System.Windows.Forms.Label lblRAM;
        private System.Windows.Forms.Panel pnlCU;
        private System.Windows.Forms.Label lblCU;
        private System.Windows.Forms.Panel pnlFlags;
        private System.Windows.Forms.Label lblFlags;
        private System.Windows.Forms.Panel pnlALU;
        private System.Windows.Forms.Label lblALU;
        private System.Windows.Forms.RichTextBox txtInput;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblClock;
        private System.Windows.Forms.Button btnIncrease;
        private System.Windows.Forms.Button btnDecrease;
        private System.Windows.Forms.Label lblRAMContents;
        private System.Windows.Forms.Label lblAddr;
    }
}

