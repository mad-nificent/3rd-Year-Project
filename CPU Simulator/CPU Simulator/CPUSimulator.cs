using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace CPU_Simulator
{
    public delegate void ReadOnlyRegister();                                    //(i.e. BUS1)
    public delegate void ReadWriteRegister(BitArray data, bool read);           //(i.e. IR, IAR, MAR, TMP etc.)
    public delegate void ReadWriteMemory(BitArray data, int index, bool read);  //registers that require an address (i.e. RAM, GPRs etc.)
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

        private Panel[] pnlGPR = new Panel[Globals.GPR_COUNT];
        private Label[] lblGPR = new Label[Globals.GPR_COUNT];
        private Label[] lblGPRContents = new Label[Globals.GPR_COUNT];

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

        private Panel pnlAccumulator = new Panel();
        private Label lblAccumulator = new Label();
        private Label lblAccumulatorContents = new Label();
        //---------------------------------------------------

        //drawing
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
                Controls.Add(pnlAccumulator);
                Controls.Add(pnlRAM);
                Controls.Add(pnlMAR);
                Controls.Add(pnlFlags);

                for (int count = 0; count < Globals.GPR_COUNT; count++)
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
                for (int count = 0; count < Globals.GPR_COUNT; count++)
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
                pnlAccumulator.BorderStyle = BorderStyle.FixedSingle;
                pnlAccumulator.Location = new Point(50, 395);
                pnlAccumulator.Size = new Size(80, 50);

                Controls.Add(pnlAccumulator);

                //labelling accumulator
                lblAccumulator.Text = "ACC";
                lblAccumulator.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
                lblAccumulator.TextAlign = ContentAlignment.MiddleCenter;
                lblAccumulator.Location = new Point(22, 4);
                lblAccumulator.Size = new Size(40, 20);

                pnlAccumulator.Controls.Add(lblAccumulator);

                //accumulator contents
                lblAccumulatorContents.Text = "00000000";
                lblAccumulatorContents.TextAlign = ContentAlignment.MiddleCenter;
                lblAccumulatorContents.BorderStyle = BorderStyle.FixedSingle;
                lblAccumulatorContents.Location = new Point(10, 26);
                lblAccumulatorContents.Size = new Size(60, 20);

                pnlAccumulator.Controls.Add(lblAccumulatorContents);
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
            Controls.Remove(pnlAccumulator);
            Controls.Remove(pnlRAM);
            Controls.Remove(pnlMAR);
            Controls.Remove(pnlFlags);

            for (int count = 0; count < Globals.GPR_COUNT; count++)
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

        //buttons
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
        }

        private void btnStack_Click(object sender, EventArgs e)
        {
            ClientSize = new Size(1137, 489);
            Text = "Stack CPU Simulator";

            hideWelcomeScreen();

            //drawCPU();
            //drawUI();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //no thread active
            if (CPUThread == null)
            {
                //start new thread
                CPUThread = new Thread(new ThreadStart(CU.start));
                CPUThread.Start();
            }

            //thread paused
            else if (CPUThread.ThreadState == ThreadState.Suspended)
            {
                CPUThread.Resume();
                btnPauseStop.Text = "Pause";
            }

            //thread stopped manually
            else if (CPUThread.ThreadState == ThreadState.Aborted)
            {
                //reset GUI and CPU state
                resetCPU();
                CU.reset();

                Thread.Sleep(Globals.CLOCK_SPEED);
                resetColours();

                //start new thread
                CPUThread = new Thread(new ThreadStart(CU.start));
                CPUThread.Start();
                btnPauseStop.Text = "Pause";
            }

            //thread completed
            else if (CPUThread.ThreadState == ThreadState.Stopped)
            {
                //reset GUI and CPU state
                resetCPU();
                CU.reset();

                Thread.Sleep(Globals.CLOCK_SPEED);
                resetColours();

                //start new thread
                CPUThread = new Thread(new ThreadStart(CU.start));
                CPUThread.Start();
            }
        }

        private void btnPauseStop_Click(object sender, EventArgs e)
        {
            //no thread active
            if (CPUThread == null || CPUThread.ThreadState == ThreadState.Stopped)
            {
                MessageBox.Show("CPU not currently running.");
            }

            //thread paused
            else if (CPUThread.ThreadState == ThreadState.Suspended)
            {
                CPUThread.Resume();
                CPUThread.Abort();
                btnPauseStop.Text = "Stopped";
            }

            //thread running
            else if (CPUThread.ThreadState != ThreadState.Aborted)
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
            hideUI();
            hideComponents();
            Invalidate();           //clear drawings with pen and brush
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

        //PUBLIC
        public MainForm()
        {
            showWelcomeScreen();

            //                                                        opcode            |  register A   |  register B
            //--------------------------------------------------------------------------|---------------|-----------------
            //BitArray load     = new BitArray(new bool[] { false, false, false, false, | false, false, | false, false, });
            //BitArray store    = new BitArray(new bool[] { false, false, false, true,  | false, false, | false, false, });
            //BitArray data     = new BitArray(new bool[] { false, false, true,  false, | false, false, | false, false, });
            //BitArray jumpRG   = new BitArray(new bool[] { false, false, true,  true,  | false, false, | false, false, });
            //BitArray jump     = new BitArray(new bool[] { false, true,  false, false, | false, false, | false, false, });
            //BitArray jumpIf   = new BitArray(new bool[] { false, true,  false, true,  | false, false, | false, false, });
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


            //instructions
            BitArray dataIntoReg1 = new BitArray(new bool[] { false, false, true, false, false, false, false, false });
            BitArray inputA       = new BitArray(new bool[] { true, true, false, false, false, true, false, false });
            BitArray dataIntoReg2 = new BitArray(new bool[] { false, false, true, false, false, false, false, true });
            BitArray inputB       = new BitArray(new bool[] { false, false, true, false, false, false, true, true });
            BitArray OR = new BitArray(new bool[] { true, true, false, true, false, false, false, true });

            dataIntoReg1 = Globals.reverseBitArray(dataIntoReg1);
            inputA       = Globals.reverseBitArray(inputA);
            dataIntoReg2 = Globals.reverseBitArray(dataIntoReg2);
            inputB       = Globals.reverseBitArray(inputB);
            OR = Globals.reverseBitArray(OR);

            byte[] lastAddress = { 4 };

            //set instructions to load
            BitArray[] instructions = new BitArray[] { dataIntoReg1, inputA, dataIntoReg2, inputB, OR };

            //link GUI events to CPU registers
            CU = new ControlUnit(instructions, accessRAMLocation, lastAddress,
                updateIARContents, updateIRContents, updateMARContents,
                updateTMPContents, toggleBUS1, accessALU, updateAccumulatorContents,
                updateFlagRegister, accessGPRContents, resetColours);
        }

        //CPU state changes
        //-------------------------------------
        public void updateIARContents(BitArray data, bool read)
        {
            if (InvokeRequired) Invoke(new ReadWriteRegister(updateIARContents), new object[] { data, read });
            else
            {
                Graphics circuit = CreateGraphics();

                if (read)
                {
                    //highlight enable wire
                    Pen controlWire = new Pen(Color.DodgerBlue);
                    controlWire.Width = 2;
                    circuit.DrawLine(controlWire, 365, 395, 365, 375);
                    controlWire.Dispose();

                    //turn on the bus
                    enableBus();

                    //highlight contents
                    lblIARContents.ForeColor = Color.DodgerBlue;
                }

                else if (data != null)
                {
                    //highlight set wire
                    Pen controlWire = new Pen(Color.Red);
                    controlWire.Width = 2;
                    circuit.DrawLine(controlWire, 370, 395, 370, 375);
                    controlWire.Dispose();

                    //overwrite and highlight contents
                    lblIARContents.Text = Globals.convertBitsToString(data);
                    lblIARContents.ForeColor = Color.Red;
                }

                circuit.Dispose();
            }
        }

        public void updateIRContents(BitArray data, bool read)
        {
            if (InvokeRequired) Invoke(new ReadWriteRegister(updateIRContents), new object[] { data, read });
            else
            {
                Graphics circuit = CreateGraphics();

                if (read)
                {
                    Pen controlWire = new Pen(Color.DodgerBlue);
                    controlWire.Width = 2;

                    int xPos = 485;
                    for (int count = 0; count < lblIRContents.Text.Length; count++, xPos += 3)
                    {
                        //highlight active opcode wires
                        if (lblIRContents.Text.ElementAt(count) == '1')
                            circuit.DrawLine(controlWire, xPos, 395, xPos, 375);
                    }

                    controlWire.Dispose();

                    //highlight contents
                    lblIRContents.ForeColor = Color.DodgerBlue;
                }

                else if (data != null)
                {
                    //highlight set wire
                    Pen controlWire = new Pen(Color.Red);
                    controlWire.Width = 2;
                    circuit.DrawLine(controlWire, 530, 395, 530, 375);
                    controlWire.Dispose();

                    //overwrite and highlight contents
                    lblIRContents.Text = Globals.convertBitsToString(data);
                    lblIRContents.ForeColor = Color.Red;
                }

                circuit.Dispose();
            }
        }

        public void updateMARContents(BitArray data, bool read)
        {
            if (InvokeRequired) Invoke(new ReadWriteRegister(updateMARContents), new object[] { data, read });
            else
            {
                Graphics circuit = CreateGraphics();

                if (read)
                {
                    //highlight enable wire
                    Pen controlWire = new Pen(Color.DodgerBlue);
                    controlWire.Width = 2;
                    circuit.DrawLine(controlWire, 430, 60, 430, 175);
                    controlWire.Dispose();

                    //turn on the bus
                    enableBus();

                    //highlight contents
                    lblMARContents.ForeColor = Color.DodgerBlue;
                }

                else if (data != null)
                {
                    //highlight set wire
                    Pen controlWire = new Pen(Color.Red);
                    controlWire.Width = 2;
                    circuit.DrawLine(controlWire, 435, 60, 435, 175);
                    controlWire.Dispose();

                    //overwrite and highlight contents
                    lblMARContents.Text = Globals.convertBitsToString(data);
                    lblMARContents.ForeColor = Color.Red;
                }

                circuit.Dispose();
            }
        }

        public void updateTMPContents(BitArray data, bool read)
        {
            if (InvokeRequired) Invoke(new ReadWriteRegister(updateTMPContents), new object[] { data, read });
            else
            {
                Graphics circuit = CreateGraphics();

                if (read)
                {
                    Pen bus = new Pen(Color.Red);
                    Pen controlWire = new Pen(Color.DodgerBlue);

                    bus.Width = 1;
                    controlWire.Width = 2;

                    //highlight enable wire
                    circuit.DrawLine(controlWire, 160, 75, 325, 75);
                    circuit.DrawLine(controlWire, 325, 75, 325, 175);

                    //activate bus from TMP -> BUS1
                    circuit.DrawLine(bus, 117.5f, 110, 117.5f, 130);
                    circuit.DrawLine(bus, 122.5f, 110, 122.5f, 130);

                    //activate bus from BUS1 -> ALU
                    circuit.DrawLine(bus, 117.5f, 155, 117.5f, 180);
                    circuit.DrawLine(bus, 122.5f, 155, 122.5f, 180);

                    controlWire.Dispose();
                    bus.Dispose();

                    //turn on the bus
                    enableBus();

                    //highlight contents
                    lblTMPContents.ForeColor = Color.DodgerBlue;
                }

                else if (data != null)
                {
                    Pen controlWire = new Pen(Color.Red);
                    controlWire.Width = 2;

                    //highlight set wire
                    circuit.DrawLine(controlWire, 160, 80, 320, 80);
                    circuit.DrawLine(controlWire, 320, 80, 320, 175);

                    controlWire.Dispose();

                    //overwrite and highlight contents
                    lblTMPContents.Text = Globals.convertBitsToString(data);
                    lblTMPContents.ForeColor = Color.Red;
                }

                circuit.Dispose();
            }
        }

        public void toggleBUS1()
        {
            if (InvokeRequired) Invoke(new ReadOnlyRegister(toggleBUS1));
            else
            {
                Graphics circuit = CreateGraphics();

                Pen bus = new Pen(Color.Red);
                Pen controlWire = new Pen(Color.DodgerBlue);

                bus.Width = 1;
                controlWire.Width = 2;

                //highlight enable wire
                circuit.DrawLine(controlWire, 151, 140, 285, 140);
                circuit.DrawLine(controlWire, 285, 140, 285, 175);

                //highlight bus from BUS1 active
                circuit.DrawLine(bus, 117.5f, 155, 117.5f, 180);
                circuit.DrawLine(bus, 122.5f, 155, 122.5f, 180);

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
                Pen controlWire = new Pen(Color.DodgerBlue);
                controlWire.Width = 2;

                if (opcode[0]) circuit.DrawLine(controlWire, 140, 190, 275, 190);
                if (opcode[1]) circuit.DrawLine(controlWire, 140, 194, 275, 194);
                if (opcode[2]) circuit.DrawLine(controlWire, 140, 198, 275, 198);

                controlWire.Dispose();
                circuit.Dispose();
            }
        }

        public void updateAccumulatorContents(BitArray data, bool read)
        {
            if (InvokeRequired) Invoke(new ReadWriteRegister(updateAccumulatorContents), new object[] { data, read });
            else
            {
                Graphics circuit = CreateGraphics();

                if (read)
                {
                    Pen controlWire = new Pen(Color.DodgerBlue);
                    controlWire.Width = 2;

                    //highlight enable wire
                    circuit.DrawLine(controlWire, 130, 410, 285, 410);
                    circuit.DrawLine(controlWire, 285, 410, 285, 375);

                    controlWire.Dispose();

                    //turn on the bus
                    enableBus();

                    //highlight contents
                    lblAccumulatorContents.ForeColor = Color.DodgerBlue;
                }

                else if (data != null)
                {
                    Pen bus = new Pen(Color.Red);
                    Pen controlWire = new Pen(Color.Red);

                    bus.Width = 1;
                    controlWire.Width = 2;

                    //highlight set wire
                    circuit.DrawLine(controlWire, 130, 415, 290, 415);
                    circuit.DrawLine(controlWire, 290, 415, 290, 375);

                    //enable bus from ALU -> accumulator
                    circuit.DrawLine(bus, 87.5f, 375, 87.5f, 395);
                    circuit.DrawLine(bus, 92.5f, 375, 92.5f, 395);

                    controlWire.Dispose();
                    bus.Dispose();

                    //overwrite and highlight contents
                    lblAccumulatorContents.Text = Globals.convertBitsToString(data);
                    lblAccumulatorContents.ForeColor = Color.Red;
                }

                circuit.Dispose();
            }
        }

        public void updateFlagRegister(bool read, int flag = -1)
        {
            if (InvokeRequired) Invoke(new ReadWriteFlags(updateFlagRegister), new object[] { read, flag });
            else
            {
                Graphics circuit = CreateGraphics();
                SolidBrush wireJoint = new SolidBrush(Color.Red);
                Pen controlWire = new Pen(Color.Red);

                controlWire.Width = 2;

                if (read)
                {
                    //highlight data being read
                    circuit.DrawLine(controlWire, 245, 280, 275, 280);  //cout flag ->
                    circuit.DrawLine(controlWire, 245, 300, 275, 300);  //a >  flag ->
                    circuit.DrawLine(controlWire, 245, 320, 275, 320);  //=    flag ->
                    circuit.DrawLine(controlWire, 245, 340, 275, 340);  //0    flag -> 

                    circuit.DrawLine(controlWire, 260, 280, 260, 230);  //cout -> cin
                    circuit.FillEllipse(wireJoint, 255, 275, 8, 8);    //connection point
                    circuit.DrawLine(controlWire, 260, 230, 140, 230);  //cout -> cin

                    lblCoutContents.ForeColor = Color.DodgerBlue;
                    lblALargerContents.ForeColor = Color.DodgerBlue;
                    lblEqualContents.ForeColor = Color.DodgerBlue;
                    lblZeroContents.ForeColor = Color.DodgerBlue;
                }

                else
                {
                    //highlight set wire
                    circuit.DrawLine(controlWire, 245, 360, 275, 360);

                    //overwrite one flag or reset all flags
                    switch (flag)
                    {
                        case 0:
                            circuit.DrawLine(controlWire, 140, 340, 170, 340);  //0    ALU  -> flag
                            circuit.DrawLine(controlWire, 245, 340, 275, 340);  //0    flag -> 

                            lblZeroContents.Text = "1";
                            lblZeroContents.ForeColor = Color.Red;
                            break;

                        case 1:
                            circuit.DrawLine(controlWire, 140, 300, 170, 300);  //a >  ALU  -> flag
                            circuit.DrawLine(controlWire, 245, 300, 275, 300);  //a >  flag -> 

                            lblALargerContents.Text = "1";
                            lblALargerContents.ForeColor = Color.Red;
                            break;

                        case 2:
                            circuit.DrawLine(controlWire, 140, 320, 170, 320);  //=    ALU  -> flag
                            circuit.DrawLine(controlWire, 245, 320, 275, 320);  //=    flag -> 

                            lblEqualContents.Text = "1";
                            lblEqualContents.ForeColor = Color.Red;
                            break;

                        case 3:
                            circuit.DrawLine(controlWire, 140, 280, 170, 280);  //cout ALU  -> flag
                            circuit.DrawLine(controlWire, 245, 280, 275, 280);  //cout flag -> 
                            circuit.DrawLine(controlWire, 260, 280, 260, 230);  //cout -> cin
                            circuit.FillEllipse(wireJoint, 255, 275, 8, 8);    //connection point
                            circuit.DrawLine(controlWire, 260, 230, 140, 230);  //cout -> cin

                            lblCoutContents.Text = "1";
                            lblCoutContents.ForeColor = Color.Red;
                            break;

                        default:
                            circuit.DrawLine(controlWire, 245, 280, 275, 280);  //cout flag -> 
                            circuit.DrawLine(controlWire, 245, 300, 275, 300);  //a >  flag -> 
                            circuit.DrawLine(controlWire, 245, 320, 275, 320);  //=    flag -> 
                            circuit.DrawLine(controlWire, 245, 340, 275, 340);  //0    flag -> 

                            circuit.DrawLine(controlWire, 260, 280, 260, 230);  //cout -> cin
                            circuit.FillEllipse(wireJoint, 255, 275, 8, 8);    //connection point
                            circuit.DrawLine(controlWire, 260, 230, 140, 230);  //cout -> cin

                            lblCoutContents.Text = "0";
                            lblALargerContents.Text = "0";
                            lblEqualContents.Text = "0";
                            lblZeroContents.Text = "0";

                            lblCoutContents.ForeColor = Color.Red;
                            lblALargerContents.ForeColor = Color.Red;
                            lblEqualContents.ForeColor = Color.Red;
                            lblZeroContents.ForeColor = Color.Red;
                            break;
                    }
                }

                wireJoint.Dispose();
                controlWire.Dispose();
                circuit.Dispose();
            }
        }

        public void accessRAMLocation(BitArray data, int address, bool read)
        {
            if (InvokeRequired) Invoke(new ReadWriteMemory(accessRAMLocation), new object[] { data, address, read });
            else
            {
                if (address < Globals.RAM_SIZE && data != null)
                {
                    Graphics circuit = CreateGraphics();

                    //show contents of address
                    lblRAMContents.Text = Globals.convertBitsToString(data);
                    lblRAMAddress.Text = "Addr " + address.ToString();

                    if (read)
                    {
                        //highlight enable wire
                        Pen controlWire = new Pen(Color.DodgerBlue);
                        controlWire.Width = 2;
                        circuit.DrawLine(controlWire, 515, 60, 515, 175);
                        controlWire.Dispose();

                        //turn on the bus
                        enableBus();

                        //highlight contents
                        lblRAMContents.ForeColor = Color.DodgerBlue;
                    }

                    else
                    {
                        //highlight set wire
                        Pen controlWire = new Pen(Color.Red);
                        controlWire.Width = 2;
                        circuit.DrawLine(controlWire, 520, 60, 520, 175);
                        controlWire.Dispose();

                        //highlight contents
                        lblRAMContents.ForeColor = Color.Red;
                    }

                    circuit.Dispose();
                }
            }
        }

        public void accessGPRContents(BitArray data, int GPRindex, bool read)
        {
            if (InvokeRequired) Invoke(new ReadWriteMemory(accessGPRContents), new object[] { data, GPRindex, read });
            else
            {
                if (GPRindex < Globals.GPR_COUNT)
                {
                    Graphics circuit = CreateGraphics();

                    if (read)
                    {
                        Pen controlWire = new Pen(Color.DodgerBlue);
                        controlWire.Width = 2;

                        //highlight enable wire
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

                        controlWire.Dispose();

                        //turn on the bus
                        enableBus();

                        //highlight contents
                        lblGPRContents[GPRindex].ForeColor = Color.DodgerBlue;
                    }

                    else if (data != null)
                    {
                        Pen controlWire = new Pen(Color.Red);
                        controlWire.Width = 2;

                        //highlight set wire
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

                        controlWire.Dispose();

                        //overwrite and highlight contents
                        lblGPRContents[GPRindex].Text = Globals.convertBitsToString(data);
                        lblGPRContents[GPRindex].ForeColor = Color.Red;
                    }

                    circuit.Dispose();
                }
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
                lblAccumulatorContents.ForeColor = Color.Black;
                lblCoutContents.ForeColor = Color.Black;
                lblALargerContents.ForeColor = Color.Black;
                lblEqualContents.ForeColor = Color.Black;
                lblZeroContents.ForeColor = Color.Black;

                for (int count = 0; count < Globals.GPR_COUNT; count++)
                {
                    lblGPRContents[count].ForeColor = Color.Black;
                    pnlGPR[count].Refresh();
                }

                pnlIR.Refresh();
                pnlIAR.Refresh();
                pnlMAR.Refresh();
                pnlRAM.Refresh();
                pnlTMP.Refresh();
                pnlAccumulator.Refresh();
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

            lblAccumulatorContents.Text = "00000000";
            lblAccumulatorContents.ForeColor = Color.Red;

            lblCoutContents.ForeColor = Color.Red;
            lblCoutContents.Text = "0";

            lblALargerContents.ForeColor = Color.Red;
            lblALargerContents.Text = "0";

            lblEqualContents.ForeColor = Color.Red;
            lblEqualContents.Text = "0";

            lblZeroContents.ForeColor = Color.Red;
            lblZeroContents.Text = "0";

            for (int count = 0; count < Globals.GPR_COUNT; count++)
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
            pnlAccumulator.Refresh();
            pnlFlags.Refresh();
        }
        //-------------------------------------
    }

    public static class Globals
    {
        public static int CLOCK_SPEED = 0;
        public const int FLAG_COUNT = 4;

        //supported memory space
        public const int RAM_SIZE = 256;
        public const int GPR_COUNT = 4;

        //supported length of data/instruction in bits
        public const int DATA_INSTRUCTION_SIZE = 8;
        public const int FIRST_DATA_INSTRUCTION_BIT = DATA_INSTRUCTION_SIZE - 1;

        //supported length of an opcode in bits
        public const int OPCODE_SIZE = DATA_INSTRUCTION_SIZE / 2;
        public const int FIRST_OPCODE_BIT = OPCODE_SIZE - 1;
        public const int ALU_OPCODE = FIRST_OPCODE_BIT;

        //supported length of GPR address in bits
        public const int REGISTER_ADDRESS_SIZE = 2;

        //register access modes
        public const bool REGISTER_READ = true;
        public const bool REGISTER_WRITE = false;

        //flag access modes
        public const bool FLAG_RESET = false;
        public const bool FLAG_READ = true;

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
        private BitArray contents = new BitArray(Globals.DATA_INSTRUCTION_SIZE);

        //PUBLIC
        public event ReadWriteRegister AccessContents;

        public Register() { }
        public Register(ReadWriteRegister accessContents) { AccessContents += accessContents; }

        //access contents of a register without running GUI events
        public BitArray getContents() { return contents; }
        public void resetContents() { contents = new BitArray(new byte[] { 0 }); }

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

    }

    public class RAM
    {
        //PRIVATE
        private Register[] locations = new Register[Globals.RAM_SIZE];

        //PUBLIC
        public event ReadWriteMemory AccessLocation;

        public RAM(BitArray[] instructions, ReadWriteMemory accessLocation)
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
    }

    public class MAR : Register
    {
        //PRIVATE
        private RAM RAM;

        //PUBLIC
        public MAR(BitArray[] instructions, ReadWriteMemory accessRAM, ReadWriteRegister accessContents)
            : base(accessContents) { RAM = new RAM(instructions, accessRAM); }

        //access MAR contents to locate memory address and read/write at the location
        public void writeToMemory(BitArray data) { RAM.writeToLocation(this.getContents(), data); }
        public BitArray readFromMemory() { return RAM.readFromLocation(this.getContents()); }
    }

    public class ALU
    {
        //PRIVATE
        private bool BUS1 = false;

        //PUBLIC
        public event ReadOnlyRegister AccessBUS1;
        public event ReadWriteFlags SetResetFlags;

        public static class Opcodes
        {
            public const string ADD = "1000", R_SHIFT = "1001", L_SHIFT = "1010", NOT = "1011",
                                AND = "1100", OR = "1101", XOR = "1110", COMPARE = "1111";
        }

        public static class Flags { public const int ZERO = 0, A_LARGER = 1, EQUAL = 2, COUT = 3; } 

        public bool[] flags = new bool[Globals.FLAG_COUNT];
        public Register TMP;

        public ALU(ReadWriteRegister overwriteTMP, ReadOnlyRegister accessBUS1, ReadWriteFlags setResetFlags)
        {
            AccessBUS1 += accessBUS1;
            SetResetFlags += setResetFlags;
            TMP = new Register(overwriteTMP);

            //initialise flags to false
            for (int count = 0; count < Globals.FLAG_COUNT; count++) flags[count] = false;
        }

        public void toggleBUS1()
        {
            if (BUS1) BUS1 = false;
            else
            {
                AccessBUS1?.Invoke();
                BUS1 = true;
            }
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

            if (carryBit)
            {
                SetResetFlags?.Invoke(Globals.FLAG_RESET, Flags.COUT);
                flags[Flags.COUT] = true;
            }

            return result;
        }

        public BitArray shiftRight(BitArray data)
        {
            BitArray result = new BitArray(Globals.DATA_INSTRUCTION_SIZE);

            //righmost bit will get shifted out, enable carry flag
            if (data[0])
            {
                SetResetFlags?.Invoke(Globals.FLAG_RESET, Flags.COUT);
                flags[Flags.COUT] = true;
            }

            //look ahead to next bit of a and copy into current bit of result
            for (int front = 1, back = 0; front < Globals.DATA_INSTRUCTION_SIZE; front++, back++)
                result[back] = data[front];

            return result;
        }

        public BitArray shiftLeft(BitArray data)
        {
            BitArray result = new BitArray(Globals.DATA_INSTRUCTION_SIZE);

            //leftmost bit will get shifted out, enable carry flag
            if (data[Globals.FIRST_DATA_INSTRUCTION_BIT])
            {
                SetResetFlags?.Invoke(Globals.FLAG_RESET, Flags.COUT);
                flags[Flags.COUT] = true;
            }

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

                    SetResetFlags?.Invoke(Globals.FLAG_RESET, Flags.A_LARGER);
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

            if (isequal)
            {
                SetResetFlags?.Invoke(Globals.FLAG_RESET, Flags.EQUAL);
                flags[Flags.EQUAL] = true;
            }

            if (iszero)
            {
                SetResetFlags?.Invoke(Globals.FLAG_RESET, Flags.ZERO);
                flags[Flags.ZERO] = true;
            }

            //any flag set, pause GUI
            if (flags[Flags.A_LARGER] || flags[Flags.ZERO] || flags[Flags.EQUAL])
                Thread.Sleep(Globals.CLOCK_SPEED);
        }
    }

    public class ControlUnit
    {
        //PRIVATE
        private bool programEnd = false;    //condition to stop execution
        private byte registerA, registerB;  //temporary storage for register addresses in use

        private ALU ALU;

        private MAR MAR;
        private Register IAR, IR, accumulator;
        private Register[] GPR = new Register[Globals.GPR_COUNT];

        //PUBLIC
        public event ReadWriteMemory AccessGPR;
        public event ReadWriteFlags SetResetFlags;
        public event ALUOperation AccessALU;
        public event RedrawGUI TurnOffSetEnableBits;

        //tracks the address of the last instruction to execute
        public readonly BitArray lastAddress = new BitArray(Globals.DATA_INSTRUCTION_SIZE);

        public ControlUnit(BitArray[] instructions, ReadWriteMemory accessRAM, byte[] lastAddress,              //RAM parameters
            ReadWriteRegister overwriteIAR, ReadWriteRegister overwriteIR, ReadWriteRegister overwriteMAR,      //register parameters
            ReadWriteRegister overwriteTMP, ReadOnlyRegister accessBUS1, ALUOperation accessALU,
            ReadWriteRegister overwriteAccumulator, ReadWriteFlags setResetFlags, ReadWriteMemory accessGPR, 
            RedrawGUI turnOffSetEnableBits)
        {
            //intialising each component and register and linking GUI events to them
            ALU = new ALU(overwriteTMP, accessBUS1, setResetFlags);
            accumulator = new Register(overwriteAccumulator);
            MAR = new MAR(instructions, accessRAM, overwriteMAR);
            IAR = new Register(overwriteIAR);
            IR = new Register(overwriteIR);

            AccessGPR += accessGPR;
            SetResetFlags += setResetFlags;
            AccessALU += accessALU;
            TurnOffSetEnableBits += turnOffSetEnableBits;

            //initialise GPRs
            for (int count = 0; count < Globals.GPR_COUNT; count++)
                GPR[count] = new Register();

            this.lastAddress = new BitArray(lastAddress);
        }

        public void start()
        {
            if (programEnd)
            {
                programEnd = false;

                //reset values of registers
                IR.resetContents();
                IAR.resetContents();
                MAR.resetContents();
                ALU.TMP.resetContents();
                accumulator.resetContents();

                for (int count = 0; count < Globals.GPR_COUNT; count++)
                    GPR[count].resetContents();

                //reset flags without invoking GUI event
                for (int count = 0; count < Globals.FLAG_COUNT; count++) ALU.flags[count] = false;
            }

            //program loop
            while (!programEnd)
            {
                fetchInstruction();
                executeInstruction();
            }

            MessageBox.Show("End of program");
        }

        public void reset() { programEnd = true; }

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
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

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

                AccessALU?.Invoke(opcode);

                switch (opcodeAsString)
                {
                    //ADD
                    case ALU.Opcodes.ADD:
                        AccessGPR?.Invoke(GPR[this.registerA].getContents(), this.registerA, Globals.REGISTER_READ);
                        accumulator.overwriteContents(ALU.add(GPR[this.registerA].readContents()));
                        break;

                    //RSHIFT
                    case ALU.Opcodes.R_SHIFT:
                        AccessGPR?.Invoke(GPR[this.registerA].getContents(), this.registerA, Globals.REGISTER_READ);
                        accumulator.overwriteContents(ALU.shiftRight(GPR[this.registerA].readContents()));
                        break;

                    //LSHIFT
                    case ALU.Opcodes.L_SHIFT:
                        AccessGPR?.Invoke(GPR[this.registerA].getContents(), this.registerA, Globals.REGISTER_READ);
                        accumulator.overwriteContents(ALU.shiftLeft(GPR[this.registerA].readContents()));
                        break;

                    //NOT
                    case ALU.Opcodes.NOT:
                        AccessGPR?.Invoke(GPR[this.registerA].getContents(), this.registerA, Globals.REGISTER_READ);
                        accumulator.overwriteContents(ALU.inverse(GPR[this.registerA].readContents()));
                        break;

                    //AND
                    case ALU.Opcodes.AND:
                        AccessGPR?.Invoke(GPR[this.registerA].getContents(), this.registerA, Globals.REGISTER_READ);
                        accumulator.overwriteContents(ALU.and(GPR[this.registerA].readContents()));
                        break;

                    //OR
                    case ALU.Opcodes.OR:
                        AccessGPR?.Invoke(GPR[this.registerA].getContents(), this.registerA, Globals.REGISTER_READ);
                        accumulator.overwriteContents(ALU.or(GPR[this.registerA].readContents()));
                        break;

                    //XOR
                    case ALU.Opcodes.XOR:
                        AccessGPR?.Invoke(GPR[this.registerA].getContents(), this.registerA, Globals.REGISTER_READ);
                        accumulator.overwriteContents(ALU.xor(GPR[this.registerA].readContents()));
                        break;

                    //compare
                    case ALU.Opcodes.COMPARE:
                        AccessGPR?.Invoke(GPR[this.registerA].getContents(), this.registerA, Globals.REGISTER_READ);
                        ALU.compare(GPR[this.registerA].readContents());

                        TurnOffSetEnableBits?.Invoke();
                        Thread.Sleep(Globals.CLOCK_SPEED);
                        break;

                    default:
                        MessageBox.Show("Invalid opcode");
                        break;
                }

                TurnOffSetEnableBits?.Invoke();
                if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

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

        //copy data from RAM to register B - address given by register A
        private void load()
        {
            //copy register A to MAR
            AccessGPR?.Invoke(GPR[registerA].getContents(), registerA, Globals.REGISTER_READ);
            MAR.overwriteContents(GPR[registerA].readContents());

            //redraw
            TurnOffSetEnableBits?.Invoke();
            if (Globals.CLOCK_SPEED != 0) Thread.Sleep(1000);

            //copy memory contents to register B
            GPR[registerB].overwriteContents(MAR.readFromMemory());
            AccessGPR?.Invoke(GPR[registerB].getContents(), registerB, Globals.REGISTER_WRITE);
            Thread.Sleep(Globals.CLOCK_SPEED);

            //redraw
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

                //GUI shows flags being read
                SetResetFlags?.Invoke(Globals.FLAG_READ);
                Thread.Sleep(Globals.CLOCK_SPEED);

                TurnOffSetEnableBits?.Invoke();
                Thread.Sleep(Globals.CLOCK_SPEED);

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

        private void resetFlags()
        {
            //reset flags on the GUI
            SetResetFlags?.Invoke(Globals.FLAG_RESET);
            Thread.Sleep(Globals.CLOCK_SPEED);

            TurnOffSetEnableBits?.Invoke();
            Thread.Sleep(Globals.CLOCK_SPEED);

            for (int count = 0; count < Globals.FLAG_COUNT; count++) ALU.flags[count] = false;
        }
    }
}