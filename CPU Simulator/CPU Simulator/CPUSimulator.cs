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
    public delegate void UpdateRegisterContents(BitArray data);

    public partial class MainForm : Form
    {
        private ControlUnit m_CU;

        public MainForm()
        {
            InitializeComponent();

            BitArray loadintoreg1 = new BitArray(new bool[] { false, false, false, false, false, true, false, false });
            BitArray inputa = new BitArray(new bool[] { true, false, true, false, false, false, false, false });
            BitArray loadintoreg2 = new BitArray(new bool[] { true, false, false, false, false, true, false, false });
            BitArray inputb = new BitArray(new bool[] { false, false, false, true, false, false, false, false });
            BitArray ADD = new BitArray(new bool[] { true, false, false, false, false, false, false, true });

            BitArray[] instructions = new BitArray[] { loadintoreg1, inputa, loadintoreg2, inputb, ADD };
            m_CU = new ControlUnit(instructions);

            m_CU.UpdateIAR += updateIAR;
            m_CU.UpdateIR += updateIR;
            m_CU.UpdateMAR += updateMAR;
            m_CU.UpdateTMP += updateTMP;
            m_CU.m_ALU.UpdateACC += updateAcc;
        }

        public void updateIAR(BitArray data)
        {
            if (InvokeRequired) Invoke(new UpdateRegisterContents(updateIAR), new object[] { data });
            else txtIAR.Text = Globals.convertBitsToString(data);
        }

        public void updateIR(BitArray data)
        {
            if (InvokeRequired) Invoke(new UpdateRegisterContents(updateIR), new object[] { data });
            else txtIR.Text = Globals.convertBitsToString(data);
        }

        public void updateMAR(BitArray data)
        {
            if (InvokeRequired) Invoke(new UpdateRegisterContents(updateMAR), new object[] { data });
            else txtMAR.Text = Globals.convertBitsToString(data);
        }

        public void updateTMP(BitArray data)
        {
            if (InvokeRequired) Invoke(new UpdateRegisterContents(updateTMP), new object[] { data });
            else txtTMP.Text = Globals.convertBitsToString(data);
        }

        public void updateAcc(BitArray data)
        {
            if (InvokeRequired) Invoke(new UpdateRegisterContents(updateAcc), new object[] { data });
            else txtACC.Text = Globals.convertBitsToString(data);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g;
            g = e.Graphics;

            Pen myPen = new Pen(Color.Black);
            myPen.Width = 2;

            //line from top left to MAR
            g.DrawLine(myPen, 30, 41, 562, 41);
            //line from RAM to bottom right
            g.DrawLine(myPen, 900, 66, 900, 440);
            //line from bottom right to bottom left
            g.DrawLine(myPen, 900, 440, 30, 440);
            //line from bottom left to top left
            g.DrawLine(myPen, 30, 440, 30, 41);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            Thread CPUThread = new Thread(new ThreadStart(m_CU.start));
            CPUThread.Start();
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
    }

    public class Register
    {
        private BitArray contents = new BitArray(Globals.BYTE_LENGTH);

        public Register() {}
        public Register(BitArray contents) { this.contents = contents; }

        public void setContents(BitArray contents) { this.contents = contents; }
        public BitArray getContents() { return contents; }
    }

    public class RAM
    {
        private Register[] m_locations = new Register[Globals.RAM_SIZE];
        public readonly int lastinstruction;

        public RAM(BitArray[] instructions)
        {
            //fill RAM with instructions
            for (int count = 0; count < instructions.Length; count++)
            {
                m_locations[count] = new Register();
                m_locations[count].setContents(instructions[count]);
            }

            lastinstruction = instructions.Length - 1;

            //fill RAM with empty registers
            for (int count = instructions.Length; count < Globals.RAM_SIZE; count++)
            {
                m_locations[count] = new Register();
                m_locations[count].setContents(new BitArray(Globals.BYTE_LENGTH));
            }
        }

        public void writeToLocation(BitArray address, BitArray data)
        {
            //get dec value of address
            byte[] addr = new byte[1];
            address.CopyTo(addr, 0);

            //access address and put the data
            m_locations[addr[0]].setContents(data);
        }

        public BitArray readFromLocation(BitArray address)
        {
            //get dec value of address
            byte[] addr = new byte[1];
            address.CopyTo(addr, 0);

            //access address and get the data
            return m_locations[addr[0]].getContents();
        }
    }

    public class MAR : Register
    {
        private RAM m_RAM;

        public MAR(BitArray[] instructions) { m_RAM = new RAM(instructions); }

        public void writeToMemory(BitArray data) { m_RAM.writeToLocation(getContents(), data); }
        public BitArray readFromMemory() { return m_RAM.readFromLocation(getContents()); }
    }

    public class ALU
    {
        public event UpdateRegisterContents UpdateACC;

        public static class Flags { public const int COUT = 0, EQUAL = 1, A_LARGER = 2, ZERO = 3; }
        public bool[] m_flags = new bool[Globals.NO_OF_FLAGS];
        public Register m_temp = new Register(new BitArray(Globals.BYTE_LENGTH));
        public Register m_accumulator = new Register(new BitArray(Globals.BYTE_LENGTH));

        public ALU()
        {
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
                    m_accumulator.setContents(add(a));
                    break;

                case "1001":
                    m_accumulator.setContents(shiftRight(a));
                    break;

                case "1010":
                    m_accumulator.setContents(shiftLeft(a));
                    break;

                case "1011":
                    m_accumulator.setContents(inverse(a));
                    break;

                case "1100":
                    m_accumulator.setContents(and(a));
                    break;

                case "1101":
                    m_accumulator.setContents(or(a));
                    break;

                case "1110":
                    m_accumulator.setContents(xor(a));
                    break;

                case "1111":
                    compare(a);
                    break;

                default:
                    MessageBox.Show("Invalid opcode");
                    break;
            }

            UpdateACC?.Invoke(m_accumulator.getContents());
        }

        private BitArray add(BitArray input_a)
        {
            BitArray input_b = new BitArray(m_temp.getContents()), 
                result = new BitArray(Globals.BYTE_LENGTH);
            
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

        private BitArray and(BitArray a) { return a.And(m_temp.getContents()); }

        private BitArray or(BitArray a) { return a.Or(m_temp.getContents()); }

        private BitArray xor(BitArray a) { return a.Xor(m_temp.getContents()); }

        private void compare(BitArray a)
        {
            BitArray b = new BitArray(m_temp.getContents());
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

        public ALU m_ALU = new ALU();

        //instruction and instruction address registers
        private Register m_IAR = new Register(new BitArray(Globals.BYTE_LENGTH));
        private Register m_IR = new Register(new BitArray(Globals.BYTE_LENGTH));

        //memory address register
        private MAR m_MAR;

        //general purpose registers and index selectors
        private Register[] m_GPR = new Register[Globals.NO_OF_GPR];
        private byte m_rega, m_regb;

        //bus1 register
        private readonly Register m_bus1 = new Register(new BitArray(Globals.BYTE_LENGTH));

        public ControlUnit(BitArray[] instructions)
        {
            m_MAR = new MAR(instructions);

            //initialise GPRs
            for (int count = 0; count < Globals.NO_OF_GPR; count++)
                m_GPR[count] = new Register(new BitArray(Globals.BYTE_LENGTH));

            m_bus1.setContents(new BitArray(new byte[] { 1 }));
        }

        public void start()
        {
            while (true)
            {
                fetchInstruction();
                executeInstruction();
            }
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
            m_MAR.setContents(m_IAR.getContents());
            UpdateMAR?.Invoke(m_MAR.getContents());
        }

        private void incrementIAR()
        {
            //implement BUS1 method TODO

            //hacky method for now
            //------------------------------------------------
            //create byte of 1 and put in TMP
            BitArray bus1 = new BitArray(Globals.BYTE_LENGTH);
            bus1.Set(0, true);
            m_ALU.m_temp.setContents(bus1);

            //create opcode for ADD
            BitArray opcode = new BitArray(Globals.OPCODE_LENGTH);
            opcode.Set(Globals.OPCODE_LAST, true);

            //add 1 and IAR
            m_ALU.readOpcode(opcode, m_IAR.getContents());

            //set new value of IAR
            m_IAR.setContents(m_ALU.m_accumulator.getContents());
            //------------------------------------------------

            UpdateIAR?.Invoke(m_IAR.getContents());
        }

        private void setInstructionRegister()
        {
            m_IR.setContents(m_MAR.readFromMemory());
            UpdateIR?.Invoke(m_IR.getContents());
        }

        private void readInstructionRegister()
        {
            BitArray opcode = new BitArray(Globals.OPCODE_LENGTH),
                register_a = new BitArray(Globals.REGISTER_CODE_LENGTH), 
                register_b = new BitArray(Globals.REGISTER_CODE_LENGTH);

            //copy opcode bits from IR
            for (int count = Globals.OPCODE_LENGTH, opindex = 0; count < Globals.BYTE_LENGTH; count++, opindex++)
                opcode[opindex] = m_IR.getContents()[count];

            //deduce the register codes for reg a and b from IR
            for (int count = 0, reg_a_index = Globals.REGISTER_CODE_LENGTH; 
                count < Globals.REGISTER_CODE_LENGTH; count++, reg_a_index++)
            {
                register_a[count] = m_IR.getContents()[reg_a_index];
                register_b[count] = m_IR.getContents()[count];
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
                m_ALU.m_temp.setContents(m_GPR[m_regb].getContents());
                UpdateTMP?.Invoke(m_ALU.m_temp.getContents());

                m_ALU.readOpcode(opcode, m_GPR[m_rega].getContents());
                m_GPR[m_regb].setContents(m_ALU.m_accumulator.getContents());
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
            m_MAR.setContents(m_GPR[m_rega].getContents());
            UpdateMAR?.Invoke(m_MAR.getContents());

            m_GPR[m_regb].setContents(m_MAR.readFromMemory());
        }

        private void store()
        {
            //reg a selects address to store contents of reg b
            m_MAR.setContents(m_GPR[m_rega].getContents());
            UpdateMAR?.Invoke(m_MAR.getContents());

            m_MAR.writeToMemory(m_GPR[m_regb].getContents());
        }

        private void data()
        {
            //instruction is data, store data in reg b and get next instruction addr
            m_MAR.setContents(m_IAR.getContents());
            UpdateMAR?.Invoke(m_MAR.getContents());

            incrementIAR();
            m_GPR[m_regb].setContents(m_MAR.readFromMemory());
        }

        private void jumpRegister()
        {
            //jump to address stored in reg b
            m_MAR.setContents(m_GPR[m_regb].getContents());
            UpdateMAR?.Invoke(m_MAR.getContents());

            m_IAR.setContents(m_MAR.readFromMemory());
            UpdateIAR?.Invoke(m_IAR.getContents());
        }

        private void jump()
        {
            //jump to address in instruction
            m_MAR.setContents(m_IAR.getContents());
            UpdateMAR?.Invoke(m_MAR.getContents());

            m_IAR.setContents(m_MAR.readFromMemory());
            UpdateIAR?.Invoke(m_IAR.getContents());
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
                    m_MAR.setContents(m_IAR.getContents());
                    UpdateMAR?.Invoke(m_MAR.getContents());

                    m_IAR.setContents(m_MAR.readFromMemory());
                    UpdateIAR?.Invoke(m_IAR.getContents());
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