using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace CPU_Simulator
{
    public delegate void ReadOnlyRegister();                                            //enable BUS1 output, enable TOS onto bus
    public delegate void ReadWriteRegister(BitArray data, bool accessMode);             //modify contents of IR, IAR, MAR, TMP etc.
    public delegate void ReadWriteMemory(BitArray data, int index, bool accessMode);    //modify contents of register which require an address (i.e. RAM)
    public delegate void ReadWriteFlags(bool accessMode, int index = -1);               //modify contents of flags

    public delegate void ReadWriteStack(bool accessMode, int stackIndex);               //push and pop to stack

    public delegate void ALUOperation(BitArray opcode);                                 //indicate ALU in use                     
    public delegate void RedrawGUI();                                                   //redraw before next step

    public partial class MainForm : Form
    {
        private ControlUnit CU;
        private Thread CPU;

        //indicates which machine in use (register = true) (stack = false)
        private bool registerMachine;

        private BitArray[] instructions;
        private int lastInstruction;
        private bool halt;

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
        Button btnShowInstructions = new Button();

        //CPU components
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

        private Panel pnlStack = new Panel();
        private Label[] lblStack = new Label[Globals.STACK_SIZE];
        private Label[] lblStackContents = new Label[Globals.STACK_SIZE];

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

        private Panel pnlTMP = new Panel();
        private Label lblTMP = new Label();
        private Label lblTMPContents = new Label();

        private Panel pnlBUS1 = new Panel();
        private Label lblBUS1 = new Label();

        private Panel pnlAcc = new Panel();
        private Label lblAcc = new Label();
        private Label lblAccContents = new Label();

        //draw methods
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
                Controls.Add(btnShowInstructions);

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

                btnShowInstructions.Text = "Supported Instructions";
                btnShowInstructions.FlatStyle = FlatStyle.Popup;
                btnShowInstructions.BackColor = Color.LightGray;
                btnShowInstructions.Location = new Point(830, 445);
                btnShowInstructions.Size = new Size(130, 25);
                btnShowInstructions.Click += new EventHandler(btnShowInstructions_Click);

                Controls.Add(btnShowInstructions);

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
            Controls.Remove(btnShowInstructions);
        }

        private void drawRegisterCPU()
        {
            drawBus();
            drawRegisterCircuit();
            drawComponents(registerScreenDrawn);

            lblTMP.Text = "TMP";

            //GPRs already created, just add back to the form (faster draw speed)
            if (registerScreenDrawn)
            {
                for (int count = 0; count < Globals.NO_OF_GPR; count++)
                    Controls.Add(pnlGPR[count]);
            }

            //create and add GPRs to the form
            else
            {
                registerScreenDrawn = true;

                for (int count = 0, yPos = 160; count < Globals.NO_OF_GPR; count++, yPos += 60)
                {
                    pnlGPR[count] = new Panel();
                    pnlGPR[count].BorderStyle = BorderStyle.FixedSingle;
                    pnlGPR[count].Location = new Point(655, yPos);
                    pnlGPR[count].Size = new Size(80, 50);

                    Controls.Add(pnlGPR[count]);

                    lblGPR[count] = new Label();
                    lblGPR[count].Text = "RG" + (count + 1).ToString();
                    lblGPR[count].Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                    lblGPR[count].TextAlign = ContentAlignment.MiddleCenter;
                    lblGPR[count].Location = new Point(20, 4);
                    lblGPR[count].Size = new Size(41, 20);

                    pnlGPR[count].Controls.Add(lblGPR[count]);

                    lblGPRContents[count] = new Label();
                    lblGPRContents[count].Text = "00000000";
                    lblGPRContents[count].TextAlign = ContentAlignment.MiddleCenter;
                    lblGPRContents[count].BorderStyle = BorderStyle.FixedSingle;
                    lblGPRContents[count].Location = new Point(10, 26);
                    lblGPRContents[count].Size = new Size(60, 20);

                    pnlGPR[count].Controls.Add(lblGPRContents[count]);
                }
            }

        }

        private void drawStackCPU()
        {
            drawBus();
            drawStackCircuit();
            drawComponents(stackScreenDrawn);

            lblTMP.Text = "TOS";

            //stack already created, just add back to the form (faster draw speed)
            if (stackScreenDrawn)
            {
                Controls.Add(pnlStack);
            }

            //create and add stack to the form
            else
            {
                stackScreenDrawn = true;

                pnlStack.BorderStyle = BorderStyle.FixedSingle;
                pnlStack.Location = new Point(645, 170);
                pnlStack.Size = new Size(95, 210);

                Controls.Add(pnlStack);

                for (int count = 0, yPos = 5; count < Globals.STACK_SIZE; count++, yPos += 25)
                {
                    lblStack[count] = new Label();
                    lblStack[count].Text = count.ToString();
                    lblStack[count].TextAlign = ContentAlignment.MiddleCenter;
                    lblStack[count].BorderStyle = BorderStyle.FixedSingle;
                    lblStack[count].Location = new Point(5, yPos);
                    lblStack[count].Size = new Size(20, 20);

                    pnlStack.Controls.Add(lblStack[count]);

                    lblStackContents[count] = new Label();
                    lblStackContents[count].Text = "00000000";
                    lblStackContents[count].TextAlign = ContentAlignment.MiddleCenter;
                    lblStackContents[count].BorderStyle = BorderStyle.FixedSingle;
                    lblStackContents[count].Location = new Point(30, yPos);
                    lblStackContents[count].Size = new Size(60, 20);

                    pnlStack.Controls.Add(lblStackContents[count]);
                }
            }
        }

        private void drawComponents(bool screenDrawn)
        {
            //object properties already calculated, just add back to the form (faster draw speed)
            if (screenDrawn)
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
            }
        }

        private void hideRegisterCPU()
        {
            hideComponents();

            for (int count = 0; count < Globals.NO_OF_GPR; count++)
                Controls.Remove(pnlGPR[count]);
        }

        private void hideStackCPU()
        {
            hideComponents();
            Controls.Remove(pnlStack);
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
        }

        private void drawRegisterCircuit()
        {
            Graphics circuit = CreateGraphics();
            Pen controlWire = new Pen(Color.Black);
            controlWire.Width = 2;

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

            controlWire.Dispose();
            circuit.Dispose();

            drawControlBits();
        }

        private void drawStackCircuit()
        {
            Graphics circuit = CreateGraphics();
            Pen controlWire = new Pen(Color.Black);
            controlWire.Width = 2;

            circuit.DrawLine(controlWire, 655, 190, 625, 190);  //enable
            circuit.DrawLine(controlWire, 655, 185, 625, 185);  //set

            controlWire.Dispose();
            circuit.Dispose();

            drawControlBits();
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

            //GPR1/Stack
            circuit.DrawLine(bus, 735, 182.5f, 760, 182.5f);
            circuit.DrawLine(bus, 735, 187.5f, 760, 187.5f);

            if (registerMachine)
            {
                //GPR2
                circuit.DrawLine(bus, 735, 242.5f, 760, 242.5f);
                circuit.DrawLine(bus, 735, 247.5f, 760, 247.5f);

                //GPR3
                circuit.DrawLine(bus, 735, 302.5f, 760, 302.5f);
                circuit.DrawLine(bus, 735, 307.5f, 760, 307.5f);

                //GPR4
                circuit.DrawLine(bus, 735, 362.5f, 760, 362.5f);
                circuit.DrawLine(bus, 735, 367.5f, 760, 367.5f);
            }

            circuit.Dispose();
            bus.Dispose();
        }

        //control methods
        private void btnRegister_Click(object sender, EventArgs e)
        {
            ClientSize = new Size(1137, 489);
            Text = "Register CPU Simulator";

            registerMachine = true;

            hideWelcomeScreen();
            drawRegisterCPU();
            drawUI();
        }

        private void btnStack_Click(object sender, EventArgs e)
        {
            ClientSize = new Size(1137, 489);
            Text = "Stack CPU Simulator";

            registerMachine = false;

            hideWelcomeScreen();
            drawStackCPU();
            drawUI();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //CPU is not active
            if (CPU == null || CPU.ThreadState == ThreadState.Aborted || CPU.ThreadState == ThreadState.Stopped)
            {
                //CPU has previously run
                if (CPU != null)
                {
                    CU.resetState();
                    resetComponents();

                    if (CPU.ThreadState == ThreadState.Aborted)
                        btnPauseStop.Text = "Pause";

                    Thread.Sleep(Globals.CLOCK_SPEED);

                    resetColours();
                }

                if (compileAndRun())
                {
                    CPU = new Thread(new ThreadStart(CU.start));
                    CPU.Start();
                }
            }

            //CPU is paused
            else if (CPU.ThreadState == ThreadState.Suspended)
            {
                CPU.Resume();
                btnPauseStop.Text = "Pause";
            }
        }

        private void btnPauseStop_Click(object sender, EventArgs e)
        {
            //no thread active
            if (CPU == null || 
                CPU.ThreadState == ThreadState.Stopped)
                MessageBox.Show("CPU not currently running.");

            //thread paused
            else if (CPU.ThreadState == ThreadState.Suspended)
            {
                CPU.Resume();
                CPU.Abort();
                btnPauseStop.Text = "Stopped";
            }

            //thread active
            else if (CPU.ThreadState == ThreadState.Running || 
                CPU.ThreadState == ThreadState.WaitSleepJoin)
            {
                CPU.Suspend();
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
            if (CPU != null)
            {
                //thread running
                if (CPU.ThreadState == ThreadState.Running ||
                    CPU.ThreadState == ThreadState.WaitSleepJoin)
                {
                    CPU.Abort();
                }

                //cannot abort suspended thread
                else if (CPU.ThreadState == ThreadState.Suspended)
                {
                    CPU.Resume();
                    CPU.Abort();
                }
            }

            hideUI();

            if (registerMachine) hideRegisterCPU();
            else hideStackCPU();

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

        private void btnShowInstructions_Click(object sender, EventArgs e)
        {
            InstructionForm instructionTable = new InstructionForm(registerMachine);
            instructionTable.Show();
        }

        private void btnLoadCode_Click(object sender, EventArgs e)
        {
            //open file explorer and search for text files
            OpenFileDialog fileExplorer = new OpenFileDialog() { Filter = "Text files (*.txt)|*.txt" };
            fileExplorer.ShowDialog();

            //load selected file into string
            if (fileExplorer.FileName != "")
            {
                string program = System.IO.File.ReadAllText(fileExplorer.FileName);

                //load string into text box
                txtCodeEditor.Text = program;
                txtCodeEditor.ForeColor = Color.Black;
            }
        }

        private void btnSaveCode_Click(object sender, EventArgs e)
        {
            string[] program = new string[txtCodeEditor.Lines.Length];

            //read each line in text box
            for (int currentLine = 0; currentLine < program.Length; currentLine++)
                program[currentLine] = txtCodeEditor.Lines[currentLine];

            //open file explorer to save a text file
            SaveFileDialog fileExplorer = new SaveFileDialog() { Filter = "Text files (*.txt)|*.txt" };
            fileExplorer.ShowDialog();

            //write program to file selected
            if (fileExplorer.FileName != "")
                System.IO.File.WriteAllLines(fileExplorer.FileName, program);
        }

        //loading methods
        private bool compileAndRun()
        {
            BitArray instruction = new BitArray(Globals.WORD_SIZE);
            string currentInstruction = "";
            bool compiled = true;

            halt = false;

            //create empty program
            instructions = new BitArray[Globals.RAM_SIZE];

            //read each line of text box until the end or there is a problem
            for (int currentLine = 0; currentLine < txtCodeEditor.Lines.Length && compiled; currentLine++)
            {
                currentInstruction = txtCodeEditor.Lines[currentLine];  //get current instruction

                //check if written as binary
                bool isBinary = true;
                string temp = "";
                for (int currentBit = 0; currentBit < currentInstruction.Length; currentBit++)
                {
                    if (currentInstruction[currentBit] != ' ' && currentInstruction[currentBit] != '0' && currentInstruction[currentBit] != '1')
                        isBinary = false;

                    else if (currentInstruction[currentBit] == '1')
                        temp += 1;

                    else if (currentInstruction[currentBit] == '0')
                        temp += 0;
                }

                //instruction is binary, convert to bitarray
                if (isBinary)
                {
                    currentInstruction = temp;

                    //small enough to fit in memory
                    if (currentInstruction.Length <= Globals.WORD_SIZE)
                    {
                        //fill remaining bits as 0s
                        if (currentInstruction.Length < Globals.WORD_SIZE)
                        {
                            string blanks = "";
                            for (int currentBit = currentInstruction.Length; currentBit < Globals.WORD_SIZE; currentBit++)
                                blanks += '0';

                            blanks += currentInstruction;   //put 0s before value
                            currentInstruction = blanks;    //copy to instruction
                        }

                        instruction = Globals.convertStringToBits(currentInstruction);
                    }

                    else
                    {
                        MessageBox.Show("Instruction at line: " + currentLine + " is larger than the supported size.");
                        compiled = false;
                    }
                }

                else isBinary = false;

                //convert assembly to binary
                if (!isBinary) instruction = convertToBinary(currentInstruction, currentLine);

                //add instruction to program
                if (instruction != null) instructions[currentLine] = instruction;
                else compiled = false;
            }

            if (!halt && compiled)
            {
                MessageBox.Show("No halt instruction used. Use 'HLT' after instruction you wish to stop at." );
                compiled = false;
            }

            if (compiled)
            {
                if (registerMachine) loadRegisterCPU();
                else loadStackCPU();
            }

            return compiled;
        }

        private BitArray convertToBinary(string instruction, int lineNo)
        {
            BitArray newInstruction = null;
            string opcode = "", parameters = "", fullInstruction = "";
            bool parametersBinary = true;

            //get opcode
            for (int opcodeBit = 0; opcodeBit < instruction.Length && instruction[opcodeBit] != ' '; opcodeBit++)
                    opcode += instruction[opcodeBit];

            //get parameters
            for (int addressBit = opcode.Length + 1; addressBit < instruction.Length; addressBit++)
            {
                if (instruction[addressBit] != ' ')
                {
                    if (instruction[addressBit] != '1' && instruction[addressBit] != '0')
                        parametersBinary = false;

                    parameters += instruction[addressBit];
                }
            }

            //valid opcode
            if (parametersBinary && Globals.keywords.ContainsKey(opcode))
            {
                //following instructions require 4 bit parameters (register A and B or flags)
                if (parameters.Length != Globals.GPR_ADDRESS_SIZE * 2 &&

                    //load, store and all ALU instructions (register model only)
                    ((registerMachine && 
                    (Globals.keywords[opcode] == Globals.OP_LOAD_POP || 
                    Globals.keywords[opcode] == Globals.OP_STORE_SWAP ||
                    Globals.keywords[opcode] == Globals.ALU_ADD ||
                    Globals.keywords[opcode] == Globals.ALU_R_SHIFT || 
                    Globals.keywords[opcode] == Globals.ALU_L_SHIFT ||
                    Globals.keywords[opcode] == Globals.ALU_NOT ||
                    Globals.keywords[opcode] == Globals.ALU_AND ||
                    Globals.keywords[opcode] == Globals.ALU_OR ||
                    Globals.keywords[opcode] == Globals.ALU_XOR ||
                    Globals.keywords[opcode] == Globals.ALU_COMPARE)) ||

                    //jump if requires flag parameters (both CPU models)
                    Globals.keywords[opcode] == Globals.OP_JUMP_IF))
                {
                    MessageBox.Show("Incorrect number of parameters provided at line: " + lineNo);
                }

                //following instructions require 2 bit parameters (register B)
                else if (parameters.Length != Globals.GPR_ADDRESS_SIZE &&

                    //data and jump register (register model only)
                    registerMachine && (Globals.keywords[opcode] == Globals.OP_JUMP_RG_TOS || 
                    Globals.keywords[opcode] == Globals.OP_DATA_PUSH))
                {
                    MessageBox.Show("Incorrect number of parameters provided at line: " + lineNo);
                }

                //following instructions require no parameters
                else if (parameters.Length != 0 &&

                    //jump (in register model only) 
                    (((registerMachine && (Globals.keywords[opcode] == Globals.OP_JUMP)) || 
                    
                    //reset (both CPU models)
                    Globals.keywords[opcode] == Globals.OP_RESET_FLAGS) ||
                    
                    //all stack instructions except jump if
                    (!registerMachine && Globals.keywords[opcode] != Globals.OP_JUMP_IF)))
                {
                    MessageBox.Show("Incorrect number of parameters provided at line: " + lineNo);
                }

                else
                {
                    //initialise bitarray so instruction can be copied
                    newInstruction = new BitArray(Globals.WORD_SIZE);

                    //put 2 0s in RGA for instructions that do not use it
                    if (parameters.Length == Globals.GPR_ADDRESS_SIZE)
                    {
                        string blanks = "00";
                        blanks += parameters;
                        parameters = blanks;
                    }

                    //combine opcode and parameters
                    fullInstruction = Globals.keywords[opcode] + parameters;

                    //convert full instruction to bitarray
                    for (int currentBit = 0; currentBit < Globals.WORD_SIZE && currentBit < fullInstruction.Length; currentBit++)
                        if (fullInstruction[currentBit] == '1') newInstruction[currentBit] = true;
                }
            }

            //line is an address or data
            else if (opcode == Globals.ADDRESS || opcode == Globals.DATA) newInstruction = parseData(lineNo, parameters);

            //line is a halt
            else if (opcode == Globals.HALT)
            {
                lastInstruction = lineNo - 1;
                halt = true;
                newInstruction = new BitArray(Globals.WORD_SIZE);
            }

            //invalid parameters
            else if (!parametersBinary)
                MessageBox.Show("Invalid parameters: " + instruction + " at line " + lineNo + ". Instruction parameters must be in binary.");

            //invalid instruction
            else MessageBox.Show("Invalid opcode: " + opcode + " at line " + lineNo);

            if (newInstruction != null) return Globals.reverseBitArray(newInstruction);
            else return null;
        }

        private BitArray parseData(int lineNo, string input)
        {
            BitArray data = null;

            //cannot be more than 8 bits
            if (input.Length > Globals.WORD_SIZE)
                MessageBox.Show("Invalid address/data: " + input + " at line " + lineNo + ". Must be 8 digits or less.");

            //cannot be first instruction
            else if (lineNo == 0)
                MessageBox.Show("Invalid address/data: " + input + " at line " + lineNo + ". Cannot be first instruction.");

            else
            {
                //initialise bitarray so instruction can be copied
                data = new BitArray(Globals.WORD_SIZE);

                //copy input to bitarray
                for (int inputBit = input.Length - 1, copyBit = Globals.WORD_SIZE - 1; inputBit >= 0; inputBit--, copyBit--)
                {
                    if (input[inputBit] == '1') data[copyBit] = true;
                    else if (input[inputBit] != '0')
                    {
                        MessageBox.Show("Invalid address/data: " + input + " at line " + lineNo + ". Must be in binary.");

                        data = null;
                        break;
                    }
                }
            }

            return data;
        }

        private void loadRegisterCPU()
        {
            //link GUI events to CPU registers
            CU = new ControlUnit(instructions, accessRAMLocation, new byte[] { (byte)lastInstruction },
                accessGPRContents, updateIARContents, updateIRContents, updateMARContents,
                updateTMPContents, readBUS1, accessALU, updateAccumulatorContents,
                updateFlagRegister, resetColours);
        }

        private void loadStackCPU()
        {
            //create CPU and link methods to respond to events
            CU = new ControlUnit(instructions, accessRAMLocation, new byte[] { (byte)lastInstruction }, 
                pushPopStack, accessStack, readTMPToBus,
                updateIARContents, updateIRContents, updateMARContents,
                updateTMPContents, readBUS1, accessALU, updateAccumulatorContents,
                updateFlagRegister, resetColours);
        }

        public MainForm()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            showWelcomeScreen();
        }

        //CPU state changes
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
                    lblTMPContents.ForeColor = Globals.WRITE_COLOR;             //highlight data (visual aid)
                    //------------------------------------------------------

                    controlWire.Dispose();
                }

                circuit.Dispose();
            }
        }

        public void readTMPToBus()
        {
            if (InvokeRequired) Invoke(new ReadOnlyRegister(readTMPToBus));
            else
            {
                Graphics circuit = CreateGraphics();
                Pen controlWire = new Pen(Globals.READ_COLOR);
                controlWire.Width = 2;

                //read process
                //-----------------------------------------------
                //activate enable wire
                circuit.DrawLine(controlWire, 160, 75, 325, 75);
                circuit.DrawLine(controlWire, 325, 75, 325, 175);

                //highlight data (visual aid)
                lblTMPContents.ForeColor = Globals.READ_COLOR;

                //data flows onto bus
                enableBus();
                //-----------------------------------------------

                controlWire.Dispose();
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
                    lblAccContents.ForeColor = Globals.READ_COLOR;          //highlight data (visual aid)
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
                        case Globals.COUT_FLAG:
                            circuit.DrawLine(controlWire, 140, 280, 170, 280);  //data flows from ALU to COUT
                            circuit.DrawLine(controlWire, 245, 280, 275, 280);  //continues flow to CU

                            //data flows from COUT back to CIN
                            circuit.DrawLine(controlWire, 260, 280, 260, 230);  
                            circuit.FillEllipse(wireJoint, 255, 275, 8, 8);     
                            circuit.DrawLine(controlWire, 260, 230, 140, 230);   

                            lblCoutContents.Text = "1";                         //data flows into memory (contents changed)
                            lblCoutContents.ForeColor = Globals.WRITE_COLOR;    //highlight data (visual aid)
                            break;

                        case Globals.EQUAL_FLAG:
                            circuit.DrawLine(controlWire, 140, 320, 170, 320);  //data flows from ALU to EQUAL
                            circuit.DrawLine(controlWire, 245, 320, 275, 320);  //continues flow to CU

                            lblEqualContents.Text = "1";                        //data flows into memory (contents changed)
                            lblEqualContents.ForeColor = Globals.WRITE_COLOR;   //highlight data (visual aid)
                            break;

                        case Globals.A_LARGER_FLAG:
                            circuit.DrawLine(controlWire, 140, 300, 170, 300);  //data flows from ALU to A LARGER
                            circuit.DrawLine(controlWire, 245, 300, 275, 300);  //continues flow to CU

                            lblALargerContents.Text = "1";                      //data flows into memory (contents changed)
                            lblALargerContents.ForeColor = Globals.WRITE_COLOR; //highlight data (visual aid)
                            break;

                        case Globals.ZERO_FLAG:
                            circuit.DrawLine(controlWire, 140, 340, 170, 340);  //data flows from ALU to ZERO
                            circuit.DrawLine(controlWire, 245, 340, 275, 340);  //continues flow to CU

                            lblZeroContents.Text = "1";                         //data flows into memory (contents changed)
                            lblZeroContents.ForeColor = Globals.WRITE_COLOR;    //highlight data (visual aid)
                            break;

                        default:
                            //data flows into memory (contents changed)
                            lblCoutContents.Text = "0";
                            lblALargerContents.Text = "0";
                            lblEqualContents.Text = "0";
                            lblZeroContents.Text = "0";

                            circuit.DrawLine(controlWire, 245, 280, 275, 280);  //data flows from COUT to CU
                            circuit.DrawLine(controlWire, 245, 300, 275, 300);  //data flows from A LARGER to CU
                            circuit.DrawLine(controlWire, 245, 320, 275, 320);  //data flows from EQUALS to CU
                            circuit.DrawLine(controlWire, 245, 340, 275, 340);  //data flows from ZERO to CU

                            //data flows from COUT back to CIN
                            circuit.DrawLine(controlWire, 260, 280, 260, 230);
                            circuit.FillEllipse(wireJoint, 255, 275, 8, 8);
                            circuit.DrawLine(controlWire, 260, 230, 140, 230);

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

        public void accessGPRContents(BitArray data, int GPRIndex, bool accessMode)
        {
            if (InvokeRequired) Invoke(new ReadWriteMemory(accessGPRContents), new object[] { data, GPRIndex, accessMode });
            else
            {
                if (GPRIndex < Globals.NO_OF_GPR)
                {
                    Graphics circuit = CreateGraphics();

                    if (accessMode == Globals.REGISTER_READ)
                    {
                        Pen controlWire = new Pen(Globals.READ_COLOR);
                        controlWire.Width = 2;

                        //read process
                        //--------------------------------------------------------
                        //activate enable wire for selected GPR
                        switch (GPRIndex)
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
                        lblGPRContents[GPRIndex].ForeColor = Globals.READ_COLOR;     //highlight data (visual aid)
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
                        switch (GPRIndex)
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
                        
                        lblGPRContents[GPRIndex].Text = Globals.convertBitsToString(data);  //data flows into memory (contents changed)
                        lblGPRContents[GPRIndex].ForeColor = Globals.WRITE_COLOR;            //highlight data (visual aid)
                        //----------------------------------------------------------------

                        controlWire.Dispose();
                    }

                    circuit.Dispose();
                }
            }
        }

        public void pushPopStack(bool accessMode, int stackIndex)
        {
            if (InvokeRequired) Invoke(new ReadWriteStack(pushPopStack), new object[] { accessMode, stackIndex});
            else
            {
                if (stackIndex < Globals.STACK_SIZE)
                {
                    Graphics circuit = CreateGraphics();

                    if (accessMode == Globals.REGISTER_READ)
                    {
                        //top element, copy into TOS
                        if (stackIndex == 0)
                        {
                            Pen controlWire = new Pen(Globals.READ_COLOR);
                            controlWire.Width = 2;

                            //read process
                            //----------------------------------------------------------
                            circuit.DrawLine(controlWire, 655, 190, 625, 190);              //activate enable wire
                            lblStackContents[stackIndex].ForeColor = Globals.READ_COLOR;    //highlight data (visual aid)
                            enableBus();                                                    //data flows onto the bus

                            //write to TOS
                            updateTMPContents(Globals.convertStringToBits(lblStackContents[stackIndex].Text), Globals.REGISTER_WRITE);
                            //----------------------------------------------------------

                            controlWire.Dispose();
                        }

                        else
                        {
                            //shift data in current element to element above
                            lblStackContents[stackIndex].ForeColor = Globals.READ_COLOR;                //highlight data (visual aid)
                            lblStackContents[stackIndex - 1].Text = lblStackContents[stackIndex].Text;  //shift data up
                            lblStackContents[stackIndex - 1].ForeColor = Globals.WRITE_COLOR;           //highlight data (visual aid)
                        }
                    }

                    else if (accessMode == Globals.REGISTER_WRITE)
                    {
                        //copy TOS to top of stack first
                        if (stackIndex == 0)
                        {
                            Pen controlWire = new Pen(Globals.READ_COLOR);
                            controlWire.Width = 2;

                            //read from TOS
                            //-----------------------------------------------
                            //activate enable wire
                            circuit.DrawLine(controlWire, 160, 75, 325, 75);
                            circuit.DrawLine(controlWire, 325, 75, 325, 175);
 
                            //highlight data (visual aid)
                            lblTMPContents.ForeColor = Globals.READ_COLOR;

                            //data flows onto the bus
                            enableBus();
                            //-----------------------------------------------

                            controlWire = new Pen(Globals.WRITE_COLOR);
                            controlWire.Width = 2;

                            //write process
                            //-----------------------------------------------------------
                            circuit.DrawLine(controlWire, 655, 185, 625, 185);              //activate set wire
                            lblStackContents[stackIndex].Text = lblTMPContents.Text;        //copy data from TOS to top of stack
                            lblStackContents[stackIndex].ForeColor = Globals.WRITE_COLOR;   //highlight data (visual aid)
                            //-----------------------------------------------------------

                            controlWire.Dispose();
                        }

                        else
                        {
                            //shift data from memory above into current memory
                            lblStackContents[stackIndex - 1].ForeColor = Globals.READ_COLOR;            //highlight data (visual aid)
                            lblStackContents[stackIndex].Text = lblStackContents[stackIndex - 1].Text;  //shift data down
                            lblStackContents[stackIndex].ForeColor = Globals.WRITE_COLOR;               //highlight data (visual aid)
                        }
                    }

                    circuit.Dispose();
                }
            }
        }

        public void accessStack(BitArray data, bool accessMode)
        {
            if (InvokeRequired) Invoke(new ReadWriteRegister(accessStack), new object[] { data, accessMode });
            else
            {
                Graphics circuit = CreateGraphics();

                if (accessMode == Globals.REGISTER_READ)
                {
                    Pen controlWire = new Pen(Globals.READ_COLOR);
                    controlWire.Width = 2;

                    //read process
                    //-------------------------------------------------
                    circuit.DrawLine(controlWire, 655, 190, 625, 190);      //activate enable wire
                    lblStackContents[0].ForeColor = Globals.READ_COLOR;     //highlight data (visual aid)
                    enableBus();                                            //data flows onto the bus
                    //-------------------------------------------------

                    controlWire.Dispose();
                }

                else if (accessMode == Globals.REGISTER_WRITE && data != null)
                {
                    Pen controlWire = new Pen(Globals.WRITE_COLOR);
                    controlWire.Width = 2;

                    //read process
                    //-----------------------------------------------------------
                    circuit.DrawLine(controlWire, 655, 185, 625, 185);              //activate set wire
                    lblStackContents[0].Text = Globals.convertBitsToString(data);   //copy data to top of stack
                    lblStackContents[0].ForeColor = Globals.WRITE_COLOR;            //highlight data (visual aid)
                    //-----------------------------------------------------------

                    controlWire.Dispose();
                }

                circuit.Dispose();
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

                if (registerMachine)
                {
                    for (int count = 0; count < Globals.NO_OF_GPR; count++)
                    {
                        lblGPRContents[count].ForeColor = Color.Black;
                        pnlGPR[count].Refresh();
                    }
                }

                else
                {
                    for (int count = 0; count < Globals.STACK_SIZE; count++)
                    {
                        lblStackContents[count].ForeColor = Color.Black;
                    }

                    pnlStack.Refresh();
                }

                pnlIR.Refresh();
                pnlIAR.Refresh();
                pnlMAR.Refresh();
                pnlRAM.Refresh();
                pnlTMP.Refresh();
                pnlAcc.Refresh();
                pnlFlags.Refresh();

                if (registerMachine) drawRegisterCircuit();
                else drawStackCircuit();

                drawBus();
            }
        }
        
        public void resetComponents()
        {
            updateIRContents(new BitArray(Globals.WORD_SIZE), Globals.REGISTER_WRITE);
            updateIARContents(new BitArray(Globals.WORD_SIZE), Globals.REGISTER_WRITE);
            updateMARContents(new BitArray(Globals.WORD_SIZE), Globals.REGISTER_WRITE);
            accessRAMLocation(new BitArray(Globals.WORD_SIZE), 0, Globals.REGISTER_WRITE);
            updateTMPContents(new BitArray(Globals.WORD_SIZE), Globals.REGISTER_WRITE);
            updateAccumulatorContents(new BitArray(Globals.WORD_SIZE), Globals.REGISTER_WRITE);
            updateFlagRegister(Globals.REGISTER_WRITE);

            if (registerMachine)
            {
                for (int count = 0; count < Globals.NO_OF_GPR; count++)
                {
                    accessGPRContents(new BitArray(Globals.WORD_SIZE), count, Globals.REGISTER_WRITE);
                    pnlGPR[count].Refresh();
                }
            }

            else
            {
                accessStack(new BitArray(Globals.WORD_SIZE), Globals.REGISTER_WRITE);

                for (int count = 0; count < Globals.STACK_SIZE; count++)
                {
                    lblStackContents[count].Text = "00000000";
                    lblStackContents[count].ForeColor = Color.Red;
                }

                pnlStack.Refresh();
            }

            pnlIR.Refresh();
            pnlIAR.Refresh();
            pnlMAR.Refresh();
            pnlRAM.Refresh();
            pnlTMP.Refresh();
            pnlAcc.Refresh();
            pnlFlags.Refresh();
        }
    }

    public partial class InstructionForm : Form
    {
        private PictureBox instructionTable = new PictureBox();

        public InstructionForm(bool registerMachine)
        {
            string image;

            ClientSize = new Size(1000, 563);
            Text = "Instructions";
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;

            if (registerMachine) image = @"assets\register_table.png";
            else image = @"assets\stack_table.png";

            try
            {
                //load instruction table image
                instructionTable.Image = new Bitmap(image);
                instructionTable.Size = new Size(1000, 563);
                instructionTable.SizeMode = PictureBoxSizeMode.StretchImage;
                Controls.Add(instructionTable);
            }

            catch
            {
                MessageBox.Show("Could not load instruction table.");
            }
        }
    }

    public static class Globals
    {
        public static int CLOCK_SPEED = 0;

        //instruction format
        public const int WORD_SIZE = 8;
        public const int OPCODE_SIZE = WORD_SIZE / 2;
        public const int GPR_ADDRESS_SIZE = 2;

        public const int WORD_START = WORD_SIZE - 1;        //word stored as big endian
        public const int OPCODE_START = OPCODE_SIZE - 1;    //opcode also big endian
        public const int ALU_OPCODE = OPCODE_START;
                    
        public const string 
            //stack and register opcodes
            OP_LOAD_POP = "0000", OP_STORE_SWAP = "0001", OP_DATA_PUSH = "0010", OP_JUMP_RG_TOS = "0011",
            OP_JUMP = "0100", OP_JUMP_IF = "0101", OP_RESET_FLAGS = "0110", OP_IO = "0111",
                            
            //ALU opcodes
            ALU_ADD = "1000", ALU_R_SHIFT = "1001", ALU_L_SHIFT = "1010", ALU_NOT = "1011", 
            ALU_AND = "1100", ALU_OR = "1101", ALU_XOR = "1110", ALU_COMPARE = "1111";

        //map keywords to opcodes
        public static Dictionary<string, string> keywords = new Dictionary<string, string>
        {
            //stack and register keywords
            { "LD", OP_LOAD_POP }, { "STR", OP_STORE_SWAP }, { "DAT", OP_DATA_PUSH }, { "JRG", OP_JUMP_RG_TOS },
            { "POP", OP_LOAD_POP }, { "SWP", OP_STORE_SWAP }, { "PSH", OP_DATA_PUSH }, { "JTS", OP_JUMP_RG_TOS },
            { "JMP", OP_JUMP }, { "JIF", OP_JUMP_IF }, { "RES", OP_RESET_FLAGS }, { "IO", OP_IO },

            //ALU keywords
            { "ADD", ALU_ADD }, { "RSH", ALU_R_SHIFT }, { "LSH", ALU_L_SHIFT }, { "NOT", ALU_NOT }, { "AND", ALU_AND },
            { "OR", ALU_OR }, { "XOR", ALU_XOR }, { "CMP", ALU_COMPARE }
        };

        //additional keywords
        public const string ADDRESS = "ADR", DATA = "VAL", HALT = "HLT";

        //indexes of flags are reversed to support BitArray big endian format 
        public const int COUT_FLAG = 3, A_LARGER_FLAG = 2, EQUAL_FLAG = 1, ZERO_FLAG = 0; 

        //available memory
        public const int RAM_SIZE = 256;
        public const int NO_OF_GPR = 4;
        public const int STACK_SIZE = 8;
        public const int NO_OF_FLAGS = 4;

        //access modes
        public static readonly Color WRITE_COLOR = Color.Red, READ_COLOR = Color.DodgerBlue, BUS_COLOR = Color.Red;
        public const bool REGISTER_READ = true, REGISTER_WRITE = false;

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

        public static BitArray convertStringToBits(string data)
        {
            BitArray result = new BitArray(WORD_SIZE);

            for (int count = 0; count < WORD_SIZE; count++)
                if (data[count] == '1') result.Set(count, true);

            result = reverseBitArray(result);
            return result;
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
        public event ReadWriteRegister ReadWriteContents;

        private BitArray contents = new BitArray(Globals.WORD_SIZE);

        //no event assigned, used for registers that require specialised events (i.e. RAM and GPRs need an address)
        public Register() { }

        //event assigned for registers that have generic read and write events (i.e IR, MAR etc., don't require an address)
        public Register(ReadWriteRegister readWriteContents) { ReadWriteContents += readWriteContents; }

        //access contents without invoking event
        public BitArray getContents() { return contents; }
        public void setContents(BitArray contents = null)
        {
            if (contents != null) this.contents = contents;
            else this.contents = new BitArray(new byte[] { 0 });
        }

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
        public event ReadWriteMemory ReadWriteContents;

        private Register[] contents = new Register[Globals.RAM_SIZE];

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

        public BitArray readFromLocation(BitArray address, bool invokeEvent)
        {
            byte addr = Globals.convertBitsToByte(address);
            if (invokeEvent) ReadWriteContents?.Invoke(contents[addr].readContents(), addr, Globals.REGISTER_READ);
            return contents[addr].readContents();
        }

        public void writeToLocation(BitArray address, BitArray data, bool invokeEvent)
        {
            byte addr = Globals.convertBitsToByte(address);
            contents[addr].overwriteContents(data);
            if (invokeEvent)
            {
                ReadWriteContents?.Invoke(data, addr, Globals.REGISTER_WRITE);
                if (ReadWriteContents != null) Thread.Sleep(Globals.CLOCK_SPEED);
            }
        }
    }

    public class MAR : Register
    {
        private RAM RAM;

        public MAR(BitArray[] instructions, ReadWriteMemory readWriteRAM, ReadWriteRegister readWriteContents)
            : base(readWriteContents) { RAM = new RAM(instructions, readWriteRAM); }

        //use MAR to address and access a location in memory
        public void writeToMemory(BitArray data, bool invokeEvent = true) { RAM.writeToLocation(getContents(), data, invokeEvent); }
        public BitArray readFromMemory(bool invokeEvent = true) { return RAM.readFromLocation(getContents(), invokeEvent); }
    }

    public class ALU
    {
        public event ReadOnlyRegister ReadBUS1;         //indicate BUS1 active
        public event ReadWriteFlags ReadWriteFlags;     //update flag with its new data (write), or indicate it is being accessed (read)

        private bool BUS1 = false;  //toggle BUS1 enabled state
        public Register TMP;        //also TOS register (stack machine)

        //flag registers
        public bool[] flags = new bool[Globals.NO_OF_FLAGS];

        public ALU(ReadWriteRegister readWriteTMP, ReadOnlyRegister readBUS1, ReadWriteFlags readWriteFlags)
        {
            TMP = new Register(readWriteTMP);

            //flags
            for (int count = 0; count < Globals.NO_OF_FLAGS; count++)
                flags[count] = false;

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
                                                        //GUI EVENTS
        public event ReadWriteMemory ReadWriteGPR;      //update GPR with its new data (write), or indicate it is being accessed (read)
        public event ReadWriteFlags ReadWriteFlags;     //same as above, but for flags
        public event ALUOperation RunALUOperation;      //indicate type of operation being performed
        public event RedrawGUI ResetControlBits;        //turn off all control bits (set, enable, opcodes) [invoke after each step of an instruction]

        public event ReadWriteStack PushPopStack;       //shifts all stack elements up (pop), or down (push)
        public event ReadOnlyRegister ReadTOS;          //reading TOS using readContents() feeds its output to the ALU, this event outputs to the bus instead

        private bool registerMachine = false;   //CPU to simulate (toggles register and stack)
        private bool programFinished = false;   //stop execution when true

        private BitArray opcode = new BitArray(Globals.OPCODE_SIZE);            //instruction to execute
        private BitArray flagParameters = new BitArray(Globals.NO_OF_FLAGS);    //parameters for a jump if
        private byte registerA, registerB;                                      //address of GPRs in use by current instruction (register machine only)
        
        //tracks the last executable instruction in memory
        public readonly BitArray lastInstruction = new BitArray(Globals.WORD_SIZE);

        private ALU ALU;

        //SPRs
        private MAR MAR;
        private Register IAR, IR, ACC;

        //local memory
        private Register[] GPR = new Register[Globals.NO_OF_GPR];
        private Register[] stack = new Register[Globals.STACK_SIZE];

        //initialise all CPU components for register machine
        public ControlUnit                                                                                                  
            (BitArray[] instructions, ReadWriteMemory readWriteRAM, byte[] lastInstruction, ReadWriteMemory readWriteGPR,   
            ReadWriteRegister readWriteIAR, ReadWriteRegister readWriteIR, ReadWriteRegister readWriteMAR,                  
            ReadWriteRegister readWriteTMP, ReadOnlyRegister readBUS1, ALUOperation runALUOperation,                                  
            ReadWriteRegister readWriteAcc, ReadWriteFlags readWriteFlags, RedrawGUI resetControlBits)
        {
            registerMachine = true;

            //create new instance of each component
            ALU = new ALU(readWriteTMP, readBUS1, readWriteFlags);
            ACC = new Register(readWriteAcc);
            MAR = new MAR(instructions, readWriteRAM, readWriteMAR);
            IAR = new Register(readWriteIAR);
            IR = new Register(readWriteIR);

            //GPRs
            for (int count = 0; count < Globals.NO_OF_GPR; count++)
                GPR[count] = new Register();

            this.lastInstruction = new BitArray(lastInstruction);

            //link methods to events
            ReadWriteGPR += readWriteGPR;        
            ReadWriteFlags += readWriteFlags;    
            RunALUOperation += runALUOperation;  
            ResetControlBits += resetControlBits;
        }

        //initialise all CPU components for stack machine
        public ControlUnit
            (BitArray[] instructions, ReadWriteMemory readWriteRAM, byte[] lastInstruction, 
            ReadWriteStack pushPopStack, ReadWriteRegister readWriteStack, ReadOnlyRegister readTOS,
            ReadWriteRegister readWriteIAR, ReadWriteRegister readWriteIR, ReadWriteRegister readWriteMAR,
            ReadWriteRegister readWriteTOS, ReadOnlyRegister readBUS1, ALUOperation runALUOperation,
            ReadWriteRegister readWriteAcc, ReadWriteFlags readWriteFlags, RedrawGUI resetControlBits)
        {
            registerMachine = false;

            //create new instance of each component
            ALU = new ALU(readWriteTOS, readBUS1, readWriteFlags);
            ACC = new Register(readWriteAcc);
            MAR = new MAR(instructions, readWriteRAM, readWriteMAR);
            IAR = new Register(readWriteIAR);
            IR = new Register(readWriteIR);

            //stack
            for (int count = 0; count < Globals.STACK_SIZE; count++)
            {
                if (count == 0) stack[count] = new Register(readWriteStack);    //top element can invoke read/write event
                else stack[count] = new Register();
            }

            this.lastInstruction = new BitArray(lastInstruction);

            //link methods to events

            PushPopStack += pushPopStack;        
            ReadTOS += readTOS;                  
            ReadWriteFlags += readWriteFlags;    
            RunALUOperation += runALUOperation;  
            ResetControlBits += resetControlBits;
        }

        public void start()
        {
            //reset eveything to 0
            if (programFinished)
            {
                IR.setContents();
                IAR.setContents();
                MAR.setContents();
                ALU.TMP.setContents();
                ACC.setContents();

                for (int count = 0; count < Globals.NO_OF_FLAGS; count++)
                    ALU.flags[count] = false;

                if (registerMachine)
                {
                    for (int count = 0; count < Globals.NO_OF_GPR; count++)
                        GPR[count].setContents();
                }

                else
                {
                    for (int count = 0; count < Globals.STACK_SIZE; count++)
                        stack[count].setContents();
                }

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

        public void resetState() { programFinished = true; }

        public void fetchInstruction()
        {
            prepareInstruction();
            incrementIAR();
            setInstructionRegister();
        }

        //point MAR to instruction to execute
        private void prepareInstruction()
        {
            //copy next instruction address to MAR
            MAR.overwriteContents(IAR.readContents());          
            ResetControlBits?.Invoke();                         
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //point IAR to next instruction
        private void incrementIAR()
        {
            //IAR is at last instruction, program will finish after executing
            if (Globals.areBitsEqual(IAR.getContents(), lastInstruction)) programFinished = true;

            ALU.toggleBUS1();                                   //enable BUS1 (ALU input B becomes 1)
            ACC.overwriteContents(ALU.add(IAR.readContents())); //add IAR and B (increments value by 1)
            ALU.toggleBUS1();                                   //disable BUS1

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //update IAR with new value
            IAR.overwriteContents(ACC.readContents());          
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
                    flagParameters[currentBit] = IR[currentBit];
            }

            //non opcode bits GPR addresses
            else if (registerMachine)
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
            if (opcode[Globals.ALU_OPCODE])
            {
                RunALUOperation?.Invoke(opcode);

                //register specific ALU instructions
                if (registerMachine)
                {
                    //2 input operation, prep TMP
                    if (opcodeAsString != Globals.ALU_R_SHIFT && opcodeAsString != Globals.ALU_L_SHIFT && opcodeAsString != Globals.ALU_NOT)
                    {
                        ReadWriteGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_READ);   //invoke GPR read
                        ALU.TMP.overwriteContents(GPR[registerB].readContents());                               //copy register B to TMP
                        ResetControlBits?.Invoke();
                        if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

                    }

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

                //stack specific ALU instructions
                else
                {
                    //*no need to prep for 2 input operations, TOS directly feeds to ALU*

                    switch (opcodeAsString)
                    {
                        case Globals.ALU_ADD:
                            ACC.overwriteContents(ALU.add(stack[0].readContents()));
                            break;

                        case Globals.ALU_R_SHIFT:
                            ACC.overwriteContents(ALU.shiftRight(ALU.TMP.readContents()));
                            break;

                        case Globals.ALU_L_SHIFT:
                            ACC.overwriteContents(ALU.shiftLeft(ALU.TMP.readContents()));
                            break;

                        case Globals.ALU_NOT:
                            ACC.overwriteContents(ALU.inverse(ALU.TMP.readContents()));
                            break;

                        case Globals.ALU_AND:
                            ACC.overwriteContents(ALU.and(stack[0].readContents()));
                            break;

                        case Globals.ALU_OR:
                            ACC.overwriteContents(ALU.or(stack[0].readContents()));
                            break;

                        case Globals.ALU_XOR:
                            ACC.overwriteContents(ALU.xor(stack[0].readContents()));
                            break;

                        case Globals.ALU_COMPARE:
                            ALU.compare(stack[0].readContents());
                            Thread.Sleep(Globals.CLOCK_SPEED);
                            break;

                        default:
                            MessageBox.Show("Invalid opcode");
                            break;
                    }

                    ResetControlBits?.Invoke();
                    if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

                    //compare operation doesn't output to accumulator
                    if (opcodeAsString != Globals.ALU_COMPARE)
                    {
                        //push accumulator result into stack
                        push(ACC);
                    }
                }
            }

            //control unit instruction
            else
            {
                switch (opcodeAsString)
                {
                    case Globals.OP_LOAD_POP:
                        if (registerMachine) load();
                        else pop();
                        break;

                    case Globals.OP_STORE_SWAP:
                        if (registerMachine) store();
                        else swap();
                        break;

                    case Globals.OP_DATA_PUSH:
                        if (registerMachine) data();
                        else push();
                        break;

                    case Globals.OP_JUMP_RG_TOS:
                        if (registerMachine) jumpRegister();
                        else jumpTOS();
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
            }
        }

        //REGISTER
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

        private void jumpRegister()
        {
            //copy register B to IAR
            ReadWriteGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_READ);
            IAR.overwriteContents(GPR[registerB].readContents());
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        //STACK
        private void pop()
        {
            //prep MAR with address in IAR
            MAR.overwriteContents(IAR.readContents());
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //point IAR to next instruction after data
            incrementIAR();

            //get location to pop data into and prep into MAR
            BitArray addressToPop = MAR.readFromMemory(false);

            //check if address provided
            bool isBlank = true;
            for (int count = 0; count < Globals.WORD_SIZE; count++)     
            {
                //break at first non zero bit
                if (addressToPop[count])                                
                {
                    isBlank = false;
                    break;
                }
            }

            if (!isBlank)
            {
                //prep memory for write
                MAR.overwriteContents(addressToPop);
                ResetControlBits?.Invoke();
                if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

                //write data in TOS to memory
                ReadTOS?.Invoke();
                MAR.writeToMemory(ALU.TMP.getContents());
                ResetControlBits?.Invoke();
                if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
            }

            //shift stack upwards by 1
            for (int lead = 0, trail = lead - 1; lead < Globals.STACK_SIZE; lead++, trail++)
            {
                if (lead == 0) ALU.TMP.setContents(stack[lead].getContents());   //copy top of stack into TOS
                else stack[trail].setContents(stack[lead].getContents());

                PushPopStack?.Invoke(Globals.REGISTER_READ, lead);
                Thread.Sleep(Globals.CLOCK_SPEED);
            }

            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        private void swap()
        {
            //copy TOS to ACC
            ACC.overwriteContents(ALU.TMP.readContents());     
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //copy top of stack into TOS
            ALU.TMP.overwriteContents(stack[0].readContents()); 
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //copy ACC to top of stack
            stack[0].overwriteContents(ACC.readContents());      
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        private void push()
        {
            //prep MAR with address in IAR
            MAR.overwriteContents(IAR.readContents());
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //point IAR to next instruction after data
            incrementIAR();

            //start from the bottom and copy data from element above to current element
            for (int lead = Globals.STACK_SIZE - 1, trail = lead - 1; trail >= 0; lead--, trail--)
            {
                stack[lead].setContents(stack[trail].getContents());
                PushPopStack?.Invoke(Globals.REGISTER_WRITE, lead);
                Thread.Sleep(Globals.CLOCK_SPEED);
            }

            //copy TOS into top of stack
            stack[0].setContents(ALU.TMP.getContents());
            PushPopStack?.Invoke(Globals.REGISTER_WRITE, 0);
            Thread.Sleep(Globals.CLOCK_SPEED);
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //push new data into TOS
            ALU.TMP.overwriteContents(MAR.readFromMemory());
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        private void push(Register register)
        {
            //start from the bottom and copy data from element above to current element
            for (int lead = Globals.STACK_SIZE - 1, trail = lead - 1; trail >= 0; lead--, trail--)
            {
                stack[lead].setContents(stack[trail].getContents());
                PushPopStack?.Invoke(Globals.REGISTER_WRITE, lead);
                Thread.Sleep(Globals.CLOCK_SPEED);
            }

            //copy TOS into top of stack
            stack[0].setContents(ALU.TMP.getContents());
            PushPopStack?.Invoke(Globals.REGISTER_WRITE, 0);
            Thread.Sleep(Globals.CLOCK_SPEED);
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //push new data into TOS
            ALU.TMP.overwriteContents(register.readContents());
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }

        private void jumpTOS()
        {
            //copy TOS to IAR
            ReadTOS?.Invoke();
            IAR.overwriteContents(ALU.TMP.getContents());
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);
        }
        
        //ALL
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

        private void jumpIf()
        {
            if (flagParameters.Length == Globals.NO_OF_FLAGS)
            {
                bool conditionmet = true;

                //invoke flag read event
                ReadWriteFlags?.Invoke(Globals.REGISTER_READ);
                Thread.Sleep(Globals.CLOCK_SPEED);
                ResetControlBits?.Invoke();
                if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

                //compare state of flags with parameters
                for (int count = 0; count < Globals.NO_OF_FLAGS && conditionmet; count++)
                {
                    if (flagParameters[count] != ALU.flags[count])
                        conditionmet = false;
                }

                if (conditionmet) jump();   //jump to specified instruction
                else incrementIAR();        //step over to next instruction
            }

            else MessageBox.Show("Invalid condition");
        }

        private void resetFlags()
        {
            //flag reset event
            ReadWriteFlags?.Invoke(Globals.REGISTER_WRITE);
            Thread.Sleep(Globals.CLOCK_SPEED);
            ResetControlBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //set all flags to 0
            for (int count = 0; count < Globals.NO_OF_FLAGS; count++)
                ALU.flags[count] = false;
        }
    }
}