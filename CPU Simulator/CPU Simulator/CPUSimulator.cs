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
        public const int BYTE_SIZE = 8;
        public const int BYTE_LAST = 7;
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
        private bool[] m_flags = new bool[Globals.NO_OF_FLAGS];
        private Register m_temp = new Register();
        private Register m_bus1 = new Register();
        private Register m_accumulator = new Register();

        public void setTempRegister(byte data) { m_temp.setContents(data); }

        public void readOpcode(byte opcode, byte a)
        {
            string binary = Convert.ToString(opcode, 2);

            switch (binary)
            {
                case "1000":
                    MessageBox.Show(add(a).ToString());
                    break;

                case "1001":
                    MessageBox.Show(shiftRight(a).ToString());
                    break;

                case "1010":
                    MessageBox.Show(shiftLeft(a).ToString());
                    break;

                case "1011":
                    break;

                case "1100":
                    break;

                case "1101":
                    break;

                case "1110":
                    break;

                case "1111":
                    break;

                default:
                    MessageBox.Show("Invalid opcode");
                    break;
            }
        }

        private byte add(byte a)
        {
            //create 3 bytes presented in binary
            BitArray binary_a = new BitArray(8), binary_b = new BitArray(8), result = new BitArray(8);

            //copy values a and b in them
            binary_a = new BitArray(new byte[] { a });
            binary_b = new BitArray(new byte[] { m_temp.getContents() });

            bool carrybit = false;
            
            //examine each bit of both values and add them together
            for (int count = 0; count < Globals.BYTE_SIZE; count++)
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
            BitArray original = new BitArray(8), result = new BitArray(8);
            original = new BitArray(new byte[] { a });

            for (int front = 1, back = 0; front < Globals.BYTE_SIZE; front++, back++)
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
            BitArray original = new BitArray(8), result = new BitArray(8);
            original = new BitArray(new byte[] { a });

            for (int front = 1, back = 0; front < Globals.BYTE_SIZE; front++, back++)
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
            BitArray result = new BitArray(8);
            result = new BitArray(new byte[] { a });

            result.Not();

            byte[] r = new byte[1];
            result.CopyTo(r, 0);

            return r[0];
        }

        private byte and(byte a)
        {
            BitArray binary_a = new BitArray(8), binary_b = new BitArray(8);
            binary_a = new BitArray(new byte[] { a });
            binary_b = new BitArray(new byte[] { m_temp.getContents() });

            binary_a.And(binary_b);

            byte[] r = new byte[1];
            binary_a.CopyTo(r, 0);

            return r[0];
        }
    }

    public class ControlUnit : ALU
    {
        private ALU m_ALU = new ALU();

        public void fetchInstruction()
        {
            m_ALU.setTempRegister(0);
            m_ALU.readOpcode(10, 127);
        }
    }
}
