﻿using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace CPU_Simulator
{
    public delegate void ReadOnlyRegister();                                    //BUS1
    public delegate void ReadWriteRegister(BitArray data, bool read);           //IR, IAR, MAR, TMP etc.
    public delegate void ReadWriteMemory(BitArray data, int index, bool read);  //require an address (i.e. RAM, GPRs etc.)
    public delegate void ReadWriteFlags(bool read, int flag = -1);
    public delegate void ALUOperation(BitArray opcode);                     
    public delegate void RedrawGUI();                                           //redraw before next step

    public partial class MainForm : Form
    {
        //PRIVATE
        private Thread CPUThread;
        private ControlUnit CU;

        //indicates if a quick redraw can be done
        private bool welcomeScreenDrawn = false;
        private bool UIDrawn = false;
        private bool registerScreenDrawn = false;
        private bool stackScreenDrawn = false;

        //welcome screen
        private Label lblWelcome = new Label();
        private Label lblMsg = new Label();
        private Button btnRegister = new Button();
        private Button btnStack = new Button();

        //controls
        GroupBox grpControls = new GroupBox();
        Button btnStart = new Button();
        Button btnPauseStop = new Button();
        Button btnReturn = new Button();
        Label lblClock = new Label();
        Label lblClockSpeed = new Label();
        Button btnIncrease = new Button();
        Button btnDecrease = new Button();

        //code editor
        RichTextBox txtCodeEditor = new RichTextBox();
        Button btnLoadCode = new Button();
        Button btnSaveCode = new Button();

        //CPU components
        //---------------------------------------------------
        private Panel pnlControlUnit = new Panel();
        private Label lblControlUnit = new Label();

        private Panel pnlIAR = new Panel();
        private Label lblIAR = new Label();
        private Label lblIARContents = new Label();

        private Panel pnlIR = new Panel();
        private Label lblIR = new Label();
        private Label lblIRContents = new Label();

        private Panel[] pnlGPR = new Panel[Globals.NO_OF_GPR];
        private Label[] lblGPR = new Label[Globals.NO_OF_GPR];
        private Label[] lblGPRContents = new Label[Globals.NO_OF_GPR];

        private Panel pnlMAR = new Panel();
        private Label lblMAR = new Label();
        private Label lblMARContents = new Label();

        private Panel pnlRAM = new Panel();
        private Label lblRAM = new Label();
        private Label lblRAMContents = new Label();
        private Label lblRAMAddress = new Label();

        private Panel pnlALU = new Panel();
        private Label lblALU = new Label();

        //flags
        //---------------------------------------------------                  
        private Panel pnlFlags = new Panel();
        private Label lblFlags = new Label();

        private Label lblCout = new Label();
        private Label lblCoutContents = new Label();

        private Label lblALarger = new Label();
        private Label lblALargerContents = new Label();

        private Label lblEqual = new Label();
        private Label lblEqualContents = new Label();

        private Label lblZero = new Label();
        private Label lblZeroContents = new Label();
        //---------------------------------------------------

        private Panel pnlTMP = new Panel();
        private Label lblTMP = new Label();
        private Label lblTMPContents = new Label();

        private Panel pnlBUS1 = new Panel();
        private Label lblBUS1 = new Label();

        private Panel pnlAcc = new Panel();
        private Label lblAcc = new Label();
        private Label lblAccContents = new Label();
        //---------------------------------------------------

        //draw methods
        //-------------------------------------
        private void showWelcomeScreen()
        {
            ClientSize = new Size(230, 140);
            Text = "Welcome!";

            //object properties already calculated, just add back to the form (faster draw speed)
            if (welcomeScreenDrawn)
            {
                Controls.Add(lblWelcome);
                Controls.Add(lblMsg);
                Controls.Add(btnRegister);
                Controls.Add(btnStack);
            }

            else
            {
                lblWelcome.Text = "CPU Simulator";
                lblWelcome.Font = new Font("Microsoft Sans Serif", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblWelcome.Location = new Point(30, 20);
                lblWelcome.Size = new Size(180, 25);

                Controls.Add(lblWelcome);

                lblMsg.Text = "Choose the CPU to use:";
                lblMsg.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblMsg.Location = new Point(25, 55);
                lblMsg.Size = new Size(180, 20);

                Controls.Add(lblMsg);

                btnRegister.Text = "Register";
                btnRegister.Location = new Point(35, 85);
                btnRegister.Size = new Size(75, 25);
                btnRegister.Click += new EventHandler(btnRegister_Click);

                Controls.Add(btnRegister);

                btnStack.Text = "Stack";
                btnStack.Location = new Point(125, 85);
                btnStack.Size = new Size(75, 25);
                btnStack.Click += new EventHandler(btnStack_Click);

                Controls.Add(btnStack);

                welcomeScreenDrawn = true;
            }
        }

        private void hideWelcomeScreen()
        {
            Controls.Remove(lblWelcome);
            Controls.Remove(lblMsg);
            Controls.Remove(btnRegister);
            Controls.Remove(btnStack);
        }

        private void drawUI()
        {
            //object properties already calculated, just add back to the form (faster draw speed)
            if (UIDrawn)
            {
                Controls.Add(grpControls);
                Controls.Add(txtCodeEditor);
                Controls.Add(btnLoadCode);
                Controls.Add(btnSaveCode);

                Graphics drawTextBoxBorder = CreateGraphics();
                Pen border = new Pen(Color.Black);

                drawTextBoxBorder.DrawRectangle(border, 785, 160, 345, 275);

                border.Dispose();
                drawTextBoxBorder.Dispose();
            }

            else
            {
                //controls box
                //-------------------------------------------------------------------------------------------------
                grpControls.Text = "Controls";
                grpControls.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
                grpControls.Location = new Point(785, 5);
                grpControls.Size = new Size(345, 145);

                Controls.Add(grpControls);

                //start CPU
                btnStart.Text = "Start";
                btnStart.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                btnStart.FlatStyle = FlatStyle.Popup;
                btnStart.BackColor = Color.LawnGreen;
                btnStart.Location = new Point(20, 30);
                btnStart.Size = new Size(75, 25);
                btnStart.Click += new EventHandler(btnStart_Click);

                grpControls.Controls.Add(btnStart);

                //pause CPU
                btnPauseStop.Text = "Pause";
                btnPauseStop.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                btnPauseStop.FlatStyle = FlatStyle.Popup;
                btnPauseStop.BackColor = Color.LightGray;
                btnPauseStop.Location = new Point(20, 65);
                btnPauseStop.Size = new Size(75, 25);
                btnPauseStop.Click += new EventHandler(btnPauseStop_Click);

                grpControls.Controls.Add(btnPauseStop);

                //return to home
                btnReturn.Text = "Go Back";
                btnReturn.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                btnReturn.FlatStyle = FlatStyle.Popup;
                btnReturn.BackColor = Color.LightGray;
                btnReturn.Location = new Point(20, 100);
                btnReturn.Size = new Size(75, 25);
                btnReturn.Click += new EventHandler(btnReturn_Click);

                grpControls.Controls.Add(btnReturn);

                //clock speed
                lblClock.Text = "Clock Speed:";
                lblClock.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblClock.Location = new Point(115, 55);
                lblClock.Size = new Size(75, 13);

                grpControls.Controls.Add(lblClock);

                btnDecrease.Text = "-";
                btnDecrease.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                btnDecrease.Location = new Point(190, 50);
                btnDecrease.Size = new Size(30, 25);
                btnDecrease.Click += new EventHandler(btnDecrease_Click);

                grpControls.Controls.Add(btnDecrease);

                lblClockSpeed.Text = "Real-time";
                lblClockSpeed.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblClockSpeed.Location = new Point(225, 55);
                lblClockSpeed.Size = new Size(51, 15);

                grpControls.Controls.Add(lblClockSpeed);

                btnIncrease.Text = "+";
                btnIncrease.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                btnIncrease.Location = new Point(280, 50);
                btnIncrease.Size = new Size(30, 25);
                btnIncrease.Click += new EventHandler(btnIncrease_Click);

                grpControls.Controls.Add(btnIncrease);
                //-------------------------------------------------------------------------------------------------

                //programming area
                //-------------------------------------------------------------------------------------------------
                txtCodeEditor.Text = "Write your code here.";
                txtCodeEditor.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
                txtCodeEditor.ForeColor = Color.Gray;

                //draw border
                //-------------------------------------
                txtCodeEditor.BorderStyle = BorderStyle.None;

                Graphics drawTextBoxBorder = CreateGraphics();
                Pen border = new Pen(Color.Black);

                drawTextBoxBorder.DrawRectangle(border, 785, 160, 345, 275);

                border.Dispose();
                drawTextBoxBorder.Dispose();
                //-------------------------------------

                txtCodeEditor.Location = new Point(786, 161);
                txtCodeEditor.Size = new Size(344, 274);
                txtCodeEditor.GotFocus += new EventHandler(txtCodeEditor_GotFocus);
                txtCodeEditor.LostFocus += new EventHandler(txtCodeEditor_LostFocus);

                Controls.Add(txtCodeEditor);

                btnLoadCode.Text = "Load";
                btnLoadCode.FlatStyle = FlatStyle.Popup;
                btnLoadCode.BackColor = Color.LightGray;
                btnLoadCode.Location = new Point(970, 445);
                btnLoadCode.Size = new Size(75, 25);
                btnLoadCode.Click += new EventHandler(btnLoadCode_Click);

                Controls.Add(btnLoadCode);

                btnSaveCode.Text = "Save";
                btnSaveCode.FlatStyle = FlatStyle.Popup;
                btnSaveCode.BackColor = Color.LightGray;
                btnSaveCode.Location = new Point(1055, 445);
                btnSaveCode.Size = new Size(75, 25);
                btnSaveCode.Click += new EventHandler(btnSaveCode_Click);

                Controls.Add(btnSaveCode);
                //-------------------------------------------------------------------------------------------------

                UIDrawn = true;
            }
        }

        private void hideUI()
        {
            Controls.Remove(grpControls);
            Controls.Remove(txtCodeEditor);
            Controls.Remove(btnLoadCode);
            Controls.Remove(btnSaveCode);
        }

        private void drawComponents()
        {
            //object properties already calculated, just add back to the form (faster draw speed)
            if (registerScreenDrawn)
            {
                Controls.Add(pnlControlUnit);
                Controls.Add(pnlIAR);
                Controls.Add(pnlIR);
                Controls.Add(pnlTMP);
                Controls.Add(pnlBUS1);
                Controls.Add(pnlALU);
                Controls.Add(pnlAcc);
                Controls.Add(pnlRAM);
                Controls.Add(pnlMAR);
                Controls.Add(pnlFlags);

                for (int count = 0; count < Globals.NO_OF_GPR; count++)
                    Controls.Add(pnlGPR[count]);
            }

            else
            {
                //control Unit
                //--------------------------------------------
                pnlControlUnit.BorderStyle = BorderStyle.FixedSingle;
                pnlControlUnit.Location = new Point(275, 175);
                pnlControlUnit.Size = new Size(350, 200);

                //add control unit to form
                Controls.Add(pnlControlUnit);

                //labelling control unit
                lblControlUnit.Text = "Control Unit";
                lblControlUnit.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblControlUnit.TextAlign = ContentAlignment.TopCenter;
                lblControlUnit.Location = new Point(130, 90);
                lblControlUnit.Size = new Size(90, 20);

                //add label to control unit
                pnlControlUnit.Controls.Add(lblControlUnit);
                //--------------------------------------------

                //IAR
                //--------------------------------------------
                pnlIAR.BorderStyle = BorderStyle.FixedSingle;
                pnlIAR.Location = new Point(350, 395);
                pnlIAR.Size = new Size(80, 50);

                Controls.Add(pnlIAR);

                //labelling IAR
                lblIAR.Text = "IAR";
                lblIAR.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblIAR.TextAlign = ContentAlignment.MiddleCenter;
                lblIAR.Location = new Point(22, 4);
                lblIAR.Size = new Size(40, 20);

                pnlIAR.Controls.Add(lblIAR);

                //contents of IAR
                lblIARContents.Text = "00000000";
                lblIARContents.TextAlign = ContentAlignment.MiddleCenter;
                lblIARContents.BorderStyle = BorderStyle.FixedSingle;
                lblIARContents.Location = new Point(10, 26);
                lblIARContents.Size = new Size(60, 20);

                pnlIAR.Controls.Add(lblIARContents);
                //--------------------------------------------

                //IR
                //--------------------------------------------
                pnlIR.BorderStyle = BorderStyle.FixedSingle;
                pnlIR.Location = new Point(470, 395);
                pnlIR.Size = new Size(80, 50);

                Controls.Add(pnlIR);

                //labelling IR
                lblIR.Text = "IR";
                lblIR.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblIR.TextAlign = ContentAlignment.MiddleCenter;
                lblIR.Location = new Point(22, 4);
                lblIR.Size = new Size(40, 20);

                pnlIR.Controls.Add(lblIR);

                //contents of IR
                lblIRContents.Text = "00000000";
                lblIRContents.TextAlign = ContentAlignment.MiddleCenter;
                lblIRContents.BorderStyle = BorderStyle.FixedSingle;
                lblIRContents.Location = new Point(10, 26);
                lblIRContents.Size = new Size(60, 20);

                pnlIR.Controls.Add(lblIRContents);
                //--------------------------------------------

                //GPR registers
                //--------------------------------------------
                for (int count = 0; count < Globals.NO_OF_GPR; count++)
                {
                    pnlGPR[count] = new Panel();
                    lblGPR[count] = new Label();
                    lblGPRContents[count] = new Label();
                }

                //GPR 1
                //--------------------------------------------
                pnlGPR[0].BorderStyle = BorderStyle.FixedSingle;
                pnlGPR[0].Location = new Point(655, 160);
                pnlGPR[0].Size = new Size(80, 50);

                Controls.Add(pnlGPR[0]);

                //labelling GPR
                lblGPR[0].Text = "RG1";
                lblGPR[0].Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblGPR[0].TextAlign = ContentAlignment.MiddleCenter;
                lblGPR[0].Location = new Point(20, 4);
                lblGPR[0].Size = new Size(41, 20);

                pnlGPR[0].Controls.Add(lblGPR[0]);

                //contents of GPR
                lblGPRContents[0].Text = "00000000";
                lblGPRContents[0].TextAlign = ContentAlignment.MiddleCenter;
                lblGPRContents[0].BorderStyle = BorderStyle.FixedSingle;
                lblGPRContents[0].Location = new Point(10, 26);
                lblGPRContents[0].Size = new Size(60, 20);

                pnlGPR[0].Controls.Add(lblGPRContents[0]);

                //GPR 2
                //--------------------------------------------
                pnlGPR[1].BorderStyle = BorderStyle.FixedSingle;
                pnlGPR[1].Location = new Point(655, 220);
                pnlGPR[1].Size = new Size(80, 50);

                Controls.Add(pnlGPR[1]);

                //labelling GPR
                lblGPR[1].Text = "RG2";
                lblGPR[1].Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblGPR[1].TextAlign = ContentAlignment.MiddleCenter;
                lblGPR[1].Location = new Point(20, 4);
                lblGPR[1].Size = new Size(41, 20);

                pnlGPR[1].Controls.Add(lblGPR[1]);

                //contents of GPR
                lblGPRContents[1].Text = "00000000";
                lblGPRContents[1].TextAlign = ContentAlignment.MiddleCenter;
                lblGPRContents[1].BorderStyle = BorderStyle.FixedSingle;
                lblGPRContents[1].Location = new Point(10, 26);
                lblGPRContents[1].Size = new Size(60, 20);

                pnlGPR[1].Controls.Add(lblGPRContents[1]);

                //GPR 3
                //--------------------------------------------
                pnlGPR[2].BorderStyle = BorderStyle.FixedSingle;
                pnlGPR[2].Location = new Point(655, 280);
                pnlGPR[2].Size = new Size(80, 50);

                Controls.Add(pnlGPR[2]);

                //labelling GPR
                lblGPR[2].Text = "RG3";
                lblGPR[2].Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblGPR[2].TextAlign = ContentAlignment.MiddleCenter;
                lblGPR[2].Location = new Point(20, 4);
                lblGPR[2].Size = new Size(41, 20);

                pnlGPR[2].Controls.Add(lblGPR[2]);

                //contents of GPR
                lblGPRContents[2].Text = "00000000";
                lblGPRContents[2].TextAlign = ContentAlignment.MiddleCenter;
                lblGPRContents[2].BorderStyle = BorderStyle.FixedSingle;
                lblGPRContents[2].Location = new Point(10, 26);
                lblGPRContents[2].Size = new Size(60, 20);

                pnlGPR[2].Controls.Add(lblGPRContents[2]);

                //GPR 4
                //--------------------------------------------
                pnlGPR[3].BorderStyle = BorderStyle.FixedSingle;
                pnlGPR[3].Location = new Point(655, 340);
                pnlGPR[3].Size = new Size(80, 50);

                Controls.Add(pnlGPR[3]);

                //labelling GPR
                lblGPR[3].Text = "RG4";
                lblGPR[3].Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblGPR[3].TextAlign = ContentAlignment.MiddleCenter;
                lblGPR[3].Location = new Point(20, 4);
                lblGPR[3].Size = new Size(41, 20);

                pnlGPR[3].Controls.Add(lblGPR[3]);

                //contents of GPR
                lblGPRContents[3].Text = "00000000";
                lblGPRContents[3].TextAlign = ContentAlignment.MiddleCenter;
                lblGPRContents[3].BorderStyle = BorderStyle.FixedSingle;
                lblGPRContents[3].Location = new Point(10, 26);
                lblGPRContents[3].Size = new Size(60, 20);

                pnlGPR[3].Controls.Add(lblGPRContents[3]);
                //--------------------------------------------

                //MAR
                //--------------------------------------------
                pnlMAR.BorderStyle = BorderStyle.FixedSingle;
                pnlMAR.Location = new Point(415, 10);
                pnlMAR.Size = new Size(80, 50);

                Controls.Add(pnlMAR);

                //labelling MAR
                lblMAR.Text = "MAR";
                lblMAR.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblMAR.TextAlign = ContentAlignment.MiddleCenter;
                lblMAR.Location = new Point(20, 4);
                lblMAR.Size = new Size(41, 20);

                pnlMAR.Controls.Add(lblMAR);

                //MAR contents
                lblMARContents.Text = "00000000";
                lblMARContents.TextAlign = ContentAlignment.MiddleCenter;
                lblMARContents.BorderStyle = BorderStyle.FixedSingle;
                lblMARContents.Location = new Point(10, 26);
                lblMARContents.Size = new Size(60, 20);

                pnlMAR.Controls.Add(lblMARContents);
                //--------------------------------------------

                //RAM
                //--------------------------------------------
                pnlRAM.BorderStyle = BorderStyle.FixedSingle;
                pnlRAM.Location = new Point(495, 10);
                pnlRAM.Size = new Size(270, 50);

                Controls.Add(pnlRAM);

                //labelling RAM
                lblRAM.Text = "RAM";
                lblRAM.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblRAM.TextAlign = ContentAlignment.TopCenter;
                lblRAM.Location = new Point(38, 16);
                lblRAM.Size = new Size(41, 18);

                pnlRAM.Controls.Add(lblRAM);

                //RAM contents
                lblRAMContents.Text = "00000000";
                lblRAMContents.TextAlign = ContentAlignment.MiddleCenter;
                lblRAMContents.BorderStyle = BorderStyle.FixedSingle;
                lblRAMContents.Location = new Point(164, 15);
                lblRAMContents.Size = new Size(60, 20);

                pnlRAM.Controls.Add(lblRAMContents);

                //RAM address
                lblRAMAddress.Text = "Addr 0:";
                lblRAMAddress.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblRAMAddress.TextAlign = ContentAlignment.TopCenter;
                lblRAMAddress.Location = new Point(107, 16);
                lblRAMAddress.Size = new Size(54, 18);

                pnlRAM.Controls.Add(lblRAMAddress);
                //--------------------------------------------

                //ALU
                //--------------------------------------------
                pnlALU.BorderStyle = BorderStyle.FixedSingle;
                pnlALU.Location = new Point(40, 175);
                pnlALU.Size = new Size(100, 200);

                Controls.Add(pnlALU);

                //labelling ALU
                lblALU.Text = "ALU";
                lblALU.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblALU.TextAlign = ContentAlignment.MiddleCenter;
                lblALU.Location = new Point(30, 90);
                lblALU.Size = new Size(40, 20);

                pnlALU.Controls.Add(lblALU);
                //--------------------------------------------

                //flag registers
                //--------------------------------------------
                pnlFlags.BorderStyle = BorderStyle.FixedSingle;
                pnlFlags.Location = new Point(170, 240);
                pnlFlags.Size = new Size(75, 135);

                Controls.Add(pnlFlags);

                //labelling flag register
                lblFlags.Text = "FLAGS";
                lblFlags.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblFlags.TextAlign = ContentAlignment.TopCenter;
                lblFlags.Location = new Point(10, 5);
                lblFlags.Size = new Size(60, 20);

                pnlFlags.Controls.Add(lblFlags);

                //flag labels & contents
                lblCout.Text = "COUT";
                lblCout.TextAlign = ContentAlignment.BottomRight;
                lblCout.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblCout.Location = new Point(5, 30);
                lblCout.Size = new Size(40, 15);

                pnlFlags.Controls.Add(lblCout);

                lblCoutContents.Text = "0";
                lblCoutContents.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblCoutContents.TextAlign = ContentAlignment.MiddleCenter;
                lblCoutContents.BorderStyle = BorderStyle.FixedSingle;
                lblCoutContents.Location = new Point(50, 30);
                lblCoutContents.Size = new Size(15, 15);

                pnlFlags.Controls.Add(lblCoutContents);

                lblALarger.Text = "A > B";
                lblALarger.TextAlign = ContentAlignment.BottomRight;
                lblALarger.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblALarger.Location = new Point(5, 50);
                lblALarger.Size = new Size(40, 15);

                pnlFlags.Controls.Add(lblALarger);

                lblALargerContents.Text = "0";
                lblALargerContents.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblALargerContents.TextAlign = ContentAlignment.MiddleCenter;
                lblALargerContents.BorderStyle = BorderStyle.FixedSingle;
                lblALargerContents.Location = new Point(50, 50);
                lblALargerContents.Size = new Size(15, 15);

                pnlFlags.Controls.Add(lblALargerContents);

                lblEqual.Text = "A = B";
                lblEqual.TextAlign = ContentAlignment.BottomRight;
                lblEqual.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblEqual.Location = new Point(5, 70);
                lblEqual.Size = new Size(40, 15);

                pnlFlags.Controls.Add(lblEqual);

                lblEqualContents.Text = "0";
                lblEqualContents.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblEqualContents.TextAlign = ContentAlignment.MiddleCenter;
                lblEqualContents.BorderStyle = BorderStyle.FixedSingle;
                lblEqualContents.Location = new Point(50, 70);
                lblEqualContents.Size = new Size(15, 15);

                pnlFlags.Controls.Add(lblEqualContents);

                lblZero.Text = "ZERO";
                lblZero.TextAlign = ContentAlignment.BottomRight;
                lblZero.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblZero.Location = new Point(5, 90);
                lblZero.Size = new Size(40, 15);

                pnlFlags.Controls.Add(lblZero);

                lblZeroContents.Text = "0";
                lblZeroContents.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblZeroContents.TextAlign = ContentAlignment.MiddleCenter;
                lblZeroContents.BorderStyle = BorderStyle.FixedSingle;
                lblZeroContents.Location = new Point(50, 90);
                lblZeroContents.Size = new Size(15, 15);

                pnlFlags.Controls.Add(lblZeroContents);
                //--------------------------------------------

                //TMP register
                //--------------------------------------------
                pnlTMP.BorderStyle = BorderStyle.FixedSingle;
                pnlTMP.Location = new Point(80, 60);
                pnlTMP.Size = new Size(80, 50);

                Controls.Add(pnlTMP);

                //labelling temp
                lblTMP.Text = "TMP";
                lblTMP.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblTMP.TextAlign = ContentAlignment.MiddleCenter;
                lblTMP.Location = new Point(20, 4);
                lblTMP.Size = new Size(40, 20);

                pnlTMP.Controls.Add(lblTMP);

                //temp contents
                lblTMPContents.Text = "00000000";
                lblTMPContents.TextAlign = ContentAlignment.MiddleCenter;
                lblTMPContents.BorderStyle = BorderStyle.FixedSingle;
                lblTMPContents.Location = new Point(10, 26);
                lblTMPContents.Size = new Size(60, 20);

                pnlTMP.Controls.Add(lblTMPContents);
                //--------------------------------------------

                //BUS1 register
                //--------------------------------------------
                //BUS1 object
                pnlBUS1.BorderStyle = BorderStyle.FixedSingle;
                pnlBUS1.Location = new Point(91, 130);
                pnlBUS1.Size = new Size(60, 25);

                Controls.Add(pnlBUS1);

                //labelling BUS1
                lblBUS1.Text = "BUS1";
                lblBUS1.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblBUS1.TextAlign = ContentAlignment.MiddleCenter;
                lblBUS1.Location = new Point(5, 1);
                lblBUS1.Size = new Size(50, 20);

                pnlBUS1.Controls.Add(lblBUS1);
                //--------------------------------------------

                //Accumulator register
                //--------------------------------------------
                pnlAcc.BorderStyle = BorderStyle.FixedSingle;
                pnlAcc.Location = new Point(50, 395);
                pnlAcc.Size = new Size(80, 50);

                Controls.Add(pnlAcc);

                //labelling accumulator
                lblAcc.Text = "ACC";
                lblAcc.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblAcc.TextAlign = ContentAlignment.MiddleCenter;
                lblAcc.Location = new Point(22, 4);
                lblAcc.Size = new Size(40, 20);

                pnlAcc.Controls.Add(lblAcc);

                //accumulator contents
                lblAccContents.Text = "00000000";
                lblAccContents.TextAlign = ContentAlignment.MiddleCenter;
                lblAccContents.BorderStyle = BorderStyle.FixedSingle;
                lblAccContents.Location = new Point(10, 26);
                lblAccContents.Size = new Size(60, 20);

                pnlAcc.Controls.Add(lblAccContents);
                //--------------------------------------------

                registerScreenDrawn = true;
            }
        }

        private void hideComponents()
        {
            Controls.Remove(pnlControlUnit);
            Controls.Remove(pnlIAR);
            Controls.Remove(pnlIR);
            Controls.Remove(pnlTMP);
            Controls.Remove(pnlBUS1);
            Controls.Remove(pnlALU);
            Controls.Remove(pnlAcc);
            Controls.Remove(pnlRAM);
            Controls.Remove(pnlMAR);
            Controls.Remove(pnlFlags);

            for (int count = 0; count < Globals.NO_OF_GPR; count++)
                Controls.Remove(pnlGPR[count]);
        }

        private void drawControlBits()
        {
            Graphics circuit = CreateGraphics();
            Pen controlWire = new Pen(Color.Black);
            SolidBrush wireJoint = new SolidBrush(Color.Black);

            controlWire.Width = 2;

            //CU position  |  x    y
            //top left     | 275, 175
            //bottom right | 625, 375

            //IAR
            circuit.DrawLine(controlWire, 365, 395, 365, 375);  //enable
            circuit.DrawLine(controlWire, 370, 395, 370, 375);  //set

            //IR
            circuit.DrawLine(controlWire, 485, 395, 485, 375);  //opcode 1
            circuit.DrawLine(controlWire, 488, 395, 488, 375);  //opcode 2
            circuit.DrawLine(controlWire, 491, 395, 491, 375);  //opcode 3
            circuit.DrawLine(controlWire, 494, 395, 494, 375);  //opcode 4
            circuit.DrawLine(controlWire, 497, 395, 497, 375);  //opcode 5
            circuit.DrawLine(controlWire, 500, 395, 500, 375);  //opcode 6
            circuit.DrawLine(controlWire, 503, 395, 503, 375);  //opcode 7
            circuit.DrawLine(controlWire, 506, 395, 506, 375);  //opcode 8

            circuit.DrawLine(controlWire, 530, 395, 530, 375);  //set

            //GPR1
            circuit.DrawLine(controlWire, 655, 190, 625, 190);  //enable
            circuit.DrawLine(controlWire, 655, 185, 625, 185);  //set

            //GPR2
            circuit.DrawLine(controlWire, 655, 250, 625, 250);  //enable
            circuit.DrawLine(controlWire, 655, 245, 625, 245);  //set

            //GPR3
            circuit.DrawLine(controlWire, 655, 315, 625, 315);  //enable
            circuit.DrawLine(controlWire, 655, 310, 625, 310);  //set

            //GPR4
            circuit.DrawLine(controlWire, 655, 365, 625, 365);  //enable
            circuit.DrawLine(controlWire, 655, 360, 625, 360);  //set

            //MAR
            circuit.DrawLine(controlWire, 430, 60, 430, 175);   //enable
            circuit.DrawLine(controlWire, 435, 60, 435, 175);   //set

            //RAM
            circuit.DrawLine(controlWire, 515, 60, 515, 175);   //enable
            circuit.DrawLine(controlWire, 520, 60, 520, 175);   //set

            //ALU
            circuit.DrawLine(controlWire, 140, 190, 275, 190);  //op 1
            circuit.DrawLine(controlWire, 140, 194, 275, 194);  //op 2
            circuit.DrawLine(controlWire, 140, 198, 275, 198);  //op 3

            //flags
            //-------------------------------------
            circuit.DrawLine(controlWire, 140, 280, 170, 280);  //cout ALU  -> flag
            circuit.DrawLine(controlWire, 245, 280, 275, 280);  //cout flag -> 

            circuit.DrawLine(controlWire, 140, 300, 170, 300);  //a >  ALU  -> flag
            circuit.DrawLine(controlWire, 245, 300, 275, 300);  //a >  flag -> 

            circuit.DrawLine(controlWire, 140, 320, 170, 320);  //=    ALU  -> flag
            circuit.DrawLine(controlWire, 245, 320, 275, 320);  //=    flag -> 

            circuit.DrawLine(controlWire, 140, 340, 170, 340);  //0    ALU  -> flag
            circuit.DrawLine(controlWire, 245, 340, 275, 340);  //0    flag -> 


            circuit.DrawLine(controlWire, 260, 280, 260, 230);  //cout -> cin
            circuit.FillEllipse(wireJoint, 255, 275, 8, 8);    //connection point
            circuit.DrawLine(controlWire, 260, 230, 140, 230);  //cout -> cin

            circuit.DrawLine(controlWire, 245, 360, 275, 360);  //set
            //-------------------------------------

            //TMP
            circuit.DrawLine(controlWire, 160, 75, 325, 75);    //enable
            circuit.DrawLine(controlWire, 325, 75, 325, 175);

            circuit.DrawLine(controlWire, 160, 80, 320, 80);    //set
            circuit.DrawLine(controlWire, 320, 80, 320, 175);

            //to BUS1
            circuit.DrawLine(controlWire, 151, 140, 285, 140);  //enable
            circuit.DrawLine(controlWire, 285, 140, 285, 175);

            //to accumulator
            circuit.DrawLine(controlWire, 130, 410, 285, 410);   //enable
            circuit.DrawLine(controlWire, 285, 410, 285, 375);

            circuit.DrawLine(controlWire, 130, 415, 290, 415);   //set
            circuit.DrawLine(controlWire, 290, 415, 290, 375);

            circuit.Dispose();
            controlWire.Dispose();
            wireJoint.Dispose();
        }

        private void drawBus(bool enable = false)
        {
            Graphics circuit = CreateGraphics();
            Pen bus;

            if (enable) bus = new Pen(Color.Red);
            else bus = new Pen(Color.Black);

            bus.Width = 1;

            //surrounding CPU
            //---------------------------------
            //MAR to top left
            circuit.DrawLine(bus, 415, 40, 15, 40);
            circuit.DrawLine(bus, 415, 35, 10, 35);

            //top left to bottom
            circuit.DrawLine(bus, 15, 40, 15, 465);
            circuit.DrawLine(bus, 10, 35, 10, 470);

            //bottom left to right
            circuit.DrawLine(bus, 10, 470, 760, 470);
            circuit.DrawLine(bus, 15, 465, 755, 465);

            //bottom right to RAM
            circuit.DrawLine(bus, 760, 470, 760, 60);
            circuit.DrawLine(bus, 755, 465, 755, 60);
            //---------------------------------

            //TMP
            circuit.DrawLine(bus, 117.5f, 35, 117.5f, 75);
            circuit.DrawLine(bus, 122.5f, 35, 122.5f, 75);

            //ALU
            circuit.DrawLine(bus, 57.5f, 35, 57.5f, 180);
            circuit.DrawLine(bus, 62.5f, 35, 62.5f, 180);

            if (!enable)
            {
                //TMP -> BUS1
                circuit.DrawLine(bus, 117.5f, 110, 117.5f, 130);
                circuit.DrawLine(bus, 122.5f, 110, 122.5f, 130);

                //BUS1 -> ALU
                circuit.DrawLine(bus, 117.5f, 155, 117.5f, 180);
                circuit.DrawLine(bus, 122.5f, 155, 122.5f, 180);

                //ALU -> accumulator
                circuit.DrawLine(bus, 87.5f, 375, 87.5f, 395);
                circuit.DrawLine(bus, 92.5f, 375, 92.5f, 395);
            }

            //accumulator
            circuit.DrawLine(bus, 87.5f, 445, 87.5f, 470);
            circuit.DrawLine(bus, 92.5f, 445, 92.5f, 470);

            //between IAR & IR
            circuit.DrawLine(bus, 447.5f, 470, 447.5f, 422.5f);
            circuit.DrawLine(bus, 452.5f, 470, 452.5f, 422.5f);

            //IAR
            circuit.DrawLine(bus, 452.5f, 422.5f, 430, 422.5f);
            circuit.DrawLine(bus, 452.5f, 427.5f, 430, 427.5f);

            //IR
            circuit.DrawLine(bus, 447.5f, 422.5f, 470, 422.5f);
            circuit.DrawLine(bus, 447.5f, 427.5f, 470, 427.5f);

            //GPR1
            circuit.DrawLine(bus, 735, 182.5f, 760, 182.5f);
            circuit.DrawLine(bus, 735, 187.5f, 760, 187.5f);

            //GPR2
            circuit.DrawLine(bus, 735, 242.5f, 760, 242.5f);
            circuit.DrawLine(bus, 735, 247.5f, 760, 247.5f);

            //GPR3
            circuit.DrawLine(bus, 735, 302.5f, 760, 302.5f);
            circuit.DrawLine(bus, 735, 307.5f, 760, 307.5f);

            //GPR4
            circuit.DrawLine(bus, 735, 362.5f, 760, 362.5f);
            circuit.DrawLine(bus, 735, 367.5f, 760, 367.5f);

            circuit.Dispose();
            bus.Dispose();
        }
        //-------------------------------------

        //control methods
        //-------------------------------------
        private void btnRegister_Click(object sender, EventArgs e)
        {
            ClientSize = new Size(1137, 489);
            Text = "Register CPU Simulator";

            hideWelcomeScreen();

            drawComponents();
            drawControlBits();
            drawBus();
            drawUI();

            loadRegisterCPU();
        }

        private void btnStack_Click(object sender, EventArgs e)
        {
            ClientSize = new Size(1137, 489);
            Text = "Stack CPU Simulator";

            hideWelcomeScreen();

            //drawComponents();
            //drawControlBits();
            //drawBus();
            //drawUI();
            //
            //loadRegisterCPU();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //no thread
            if (CPUThread == null)
            {
                CPUThread = new Thread(new ThreadStart(CU.start));
                CPUThread.Start();
            }

            //thread paused
            else if (CPUThread.ThreadState == ThreadState.Suspended)
            {
                CPUThread.Resume();
                btnPauseStop.Text = "Pause";
            }

            //thread stopped
            else if (CPUThread.ThreadState == ThreadState.Aborted)
            {
                //CPU will reset on thread start
                CU.triggerRestart();    

                //reset GUI
                resetCPU();             
                Thread.Sleep(Globals.CLOCK_SPEED);

                resetColours();

                CPUThread = new Thread(new ThreadStart(CU.start));
                CPUThread.Start();
                btnPauseStop.Text = "Pause";
            }

            //thread finished
            else if (CPUThread.ThreadState == ThreadState.Stopped)
            {
                //CPU will reset on thread start
                CU.triggerRestart();

                //reset GUI
                resetCPU();
                Thread.Sleep(Globals.CLOCK_SPEED);

                resetColours();
                
                CPUThread = new Thread(new ThreadStart(CU.start));
                CPUThread.Start();
            }
        }

        private void btnPauseStop_Click(object sender, EventArgs e)
        {
            //no thread active
            if (CPUThread == null || 
                CPUThread.ThreadState == ThreadState.Stopped)
                MessageBox.Show("CPU not currently running.");

            //thread paused
            else if (CPUThread.ThreadState == ThreadState.Suspended)
            {
                CPUThread.Resume();
                CPUThread.Abort();
                btnPauseStop.Text = "Stopped";
            }

            //thread active
            else if (CPUThread.ThreadState == ThreadState.Running || 
                CPUThread.ThreadState == ThreadState.WaitSleepJoin)
            {
                CPUThread.Suspend();
                btnPauseStop.Text = "Stop";
            }
        }

        private void btnDecrease_Click(object sender, EventArgs e)
        {
            if (Globals.CLOCK_SPEED != 0)
            {
                Globals.CLOCK_SPEED -= 1000;

                if (Globals.CLOCK_SPEED == 0) lblClockSpeed.Text = "Real-time";
                else lblClockSpeed.Text = Globals.CLOCK_SPEED.ToString();
            }
        }

        private void btnIncrease_Click(object sender, EventArgs e)
        {
            Globals.CLOCK_SPEED += 1000;
            lblClockSpeed.Text = Globals.CLOCK_SPEED.ToString();
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            //thread exists
            if (CPUThread != null)
            {
                //thread running
                if (CPUThread.ThreadState == ThreadState.Running ||
                    CPUThread.ThreadState == ThreadState.WaitSleepJoin)
                {
                    CPUThread.Suspend();
                }
            }

            hideUI();
            hideComponents();
            Invalidate();

            showWelcomeScreen();
        }

        private void txtCodeEditor_GotFocus(object sender, EventArgs e)
        {
            if (txtCodeEditor.Text == "Write your code here.")
            {
                txtCodeEditor.Text = "";
                txtCodeEditor.ForeColor = Color.Black;
            }
        }

        private void txtCodeEditor_LostFocus(object sender, EventArgs e)
        {
            if (txtCodeEditor.Text == "")
            {
                txtCodeEditor.Text = "Write your code here.";
                txtCodeEditor.ForeColor = Color.Gray;
            }
        }

        private void btnLoadCode_Click(object sender, EventArgs e)
        {
            //todo
        }

        private void btnSaveCode_Click(object sender, EventArgs e)
        {
            //todo
        }
        //-------------------------------------

        private void loadRegisterCPU()
        {
            //-------------------------------------------------------------------------------------------------------------
            //                                                        opcode            |  register A   |  register B
            //--------------------------------------------------------------------------|---------------|------------------
            //BitArray load     = new BitArray(new bool[] { false, false, false, false, | false, false, | false, false, });
            //BitArray store    = new BitArray(new bool[] { false, false, false, true,  | false, false, | false, false, });
            //BitArray data     = new BitArray(new bool[] { false, false, true,  false, | false, false, | false, false, });
            //BitArray jumpRG   = new BitArray(new bool[] { false, false, true,  true,  | false, false, | false, false, });
            //BitArray jump     = new BitArray(new bool[] { false, true,  false, false, | false, false, | false, false, });
            //--------------------------------------------------------------------------|----------------------------------
            //                                                        opcode            |   0   |  A >  |   =   | COUT
            //BitArray jumpIf   = new BitArray(new bool[] { false, true,  false, true,  | false, false, | false, false, });
            //--------------------------------------------------------------------------|----------------------------------
            //BitArray resetFlg = new BitArray(new bool[] { false, true,  true,  false, | false, false, | false, false, });
            //BitArray IO       = new BitArray(new bool[] { false, true,  true,  true,  | false, false, | false, false, });
            //--------------------------------------------------------------------------|---------------|------------------
            //BitArray add      = new BitArray(new bool[] { true,  false, false, false, | false, false, | false, false, });
            //BitArray rShift   = new BitArray(new bool[] { true,  false, false, true,  | false, false, | false, false, });
            //BitArray lShift   = new BitArray(new bool[] { true,  false, true,  false, | false, false, | false, false, });
            //BitArray not      = new BitArray(new bool[] { true,  false, true,  true,  | false, false, | false, false, });
            //BitArray and      = new BitArray(new bool[] { true,  true,  false, false, | false, false, | false, false, });
            //BitArray or       = new BitArray(new bool[] { true,  true,  false, true,  | false, false, | false, false, });
            //BitArray xor      = new BitArray(new bool[] { true,  true,  true,  false, | false, false, | false, false, });
            //BitArray compare  = new BitArray(new bool[] { true,  true,  true,  true,  | false, false, | false, false, });
            //-------------------------------------------------------------------------------------------------------------

            //instructions
            BitArray dataIntoReg1 = new BitArray(new bool[] { false, false, true, false, false, false, false, false });
            BitArray inputA = new BitArray(new bool[] { true, false, false, false, false, false, false, false });
            BitArray dataIntoReg2 = new BitArray(new bool[] { false, false, true, false, false, false, false, true });
            BitArray inputB = new BitArray(new bool[] { true, true, true, true, true, true, true, false });
            BitArray add = new BitArray(new bool[] { true, false, false, false, false, false, false, true });

            BitArray jumpIf = new BitArray(new bool[] { false, true, false, true, false, false, false, true });
            BitArray address = new BitArray(new bool[] { false, false, false, false, true, false, true, false });
            BitArray empty = new BitArray(new bool[] { false, false, false, false, false, false, false, false });
            BitArray dataIntoReg3 = new BitArray(new bool[] { false, false, true, false, false, false, true, false });
            BitArray inputC = new BitArray(new bool[] { false, true, true, true, true, true, true, false });

            dataIntoReg1 = Globals.reverseBitArray(dataIntoReg1);
            inputA = Globals.reverseBitArray(inputA);
            dataIntoReg2 = Globals.reverseBitArray(dataIntoReg2);
            inputB = Globals.reverseBitArray(inputB);
            add = Globals.reverseBitArray(add);

            jumpIf = Globals.reverseBitArray(jumpIf);
            address = Globals.reverseBitArray(address);
            dataIntoReg3 = Globals.reverseBitArray(dataIntoReg3);
            inputC = Globals.reverseBitArray(inputC);

            byte[] lastAddress = { 10 };

            //set instructions to load
            BitArray[] instructions = new BitArray[] { dataIntoReg1, inputA, dataIntoReg2, inputB, add, jumpIf, address, empty, empty, empty, dataIntoReg3, inputC };

            //link GUI events to CPU registers
            CU = new ControlUnit(instructions, accessRAMLocation, lastAddress,
                accessGPRContents, updateIARContents, updateIRContents, updateMARContents,
                updateTMPContents, readBUS1, accessALU, updateAccumulatorContents,
                updateFlagRegister, resetColours);
        }

        private void loadStackCPU()
        {
            //todo
        }

        //PUBLIC
        public MainForm()
        {
            showWelcomeScreen();
        }

        //CPU state changes
        //-------------------------------------
        public void updateIARContents(BitArray address, bool accessMode)
        {
            if (InvokeRequired) Invoke(new ReadWriteRegister(updateIARContents), new object[] { address, accessMode });
            else
            {
                Graphics circuit = CreateGraphics();

                if (accessMode == Globals.REGISTER_READ)
                {
                    Pen controlWire = new Pen(Globals.READ_COLOR);
                    controlWire.Width = 2;

                    //read process
                    //------------------------------------------------
                    circuit.DrawLine(controlWire, 365, 395, 365, 375);  //activate enable wire
                    enableBus();                                        //data flows onto the bus
                    lblIARContents.ForeColor = Globals.READ_COLOR;       //highlight data (visual aid)
                    //------------------------------------------------

                    controlWire.Dispose();
                }

                else if (accessMode == Globals.REGISTER_WRITE && address != null)
                {
                    Pen controlWire = new Pen(Globals.WRITE_COLOR);
                    controlWire.Width = 2;

                    //write process
                    //------------------------------------------------------
                    circuit.DrawLine(controlWire, 370, 395, 370, 375);          //activate set wire
                    lblIARContents.Text = Globals.convertBitsToString(address);    //data flows into memory (contents changed)
                    lblIARContents.ForeColor = Globals.WRITE_COLOR;              //highlight data (visual aid)
                    //------------------------------------------------------

                    controlWire.Dispose();
                }

                circuit.Dispose();
            }
        }

        public void updateIRContents(BitArray instruction, bool accessMode)
        {
            if (InvokeRequired) Invoke(new ReadWriteRegister(updateIRContents), new object[] { instruction, accessMode });
            else
            {
                Graphics circuit = CreateGraphics();

                if (accessMode == Globals.REGISTER_READ)
                {
                    Pen controlWire = new Pen(Globals.READ_COLOR);
                    controlWire.Width = 2;

                    //read process
                    //----------------------------------------------------------------
                    int xPos = 485;                                                     //position of leftmost opcode wire
                    for (int count = 0; count < Globals.WORD_SIZE; count++, xPos += 3)  //move to next wire along horizontal axis
                    {
                        if (lblIRContents.Text.ElementAt(count) == '1')                 //active opcode bits flow through the..
                            circuit.DrawLine(controlWire, xPos, 395, xPos, 375);        //..corresponding wire
                    }

                    lblIRContents.ForeColor = Globals.READ_COLOR;
                    //----------------------------------------------------------------

                    controlWire.Dispose();
                }

                else if (accessMode == Globals.REGISTER_WRITE && instruction != null)
                {
                    Pen controlWire = new Pen(Globals.WRITE_COLOR);
                    controlWire.Width = 2;

                    //write process
                    //-----------------------------------------------------
                    circuit.DrawLine(controlWire, 530, 395, 530, 375);      //activate set wire
                    lblIRContents.Text = Globals.convertBitsToString(instruction); //data flows into memory (contents changed)
                    lblIRContents.ForeColor = Globals.WRITE_COLOR;           //highlight data (visual aid)
                    //-----------------------------------------------------

                    controlWire.Dispose();
                }

                circuit.Dispose();
            }
        }

        public void updateMARContents(BitArray address, bool accessMode)
        {
            if (InvokeRequired) Invoke(new ReadWriteRegister(updateMARContents), new object[] { address, accessMode });
            else
            {
                Graphics circuit = CreateGraphics();

                if (accessMode == Globals.REGISTER_READ)
                {
                    Pen controlWire = new Pen(Globals.READ_COLOR);
                    controlWire.Width = 2;

                    //read process
                    //-----------------------------------------------
                    circuit.DrawLine(controlWire, 430, 60, 430, 175);   //activate enable wire
                    enableBus();                                        //data flows onto the bus
                    lblMARContents.ForeColor = Globals.READ_COLOR;       //highlight data (visual aid)
                    //-----------------------------------------------

                    controlWire.Dispose();
                }

                else if (accessMode == Globals.REGISTER_WRITE && address != null)
                {
                    Pen controlWire = new Pen(Globals.WRITE_COLOR);
                    controlWire.Width = 2;

                    //write process
                    //------------------------------------------------------
                    circuit.DrawLine(controlWire, 435, 60, 435, 175);           //activate set wire
                    lblMARContents.Text = Globals.convertBitsToString(address);    //data flows into memory (contents changed)
                    lblMARContents.ForeColor = Globals.WRITE_COLOR;              //highlight data (visual aid)
                    //------------------------------------------------------

                    controlWire.Dispose();
                }

                circuit.Dispose();
            }
        }

        public void updateTMPContents(BitArray data, bool accessMode)
        {
            if (InvokeRequired) Invoke(new ReadWriteRegister(updateTMPContents), new object[] { data, accessMode });
            else
            {
                Graphics circuit = CreateGraphics();

                if (accessMode == Globals.REGISTER_READ)
                {
                    Pen bus = new Pen(Globals.BUS_COLOR);
                    Pen controlWire = new Pen(Globals.READ_COLOR);

                    bus.Width = 1;
                    controlWire.Width = 2;

                    //read process
                    //-----------------------------------------------
                    //activate enable wire
                    circuit.DrawLine(controlWire, 160, 75, 325, 75);
                    circuit.DrawLine(controlWire, 325, 75, 325, 175);

                    //data flows into BUS1
                    circuit.DrawLine(bus, 117.5f, 110, 117.5f, 130);
                    circuit.DrawLine(bus, 122.5f, 110, 122.5f, 130);

                    //continues flow through BUS1 to ALU
                    circuit.DrawLine(bus, 117.5f, 155, 117.5f, 180);
                    circuit.DrawLine(bus, 122.5f, 155, 122.5f, 180);

                    //highlight data (visual aid)
                    lblTMPContents.ForeColor = Globals.READ_COLOR;
                    //-----------------------------------------------

                    controlWire.Dispose();
                    bus.Dispose();
                }

                else if (accessMode == Globals.REGISTER_WRITE && data != null)
                {
                    Pen controlWire = new Pen(Globals.WRITE_COLOR);
                    controlWire.Width = 2;

                    //write process
                    //------------------------------------------------------
                    //activate set wire
                    circuit.DrawLine(controlWire, 160, 80, 320, 80);
                    circuit.DrawLine(controlWire, 320, 80, 320, 175);

                    lblTMPContents.Text = Globals.convertBitsToString(data);    //data flows into memory (contents changed)
                    lblTMPContents.ForeColor = Globals.WRITE_COLOR;              //highlight data (visual aid)
                    //------------------------------------------------------

                    controlWire.Dispose();
                }

                circuit.Dispose();
            }
        }

        public void updateAccumulatorContents(BitArray data, bool accessMode)
        {
            if (InvokeRequired) Invoke(new ReadWriteRegister(updateAccumulatorContents), new object[] { data, accessMode });
            else
            {
                Graphics circuit = CreateGraphics();

                if (accessMode == Globals.REGISTER_READ)
                {
                    Pen controlWire = new Pen(Globals.READ_COLOR);
                    controlWire.Width = 2;

                    //read process
                    //---------------------------------------------------
                    //activate enable wire
                    circuit.DrawLine(controlWire, 130, 410, 285, 410);
                    circuit.DrawLine(controlWire, 285, 410, 285, 375);

                    enableBus();                                            //data flows onto the bus
                    lblAccContents.ForeColor = Globals.READ_COLOR;   //highlight data (visual aid)
                    //---------------------------------------------------

                    controlWire.Dispose();
                }

                else if (accessMode == Globals.REGISTER_WRITE && data != null)
                {
                    Pen bus = new Pen(Globals.BUS_COLOR);
                    Pen controlWire = new Pen(Globals.WRITE_COLOR);

                    bus.Width = 1;
                    controlWire.Width = 2;

                    //write process
                    //--------------------------------------------------------------
                    //activate set wire
                    circuit.DrawLine(controlWire, 130, 415, 290, 415);
                    circuit.DrawLine(controlWire, 290, 415, 290, 375);

                    //data flows from ALU to accumulator
                    circuit.DrawLine(bus, 87.5f, 375, 87.5f, 395);
                    circuit.DrawLine(bus, 92.5f, 375, 92.5f, 395);

                    lblAccContents.Text = Globals.convertBitsToString(data);    //data flows into memory (contents changed)
                    lblAccContents.ForeColor = Globals.WRITE_COLOR;              //highlight data (visual aid)
                    //--------------------------------------------------------------

                    controlWire.Dispose();
                    bus.Dispose();
                }

                circuit.Dispose();
            }
        }

        public void updateFlagRegister(bool accessMode, int flagIndex = -1)
        {
            if (InvokeRequired) Invoke(new ReadWriteFlags(updateFlagRegister), new object[] { accessMode, flagIndex });
            else
            {
                Graphics circuit = CreateGraphics();

                SolidBrush wireJoint = new SolidBrush(Globals.BUS_COLOR);
                Pen controlWire = new Pen(Globals.BUS_COLOR);
                controlWire.Width = 2;

                if (accessMode == Globals.REGISTER_READ)
                {
                    //read process
                    //------------------------------------------------
                    circuit.DrawLine(controlWire, 245, 280, 275, 280);  //data flows from COUT to CU
                    circuit.DrawLine(controlWire, 245, 300, 275, 300);  //data flows from A LARGER to CU
                    circuit.DrawLine(controlWire, 245, 320, 275, 320);  //data flows from EQUALS to CU
                    circuit.DrawLine(controlWire, 245, 340, 275, 340);  //data flows from ZERO to CU

                    //data flows from COUT back to CIN
                    circuit.DrawLine(controlWire, 260, 280, 260, 230);  
                    circuit.FillEllipse(wireJoint, 255, 275, 8, 8);
                    circuit.DrawLine(controlWire, 260, 230, 140, 230);

                    //highlight data (visual aid)
                    lblCoutContents.ForeColor = Globals.READ_COLOR;
                    lblALargerContents.ForeColor = Globals.READ_COLOR;
                    lblEqualContents.ForeColor = Globals.READ_COLOR;
                    lblZeroContents.ForeColor = Globals.READ_COLOR;
                    //------------------------------------------------
                }

                else if (accessMode == Globals.REGISTER_WRITE)
                {
                    //write process
                    //--------------------------------------------------------
                    circuit.DrawLine(controlWire, 245, 360, 275, 360);          //activate set wire

                    switch (flagIndex)
                    {
                        case 0:
                            circuit.DrawLine(controlWire, 140, 280, 170, 280);  //data flows from ALU to COUT
                            circuit.DrawLine(controlWire, 245, 280, 275, 280);  //continues flow to CU

                            //data flows from COUT back to CIN
                            circuit.DrawLine(controlWire, 260, 280, 260, 230);  
                            circuit.FillEllipse(wireJoint, 255, 275, 8, 8);     
                            circuit.DrawLine(controlWire, 260, 230, 140, 230);   

                            lblCoutContents.Text = "1";                         //data flows into memory (contents changed)
                            lblCoutContents.ForeColor = Globals.WRITE_COLOR;     //highlight data (visual aid)
                            break;

                        case 1:
                            circuit.DrawLine(controlWire, 140, 320, 170, 320);  //data flows from ALU to EQUAL
                            circuit.DrawLine(controlWire, 245, 320, 275, 320);  //continues flow to CU

                            lblEqualContents.Text = "1";                        //data flows into memory (contents changed)
                            lblEqualContents.ForeColor = Globals.WRITE_COLOR;    //highlight data (visual aid)
                            break;

                        case 2:
                            circuit.DrawLine(controlWire, 140, 300, 170, 300);  //data flows from ALU to A LARGER
                            circuit.DrawLine(controlWire, 245, 300, 275, 300);  //continues flow to CU

                            lblALargerContents.Text = "1";                      //data flows into memory (contents changed)
                            lblALargerContents.ForeColor = Globals.WRITE_COLOR;  //highlight data (visual aid)
                            break;

                        case 3:
                            circuit.DrawLine(controlWire, 140, 340, 170, 340);  //data flows from ALU to ZERO
                            circuit.DrawLine(controlWire, 245, 340, 275, 340);  //continues flow to CU

                            lblZeroContents.Text = "1";                         //data flows into memory (contents changed)
                            lblZeroContents.ForeColor = Globals.WRITE_COLOR;     //highlight data (visual aid)
                            break;

                        default:
                            //data flows into memory (contents changed)
                            lblCoutContents.Text = "0";
                            lblALargerContents.Text = "0";
                            lblEqualContents.Text = "0";
                            lblZeroContents.Text = "0";

                            //highlight data (visual aid)
                            lblCoutContents.ForeColor = Globals.WRITE_COLOR;
                            lblALargerContents.ForeColor = Globals.WRITE_COLOR;
                            lblEqualContents.ForeColor = Globals.WRITE_COLOR;
                            lblZeroContents.ForeColor = Globals.WRITE_COLOR;
                            break;
                    //--------------------------------------------------------
                    }
                }

                wireJoint.Dispose();
                controlWire.Dispose();
                circuit.Dispose();
            }
        }

        public void accessRAMLocation(BitArray data, int address, bool accessMode)
        {
            if (InvokeRequired) Invoke(new ReadWriteMemory(accessRAMLocation), new object[] { data, address, accessMode });
            else
            {
                if (address < Globals.RAM_SIZE && data != null)
                {
                    Graphics circuit = CreateGraphics();

                    lblRAMAddress.Text = "Addr " + address.ToString();          //indicate address being accessed
                    lblRAMContents.Text = Globals.convertBitsToString(data);    //show contents of address

                    if (accessMode == Globals.REGISTER_READ)
                    {
                        Pen controlWire = new Pen(Globals.READ_COLOR);
                        controlWire.Width = 2;

                        //read process
                        //-----------------------------------------------
                        circuit.DrawLine(controlWire, 515, 60, 515, 175);   //activate enable wire
                        enableBus();                                        //data flows onto the bus
                        lblRAMContents.ForeColor = Globals.READ_COLOR;       //highlight data (visual aid)
                        //-----------------------------------------------

                        controlWire.Dispose();
                    }

                    else if (accessMode == Globals.REGISTER_WRITE)
                    {
                        Pen controlWire = new Pen(Globals.WRITE_COLOR);
                        controlWire.Width = 2;

                        //write process
                        //-----------------------------------------------
                        circuit.DrawLine(controlWire, 520, 60, 520, 175);   //activate set wire
                        lblRAMContents.ForeColor = Globals.WRITE_COLOR;      //highlight data (visual aid)
                        //-----------------------------------------------

                        controlWire.Dispose();
                    }

                    circuit.Dispose();
                }
            }
        }

        public void accessGPRContents(BitArray data, int GPRindex, bool accessMode)
        {
            if (InvokeRequired) Invoke(new ReadWriteMemory(accessGPRContents), new object[] { data, GPRindex, accessMode });
            else
            {
                if (GPRindex < Globals.NO_OF_GPR)
                {
                    Graphics circuit = CreateGraphics();

                    if (accessMode == Globals.REGISTER_READ)
                    {
                        Pen controlWire = new Pen(Globals.READ_COLOR);
                        controlWire.Width = 2;

                        //read process
                        //--------------------------------------------------------
                        //activate enable wire for selected GPR
                        switch (GPRindex)
                        {
                            case 0:
                                circuit.DrawLine(controlWire, 655, 190, 625, 190);
                                break;
                            case 1:
                                circuit.DrawLine(controlWire, 655, 250, 625, 250);
                                break;
                            case 2:
                                circuit.DrawLine(controlWire, 655, 315, 625, 315);
                                break;
                            case 3:
                                circuit.DrawLine(controlWire, 655, 365, 625, 365);
                                break;
                            default:
                                break;
                        }

                        enableBus();                                                //data flows onto the bus
                        lblGPRContents[GPRindex].ForeColor = Globals.READ_COLOR;     //highlight data (visual aid)
                        //--------------------------------------------------------

                        controlWire.Dispose();
                    }

                    else if (accessMode == Globals.REGISTER_WRITE && data != null)
                    {
                        Pen controlWire = new Pen(Globals.WRITE_COLOR);
                        controlWire.Width = 2;

                        //write process
                        //----------------------------------------------------------------
                        //activate set wire for selected GPR
                        switch (GPRindex)
                        {
                            case 0:
                                circuit.DrawLine(controlWire, 655, 185, 625, 185);
                                break;
                            case 1:
                                circuit.DrawLine(controlWire, 655, 245, 625, 245);
                                break;
                            case 2:
                                circuit.DrawLine(controlWire, 655, 310, 625, 310);
                                break;
                            case 3:
                                circuit.DrawLine(controlWire, 655, 360, 625, 360);
                                break;
                            default:
                                break;
                        }
                        
                        lblGPRContents[GPRindex].Text = Globals.convertBitsToString(data);  //data flows into memory (contents changed)
                        lblGPRContents[GPRindex].ForeColor = Globals.WRITE_COLOR;            //highlight data (visual aid)
                        //----------------------------------------------------------------

                        controlWire.Dispose();
                    }

                    circuit.Dispose();
                }
            }
        }

        public void readBUS1()
        {
            if (InvokeRequired) Invoke(new ReadOnlyRegister(readBUS1));
            else
            {
                Graphics circuit = CreateGraphics();

                Pen bus = new Pen(Globals.BUS_COLOR);
                Pen controlWire = new Pen(Globals.READ_COLOR);

                bus.Width = 1;
                controlWire.Width = 2;

                //read process
                //------------------------------------------------
                //activate enable wire
                circuit.DrawLine(controlWire, 151, 140, 285, 140);
                circuit.DrawLine(controlWire, 285, 140, 285, 175);

                //data flows from BUS1 to ALU
                circuit.DrawLine(bus, 117.5f, 155, 117.5f, 180);
                circuit.DrawLine(bus, 122.5f, 155, 122.5f, 180);
                //------------------------------------------------

                controlWire.Dispose();
                bus.Dispose();
                circuit.Dispose();
            }
        }

        public void accessALU(BitArray opcode)
        {
            if (InvokeRequired) Invoke(new ALUOperation(accessALU), new object[] { opcode });
            else
            {
                Graphics circuit = CreateGraphics();

                Pen controlWire = new Pen(Globals.READ_COLOR);
                controlWire.Width = 2;

                //active opcode bits flow through the corresponding wire
                //--------------------------------------------------------------
                if (opcode[0]) circuit.DrawLine(controlWire, 140, 190, 275, 190);
                if (opcode[1]) circuit.DrawLine(controlWire, 140, 194, 275, 194);
                if (opcode[2]) circuit.DrawLine(controlWire, 140, 198, 275, 198);
                //--------------------------------------------------------------

                controlWire.Dispose();
                circuit.Dispose();
            }
        }

        public void enableBus() { drawBus(true); }

        public void resetColours()
        {
            if (InvokeRequired) Invoke(new RedrawGUI(resetColours));
            else
            {
                lblIRContents.ForeColor = Color.Black;
                lblIARContents.ForeColor = Color.Black;
                lblMARContents.ForeColor = Color.Black;
                lblRAMContents.ForeColor = Color.Black;
                lblTMPContents.ForeColor = Color.Black;
                lblAccContents.ForeColor = Color.Black;
                lblCoutContents.ForeColor = Color.Black;
                lblALargerContents.ForeColor = Color.Black;
                lblEqualContents.ForeColor = Color.Black;
                lblZeroContents.ForeColor = Color.Black;

                for (int count = 0; count < Globals.NO_OF_GPR; count++)
                {
                    lblGPRContents[count].ForeColor = Color.Black;
                    pnlGPR[count].Refresh();
                }

                pnlIR.Refresh();
                pnlIAR.Refresh();
                pnlMAR.Refresh();
                pnlRAM.Refresh();
                pnlTMP.Refresh();
                pnlAcc.Refresh();
                pnlFlags.Refresh();

                drawControlBits();
                drawBus();
            }
        }
        
        public void resetCPU()
        {
            lblIRContents.Text = "00000000";
            lblIRContents.ForeColor = Color.Red;

            lblIARContents.Text = "00000000";
            lblIARContents.ForeColor = Color.Red;

            lblMARContents.Text = "00000000";
            lblMARContents.ForeColor = Color.Red;

            lblRAMContents.Text = "00000000";
            lblRAMContents.ForeColor = Color.Red;

            lblRAMAddress.Text = "Addr 0";

            lblTMPContents.Text = "00000000";
            lblTMPContents.ForeColor = Color.Red;

            lblAccContents.Text = "00000000";
            lblAccContents.ForeColor = Color.Red;

            lblCoutContents.ForeColor = Color.Red;
            lblCoutContents.Text = "0";

            lblALargerContents.ForeColor = Color.Red;
            lblALargerContents.Text = "0";

            lblEqualContents.ForeColor = Color.Red;
            lblEqualContents.Text = "0";

            lblZeroContents.ForeColor = Color.Red;
            lblZeroContents.Text = "0";

            for (int count = 0; count < Globals.NO_OF_GPR; count++)
            {
                lblGPRContents[count].Text = "00000000";
                lblGPRContents[count].ForeColor = Color.Red;
                pnlGPR[count].Refresh();
            }

            pnlIR.Refresh();
            pnlIAR.Refresh();
            pnlMAR.Refresh();
            pnlRAM.Refresh();
            pnlTMP.Refresh();
            pnlAcc.Refresh();
            pnlFlags.Refresh();
        }
        //-------------------------------------
    }

    public static class Globals
    {
        public static int CLOCK_SPEED = 0;

        //instruction format
        //---------------------------------------------------------------------------------------------------------------
        public const int WORD_SIZE = 8;
        public const int OPCODE_SIZE = WORD_SIZE / 2;
        public const int GPR_ADDRESS_SIZE = 2;

        public const int WORD_START = WORD_SIZE - 1;        //word stored as big endian
        public const int OPCODE_START = OPCODE_SIZE - 1;    //opcode also big endian
        public const int ALU_OPCODE = OPCODE_START;

        public const string OP_LOAD = "0000", OP_STORE = "0001", OP_DATA = "0010", OP_JUMP_RG = "0011", 
                            OP_JUMP = "0100", OP_JUMP_IF = "0101", OP_RESET_FLAGS = "0110", OP_IO = "0111",

                            ALU_ADD = "1000", ALU_R_SHIFT = "1001", ALU_L_SHIFT = "1010", ALU_NOT = "1011", 
                            ALU_AND = "1100", ALU_OR = "1101", ALU_XOR = "1110", ALU_COMPARE = "1111";

        public const int ZERO_FLAG = 3, A_LARGER_FLAG = 2, EQUAL_FLAG = 1, COUT_FLAG = 0;
        //---------------------------------------------------------------------------------------------------------------

        //available memory
        //-------------------------------
        public const int RAM_SIZE = 256;
        public const int NO_OF_GPR = 4;
        public const int NO_OF_FLAGS = 4;
        //-------------------------------

        //access modes
        //---------------------------------------------------------------------------------------------------------
        public static readonly Color WRITE_COLOR = Color.Red, READ_COLOR = Color.DodgerBlue, BUS_COLOR = Color.Red;
        public const bool REGISTER_READ = true, REGISTER_WRITE = false;
        //---------------------------------------------------------------------------------------------------------

        //methods
        public static BitArray reverseBitArray(BitArray data)
        {
            BitArray reversedData = new BitArray(data.Length);

            for (int reverseCount = data.Length - 1, count = 0; reverseCount >= 0 && count < data.Length; reverseCount--, count++)
                reversedData[count] = data[reverseCount];

            return reversedData;
        }

        public static bool areBitsEqual(BitArray one, BitArray two)
        {
            bool equal = true;

            for (int count = 0; count < WORD_SIZE && equal; count++)
                if (one[count] != two[count]) equal = false;

            return equal;
        }

        public static string convertBitsToString(BitArray data)
        {
            char[] result = new char[data.Length];
            
            //data should be displayed in little endian form
            data = reverseBitArray(data);
            for (int count = 0; count < data.Length; count++)
            {
                if (data[count]) result[count] = '1';
                else result[count] = '0';
            }

            return new string(result);
        }

        public static byte convertBitsToByte(BitArray data)
        {
            byte[] result = new byte[1];
            data.CopyTo(result, 0);
            return result[0];
        }
    }

    public class Register
    {
        //PRIVATE
        private BitArray contents = new BitArray(Globals.WORD_SIZE);

        //PUBLIC
        public event ReadWriteRegister ReadWriteContents;

        //no event assigned, used for registers that require specialised events (i.e. RAM and GPRs need an address)
        public Register() { }

        //event assigned for registers that have generic read and write events (i.e IR, MAR etc., don't require an address)
        public Register(ReadWriteRegister readWriteContents) { ReadWriteContents += readWriteContents; }

        //access contents without invoking event
        public BitArray getContents() { return contents; }
        public void resetContents() { contents = new BitArray(new byte[] { 0 }); }

        public void overwriteContents(BitArray contents)
        {
            ReadWriteContents?.Invoke(contents, Globals.REGISTER_WRITE);
            if (ReadWriteContents != null) Thread.Sleep(Globals.CLOCK_SPEED);
            this.contents = contents;
        }

        public BitArray readContents()
        {
            ReadWriteContents?.Invoke(null, Globals.REGISTER_READ);
            return contents;
        }

    }

    public class RAM
    {
        //PRIVATE
        private Register[] contents = new Register[Globals.RAM_SIZE];

        //PUBLIC
        public event ReadWriteMemory ReadWriteContents;

        public RAM(BitArray[] instructions, ReadWriteMemory readWriteContents)
        {
            ReadWriteContents += readWriteContents;

            //load instructions into memory
            for (int count = 0; count < instructions.Length; count++)
            {
                contents[count] = new Register();
                contents[count].overwriteContents(instructions[count]);
            }

            //create instances of empty memory where instructions end
            //e.g. last instruction loc 34, 35-255 will be empty
            for (int count = instructions.Length; count < Globals.RAM_SIZE; count++)
                contents[count] = new Register();
        }

        public BitArray readFromLocation(BitArray address)
        {
            byte addr = Globals.convertBitsToByte(address);
            ReadWriteContents?.Invoke(contents[addr].readContents(), addr, Globals.REGISTER_READ);
            return contents[addr].readContents();
        }

        public void writeToLocation(BitArray address, BitArray data)
        {
            byte addr = Globals.convertBitsToByte(address);
            contents[addr].overwriteContents(data);
            ReadWriteContents?.Invoke(data, addr, Globals.REGISTER_WRITE);
            if (ReadWriteContents != null) Thread.Sleep(Globals.CLOCK_SPEED);
        }
    }

    public class MAR : Register
    {
        //PRIVATE
        private RAM RAM;

        //PUBLIC
        public MAR(BitArray[] instructions, ReadWriteMemory readWriteRAM, ReadWriteRegister readWriteContents)
            : base(readWriteContents) { RAM = new RAM(instructions, readWriteRAM); }

        //use MAR to address and access a location in memory
        public void writeToMemory(BitArray data) { RAM.writeToLocation(getContents(), data); }
        public BitArray readFromMemory() { return RAM.readFromLocation(getContents()); }
    }

    public class ALU
    {
        //PRIVATE
        private bool BUS1 = false;

        //PUBLIC
        public event ReadOnlyRegister ReadBUS1;
        public event ReadWriteFlags ReadWriteFlags;

        public bool[] flags = new bool[Globals.NO_OF_FLAGS];
        public Register TMP;

        public ALU(ReadWriteRegister readWriteTMP, ReadOnlyRegister readBUS1, ReadWriteFlags readWriteFlags)
        {
            TMP = new Register(readWriteTMP);

            //flags
            for (int count = 0; count < Globals.NO_OF_FLAGS; count++) flags[count] = false;

            ReadBUS1 += readBUS1;
            ReadWriteFlags += readWriteFlags;
        }

        public void toggleBUS1()
        {
            if (BUS1) BUS1 = false;
            else
            {
                ReadBUS1?.Invoke();
                BUS1 = true;
            }
        }

        public BitArray add(BitArray firstNumber)
        {
            BitArray secondNumber, result = new BitArray(Globals.WORD_SIZE);

            if (BUS1) secondNumber = new BitArray(new byte[] { 1 });    //ignore TMP input, output value 1
            else secondNumber = new BitArray(TMP.readContents());       //read contents of TMP

            bool carryOut = false;
            for (int count = 0; count < Globals.WORD_SIZE; count++)     //add from LSB upwards
            {
                //last add caused a carry
                if (carryOut)
                {
                    //1 no carry
                    if (firstNumber[count] == false && secondNumber[count] == false)
                    {
                        result[count] = true;
                        carryOut = false;
                    }

                    //1 carry
                    else if (firstNumber[count] == true && secondNumber[count] == true)
                        result[count] = true;

                    //0 carry
                    else result[count] = false;
                }

                else
                {
                    //0 no carry
                    if (firstNumber[count] == false && secondNumber[count] == false)
                        result[count] = false;

                    //0 carry
                    else if (firstNumber[count] == true && secondNumber[count] == true)
                    {
                        result[count] = false;
                        carryOut = true;
                    }

                    //1 no carry
                    else result[count] = true;
                }
            }

            //last add caused carry
            if (carryOut)
            {
                ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.COUT_FLAG);
                flags[Globals.COUT_FLAG] = true;
            }

            //result was 0
            if (Globals.areBitsEqual(result, new BitArray(Globals.WORD_SIZE)))
            {
                ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.ZERO_FLAG);
                flags[Globals.ZERO_FLAG] = true;
            }

            return result;
        }

        public BitArray shiftRight(BitArray data)
        {
            BitArray result = new BitArray(Globals.WORD_SIZE);

            //LSB is 1, will be shifted out
            if (data[0])
            {
                ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.COUT_FLAG);
                flags[Globals.COUT_FLAG] = true;
            }

            //shift
            for (int front = 1, back = 0; front < Globals.WORD_SIZE; front++, back++)
                result[back] = data[front];

            //result was 0
            if (Globals.areBitsEqual(result, new BitArray(Globals.WORD_SIZE)))
            {
                ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.ZERO_FLAG);
                flags[Globals.ZERO_FLAG] = true;
            }

            return result;
        }

        public BitArray shiftLeft(BitArray data)
        {
            BitArray result = new BitArray(Globals.WORD_SIZE);

            //MSB is 1, will be shifted out
            if (data[Globals.WORD_START])
            {
                ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.COUT_FLAG);
                flags[Globals.COUT_FLAG] = true;
            }

            //shift
            for (int front = 1, back = 0; front < Globals.WORD_SIZE; front++, back++)
                result[front] = data[back];

            //result was 0
            if (Globals.areBitsEqual(result, new BitArray(Globals.WORD_SIZE)))
            {
                ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.ZERO_FLAG);
                flags[Globals.ZERO_FLAG] = true;
            }

            return result;
        }

        public BitArray inverse(BitArray data)
        {
            BitArray result = data.Not();

            //result was 0
            if (Globals.areBitsEqual(result, new BitArray(Globals.WORD_SIZE)))
            {
                ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.ZERO_FLAG);
                flags[Globals.ZERO_FLAG] = true;
            }

            return result;
        }

        public BitArray and(BitArray firstNumber)
        {
            BitArray result = firstNumber.And(TMP.readContents());

            //result was 0
            if (Globals.areBitsEqual(result, new BitArray(Globals.WORD_SIZE)))
            {
                ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.ZERO_FLAG);
                flags[Globals.ZERO_FLAG] = true;
            }

            return result;
        }

        public BitArray or(BitArray firstNumber)
        {
            BitArray result = firstNumber.Or(TMP.readContents());

            //result was 0
            if (Globals.areBitsEqual(result, new BitArray(Globals.WORD_SIZE)))
            {
                ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.ZERO_FLAG);
                flags[Globals.ZERO_FLAG] = true;
            }

            return result;
        }

        public BitArray xor(BitArray firstNumber)
        {
            BitArray result = firstNumber.Xor(TMP.readContents());

            //result was 0
            if (Globals.areBitsEqual(result, new BitArray(Globals.WORD_SIZE)))
            {
                ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.ZERO_FLAG);
                flags[Globals.ZERO_FLAG] = true;
            }

            return result;
        }

        public void compare(BitArray firstNumber)
        {
            BitArray secondNumber = new BitArray(TMP.readContents());
            bool isequal = true;

            int count = Globals.WORD_START;

            //examine MSB downward, first unequal bits breaks loop
            while (isequal && count >= 0)
            {
                //first > second
                if (firstNumber[count] && !secondNumber[count])
                {
                    isequal = false;

                    ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.A_LARGER_FLAG);
                    flags[Globals.A_LARGER_FLAG] = true;
                }

                //first < second
                else if (!firstNumber[count] && secondNumber[count])
                    isequal = false;

                count--;
            }

            if (isequal)
            {
                ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE, Globals.EQUAL_FLAG);
                flags[Globals.EQUAL_FLAG] = true;
            }
        }
    }

    public class ControlUnit
    {
        //PRIVATE
        private bool programFinished = false;                               //stop execution when true
        private BitArray opcode = new BitArray(Globals.OPCODE_SIZE);        //instruction to execute
        private BitArray flagCondition = new BitArray(Globals.NO_OF_FLAGS); //parameters for a jump if
        private byte registerA, registerB;                                  //address of GPRs in use by current instruction

        private ALU ALU;

        private MAR MAR;
        private Register IAR, IR, ACC;
        private Register[] GPR = new Register[Globals.NO_OF_GPR];

        //PUBLIC
        public event ReadWriteMemory ReadWriteGPR;      //when invoked, GUI will update GPR with its new data (write), or indicate it is being accessed (read)
        public event ReadWriteFlags ReadWriteFlags;     //same as above, but for flags
        public event ALUOperation RunALUOperation;      //the ALU opcode bits will pass to the ALU opcode wires on the GUI
        public event RedrawGUI ResetControlBits;        //GUI will turn off all control bits (set, enable, opcodes)

        //tracks the last executable instruction in memory
        public readonly BitArray lastInstruction = new BitArray(Globals.WORD_SIZE);

        //initialise all CPU components
        public ControlUnit                                                                                                  
            (BitArray[] instructions, ReadWriteMemory readWriteRAM, byte[] lastInstruction, ReadWriteMemory readWriteGPR,   
            ReadWriteRegister readWriteIAR, ReadWriteRegister readWriteIR, ReadWriteRegister readWriteMAR,                  
            ReadWriteRegister readWriteTMP, ReadOnlyRegister readBUS1, ALUOperation runALUOperation,                                  
            ReadWriteRegister readWriteAcc, ReadWriteFlags readWriteFlags, RedrawGUI resetControlBits)
        {
            //create new instance of each component
            //------------------------------------------------------
            ALU = new ALU(readWriteTMP, readBUS1, readWriteFlags);
            ACC = new Register(readWriteAcc);
            MAR = new MAR(instructions, readWriteRAM, readWriteMAR);
            IAR = new Register(readWriteIAR);
            IR = new Register(readWriteIR);

            //GPRs
            for (int count = 0; count < Globals.NO_OF_GPR; count++)
                GPR[count] = new Register();
            //------------------------------------------------------

            this.lastInstruction = new BitArray(lastInstruction);

            //link methods to events
            //-----------------------------------
            ReadWriteGPR += readWriteGPR;           //invoke when the CPU needs to read or write data to a GPR
            ReadWriteFlags += readWriteFlags;       //same as above, but for a flag
            RunALUOperation += runALUOperation;     //invoke when the CPU runs an ALU instruction
            ResetControlBits += resetControlBits;   //invoke at the end of an instruction step (i.e. writing to a register, resetting flags etc.)
            //-----------------------------------
        }

        public void start()
        {
            //reset eveything to 0
            if (programFinished)
            {
                IR.resetContents();
                IAR.resetContents();
                MAR.resetContents();
                ALU.TMP.resetContents();
                ACC.resetContents();

                for (int count = 0; count < Globals.NO_OF_GPR; count++)
                    GPR[count].resetContents();

                for (int count = 0; count < Globals.NO_OF_FLAGS; count++) ALU.flags[count] = false;

                programFinished = false;
            }

            //fetch execute loop
            while (!programFinished)
            {
                fetchInstruction();
                decodeInstruction();
                executeInstruction();
            }

            MessageBox.Show("End of program");
        }

        public void triggerRestart() { programFinished = true; }

        public void fetchInstruction()
        {
            prepareInstruction();
            incrementIAR();
            setInstructionRegister();
        }

        //point MAR to instruction to execute
        private void prepareInstruction()
        {
            MAR.overwriteContents(IAR.readContents());          //copy next instruction address to MAR
            ResetControlBits?.Invoke();                         
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //point IAR to next instruction
        private void incrementIAR()
        {
            //IAR is at last executable instruction, program will finish after this instruction
            if (Globals.areBitsEqual(IAR.getContents(), lastInstruction))
                programFinished = true;

            ALU.toggleBUS1();                                   //enable BUS1 (ALU input B becomes 1)
            ACC.overwriteContents(ALU.add(IAR.readContents())); //add IAR and B (increments value by 1)
            ALU.toggleBUS1();                                   //disable BUS1

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            IAR.overwriteContents(ACC.readContents());          //update IAR with new value

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //get instruction to execute
        private void setInstructionRegister()
        {
            //access next instruction in memory and copy to IR
            IR.overwriteContents(MAR.readFromMemory());         

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //divide instruction into opcode and addresses/parameters
        private void decodeInstruction()
        {
            BitArray IR = this.IR.readContents(),
                registerA = new BitArray(Globals.GPR_ADDRESS_SIZE),
                registerB = new BitArray(Globals.GPR_ADDRESS_SIZE);

            //by copying IR contents, invoked read event
            Thread.Sleep(Globals.CLOCK_SPEED);
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //splice opcode from instruction
            for (int currentBit = Globals.OPCODE_SIZE, opcodeBit = 0; currentBit < Globals.WORD_SIZE; currentBit++, opcodeBit++)
                opcode[opcodeBit] = IR[currentBit];

            string opcodeAsString = Globals.convertBitsToString(opcode);

            //non opcode bits flag parameters
            if (opcodeAsString == Globals.OP_JUMP_IF)
            {
                //splice flag parameters from instruction
                for (int currentBit = 0; currentBit < Globals.NO_OF_FLAGS; currentBit++)
                    flagCondition[currentBit] = IR[currentBit];
            }

            //non opcode bits GPR addresses
            else
            {
                //splice register address from instruction
                for (int currentBit = 0, registerAIndex = Globals.GPR_ADDRESS_SIZE, registerBIndex = 0;
                    registerBIndex < Globals.GPR_ADDRESS_SIZE; currentBit++, registerAIndex++, registerBIndex++)
                {
                    registerA[currentBit] = IR[registerAIndex];
                    registerB[currentBit] = IR[registerBIndex];
                }

                //convert to decimal (so it can index GPR array)
                this.registerA = Globals.convertBitsToByte(registerA);
                this.registerB = Globals.convertBitsToByte(registerB);
            }
        }
        
        //call method that matches the opcode
        private void executeInstruction()
        {
            string opcodeAsString = Globals.convertBitsToString(opcode);

            //ALU opcode bit active, is an ALU instruction
            //--------------------------------------------------------------------------------------------------------------------------
            if (opcode[Globals.ALU_OPCODE])
            {
                //2 input operation, prep TMP
                if (opcodeAsString != Globals.ALU_R_SHIFT && opcodeAsString != Globals.ALU_L_SHIFT && opcodeAsString != Globals.ALU_NOT)
                {
                    ReadWriteGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_READ);   //invoke GPR read
                    ALU.TMP.overwriteContents(GPR[registerB].readContents());                               //copy register B to TMP

                    ResetControlBits?.Invoke();
                    if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

                }

                //run ALU operation and provide contents of register A as input
                //-------------------------------------------------------------------------------------------
                RunALUOperation?.Invoke(opcode);

                switch (opcodeAsString)
                {
                    case Globals.ALU_ADD:
                        ReadWriteGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
                        ACC.overwriteContents(ALU.add(GPR[registerA].readContents()));
                        break;

                    case Globals.ALU_R_SHIFT:
                        ReadWriteGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
                        ACC.overwriteContents(ALU.shiftRight(GPR[registerA].readContents()));
                        break;

                    case Globals.ALU_L_SHIFT:
                        ReadWriteGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
                        ACC.overwriteContents(ALU.shiftLeft(GPR[registerA].readContents()));
                        break;

                    case Globals.ALU_NOT:
                        ReadWriteGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
                        ACC.overwriteContents(ALU.inverse(GPR[registerA].readContents()));
                        break;

                    case Globals.ALU_AND:
                        ReadWriteGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
                        ACC.overwriteContents(ALU.and(GPR[registerA].readContents()));
                        break;

                    case Globals.ALU_OR:
                        ReadWriteGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
                        ACC.overwriteContents(ALU.or(GPR[registerA].readContents()));
                        break;

                    case Globals.ALU_XOR:
                        ReadWriteGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
                        ACC.overwriteContents(ALU.xor(GPR[registerA].readContents()));
                        break;

                    case Globals.ALU_COMPARE:
                        ReadWriteGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
                        ALU.compare(GPR[registerA].readContents());
                        Thread.Sleep(Globals.CLOCK_SPEED);
                        break;

                    default:
                        MessageBox.Show("Invalid opcode");
                        break;
                }
                //-------------------------------------------------------------------------------------------

                ResetControlBits?.Invoke();
                if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

                //compare operation doesn't output to accumulator
                if (opcodeAsString != Globals.ALU_COMPARE)
                {
                    //copy accumulator into register B
                    GPR[registerB].overwriteContents(ACC.readContents());
                    ReadWriteGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_WRITE);
                    Thread.Sleep(Globals.CLOCK_SPEED);

                    ResetControlBits?.Invoke();
                    if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
                }
            }
            //--------------------------------------------------------------------------------------------------------------------------

            //control unit instruction
            //--------------------------------------------------------------------
            else
            {
                switch (opcodeAsString)
                {
                    case Globals.OP_LOAD:
                        load();
                        break;

                    case Globals.OP_STORE:
                        store();
                        break;

                    case Globals.OP_DATA:
                        data();
                        break;

                    case Globals.OP_JUMP_RG:
                        jumpRegister();
                        break;

                    case Globals.OP_JUMP:
                        jump();
                        break;

                    case Globals.OP_JUMP_IF:
                        jumpIf();
                        break;

                    case Globals.OP_RESET_FLAGS:
                        resetFlags();
                        break;

                    case Globals.OP_IO:
                        //IO
                        break;
                }
                //--------------------------------------------------------------------
            }
        }

        //save data to register B, data located in RAM by register A
        private void load()
        {
            //prep MAR with address in register A
            ReadWriteGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
            MAR.overwriteContents(GPR[registerA].readContents());

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //copy data from memory address to register B
            GPR[registerB].overwriteContents(MAR.readFromMemory());
            ReadWriteGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_WRITE);
            Thread.Sleep(Globals.CLOCK_SPEED);

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //save data to RAM, location specified by register A, data specified by register B
        private void store()
        {
            //prep MAR with address in register A
            ReadWriteGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
            MAR.overwriteContents(GPR[registerA].readContents());

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //copy data from register B to memory
            ReadWriteGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_READ);
            MAR.writeToMemory(GPR[registerB].readContents());

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //save data to register B, data located in next RAM address
        private void data()
        {
            //prep MAR with address in IAR
            MAR.overwriteContents(IAR.readContents());

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //point IAR to next instruction after data
            incrementIAR();

            //copy data from memory address to register B
            GPR[registerB].overwriteContents(MAR.readFromMemory());
            ReadWriteGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_WRITE);
            Thread.Sleep(Globals.CLOCK_SPEED);

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //jump to address stored in register B
        private void jumpRegister()
        {
            //copy register B to IAR
            ReadWriteGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_READ);
            IAR.overwriteContents(GPR[registerB].readContents());

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //jump to address stored in next memory location
        private void jump()
        {
            //prep MAR with address in IAR
            MAR.overwriteContents(IAR.readContents());

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //copy next address to IAR
            IAR.overwriteContents(MAR.readFromMemory());

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //jump to address stored in next memory location if condition is met
        private void jumpIf()
        {
            if (flagCondition.Length == Globals.NO_OF_FLAGS)
            {
                bool conditionmet = true;

                //invoke flag read event
                ReadWriteFlags?.Invoke(Globals.REGISTER_READ);
                Thread.Sleep(Globals.CLOCK_SPEED);

                ResetControlBits?.Invoke();
                if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

                //compare state of flags with condition
                for (int count = 0; count < Globals.NO_OF_FLAGS && conditionmet; count++)
                    if (flagCondition[count] && !ALU.flags[count]) conditionmet = false;

                if (conditionmet) jump();
                else incrementIAR();        //step over jump address
            }

            else MessageBox.Show("Invalid condition");
        }

        private void resetFlags()
        {
            //flag reset event
            ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE);
            Thread.Sleep(Globals.CLOCK_SPEED);

            ResetControlBits?.Invoke();
            Thread.Sleep(Globals.CLOCK_SPEED);

            //set all flags to 0
            for (int count = 0; count < Globals.NO_OF_FLAGS; count++) ALU.flags[count] = false;
        }
    }
}