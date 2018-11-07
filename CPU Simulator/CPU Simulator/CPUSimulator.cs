﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace CPU_Simulator
{
    public delegate void EnableBus();
    public delegate void UpdateRegisterContents(BitArray data);
    public delegate void UpdateGPRContents(BitArray data, int index);

    public partial class MainForm : Form
    {
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        private ControlUnit m_CU;

        private Panel pnlTmp = new Panel();
        private Label lblTmp = new Label();
        private Label lblTmpContents = new Label();

        private Panel pnlBus1 = new Panel();
        private Label lblBus1 = new Label();

        private Panel pnlAcc = new Panel();
        private Label lblAcc = new Label();
        private Label lblAccContents = new Label();

        private Panel pnlIar = new Panel();
        private Label lblIar = new Label();
        private Label lblIarContents = new Label();

        private Panel pnlIr = new Panel();
        private Label lblIr = new Label();
        private Label lblIrContents = new Label();

        private Panel pnlMar = new Panel();
        private Label lblMar = new Label();
        private Label lblMarContents = new Label();

        private Panel[] pnlGPR = new Panel[Globals.NO_OF_GPR];
        private Label[] lblGPR = new Label[Globals.NO_OF_GPR];
        private Label[] lblGPRContents = new Label[Globals.NO_OF_GPR];

        public MainForm()
        {
            InitializeComponent();
            Paint += drawBus;
            drawCPU();

            BitArray loadintoreg1 = new BitArray(new bool[] { false, false, false, false, false, true, false, false });
            BitArray inputa = new BitArray(new bool[] { false, false, true, false, false, false, false, false });
            BitArray jumptoaddr = new BitArray(new bool[] { false, false, false, false, true, true, false, false });
            BitArray empty = new BitArray(new bool[] { false, false, false, false, false, false, false, false });
            BitArray loadintoreg3 = new BitArray(new bool[] { false, true, false, false, false, true, false, false });
            BitArray inputc = new BitArray(new bool[] { false, false, false, false, false, false, false, true });

            BitArray[] instructions = new BitArray[] { loadintoreg1, inputa, jumptoaddr, empty, loadintoreg3, inputc };
            m_CU = new ControlUnit(instructions, setRAM, readRAM, updateAcc, enableBus);

            timer.Interval = Globals.clockspeed;
            timer.Tick += new EventHandler(resetColours);
            timer.Enabled = true;
            timer.Start();

            m_CU.UpdateIAR += updateIAR;
            m_CU.UpdateIR += updateIR;
            m_CU.UpdateMAR += updateMAR;
            m_CU.UpdateTMP += updateTMP;
            m_CU.UpdateGPRs += updateGPR;
        }

        private void drawCPU()
        {
            SuspendLayout();

            //TMP register
            //--------------------------------------------
            pnlTmp.SuspendLayout();
        
            pnlTmp.BorderStyle = BorderStyle.FixedSingle;
            pnlTmp.Location = new Point(80, 60);
            pnlTmp.Size = new Size(80, 50);
            Controls.Add(pnlTmp);

            lblTmp.Text = "TMP";
            lblTmp.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblTmp.TextAlign = ContentAlignment.MiddleCenter;
            lblTmp.Location = new Point(20, 4);
            lblTmp.Size = new Size(40, 20);
            pnlTmp.Controls.Add(lblTmp);

            lblTmpContents.Text = "00000000";
            lblTmpContents.TextAlign = ContentAlignment.MiddleCenter;
            lblTmpContents.BorderStyle = BorderStyle.FixedSingle;
            lblTmpContents.Location = new Point(10, 26);
            lblTmpContents.Size = new Size(60, 20);
            pnlTmp.Controls.Add(lblTmpContents);

            pnlTmp.ResumeLayout();
            pnlTmp.PerformLayout();
            //--------------------------------------------

            //BUS1 register
            //--------------------------------------------
            pnlBus1.SuspendLayout();

            pnlBus1.BorderStyle = BorderStyle.FixedSingle;
            pnlBus1.Location = new Point(91, 130);
            pnlBus1.Size = new Size(60, 25);
            Controls.Add(pnlBus1);

            lblBus1.Text = "BUS1";
            lblBus1.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblBus1.TextAlign = ContentAlignment.MiddleCenter;
            lblBus1.Location = new Point(5, 1);
            lblBus1.Size = new Size(50, 20);
            pnlBus1.Controls.Add(lblBus1);

            pnlBus1.ResumeLayout();
            pnlBus1.PerformLayout();
            //--------------------------------------------

            //ACC register
            //--------------------------------------------
            pnlAcc.SuspendLayout();

            pnlAcc.BorderStyle = BorderStyle.FixedSingle;
            pnlAcc.Location = new Point(50, 395);
            pnlAcc.Size = new Size(80, 50);
            Controls.Add(pnlAcc);

            lblAcc.Text = "ACC";
            lblAcc.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblAcc.TextAlign = ContentAlignment.MiddleCenter;
            lblAcc.Location = new Point(22, 4);
            lblAcc.Size = new Size(40, 20);
            pnlAcc.Controls.Add(lblAcc);

            lblAccContents.Text = "00000000";
            lblAccContents.TextAlign = ContentAlignment.MiddleCenter;
            lblAccContents.BorderStyle = BorderStyle.FixedSingle;
            lblAccContents.Location = new Point(10, 26);
            lblAccContents.Size = new Size(60, 20);
            pnlAcc.Controls.Add(lblAccContents);

            pnlAcc.ResumeLayout();
            pnlAcc.PerformLayout();
            //--------------------------------------------

            //IAR register
            //--------------------------------------------
            pnlIar.SuspendLayout();

            pnlIar.BorderStyle = BorderStyle.FixedSingle;
            pnlIar.Location = new Point(350, 395);
            pnlIar.Size = new Size(80, 50);
            Controls.Add(pnlIar);

            lblIar.Text = "IAR";
            lblIar.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblIar.TextAlign = ContentAlignment.MiddleCenter;
            lblIar.Location = new Point(22, 4);
            lblIar.Size = new Size(40, 20);
            pnlIar.Controls.Add(lblIar);

            lblIarContents.Text = "00000000";
            lblIarContents.TextAlign = ContentAlignment.MiddleCenter;
            lblIarContents.BorderStyle = BorderStyle.FixedSingle;
            lblIarContents.Location = new Point(10, 26);
            lblIarContents.Size = new Size(60, 20);
            pnlIar.Controls.Add(lblIarContents);

            pnlIar.ResumeLayout();
            pnlIar.PerformLayout();
            //--------------------------------------------

            //IR register
            //--------------------------------------------
            pnlIr.SuspendLayout();

            pnlIr.BorderStyle = BorderStyle.FixedSingle;
            pnlIr.Location = new Point(470, 395);
            pnlIr.Size = new Size(80, 50);
            Controls.Add(pnlIr);

            lblIr.Text = "IR";
            lblIr.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblIr.TextAlign = ContentAlignment.MiddleCenter;
            lblIr.Location = new Point(22, 4);
            lblIr.Size = new Size(40, 20);
            pnlIr.Controls.Add(lblIr);

            lblIrContents.Text = "00000000";
            lblIrContents.TextAlign = ContentAlignment.MiddleCenter;
            lblIrContents.BorderStyle = BorderStyle.FixedSingle;
            lblIrContents.Location = new Point(10, 26);
            lblIrContents.Size = new Size(60, 20);
            pnlIr.Controls.Add(lblIrContents);

            pnlIr.ResumeLayout();
            pnlIr.PerformLayout();
            //--------------------------------------------

            //MAR register
            //--------------------------------------------
            pnlMar.SuspendLayout();

            pnlMar.BorderStyle = BorderStyle.FixedSingle;
            pnlMar.Location = new Point(415, 10);
            pnlMar.Size = new Size(80, 50);
            Controls.Add(pnlMar);

            lblMar.Text = "MAR";
            lblMar.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblMar.TextAlign = ContentAlignment.MiddleCenter;
            lblMar.Location = new Point(20, 4);
            lblMar.Size = new Size(41, 20);
            pnlMar.Controls.Add(lblMar);

            lblMarContents.Text = "00000000";
            lblMarContents.TextAlign = ContentAlignment.MiddleCenter;
            lblMarContents.BorderStyle = BorderStyle.FixedSingle;
            lblMarContents.Location = new Point(10, 26);
            lblMarContents.Size = new Size(60, 20);
            pnlMar.Controls.Add(lblMarContents);

            pnlMar.ResumeLayout();
            pnlMar.PerformLayout();
            //--------------------------------------------

            for (int count = 0; count < Globals.NO_OF_GPR; count++)
            {
                pnlGPR[count] = new Panel();
                lblGPR[count] = new Label();
                lblGPRContents[count] = new Label();
            }

            //GPR registers
            //--------------------------------------------
            pnlGPR[0].SuspendLayout();

            pnlGPR[0].BorderStyle = BorderStyle.FixedSingle;
            pnlGPR[0].Location = new Point(655, 160);
            pnlGPR[0].Size = new Size(80, 50);
            Controls.Add(pnlGPR[0]);

            lblGPR[0].Text = "RG1";
            lblGPR[0].Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblGPR[0].TextAlign = ContentAlignment.MiddleCenter;
            lblGPR[0].Location = new Point(20, 4);
            lblGPR[0].Size = new Size(41, 20);
            pnlGPR[0].Controls.Add(lblGPR[0]);

            lblGPRContents[0].Text = "00000000";
            lblGPRContents[0].TextAlign = ContentAlignment.MiddleCenter;
            lblGPRContents[0].BorderStyle = BorderStyle.FixedSingle;
            lblGPRContents[0].Location = new Point(10, 26);
            lblGPRContents[0].Size = new Size(60, 20);
            pnlGPR[0].Controls.Add(lblGPRContents[0]);

            pnlGPR[0].ResumeLayout();
            pnlGPR[0].PerformLayout();

            //--------------------------------------------
            pnlGPR[1].SuspendLayout();

            pnlGPR[1].BorderStyle = BorderStyle.FixedSingle;
            pnlGPR[1].Location = new Point(655, 220);
            pnlGPR[1].Size = new Size(80, 50);
            Controls.Add(pnlGPR[1]);

            lblGPR[1].Text = "RG2";
            lblGPR[1].Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblGPR[1].TextAlign = ContentAlignment.MiddleCenter;
            lblGPR[1].Location = new Point(20, 4);
            lblGPR[1].Size = new Size(41, 20);
            pnlGPR[1].Controls.Add(lblGPR[1]);

            lblGPRContents[1].Text = "00000000";
            lblGPRContents[1].TextAlign = ContentAlignment.MiddleCenter;
            lblGPRContents[1].BorderStyle = BorderStyle.FixedSingle;
            lblGPRContents[1].Location = new Point(10, 26);
            lblGPRContents[1].Size = new Size(60, 20);
            pnlGPR[1].Controls.Add(lblGPRContents[1]);

            pnlGPR[1].ResumeLayout();
            pnlGPR[1].PerformLayout();

            //--------------------------------------------
            pnlGPR[2].SuspendLayout();

            pnlGPR[2].BorderStyle = BorderStyle.FixedSingle;
            pnlGPR[2].Location = new Point(655, 280);
            pnlGPR[2].Size = new Size(80, 50);
            Controls.Add(pnlGPR[2]);

            lblGPR[2].Text = "RG3";
            lblGPR[2].Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblGPR[2].TextAlign = ContentAlignment.MiddleCenter;
            lblGPR[2].Location = new Point(20, 4);
            lblGPR[2].Size = new Size(41, 20);
            pnlGPR[2].Controls.Add(lblGPR[2]);

            lblGPRContents[2].Text = "00000000";
            lblGPRContents[2].TextAlign = ContentAlignment.MiddleCenter;
            lblGPRContents[2].BorderStyle = BorderStyle.FixedSingle;
            lblGPRContents[2].Location = new Point(10, 26);
            lblGPRContents[2].Size = new Size(60, 20);
            pnlGPR[2].Controls.Add(lblGPRContents[2]);

            pnlGPR[2].ResumeLayout();
            pnlGPR[2].PerformLayout();

            //--------------------------------------------
            pnlGPR[3].SuspendLayout();

            pnlGPR[3].BorderStyle = BorderStyle.FixedSingle;
            pnlGPR[3].Location = new Point(655, 340);
            pnlGPR[3].Size = new Size(80, 50);
            Controls.Add(pnlGPR[3]);

            lblGPR[3].Text = "RG4";
            lblGPR[3].Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblGPR[3].TextAlign = ContentAlignment.MiddleCenter;
            lblGPR[3].Location = new Point(20, 4);
            lblGPR[3].Size = new Size(41, 20);
            pnlGPR[3].Controls.Add(lblGPR[3]);

            lblGPRContents[3].Text = "00000000";
            lblGPRContents[3].TextAlign = ContentAlignment.MiddleCenter;
            lblGPRContents[3].BorderStyle = BorderStyle.FixedSingle;
            lblGPRContents[3].Location = new Point(10, 26);
            lblGPRContents[3].Size = new Size(60, 20);
            pnlGPR[3].Controls.Add(lblGPRContents[3]);

            pnlGPR[3].ResumeLayout();
            pnlGPR[3].PerformLayout();
            //--------------------------------------------

            Invalidate();
            ResumeLayout();
            PerformLayout();
        }

        private void btnDecrease_Click(object sender, EventArgs e)
        {
            if (Globals.clockspeed != 1)
            {
                if (Globals.clockspeed == 500) Globals.clockspeed -= 499;
                else Globals.clockspeed -= 500;

                lblClock.Text = Globals.clockspeed.ToString();
                timer.Interval = Globals.clockspeed;
            }
        }

        private void btnIncrease_Click(object sender, EventArgs e)
        {
            if (Globals.clockspeed == 1) Globals.clockspeed += 499;
            else Globals.clockspeed += 500;

            lblClock.Text = Globals.clockspeed.ToString();
            timer.Interval = Globals.clockspeed;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Thread CPUThread = new Thread(new ThreadStart(m_CU.start));
            CPUThread.Start();
        }

        private void drawBus(object sender, PaintEventArgs e)
        {
            Graphics g = CreateGraphics();

            Pen myPen = new System.Drawing.Pen(Color.Black);
            myPen.Width = 1;

            //bus
            //---------------------------------
            //line from MAR to top left
            g.DrawLine(myPen, 415, 40, 15, 40);
            g.DrawLine(myPen, 415, 35, 10, 35);

            //line from top left to bottom
            g.DrawLine(myPen, 15, 40, 15, 465);
            g.DrawLine(myPen, 10, 35, 10, 470);

            //line from bottom left to right
            g.DrawLine(myPen, 10, 470, 760, 470);
            g.DrawLine(myPen, 15, 465, 755, 465);

            //line from bottom right to RAM
            g.DrawLine(myPen, 760, 470, 760, 60);
            g.DrawLine(myPen, 755, 465, 755, 60);
            //---------------------------------

            //bus to other components
            //----------------------------------------
            //line from bus to tmp
            g.DrawLine(myPen, 117.5f, 35, 117.5f, 75);
            g.DrawLine(myPen, 122.5f, 35, 122.5f, 75);

            //line from bus to ALU
            g.DrawLine(myPen, 57.5f, 35, 57.5f, 180);
            g.DrawLine(myPen, 62.5f, 35, 62.5f, 180);

            //line from tmp to bus1
            g.DrawLine(myPen, 117.5f, 110, 117.5f, 130);
            g.DrawLine(myPen, 122.5f, 110, 122.5f, 130);

            //line from bus1 to ALU
            g.DrawLine(myPen, 117.5f, 155, 117.5f, 180);
            g.DrawLine(myPen, 122.5f, 155, 122.5f, 180);

            //line from ALU to ACC
            g.DrawLine(myPen, 87.5f, 375, 87.5f, 395);
            g.DrawLine(myPen, 92.5f, 375, 92.5f, 395);

            //line from ACC to bus
            g.DrawLine(myPen, 87.5f, 445, 87.5f, 470);
            g.DrawLine(myPen, 92.5f, 445, 92.5f, 470);

            //line from bus to between IAR and IR
            g.DrawLine(myPen, 447.5f, 470, 447.5f, 422.5f);
            g.DrawLine(myPen, 452.5f, 470, 452.5f, 422.5f);

            //line from bus to IAR
            g.DrawLine(myPen, 452.5f, 422.5f, 430, 422.5f);
            g.DrawLine(myPen, 452.5f, 427.5f, 430, 427.5f);

            //line from bus to IR
            g.DrawLine(myPen, 447.5f, 422.5f, 470, 422.5f);
            g.DrawLine(myPen, 447.5f, 427.5f, 470, 427.5f);

            //line from bus to GPR1
            g.DrawLine(myPen, 735, 182.5f, 760, 182.5f);
            g.DrawLine(myPen, 735, 187.5f, 760, 187.5f);

            //line from bus to GPR2
            g.DrawLine(myPen, 735, 242.5f, 760, 242.5f);
            g.DrawLine(myPen, 735, 247.5f, 760, 247.5f);

            //line from bus to GPR3
            g.DrawLine(myPen, 735, 302.5f, 760, 302.5f);
            g.DrawLine(myPen, 735, 307.5f, 760, 307.5f);

            //line from bus to GPR4
            g.DrawLine(myPen, 735, 362.5f, 760, 362.5f);
            g.DrawLine(myPen, 735, 367.5f, 760, 367.5f);
            //----------------------------------------
        }

        public void drawBus()
        {
            Graphics g = CreateGraphics();

            Pen myPen = new Pen(Color.Black);
            myPen.Width = 1;

            //bus
            //---------------------------------
            //line from MAR to top left
            g.DrawLine(myPen, 415, 40, 15, 40);
            g.DrawLine(myPen, 415, 35, 10, 35);

            //line from top left to bottom
            g.DrawLine(myPen, 15, 40, 15, 465);
            g.DrawLine(myPen, 10, 35, 10, 470);

            //line from bottom left to right
            g.DrawLine(myPen, 10, 470, 760, 470);
            g.DrawLine(myPen, 15, 465, 755, 465);

            //line from bottom right to RAM
            g.DrawLine(myPen, 760, 470, 760, 60);
            g.DrawLine(myPen, 755, 465, 755, 60);
            //---------------------------------

            //bus to other components
            //----------------------------------------
            //line from bus to tmp
            g.DrawLine(myPen, 117.5f, 35, 117.5f, 75);
            g.DrawLine(myPen, 122.5f, 35, 122.5f, 75);

            //line from bus to ALU
            g.DrawLine(myPen, 57.5f, 35, 57.5f, 180);
            g.DrawLine(myPen, 62.5f, 35, 62.5f, 180);

            //line from tmp to bus1
            g.DrawLine(myPen, 117.5f, 110, 117.5f, 130);
            g.DrawLine(myPen, 122.5f, 110, 122.5f, 130);

            //line from bus1 to ALU
            g.DrawLine(myPen, 117.5f, 155, 117.5f, 180);
            g.DrawLine(myPen, 122.5f, 155, 122.5f, 180);

            //line from ALU to ACC
            g.DrawLine(myPen, 87.5f, 375, 87.5f, 395);
            g.DrawLine(myPen, 92.5f, 375, 92.5f, 395);

            //line from ACC to bus
            g.DrawLine(myPen, 87.5f, 445, 87.5f, 470);
            g.DrawLine(myPen, 92.5f, 445, 92.5f, 470);

            //line from bus to between IAR and IR
            g.DrawLine(myPen, 447.5f, 470, 447.5f, 422.5f);
            g.DrawLine(myPen, 452.5f, 470, 452.5f, 422.5f);

            //line from bus to IAR
            g.DrawLine(myPen, 452.5f, 422.5f, 430, 422.5f);
            g.DrawLine(myPen, 452.5f, 427.5f, 430, 427.5f);

            //line from bus to IR
            g.DrawLine(myPen, 447.5f, 422.5f, 470, 422.5f);
            g.DrawLine(myPen, 447.5f, 427.5f, 470, 427.5f);

            //line from bus to GPR1
            g.DrawLine(myPen, 735, 182.5f, 760, 182.5f);
            g.DrawLine(myPen, 735, 187.5f, 760, 187.5f);

            //line from bus to GPR2
            g.DrawLine(myPen, 735, 242.5f, 760, 242.5f);
            g.DrawLine(myPen, 735, 247.5f, 760, 247.5f);

            //line from bus to GPR3
            g.DrawLine(myPen, 735, 302.5f, 760, 302.5f);
            g.DrawLine(myPen, 735, 307.5f, 760, 307.5f);

            //line from bus to GPR4
            g.DrawLine(myPen, 735, 362.5f, 760, 362.5f);
            g.DrawLine(myPen, 735, 367.5f, 760, 367.5f);
            //----------------------------------------
        }

        public void resetColours(object source, EventArgs e)
        {
            SuspendLayout();

            lblTmpContents.ForeColor = Color.Black;
            lblAccContents.ForeColor = Color.Black;
            lblIarContents.ForeColor = Color.Black;
            lblIrContents.ForeColor = Color.Black;
            lblMarContents.ForeColor = Color.Black;
            lblRAMContents.ForeColor = Color.Black;

            for (int count = 0; count < Globals.NO_OF_GPR; count++)
            {
                lblGPRContents[count].ForeColor = Color.Black;
            }

            ResumeLayout();
            drawBus();
        }

        public void updateIAR(BitArray data)
        {
            if (InvokeRequired) Invoke(new UpdateRegisterContents(updateIAR), new object[] { data });
            else
            {
                lblIarContents.Text = Globals.convertBitsToString(data);
                lblIarContents.ForeColor = Color.Red;
            }
        }

        public void updateIR(BitArray data)
        {
            if (InvokeRequired) Invoke(new UpdateRegisterContents(updateIR), new object[] { data });
            else
            {
                lblIrContents.Text = Globals.convertBitsToString(data);
                lblIrContents.ForeColor = Color.Red;
            }
        }

        public void updateMAR(BitArray data)
        {
            if (InvokeRequired) Invoke(new UpdateRegisterContents(updateMAR), new object[] { data });
            else
            {
                lblMarContents.Text = Globals.convertBitsToString(data);
                lblMarContents.ForeColor = Color.Red;
            }
        }

        public void updateTMP(BitArray data)
        {
            if (InvokeRequired) Invoke(new UpdateRegisterContents(updateTMP), new object[] { data });
            else
            {
                lblTmpContents.Text = Globals.convertBitsToString(data);
                lblTmpContents.ForeColor = Color.Red;
            }
        }

        public void updateAcc(BitArray data)
        {
            if (InvokeRequired) Invoke(new UpdateRegisterContents(updateAcc), new object[] { data });
            else
            {
                lblAccContents.Text = Globals.convertBitsToString(data);
                lblAccContents.ForeColor = Color.Red;
            }
        }

        public void readRAM(BitArray data, int address)
        {
            if (InvokeRequired) Invoke(new UpdateGPRContents(readRAM), new object[] { data, address });
            else
            {
                lblRAMContents.Text = Globals.convertBitsToString(data);
                lblRAMContents.ForeColor = Color.Blue;
                lblAddr.Text = "Addr " + address.ToString();
            }
        }

        public void setRAM(BitArray data, int address)
        {
            if (InvokeRequired) Invoke(new UpdateGPRContents(setRAM), new object[] { data, address });
            else
            {
                lblRAMContents.Text = Globals.convertBitsToString(data);
                lblRAMContents.ForeColor = Color.Red;
                lblAddr.Text = "Addr " + address.ToString();
            }
        }

        public void updateGPR(BitArray data, int GPRindex)
        {
            if (InvokeRequired) Invoke(new UpdateGPRContents(updateGPR), new object[] { data, GPRindex });
            else
            {
                if (GPRindex < Globals.NO_OF_GPR)
                {
                    lblGPRContents[GPRindex].Text = Globals.convertBitsToString(data);
                    lblGPRContents[GPRindex].ForeColor = Color.Red;
                }
            }
        }

        public void enableBus()
        {
            if (InvokeRequired) Invoke(new EnableBus(enableBus));
            else
            {
                Graphics g = CreateGraphics();

                Pen myPen = new Pen(Color.Red);
                myPen.Width = 1;

                //bus
                //---------------------------------
                //line from MAR to top left
                g.DrawLine(myPen, 415, 40, 15, 40);
                g.DrawLine(myPen, 415, 35, 10, 35);

                //line from top left to bottom
                g.DrawLine(myPen, 15, 40, 15, 465);
                g.DrawLine(myPen, 10, 35, 10, 470);

                //line from bottom left to right
                g.DrawLine(myPen, 10, 470, 760, 470);
                g.DrawLine(myPen, 15, 465, 755, 465);

                //line from bottom right to RAM
                g.DrawLine(myPen, 760, 470, 760, 60);
                g.DrawLine(myPen, 755, 465, 755, 60);
                //---------------------------------

                //bus to other components
                //----------------------------------------
                //line from bus to tmp
                g.DrawLine(myPen, 117.5f, 35, 117.5f, 75);
                g.DrawLine(myPen, 122.5f, 35, 122.5f, 75);

                //line from bus to ALU
                g.DrawLine(myPen, 57.5f, 35, 57.5f, 180);
                g.DrawLine(myPen, 62.5f, 35, 62.5f, 180);

                //line from tmp to bus1
                g.DrawLine(myPen, 117.5f, 110, 117.5f, 130);
                g.DrawLine(myPen, 122.5f, 110, 122.5f, 130);

                //line from bus1 to ALU
                g.DrawLine(myPen, 117.5f, 155, 117.5f, 180);
                g.DrawLine(myPen, 122.5f, 155, 122.5f, 180);

                //line from ALU to ACC
                g.DrawLine(myPen, 87.5f, 375, 87.5f, 395);
                g.DrawLine(myPen, 92.5f, 375, 92.5f, 395);

                //line from ACC to bus
                g.DrawLine(myPen, 87.5f, 445, 87.5f, 470);
                g.DrawLine(myPen, 92.5f, 445, 92.5f, 470);

                //line from bus to between IAR and IR
                g.DrawLine(myPen, 447.5f, 470, 447.5f, 422.5f);
                g.DrawLine(myPen, 452.5f, 470, 452.5f, 422.5f);

                //line from bus to IAR
                g.DrawLine(myPen, 452.5f, 422.5f, 430, 422.5f);
                g.DrawLine(myPen, 452.5f, 427.5f, 430, 427.5f);

                //line from bus to IR
                g.DrawLine(myPen, 447.5f, 422.5f, 470, 422.5f);
                g.DrawLine(myPen, 447.5f, 427.5f, 470, 427.5f);

                //line from bus to GPR1
                g.DrawLine(myPen, 735, 182.5f, 760, 182.5f);
                g.DrawLine(myPen, 735, 187.5f, 760, 187.5f);

                //line from bus to GPR2
                g.DrawLine(myPen, 735, 242.5f, 760, 242.5f);
                g.DrawLine(myPen, 735, 247.5f, 760, 247.5f);

                //line from bus to GPR3
                g.DrawLine(myPen, 735, 302.5f, 760, 302.5f);
                g.DrawLine(myPen, 735, 307.5f, 760, 307.5f);

                //line from bus to GPR4
                g.DrawLine(myPen, 735, 362.5f, 760, 362.5f);
                g.DrawLine(myPen, 735, 367.5f, 760, 367.5f);
                //----------------------------------------
            }
        }
    }

    public static class Globals
    {
        public const int RAM_SIZE = 256;
        public const int NO_OF_FLAGS = 4;
        public const int NO_OF_GPR = 4;
        public const int BYTE_LENGTH = 8;
        public const int BYTE_LAST = BYTE_LENGTH - 1;
        public const int OPCODE_LENGTH = BYTE_LENGTH / 2;
        public const int OPCODE_LAST = OPCODE_LENGTH - 1;
        public const int REGISTER_CODE_LENGTH = 2;

        public static int clockspeed = 1;

        public static string convertBitsToString(BitArray data)
        {
            char[] result = new char[data.Length];
            int lastbit = data.Length - 1;

            //set value to 0 for false and 1 for true
            for (int count = 0, reverse = lastbit; count < data.Length; count++, reverse--)
            {
                //copy MSB into front of string as bitarray stores in reverse
                if (data[count]) result[reverse] = '1';
                else result[reverse] = '0';
            }

            return new string(result);
        }

        public static bool areBitsEqual(BitArray one, BitArray two)
        {
            bool equal = true;

            for (int count = 0; count < BYTE_LENGTH && equal; count++)
            {
                if (!one[count] && two[count] 
                    || one[count] && !two[count]) equal = false;
            }

            return equal;
        }
    }

    public class Register
    {
        public event EnableBus EnableRegister;

        private BitArray contents = new BitArray(Globals.BYTE_LENGTH);

        public Register(EnableBus eventhandler) { EnableRegister += eventhandler; }
        public void set(BitArray contents) { this.contents = contents; }
        public BitArray enable()
        {
            EnableRegister?.Invoke();
            return contents;
        }
    }

    public class RAM
    {
        public event UpdateGPRContents SetRAM;
        public event UpdateGPRContents GetRAM;

        private Register[] m_locations = new Register[Globals.RAM_SIZE];

        public RAM(BitArray[] instructions, EnableBus eventhandler)
        {
            //fill RAM with instructions
            for (int count = 0; count < instructions.Length; count++)
            {
                m_locations[count] = new Register(eventhandler);
                m_locations[count].set(instructions[count]);
            }

            //fill RAM with empty registers
            for (int count = instructions.Length; count < Globals.RAM_SIZE; count++)
                m_locations[count] = new Register(eventhandler);
        }

        public void writeToLocation(BitArray address, BitArray data)
        {
            //get dec value of address
            byte[] addr = new byte[1];
            address.CopyTo(addr, 0);

            //access address and put the data
            m_locations[addr[0]].set(data);

            SetRAM?.Invoke(data, addr[0]);
        }

        public BitArray readFromLocation(BitArray address)
        {
            //get dec value of address
            byte[] addr = new byte[1];
            address.CopyTo(addr, 0);

            GetRAM?.Invoke(m_locations[addr[0]].enable(), addr[0]);
            
            //access address and get the data
            return m_locations[addr[0]].enable();
        }
    }

    public class MAR : Register
    {
        private RAM m_RAM;

        public MAR(BitArray[] instructions, UpdateGPRContents setram, UpdateGPRContents getram, 
            EnableBus enablebus) : base(enablebus)
        {
            m_RAM = new RAM(instructions, enablebus);
            m_RAM.SetRAM += setram;
            m_RAM.GetRAM += getram;
        }

        public void writeToMemory(BitArray data) { m_RAM.writeToLocation(enable(), data); }
        public BitArray readFromMemory() { return m_RAM.readFromLocation(enable()); }
    } 

    public class ALU
    {
        public event UpdateRegisterContents UpdateACC;

        public static class Flags { public const int COUT = 3, EQUAL = 2, A_LARGER = 1, ZERO = 0; }

        //change to private (find workaround)
        public bool bus1 = false;
        public bool[] m_flags = new bool[Globals.NO_OF_FLAGS];
        public Register m_temp, m_accumulator;

        public ALU(EnableBus eventhandler)
        {
            m_temp = new Register(eventhandler);
            m_accumulator = new Register(eventhandler);

            //set flags to false
            for (int count = 0; count < Globals.NO_OF_FLAGS; count++)
                m_flags[count] = false;
        }

        public void readOpcode(BitArray opcode, BitArray a)
        {
            BitArray result = new BitArray(Globals.BYTE_LENGTH);
            string binary = Globals.convertBitsToString(opcode);

            switch (binary)
            {
                case "1000":
                    m_accumulator.set(add(a));
                    break;

                case "1001":
                    m_accumulator.set(shiftRight(a));
                    break;

                case "1010":
                    m_accumulator.set(shiftLeft(a));
                    break;

                case "1011":
                    m_accumulator.set(inverse(a));
                    break;

                case "1100":
                    m_accumulator.set(and(a));
                    break;

                case "1101":
                    m_accumulator.set(or(a));
                    break;

                case "1110":
                    m_accumulator.set(xor(a));
                    break;

                case "1111":
                    compare(a);
                    break;

                default:
                    MessageBox.Show("Invalid opcode");
                    break;
            }

            UpdateACC?.Invoke(m_accumulator.enable());
            Thread.Sleep(Globals.clockspeed);
        }

        private BitArray add(BitArray input_a)
        {
            BitArray input_b, result = new BitArray(Globals.BYTE_LENGTH);

            if (bus1) input_b = new BitArray(new byte[] { 1 });
            else input_b = new BitArray(m_temp.enable());

            //examine each bit of both values and add them together
            bool carrybit = false;
            for (int count = 0; count < Globals.BYTE_LENGTH; count++)
            {
                if (carrybit)
                {
                    //0+0+1 = 1 no carry
                    if (input_a[count] == false && input_b[count] == false)
                    {
                        result[count] = true;
                        carrybit = false;
                    }

                    //1+1+1 = 1 c1
                    else if (input_a[count] == true && input_b[count] == true)
                        result[count] = true;

                    //1+0+1 = 0 c1
                    else result[count] = false;
                }

                else
                {
                    //0+0 = 0 no carry
                    if (input_a[count] == false && input_b[count] == false)
                        result[count] = false;

                    //1+1 = 0 c1
                    else if (input_a[count] == true && input_b[count] == true)
                    {
                        result[count] = false;
                        carrybit = true;
                    }

                    //0+1 = 1 no carry
                    else result[count] = true;
                }
            }

            return result;
        }

        private BitArray shiftRight(BitArray a)
        {
            BitArray result = new BitArray(Globals.BYTE_LENGTH);

            //if rightmost bit on its shifted out, enable COUT flag
            if (a[0]) m_flags[Flags.COUT] = true;

            //look ahead to next bit of a and copy into current bit of result
            for (int front = 1, back = 0; front < Globals.BYTE_LENGTH; front++, back++)
                result[back] = a[front];

            return result;
        }

        private BitArray shiftLeft(BitArray a)
        {
            BitArray result = new BitArray(Globals.BYTE_LENGTH);

            //if leftmost bit on its shifted out, enable COUT flag
            if (a[Globals.BYTE_LAST]) m_flags[Flags.COUT] = true;

            //copy current bit of a into next bit of result
            for (int front = 1, back = 0; front < Globals.BYTE_LENGTH; front++, back++)
                result[front] = a[back];

            return result;
        }

        private BitArray inverse(BitArray a) { return a.Not(); }

        private BitArray and(BitArray a) { return a.And(m_temp.enable()); }

        private BitArray or(BitArray a) { return a.Or(m_temp.enable()); }

        private BitArray xor(BitArray a) { return a.Xor(m_temp.enable()); }

        private void compare(BitArray a)
        {
            BitArray b = new BitArray(m_temp.enable());
            bool isequal = true, iszero = true;

            //count from MSB downward, first unequal bits breaks loop
            int count = Globals.BYTE_LAST;
            while (isequal && count >= 0)
            {
                //check if bit a is true and b is false
                if (a[count] && !b[count])
                {
                    isequal = false;
                    iszero = false;
                    m_flags[Flags.A_LARGER] = true;
                }

                //check if bit a is false and b is true
                else if (!a[count] && b[count])
                {
                    isequal = false;
                    iszero = false;
                }

                //check if both bits not false
                else if (a[count] && b[count])
                    iszero = false;

                count--;
            }

            if (isequal) m_flags[Flags.EQUAL] = true;
            if (iszero)  m_flags[Flags.ZERO] = true;
        }
    }

    public class ControlUnit
    {
        public event UpdateRegisterContents UpdateIAR;
        public event UpdateRegisterContents UpdateIR;
        public event UpdateRegisterContents UpdateMAR;
        public event UpdateRegisterContents UpdateTMP;
        public event UpdateGPRContents UpdateGPRs;

        public readonly BitArray lastaddress = new BitArray(Globals.BYTE_LENGTH);

        private byte m_rega, m_regb;
        private bool programend = false;

        private ALU m_ALU;
        private MAR m_MAR;
        private Register m_IAR, m_IR;
        private Register[] m_GPR = new Register[Globals.NO_OF_GPR];

        public ControlUnit(BitArray[] instructions, UpdateGPRContents setram, UpdateGPRContents getram, 
            UpdateRegisterContents ACCevent, EnableBus enablebus)
        {
            m_ALU = new ALU(enablebus);
            m_ALU.UpdateACC += ACCevent;

            m_MAR = new MAR(instructions, setram, getram, enablebus);
            m_IAR = new Register(enablebus);
            m_IR = new Register(enablebus);

            lastaddress = new BitArray(new byte[] { (byte)(instructions.Length - 1) });

            //initialise GPRs
            for (int count = 0; count < Globals.NO_OF_GPR; count++)
                m_GPR[count] = new Register(enablebus);
        }

        public void start()
        {
            while (!programend)
            {
                fetchInstruction();
                executeInstruction();
            }

            MessageBox.Show("End of program");
        }

        public void fetchInstruction()
        {
            accessMemory();
            incrementIAR();
            setInstructionRegister();
        }

        public void executeInstruction()
        {
            readInstructionRegister();
        }

        private void accessMemory()
        {
            m_MAR.set(m_IAR.enable());
            UpdateMAR?.Invoke(m_MAR.enable());
            Thread.Sleep(Globals.clockspeed);
        }

        private void incrementIAR()
        {
            if (Globals.areBitsEqual(m_IAR.enable(), lastaddress)) programend = true;

            //enable bus1 register
            m_ALU.bus1 = true;

            //create opcode for ADD
            BitArray opcode = new BitArray(Globals.OPCODE_LENGTH);
            opcode.Set(Globals.OPCODE_LAST, true);

            //method will add bus1 and the IAR
            m_ALU.readOpcode(opcode, m_IAR.enable());

            //turn off bus1
            m_ALU.bus1 = false;

            //set new value of IAR
            m_IAR.set(m_ALU.m_accumulator.enable());
            UpdateIAR?.Invoke(m_IAR.enable());
            Thread.Sleep(Globals.clockspeed);
        }

        private void setInstructionRegister()
        {
            m_IR.set(m_MAR.readFromMemory());
            UpdateIR?.Invoke(m_IR.enable());
            Thread.Sleep(Globals.clockspeed);
        }

        private void readInstructionRegister()
        {
            BitArray opcode = new BitArray(Globals.OPCODE_LENGTH),
                register_a = new BitArray(Globals.REGISTER_CODE_LENGTH), 
                register_b = new BitArray(Globals.REGISTER_CODE_LENGTH);

            //copy opcode bits from IR
            for (int count = Globals.OPCODE_LENGTH, opindex = 0; count < Globals.BYTE_LENGTH; count++, opindex++)
                opcode[opindex] = m_IR.enable()[count];

            //deduce the register codes for reg a and b from IR
            for (int count = 0, reg_a_index = Globals.REGISTER_CODE_LENGTH; 
                count < Globals.REGISTER_CODE_LENGTH; count++, reg_a_index++)
            {
                register_a[count] = m_IR.enable()[reg_a_index];
                register_b[count] = m_IR.enable()[count];
            }

            //copy the register codes back to a decimal value
            byte[] rega = new byte[1], regb = new byte[1];
            register_a.CopyTo(rega, 0);
            register_b.CopyTo(regb, 0);

            //note the selected registers for later
            m_rega = rega[0];
            m_regb = regb[0];

            //opcode leftmost bit is ALU bit
            if (opcode[3])
            {
                //should only set TMP if 2 input operation (needs workaround)
                
                //put contents of b in TMP, read and perform instruction, store answer in regb
                m_ALU.m_temp.set(m_GPR[m_regb].enable());
                UpdateTMP?.Invoke(m_ALU.m_temp.enable());
                Thread.Sleep(Globals.clockspeed);

                m_ALU.readOpcode(opcode, m_GPR[m_rega].enable());
                m_GPR[m_regb].set(m_ALU.m_accumulator.enable());
                UpdateGPRs?.Invoke(m_GPR[m_regb].enable(), m_regb);
                Thread.Sleep(Globals.clockspeed);
            }

            else
            {
                string binary = Globals.convertBitsToString(opcode);

                switch (binary)
                {
                    case "0000":
                        load();
                        break;

                    case "0001":
                        store();
                        break;

                    case "0010":
                        data();
                        break;

                    case "0011":
                        jumpRegister();
                        break;

                    case "0100":
                        jump();
                        break;

                    case "0101":
                        //combine reg a and b codes as they correspond to flags
                        bool[] cat = new bool[Globals.NO_OF_FLAGS];
                        register_a.CopyTo(cat, Globals.REGISTER_CODE_LENGTH);
                        register_b.CopyTo(cat, 0);

                        BitArray condition = new BitArray(cat);
                        jumpIf(condition);
                        break;

                    case "0110":
                        resetFlags();
                        break;

                    case "0111":
                        //i/o
                        break;
                }
            }
        }

        private void load()
        {
            //reg a selects address of data to load into reg b
            m_MAR.set(m_GPR[m_rega].enable());
            UpdateMAR?.Invoke(m_MAR.enable());
            Thread.Sleep(Globals.clockspeed);

            m_GPR[m_regb].set(m_MAR.readFromMemory());
            UpdateGPRs?.Invoke(m_GPR[m_regb].enable(), m_regb);
            Thread.Sleep(Globals.clockspeed);
        }

        private void store()
        {
            //reg a selects address to store contents of reg b
            m_MAR.set(m_GPR[m_rega].enable());
            UpdateMAR?.Invoke(m_MAR.enable());
            Thread.Sleep(Globals.clockspeed);

            m_MAR.writeToMemory(m_GPR[m_regb].enable());
        }

        private void data()
        {
            //instruction is data, store data in reg b and get next instruction addr
            m_MAR.set(m_IAR.enable());
            UpdateMAR?.Invoke(m_MAR.enable());
            Thread.Sleep(Globals.clockspeed);

            incrementIAR();

            m_GPR[m_regb].set(m_MAR.readFromMemory());
            UpdateGPRs?.Invoke(m_GPR[m_regb].enable(), m_regb);
            Thread.Sleep(Globals.clockspeed);
        }

        private void jumpRegister()
        {
            m_IAR.set(m_GPR[m_regb].enable());
            UpdateIAR?.Invoke(m_IAR.enable());
            Thread.Sleep(Globals.clockspeed);
        }

        private void jump()
        {
            //jump to address in instruction
            m_MAR.set(m_IAR.enable());
            UpdateMAR?.Invoke(m_MAR.enable());
            Thread.Sleep(Globals.clockspeed);

            m_IAR.set(m_MAR.readFromMemory());
            UpdateIAR?.Invoke(m_IAR.enable());
            Thread.Sleep(Globals.clockspeed);
        }

        private void jumpIf(BitArray condition)
        {
            if (condition.Length == Globals.NO_OF_FLAGS)
            {
                bool conditionmet = true;

                //check provided flags are all on
                for (int count = 0; count < Globals.NO_OF_FLAGS && conditionmet; count++)
                    if (condition[count] && !m_ALU.m_flags[count]) conditionmet = false;

                //flags on, jump to address
                if (conditionmet)
                {
                    m_MAR.set(m_IAR.enable());
                    UpdateMAR?.Invoke(m_MAR.enable());
                    Thread.Sleep(Globals.clockspeed);

                    m_IAR.set(m_MAR.readFromMemory());
                    UpdateIAR?.Invoke(m_IAR.enable());
                    Thread.Sleep(Globals.clockspeed);
                }

                //move to next instruction
                else incrementIAR();
            }

            else MessageBox.Show("Invalid condition");
        }

        private void resetFlags()
        {
            for (int count = 0; count < Globals.NO_OF_FLAGS; count++)
                m_ALU.m_flags[count] = false;
        }
    }
}