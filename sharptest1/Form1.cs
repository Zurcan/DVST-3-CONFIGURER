using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
//using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Globalization;
using System.Net;
using Microsoft.VisualBasic;

//using System.Management;
//using System.Management.Instrumentation;
//using System.Management.Instrumentation;




namespace sharptest1
{
    public partial class Form1 : Form
    {
        private BackgroundWorker backgroundWorker1;
        public Form1()
        {
            InitializeComponent();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            this.tabControl1.SelectedIndex = 2;
            
        }
        string spRead;
        bool timer6finished=false;
        int ReadBytesLastCycle=0;
        int bytesCount = 0;
        int blankCounter = 0;
        int resendCounter = 0;
        bool searchMode = false;
        int resendCommandCounter = 0;
        int delay = 0;
        bool waitOneCycle = false;
        bool answerWaiting = false;
        int savedInterval = 1000;
        int ReadDataCRCError;//счетчик ошибок, считает до 5ти, а дальше отключает опрос
        bool ReadDataCRCOk=false;
        int CRCerrors = 0;
        int MessagesRecieved = 0;
        string[] tmpItem = { "slaveaddress", "devtype", "Serial#", "software rev.","range"};
        string upperlimit = "20";
        string lowerlimit = "0";
        delegate void SetTextCallback(string text);
        private Thread demoThread = null;
        private void Form1_Load(object sender, EventArgs e)
        {
    
               
            //}
           // comPortFinder();
            while (comboBox1.Items.Count == 0)
            {
               // this.label6.Text = "HART-модем не найден";
                var result = MessageBox.Show("Подключите HART-модем к компьютеру для начала работы!", "Внимание!",MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    string[] ports = SerialPort.GetPortNames();
                    //this.comboBox1.Items.AddRange(portnames);
                    foreach (string port in ports)//формируем массив доступных для открытия портов
                    {
                        this.comboBox1.Items.Add(port);
                        toolStripComboBox1.Items.Add(port);
                    }
                }
                //comPortFinder();
                else
                {

                    this.Close();
                    break;
                }
            }
            this.comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
            toolStripComboBox1.SelectedIndex = comboBox1.Items.Count - 1;
          //  this.comboBox3.SelectedIndex = 0;
            serialPort1.ReceivedBytesThreshold = 1;// HartProtocol.WaitingBytesQ;
            //this.textBox3.Text = HartProtocol.WaitingBytesQ.ToString();
            this.textBox4.Text = HartProtocol.NumberOfPreambulas.ToString();
          //  this.textBox5.Text = timer1.Interval.ToString();
            this.comboBox2.SelectedIndex = 0;
            
            this.panel1.Enabled = false;
            button2.Enabled = false;
            this.panel3.Enabled = false;
            this.textBox2.AppendText((DateTime.Now.ToString()+" ---> ")+"Начало сессии пользователя\r\n");
            this.tabPage1.Hide();
            this.label6.Text = "Выберите COM порт";
            
            //пользовательToolStripMenuItem.Checked = true;
        }

        //private void comPortFinder()
        //{
        //    string[] ports = SerialPort.GetPortNames();
        //    string sInstanceName = string.Empty;
        //    string sPortName = string.Empty;
        //    bool bFound = false;
        //    //foreach (string port in ports)//формируем массив доступных для открытия портов
        //    //{
        //    //    this.comboBox1.Items.Add(port);
        //    //    toolStripComboBox1.Items.Add(port);
        //    if (ports.Length != 0)
        //        for (int y = 0; y < ports.Length; y++)
        //        {
        //            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSSerial_PortName");
        //            ManagementObjectCollection qcollection = searcher.Get();
        //            if ((qcollection != null) | (qcollection.Count != 0))
        //                foreach (ManagementObject queryObj in qcollection)
        //                {
        //                    sInstanceName = queryObj["InstanceName"].ToString();

        //                    if (sInstanceName.IndexOf("FTDIBUS\\VID_0403+PID_6001+A601PZS2A") > -1)
        //                    {
        //                        //    {queryObj = {\\ALEXANDR_S\root\WMI:MSSerial_PortName.InstanceName="FTDIBUS\\VID_0403+PID_6001+A601PZS2A\\0000_0"}
        //                        sPortName = queryObj["PortName"].ToString();
        //                        for (int i = 0; i < comboBox1.Items.Count; i++)
        //                        {
        //                            if (sPortName == comboBox1.Items[i].ToString())
        //                                bFound = true;
        //                        }
        //                        if (!bFound)
        //                        {
        //                            this.comboBox1.Items.Add(sPortName);
        //                            toolStripComboBox1.Items.Add(sPortName);
        //                            //  port = new SerialPort(sPortName, 9600, Parity.None, 8, StopBits.One);

        //                            break;
        //                        }

        //                    }

        //                }


