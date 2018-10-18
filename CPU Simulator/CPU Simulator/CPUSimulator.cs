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

namespace CPU_Simulator
{
    public static class Globals
    {
        public const int RAM_SIZE = 256;
        public const int NO_OF_FLAGS = 4;
        public const int NO_OF_GPR = 4;
        public const int BYTE_LENGTH = 8;
        public const int BYTE_LAST = 7;
        public const int OPCODE_LENGTH = BYTE_LENGTH / 2;
        public const int REGISTER_CODE_LENGTH = 2;
    }

    public static class Registers
    {

    }

    public partial class MainForm : Form
    {
        private ControlUnit m_CU = new ControlUnit();

        public MainForm()
        {
            InitializeComponent();

            m_CU.fetchInstruction();
        }
    }

    public class Register
    {
        private byte contents;

        public Register(byte contents) { this.contents = contents; }
        public Register() {}

        public void setContents(byte contents) { this.contents = contents; }
        public byte getContents() { return contents; }
    }

    public class RAM
    {
        private Register[] m_locations = new Register[Globals.RAM_SIZE];

        public void writeToLocation(byte address, byte data) { m_locations[address].setContents(data); }
        public byte readFromLocation(byte address) { return m_locations[address].getContents(); }
    }

    public class MAR : Register
    {
        private RAM m_RAM = new RAM();

        public void writeToMemory(byte data) { m_RAM.writeToLocation(getContents(), data); }
        public byte readFromMemory() { return m_RAM.readFromLocation(getContents()); }
    }

    public class ALU
    {
        enum flagtype { COUT, EQUAL, A_LARGER, ZERO };

        private bool[] m_flags = new bool[Globals.NO_OF_FLAGS];
        private Register m_temp = new Register(0);
        private static readonly Register m_bus1 = new Register(1);
        private Register m_accumulator = new Register(0);

        public ALU()
        {
            for (int count = 0; count < Globals.NO_OF_FLAGS; count++)
                m_flags[count] = false;
        }

        public void setTempRegister(byte data) { m_temp.setContents(data); }
        public byte getAccumulatorContents() { return m_accumulator.getContents(); }

        public void readOpcode(byte opcode, byte a)
        {
            string binary = Convert.ToString(opcode, 2);

            switch (binary)
            {
                case "1000":
                    m_accumulator.setContents(add(a));
                    //MessageBox.Show(add(a).ToString());
                    break;

                case "1001":
                    m_accumulator.setContents(shiftRight(a));
                    //MessageBox.Show(shiftRight(a).ToString());
                    break;

                case "1010":
                    m_accumulator.setContents(shiftLeft(a));
                    //MessageBox.Show(shiftLeft(a).ToString());
                    break;

                case "1011":
                    m_accumulator.setContents(inverse(a));
                    //MessageBox.Show(inverse(a).ToString());
                    break;

                case "1100":
                    m_accumulator.setContents(and(a));
                    //MessageBox.Show(and(a).ToString());
                    break;

                case "1101":
                    m_accumulator.setContents(or(a));
                    //MessageBox.Show(or(a).ToString());
                    break;

                case "1110":
                    m_accumulator.setContents(xor(a));
                    //MessageBox.Show(xor(a).ToString());
                    break;

                case "1111":
                    compare(a);
                    MessageBox.Show(m_flags[0].ToString() + ", " + m_flags[1].ToString() + ", "  + m_flags[2].ToString() + ", " + m_flags[3].ToString());
                    break;

                default:
                    MessageBox.Show("Invalid opcode");
                    break;
            }
        }

        private byte add(byte a)
        {
            //create 3 bytes presented in binary
            BitArray binary_a = new BitArray(Globals.BYTE_LENGTH), binary_b = new BitArray(Globals.BYTE_LENGTH), result = new BitArray(Globals.BYTE_LENGTH);

            //copy values a and b in them
            binary_a = new BitArray(new byte[] { a });
            binary_b = new BitArray(new byte[] { m_temp.getContents() });

            bool carrybit = false;
            
            //examine each bit of both values and add them together
            for (int count = 0; count < Globals.BYTE_LENGTH; count++)
            {
                if (carrybit)
                {
                    //0+0+1 = 1 no carry
                    if (binary_a[count] == false && binary_b[count] == false)
                    {
                        result[count] = true;
                        carrybit = false;
                    }

                    //1+1+1 = 1 c1
                    else if (binary_a[count] == true && binary_b[count] == true)
                        result[count] = true;

                    //1+0+1 = 0 c1
                    else result[count] = false;
                }

                else
                {
                    //0+0 = 0 no carry
                    if (binary_a[count] == false && binary_b[count] == false)
                        result[count] = false;

                    //1+1 = 0 c1
                    else if (binary_a[count] == true && binary_b[count] == true)
                    {
                        result[count] = false;
                        carrybit = true;
                    }

                    //0+1 = 1 no carry
                    else result[count] = true;
                }
            }

            //copy binary values back to decimal byte
            byte[] r = new byte[1];
            result.CopyTo(r, 0);

            return r[0];
        }

        private byte shiftRight(byte a)
        {
            BitArray original = new BitArray(Globals.BYTE_LENGTH), result = new BitArray(Globals.BYTE_LENGTH);
            original = new BitArray(new byte[] { a });

            for (int front = 1, back = 0; front < Globals.BYTE_LENGTH; front++, back++)
            {
                //enable cout flag TODO
                result[back] = original[front];
            }

            byte[] r = new byte[1];
            result.CopyTo(r, 0);

            return r[0];
        }

