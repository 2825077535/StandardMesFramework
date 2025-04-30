using JOJO.Mes.CommModel;
using JOJO.Mes.Const;
using JOJO.Mes.Log;
using JOJO.Mes.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace JOJO.Mes.Client
{
    /// <summary>
    /// Mes1
    /// </summary>
    public class Mes1 : IMesSend
    {
        MesModel MesModel = new MesModel();
        private MesSocket socket = new MesSocket();
        private static readonly object _lockObject = new object();
        private Dictionary<string, Mes1Rec> _recDic = new Dictionary<string, Mes1Rec>();
        private Dictionary<string, Mes1Rec> RecDic
        {
            get => _recDic;
            set
            {
                lock (_lockObject)
                {
                    _recDic = value;
                }
            }
        }
        DateTime HeartTime = DateTime.Now;
        private bool IsAlarm = false;
        private string Heade { get; } = Header;
        public Mes1()
        {
            socket.Address = MesApp.Instance.JOJOMesConfig.MesAddress;
            socket.Point = MesApp.Instance.JOJOMesConfig.Mes1.Port;
            socket.CreatSocket();

            socket.ConnectChange += HandleConnectChangeChanged;
            Receive();
            HeartTimeAndIsConnect();
        }
        private void HandleConnectChangeChanged(object sender, SocketIsConnect e)
        {
            if (e.Connect)
            {
                MesModel.SetMesConnect();
            }
            else
            {
                MesModel.SetMesDisConnect();
            }
        }
        private object lockObj = new object();

        private void Receive()
        {
            Task MesContralMachine = Task.Run(async () =>
            {
                while (true)
                {
                    lock (lockObj)
                    {
                        if (socket.SocketQueue.Count > 0)
                        {
                            string message = socket.SocketQueue.Dequeue();
                            MesLog.Info($"接收Mes1数据: {message}");
                            MesProcessData ProcessData = MesXml.DeserializeXml(message);
                            string MesInterface = ProcessData.FindValueByPath(new string[] { Header, MESSAGENAME }, 0).ToString();
                            switch (MesInterface)
                            {
                                case EAP_LinkTest_Request:
                                    EAP_LinkTest_Request_Accept(ProcessData);
                                    break;
                                case DATE_TIME_CALIBREATION_COMMAND:
                                    ChangeTime(ProcessData);
                                    break;
                                case PROCESS_STOP_COMMAND:
                                    SetMachineProcessStop(ProcessData);
                                    break;
                                case "ALARM_REPORT_R":
                                case "EQP_STATUS_REPORT_R":
                                case "JOB_RECEIVE_REPORT_R":
                                case "JOB_SEND_REPORT_R":
                                case "EDC_REPORT_R":
                                case "JOB_REMOVE_RECOVERY_REPORT_R":
                                    CheckSend(ProcessData);
                                    break;
                                case "123"://预留Mes控制设备部分
                                    MesApp.Instance.MesQueueAccept.Enqueue(ProcessData); break;
                                default:
                                    break;
                            }
                        }
                    }
                    // 等待一段时间，避免忙等待
                    await Task.Delay(10);
                }
            });
        }
        public bool MesEnable()
        {
            return MesApp.Instance.JOJOMesConfig.IsEnableMes && socket.handler.Connected;
        }
        public async Task<MesProcessData> AlarmInformation(string message, int Level)
        {
            List<MesProcessData> datas = new List<MesProcessData>();
            datas.Add(new MesProcessData { MesName = EquipmentID, MesValue = MesApp.Instance.JOJOMesConfig.EquipmentID });

            datas.Add(new MesProcessData { MesName = "AlarmStatus", MesValue = 0 });
            datas.Add(new MesProcessData { MesName = "AlarmLevel", MesValue = 0 });

            datas.Add(new MesProcessData { MesName = "AlarmCode", MesValue = "00" });
            datas.Add(new MesProcessData { MesName = "AlarmText", MesValue = message });

            MesDynamic dynamic = new MesDynamic();
            dynamic.AddressInterface = "ALARM_REPORT";
            IsAlarm = true;

            List<MesProcessData> Returndatas = new List<MesProcessData>();
            Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
            return MesModel.ReturnData(Returndatas);

        }
        public Task<MesProcessData> ProcessParameters()
        {
            return Task.FromResult(new MesProcessData());
        }
        /// <summary>
        /// 过站检查
        /// </summary>
        /// <param name="BoardCode"></param>
        /// <returns></returns>
        public async Task<MesProcessData> CheckBoard(MesDynamic dynamic)
        {
            List<MesProcessData> datas = new List<MesProcessData>();
            if (dynamic.BoardCode == "" || dynamic.BoardCode == null)
            {
                dynamic.BoardCode = MesApp.Instance.Const.NowTimeF;
            }
            datas.Add(new MesProcessData { MesName = EquipmentID, MesValue = MesApp.Instance.JOJOMesConfig.EquipmentID });
            datas.Add(new MesProcessData { MesName = JobID, MesValue = dynamic.BoardCode });
            dynamic.AddressInterface = JOB_RECEIVE_REPORT;

            List<MesProcessData> Returndatas = new List<MesProcessData>();
            Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
            return MesModel.ReturnData(Returndatas);
        }
        /// <summary>
        /// 发送设备状态
        /// </summary>
        /// <param name="on"></param>
        public async Task<MesProcessData> ProcessStop(MesEnum.MachineState on)
        {
            int EquipmentStatus = -1;
            switch (on)
            {
                case MesEnum.MachineState.stop:
                case MesEnum.MachineState.YelloLight:
                    EquipmentStatus = 3;
                    break;
                case MesEnum.MachineState.start:
                case MesEnum.MachineState.GreenLight:
                    EquipmentStatus = 1;
                    break;
                case MesEnum.MachineState.RedLight:
                    EquipmentStatus = 2;
                    break;
                case MesEnum.MachineState.AwaitEnterBoard:
                    EquipmentStatus = 4;
                    break;
                case MesEnum.MachineState.AwaitOutBoard:
                    EquipmentStatus = 5;
                    break;
                default:
                    EquipmentStatus = 2;
                    break;
            }
            List<MesProcessData> datas = new List<MesProcessData>();
            datas.Add(new MesProcessData { MesName = EquipmentID, MesValue = MesApp.Instance.JOJOMesConfig.EquipmentID });
            List<MesProcessData> StationInfoList = new List<MesProcessData>();
            List<MesProcessData> StationInfo = new List<MesProcessData>();
            StationInfo.Add(new MesProcessData { MesName = Station, MesValue = MesApp.Instance.JOJOMesConfig.Mes1.Mes1StatueName });
            StationInfo.Add(new MesProcessData { MesName = "EquipmentStatus", MesValue = EquipmentStatus });
            StationInfoList.Add(new MesProcessData { MesName = "StationInfo", MesValue = StationInfo.ToArray() });

            datas.Add(new MesProcessData { MesName = "StationInfoList", MesValue = StationInfoList.ToArray() });
            MesDynamic dynamic = new MesDynamic();
            dynamic.AddressInterface = EQP_STATUS_REPORT;

            List<MesProcessData> Returndatas = new List<MesProcessData>();
            Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
            return MesModel.ReturnData(Returndatas);
        }

        public async Task<MesProcessData> RemovePCB(MesDynamic dynamic)
        {

            List<MesProcessData> datas = new List<MesProcessData>();
            datas.Add(new MesProcessData { MesName = EquipmentID, MesValue = MesApp.Instance.JOJOMesConfig.EquipmentID });
            datas.Add(new MesProcessData { MesName = JobID, MesValue = dynamic.BoardCode });
            datas.Add(new MesProcessData { MesName = "RemoveFlag", MesValue = 0 });
            dynamic.AddressInterface = JOB_REMOVE_RECOVERY_REPORT;

            List<MesProcessData> Returndatas = new List<MesProcessData>();
            Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
            return MesModel.ReturnData(Returndatas);

        }

        public async Task<MesProcessData> Result(MesDynamic data)
        {
            List<MesProcessData> datas = new List<MesProcessData>();
            datas.Add(new MesProcessData { MesName = EquipmentID, MesValue = MesApp.Instance.JOJOMesConfig.EquipmentID.ToString() });
            datas.Add(new MesProcessData { MesName = JobID, MesValue = data.BoardCode.ToString() });
            datas.Add(new MesProcessData { MesName = "ProcessTime", MesValue = data.TimeCost.ToString() });
            datas.Add(new MesProcessData { MesName = "ProcessStartTime", MesValue = data.ProcessStartTime.ToString() });
            datas.Add(new MesProcessData { MesName = "ProcessEndTime", MesValue = data.ProcessEndTime.ToString() });

            List<MesProcessData> MesProcessDataList = new List<MesProcessData>();

            List<MesProcessData> ProcessData1 = new List<MesProcessData>();
            ProcessData1.Add(new MesProcessData { MesName = Station, MesValue = MesApp.Instance.JOJOMesConfig.Mes1.Mes1StatueName.ToString() });
            ProcessData1.Add(new MesProcessData { MesName = Name, MesValue = "TotalResult" });
            ProcessData1.Add(new MesProcessData { MesName = value, MesValue = int.Parse(data.VerifiedBoardResult) == 0 ? "NG" : "OK" });

            List<MesProcessData> ProcessData2 = new List<MesProcessData>();
            ProcessData2.Add(new MesProcessData { MesName = Station, MesValue = MesApp.Instance.JOJOMesConfig.Mes1.Mes1StatueName.ToString() });
            ProcessData2.Add(new MesProcessData { MesName = Name, MesValue = "ProductCode" });
            ProcessData2.Add(new MesProcessData { MesName = value, MesValue = data.BoardCode.ToString() });

            List<MesProcessData> ProcessData3 = new List<MesProcessData>();
            ProcessData3.Add(new MesProcessData { MesName = Station, MesValue = MesApp.Instance.JOJOMesConfig.Mes1.Mes1StatueName.ToString() });
            ProcessData3.Add(new MesProcessData { MesName = Name, MesValue = "ProcessName" });
            ProcessData3.Add(new MesProcessData { MesName = value, MesValue = data.ProduceName.ToString() });

            List<MesProcessData> ProcessData4 = new List<MesProcessData>();
            ProcessData4.Add(new MesProcessData { MesName = Station, MesValue = MesApp.Instance.JOJOMesConfig.Mes1.Mes1StatueName.ToString() });
            ProcessData4.Add(new MesProcessData { MesName = Name, MesValue = "PartNum" });
            ProcessData4.Add(new MesProcessData { MesName = value, MesValue = data.TPNumber.ToString() });

            List<MesProcessData> ProcessData5 = new List<MesProcessData>();
            ProcessData5.Add(new MesProcessData { MesName = Station, MesValue = MesApp.Instance.JOJOMesConfig.Mes1.Mes1StatueName.ToString() });
            ProcessData5.Add(new MesProcessData { MesName = Name, MesValue = "NGNum" });
            ProcessData5.Add(new MesProcessData { MesName = value, MesValue = data.NGTPNumber.ToString() });

            JArray Detailes = new JArray();
            foreach (var item in data.TPNGs)
            {
                string codeName = string.Join(",", item.NGCodeName);
                JObject Detailedata = new JObject
                    {
                        {"Code", item.SubBoardCode},
                        {"Results", "NG"},
                        {"TEST_ITEM", item.TagNumber},
                        {"Result", codeName},
                        {"PartName", item.PartNumber}
                    };
                Detailes.Add(Detailedata);
            }

            List<MesProcessData> ProcessData6 = new List<MesProcessData>();
            ProcessData6.Add(new MesProcessData { MesName = Station, MesValue = MesApp.Instance.JOJOMesConfig.Mes1.Mes1StatueName });
            ProcessData6.Add(new MesProcessData { MesName = Name, MesValue = "Details" });
            ProcessData6.Add(new MesProcessData { MesName = value, MesValue = Detailes.ToString() });

            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData1.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData2.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData3.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData4.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData5.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData6.ToArray() });

            datas.Add(new MesProcessData { MesName = "ProcessDataList", MesValue = MesProcessDataList.ToArray() });

            MesDynamic dynamic = new MesDynamic
            {
                AddressInterface = "EDC_REPORT"
            };

            List<MesProcessData> Returndatas = new List<MesProcessData>();
            Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
            return MesModel.ReturnData(Returndatas);
        }

        public Task<MesProcessData> SwitchPrograms()
        {
            return Task.FromResult(new MesProcessData());
        }
        private static DateTime _lastCallTime = DateTime.MinValue;
        /// <summary>
        /// 发送Mes1数据
        /// </summary>
        /// <param name="datas">数据源</param>
        /// <param name="dynamic">接口</param>
        /// <returns></returns>
        public async Task<bool> MesSend(List<MesProcessData> datas, MesDynamic dynamic)
        {
            int number = GetUniqueFiveDigitNumber();

            List<MesProcessData> Message = new List<MesProcessData>();
            List<MesProcessData> Head = new List<MesProcessData>();
            Head.Add(new MesProcessData { MesName = MESSAGENAME, MesValue = dynamic.AddressInterface });
            Head.Add(new MesProcessData { MesName = "TRANSACTIONID", MesValue = MesApp.Instance.Const.NowTime.ToString("yyyyMMddHHmmssffff") + number });
            Head.Add(new MesProcessData { MesName = MESSAGEID, MesValue = number.ToString() });
            Head.Add(new MesProcessData { MesName = REPLYSUBJECTNAME, MesValue = MesApp.Instance.JOJOMesConfig.MesAddress + ":" + MesApp.Instance.JOJOMesConfig.Mes1.Port });
            Message.Add(new MesProcessData { MesName = Header, MesValue = Head.ToArray() });

            Message.Add(new MesProcessData { MesName = Body, MesValue = datas.ToArray() });

            List<MesProcessData> MessageReturn = new List<MesProcessData>();
            MessageReturn.Add(new MesProcessData { MesName = ReturnCode, MesValue = "" });
            MessageReturn.Add(new MesProcessData { MesName = ReturnMessage, MesValue = "" });
            Message.Add(new MesProcessData { MesName = MesReturn, MesValue = MessageReturn.ToArray() });


            MesProcessData Top = new MesProcessData();
            Top.MesName = "Message";
            Top.MesValue = Message.ToArray();

            if ((DateTime.Now - _lastCallTime).TotalMilliseconds >= 500)
            {
                _lastCallTime = DateTime.Now;
                MesSend_Accept(Top);
            }

            return await AwaitReceive(Top);
        }
        /// <summary>
        /// 发送数据后，将参数存储到字典中，等待超时和反馈信号
        /// </summary>
        /// <param name="mesData"></param>
        /// <returns></returns>
        private async Task<bool> AwaitReceive(MesProcessData Data)
        {
            bool Result = false;
            try
            {
                using CancellationTokenSource cts1 = new CancellationTokenSource();
                Mes1Rec Mes1Rec = new Mes1Rec();
                string Time = DateTime.Now.ToString();
                Mes1Rec.Cts = cts1;
                Mes1Rec.Data = Data;
                Mes1Rec.ReSend++;
                string MESSAGEID = Data.FindValueByPath(new string[] { Heade, MESSAGEID }).ToString();
                if (RecDic.ContainsKey(MESSAGEID))
                {
                    RecDic[MESSAGEID] = Mes1Rec;
                }
                else
                {
                    RecDic.Add(MESSAGEID, Mes1Rec);
                }
                await Task.Run(async () =>
                {
                    int timeoutCount = 0;
                    while (true)
                    {
                        try
                        {
                            Task.Delay(TimeSpan.FromMilliseconds(MesApp.Instance.JOJOMesConfig.MesTimeOut), cts1.Token).Wait(cts1.Token);
                        }
                        catch (OperationCanceledException ex)
                        {
                            if (ex.CancellationToken == cts1.Token)
                            {
                                if (RecDic[MESSAGEID].Result)
                                {
                                    RecDic.Remove(MESSAGEID);
                                    MesLog.Info(MESSAGEID + "return true");
                                    Result = true;
                                    return;
                                }
                                return;
                            }

                            timeoutCount++;
                            if (timeoutCount >= MesApp.Instance.JOJOMesConfig.Mes1.Mes1ReNumber)
                            {
                                RecDic.Remove(MESSAGEID);
                                MesLog.Error("Mes重新发送3次失败。接口为：" + Data.FindValueByPath(new string[] { Heade, MESSAGENAME }).ToString() + " 时间为：" + Time +
                                            "随机ID为：" + MESSAGEID);
                                MesModel.SetMachinAlarm("Mes重新发送3次失败");
                                break;
                            }
                            else
                            {
                                MesSend_Accept(Data);
                                continue;
                            }
                        }
                        await Task.Delay(5000 * 100);
                    }
                    Result = false;
                    return;
                }, cts1.Token);

            }
            catch (Exception ex)
            {
                MesLog.Error("等待返回信息失败:" + ex.ToString());
                return Result;
            }
            return Result;
        }

        private void CheckSend(MesProcessData data)
        {
            string MESSAGEID = data.FindValueByPath(new string[] { Header, MESSAGEID }).ToString();

            if (RecDic.ContainsKey(MESSAGEID))
            {
                if (data.FindValueByPath(new string[] { MesReturn, ReturnCode }).ToString().Contains("2"))
                {
                    MesLog.Error("Mes执行失败。 接口为：" + data.FindValueByPath(new string[] { Header, MESSAGENAME }).ToString());
                    if (RecDic[MESSAGEID].ReSend < 3)
                    {
                        RecDic[MESSAGEID].Result = false;
                        RecDic[MESSAGEID].Cts.Cancel();
                        MesSend_Accept(RecDic[MESSAGEID].Data);
                    }
                    else
                    {
                        RecDic[MESSAGEID].Result = false;
                        RecDic[MESSAGEID].Cts.Cancel();
                    }
                }
                else
                {
                    RecDic[MESSAGEID].Result = true;
                    RecDic[MESSAGEID].Cts.Cancel();
                }
            }
            else
            {
                MesLog.Error("Mes没有此发送数据");
            }
        }
        public void MesSend_Accept(MesProcessData Data)
        {
            string data = MesXml.SerializeToXml(Data);

            data = Regex.Replace(data, @"<\?.*?\?>", "");
            //整理XMl的缩进
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(data);

            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true; // 设置缩进为 true
            settings.IndentChars = "  "; // 设置缩进字符，这里使用两个空格

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                xmlDoc.Save(writer);
            }
            data = sb.ToString();

            if (!socket.handler.Connected)
            {
                MesModel.SetMachineLog("Mes1 没有连接");
                MesModel.SetMachinAlarm("Mes尚未连接，无法发送信息");
                return;
            }
            socket.SendObject(data);
        }


        /// <summary>
        /// 接收心跳,反馈接收的心跳
        /// </summary>
        public void EAP_LinkTest_Request_Accept(MesProcessData Data)
        {
            HeartTime = DateTime.Now;

            Data = Data.ModifyValueByPath(new string[] { Header, MESSAGENAME }, "EAP_LinkTest_Request_R");
            Data = Data.ModifyValueByPath(new string[] { Header, REPLYSUBJECTNAME }, MesApp.Instance.JOJOMesConfig.MesAddress + ":" + MesApp.Instance.JOJOMesConfig.Mes1.Port);
            Data = Data.ModifyValueByPath(new string[] { MesReturn, ReturnCode }, OK);
            Data = Data.ModifyValueByPath(new string[] { MesReturn, ReturnMessage }, "指令执行成功");

            MesSend_Accept(Data);
        }
        public bool AwaitHeartTime(TimeSpan time)
        {
            if (HeartTime + time < DateTime.Now)
            {
                return false;
            }
            return true;
        }
        public async Task<MesProcessData> OutBoard(MesDynamic dynamic)
        {
            List<MesProcessData> datas = new List<MesProcessData>();
            if (dynamic.BoardCode == "" || dynamic.BoardCode == null)
            {
                dynamic.BoardCode = MesApp.Instance.Const.NowTimeF;
            }
            datas.Add(new MesProcessData { MesName = EquipmentID, MesValue = MesApp.Instance.JOJOMesConfig.EquipmentID });
            datas.Add(new MesProcessData { MesName = JobID, MesValue = dynamic.BoardCode });
            dynamic.AddressInterface = JOB_SEND_REPORT;

            List<MesProcessData> Returndatas = new List<MesProcessData>();
            Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
            return MesModel.ReturnData(Returndatas);
        }
        /// <summary>
        /// Mes控制设备开启暂停
        /// </summary>
        /// <param name="data"></param>
        private async void SetMachineProcessStop(MesProcessData data)
        {
            bool Commond = false;
            data = data.ModifyValueByPath(new string[] { Header, MESSAGENAME }, PROCESS_STOP_COMMAND+_R);
            data = data.ModifyValueByPath(new string[] { Header, REPLYSUBJECTNAME }, MesApp.Instance.JOJOMesConfig.MesAddress + ":" + MesApp.Instance.JOJOMesConfig.Mes1.Port);
            try
            {
                string input = data.FindValueByPath(new string[] { Body, "ProcessStop" }).ToString();
                if (input != null)
                {
                    if (input.Contains("1"))
                    {
                        Commond = await MesModel.SetMesMachineStop();
                    }
                    else if (input.Contains("2"))
                    {
                        Commond = await MesModel.SetMesMachineRun();
                    }
                    MesProcessData processData = new MesProcessData();
                    processData.MesName = EquipmentID;
                    processData.MesValue = MesApp.Instance.JOJOMesConfig.EquipmentID;
                    List<MesProcessData> processDatas = new List<MesProcessData>();
                    processDatas.Add(processData);
                    data = data.ModifyValueByPath(new string[] { Body }, processDatas.ToArray());
                    if (Commond)
                    {
                        data = data.ModifyValueByPath(new string[] { MesReturn, ReturnCode }, OK);
                        data = data.ModifyValueByPath(new string[] { MesReturn, ReturnMessage }, "指令执行成功");
                    }
                    else
                    {
                        data = data.ModifyValueByPath(new string[] { MesReturn, ReturnCode }, OK);
                        data = data.ModifyValueByPath(new string[] { MesReturn, ReturnMessage }, "指令执行成功");

                    }
                }
                else
                {
                    data = data.ModifyValueByPath(new string[] { MesReturn, ReturnCode }, NG);
                    data = data.ModifyValueByPath(new string[] { MesReturn, ReturnMessage }, "指令执行失败");
                }
            }
            catch (Exception ex)
            {
                data = data.ModifyValueByPath(new string[] { MesReturn, ReturnCode }, NG);
                data = data.ModifyValueByPath(new string[] { MesReturn, ReturnMessage }, "指令执行失败");
            }
            MesSend_Accept(data);
        }

        /// <summary>
        /// 校准系统时间
        /// </summary>
        /// <param name="data"></param>
        private void ChangeTime(MesProcessData data)
        {
            data = data.ModifyValueByPath(new string[] { Header, MESSAGENAME }, DATE_TIME_CALIBREATION_COMMAND+_R);
            data = data.ModifyValueByPath(new string[] { Header, REPLYSUBJECTNAME }, MesApp.Instance.JOJOMesConfig.MesAddress + ":" + MesApp.Instance.JOJOMesConfig.Mes1.Port);

            try
            {
                string input = data.FindValueByPath(new string[] { Body, "DateTime" }).ToString();
                string format = "yyyyMMddHHmmss";

                DateTime result = DateTime.ParseExact(input, format, null);
                DateTime dt = result;
                bool r = UpdateTime.SetDate(dt);

                if (r)
                {
                    data = data.ModifyValueByPath(new string[] { MesReturn, ReturnCode }, OK);
                    data = data.ModifyValueByPath(new string[] { MesReturn, ReturnMessage }, "指令执行成功");
                }
                else
                {
                    data = data.ModifyValueByPath(new string[] { MesReturn, ReturnCode }, NG);
                    data = data.ModifyValueByPath(new string[] { MesReturn, ReturnMessage }, "指令执行失败");
                }
            }
            catch (Exception ex)
            {
                data = data.ModifyValueByPath(new string[] { MesReturn, ReturnCode }, NG);
                data = data.ModifyValueByPath(new string[] { MesReturn, ReturnMessage }, "指令执行失败");
                MesLog.Error("Mes矫正时间失败:" + ex.Message);
            }
            MesSend_Accept(data);
        }

        private Random _random = new Random();
        private HashSet<int> _uniqueNumbers = new HashSet<int>();
        /// <summary>
        /// 获取五位的随机数
        /// </summary>
        /// <returns></returns>
        int GetUniqueFiveDigitNumber()
        {
            int fiveDigitNumber;
            do
            {
                fiveDigitNumber = _random.Next(10000, 100000);
            } while (!_uniqueNumbers.Add(fiveDigitNumber));

            return fiveDigitNumber;
        }
        public async Task<MesProcessData> CancelAlarmInformation(string message)
        {
            List<MesProcessData> Returndatas = new List<MesProcessData>();
            if (!IsAlarm)
            {
                Returndatas.Add(MesModel.SetResultOKEnable(true));
                return MesModel.ReturnData(Returndatas);
            }
            IsAlarm = false;
            message += "，报警已消除";
            List<MesProcessData> datas = new List<MesProcessData>();
            datas.Add(new MesProcessData { MesName = EquipmentID, MesValue = MesApp.Instance.JOJOMesConfig.EquipmentID });

            datas.Add(new MesProcessData { MesName = "AlarmStatus", MesValue = 1 });
            datas.Add(new MesProcessData { MesName = "AlarmLevel", MesValue = 0 });

            datas.Add(new MesProcessData { MesName = "AlarmCode", MesValue = "10" });
            datas.Add(new MesProcessData { MesName = "AlarmText", MesValue = message });

            MesDynamic dynamic = new MesDynamic();
            dynamic.AddressInterface = "ALARM_REPORT";

            Returndatas = new List<MesProcessData>();
            Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
            return MesModel.ReturnData(Returndatas);

        }
        /// <summary>
        /// 判断心跳是否连接
        /// </summary>
        private void HeartTimeAndIsConnect()
        {

            Task task1 = Task.Run(() =>
            {
                while (true)
                {
                    if (!AwaitHeartTime(MesApp.Instance.JOJOMesConfig.Mes1.HeartTime))
                    {
                        MesLog.Error("Mes心跳异常");
                        MesModel.SetMachinAlarm("Mes心跳异常断开");
                        CloseMes();
                        break;
                    }
                }
            });
        }

        public void CloseMes()
        {
            socket.Close();
        }

        public async Task<MesProcessData> MesLogin(MesDynamic dynamic)
        {
            List<MesProcessData> datas = new List<MesProcessData>();
            datas.Add(new MesProcessData { MesName = EquipmentID, MesValue = MesApp.Instance.JOJOMesConfig.EquipmentID });

            datas.Add(new MesProcessData { MesName = "OperatorID", MesValue = dynamic.LoginName });
            datas.Add(new MesProcessData { MesName = "ModifiedTime", MesValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });

            dynamic.AddressInterface = "ALARM_REPORT";

            List<MesProcessData> Returndatas = new List<MesProcessData>();
            Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
            return MesModel.ReturnData(Returndatas);
        }

        public Task<MesProcessData> Dynamic(MesDynamic dynamic)
        {
            throw new NotImplementedException();
        }

        public class Mes1Rec
        {
            public CancellationTokenSource Cts { get; set; } = new CancellationTokenSource();
            public bool Result { get; set; } = false;
            public int ReSend { get; set; } = 0;
            public MesProcessData Data { get; set; } = new MesProcessData();
        }
    }
}