        //        }
        //}
        private void button1_Click(object sender, EventArgs e)//открываем СОМ порт
        {

            this.errorProvider1.Clear();
            try
            {
                if(button1.Text == "Открыть COM порт")
                {

                    if (!serialPort1.IsOpen)
                    {
                        this.label6.Text = "Нажмите <Поиск> для определения подключённых к СОМ порту устроств";
                        button1.Text = "Закрыть СОМ порт";
                        serialPort1.PortName = this.comboBox1.SelectedItem.ToString();
                        toolStripComboBox1.SelectedIndex = comboBox1.SelectedIndex;
                        comboBox1.Enabled = false;
                        toolStripComboBox1.Enabled = false;
                        serialPort1.Open();
                        textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "открыт последовательный порт " + this.comboBox1.SelectedItem.ToString() + "\r\n");
                        //this.panel2.Enabled = true;
                        //this.groupBox2.Enabled = true;
                        this.panel3.Enabled = true;
                        this.button11.Enabled = false;
                        checkBox1.Checked = false;
                        if (panel1.Enabled) button2.Enabled = true;
                        открытьToolStripMenuItem.Enabled = false;
                        закрытьToolStripMenuItem.Enabled = true;
                    }
                    else
                    {

                      //  serialPort1.Close();
                    }
                }
                else if(button1.Text == "Закрыть СОМ порт")
                {
                    if (serialPort1.IsOpen)
                    {
                       // serialPort1.PortName = this.comboBox1.SelectedItem.ToString();
                        timer1.Stop();
                        timer2.Stop();
                        timer3.Stop();
                        serialPort1.Close();
                       // this.panel2.Enabled = false;
                        this.groupBox2.Enabled = false;
                        groupBox4.Enabled = false;
                        groupBox3.Enabled = false;
                        groupBox5.Enabled = false;
                        button1.Text = "Открыть COM порт";
                        textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "закрыт последовательный порт " + this.comboBox1.SelectedItem.ToString() + "\r\n");
                        comboBox1.Enabled = true;
                        toolStripComboBox1.Enabled = true;
                        закрытьToolStripMenuItem.Enabled = false;
                        открытьToolStripMenuItem.Enabled = true;
                        //this.panel1.Enabled = false;
                        panel3.Enabled = false;
                        button2.Enabled = true;
                        listView1.Items.Clear();
                        this.label6.Text = "Выберите COM порт";
                        comboBox1.SelectedIndex = -1;
                    }
                   // else serialPort1.Open();
                }
                

            }
            catch (Exception ex)
            {
                //errorProvider1.SetError(this.serialPort1, ex.Message);
                //this.errorProvider1.
               // this.textBox2.AppendText(ex.Message);
               // this.textBox2.AppendText("\r\n");
               // this.errorProvider1.SetError(comboBox1, "Ошибка при выборе COM порта, он либо отсутствует, либо занят, выберите другой");

                if (button1.Text == "Закрыть СОМ порт")
                {
                    button1.Text = "Открыть COM порт";
                    comboBox1.SelectedIndex = 0;
                    comboBox1.Enabled = true;
                    var result = MessageBox.Show("Внимание! Данный последовательный порт уже используется другой программой, либо отключён. Подключите его к компьютеру, либо отключите использующую его программу и попробуйте ещё раз.", "Предупреждение");
                }
                else
                {
                    button1.Text = "Закрыть СОМ порт";
                    comboBox1.SelectedIndex = -1;
                    comboBox1.Enabled = true;
                }
            }
            finally
            {
                
            }
        }

        private void button2_Click(object sender, EventArgs e)//кнопка отправки
        {
            SendHartMessage();

        }

        private void SendHartMessage()
        {
            float ftext;
            answerWaiting = true;
            if (listView1.Items.Count > 0)
            {
                if (listView1.Items.Count == 1)
                {
                    if (timer2.Enabled == false)
                    {
                        listView1.Items[0].Focused = true;
                        listView1.Items[0].Selected = true;
                        HartProtocol.SlaveAddress = Convert.ToByte(listView1.Items[0].Text);
                        Debug.WriteLine("index in send hart mes1");
                    }
                }
                else
                {
                    if (timer2.Enabled == false)
                    {
                        HartProtocol.SlaveAddress = Convert.ToByte(listView1.FocusedItem.Text);
                        Debug.WriteLine("index in send hart mes2");
                    }
                }

            }
            if (this.comboBox2.SelectedIndex == 17)
            {
                HartProtocol.PVunitsCode = (int)Convert.ToInt32("20");
                float.TryParse("20", System.Globalization.NumberStyles.Currency, CultureInfo.CurrentCulture, out ftext);
                HartProtocol.UpperRangeLimit = BitConverter.GetBytes(ftext);
                float.TryParse("0", System.Globalization.NumberStyles.Currency, CultureInfo.CurrentCulture, out ftext);
                HartProtocol.LowerRangeLimit = BitConverter.GetBytes(ftext);
            }

            //if ((this.checkBox1.Checked) & (this.timer1.Enabled)) this.checkBox1.Checked = false;//снимаем галочку циклического отправления в случае, если следует единичная отправка
            //else
            {
                byte[] buf = HartProtocol.AppendCRC(HexToByte(this.textBox1.Text));//добавляем контрольную сумму к запросу
                HartProtocol.lastCommand = buf[6];
                if (serialPort1.IsOpen) serialPort1.Write(
                      buf, 0, buf.GetLength(0));//в этой части мы должны устанавливать количество байт, ожидаемых к приему WaitingBytesQ
                //this.textBox2.Text = HartProtocol.CalculateCRC(buf).ToString();
                timer3.Start();
                if ((this.checkBox1.Checked) &(HartProtocol.lastCommand == 0x01))
                {
                    this.timer1.Interval = Convert.ToInt16(this.numericUpDown2.Value);//textBox5.Text);//проверяем, что записано в графе интервала таймера, устанавливаем в качестве интервала и
                    this.timer1.Start();//стартуем таймер
                }
            }

        }

        private void backgroundWorker1_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            //if (messageRecieved)
            //{
            //    this.textBox2.Text = spRead;
            //    messageRecieved = false;
            //}
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
           
                //int bytestoread, i = 0;
                
                //if (serialPort1.BytesToRead == ReadBytesLastCycle)//serialPort1.BytesToRead >= HartProtocol.WaitingBytesQ + 1)
                //{
                //bytestoread = serialPort1.BytesToRead;
                //byte[] buffer = new byte[bytestoread];
                //for (i = 0; i < bytestoread; ++i)// (serialPort1.BytesToRead > 0) 
                //    {
                //    buffer[i] = (byte)serialPort1.ReadByte();

                //    }
 
                //spRead = ByteToHex(buffer);
                ////byte[] buffer_= HartProtocol.CutOffPreambulasRecieved(buffer);
                
                //if (HartProtocol.CheckCRC(buffer) == 1) spRead += " ---> CRC OK!";
                //else spRead+=" ---> CRC Wrong!";
                //HartProtocol.GenerateAnswer(buffer);
                //this.serialPort1.DiscardInBuffer();
                //this.serialPort1.DiscardOutBuffer();
                //this.demoThread =
                //    new Thread(new ThreadStart(this.ThreadProcSafe));

                //this.demoThread.Start();

                //ReadBytesLastCycle = 0;
                //}
               // ReadBytesLastCycle = serialPort1.BytesToRead;//обновляем количество байт, ожидающих чтения в буфере
                
        }
        private void ThreadProcSafe()
        {
            this.SetText(spRead);
        }
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.

            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
               // this.textBox2.Clear();
                //text += "\r\n";
                this.label18.Text = "принято корректных сообщений " + Math.Round(((Convert.ToDouble(MessagesRecieved - CRCerrors))*100 / MessagesRecieved),1).ToString() + " %";//проценты удачно принятых сообщений
                this.textBox2.AppendText(DateTime.Now.ToString()+" ---> ");//добавляем к принятому сообщению дату и красивую стрелочку
                this.textBox2.AppendText(text);//  = text;
                this.textBox2.AppendText("\r\n");//переходим на следующую строку
            }
            //ниже представлены формы для каждого вида принятых команд по номерам
            if (HartProtocol.RecievedCommand == 0x00)
            {
                this.textBox6.Text = Convert.ToString(HartProtocol.ManufacturerID);
                this.textBox7.Text = Convert.ToString(HartProtocol.DevTypeCode);
                this.textBox8.Text = Convert.ToString(HartProtocol.NumberOfPreambulas);
                this.textBox11.Text = Convert.ToString(HartProtocol.UniversalComRev);
                this.textBox9.Text = Convert.ToString(HartProtocol.DevSpecComRev);
                this.textBox10.Text = Convert.ToString(HartProtocol.SoftwareRev);
                this.textBox14.Text = Convert.ToString(HartProtocol.HardwareRev);
                this.textBox13.Text = Convert.ToString(HartProtocol.DevFuncFlags);
                this.textBox12.Text = Convert.ToString(HartProtocol.DevIDNumber);
                this.textBox3.Text = "0x"+Convert.ToString(ByteToHex(HartProtocol.DeviceTypeID));
                this.textBox19.Text = "0x" + Convert.ToString(ByteToHex(HartProtocol.SoftwareVer));
                this.textBox20.Text = "0x" + Convert.ToString(ByteToHex(HartProtocol.DeviceSoftwareCRC));
            }
            if (HartProtocol.RecievedCommand == 0x01)
            {

                this.textBox7.Text = Math.Round(BitConverter.ToSingle(HartProtocol.PV, 0),2).ToString();
                this.textBox6.Text = Convert.ToString(HartProtocol.PVunitsCode);
                this.textBox18.Text = Math.Round(BitConverter.ToSingle(HartProtocol.PV, 0),2).ToString();
                Single currentval,pv,sensordiap;
                
                pv = BitConverter.ToSingle(HartProtocol.PV, 0);
                currentval = 4 + 16 * (pv/HartProtocol.PVunitsCode);
                sensordiap = 100*pv / HartProtocol.PVunitsCode;
                this.textBox15.Text = Math.Round(currentval, 2).ToString();
                this.textBox16.Text = Math.Round(sensordiap, 2).ToString();
              //  this.textBox17.Text = "0 - "+Convert.ToString(HartProtocol.PVunitsCode)+" мм/с";
                //this.textBox7.Text = ByteToHex(HartProtocol.PV);
            }
            if (HartProtocol.RecievedCommand == 0x02)
            {
                
                this.textBox6.Text = Math.Round(BitConverter.ToSingle(HartProtocol.SensorCurrentValue, 0),2).ToString(); 
                this.textBox7.Text = Math.Round(BitConverter.ToSingle(HartProtocol.SensorDiapPrcnt, 0),2).ToString();
                this.textBox15.Text = Math.Round(BitConverter.ToSingle(HartProtocol.SensorCurrentValue, 0),2).ToString();
                this.textBox16.Text = Math.Round(BitConverter.ToSingle(HartProtocol.SensorDiapPrcnt, 0),2).ToString();
            }
            if (HartProtocol.RecievedCommand == 0x30)
            {
                this.textBox6.Text = Math.Round(BitConverter.ToSingle(HartProtocol.calibrationK, 0),2).ToString();
                this.textBox7.Text = Math.Round(BitConverter.ToSingle(HartProtocol.calibrationB, 0),2).ToString();
                this.textBox21.Text = Math.Round(BitConverter.ToSingle(HartProtocol.calibrationK, 0), 2).ToString();
                this.textBox22.Text = Math.Round(BitConverter.ToSingle(HartProtocol.calibrationB, 0),2).ToString();
                
            }

        }

        #region ByteToHex
        /// <summary>
        /// method to convert a byte array into a hex string
        /// </summary>
        /// <param name="comByte">byte array to convert</param>
        /// <returns>a hex string</returns>
        private string ByteToHex(byte[] comByte)
        {
            //create a new StringBuilder object
            StringBuilder builder = new StringBuilder(comByte.Length * 3);
            //loop through each byte in the array
            foreach (byte data in comByte)
                //convert the byte to a string and add to the stringbuilder
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
            //return the converted value
            return builder.ToString().ToUpper();
        }
        #endregion
        #region HexToByte
        /// <summary>
        /// method to convert hex string into a byte array
        /// </summary>
        /// <param name="msg">string to convert</param>
        /// <returns>a byte array</returns>
        private byte[] HexToByte(string msg)
        {
            //remove any spaces from the string
            msg = msg.Replace(" ", "");
            //create a byte array the length of the
            //string divided by 2
            byte[] comBuffer = new byte[msg.Length / 2];
            //loop through the length of the provided string
            for (int i = 0; i < msg.Length; i += 2)
                //convert each set of 2 characters to a byte
                //and add to the array
                comBuffer[i / 2] = (byte)Convert.ToByte(msg.Substring(i, 2), 16);
            //return the array
            //byte[] buf2 = HartProtocol.AppendCRC(comBuffer);
            return comBuffer;
        }
        #endregion



        private void serialPort1_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            //this.textBox2.Text = spRead;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
 
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (waitOneCycle)
            {
                waitOneCycle = false;
                timer1.Interval = savedInterval;
                this.timer1.Start();
            }
            else
            {
                if (this.checkBox1.Checked)//если установлена галка циклической отправки
                {
                    this.serialPort1.DiscardInBuffer();//обнуляем буфер входящих
                    this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(1));
                    byte[] buf = HartProtocol.AppendCRC(HexToByte(this.textBox1.Text));//сообщение в графе "на отправку" не изменяется, но к нему добавляется контрольная сумма
                    HartProtocol.lastCommand = buf[6];
                    if (serialPort1.IsOpen)
                        serialPort1.Write(buf, 0, buf.GetLength(0));//шлём сообщение
                    this.timer1.Start();//стартуем таймер
                    this.timer3.Start();
                }
                else this.timer1.Stop();
            }
        }

        private void button3_Click(object sender, EventArgs e)//работаем с сообщением на отправку
        {
            //HartProtocol.NumberOfPreambulas = (int)Convert.ToInt16(this.textBox4.Text);
            //HartProtocol.WaitingBytesQ = (int)Convert.ToInt16(this.textBox3.Text);
            if(serialPort1.IsOpen)this.serialPort1.DiscardInBuffer();
            float ftext;
            byte[] bfloat = new byte[4];
            //HartProtocol.WaitingBytesQ = HartProtocol.CommandDataAnswerBytes[this.comboBox2.SelectedIndex] + 7 + HartProtocol.NumberOfPreambulas_send;
            //serialPort1.ReceivedBytesThreshold = 1;//HartProtocol.CommandDataAnswerBytes[this.comboBox2.SelectedIndex]+7+HartProtocol.NumberOfPreambulas;
           /*так нужно работать с float,string,byte
            float ftext;
            float.TryParse(this.textBox6.Text.Replace(".",","),System.Globalization.NumberStyles.Currency,CultureInfo.CurrentCulture,out ftext);
            this.textBox7.Text = ftext.ToString();
            byte[] bfloat = new byte[4];
            byte[] bfloat_ = new byte[4];
            bfloat = BitConverter.GetBytes(ftext);
            for (int i = 0; i < 4; i++)
            {
                bfloat_[3 - i] = bfloat[i];
            }
            this.textBox8.Text=ByteToHex(bfloat_);*/
            // а вот так нужно работать с форматом HART ASCII:
            if(this.comboBox2.SelectedIndex==4)//преобразуем запись для соответствующих команд
            HartProtocol.SlaveAddress = (int)Convert.ToInt32(this.textBox6.Text);
            if(this.comboBox2.SelectedIndex==5)
            HartProtocol.Tag  = Encoding.ASCII.GetBytes(this.textBox6.Text);
            if (this.comboBox2.SelectedIndex==11)
            HartProtocol.Message = Encoding.ASCII.GetBytes(this.textBox6.Text);
            if (this.comboBox2.SelectedIndex == 12)
            {
                HartProtocol.Tag = Encoding.ASCII.GetBytes(this.textBox6.Text);
                HartProtocol.Descriptor = Encoding.ASCII.GetBytes(this.textBox7.Text);
                HartProtocol.Date = HexToByte(this.textBox8.Text);
            }
            if (this.comboBox2.SelectedIndex == 14)
            {
                HartProtocol.PVunitsCode = (int)Convert.ToInt32(this.textBox6.Text);
                float.TryParse(this.textBox7.Text.Replace(".", ","), System.Globalization.NumberStyles.Currency, CultureInfo.CurrentCulture, out ftext);
                HartProtocol.UpperRangeLimit = BitConverter.GetBytes(ftext);
                float.TryParse(this.textBox8.Text.Replace(".", ","), System.Globalization.NumberStyles.Currency, CultureInfo.CurrentCulture, out ftext);
                HartProtocol.LowerRangeLimit = BitConverter.GetBytes(ftext);
                //for (int i = 0; i < 4; i++)
                //{
                //    HartProtocol.MeasuredCurrent[3 - i] = bfloat[i];
                //}

            }
            if (this.comboBox2.SelectedIndex == 23) 
            {
                float.TryParse(this.textBox6.Text.Replace(".",","),System.Globalization.NumberStyles.Currency,CultureInfo.CurrentCulture,out ftext);
                HartProtocol.MeasuredCurrentZero = BitConverter.GetBytes(ftext);
                //for (int i = 0; i < 4; i++)
                //{
                //    HartProtocol.MeasuredCurrent[3 - i] = bfloat[i];
                //}
                
            }
            if (this.comboBox2.SelectedIndex == 24)
            {
                float.TryParse(this.textBox6.Text.Replace(".", ","), System.Globalization.NumberStyles.Currency, CultureInfo.CurrentCulture, out ftext);
                HartProtocol.MeasuredCurrentGain = BitConverter.GetBytes(ftext);
                //for (int i = 0; i < 4; i++)
                //{
                //    HartProtocol.MeasuredCurrent[3 - i] = bfloat[i];
                //}

            }
            if (this.comboBox2.SelectedIndex == 26) // запрос на получение калибровочных коэффициентов
            {

            }
            this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(this.comboBox2.SelectedIndex));
            //byte[] tmp = Encoding.ASCII.GetBytes(this.textBox4.Text);
            //this.textBox3.Text = ByteToHex(HartProtocol.ConvertASCIIToHartASCII(tmp));
            //this.textBox14.Text = ByteToHex(HartProtocol.ConvertHartASCIIToASCII(HexToByte(this.textBox3.Text)));
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)//если галка циклической отправки стоит, то разрешаем изменять интервал таймера
        {
            if (button8.Text == "Запрос измерений")
            {
                button8.Text = "Включить циклический запрос измерений";
            }
            else if (button8.Text == "Включить циклический запрос измерений")
            {
                button8.Text = "Запрос измерений";
            }
            else if (button8.Text == "Отключить циклический запрос измерений")
            {
                button8.Text = "Запрос измерений";
            }
           // if (this.checkBox1.Checked) this.numericUpDown2.Enabled = true;//textBox5.Enabled=true;
           // else this.numericUpDown2.Enabled = false;//this.textBox5.Enabled = false;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)//выбор формы для соответствующего запроса
        {
            this.disableUnused(0);
            
            if (this.comboBox2.SelectedIndex == 0)
            {
                this.label9.Text = "код изготовителя";
                this.label10.Text = "код типа устройств";
                this.label11.Text = "число преамбул";
                this.label12.Text = "ревизия универсальных команд";
                this.label13.Text = "ревизия специфических команд";
                this.label14.Text = "ревизия ПО";
                this.label15.Text = "ревизия АО";
                this.label16.Text = "флаги функций изделия";
                this.label17.Text = "№ ID изделия";
                
            }
            if (this.comboBox2.SelectedIndex == 1)
            {
                this.label9.Text = "код единиц измерения первичной переменной";
                this.label10.Text = "первичная переменная";
            }
            if (this.comboBox2.SelectedIndex == 2)
            {
                this.label9.Text = "ток, мА";
                this.label10.Text = "процент диапазона";
            }
            if (this.comboBox2.SelectedIndex == 3)
            {
                this.label9.Text = "ток, мА";
                this.label10.Text = "код единиц измерения первичной переменной";
                this.label11.Text = "первичная переменная";
            }
            if (this.comboBox2.SelectedIndex == 4)
            {
                this.label9.Text = "адрес опроса";
                
            }
            if (this.comboBox2.SelectedIndex == 5)//принимать нужно, выставив предварительно переменные как в команде 0
            {
                this.label9.Text = "тэг";
            }
            if (this.comboBox2.SelectedIndex == 6)
            {
                this.label9.Text = "сообщение";
            }
            if (this.comboBox2.SelectedIndex == 7)
            {
                this.label9.Text = "тэг";
                this.label10.Text = "дескриптор";
                this.label11.Text = "дата";
            }
            if (this.comboBox2.SelectedIndex == 8)
            {
                this.label9.Text = "серийный номер чувствительного элемента";
                this.label10.Text = "код единиц измерения первичной переменной в установленных пределах";
                this.label11.Text = "верхний предел измерений чувствительного элемента";
                this.label12.Text = "нижний предел измерений чувствительного элемента";
                this.label13.Text = "минимальный шаг";
            }
            if (this.comboBox2.SelectedIndex == 9)
            {
                this.label9.Text = "код выбора оповещения";
                this.label10.Text = "код функции преобразования";
                this.label11.Text = "код единиц измерения границ диапазона";
                this.label12.Text = "верхний диапазон измерений";
                this.label13.Text = "нижний диапазон измерений";
                this.label14.Text = "код защиты от записи";
                this.label15.Text = "код индивидуального ярлыка дистрибьютора";
            }
            if (this.comboBox2.SelectedIndex == 10)
            {
                this.label9.Text = "серийный номер";
            }
            if (this.comboBox2.SelectedIndex == 11)
            {
                this.label9.Text = "сообщение";
            }
            if (this.comboBox2.SelectedIndex == 12)
            {
                this.label9.Text = "тэг";
                this.label10.Text = "дескриптор";
                this.label11.Text = "дата";
            }
            if (this.comboBox2.SelectedIndex == 13)
            {
                this.label9.Text = "серийный номер";
            }
            if (this.comboBox2.SelectedIndex == 14)
            {
                this.label9.Text = "код единиц измерения диапазона";
                this.label10.Text = "верхняя граница диапазона";
                this.label11.Text = "нижняя граница диапазона";
            }
            if (this.comboBox2.SelectedIndex == 18)
            {
                this.label9.Text = "ток, мА";
            }
            if (this.comboBox2.SelectedIndex == 22)
            {
                this.label9.Text = "код единиц измерения первичной переменной";
            }
            if ((this.comboBox2.SelectedIndex == 23)||(this.comboBox2.SelectedIndex == 24))
            {
                this.label9.Text = "измеренный ток, мА";
            }
            if (this.comboBox2.SelectedIndex == 25)
            {
                this.label9.Text = "код функции преобразования";
            }
            if (this.comboBox2.SelectedIndex == 26)
            {
                this.label9.Text = "специфический статус устройства";
            }
            if (this.comboBox2.SelectedIndex == 27)
            {
                this.label9.Text = "серийный номер чувствительного элемента";
            }
            if (this.comboBox2.SelectedIndex == 28)
            {
                this.label9.Text = "количество преамбул";
            }
            if (this.comboBox2.SelectedIndex == 29)
            {
                this.label9.Text = "номер команды";
            }
            if (this.comboBox2.SelectedIndex == 30)
            {
                this.label9.Text = "включение/выключение";
            }
            //if (this.comboBox2.SelectedIndex == 31)
            //{
            //    this.label9.Text = "установить вторую точку калибровки в 100% диапазона";
            //}
        }

        private void InitHartParametersOnRequest(int commNumber)
        {
            if(this.comboBox2.SelectedIndex==4)
            HartProtocol.SlaveAddress = (int)Convert.ToInt32(this.textBox6.Text);
            if (this.comboBox2.SelectedIndex == 5)
            {
                
            }

        }

        private void disableUnused(int usedParameters)//обнуляем поля в форме запроса
        {
            switch (usedParameters) 
            {
                case 0:
                    {
                        this.label9.Text = "";
                        this.label10.Text = "";
                        this.label11.Text = "";
                        this.label12.Text = "";
                        this.label13.Text = "";
                        this.label14.Text = "";
                        this.label15.Text = "";
                        this.label16.Text = "";
                        this.label17.Text = "";
                        this.textBox6.Text = "";
                        this.textBox7.Text = "";
                        this.textBox8.Text = "";
                        this.textBox9.Text = "";
                        this.textBox10.Text = "";
                        this.textBox11.Text = "";
                        this.textBox12.Text = "";
                        this.textBox13.Text = "";
                        this.textBox14.Text = "";
                        break;
                    }
                case 1:
                    {
                        this.label10.Text = "";
                        this.label11.Text = "";
                        this.label12.Text = "";
                        this.label13.Text = "";
                        this.label14.Text = "";
                        this.label15.Text = "";
                        this.label16.Text = "";
                        this.label17.Text = "";
                        break;
                    }
            }
        }

        private void serialPort1_PinChanged_1(object sender, SerialPinChangedEventArgs e)//событие при изменении состояния линии СОМ-порта, в нашем случае, когда принимаемое сообщение закончилось и выставлен соответствующий уровень линии
        {

        }

        private void button4_Click(object sender, EventArgs e)//калибровка 5%(0)
        {
           int length = HartProtocol.NumberOfPreambulas-1+3;
           byte[] tmp = new byte[length];
           tmp = HartProtocol.GenerateRequest(21);
           this.textBox1.Text = ByteToHex(tmp);
           SendHartMessage();

            
        }

        private void button5_Click(object sender, EventArgs e)//калибровка максимума
        {
            int length = HartProtocol.NumberOfPreambulas - 1 + 3;
            byte[] tmp = new byte[length];
            tmp = HartProtocol.GenerateRequest(31);
            this.textBox1.Text = ByteToHex(tmp);
            SendHartMessage();
        }

        private void Form1_TextChanged(object sender, EventArgs e)
        {
            //comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            //comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_TextUpdate(object sender, EventArgs e)
        {
            //comboBox1.SelectedIndex = 0;
        }

        private void comboBox1_Validating(object sender, CancelEventArgs e)
        {
            //string[] ports = SerialPort.GetPortNames();
            //foreach (string port in ports)//формируем массив доступных для открытия портов
            //{
            //    if(this.
            //}

        }

        private void button6_Click(object sender, EventArgs e)
        {
            button6.Enabled = false;
            timer5.Start();
            label6.Text = "";
            float ftext;
            serialPort1.DiscardInBuffer();
            serialPort1.DiscardOutBuffer();
            if (comboBox3.SelectedIndex == 0) upperlimit = "10";
            if (comboBox3.SelectedIndex == 1) upperlimit = "20";
            if (comboBox3.SelectedIndex == 2) upperlimit = "30";
            if (comboBox3.SelectedIndex == 3) upperlimit = "50";
            HartProtocol.PVunitsCode = (int)Convert.ToInt32(upperlimit);
            float.TryParse(upperlimit, System.Globalization.NumberStyles.Currency, CultureInfo.CurrentCulture, out ftext);
            HartProtocol.UpperRangeLimit = BitConverter.GetBytes(ftext);
            float.TryParse(lowerlimit, System.Globalization.NumberStyles.Currency, CultureInfo.CurrentCulture, out ftext);
            HartProtocol.LowerRangeLimit = BitConverter.GetBytes(ftext);
            this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(14));
            if(timer1.Enabled)
            {
                waitOneCycle = true;
                savedInterval = timer1.Interval;
                timer1.Stop();
                timer1.Interval = 1000;
                timer1.Start();
            }
            //this.textBox2.Text = ByteToHex(HartProtocol.GenerateRequest(14));
            SendHartMessage();
            //timer4.Interval = 2000;
            //timer4.Start();
            //еще один делэй на 1 сек


        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(2));
            SendHartMessage();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //button8.Enabled = false;
            //timer5.Interval = 300;
            //timer5.Start();
            //timer5.Interval = 1000;
            label6.Text = "";

            if (checkBox1.Checked)
            {
                if (button8.Text == "Включить циклический запрос измерений")
                {
                    this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(1));
                    SendHartMessage();
                    blankCounter = 0;
                    button8.Text = "Отключить циклический запрос измерений";
                }
                else if (button8.Text == "Отключить циклический запрос измерений")
                {
                    checkBox1.Checked = false;
                }
            }
            else
            {
                this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(1));
                SendHartMessage();
                blankCounter = 0;
            }



        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (textBox1.Enabled)
            {
                textBox1.Enabled = false;
                button2.Enabled = true;
            }
            else textBox1.Enabled = true;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            label6.Text = "";
            timer1.Stop();
            var result = MessageBox.Show("Внимание! При сбросе калибровочные коэффициенты примут значения, установленные <по умолчанию>, это приведёт к изменению метрологически значимых настроек. Если Вы не уверены в необходимости сброса - откажитесь от её проведения, нажав <Отмена>, иначе - <OK>", "Предупреждение", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                int length = HartProtocol.NumberOfPreambulas - 1 + 3;
                byte[] tmp = new byte[length];
                tmp = HartProtocol.GenerateRequest(17);
                this.textBox1.Text = ByteToHex(tmp);
                SendHartMessage();
            }
           // this.textBox2.AppendText("Возвращены заводские установки \r\n");
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)
            {
                serialPort1.PortName = this.toolStripComboBox1.SelectedItem.ToString();
                comboBox1.SelectedIndex = toolStripComboBox1.SelectedIndex;
                comboBox1.Enabled = false;
                toolStripComboBox1.Enabled = false;
                textBox2.AppendText((DateTime.Now.ToString() + " ---> ")+"открыт последовательный порт " + this.comboBox1.SelectedItem.ToString() + "\r\n");
                serialPort1.Open();

            }
            button1.Text = "Закрыть СОМ порт";
            button2.Enabled = true;
            закрытьToolStripMenuItem.Enabled = true;
            открытьToolStripMenuItem.Enabled = false;
            panel3.Enabled = true;
            this.button11.Enabled = false;
            if (panel1.Enabled) button2.Enabled = true;
            
            //listView1.Items.Clear();
        }
        /*
         *                    if (!serialPort1.IsOpen)
                    {
                        button1.Text = "Закрыть СОМ порт";
                        serialPort1.PortName = this.comboBox1.SelectedItem.ToString();
                        toolStripComboBox1.SelectedIndex=comboBox1.SelectedIndex;
                        comboBox1.Enabled = false;
                        toolStripComboBox1.Enabled = false;
                        serialPort1.Open();
                        textBox2.AppendText((DateTime.Now.ToString()+" ---> ")+"открыт последовательный порт " + this.comboBox1.SelectedItem.ToString()+"\r\n");
                        //this.panel2.Enabled = true;
                        //this.groupBox2.Enabled = true;
                        this.panel3.Enabled = true;
                        this.button11.Enabled = false;

                        if (panel1.Enabled) button2.Enabled = true;
                        открытьToolStripMenuItem.Enabled = false;
                        закрытьToolStripMenuItem.Enabled = true;
                    }
                    else serialPort1.Close();
         */
        private void закрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                //serialPort1.PortName = this.comboBox1.SelectedItem.ToString();
                textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "закрыт последовательный порт " + this.comboBox1.SelectedItem.ToString() + "\r\n");
                serialPort1.Close();
                toolStripComboBox1.Enabled = true;
                comboBox1.Enabled = true;
            }
            button1.Text = "Открыть COM порт";
            button2.Enabled = false;
            закрытьToolStripMenuItem.Enabled = false;
            открытьToolStripMenuItem.Enabled = true;
            panel3.Enabled = false;
            listView1.Items.Clear();
            panel1.Enabled = false;
           // panel2.Enabled = false;
            groupBox2.Enabled = false;
            //if (panel1.Enabled) button2.Enabled = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.label6.Text = "Нажмите <Открыть COM порт>";
            this.button1.Enabled = true;
            for (int i = 0; i < comboBox1.Items.Count; i++)
            {
                serialPort1.PortName = comboBox1.Items[i].ToString();
                //if (serialPort1.PortName == comboBox1.Items[i])
                    if (serialPort1.IsOpen)
                        comboBox1.Items.RemoveAt(i);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (toolStripTextBox1.Text == "VT003")
            {
                tabControl1.Enabled = true;
                panel1.Enabled = true;
                администраторToolStripMenuItem.Checked = true;
                пользовательToolStripMenuItem.Checked = false;
                toolStripTextBox1.Text = "";
                if (button1.Text == "Закрыть COM порт") button2.Enabled = true;
                textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "Начало сессии администратора\r\n");
            }
                
        }

        private void пользовательToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.Enabled = true;
            panel1.Enabled = false;
            пользовательToolStripMenuItem.Checked = true;
            администраторToolStripMenuItem.Checked = false;
        }

        private void toolStripTextBox1_Enter(object sender, EventArgs e)
        {

        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (toolStripTextBox1.Text == "VT003")
                {
                    tabControl1.Enabled = true;
                    panel1.Enabled = true;
                    администраторToolStripMenuItem.Checked = true;
                    пользовательToolStripMenuItem.Checked = false;
                    toolStripTextBox1.Text = "";
                    if (button1.Text == "Закрыть COM порт") button2.Enabled = true;
                    textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "Начало сессии администратора\r\n");
                }
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            timer5.Start();
            button10.Enabled = false;
            progressBar1.Visible = true;
            progressBar1.Value = 0;
            searchMode = true;
            if (this.button10.Text == "Поиск")
            {
                listView1.Items.Clear();
                button11.Enabled = false;
                groupBox5.Enabled = false;
                groupBox4.Enabled = false;
                groupBox3.Enabled = false;
                //  panel2.Enabled = false;
                groupBox2.Enabled = false;
              //  this.label6.Text = "Нажмите <Поиск> для определения подключённых к СОМ порту устроств";
                ReadDataCRCOk = false;
                ReadDataCRCError = 0;
                textBox3.Clear();
                textBox15.Clear();
                textBox16.Clear();
                textBox18.Clear();
                textBox19.Clear();
                textBox20.Clear();
                textBox21.Clear();
                textBox22.Clear();
                textBox23.Clear();
                textBox24.Clear();
                HartProtocol.SlaveAddress = 0;
                HartProtocol.LastSendedCommand = 0;
                this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(0));
                this.serialPort1.DiscardInBuffer();
                this.serialPort1.DiscardOutBuffer();
                this.textBox2.AppendText("\r\n"+(DateTime.Now.ToString() + " ---> ") + "Поиск доступных устройств начат\r\n");
                SendHartMessage();
                timer2.Start();
                this.button10.Text = "Стоп";
            }
            else
            {
                this.timer2.Stop();
                searchMode = false;
               // ReadDataCRCOk = false;
                progressBar1.Value = 15;
                HartProtocol.SlaveAddress = 0;
                HartProtocol.LastSendedCommand = 0;
                HartProtocol.SensorSerialNumber = 0;
                this.serialPort1.DiscardInBuffer();
                this.serialPort1.DiscardOutBuffer();
                this.button10.Text = "Поиск";
                this.textBox2.AppendText("\r\n" + (DateTime.Now.ToString() + " ---> ") + "Поиск доступных устройств завершен\r\n");
               // MessageBox.Show( "Поиск устройств завершен!", "Внимание!");
                progressBar1.Value = 0;
                if(listView1.Items.Count!=0)
                    this.label6.Text = "Если в списке более 1-го подключённого устройства," + "\r\n" + " выберите устройство для дальнейшей работы";
                else
                    this.label6.Text = "Ни одного устройства не обнаружено," + "\r\n" + " проверьте корректность подключения датчика и наличие питания";
                timer3.Stop();
              //  timer3.Stop();
                this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(1));
                SendHartMessage();
                blankCounter = 0;
                timer3.Stop();
                //ReadDataCRCError = 7;
                //if (listView1.Items.Count != 0)
                //{
                    
                //}
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
         //   listView1.Items.Remove(listView1.FocusedItem);
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if ((listView1.Items[i].Focused == true)|(listView1.Items[i].Selected==true))
                    listView1.Items[i].Remove();
                //button11.Enabled = false;
                //groupBox5.Enabled = false;
                //groupBox4.Enabled = false;
                //groupBox3.Enabled = false;
                ////  panel2.Enabled = false;
                //groupBox2.Enabled = false;
            }
            if (listView1.Items.Count == 0)
            {
                button11.Enabled = false;
                groupBox5.Enabled = false;
                groupBox4.Enabled = false;
                groupBox3.Enabled = false;
              //  panel2.Enabled = false;
                groupBox2.Enabled = false;
                textBox3.Clear();
                textBox15.Clear();
                textBox16.Clear();
                textBox18.Clear();
                textBox19.Clear();
                textBox20.Clear();
                textBox21.Clear();
                textBox22.Clear();
                textBox23.Clear();
                textBox24.Clear();
                this.label6.Text = "Нажмите <Поиск> для определения подключённых к СОМ порту устроств";
            }
           // else listView1.SelectedItems.IndexOf = listView1.FocusedItem.Index;
            //listView1.Items.GetEnumerator().MoveNext();
            //textBox19.Text = listView1.Items.GetEnumerator().Current.ToString();
        }
        private void AddDeviceToList(int mode)
        {
            string tmp;
            string[] itemdata = { "slaveaddress", "devtype", "Serial#", "software rev.", "range" };
            if (mode == 0)
            {

                tmpItem[0] = HartProtocol.ActualSlaveAddress.ToString();
                if (HartProtocol.DevTypeCode == 0xB3) tmpItem[1] = "ДВСТ-3";
//                itemdata[1] = HartProtocol.DevTypeCode.ToString();
                //double.TryParse("20", System.Globalization.NumberStyles.Currency, CultureInfo.CurrentCulture, out ftext);
                tmp = "B3_" +  (1 + (Convert.ToDouble(HartProtocol.SoftwareRev) / 10)).ToString();
                //tmp.
                //tmp.Replace("VT3_1,1", "VT3_1.1");
                tmp = tmp.Replace(",", ".");
                tmpItem[3] = tmp;
            }
            else if(mode==16)
            {

                tmpItem[2] = HartProtocol.SensorSerialNumber.ToString();
                ReadDataCRCOk = false;
                ListViewItem NewItem = new ListViewItem(tmpItem, 1);
                NewItem.UseItemStyleForSubItems = true;
                //NewItem.Name = "New";
                NewItem.ForeColor = Color.BlueViolet;
                //NewItem.Bounds.Size.Height = 100;
               // listView1.Items.Add(NewItem);
                //listView1.Items.GetEnumerator().MoveNext();
            }
            else if (mode == 1)
            {
                if (HartProtocol.PVunitsCode == 109) HartProtocol.PVunitsCode = 20;
                switch (HartProtocol.PVunitsCode)
                {
                    case 10:
                        {
                            comboBox3.SelectedIndex = 0;
                            break;
                        }
                    case 20:
                        {
                            comboBox3.SelectedIndex = 1;
                            break;
                        }
                    case 30:
                        {
                            comboBox3.SelectedIndex = 2;
                            break;
                        }
                    case 50:
                        {
                            comboBox3.SelectedIndex = 3;
                            break;
                        }

                }
                tmpItem[4] = "0..." + HartProtocol.PVunitsCode.ToString() + " мм/с";
                ReadDataCRCOk = false;
                ListViewItem NewItem = new ListViewItem(tmpItem, 1);
                NewItem.UseItemStyleForSubItems = true;
                NewItem.ForeColor = Color.BlueViolet;
                
                //NewItem.Selected = true;
                listView1.Items.Add(NewItem);
                //if(listView1.Items.Count!=0)
                  
                //if (listView1.FocusedItem == NewItem) ; //= NewItem;
                //NewItem.Selected = true;
               // listView1.SetBounds(
              //  listView1.Select();
                this.textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "добавлено устройство с адресом " + tmpItem[0] + "\r\n");
                if (listView1.Items.Count != 0)
                {
                    listView1.Items[0].Focused = true;
                    listView1.Items[0].Selected = true;
                    
                }
            }
            else
            {
                tmpItem[0] = HartProtocol.ActualSlaveAddress.ToString();
                tmpItem[1] = "Неизвестное устройство";
                tmpItem[2] = "Недоступно";
                tmpItem[3] = "Недоступно";
                tmpItem[4] = "Недоступно";
                ReadDataCRCOk = false;
                ListViewItem NewUnknownItem = new ListViewItem(tmpItem, 1);
                NewUnknownItem.UseItemStyleForSubItems = true;
                //NewItem.Name = "New";
                NewUnknownItem.ForeColor = Color.BlueViolet;
                //NewItem.Bounds.Size.Height = 100;
                listView1.Items.Add(NewUnknownItem);
                //listView1.Items.GetEnumerator().MoveNext();
            }
 

        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            //ReadDataCRCOk = false;
            int SendMessageMode=0;// 0 - обычный режим отправки запросов, сопровождается наращиванием адресов в случае отсутствия ответа (но не в случае ошибки CRC), 1 - повторная отсылка запроса на тот же адрес, в случае наличии ошибок CRC, 2 - отправка 2го запроса ответившему устройству для получения его серийного номера.
            if ((ReadDataCRCError > 0) & (ReadDataCRCError < 6)) SendMessageMode = 1;
            if ((ReadDataCRCError == 0) & (!ReadDataCRCOk))// это означает ошибку по таймауту, а значит переходим на следующий адрес
            {
                Debug.WriteLine("timeout error");
                textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "время ожидания истекло, опрашиваем следующий адрес...\r\n");
                HartProtocol.SlaveAddress++;
                HartProtocol.ActualSlaveAddress++;
                Debug.WriteLine(HartProtocol.SlaveAddress);
                progressBar1.Value++;
                ReadDataCRCOk = false;
                SendMessageMode = 0;
            }
            if ((ReadDataCRCError == 0) & (ReadDataCRCOk))//ошибок нет, а значит спрашиваем у датчика его серийник и диапазон
            {
                AddDeviceToList(HartProtocol.RecievedCommand);
                ReadDataCRCOk = false;
                switch (HartProtocol.RecievedCommand)
                {
                    case 0:
                        {
                            Debug.WriteLine("case 0");
                            SendMessageMode = 2;
                            ReadDataCRCOk = false;
                            break;
                        }
                    case 16:
                        {
                            Debug.WriteLine("case 16");
                            SendMessageMode = 3;
                            ReadDataCRCOk = false;
                            break;
                        }
                    default:
                        {
                            Debug.WriteLine("case default");
                            ReadDataCRCOk = false;
                            HartProtocol.SlaveAddress++;
                            progressBar1.Value++;
                            SendMessageMode = 0;
                            break;
                        }
                }

            }
            SendMessage(SendMessageMode);
            
            //if(HartProtocol.SlaveAddress<)
            if ((HartProtocol.SlaveAddress >= 0x0f) || (ReadDataCRCError >= 6))
            {
                this.button10.Text = "Поиск";
                searchMode = false;
                timer3.Stop();
                timer1.Stop();
                timer2.Stop();
                MessageBox.Show("Поиск устройств завершен!", "Внимание!");
                if(listView1.Items.Count!=0)
                    this.label6.Text = "Если в списке более 1-го подключённого устройства," + "\r\n" + " выберите устройство для дальнейшей работы";
                else
                    this.label6.Text = "Ни одного устройства не обнаружено," + "\r\n" + " проверьте корректность подключения датчика и наличие питания";

                progressBar1.Value = 0;
                this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(1));
                SendHartMessage();
                blankCounter = 0;
                timer3.Stop();
            }
            else SendHartMessage();
        }

        public void SendMessage(int sendMode)
        {

            if (sendMode == 0)
            {
                ReadDataCRCOk = false;
                this.textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "проверяем следующий адрес" + HartProtocol.SlaveAddress.ToString() + "\r\n");
                this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(0));
            }
            if (sendMode == 1)
            {
                ReadDataCRCOk = false;
                this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(HartProtocol.LastSendedCommand));
            }
            if (sendMode == 2)
            {
                ReadDataCRCOk = false;
                this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(10));
            }
            if (sendMode == 3)
            {
                ReadDataCRCOk = false;
                this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(1));//вообще нужно слать не 1ю команду, а 15ю (там, где есть собственно значения диапазона и парсить соответсна еЁ)
            }
        }

        private void мониторВТ003ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Программа разработана специально для конфигурирования и управления датчиком ДВСТ-3.\r\nг. Таганрог ТКБ 'Виброприбор'. Версия ПО 1.0, релиз от 15.03.2015", "О программе 'Конфигуратор ДВСТ-3'");
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
           // this.textBox2.AppendText(listView1.FocusedItem.Text);
            if (listView1.Items.Count > 0)
            {
                if (listView1.Items.Count == 1)
                {
                    listView1.Items[0].Focused = true;
                    listView1.Items[0].Selected = true;
                    HartProtocol.SlaveAddress = Convert.ToByte(listView1.Items[0].Text);
                    Debug.WriteLine("index changed1");
                }
                else
                {
                    HartProtocol.SlaveAddress = Convert.ToByte(listView1.FocusedItem.Text);
                    Debug.WriteLine("index changed2");
                }

            }

                
            
            //textBox2.AppendText(listView1.FocusedItem.ToString());
            if (listView1.Items.Count != 0)
            {
                button11.Enabled = true;
                groupBox5.Enabled = true;
            }
            else
            {
                button11.Enabled = false;
                groupBox5.Enabled = false;
            }
            
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            HartProtocol.SlaveAddress = Convert.ToByte(this.numericUpDown1.Value);
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
            {
                button11.Enabled = true;
                groupBox4.Enabled = true;
                groupBox3.Enabled = true;
                //panel2.Enabled = true;
                groupBox2.Enabled = true;
            }
            else button11.Enabled = false;

        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count != 0) button11.Enabled = true;
            else button11.Enabled = false;
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count != 0) button11.Enabled = true;
            else button11.Enabled = false;
        }

        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count != 0) button11.Enabled = true;
            else button11.Enabled = false;
        }

        private void listView1_MouseMove(object sender, MouseEventArgs e)//в случае, если ни один из элементов не выбран, кнопка "удалить" неактивна
        {
            //if (listView1.SelectedItems.Count != 0)
            //{
            //    button11.Enabled = true;
            //    //panel2.Enabled = true;
            //    groupBox2.Enabled = true;
            //}
            //else
            //{
            //    button11.Enabled = false;
            //    //panel2.Enabled = false;
            //    groupBox2.Enabled = false;
            //}
        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            button12.Enabled = false;
            timer5.Start();
            this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(0));
            if (timer1.Enabled)
            {
                waitOneCycle = true;
                savedInterval = timer1.Interval;
                timer1.Stop();
                timer1.Interval = 1000;
                timer1.Start();
            }
            SendHartMessage();
        }

        private void button13_Click(object sender, EventArgs e)
        {

            label6.Text = "";
            bool calibrationBreak = false;
            timer1.Stop();
            textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "Начало калибровки\r\n");
            var result = MessageBox.Show("Внимание! При проведении калибровки будут изменены калибровочные коэффициенты. Значения калибровочных коэффициентов должны быть указаны в свидетельстве о поверке. Если Вы не уверены в необходимости калибровки - откажитесь от её проведения, нажав <Отмена>, иначе - <OK>.", "Предупреждение", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                string tmp1="",tmp2="",text1="",text2="";
                switch (comboBox3.SelectedIndex)
                {
                    case 0:
                        {
                            tmp1 = "0,5 мм/с";
                            tmp2 = "10 мм/с";
                            break;
                        }
                    case 1:
                        {
                            tmp1 = "1 мм/с";
                            tmp2 = "20 мм/с";
                            break;
                        }
                    case 2:
                        {
                            tmp1 = "1,5 мм/с";
                            tmp2 = "30 мм/с";
                            break;
                        }
                    case 3:
                        {
                            tmp1 = "2,5 мм/с";
                            tmp2 = "50 мм/с";
                            break;
                        }
                }
               // text1 = ;
                result = MessageBox.Show("Для калибровки установите на образцовом средстве виброскорость " + tmp1 + ", частотой 80 Гц, нажмите кнопку <OK>, для прекращения калибровки - <Отмена>", "Выполнение калибровки", MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                {
                    System.Threading.Thread.Sleep(1000);
                    textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "Калибровка в первой точке 5% диапазона преобразования датчика...\r\n");
                    int length = HartProtocol.NumberOfPreambulas - 1 + 3;
                    byte[] tmp = new byte[length];
                    tmp = HartProtocol.GenerateRequest(21);
                    this.textBox1.Text = ByteToHex(tmp);
                    SendHartMessage();
                    result = MessageBox.Show("Для калибровки установите на образцовом средстве виброскорость "+tmp2+", частотой 80 Гц, нажмите кнопку <OK>, для прекращения калибровки - <Отмена>", "Выполнение калибровки", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        System.Threading.Thread.Sleep(1000);
                        textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "Калибровка во второй точке 100% диапазона преобразования датчика...\r\n");
                        length = HartProtocol.NumberOfPreambulas - 1 + 3;
                        tmp = new byte[length];
                        tmp = HartProtocol.GenerateRequest(31);
                        this.textBox1.Text = ByteToHex(tmp);
                        SendHartMessage();

                    }
                    else
                    {
                        textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "Калибровка во второй точке 100% от диапазона преобразования датчика отменена\r\n");
                        calibrationBreak = true;
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                    textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "Калибровка в первой точке 5% от диапазона преобразования датчика отменена\r\n");
                    result = MessageBox.Show("Для калибровки установите на образцовом средстве виброскорость "+tmp2+", частотой 80 Гц, нажмите кнопку <OK>, для прекращения калибровки - <Отмена>", "Выполнение калибровки", MessageBoxButtons.OKCancel);
                    if (result == DialogResult.OK)
                    {
                        System.Threading.Thread.Sleep(1000);
                        textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "Калибровка во второй точке 100% диапазона преобразования датчика...\r\n");
                        int length = HartProtocol.NumberOfPreambulas - 1 + 3;
                        byte[] tmp = new byte[length];
                        tmp = HartProtocol.GenerateRequest(31);
                        this.textBox1.Text = ByteToHex(tmp);
                        SendHartMessage();
                        //  textBox2.AppendText("Калибровка звершена\r\n");
                    }
                    else textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "Калибровка во второй точке 100% от диапазона преобразования датчика отменена\r\n");
                }
                if (calibrationBreak)
                {
                    textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "Калибровка прервана\r\n");
                   // calibrationBreak
                }
                else textBox2.AppendText((DateTime.Now.ToString() + " ---> ") + "Калибровка завершена\r\n");
            }
        }

        private void label19_Click(object sender, EventArgs e)
        {

        }
        //public void CloseIt()
        //{
        //    System.Threading.Thread.Sleep(2000);
        //    Microsoft.VisualBasic.Interaction.AppActivate(
        //         System.Diagnostics.Process.GetCurrentProcess().Id);
        //    System.Windows.Forms.SendKeys.SendWait(" ");
        //}
        private void timer3_Tick(object sender, EventArgs e)
        {
            timer3.Stop();
            bool exitFoo = false;
            //if (serialPort1.BytesToRead > 0)
            //{
            //    if (serialPort1.BytesToRead > bytesCount)
            //    {
            //        bytesCount = serialPort1.BytesToRead;

            //        Debug.WriteLine("bytes in port");
            //        Debug.WriteLine(serialPort1.BytesToRead.ToString());
            //        timer3.Start();
            //        delay = 0;
            //    }
            //    else
            //    {
            //        delay++;
            //        Debug.WriteLine("delaay increased, bytes in port");
            //        Debug.WriteLine(serialPort1.BytesToRead.ToString());
            //        if (delay > 8)
            //        {
            //            if (bytesCount < 7)//если число принятых байт меньше 7 (3 из которых преамбула), значит мы приняли кривое сообщение, нужно сделать запрос ещё раз
            //            {
            //                blankCounter++;
            //                if (blankCounter > 50)
            //                {
            //                    blankCounter = 0;
            //                    serialPort1.DiscardOutBuffer();
            //                    serialPort1.DiscardInBuffer();
            //                    Debug.WriteLine("need resend");
            //                    SendHartMessage();
            //                    resendCounter++;
            //                    if (resendCounter > 10)
            //                    {
            //                        resendCounter = 0;
            //                        timer3.Stop();
            //                    }
            //                }
            //                timer3.Start();
            //            }
            //            else
            //            {
            //                bytesCount = 0;
            //                resendCounter = 0;
            //                delay = 0;
            //                incomingMessageProcessor();
            //            }
            //        }
            //        else timer3.Start();
            //    }
            //}
            //else
            //{
            //    blankCounter++;
            //    if (blankCounter > 20)
            //    {
            //        blankCounter = 0;
            //        serialPort1.DiscardOutBuffer();
            //        serialPort1.DiscardInBuffer();
            //        Debug.WriteLine("need resend");
            //        SendHartMessage();
            //        resendCounter++;
            //        if (resendCounter > 10)
            //        {
            //            resendCounter = 0;
            //            timer3.Stop();
            //        }
            //    }
            //    timer3.Start();
            //}
            //if (serialPort1.BytesToRead - 10 >= HartProtocol.GetCommandDataLength(HartProtocol.lastCommand))
            //{
            //    Debug.WriteLine("bytes in port");
            //    Debug.WriteLine(serialPort1.BytesToRead.ToString());

            //    incomingMessageProcessor();
            //}
            //else
            //{
            if(answerWaiting)
            {
                if (serialPort1.BytesToRead >= 7)
                {
                    if (ReadBytesLastCycle == serialPort1.BytesToRead)
                        resendCounter++;
                    else
                        resendCounter = 0;
                    Debug.WriteLine("bytes in port");
                    Debug.WriteLine(serialPort1.BytesToRead.ToString());
                }
                else//resend hart message cycle
                {
                    if(!searchMode)
                        resendCommandCounter++;
                    resendCounter = 0;
                    switch (resendCommandCounter)
                    {
                        case 200:
                            SendHartMessage();
                            break;
                        case 400:
                            SendHartMessage();
                            break;
                        case 600:
                            SendHartMessage();
                            break;
                        case 800:
                            textBox2.AppendText("\r\n" + (DateTime.Now.ToString() + " ---> ")+"Внимание! Ответ от датчика не получен! Повторите запрос!");
                            answerWaiting = false;
                            resendCommandCounter = 0;
                            exitFoo = true;
                            break;
                    }

                }
                ReadBytesLastCycle = serialPort1.BytesToRead;
                if (resendCounter >= 10)
                {
                    answerWaiting = false;
                    Debug.WriteLine("bytes in port");
                    Debug.WriteLine(serialPort1.BytesToRead.ToString());
                    resendCounter = 0;
                    resendCommandCounter = 0;
                    incomingMessageProcessor();
                }
                else
                {
                    if (!exitFoo)
                        timer3.Start();
                    else
                        timer3.Stop();
                }
           }

        }
        private void incomingMessageProcessor()
        {

            int i = 0;

            {
                //int dataQ = HartProtocol.GetCommandDataLength(HartProtocol.lastCommand);
                //if ((serialPort1.BytesToRead > dataQ))
                //{

                byte[] buffer = new byte[serialPort1.BytesToRead];//new byte[serialPort1.BytesToRead];

                //for (i = 0; i < dataQ; i++)// (serialPort1.BytesToRead > 0) 
                //{
                //    buffer[i] = (byte)serialPort1.ReadByte();

                //}
                
                this.serialPort1.Read(buffer, 0, serialPort1.BytesToRead);
                //Array.Reverse(buffer);

                System.Diagnostics.Debug.WriteLine(ByteToHex(buffer));
                if (HartProtocol.CheckMessageIntegrity(buffer))
                {
                    // HartProtocol.CutOffGhostBytes(buffer);
                    Array.Reverse(buffer);
                    spRead = ByteToHex(buffer);

                    byte[] buffer_ = HartProtocol.CutOffPreambulasRecieved(buffer);
                    
                    buffer_ = HartProtocol.CutOffGhostBytes(buffer_);
                    StringBuilder builder = new StringBuilder(buffer_.Length * 3);
                    //loop through each byte in the array
                    foreach (byte data in buffer_)
                        //convert the byte to a string and add to the stringbuilder
                        builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
                    Debug.WriteLine(builder.ToString().ToUpper());
                    if (HartProtocol.CheckCRC(buffer_) == 1)
                    {
                        spRead += " ---> CRC OK!";
                        MessagesRecieved++;
                        ReadDataCRCError = 0;
                        ReadDataCRCOk = true;
                        if (HartProtocol.LastSendedCommand == 14)
                        {
                            
                            spRead += "\r\n" + (DateTime.Now.ToString() + " ---> ");
                            if (upperlimit == "10")
                            {
                                spRead += "установлен диапазон измерений 0-10 мм/с";
                                tmpItem[4] = "0...10 мм/с";
                            }
                            if (upperlimit == "20")
                            {
                                spRead += "установлен диапазон измерений 0-20 мм/с";
                                tmpItem[4] = "0...20 мм/с";
                            }
                            if (upperlimit == "30")
                            {
                                spRead += "установлен диапазон измерений 0-30 мм/с";
                                tmpItem[4] = "0...30 мм/с";
                            }
                            if (upperlimit == "50")
                            {
                                spRead += "установлен диапазон измерений 0-50 мм/с";
                                tmpItem[4] = "0...50 мм/с";
                            }
                            
                            ListViewItem NewItem = new ListViewItem(tmpItem, 1);
                            NewItem.UseItemStyleForSubItems = true;
                            NewItem.ForeColor = Color.BlueViolet;
                            listView1.Items.RemoveAt(0);
                            listView1.Items.Add(NewItem);
                            
                            
                        }
                    }
                    else
                    {
                        spRead += " ---> CRC Wrong!";
                        MessagesRecieved++;
                        CRCerrors++;
                        ReadDataCRCError++;
                        ReadDataCRCOk = false;
                    }

                    HartProtocol.GenerateAnswer(buffer_);


                    this.demoThread =
                        new Thread(new ThreadStart(this.ThreadProcSafe));

                    this.demoThread.Start();
                    // ReadBytesLastCycle = 0;
                    this.serialPort1.DiscardInBuffer();
                    this.serialPort1.DiscardOutBuffer();
                    this.serialPort1.Close();
                    this.serialPort1.Open();
                    ReadBytesLastCycle = 0;
                }
                // }
            }
            this.serialPort1.DiscardInBuffer();
            // ReadBytesLastCycle = serialPort1.BytesToRead;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            button14.Enabled = false;
            timer5.Start();
            label6.Text = "";
            this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(26));
            if (timer1.Enabled)
            {
                waitOneCycle = true;
                savedInterval = timer1.Interval;
                timer1.Stop();
                timer1.Interval = 1000;
                timer1.Start();
            }
            SendHartMessage();
        }

        private void textBox20_TextChanged(object sender, EventArgs e)
        {
            this.textBox23.Text = "CRC-16";
        }

        private void textBox23_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            this.textBox24.Text = "ПО ДВСТ-3";
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void listView1_ControlAdded(object sender, ControlEventArgs e)
        {
            //this.label6.Text = "OOOO";
        }

        private void listView1_TabIndexChanged(object sender, EventArgs e)
        {
            //this.label6.Text = "OOOO";
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            //this.label6.Text = "OOOO";
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            //this.label6.Text = "OOOO";
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //groupBox1.Width = this.Width / 2 - 3 * 15;
            int tmp = this.Width - this.MinimumSize.Width;
            Debug.WriteLine(tmp.ToString());

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (Int32)numericUpDown2.Value;
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            this.textBox1.Text = ByteToHex(HartProtocol.GenerateRequest(1));
            if (timer1.Enabled)
            {
                waitOneCycle = true;
                savedInterval = timer1.Interval;
                timer1.Stop();
                timer1.Interval = 1000;
                timer1.Start();
            }
            SendHartMessage();
            timer4.Stop();

        }

        private void numericUpDown2_Leave(object sender, EventArgs e)
        {
            timer1.Interval = (Int32)numericUpDown2.Value;
        }

        private void numericUpDown2_Validated(object sender, EventArgs e)
        {
            timer1.Interval = (Int32)numericUpDown2.Value;
        }

        private void numericUpDown2_KeyPress(object sender, KeyPressEventArgs e)
        {
            timer1.Interval = (Int32)numericUpDown2.Value;
        }

        private void numericUpDown2_ImeModeChanged(object sender, EventArgs e)
        {
            timer1.Interval = (Int32)numericUpDown2.Value;
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine(sender.ToString());
            timer5.Stop();
            button10.Enabled = true;
            button12.Enabled = true;
            button14.Enabled = true;
            button8.Enabled = true;
            button6.Enabled = true;
        }

        private void timer6_Tick(object sender, EventArgs e)
        {
            timer6.Stop();
            timer6finished = true;
            Debug.WriteLine("timer6 tick tick");
        }

 
    }
}