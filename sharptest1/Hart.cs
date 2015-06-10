//using Form1;
using System;
using System.Data;
using sharptest1;
using System.Text;
using System.Diagnostics;
public class HartProtocol

{
    //public HartProtocol()
    //{
        
    //}

    public static int WaitingBytesQ = 8, ManufacturerID, DevTypeCode, PVunitsCode, UniversalComRev, DevSpecComRev, SoftwareRev, HardwareRev, DevFuncFlags, DevIDNumber,RangeIndex=0;
    public static int NumberOfPreambulas_send = 4, RecievedCommand, NumberOfPreambulas=4;
    public static int BurstModeEnabled = 0;
    public static int LastSendedCommand = 0;
    public static int SlaveAddress=0x02;
    public static int MasterAddress = 0x80;
    public static int SensorSerialNumber = 0x0000;
    public static int lastCommand = 0;
    public static byte[] calibrationK = new byte[4];
    public static byte[] calibrationB = new byte[4];
    public static byte[] SensorCurrentValue = new byte[4];
    public static byte[] SensorDiapPrcnt = new byte[4];
    public static byte[] PV = new byte[4];
    public static byte[] Tag = new byte[8];
    public static byte[] Descriptor = new byte[16];
    public static byte[] Message = new byte[32];
    public static byte[] Date = new byte[3];
    public static byte[] DeviceTypeID = new byte[1];
    public static byte[] SoftwareVer = new byte[1];
    public static byte[] MeasuredCurrentZero = new byte[4];
    public static byte[] MeasuredCurrentGain = new byte[4];
    public static byte[] UpperRangeLimit = new byte[4];
    public static byte[] LowerRangeLimit = new byte[4];
    public static byte[] DeviceSoftwareCRC = new byte[2];
    public static byte ActualSlaveAddress = 0x00;
    public static byte ActualDevIDNumber = 0x00;
    public static byte ActualSoftwareRev = 0x00;
    public static byte ActualDevFuncFlags = 0x00;
    public static byte OperationMode = 0;
    public static int[] CommandDataAnswerBytes = new int[32]   { 12, 5, 8, 9, 1, 12, 24, 21, 16, 17,  3, 24, 21,  3,  9,  0,  0,  0,  4,  0,  0,  0,  1,  4,  4,  1, 14, 3,  1,   1,   1,   1};
    private static int[] CommandDataRequestBytes = new int[32] { 0,  0, 0, 0, 1,  6,  0,  0,  0,  0,  0, 24, 21,  3,  9,  0,  0,  0,  4,  0,  0,  0,  1,  4,  4,  1, 16, 3,  1,   1,   1,   0};
    private static int[] CommandNumbersArray = new int[32]     { 0,  1, 2, 3, 6, 11, 12, 13, 14, 15, 16, 17, 18, 19, 35, 36, 37, 38, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 59, 108, 109, 111};
    public static byte[] GenerateRequest(int commNumber)
    {
        byte[] messageToSend = new byte[HartProtocol.NumberOfPreambulas];
        LastSendedCommand = commNumber;
        for (int i = 0; i < NumberOfPreambulas;i++ ) messageToSend[i] = 0xff;
        Array.Resize(ref messageToSend, messageToSend.Length + 4 + CommandDataRequestBytes[commNumber]);
        if (HartProtocol.BurstModeEnabled == 1) { }
        else messageToSend[messageToSend.Length - 4 - CommandDataRequestBytes[commNumber]] = 0x02;//формируем стартовый символ, в данном случае он шлется от главного (0х02 - от главного, 0х06 - от подчиненного, 0х01 - пакетный режим)
        messageToSend[messageToSend.Length - 3 - CommandDataRequestBytes[commNumber]] = (byte)(MasterAddress | SlaveAddress | BurstModeEnabled);//формируем байт адреса, он составляется из адреса главного устройства(старший бит), адреса подчиненного устройства (4 младших бита) и флага пакетного режима (6й бит)
        messageToSend[messageToSend.Length - 2 - CommandDataRequestBytes[commNumber]] = (byte)CommandNumbersArray[commNumber];//формируем байт номера команды, который находится в соответствующем массиве
        messageToSend[messageToSend.Length - 1 - CommandDataRequestBytes[commNumber]] = (byte)CommandDataRequestBytes[commNumber];//формируем байт числа, который находится в соответствующем массиве
        if (CommandDataRequestBytes[commNumber]!=0)
        {
            byte[] buf = ReadInputdataToSend(commNumber); 
            for (int i = 0; i < buf.Length;i++ )
                messageToSend[messageToSend.Length - buf.Length + i] = buf[i];
        }

        return AppendCRC(messageToSend);
        
    }
    public static void GenerateAnswer(byte[] ReadBuff_)
    {
        int commNumber=0;
        //Array.Resize(ref ReadBuff, ReadBuff.Length - NumberOfPreambulas);
        //byte[] ReadBuff_=CutOffPreambulasRecieved(ReadBuff);
        //NumberOfPreambulas=0;
        if (CheckCRC(ReadBuff_) == 1) 
        {
            if (ReadBuff_[0] == 0x06)//проверяем стартовый символ, в данном случае он шлется от подчиненного (0х02 - от главного, 0х06 - от подчиненного, 0х01 - пакетный режим)
            {
                if (ReadBuff_[1 ] >= 0x80)//проверяем корректность адреса подчиненного и обращение к ПК 
                {
                   
                    for (int a=0; a < 32;a++ )                    
                        if (CommandNumbersArray[a] == ReadBuff_[2]) commNumber = a;
                    
                    if (ReadBuff_[3] == CommandDataAnswerBytes[commNumber] + 2)//проверяем корректность принятого числа байт
                    {
                        RecievedCommand = ReadBuff_[2 ];
                        if (RecievedCommand == 0)//нулевая команда
                        {
                            ActualSlaveAddress = Convert.ToByte(Convert.ToInt16(ReadBuff_[1]) & 0x0f);
                            ManufacturerID = ReadBuff_[7 ];
                            DevTypeCode = ReadBuff_[8 ];
                            DeviceTypeID[0] = ReadBuff_[8];
                            NumberOfPreambulas = ReadBuff_[9 ];
                            UniversalComRev = ReadBuff_[10 ];
                            DevSpecComRev = ReadBuff_[11 ];
                            SoftwareRev = ReadBuff_[12 ];
                            SoftwareVer[0] = ReadBuff_[12];
                            HardwareRev = ReadBuff_[13 ];
                            DevFuncFlags = ReadBuff_[14 ];
                            DevIDNumber = (int)ReadBuff_[15 ] << 16 | (int)ReadBuff_[16 ] << 8 | (int)ReadBuff_[17 ];
                            DeviceSoftwareCRC[0] = ReadBuff_[16];
                            DeviceSoftwareCRC[1] = ReadBuff_[17];
                            //FF FF FF FF 06 82 00 0E 00 00 00 56 03 03 01 01 01 21 00 14 06 0B E5
                        }
                        if (RecievedCommand == 1)
                        {
                            PVunitsCode= ReadBuff_[6];
                            for (int i = 0; i < 4; i++)
                            {
                                
                                PV[i] = ReadBuff_[7 + i ];
                            }
                        }
                        if (RecievedCommand == 2)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                SensorCurrentValue[i] = ReadBuff_[6 + i ];
                                SensorDiapPrcnt[i] = ReadBuff_[10 + i ];
                            }
                        }
                        if (RecievedCommand == 16)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                SensorSerialNumber += (Int32)(ReadBuff_[6 + i]<<((2-i)*8));
                                
                            }
                        }
                        if (RecievedCommand == 48)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                calibrationK[i] = ReadBuff_[12 + i];
                                calibrationB[i] = ReadBuff_[16 + i];
                            }
                        }
                    }
                    else { }//ошибка формирования фрейма

                }
                else { }//address error
            }
            if (ReadBuff_[0] == 0x01)//пакетный режим
            {

            }
            else//ошибка адреса
            {

            }
        }
        
    }

    public static byte[] CutOffPreambulasRecieved(byte[] rec_mes)//обрезаем преамбулы, в данном случае считаем, что первые 2 байта преамбул можно отбросить ввиду того, что их минимальное количество - 3
    {
        int i = 0,preambsCounter=0;
        bool cuttingFinished = false;
        //Array.Reverse(rec_mes);//"переворачиваем" сообщение
        //Array.Resize(ref rec_mes, rec_mes.Length - 2);//обрезаем 2 первых байта в сообщении
        //Array.Reverse(rec_mes);//"переворачиваем" сообщение
        //while (rec_mes[i] == 0xFF)//теперь ждем, пока преамбулы закончатся, обрезая их на каждой итерации
        //while (rec_mes[i] != 0xFF)//теперь ждем, пока преамбулы закончатся, обрезая их на каждой итерации
        //{
        //    Array.Reverse(rec_mes);
        //    Array.Resize(ref rec_mes, rec_mes.Length - 1);
        //    Array.Reverse(rec_mes);
        //}
        //while (rec_mes[i] == 0xFF)//теперь ждем, пока преамбулы закончатся, обрезая их на каждой итерации
        //{
        //    Array.Reverse(rec_mes);
        //    Array.Resize(ref rec_mes, rec_mes.Length - 1);
        //    Array.Reverse(rec_mes);
        //}
        for (int a = 0; a < rec_mes.Length; a++)
        {
            //while (!cuttingFinished)
            //{
                if (rec_mes[a] == 0xff)
                {
                    preambsCounter++;
                    //if (preambsCounter >= 3)
                    //{
                    //    Array.Reverse(rec_mes);
                    //    Array.Resize(ref rec_mes, rec_mes.Length - preambsCounter);
                    //    Array.Reverse(rec_mes);
                    //    cuttingFinished = true;

                    //}
                }
                else
                {
                    if (preambsCounter >= 3)
                    {
                        Array.Reverse(rec_mes);
                        Array.Resize(ref rec_mes, rec_mes.Length - a);
                        Array.Reverse(rec_mes);
                   //     cuttingFinished = true;
                    }
                    preambsCounter = 0;
                }
           // }

        }
        //else   
        //Debug.WriteLine("after cutoff preambulas");
        //string tmp = " ";

        return rec_mes;//возвращаем сообщение без преамбул
        //int i=0;
        //Array.Reverse(rec_mes);//"переворачиваем" сообщение
        //Array.Resize(ref rec_mes, rec_mes.Length - 1);//обрезаем a первых байта в сообщении
        //Array.Reverse(rec_mes);//"переворачиваем" сообщение
        //for(int a =0; a < rec_mes.Length;a++)
        //{
        //    if (rec_mes[a] == 0xFF)
        //    {
        //       // i = a;
        //        Array.Reverse(rec_mes);//"переворачиваем" сообщение
        //        Array.Resize(ref rec_mes, rec_mes.Length - a);//обрезаем a первых байта в сообщении
        //        Array.Reverse(rec_mes);//"переворачиваем" сообщение
        //        StringBuilder builder = new StringBuilder(rec_mes.Length * 3);
        //        Debug.WriteLine("not preambulas count");
        //        Debug.WriteLine(a.ToString());
        //        //loop through each byte in the array
        //        foreach (byte data in rec_mes)
        //            //convert the byte to a string and add to the stringbuilder
        //            builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' '));
        //        a = rec_mes.Length - 1;
        //        Debug.WriteLine(builder);
                
                
        //    }

        //}
        //if(rec_mes.Length>0)
        //    if (rec_mes[i] == 0xff)
        //    {
        //        while (rec_mes[i] == 0xFF)//теперь ждем, пока преамбулы закончатся, обрезая их на каждой итерации
        //        {
        //            Array.Reverse(rec_mes);
        //            Array.Resize(ref rec_mes, rec_mes.Length - 1);
        //            Array.Reverse(rec_mes);
        //        }
        //    }
        //return rec_mes;//возвращаем сообщение без преамбул
    }

    public static byte[] ReadInputdataToSend(int commNumber)//здесь в зависимости от команды выбираем, что будем слать
    {
        byte[] dataToSend = new byte[CommandDataRequestBytes[commNumber]];//
        
        
            if (commNumber == 4) 
            {
                dataToSend[0] = (byte)SlaveAddress;//адрес для подчиненного устройства
            }
            
            if (commNumber== 5)
            {
                dataToSend = HartProtocol.ConvertASCIIToHartASCII(Tag);//отправляем тэг
            }

            if (commNumber==11)
            {
                dataToSend = HartProtocol.ConvertASCIIToHartASCII(Message);//отправляем сообщение
            }

            if (commNumber == 12)//формирование сообщение с тэгом, дескрипотором и датой
            {
                byte[] tmp_tag = HartProtocol.ConvertASCIIToHartASCII(Tag);
                byte[] tmp_desc = HartProtocol.ConvertASCIIToHartASCII(Descriptor);
                for (int i = 0; i < tmp_tag.Length;i++ )
                {
                    dataToSend[i] = tmp_tag[i];
                }
                for (int i = tmp_tag.Length; i < tmp_tag.Length+tmp_desc.Length; i++)
                {
                    dataToSend[i] = tmp_desc[i-tmp_tag.Length];
                }
                for (int i = tmp_tag.Length + tmp_desc.Length; i < tmp_tag.Length + tmp_desc.Length + Date.Length;i++ )
                {
                    dataToSend[i] = Date[i - tmp_tag.Length - tmp_desc.Length];
                }
            }

            if (commNumber == 14)
            {
                dataToSend[0] = (byte)HartProtocol.PVunitsCode;//единицы измерения первичной переменной
                for (int i = 1; i < 5; i++)
                {
                    dataToSend[i] = HartProtocol.UpperRangeLimit[i - 1];//верхний предел диапазона
                    dataToSend[i + 4] = HartProtocol.LowerRangeLimit[i - 1];//нижний предел диапазона
                }
            }
            if (commNumber == 23)//измеренное значение нуля тока
            {
                dataToSend= HartProtocol.MeasuredCurrentZero;
            }
            if (commNumber == 24)//измеренное значение приращения тока
            {
                dataToSend = HartProtocol.MeasuredCurrentGain;
            }

        return dataToSend;
    }

    

    public static byte[] ConvertHartASCIIToASCII(byte[] DataToConvert)//преобразуем HART ASCII в стандартный ASCII
    //HART ASCII характерен тем, что в нем символы упакованы по 3 на 4 байта, т.е. по 6 бит на символ
    //получается это засчет исключения первых 2х бит в стандартном ASCII
    {
        Int32 tmp;//32х разрядный формат я использовал потому, что приходится сдвигать на 23 разряда вправо
        byte[] tmp_b = new byte[DataToConvert.Length*4/3];//создаем массив соответствующей размерности
        byte[] tmptmp_b = new byte[DataToConvert.Length];
        for (int i = 0; i < DataToConvert.Length/3; i++)//определяем количество байт для конвертации
        {

            tmp = (Int32)DataToConvert[3 * i] << (16) | (Int32)DataToConvert[3 * i + 1] << (8) | (Int32)DataToConvert[3 * i + 2];//записываем 3 байта сообщения HART в одну переменную 32х битную
            tmp_b[4*i] = (byte)(((tmp >> 18) & 0x0000003f)|((tmp >> 23)^1)<<6);//подобную операцию мы проводим с каждыми 4мя байтами, по сути это преобразование к 8ми битному виду
            tmp_b[4*i + 1] = (byte)(((tmp >> 12) & 0x0000003f) | ((tmp >> 23) ^ 1) << 6);
            tmp_b[4*i + 2] = (byte)(((tmp >> 6) & 0x0000003f) | ((tmp >> 23) ^ 1) << 6);
            tmp_b[4*i + 3] = (byte)((tmp & 0x0000003f) | ((tmp >> 23) ^ 1) << 6);
            //for (int j = 0; j < 4; j++)
            //{
            //    tmptmp_b[3 - j] = tmp_b[j];
            tmp = 0;      
            //}
        }
        return tmp_b;
    }

    public static byte[] ConvertASCIIToHartASCII(byte[] DataToConvert)//обратная конвертация из ASCII в HART ASCII
    {
        Int32 tmp;
        byte[] tmp_b = new byte[4];
        if (DataToConvert.Length % 4 !=0)
        {
            for (int i = 1; i <= DataToConvert.Length % 4;i++ )
            {
                Array.Resize(ref DataToConvert, DataToConvert.Length + 1);
            }
        }
        byte[] tmptmp_b = new byte[DataToConvert.Length];
        for (int i = 0; i < DataToConvert.Length / 4; i++)
        {
            tmp = (Int32)(0x3f & DataToConvert[4 * i]) << (18) | (Int32)(0x3f & DataToConvert[4 * i + 1]) << (12) | (Int32)(0x3f & DataToConvert[4 * i + 2]) << (6) | (Int32)(0x3f & DataToConvert[4 * i + 3]);
            tmp_b = BitConverter.GetBytes(tmp);
            for (int j = 0; j < 3; j++)
            {
                tmptmp_b[3*i+2-j] = tmp_b[j];
                tmp_b[j] = 0;
            }
            Array.Resize(ref tmptmp_b, tmptmp_b.Length - 1);
            tmp = 0;
       
        }
        return tmptmp_b;
    }

    public static byte[] AppendCRC(byte[] msg)//добавляем CRC
    // вообще CRC в харте высчитывается как последовательное исключающее или всех байт и состоит из 1го байта
    {
        byte CRC=0x00;
        for (int i = HartProtocol.NumberOfPreambulas; i < msg.Length; ++i)//начинаем с преамбул
        {
            CRC ^= msg[i];
        }
        Array.Resize(ref msg, msg.Length + 1);
        msg[msg.Length - 1] = CRC;//исключаем последний байт CRC
        return msg;//возвращаем сообщение уже с CRC
    }

    public static int CheckCRC(byte[] msg)//проверка CRC в принятом сообщении, аналогично формированию CRC, только возвращаем лишь посчитанное CRC
    {
        byte CRC = 0;
        for (int i = 0; i < msg.Length - 1; ++i)
        {
            CRC ^= msg[i];
        }
        if (CRC == msg[msg.Length - 1]) CRC = 1;
        else CRC = 0;
        return CRC;
    }
    public static bool CheckMessageIntegrity(byte[] recMes)
    {
        recMes = CutOffPreambulasRecieved(recMes);
        if (recMes[3] + 4 > recMes.Length) return false;
        else return true;

        //recMes = CutOffPreambulasRecieved(recMes);
        //if (recMes.Length > 4)
        //{
        //    if (recMes[3] + 4 > recMes.Length) return false;
        //    else return true;
        //}
        //else return false;

    }
    public static byte[] CutOffGhostBytes(byte[] recMes)
    {
        int lastRealByteIndex = 5 + recMes[3];
        Array.Resize(ref recMes, lastRealByteIndex);
        return recMes;
    }
    public static int GetCommandDataLength(int commNumber)
    {
        int bytesInAnswer = 0;
        for (int i = 0; i < CommandNumbersArray.Length; i++)
            if (CommandNumbersArray[i] == commNumber)
            {
                bytesInAnswer = 7 + CommandDataAnswerBytes[i];
                return bytesInAnswer;
            }

        return bytesInAnswer;
    }
}
