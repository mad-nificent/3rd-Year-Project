using System;
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
    public delegate void EnableRegister();
    public delegate void SetRegister(BitArray data);
    public delegate void SetEnableGPR(BitArray data, int index);    //this is used to set GUI contents of RAM and GPR, and also to show contents being read from RAM

    public partial class MainForm : Form
    {
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        private ControlUnit CU;

        //CPU components
        //--------------------------------
        private Panel pnlTMP = new Panel();
        private Label lblTMP = new Label();
        private Label lblTMPContents = new Label();

        private Panel pnlBUS1 = new Panel();
        private Label lblBUS1 = new Label();

        private Panel pnlAccumulator = new Panel();
        private Label lblAccumulator = new Label();
        private Label lblAccumulatorContents = new Label();

        private Panel pnlIAR = new Panel();
        private Label lblIAR = new Label();
        private Label lblIARContents = new Label();

        private Panel pnlIR = new Panel();
        private Label lblIR = new Label();
        private Label lblIRContents = new Label();

        private Panel pnlMAR = new Panel();
        private Label lblMAR = new Label();
        private Label lblMARContents = new Label();

        private Panel[] pnlGPR = new Panel[Globals.NO_OF_GPR];
        private Label[] lblGPR = new Label[Globals.NO_OF_GPR];
        private Label[] lblGPRContents = new Label[Globals.NO_OF_GPR];
        //--------------------------------

        public MainForm()
        {
            InitializeComponent();
            Paint += drawBus;           //bus will be reset on redraw

            drawCPU();

            //instructions
            BitArray loadintoreg1 = new BitArray(new bool[] { false, false, false, false, false, true,  false, false });
            BitArray inputa       = new BitArray(new bool[] { false, false, true,  false, false, false, false, false });
            BitArray jumptoaddr   = new BitArray(new bool[] { false, false, false, false, true,  true,  false, false });
            BitArray empty        = new BitArray(new bool[] { false, false, false, false, false, false, false, false });
            BitArray loadintoreg3 = new BitArray(new bool[] { false, true,  false, false, false, true,  false, false });
            BitArray inputc       = new BitArray(new bool[] { false, false, false, false, false, false, false, true  });

            //set instructions to load
            BitArray[] instructions = new BitArray[] { loadintoreg1, inputa, jumptoaddr, empty, loadintoreg3, inputc };

            //link GUI events to CPU registers
            CU = new ControlUnit(instructions, readRAMLocation, overwriteRAMLocation, 
                enableBus, updateIARContents, updateIRContents, updateMARContents,
                updateTMPContents, updateAccumulatorContents);

            timer.Interval = Globals.clockspeed;
            timer.Tick += new EventHandler(resetColours);
            timer.Enabled = true;
            timer.Start();

            CU.WriteToGPR += updateGPRContents;
        }

        private void drawCPU()
        {
            SuspendLayout();

            //TMP register
            //--------------------------------------------
            pnlTMP.SuspendLayout();
        
            pnlTMP.BorderStyle = BorderStyle.FixedSingle;
            pnlTMP.Location = new Point(80, 60);
            pnlTMP.Size = new Size(80, 50);
            Controls.Add(pnlTMP);

            lblTMP.Text = "TMP";
            lblTMP.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblTMP.TextAlign = ContentAlignment.MiddleCenter;
            lblTMP.Location = new Point(20, 4);
            lblTMP.Size = new Size(40, 20);
            pnlTMP.Controls.Add(lblTMP);

            lblTMPContents.Text = "00000000";
            lblTMPContents.TextAlign = ContentAlignment.MiddleCenter;
            lblTMPContents.BorderStyle = BorderStyle.FixedSingle;
            lblTMPContents.Location = new Point(10, 26);
            lblTMPContents.Size = new Size(60, 20);
            pnlTMP.Controls.Add(lblTMPContents);

            pnlTMP.ResumeLayout();
            pnlTMP.PerformLayout();
            //--------------------------------------------

            //BUS1 register
            //--------------------------------------------
            pnlBUS1.SuspendLayout();

            pnlBUS1.BorderStyle = BorderStyle.FixedSingle;
            pnlBUS1.Location = new Point(91, 130);
            pnlBUS1.Size = new Size(60, 25);
            Controls.Add(pnlBUS1);

            lblBUS1.Text = "BUS1";
            lblBUS1.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblBUS1.TextAlign = ContentAlignment.MiddleCenter;
            lblBUS1.Location = new Point(5, 1);
            lblBUS1.Size = new Size(50, 20);
            pnlBUS1.Controls.Add(lblBUS1);

            pnlBUS1.ResumeLayout();
            pnlBUS1.PerformLayout();
            //--------------------------------------------

            //Accumulator register
            //--------------------------------------------
            pnlAccumulator.SuspendLayout();

            pnlAccumulator.BorderStyle = BorderStyle.FixedSingle;
            pnlAccumulator.Location = new Point(50, 395);
            pnlAccumulator.Size = new Size(80, 50);
            Controls.Add(pnlAccumulator);

            lblAccumulator.Text = "ACC";
            lblAccumulator.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblAccumulator.TextAlign = ContentAlignment.MiddleCenter;
            lblAccumulator.Location = new Point(22, 4);
            lblAccumulator.Size = new Size(40, 20);
            pnlAccumulator.Controls.Add(lblAccumulator);

            lblAccumulatorContents.Text = "00000000";
            lblAccumulatorContents.TextAlign = ContentAlignment.MiddleCenter;
            lblAccumulatorContents.BorderStyle = BorderStyle.FixedSingle;
            lblAccumulatorContents.Location = new Point(10, 26);
            lblAccumulatorContents.Size = new Size(60, 20);
            pnlAccumulator.Controls.Add(lblAccumulatorContents);

            pnlAccumulator.ResumeLayout();
            pnlAccumulator.PerformLayout();
            //--------------------------------------------

            //IAR register
            //--------------------------------------------
            pnlIAR.SuspendLayout();

            pnlIAR.BorderStyle = BorderStyle.FixedSingle;
            pnlIAR.Location = new Point(350, 395);
            pnlIAR.Size = new Size(80, 50);
            Controls.Add(pnlIAR);

            lblIAR.Text = "IAR";
            lblIAR.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblIAR.TextAlign = ContentAlignment.MiddleCenter;
            lblIAR.Location = new Point(22, 4);
            lblIAR.Size = new Size(40, 20);
            pnlIAR.Controls.Add(lblIAR);

            lblIARContents.Text = "00000000";
            lblIARContents.TextAlign = ContentAlignment.MiddleCenter;
            lblIARContents.BorderStyle = BorderStyle.FixedSingle;
            lblIARContents.Location = new Point(10, 26);
            lblIARContents.Size = new Size(60, 20);
            pnlIAR.Controls.Add(lblIARContents);

            pnlIAR.ResumeLayout();
            pnlIAR.PerformLayout();
            //--------------------------------------------

            //IR register
            //--------------------------------------------
            pnlIR.SuspendLayout();

            pnlIR.BorderStyle = BorderStyle.FixedSingle;
            pnlIR.Location = new Point(470, 395);
            pnlIR.Size = new Size(80, 50);
            Controls.Add(pnlIR);

            lblIR.Text = "IR";
            lblIR.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblIR.TextAlign = ContentAlignment.MiddleCenter;
            lblIR.Location = new Point(22, 4);
            lblIR.Size = new Size(40, 20);
            pnlIR.Controls.Add(lblIR);

            lblIRContents.Text = "00000000";
            lblIRContents.TextAlign = ContentAlignment.MiddleCenter;
            lblIRContents.BorderStyle = BorderStyle.FixedSingle;
            lblIRContents.Location = new Point(10, 26);
            lblIRContents.Size = new Size(60, 20);
            pnlIR.Controls.Add(lblIRContents);

            pnlIR.ResumeLayout();
            pnlIR.PerformLayout();
            //--------------------------------------------

            //MAR register
            //--------------------------------------------
            pnlMAR.SuspendLayout();

            pnlMAR.BorderStyle = BorderStyle.FixedSingle;
            pnlMAR.Location = new Point(415, 10);
            pnlMAR.Size = new Size(80, 50);
            Controls.Add(pnlMAR);

            lblMAR.Text = "MAR";
            lblMAR.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            lblMAR.TextAlign = ContentAlignment.MiddleCenter;
            lblMAR.Location = new Point(20, 4);
            lblMAR.Size = new Size(41, 20);
            pnlMAR.Controls.Add(lblMAR);

            lblMARContents.Text = "00000000";
            lblMARContents.TextAlign = ContentAlignment.MiddleCenter;
            lblMARContents.BorderStyle = BorderStyle.FixedSingle;
            lblMARContents.Location = new Point(10, 26);
            lblMARContents.Size = new Size(60, 20);
            pnlMAR.Controls.Add(lblMARContents);

            pnlMAR.ResumeLayout();
            pnlMAR.PerformLayout();
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
                if (Globals.clockspeed == 500)
                {
                    Globals.clockspeed -= 499;
                    lblClock.Text = "Real-time";
                }

                else
                {
                    Globals.clockspeed -= 500;
                    lblClock.Text = Globals.clockspeed.ToString();
                }

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
            Thread CPUThread = new Thread(new ThreadStart(CU.start));
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

            lblIRContents.ForeColor = Color.Black;
            lblIARContents.ForeColor = Color.Black;
            lblMARContents.ForeColor = Color.Black;
            lblRAMContents.ForeColor = Color.Black;
            lblTMPContents.ForeColor = Color.Black;
            lblAccumulatorContents.ForeColor = Color.Black;

            for (int count = 0; count < Globals.NO_OF_GPR; count++)
            {
                lblGPRContents[count].ForeColor = Color.Black;
            }

            ResumeLayout();
            drawBus();
        }

        //CPU state changes
        //-------------------------------------
        public void updateIARContents(BitArray data)
        {
            if (InvokeRequired) Invoke(new SetRegister(updateIARContents), new object[] { data });
            else
            {
                lblIARContents.Text = Globals.convertBitsToString(data);
                lblIARContents.ForeColor = Color.Red;
            }
        }

        public void updateIRContents(BitArray data)
        {
            if (InvokeRequired) Invoke(new SetRegister(updateIRContents), new object[] { data });
            else
            {
                lblIRContents.Text = Globals.convertBitsToString(data);
                lblIRContents.ForeColor = Color.Red;
            }
        }

        public void updateMARContents(BitArray data)
        {
            if (InvokeRequired) Invoke(new SetRegister(updateMARContents), new object[] { data });
            else
            {
                lblMARContents.Text = Globals.convertBitsToString(data);
                lblMARContents.ForeColor = Color.Red;
            }
        }

        public void updateTMPContents(BitArray data)
        {
            if (InvokeRequired) Invoke(new SetRegister(updateTMPContents), new object[] { data });
            else
            {
                lblTMPContents.Text = Globals.convertBitsToString(data);
                lblTMPContents.ForeColor = Color.Red;
            }
        }

        public void updateAccumulatorContents(BitArray data)
        {
            if (InvokeRequired) Invoke(new SetRegister(updateAccumulatorContents), new object[] { data });
            else
            {
                lblAccumulatorContents.Text = Globals.convertBitsToString(data);
                lblAccumulatorContents.ForeColor = Color.Red;
            }
        }

        public void readRAMLocation(BitArray data, int address)
        {
            if (InvokeRequired) Invoke(new SetEnableGPR(readRAMLocation), new object[] { data, address });
            else
            {
                lblRAMContents.Text = Globals.convertBitsToString(data);
                lblRAMContents.ForeColor = Color.Blue;
                lblAddr.Text = "Addr " + address.ToString();
            }
        }

        public void overwriteRAMLocation(BitArray data, int address)
        {
            if (InvokeRequired) Invoke(new SetEnableGPR(overwriteRAMLocation), new object[] { data, address });
            else
            {
                lblRAMContents.Text = Globals.convertBitsToString(data);
                lblRAMContents.ForeColor = Color.Red;
                lblAddr.Text = "Addr " + address.ToString();
            }
        }

        public void updateGPRContents(BitArray data, int GPRindex)
        {
            if (InvokeRequired) Invoke(new SetEnableGPR(updateGPRContents), new object[] { data, GPRindex });
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
            if (InvokeRequired) Invoke(new EnableRegister(enableBus));
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
        //-------------------------------------
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
        public event EnableRegister OutputToBus;
        public event SetRegister OverwriteContents;

        private BitArray contents = new BitArray(Globals.BYTE_LENGTH);

        //used by GPRs and RAM, they are set with an index so GUI can show address/select correct GPU
        public Register(EnableRegister outputToBus) { OutputToBus += outputToBus; }

        //used by SPRs
        public Register(EnableRegister outputToBus, SetRegister overwriteContents)
        {
            OutputToBus += outputToBus;
            OverwriteContents += overwriteContents;
        }

        //write data to the register
        public void overwriteContents(BitArray contents)
        {
            OverwriteContents?.Invoke(contents);
            Thread.Sleep(Globals.clockspeed);
            this.contents = contents;
        }

        //read data stored in the register
        public BitArray readContents()
        {
            OutputToBus?.Invoke();
            Thread.Sleep(Globals.clockspeed);
            return contents;
        }
    }

    public class RAM
    {
        public event SetEnableGPR ReadLocation;
        public event SetEnableGPR OverwriteLocation;

        private Register[] locations = new Register[Globals.RAM_SIZE];

        public RAM(BitArray[] instructions, EnableRegister outputToBus, SetEnableGPR readLocation, SetEnableGPR overwriteLocation)
        {
            ReadLocation  += readLocation;
            OverwriteLocation += overwriteLocation;

            //fill RAM with instructions
            for (int count = 0; count < instructions.Length; count++)
            {
                locations[count] = new Register(outputToBus);
                locations[count].overwriteContents(instructions[count]);
            }

            //fill remaining RAM with empty registers
            for (int count = instructions.Length; count < Globals.RAM_SIZE; count++)
                locations[count] = new Register(outputToBus);
        }

        public BitArray readFromLocation(BitArray address)
        {
            //convert binary address to decimal, used to index RAM
            byte[] addr = new byte[1];
            address.CopyTo(addr, 0);

            //access address and get the data
            ReadLocation?.Invoke(locations[addr[0]].readContents(), addr[0]);
            Thread.Sleep(Globals.clockspeed);
            return locations[addr[0]].readContents();
        }

        public void writeToLocation(BitArray address, BitArray data)
        {
            //convert binary address to decimal, used to index RAM
            byte[] addr = new byte[1];
            address.CopyTo(addr, 0);

            //access address and put the data
            OverwriteLocation?.Invoke(data, addr[0]);
            Thread.Sleep(Globals.clockspeed);
            locations[addr[0]].overwriteContents(data);
        }
    }

    public class MAR : Register
    {
        private RAM RAM;

        public MAR(BitArray[] instructions, SetEnableGPR readFromRAM, SetEnableGPR writeToRAM,                  //RAM parameters
            EnableRegister outputToBus, SetRegister overwriteContents) : base(outputToBus, overwriteContents)   //MAR parameters
        { RAM = new RAM(instructions, outputToBus, readFromRAM, writeToRAM); }

        //pass address in MAR to RAM to access location and read or write to it
        public void writeToMemory(BitArray data) { RAM.writeToLocation(this.readContents(), data); }
        public BitArray readFromMemory() { return RAM.readFromLocation(this.readContents()); }
    } 

    public class ALU
    {
        public static class Flags { public const int COUT = 3, EQUAL = 2, A_LARGER = 1, ZERO = 0; }

        //change to private (find workaround)
        public bool bus1 = false;

        //flag register and SPRs used by ALU
        public bool[] flags = new bool[Globals.NO_OF_FLAGS];
        public Register TMP, accumulator;

        public ALU(EnableRegister outputToBus, SetRegister overwriteTMP, SetRegister overwriteAccumulator)
        {
            TMP = new Register(outputToBus, overwriteTMP);
            accumulator = new Register(outputToBus, overwriteAccumulator);

            //set flags to false
            for (int count = 0; count < Globals.NO_OF_FLAGS; count++)
                flags[count] = false;
        }

        //decode instruction to execute
        public void readOpcode(BitArray opcode, BitArray primaryInput)
        {
            string opcodeAsString = Globals.convertBitsToString(opcode);

            switch (opcodeAsString)
            {
                //ADD
                case "1000":
                    accumulator.overwriteContents(add(primaryInput));
                    break;

                //RSHIFT
                case "1001":
                    accumulator.overwriteContents(shiftRight(primaryInput));
                    break;

                //LSHIFT
                case "1010":
                    accumulator.overwriteContents(shiftLeft(primaryInput));
                    break;

                //NOT
                case "1011":
                    accumulator.overwriteContents(inverse(primaryInput));
                    break;
                
                //AND
                case "1100":
                    accumulator.overwriteContents(and(primaryInput));
                    break;
                
                //OR
                case "1101":
                    accumulator.overwriteContents(or(primaryInput));
                    break;
                
                //XOR
                case "1110":
                    accumulator.overwriteContents(xor(primaryInput));
                    break;
                
                //compare
                case "1111":
                    compare(primaryInput);
                    break;

                //invalid opcode
                default:
                    MessageBox.Show("Invalid opcode");
                    break;
            }
        }

        private BitArray add(BitArray firstNumber)
        {
            BitArray secondNumber, result = new BitArray(Globals.BYTE_LENGTH);

            //examine if request was an increment or add 2 values
            if (bus1) secondNumber = new BitArray(new byte[] { 1 });
            else secondNumber = new BitArray(TMP.readContents());

            //examine each bit of both values and add them together
            bool carrybit = false;
            for (int count = 0; count < Globals.BYTE_LENGTH; count++)
            {
                if (carrybit)
                {
                    //0+0+1 = 1 no carry
                    if (firstNumber[count] == false && secondNumber[count] == false)
                    {
                        result[count] = true;
                        carrybit = false;
                    }

                    //1+1+1 = 1 c1
                    else if (firstNumber[count] == true && secondNumber[count] == true)
                        result[count] = true;

                    //1+0+1 = 0 c1
                    else result[count] = false;
                }

                else
                {
                    //0+0 = 0 no carry
                    if (firstNumber[count] == false && secondNumber[count] == false)
                        result[count] = false;

                    //1+1 = 0 c1
                    else if (firstNumber[count] == true && secondNumber[count] == true)
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

        private BitArray shiftRight(BitArray data)
        {
            BitArray result = new BitArray(Globals.BYTE_LENGTH);

            //if rightmost bit on its shifted out, enable COUT flag
            if (data[0]) flags[Flags.COUT] = true;

            //look ahead to next bit of a and copy into current bit of result
            for (int front = 1, back = 0; front < Globals.BYTE_LENGTH; front++, back++)
                result[back] = data[front];

            return result;
        }

        private BitArray shiftLeft(BitArray data)
        {
            BitArray result = new BitArray(Globals.BYTE_LENGTH);

            //if leftmost bit on its shifted out, enable COUT flag
            if (data[Globals.BYTE_LAST]) flags[Flags.COUT] = true;

            //copy current bit of a into next bit of result
            for (int front = 1, back = 0; front < Globals.BYTE_LENGTH; front++, back++)
                result[front] = data[back];

            return result;
        }

        private BitArray inverse(BitArray data) { return data.Not(); }

        private BitArray and(BitArray data) { return data.And(TMP.readContents()); }

        private BitArray or(BitArray data) { return data.Or(TMP.readContents()); }

        private BitArray xor(BitArray data) { return data.Xor(TMP.readContents()); }

        private void compare(BitArray firstNumber)
        {
            BitArray secondNumber = new BitArray(TMP.readContents());
            bool isequal = true, iszero = true;

            //count from MSB downward, first unequal bits breaks loop
            int count = Globals.BYTE_LAST;
            while (isequal && count >= 0)
            {
                //check if bit a is true and b is false
                if (firstNumber[count] && !secondNumber[count])
                {
                    isequal = false;
                    iszero = false;
                    flags[Flags.A_LARGER] = true;
                }

                //check if bit a is false and b is true
                else if (!firstNumber[count] && secondNumber[count])
                {
                    isequal = false;
                    iszero = false;
                }

                //check if both bits not false
                else if (firstNumber[count] && secondNumber[count])
                    iszero = false;

                count--;
            }

            if (isequal) flags[Flags.EQUAL] = true;
            if (iszero)  flags[Flags.ZERO] = true;
        }
    }

    public class ControlUnit
    {
        public event SetEnableGPR WriteToGPR;

        //tracks the address of the last instruction to execute
        public readonly BitArray lastAddress = new BitArray(Globals.BYTE_LENGTH);

        private bool programEnd = false;    //condition to stop execution
        private byte registerA, registerB;  //temporary storage for register addresses in use

        //components and registers
        private ALU ALU;
        private MAR MAR;
        private Register IAR, IR;
        private Register[] GPR = new Register[Globals.NO_OF_GPR];

        public ControlUnit(BitArray[] instructions, SetEnableGPR readFromRAM, SetEnableGPR writeToRAM, 
            EnableRegister outputToBus, SetRegister overwriteIAR, SetRegister overwriteIR, SetRegister overwriteMAR, 
            SetRegister overwriteTMP, SetRegister overwriteAccumulator)
        {
            //intialising each component and register and linking GUI events to them
            ALU = new ALU(outputToBus, overwriteTMP, overwriteAccumulator);
            MAR = new MAR(instructions, readFromRAM, writeToRAM, outputToBus, overwriteMAR);
            IAR = new Register(outputToBus, overwriteIAR);
            IR = new Register(outputToBus, overwriteIR);

            lastAddress = new BitArray(new byte[] { (byte)(instructions.Length - 1) });

            //initialise GPRs
            for (int count = 0; count < Globals.NO_OF_GPR; count++)
                GPR[count] = new Register(outputToBus);
        }

        public void start()
        {
            //restart program if start clicked again
            if (programEnd)
            {
                programEnd = false;

                //reset values of registers
                MAR.overwriteContents(new BitArray(new byte[] { 0 }));
                IAR.overwriteContents(new BitArray(new byte[] { 0 }));
                IR.overwriteContents(new BitArray(new byte[] { 0 }));

                ALU.accumulator.overwriteContents(new BitArray(new byte[] { 0 }));
                ALU.TMP.overwriteContents(new BitArray(new byte[] { 0 }));

                for (int count = 0; count < Globals.NO_OF_GPR; count++)
                    GPR[count].overwriteContents(new BitArray(new byte[] { 0 }));
            }

            while (!programEnd)
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

        public void executeInstruction() { readInstructionRegister(); }

        private void accessMemory() { MAR.overwriteContents(IAR.readContents()); }

        private void incrementIAR()
        {
            if (Globals.areBitsEqual(IAR.readContents(), lastAddress)) programEnd = true;

            //enable bus1 register
            ALU.bus1 = true;

            //create opcode for ADD
            BitArray opcode = new BitArray(Globals.OPCODE_LENGTH);
            opcode.Set(Globals.OPCODE_LAST, true);

            //method will add bus1 and the IAR
            ALU.readOpcode(opcode, IAR.readContents());

            //turn off bus1
            ALU.bus1 = false;

            //set new value of IAR
            IAR.overwriteContents(ALU.accumulator.readContents());
        }

        private void setInstructionRegister() { IR.overwriteContents(MAR.readFromMemory()); }

        private void readInstructionRegister()
        {
            BitArray opcode = new BitArray(Globals.OPCODE_LENGTH),
                registerA = new BitArray(Globals.REGISTER_CODE_LENGTH), 
                registerB = new BitArray(Globals.REGISTER_CODE_LENGTH);

            //copy opcode bits from IR
            for (int count = Globals.OPCODE_LENGTH, opindex = 0; count < Globals.BYTE_LENGTH; count++, opindex++)
                opcode[opindex] = IR.readContents()[count];

            //deduce the register addresses from IR
            for (int registerBIndex = 0, registerAIndex = Globals.REGISTER_CODE_LENGTH;     //B is at the start of the code, A is at the index after the end of B
                registerBIndex < Globals.REGISTER_CODE_LENGTH; 
                registerBIndex++, registerAIndex++)
            {
                registerA[registerBIndex] = IR.readContents()[registerAIndex];
                registerB[registerBIndex] = IR.readContents()[registerBIndex];
            }

            //copy the register addresses back to a decimal value
            byte[] registerADecimal = new byte[1], registerBDecimal = new byte[1];
            registerA.CopyTo(registerADecimal, 0);
            registerB.CopyTo(registerBDecimal, 0);

            //note the selected registers for later
            this.registerA = registerADecimal[0];
            this.registerB = registerBDecimal[0];

            //opcode leftmost bit is ALU bit
            if (opcode[3])
            {
                //should only set TMP if 2 input operation (needs workaround)

                //put contents of b in TMP, read and perform instruction, store answer in regb
                ALU.TMP.overwriteContents(GPR[this.registerB].readContents());

                //run the ALU instruction
                ALU.readOpcode(opcode, GPR[this.registerA].readContents());

                //SHOULD ONLY SET REG B IF NOT COMPARISON (NEEDS FIX)
                GPR[this.registerB].overwriteContents(ALU.accumulator.readContents());
                WriteToGPR?.Invoke(GPR[this.registerB].readContents(), this.registerB);
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
                        registerA.CopyTo(cat, Globals.REGISTER_CODE_LENGTH);
                        registerB.CopyTo(cat, 0);

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

        //copy data from RAM to register (address specified)
        private void load()
        {
            //load address in register A to MAR
            MAR.overwriteContents(GPR[registerA].readContents());

            //save the data in RAM to register B
            GPR[registerB].overwriteContents(MAR.readFromMemory());
            WriteToGPR?.Invoke(GPR[registerB].readContents(), registerB);
            Thread.Sleep(Globals.clockspeed);
        }

        //copy data from register to RAM
        private void store()
        {
            //load address in register A to MAR
            MAR.overwriteContents(GPR[registerA].readContents());

            //save the data in register B to RAM
            MAR.writeToMemory(GPR[registerB].readContents());
        }

        //copy data from RAM to register (next RAM location)
        private void data()
        {
            //prepares next RAM location for access
            MAR.overwriteContents(IAR.readContents());

            //IAR steps over data to next instruction
            incrementIAR();

            //copy data to register B
            GPR[registerB].overwriteContents(MAR.readFromMemory());
            WriteToGPR?.Invoke(GPR[registerB].readContents(), registerB);
            Thread.Sleep(Globals.clockspeed);
        }

        //jump to address stored in register B
        private void jumpRegister() { IAR.overwriteContents(GPR[registerB].readContents()); }

        //jump to address thats in the next RAM location
        private void jump()
        {
            MAR.overwriteContents(IAR.readContents());
            IAR.overwriteContents(MAR.readFromMemory());
        }

        //jumps to an address in next RAM location if condition is met
        private void jumpIf(BitArray condition)
        {
            if (condition.Length == Globals.NO_OF_FLAGS)
            {
                bool conditionmet = true;

                //check provided flags are all on
                for (int count = 0; count < Globals.NO_OF_FLAGS && conditionmet; count++)
                    if (condition[count] && !ALU.flags[count]) conditionmet = false;

                //flags on, jump to address
                if (conditionmet)
                {
                    MAR.overwriteContents(IAR.readContents());
                    IAR.overwriteContents(MAR.readFromMemory());
                }

                //skip jump address
                else incrementIAR();
            }

            else MessageBox.Show("Invalid condition");
        }

        private void resetFlags() { for (int count = 0; count < Globals.NO_OF_FLAGS; count++) ALU.flags[count] = false; }
    }
}