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
            this.pnlGPR1 = new System.Windows.Forms.Panel();
            this.lblGPR1Enable = new System.Windows.Forms.Label();
            this.lblGPR1 = new System.Windows.Forms.Label();
            this.txtGPR1 = new System.Windows.Forms.TextBox();
            this.lblGPR1Set = new System.Windows.Forms.Label();
            this.pnlGPR2 = new System.Windows.Forms.Panel();
            this.lblGPR2Enable = new System.Windows.Forms.Label();
            this.lblGPR2 = new System.Windows.Forms.Label();
            this.txtGPR2 = new System.Windows.Forms.TextBox();
            this.lblGPR2Set = new System.Windows.Forms.Label();
            this.pnlGPR3 = new System.Windows.Forms.Panel();
            this.lblGPR3Set = new System.Windows.Forms.Label();
            this.lblGPR3Enable = new System.Windows.Forms.Label();
            this.lblGPR3 = new System.Windows.Forms.Label();
            this.txtGPR3 = new System.Windows.Forms.TextBox();
            this.pnlGPR4 = new System.Windows.Forms.Panel();
            this.lblGPR4Set = new System.Windows.Forms.Label();
            this.lblGPR4Enable = new System.Windows.Forms.Label();
            this.lblGPR4 = new System.Windows.Forms.Label();
            this.txtGPR4 = new System.Windows.Forms.TextBox();
            this.pnlRAM = new System.Windows.Forms.Panel();
            this.lblRAM = new System.Windows.Forms.Label();
            this.pnlCU = new System.Windows.Forms.Panel();
            this.lblCU = new System.Windows.Forms.Label();
            this.pnlFlags = new System.Windows.Forms.Panel();
            this.lblFlags = new System.Windows.Forms.Label();
            this.pnlALU = new System.Windows.Forms.Panel();
            this.lblALU = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.RichTextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.pnlGPR1.SuspendLayout();
            this.pnlGPR2.SuspendLayout();
            this.pnlGPR3.SuspendLayout();
            this.pnlGPR4.SuspendLayout();
            this.pnlRAM.SuspendLayout();
            this.pnlCU.SuspendLayout();
            this.pnlFlags.SuspendLayout();
            this.pnlALU.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlGPR1
            // 
            this.pnlGPR1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGPR1.Controls.Add(this.lblGPR1Enable);
            this.pnlGPR1.Controls.Add(this.lblGPR1);
            this.pnlGPR1.Controls.Add(this.txtGPR1);
            this.pnlGPR1.Controls.Add(this.lblGPR1Set);
            this.pnlGPR1.Location = new System.Drawing.Point(655, 160);
            this.pnlGPR1.Name = "pnlGPR1";
            this.pnlGPR1.Size = new System.Drawing.Size(80, 50);
            this.pnlGPR1.TabIndex = 4;
            // 
            // lblGPR1Enable
            // 
            this.lblGPR1Enable.AutoSize = true;
            this.lblGPR1Enable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR1Enable.Location = new System.Drawing.Point(0, 0);
            this.lblGPR1Enable.Name = "lblGPR1Enable";
            this.lblGPR1Enable.Size = new System.Drawing.Size(18, 20);
            this.lblGPR1Enable.TabIndex = 2;
            this.lblGPR1Enable.Text = "e";
            // 
            // lblGPR1
            // 
            this.lblGPR1.AutoSize = true;
            this.lblGPR1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR1.Location = new System.Drawing.Point(20, 4);
            this.lblGPR1.MinimumSize = new System.Drawing.Size(40, 20);
            this.lblGPR1.Name = "lblGPR1";
            this.lblGPR1.Size = new System.Drawing.Size(40, 20);
            this.lblGPR1.TabIndex = 2;
            this.lblGPR1.Text = "RG1";
            this.lblGPR1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtGPR1
            // 
            this.txtGPR1.Location = new System.Drawing.Point(17, 26);
            this.txtGPR1.Name = "txtGPR1";
            this.txtGPR1.ReadOnly = true;
            this.txtGPR1.Size = new System.Drawing.Size(60, 20);
            this.txtGPR1.TabIndex = 0;
            this.txtGPR1.Text = "0000 0000";
            this.txtGPR1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblGPR1Set
            // 
            this.lblGPR1Set.AutoSize = true;
            this.lblGPR1Set.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR1Set.Location = new System.Drawing.Point(0, 25);
            this.lblGPR1Set.Name = "lblGPR1Set";
            this.lblGPR1Set.Size = new System.Drawing.Size(17, 20);
            this.lblGPR1Set.TabIndex = 3;
            this.lblGPR1Set.Text = "s";
            // 
            // pnlGPR2
            // 
            this.pnlGPR2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGPR2.Controls.Add(this.lblGPR2Enable);
            this.pnlGPR2.Controls.Add(this.lblGPR2);
            this.pnlGPR2.Controls.Add(this.txtGPR2);
            this.pnlGPR2.Controls.Add(this.lblGPR2Set);
            this.pnlGPR2.Location = new System.Drawing.Point(655, 220);
            this.pnlGPR2.Name = "pnlGPR2";
            this.pnlGPR2.Size = new System.Drawing.Size(80, 50);
            this.pnlGPR2.TabIndex = 5;
            // 
            // lblGPR2Enable
            // 
            this.lblGPR2Enable.AutoSize = true;
            this.lblGPR2Enable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR2Enable.Location = new System.Drawing.Point(0, 0);
            this.lblGPR2Enable.Name = "lblGPR2Enable";
            this.lblGPR2Enable.Size = new System.Drawing.Size(18, 20);
            this.lblGPR2Enable.TabIndex = 2;
            this.lblGPR2Enable.Text = "e";
            // 
            // lblGPR2
            // 
            this.lblGPR2.AutoSize = true;
            this.lblGPR2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR2.Location = new System.Drawing.Point(20, 4);
            this.lblGPR2.Name = "lblGPR2";
            this.lblGPR2.Size = new System.Drawing.Size(39, 18);
            this.lblGPR2.TabIndex = 2;
            this.lblGPR2.Text = "RG2";
            this.lblGPR2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtGPR2
            // 
            this.txtGPR2.Location = new System.Drawing.Point(17, 26);
            this.txtGPR2.Name = "txtGPR2";
            this.txtGPR2.ReadOnly = true;
            this.txtGPR2.Size = new System.Drawing.Size(60, 20);
            this.txtGPR2.TabIndex = 0;
            this.txtGPR2.Text = "0000 0000";
            this.txtGPR2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblGPR2Set
            // 
            this.lblGPR2Set.AutoSize = true;
            this.lblGPR2Set.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR2Set.Location = new System.Drawing.Point(0, 25);
            this.lblGPR2Set.Name = "lblGPR2Set";
            this.lblGPR2Set.Size = new System.Drawing.Size(17, 20);
            this.lblGPR2Set.TabIndex = 3;
            this.lblGPR2Set.Text = "s";
            // 
            // pnlGPR3
            // 
            this.pnlGPR3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGPR3.Controls.Add(this.lblGPR3Set);
            this.pnlGPR3.Controls.Add(this.lblGPR3Enable);
            this.pnlGPR3.Controls.Add(this.lblGPR3);
            this.pnlGPR3.Controls.Add(this.txtGPR3);
            this.pnlGPR3.Location = new System.Drawing.Point(655, 280);
            this.pnlGPR3.Name = "pnlGPR3";
            this.pnlGPR3.Size = new System.Drawing.Size(80, 50);
            this.pnlGPR3.TabIndex = 6;
            // 
            // lblGPR3Set
            // 
            this.lblGPR3Set.AutoSize = true;
            this.lblGPR3Set.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR3Set.Location = new System.Drawing.Point(0, 25);
            this.lblGPR3Set.Name = "lblGPR3Set";
            this.lblGPR3Set.Size = new System.Drawing.Size(17, 20);
            this.lblGPR3Set.TabIndex = 3;
            this.lblGPR3Set.Text = "s";
            // 
            // lblGPR3Enable
            // 
            this.lblGPR3Enable.AutoSize = true;
            this.lblGPR3Enable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR3Enable.Location = new System.Drawing.Point(0, 0);
            this.lblGPR3Enable.Name = "lblGPR3Enable";
            this.lblGPR3Enable.Size = new System.Drawing.Size(18, 20);
            this.lblGPR3Enable.TabIndex = 2;
            this.lblGPR3Enable.Text = "e";
            // 
            // lblGPR3
            // 
            this.lblGPR3.AutoSize = true;
            this.lblGPR3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR3.Location = new System.Drawing.Point(20, 4);
            this.lblGPR3.Name = "lblGPR3";
            this.lblGPR3.Size = new System.Drawing.Size(39, 18);
            this.lblGPR3.TabIndex = 2;
            this.lblGPR3.Text = "RG3";
            this.lblGPR3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtGPR3
            // 
            this.txtGPR3.Location = new System.Drawing.Point(17, 26);
            this.txtGPR3.Name = "txtGPR3";
            this.txtGPR3.ReadOnly = true;
            this.txtGPR3.Size = new System.Drawing.Size(60, 20);
            this.txtGPR3.TabIndex = 0;
            this.txtGPR3.Text = "0000 0000";
            this.txtGPR3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pnlGPR4
            // 
            this.pnlGPR4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGPR4.Controls.Add(this.lblGPR4Set);
            this.pnlGPR4.Controls.Add(this.lblGPR4Enable);
            this.pnlGPR4.Controls.Add(this.lblGPR4);
            this.pnlGPR4.Controls.Add(this.txtGPR4);
            this.pnlGPR4.Location = new System.Drawing.Point(655, 340);
            this.pnlGPR4.Name = "pnlGPR4";
            this.pnlGPR4.Size = new System.Drawing.Size(80, 50);
            this.pnlGPR4.TabIndex = 7;
            // 
            // lblGPR4Set
            // 
            this.lblGPR4Set.AutoSize = true;
            this.lblGPR4Set.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR4Set.Location = new System.Drawing.Point(0, 25);
            this.lblGPR4Set.Name = "lblGPR4Set";
            this.lblGPR4Set.Size = new System.Drawing.Size(17, 20);
            this.lblGPR4Set.TabIndex = 3;
            this.lblGPR4Set.Text = "s";
            // 
            // lblGPR4Enable
            // 
            this.lblGPR4Enable.AutoSize = true;
            this.lblGPR4Enable.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR4Enable.Location = new System.Drawing.Point(0, 0);
            this.lblGPR4Enable.Name = "lblGPR4Enable";
            this.lblGPR4Enable.Size = new System.Drawing.Size(18, 20);
            this.lblGPR4Enable.TabIndex = 2;
            this.lblGPR4Enable.Text = "e";
            // 
            // lblGPR4
            // 
            this.lblGPR4.AutoSize = true;
            this.lblGPR4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGPR4.Location = new System.Drawing.Point(20, 4);
            this.lblGPR4.Name = "lblGPR4";
            this.lblGPR4.Size = new System.Drawing.Size(39, 18);
            this.lblGPR4.TabIndex = 2;
            this.lblGPR4.Text = "RG4";
            this.lblGPR4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtGPR4
            // 
            this.txtGPR4.Location = new System.Drawing.Point(17, 26);
            this.txtGPR4.Name = "txtGPR4";
            this.txtGPR4.ReadOnly = true;
            this.txtGPR4.Size = new System.Drawing.Size(60, 20);
            this.txtGPR4.TabIndex = 0;
            this.txtGPR4.Text = "0000 0000";
            this.txtGPR4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // pnlRAM
            // 
            this.pnlRAM.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRAM.Controls.Add(this.lblRAM);
            this.pnlRAM.Location = new System.Drawing.Point(495, 10);
            this.pnlRAM.Name = "pnlRAM";
            this.pnlRAM.Size = new System.Drawing.Size(270, 50);
            this.pnlRAM.TabIndex = 11;
            // 
            // lblRAM
            // 
            this.lblRAM.AutoSize = true;
            this.lblRAM.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRAM.Location = new System.Drawing.Point(112, 16);
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
            this.btnStart.Location = new System.Drawing.Point(800, 450);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 17;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1137, 489);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtInput);
            this.Controls.Add(this.pnlALU);
            this.Controls.Add(this.pnlFlags);
            this.Controls.Add(this.pnlCU);
            this.Controls.Add(this.pnlRAM);
            this.Controls.Add(this.pnlGPR4);
            this.Controls.Add(this.pnlGPR3);
            this.Controls.Add(this.pnlGPR2);
            this.Controls.Add(this.pnlGPR1);
            this.Name = "MainForm";
            this.Text = "CPU Simulator";
            this.pnlGPR1.ResumeLayout(false);
            this.pnlGPR1.PerformLayout();
            this.pnlGPR2.ResumeLayout(false);
            this.pnlGPR2.PerformLayout();
            this.pnlGPR3.ResumeLayout(false);
            this.pnlGPR3.PerformLayout();
            this.pnlGPR4.ResumeLayout(false);
            this.pnlGPR4.PerformLayout();
            this.pnlRAM.ResumeLayout(false);
            this.pnlRAM.PerformLayout();
            this.pnlCU.ResumeLayout(false);
            this.pnlCU.PerformLayout();
            this.pnlFlags.ResumeLayout(false);
            this.pnlFlags.PerformLayout();
            this.pnlALU.ResumeLayout(false);
            this.pnlALU.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel pnlGPR1;
        private System.Windows.Forms.Label lblGPR1Set;
        private System.Windows.Forms.Label lblGPR1Enable;
        private System.Windows.Forms.Label lblGPR1;
        private System.Windows.Forms.TextBox txtGPR1;
        private System.Windows.Forms.Panel pnlGPR2;
        private System.Windows.Forms.Label lblGPR2Set;
        private System.Windows.Forms.Label lblGPR2Enable;
        private System.Windows.Forms.Label lblGPR2;
        private System.Windows.Forms.TextBox txtGPR2;
        private System.Windows.Forms.Panel pnlGPR3;
        private System.Windows.Forms.Label lblGPR3Set;
        private System.Windows.Forms.Label lblGPR3Enable;
        private System.Windows.Forms.Label lblGPR3;
        private System.Windows.Forms.TextBox txtGPR3;
        private System.Windows.Forms.Panel pnlGPR4;
        private System.Windows.Forms.Label lblGPR4Set;
        private System.Windows.Forms.Label lblGPR4Enable;
        private System.Windows.Forms.Label lblGPR4;
        private System.Windows.Forms.TextBox txtGPR4;
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
    }
}