        private byte shiftLeft(byte a)
        {
            BitArray original = new BitArray(Globals.BYTE_LENGTH), result = new BitArray(Globals.BYTE_LENGTH);
            original = new BitArray(new byte[] { a });

            for (int front = 1, back = 0; front < Globals.BYTE_LENGTH; front++, back++)
            {
                //enable cout flag TODO
                result[front] = original[back];
            }

            byte[] r = new byte[1];
            result.CopyTo(r, 0);

            return r[0];
        }

        private byte inverse(byte a)
        {
            BitArray result = new BitArray(Globals.BYTE_LENGTH);
            result = new BitArray(new byte[] { a });

            result.Not();

            byte[] r = new byte[1];
            result.CopyTo(r, 0);

            return r[0];
        }

        private byte and(byte a)
        {
            BitArray binary_a = new BitArray(Globals.BYTE_LENGTH), binary_b = new BitArray(Globals.BYTE_LENGTH);
            binary_a = new BitArray(new byte[] { a });
            binary_b = new BitArray(new byte[] { m_temp.getContents() });

            binary_a.And(binary_b);

            byte[] r = new byte[1];
            binary_a.CopyTo(r, 0);

            return r[0];
        }

        private byte or(byte a)
        {
            BitArray binary_a = new BitArray(Globals.BYTE_LENGTH), binary_b = new BitArray(Globals.BYTE_LENGTH);
            binary_a = new BitArray(new byte[] { a });
            binary_b = new BitArray(new byte[] { m_temp.getContents() });

            binary_a.Or(binary_b);

            byte[] r = new byte[1];
            binary_a.CopyTo(r, 0);

            return r[0];
        }

        private byte xor(byte a)
        {
            BitArray binary_a = new BitArray(Globals.BYTE_LENGTH), binary_b = new BitArray(Globals.BYTE_LENGTH);
            binary_a = new BitArray(new byte[] { a });
            binary_b = new BitArray(new byte[] { m_temp.getContents() });

            binary_a.Xor(binary_b);

            byte[] r = new byte[1];
            binary_a.CopyTo(r, 0);

            return r[0];
        }

        private void compare(byte a)
        {
            BitArray binary_a = new BitArray(Globals.BYTE_LENGTH), binary_b = new BitArray(Globals.BYTE_LENGTH);
            binary_a = new BitArray(new byte[] { a });
            binary_b = new BitArray(new byte[] { m_temp.getContents() });

            bool isequal = true, iszero = true;
            int count = Globals.BYTE_LAST;

            //count from MSB downward (that way first unequal bit means one number is larger)
            while (isequal && count >= 0)
            {
                //check if bit a is true and b is false
                if (binary_a[count] && !binary_b[count])
                {
                    isequal = false;
                    iszero = false;
                    m_flags[(int)flagtype.A_LARGER] = true;
                }

                //check if bit a is false and b is true
                else if (!binary_a[count] && binary_b[count])
                {
                    isequal = false;
                    iszero = false;
                }

                //check if both bits not false
                else if (binary_a[count] && binary_b[count])
                    iszero = false;

                count--;
            }

            if (isequal) m_flags[(int)flagtype.EQUAL] = true;
            if (iszero)  m_flags[(int)flagtype.ZERO] = true;
        }
    }

    public class ControlUnit
    {
        public enum opcodes { ADD = 8, RIGHT_SHIFT, LEFT_SHIFT, NOT, AND, OR, XOR, COMPARE };

        private ALU m_ALU = new ALU();
        private MAR m_MAR = new MAR();
        private Register m_IAR = new Register(0);
        private Register m_IR = new Register(0);
        private Register[] m_GPR = new Register[Globals.NO_OF_GPR];

        public ControlUnit()
        {
            for (int count = 0; count < Globals.NO_OF_GPR; count++)
                m_GPR[count].setContents(0);
        }

        public void fetchInstruction()
        {
            accessMemory();
            incrementIAR();
            setInstructionRegister();
        }

        public void executeInstruction()
        {

        }

        private void accessMemory() { m_MAR.setContents(m_IAR.getContents()); }

        private void incrementIAR()
        {
            m_ALU.setTempRegister(m_IAR.getContents());

            //CHANGE TO USE BUS1
            m_ALU.readOpcode((int)opcodes.ADD, 1);

            m_IAR.setContents(m_ALU.getAccumulatorContents());
        }

        private void setInstructionRegister() { m_IR.setContents(m_MAR.readFromMemory()); }

        private void readInstructionRegister()
        {
            BitArray instruction = new BitArray(Globals.BYTE_LENGTH), opcode = new BitArray(Globals.OPCODE_LENGTH),
                register_a = new BitArray(Globals.REGISTER_CODE_LENGTH), 
                register_b = new BitArray(Globals.REGISTER_CODE_LENGTH);

            instruction = new BitArray(new byte[] { m_IR.getContents() });

            //get opcode from full instruction
            byte[] temp = new byte[1];
            instruction.CopyTo(temp, Globals.OPCODE_LENGTH);
            opcode = new BitArray(temp);

            //fetch index 0 and 1 for reg a and 2 and 3 for reg b
            for (int count = 0, reg_b_index = Globals.REGISTER_CODE_LENGTH; 
                count < Globals.REGISTER_CODE_LENGTH; count++, reg_b_index++)
            {
                register_a[count] = instruction[count];
                register_b[count] = instruction[reg_b_index];
            }

            byte[] rega = new byte[1], regb = new byte[1];
            register_a.CopyTo(rega, 0);
            register_b.CopyTo(regb, 0);
        }
    }
}