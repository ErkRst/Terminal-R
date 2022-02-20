using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Terminal_R
{
    public partial class Form1 : Form
    {
        private string PATH_BAUDRATES_FILE_PATH = @"configs\stdbaud.cfg";
        private string PATH_START_FILE_PATH = @"configs\start.cfg";
        private char DELIMITER = ';';

       

        private TerminalR terminalR;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var MainThread = Thread.CurrentThread;
            MainThread.Priority = ThreadPriority.Highest;

            UpdateCOMsNameCombobox(comboBoxPortName);
            AddToComboboxBaudRate(comboBoxBaudrate, BaudRatesFromFile(PATH_BAUDRATES_FILE_PATH));

            timerFormUpd.Start();
        }
        private void UpdateCOMsNameCombobox(object sender)
        {

            if (sender == null)
                return;

            // Get component
            ComboBox comboBox = (ComboBox)sender;
            // Clear items combobox
            comboBox.Items.Clear();

            // Get COMs Names
            string[] portNames = SerialPort.GetPortNames();

            // If ports not found, then exit method
            if (portNames.Length == 0)
                return;

            if (portNames.Length != 0)
            {


                SerialPort serialPort = new SerialPort();

                //Ports check
                foreach (string name in portNames)
                {
                    serialPort.PortName = name;

                    // if port is closed, then add item
                    if (!serialPort.IsOpen) // TODO: пересмотреть проверку на занятость порта
                    {
                        comboBox.Items.Add(name);
                    }
                }
            }

            // if open port found, then select first open port
            if (comboBox.Items.Count != 0)
                comboBox.SelectedIndex = 0;
        }
        private void AddToComboboxBaudRate(object sender, string[] baudrates)
        {
            //check object sender
            if (sender == null)
                return;

            //check baudrates null
            if (baudrates == null)
                return;

            //check string array length
            if (baudrates.Length == 0)
                return;

            // Get component
            var comboBox = (ComboBox)sender;
            // Clear items combobox
            comboBox.Items.Clear();
            comboBox.Items.AddRange(baudrates);
            var index = comboBox.Items.IndexOf("115200");
            comboBox.SelectedIndex = index;
        }

        private string[] BaudRatesFromFile(string path)
        {
            //check file
            if (!File.Exists(path))
            {
                MessageBox.Show("File \""+ path + "\" not found in configs");
                return null;
            }

            string baudrates = "";
            using (StreamReader sr = new StreamReader(path))
            {
                baudrates = sr.ReadToEnd();
            }
            // var baudratesTrim = "Hello World!!!".Trim(new char[]{' ','!'});//baudrates.Trim(new char[] {'\r', '\n'});
            string[] baudratesArray = baudrates.Split(DELIMITER);

            for (int i = 0; i < baudratesArray.Length; i++)
            {
                baudratesArray[i] = baudratesArray[i].Trim(new char[] { '\r', '\n' });
            }
            return baudratesArray;
        }


        private SerialPort serialPort;
        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (terminalR == null)
            {
                if (comboBoxPortName.Text == "")
                {
                    MessageBox.Show("Select a port!");
                    return;
                }

                if (comboBoxBaudrate.Text == "")
                {
                    MessageBox.Show("Select baudRate");
                    return;
                }

                terminalR = new TerminalR(comboBoxPortName.Text, int.Parse(comboBoxBaudrate.Text));
                terminalR.Start();
            }
            else if (terminalR != null)
            {
                terminalR.Stop();
                terminalR = null;

                buttonConnect.Text = "Connect";

            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            UpdateCOMsNameCombobox(comboBoxPortName);
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (terminalR!=null)
            {
                if (terminalR.PortIsOpen)
                {
                    if (radioButtonNoEndl.Checked)
                    {
                        terminalR.Write(textBoxSend.Text);
                    }else if (radioButtonCR.Checked)
                    {
                        terminalR.Write(textBoxSend.Text+"\r");
                    }else if (radioButtonCRLF.Checked)
                    {
                        terminalR.Write(textBoxSend.Text+"\r\n");
                    }
                }
            }
        }


        private int lastnumByteInArray_prev = 0;
        private int numByteInArray = 0;
        private float Speed = 0;

        private ulong bytesRx_current = 0;
        private ulong bytesRx_prev = 0;

        private DateTime dateTime1 = new DateTime();
        private DateTime dateTime2 = new DateTime();

        private void timerFormUpd_Tick(object sender, EventArgs e)
        {
            if (terminalR!=null)
            {


                labelStatus.Text = "Status:\r\n" +
                                   "Received = " + (terminalR.bytesRx/1024f).ToString("F2") + " [KByte]\r\n";



                numByteInArray = terminalR.numByteInArray;
                if (numByteInArray > lastnumByteInArray_prev)
                {
                    var str = Encoding.ASCII.GetString(terminalR.bytesDataReceived, lastnumByteInArray_prev,
                        numByteInArray - lastnumByteInArray_prev);
                    textBoxReceive.AppendText(str);
                }
                lastnumByteInArray_prev = numByteInArray;

                if (terminalR.PortIsOpen)
                {
                    buttonConnect.Text = "Disconnect";
                }
                else
                {
                    MessageBox.Show("Port Disconnect");
                    buttonConnect.Text = "Connect";
                    buttonConnect.BackColor = SystemColors.Control;
                }

            }
        }
    }
}
