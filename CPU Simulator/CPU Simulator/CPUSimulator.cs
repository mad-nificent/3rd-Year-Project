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
    //reflect contents being written to or read from a register
    public delegate void SetEnableRegister(BitArray data, bool read);
    
    //same principle, but uses index of GPR or RAM address
    public delegate void SetEnableGPR(BitArray data, int index, bool read);

    //reset item colours after a period of time
    public delegate void RedrawGUI();

    public partial class MainForm : Form
    {
        //PRIVATE
        //------------------------------------------------------------
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

        private Panel[] pnlGPR = new Panel[Globals.GPR_COUNT];
        private Label[] lblGPR = new Label[Globals.GPR_COUNT];
        private Label[] lblGPRContents = new Label[Globals.GPR_COUNT];
        //--------------------------------

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

            for (int count = 0; count < Globals.GPR_COUNT; count++)
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

            ResumeLayout();
            PerformLayout();

            drawBus();
        }

        private void drawBus()
        {
            SuspendLayout();

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

            ResumeLayout();
            PerformLayout();
        }

        //GUI controls
        //-------------------------------------
        private void btnDecrease_Click(object sender, EventArgs e)
        {
            if (Globals.CLOCK_SPEED != 0)
            {
                Globals.CLOCK_SPEED -= 1000;

                if (Globals.CLOCK_SPEED == 0) lblClock.Text = "Real-time";
                else lblClock.Text = Globals.CLOCK_SPEED.ToString();
            }
        }

        private void btnIncrease_Click(object sender, EventArgs e)
        {
            Globals.CLOCK_SPEED += 1000;
            lblClock.Text = Globals.CLOCK_SPEED.ToString();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Thread CPUThread = new Thread(new ThreadStart(CU.start));
            CPUThread.Start();
        }
        //-------------------------------------

        //------------------------------------------------------------

        //PUBLIC
        //------------------------------------------------------------
        public MainForm()
        {
            InitializeComponent();

            drawCPU();

            //                                               register B   |  register A   |           opcode
            //                                                2      1    |   2      1    | last                 first
            //BitArray load     = new BitArray(new bool[] { false, false, | false, false, | false, false, false, false });
            //BitArray store    = new BitArray(new bool[] { false, false, | false, false, | true,  false, false, false });
            //BitArray data     = new BitArray(new bool[] { false, false, | false, false, | false, true,  false, false });
            //BitArray jumpRG   = new BitArray(new bool[] { false, false, | false, false, | true,  true,  false, false });
            //BitArray jump     = new BitArray(new bool[] { false, false, | false, false, | false, false, true,  false });
            //BitArray jumpIf   = new BitArray(new bool[] { false, false, | false, false, | true,  false, true,  false });
            //BitArray resetFlg = new BitArray(new bool[] { false, false, | false, false, | false, true,  true,  false });
            //BitArray IO       = new BitArray(new bool[] { false, false, | false, false, | true,  true,  true,  false });
            //------------------------------------------------------------|---------------|-------------------------------
            //BitArray add      = new BitArray(new bool[] { false, false, | false, false, | false, false, false, true });
            //BitArray rShift   = new BitArray(new bool[] { false, false, | false, false, | true,  false, false, true });
            //BitArray lShift   = new BitArray(new bool[] { false, false, | false, false, | false, true,  false, true });
            //BitArray not      = new BitArray(new bool[] { false, false, | false, false, | true,  true,  false, true });
            //BitArray and      = new BitArray(new bool[] { false, false, | false, false, | false, false, true,  true });
            //BitArray or       = new BitArray(new bool[] { false, false, | false, false, | true,  false, true,  true });
            //BitArray xor      = new BitArray(new bool[] { false, false, | false, false, | false, true,  true,  true });
            //BitArray compare  = new BitArray(new bool[] { false, false, | false, false, | true,  true,  true,  true });


            //instructions
            BitArray data       = new BitArray(new bool[] { false, true,    false, false,   false,  true, false, false });
            //------------------------------------------------------------------------------------------------------------
            BitArray value      = new BitArray(new bool[] { true,  true,    false, false,   false, false, false, false });
            //------------------------------------------------------------------------------------------------------------

            BitArray dataB      = new BitArray(new bool[] { true, false,    false, false,   false,  true, false, false });
            //------------------------------------------------------------------------------------------------------------
            BitArray valueB     = new BitArray(new bool[] { false, true,    false, false,   false, false, false, false });
            //------------------------------------------------------------------------------------------------------------

            BitArray compare    = new BitArray(new bool[] { true,  false,   false,  true,    true,  true,  true,  true });
            BitArray jumpIf     = new BitArray(new bool[] { false,  true,   false,  true,    true, false,  true, false });
            BitArray nextAddr   = new BitArray(new bool[] { false, false,   false,  true,   false, false, false, false });
            BitArray skip       = new BitArray(new bool[] { false,  true,   false, false,   false,  true, false, false });

            BitArray dataC      = new BitArray(new bool[] {  true,  true,   false, false,   false,  true, false, false });
            //------------------------------------------------------------------------------------------------------------
            BitArray valueC     = new BitArray(new bool[] { true,  false,   false, false,   false, false, false, false });
            //------------------------------------------------------------------------------------------------------------

            //set instructions to load
            BitArray[] instructions = new BitArray[] { data, value, dataB, valueB, compare, jumpIf, nextAddr, skip, dataC, valueC };

            //link GUI events to CPU registers
            CU = new ControlUnit(instructions, accessRAMLocation,                                   
                updateIARContents, updateIRContents, updateMARContents,                             
                updateTMPContents, updateAccumulatorContents, 
                accessGPRContents, resetColours);
        }

        //CPU state changes
        //-------------------------------------
        public void updateIARContents(BitArray data, bool read)
        {
            if (InvokeRequired) Invoke(new SetEnableRegister(updateIARContents), new object[] { data, read });
            else
            {
                if (read)
                {
                    enableBus();
                    lblIARContents.ForeColor = Color.Blue;
                }

                else if (data != null)
                {
                    lblIARContents.Text = Globals.convertBitsToString(data);
                    lblIARContents.ForeColor = Color.Red;
                }
            }
        }

        public void updateIRContents(BitArray data, bool read)
        {
            if (InvokeRequired) Invoke(new SetEnableRegister(updateIRContents), new object[] { data, read });
            else
            {
                if (read)
                {
                    enableBus();
                    lblIRContents.ForeColor = Color.Blue;
                }

                else if (data != null)
                {
                    lblIRContents.Text = Globals.convertBitsToString(data);
                    lblIRContents.ForeColor = Color.Red;
                }
            }
        }

        public void updateMARContents(BitArray data, bool read)
        {
            if (InvokeRequired) Invoke(new SetEnableRegister(updateMARContents), new object[] { data, read });
            else
            {
                if (read)
                {
                    enableBus();
                    lblMARContents.ForeColor = Color.Blue;
                }

                else if (data != null)
                {
                    lblMARContents.Text = Globals.convertBitsToString(data);
                    lblMARContents.ForeColor = Color.Red;
                }
            }
        }

        public void updateTMPContents(BitArray data, bool read)
        {
            if (InvokeRequired) Invoke(new SetEnableRegister(updateTMPContents), new object[] { data, read });
            else
            {
                if (read)
                {
                    enableBus();
                    lblTMPContents.ForeColor = Color.Blue;
                }

                else if (data != null)
                {
                    lblTMPContents.Text = Globals.convertBitsToString(data);
                    lblTMPContents.ForeColor = Color.Red;
                }
            }
        }

        public void updateAccumulatorContents(BitArray data, bool read)
        {
            if (InvokeRequired) Invoke(new SetEnableRegister(updateAccumulatorContents), new object[] { data, read });
            else
            {
                if (read)
                {
                    enableBus();
                    lblAccumulatorContents.ForeColor = Color.Blue;
                }

                else if (data != null)
                {
                    lblAccumulatorContents.Text = Globals.convertBitsToString(data);
                    lblAccumulatorContents.ForeColor = Color.Red;
                }
            }
        }

        public void accessRAMLocation(BitArray data, int address, bool read)
        {
            if (InvokeRequired) Invoke(new SetEnableGPR(accessRAMLocation), new object[] { data, address, read });
            else
            {
                if (address < Globals.RAM_SIZE && data != null)
                {
                    lblRAMContents.Text = Globals.convertBitsToString(data);
                    lblAddr.Text = "Addr " + address.ToString();

                    if (read)
                    {
                        lblRAMContents.ForeColor = Color.Blue;
                        enableBus();
                    }

                    else
                    {
                        lblRAMContents.ForeColor = Color.Red;
                    }
                }
            }
        }

        public void accessGPRContents(BitArray data, int GPRindex, bool read)
        {
            if (InvokeRequired) Invoke(new SetEnableGPR(accessGPRContents), new object[] { data, GPRindex, read });
            else
            {
                if (GPRindex < Globals.GPR_COUNT)
                {
                    if (read)
                    {
                        lblGPRContents[GPRindex].ForeColor = Color.Blue;
                        enableBus();
                    }

                    else if (data != null)
                    {
                        lblGPRContents[GPRindex].Text = Globals.convertBitsToString(data);
                        lblGPRContents[GPRindex].ForeColor = Color.Red;
                    }
                }
            }
        }

        public void enableBus()
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

        public void resetColours()
        {
            if (InvokeRequired) Invoke(new RedrawGUI(resetColours));
            else
            {
                SuspendLayout();

                lblIRContents.ForeColor = Color.Black;
                lblIARContents.ForeColor = Color.Black;
                lblMARContents.ForeColor = Color.Black;
                lblRAMContents.ForeColor = Color.Black;
                lblTMPContents.ForeColor = Color.Black;
                lblAccumulatorContents.ForeColor = Color.Black;

                for (int count = 0; count < Globals.GPR_COUNT; count++)
                {
                    lblGPRContents[count].ForeColor = Color.Black;
                }

                ResumeLayout();
                drawBus();
            }
        }
        //-------------------------------------

        //------------------------------------------------------------
    }

    public static class Globals
    {
        public static int CLOCK_SPEED = 0;
        public const int  FLAG_COUNT  = 4;
        
        //supported memory space
        public const int RAM_SIZE   = 256;
        public const int GPR_COUNT  = 4;

        //supported length of data/instruction in bits
        public const int DATA_INSTRUCTION_SIZE      = 8;
        public const int FIRST_DATA_INSTRUCTION_BIT = DATA_INSTRUCTION_SIZE - 1;

        //supported length of an opcode in bits
        public const int OPCODE_SIZE      = DATA_INSTRUCTION_SIZE / 2;
        public const int FIRST_OPCODE_BIT = OPCODE_SIZE - 1;
        public const int ALU_OPCODE       = FIRST_OPCODE_BIT;

        //supported length of GPR address in bits
        public const int REGISTER_ADDRESS_SIZE = 2;

        //register access modes
        public const bool REGISTER_READ  = true;
        public const bool REGISTER_WRITE = false;

        public static BitArray reverseBitArray(BitArray data)
        {
            BitArray reversedData = new BitArray(data.Length);

            for (int reverseCount = data.Length - 1, count = 0; reverseCount >= 0 && count < data.Length; reverseCount--, count++)
                reversedData[count] = data[reverseCount];

            return reversedData;
        }

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

            for (int count = 0; count < DATA_INSTRUCTION_SIZE && equal; count++)
            {
                if (!one[count] && two[count] 
                    || one[count] && !two[count]) equal = false;
            }

            return equal;
        }
    }

    public class Register
    {
        //PRIVATE
        //------------------------------------------------------------
        private BitArray contents = new BitArray(Globals.DATA_INSTRUCTION_SIZE);
        //------------------------------------------------------------

        //PUBLIC
        //------------------------------------------------------------
        public event SetEnableRegister AccessContents;

        public Register() {}

        public Register(SetEnableRegister accessContents) { AccessContents += accessContents; }

        public void overwriteContents(BitArray contents)
        {
            //invoke event in write state
            AccessContents?.Invoke(contents, Globals.REGISTER_WRITE);
            if (AccessContents != null) Thread.Sleep(Globals.CLOCK_SPEED);
            this.contents = contents;
        }

        public BitArray readContents()
        {
            //invoke event in read state
            AccessContents?.Invoke(null, Globals.REGISTER_READ);
            return contents;
        }

        //access contents of a register without running GUI events
        public BitArray getContents() { return contents; }
        //------------------------------------------------------------
    }

    public class RAM
    {
        //PRIVATE
        //------------------------------------------------------------
        private Register[] locations = new Register[Globals.RAM_SIZE];
        //------------------------------------------------------------

        //PUBLIC
        //------------------------------------------------------------
        public event SetEnableGPR AccessLocation;

        public RAM(BitArray[] instructions, SetEnableGPR accessLocation)
        {
            AccessLocation += accessLocation;

            //load instructions into memory
            for (int count = 0; count < instructions.Length; count++)
            {
                locations[count] = new Register();
                locations[count].overwriteContents(instructions[count]);
            }

            //create instances of empty memory where instructions end
            //e.g. if last instruction at loc 34, 35-255 will be empty
            for (int count = instructions.Length; count < Globals.RAM_SIZE; count++)
                locations[count] = new Register();
        }

        public BitArray readFromLocation(BitArray address)
        {
            //convert address to decimal - so value can be used to index RAM array
            byte[] addr = new byte[1];
            address.CopyTo(addr, 0);

            //access address and read the data
            AccessLocation?.Invoke(locations[addr[0]].readContents(), addr[0], Globals.REGISTER_READ);
            return locations[addr[0]].readContents();
        }

        public void writeToLocation(BitArray address, BitArray data)
        {
            //convert address to decimal - so value can be used to index RAM array
            byte[] addr = new byte[1];
            address.CopyTo(addr, 0);

            //access address and write new data
            locations[addr[0]].overwriteContents(data);
            AccessLocation?.Invoke(data, addr[0], Globals.REGISTER_WRITE);
            if (AccessLocation != null) Thread.Sleep(Globals.CLOCK_SPEED);
        }
        //------------------------------------------------------------
    }

    public class MAR : Register
    {
        //PRIVATE
        //-------------------------------------
        private RAM RAM;
        //-------------------------------------

        //PUBLIC
        //-------------------------------------
        public MAR(BitArray[] instructions, SetEnableGPR accessRAM, SetEnableRegister accessContents) 
            : base(accessContents)
        {
            RAM = new RAM(instructions, accessRAM);
        }

        //access MAR contents to locate memory address and read/write at the location
        public void writeToMemory(BitArray data) { RAM.writeToLocation(this.getContents(), data);   }
        public BitArray readFromMemory()         { return RAM.readFromLocation(this.getContents()); }
        //-------------------------------------
    }

    public class ALU
    {
        //PRIVATE
        //-------------------------------------
        private bool BUS1 = false;
        //-------------------------------------

        //PUBLIC
        //-------------------------------------
        public static class Opcodes { public const string ADD = "1000", R_SHIFT = "1001", L_SHIFT = "1010", NOT     = "1011",
                                                          AND = "1100", OR      = "1101", XOR     = "1110", COMPARE = "1111"; }

        public static class Flags   { public const int COUT = 3, EQUAL = 2, A_LARGER = 1, ZERO = 0; }

        public bool[] flags = new bool[Globals.FLAG_COUNT];
        public Register TMP;

        public ALU(SetEnableRegister overwriteTMP)
        {
            TMP = new Register(overwriteTMP);

            //initialise flags to false
            for (int count = 0; count < Globals.FLAG_COUNT; count++) flags[count] = false;
        }

        public void toggleBUS1()
        {
            if (BUS1) BUS1 = false;
            else BUS1 = true;
        }

        public BitArray add(BitArray firstNumber)
        {
            BitArray secondNumber, result = new BitArray(Globals.DATA_INSTRUCTION_SIZE);

            //add instruction can use BUS1 register to increment first number
            //e.g. passing IAR to ALU, enabling BUS1 and running add -> increments IAR
            if (BUS1) secondNumber = new BitArray(new byte[] { 1 });
            else secondNumber = new BitArray(TMP.readContents());

            bool carryBit = false;

            //examine each bit of both values and add them together
            for (int count = 0; count < Globals.DATA_INSTRUCTION_SIZE; count++)
            {
                //last operation resulted in a carry
                if (carryBit)
                {
                    //0 + 0 + 1 = 1 no carry
                    if (firstNumber[count] == false && secondNumber[count] == false)
                    {
                        result[count] = true;
                        carryBit = false;
                    }

                    //1 + 1 + 1 = 1 carry 1
                    else if (firstNumber[count] == true && secondNumber[count] == true)
                        result[count] = true;

                    //1 + 0 + 1 = 0 carry 1
                    else result[count] = false;
                }

                else
                {
                    //0 + 0 = 0 no carry
                    if (firstNumber[count] == false && secondNumber[count] == false)
                        result[count] = false;

                    //1 + 1 = 0 carry 1
                    else if (firstNumber[count] == true && secondNumber[count] == true)
                    {
                        result[count] = false;
                        carryBit = true;
                    }

                    //0 + 1 = 1 no carry
                    else result[count] = true;
                }
            }

            return result;
        }

        public BitArray shiftRight(BitArray data)
        {
            BitArray result = new BitArray(Globals.DATA_INSTRUCTION_SIZE);

            //righmost bit will get shifted out, enable carry flag
            if (data[0]) flags[Flags.COUT] = true;

            //look ahead to next bit of a and copy into current bit of result
            for (int front = 1, back = 0; front < Globals.DATA_INSTRUCTION_SIZE; front++, back++)
                result[back] = data[front];

            return result;
        }

        public BitArray shiftLeft(BitArray data)
        {
            BitArray result = new BitArray(Globals.DATA_INSTRUCTION_SIZE);

            //leftmost bit will get shifted out, enable carry flag
            if (data[Globals.FIRST_DATA_INSTRUCTION_BIT]) flags[Flags.COUT] = true;

            //copy current bit of a into next bit of result
            for (int front = 1, back = 0; front < Globals.DATA_INSTRUCTION_SIZE; front++, back++)
                result[front] = data[back];

            return result;
        }

        public BitArray inverse(BitArray data) { return data.Not(); }

        public BitArray and(BitArray data) { return data.And(TMP.readContents()); }

        public BitArray or(BitArray data) { return data.Or(TMP.readContents()); }

        public BitArray xor(BitArray data) { return data.Xor(TMP.readContents()); }

        public void compare(BitArray firstNumber)
        {
            BitArray secondNumber = new BitArray(TMP.readContents());
            bool isequal = true, iszero = true;

            int count = Globals.FIRST_DATA_INSTRUCTION_BIT;

            //examine most significant bits and work downward
            //first unequal bits breaks loop
            while (isequal && count >= 0)
            {
                //first > second
                if (firstNumber[count] && !secondNumber[count])
                {
                    isequal = false;
                    iszero = false;
                    flags[Flags.A_LARGER] = true;
                }

                //first < second
                else if (!firstNumber[count] && secondNumber[count])
                {
                    isequal = false;
                    iszero = false;
                }

                //first == 1 == second
                //values are not 0
                else if (iszero && firstNumber[count] && secondNumber[count])
                    iszero = false;

                count--;
            }

            if (isequal) flags[Flags.EQUAL] = true;
            if (iszero)  flags[Flags.ZERO] = true;
        }
        //-------------------------------------
    }

    public class ControlUnit
    {
        //PRIVATE
        //-------------------------------------
        private bool programEnd = false;    //condition to stop execution
        private byte registerA, registerB;  //temporary storage for register addresses in use

        private ALU ALU;

        private MAR MAR;
        private Register IAR, IR, accumulator;
        private Register[] GPR = new Register[Globals.GPR_COUNT];
        //-------------------------------------

        //PUBLIC
        //-------------------------------------
        public event SetEnableGPR AccessGPR;
        public event RedrawGUI TurnOffSetEnableBits;

        //tracks the address of the last instruction to execute
        public readonly BitArray lastAddress = new BitArray(Globals.DATA_INSTRUCTION_SIZE);

        public ControlUnit(BitArray[] instructions, SetEnableGPR accessRAM,                                     //RAM parameters
            SetEnableRegister overwriteIAR, SetEnableRegister overwriteIR, SetEnableRegister overwriteMAR,      //register parameters
            SetEnableRegister overwriteTMP, SetEnableRegister overwriteAccumulator, SetEnableGPR accessGPR,     
            RedrawGUI turnOffSetEnableBits)
        {
            //intialising each component and register and linking GUI events to them
            ALU         = new ALU(overwriteTMP);
            accumulator = new Register(overwriteAccumulator);
            MAR         = new MAR(instructions, accessRAM, overwriteMAR);
            IAR         = new Register(overwriteIAR);
            IR          = new Register(overwriteIR);

            AccessGPR += accessGPR;
            TurnOffSetEnableBits += turnOffSetEnableBits;

            lastAddress = new BitArray(new byte[] { (byte)(instructions.Length - 1) });

            //initialise GPRs
            for (int count = 0; count < Globals.GPR_COUNT; count++)
                GPR[count] = new Register();
        }

        public void start()
        {
            //restart program if start clicked again
            if (programEnd)
            {
                programEnd = false;

                //reset values of registers
                //-------------------------------------
                MAR.overwriteContents(new BitArray(new byte[] { 0 }));
                IAR.overwriteContents(new BitArray(new byte[] { 0 }));
                IR.overwriteContents(new BitArray(new byte[] { 0 }));
                accumulator.overwriteContents(new BitArray(new byte[] { 0 }));
                ALU.TMP.overwriteContents(new BitArray(new byte[] { 0 }));

                for (int count = 0; count < Globals.GPR_COUNT; count++)
                    GPR[count].overwriteContents(new BitArray(new byte[] { 0 }));
                //-------------------------------------
            }

            //program loop
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

        private void accessMemory()
        {
            MAR.overwriteContents(IAR.readContents());
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        private void incrementIAR()
        {
            if (Globals.areBitsEqual(IAR.getContents(), lastAddress)) programEnd = true;

            //enable bus1 register
            ALU.toggleBUS1();

            //call ADD instruction on ALU to increment IAR
            accumulator.overwriteContents(ALU.add(IAR.readContents()));
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0)  Thread.Sleep(1000);

            //turn off bus1
            ALU.toggleBUS1();

            //set new value of IAR
            IAR.overwriteContents(accumulator.readContents());
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        private void setInstructionRegister()
        {
            IR.overwriteContents(MAR.readFromMemory());
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        public void executeInstruction() { readInstructionRegister(); }

        private void readInstructionRegister()
        {
            BitArray opcode = new BitArray(Globals.OPCODE_SIZE),
                registerA = new BitArray(Globals.REGISTER_ADDRESS_SIZE), 
                registerB = new BitArray(Globals.REGISTER_ADDRESS_SIZE);

            //have the GUI show CU is reading the contents
            IR.readContents();
            Thread.Sleep(Globals.CLOCK_SPEED);
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //copy opcode bits from IR
            for (int count = Globals.OPCODE_SIZE, opindex = 0; count < Globals.DATA_INSTRUCTION_SIZE; count++, opindex++)
                opcode[opindex] = IR.getContents()[count];

            //deduce the register addresses from IR
            for (int count = 0, registerACount = Globals.REGISTER_ADDRESS_SIZE;     //B is at the start of the code, A is at the index after the end of B
                count < Globals.REGISTER_ADDRESS_SIZE; 
                count++, registerACount++)
            {
                registerA[count] = IR.getContents()[registerACount];
                registerB[count] = IR.getContents()[count];
            }

            registerA = Globals.reverseBitArray(registerA);
            registerB = Globals.reverseBitArray(registerB);

            //copy the register addresses back to a decimal value
            byte[] registerADecimal = new byte[1], registerBDecimal = new byte[1];
            registerA.CopyTo(registerADecimal, 0);
            registerB.CopyTo(registerBDecimal, 0);

            //note the selected registers for later
            this.registerA = registerADecimal[0];
            this.registerB = registerBDecimal[0];

            //ALU instruction (1xxx <- 1st bit represents ALU operation)
            //                (xxx1 <- bitarray stores binary in reverse)
            if (opcode[Globals.ALU_OPCODE])
            {
                string opcodeAsString = Globals.convertBitsToString(opcode);

                //is a 2 input operation
                if (opcodeAsString != ALU.Opcodes.R_SHIFT && opcodeAsString != ALU.Opcodes.L_SHIFT && opcodeAsString != ALU.Opcodes.NOT)
                {
                    //copy register B into TMP
                    AccessGPR?.Invoke(GPR[this.registerB].getContents(), this.registerB, Globals.REGISTER_READ);
                    ALU.TMP.overwriteContents(GPR[this.registerB].readContents());
                    Thread.Sleep(Globals.CLOCK_SPEED);

                    TurnOffSetEnableBits?.Invoke();
                    if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
                }

                switch (opcodeAsString)
                {
                    //ADD
                    case ALU.Opcodes.ADD:
                        accumulator.overwriteContents(ALU.add(GPR[this.registerA].readContents()));
                        break;

                    //RSHIFT
                    case ALU.Opcodes.R_SHIFT:
                        accumulator.overwriteContents(ALU.shiftRight(GPR[this.registerA].readContents()));
                        break;

                    //LSHIFT
                    case ALU.Opcodes.L_SHIFT:
                        accumulator.overwriteContents(ALU.shiftLeft(GPR[this.registerA].readContents()));
                        break;

                    //NOT
                    case ALU.Opcodes.NOT:
                        accumulator.overwriteContents(ALU.inverse(GPR[this.registerA].readContents()));
                        break;

                    //AND
                    case ALU.Opcodes.AND:
                        accumulator.overwriteContents(ALU.and(GPR[this.registerA].readContents()));
                        break;

                    //OR
                    case ALU.Opcodes.OR:
                        accumulator.overwriteContents(ALU.or(GPR[this.registerA].readContents()));
                        break;

                    //XOR
                    case ALU.Opcodes.XOR:
                        accumulator.overwriteContents(ALU.xor(GPR[this.registerA].readContents()));
                        break;

                    //compare
                    case ALU.Opcodes.COMPARE:
                        ALU.compare(GPR[this.registerA].readContents());
                        break;

                    default:
                        MessageBox.Show("Invalid opcode");
                        break;
                }

                //compare should only output flags
                if (opcodeAsString != ALU.Opcodes.COMPARE)
                {
                    //copy accumulator into register B
                    GPR[this.registerB].overwriteContents(accumulator.readContents());
                    AccessGPR?.Invoke(GPR[this.registerB].getContents(), this.registerB, Globals.REGISTER_WRITE);
                    Thread.Sleep(Globals.CLOCK_SPEED);
                    TurnOffSetEnableBits?.Invoke();
                    if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
                }
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
                        //register addresses correspond to flags
                        //recombine them
                        bool[] combined = new bool[Globals.FLAG_COUNT];

                        registerA.CopyTo(combined, Globals.REGISTER_ADDRESS_SIZE);
                        registerB.CopyTo(combined, 0);

                        BitArray condition = new BitArray(combined);

                        jumpIf(condition);
                        break;

                    case "0110":
                        resetFlags();
                        break;

                    case "0111":
                        //IO
                        break;
                }
            }
        }

        //copy data from RAM to register (address specified)
        private void load()
        {
            //load address in register A to MAR
            AccessGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
            MAR.overwriteContents(GPR[registerA].readContents());
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //save the data in RAM to register B
            GPR[registerB].overwriteContents(MAR.readFromMemory());
            AccessGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_WRITE);
            Thread.Sleep(Globals.CLOCK_SPEED);
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //copy data from register to RAM
        private void store()
        {
            //load address in register A to MAR
            AccessGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
            MAR.overwriteContents(GPR[registerA].readContents());
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //save the data in register B to RAM
            AccessGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_READ);
            MAR.writeToMemory(GPR[registerB].readContents());
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //copy data from RAM to register (next RAM location)
        private void data()
        {
            //prepares next RAM location for access
            MAR.overwriteContents(IAR.readContents());
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //IAR steps over data to next instruction
            incrementIAR();

            //copy data to register B
            GPR[registerB].overwriteContents(MAR.readFromMemory());
            AccessGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_WRITE);
            Thread.Sleep(Globals.CLOCK_SPEED);
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //jump to address stored in register B
        private void jumpRegister()
        {
            AccessGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_READ);
            IAR.overwriteContents(GPR[registerB].readContents());
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //jump to address thats in the next RAM location
        private void jump()
        {
            MAR.overwriteContents(IAR.readContents());
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            IAR.overwriteContents(MAR.readFromMemory());
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //jumps to an address in next RAM location if condition is met
        private void jumpIf(BitArray condition)
        {
            if (condition.Length == Globals.FLAG_COUNT)
            {
                bool conditionmet = true;

                //check provided flags are all on
                for (int count = 0; count < Globals.FLAG_COUNT && conditionmet; count++)
                    if (condition[count] && !ALU.flags[count]) conditionmet = false;

                //flags on, jump to address
                if (conditionmet)
                {
                    MAR.overwriteContents(IAR.readContents());
                    TurnOffSetEnableBits?.Invoke();
                    if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

                    IAR.overwriteContents(MAR.readFromMemory());
                    TurnOffSetEnableBits?.Invoke();
                    if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
                }

                //skip jump address
                else incrementIAR();
            }

            else MessageBox.Show("Invalid condition");
        }

        private void resetFlags() { for (int count = 0; count < Globals.FLAG_COUNT; count++) ALU.flags[count] = false; }
        //-------------------------------------
    }
}