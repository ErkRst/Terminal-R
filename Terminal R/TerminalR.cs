using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


public class TerminalR
{
    private static int BUFFER_RX_SIZE = 10_485_760; // 10 MByte
    public SerialPort serialPort { get; private set; }

    private bool Active = false;
    private Thread thread;

    public bool PortIsOpen { get; private set; } = false;
    public string Status { get; private set; } = "No Errors";
    public Color StatusColor { get; private set; } = Color.Black;

    public byte[] bytesDataReceived { get; private set; } = new byte[BUFFER_RX_SIZE];
    public int numByteInArray { get; private set; } = 0;


    public ulong bytesRx { get; private set; } = 0;

    public TerminalR(string COMName, int baudRate)
    {
        serialPort = new SerialPort(COMName, baudRate);
        serialPort.Parity = Parity.None;
        serialPort.DtrEnable = true;
        serialPort.RtsEnable = true;
    }


    public void Start()
    {
        if (serialPort.IsOpen)
        {
            MessageBox.Show("Port \"" + serialPort.PortName + "\" already in use");
            return;
        }
        serialPort.Open();

        PortIsOpen = true;

        Active = true;
        thread = new Thread(ThreadMethodReceive);
        thread.Start();
    }

    public void Stop()
    {
        Active = false;
        thread.Join();
        serialPort.Close();
    }

    public void Write(string str)
    {
        serialPort.Write(str);
    }

    public void WriteAsync(string str)
    {
        Task.Run((Action)(() =>
        {
            serialPort.Write(str);
        }));
    }

    public void WriteAsync(byte[] dataBytes)
    {
        Task.Run((Action)(() =>
        {
            serialPort.Write(dataBytes, 0, dataBytes.Length);
        }));
    }


    private void ThreadMethodReceive()
    {
        while (Active)
        {
            try
            {
                if (!Active)
                    break;

                while (serialPort.BytesToRead > 0)
                {
                    if (!Active)
                        break;
                    bytesRx++;
                    numByteInArray++;
                    if (numByteInArray >= BUFFER_RX_SIZE)
                    {
                        numByteInArray = 0;
                    }
                    bytesDataReceived[numByteInArray] = (byte)serialPort.ReadByte();
                }
                Thread.Sleep(1);
            }
            catch (Exception e)
            {
                Active = false;
                PortIsOpen = false;
                break;
            }
            
        }
        if (!serialPort.IsOpen || !Active)
            PortIsOpen = false;
        else if (serialPort.IsOpen)
        {
            serialPort.Close();
        }
    }

    private void ThreadMethodSend()
    {
        while (Active)
        {

            Thread.Sleep(1);
        }
    }
}

