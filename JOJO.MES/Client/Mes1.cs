using JOJO.Mes.CommModel;
using JOJO.Mes.Const;
using JOJO.Mes.Log;
using JOJO.Mes.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
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
        #region 常量
        const string Header = nameof(Header);
        const string Body = nameof(Body);
        const string MESSAGENAME=nameof(MESSAGENAME);
        const string EAP_LinkTest_Request=nameof(EAP_LinkTest_Request);
        const string DATE_TIME_CALIBREATION_COMMAND=nameof(DATE_TIME_CALIBREATION_COMMAND);
        const string PROCESS_STOP_COMMAND=nameof(PROCESS_STOP_COMMAND);
        const string NG_EJECT=nameof(NG_EJECT);
        const string RECIPE_PARAMETER_DOWNLOAD_COMMAND=nameof(RECIPE_PARAMETER_DOWNLOAD_COMMAND);
        const string ALARM_REPORT = nameof(ALARM_REPORT);
        const string EQP_STATUS_REPORT=nameof(EQP_STATUS_REPORT);
        const string JOB_RECEIVE_REPORT=nameof(JOB_RECEIVE_REPORT);
        const string JOB_SEND_REPORT=nameof(JOB_SEND_REPORT);
        const string EDC_REPORT=nameof(EDC_REPORT);
        const string JOB_REMOVE_RECOVERY_REPORT=nameof(JOB_REMOVE_RECOVERY_REPORT);
        const string OPERATOR_LOGIN_LOGOUT_REPORT=nameof(OPERATOR_LOGIN_LOGOUT_REPORT);
        const string _R=nameof(_R);
        const string EquipmentID=nameof(EquipmentID);
        const string JobID=nameof(JobID);
        const string REPLYSUBJECTNAME = nameof(REPLYSUBJECTNAME);
        const string MesReturn="Return";
        const string ReturnCode=nameof(ReturnCode);
        const string ReturnMessage=nameof(ReturnMessage);
        const string Station=nameof(Station);
        const string MESSAGEID=nameof(MESSAGEID);
        const string Name=nameof(Name);
        const string value = nameof(value);
        const string ProcessData=nameof(ProcessData);
        const string OK = "01";
        const string NG = "02";
        #endregion

        MesModel MesModel = new MesModel();
        private MesSocket socket = new MesSocket();
        private static readonly object _lockObject = new object();
        private Dictionary<string, Mes1Rec> _recDic = new Dictionary<string, Mes1Rec>();
        private bool NowBoardIsCheck { get; set; } = false;
        private Queue<string> SendStrQueue = new Queue<string>();
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
        public Mes1()
        {
            socket.Address = MesApp.Instance.JOJOMesConfig.MesAddress;
            socket.Point = MesApp.Instance.JOJOMesConfig.Port;
            socket.CreatSocket();
            socket.ConnectChange -= HandleConnectChangeChanged;
            socket.ConnectChange += HandleConnectChangeChanged;
            Receive();
            HeartTimeAndIsConnect();
            AwaitSend();   
        }
        private void HandleConnectChangeChanged(object sender, SocketIsConnect e)
        {
            if (e.Connect)
            {
                MesModel.SetMachineLog("Mes接入成功");
                MesModel.SetMesConnect();
            }
            else
            {
                MesModel.SetMachineLog("Mes接入断开");
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
                            MesProcessData ProcessData = MesXml.DeserializeXml(message);
                            string MesInterface = ProcessData.FindValueByPath(new string[] { Header, MESSAGENAME }, 0).ToString();
                            if (!MesInterface.Contains(EAP_LinkTest_Request))
                            {
                                MesLog.Info(message);
                            }
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
                                case NG_EJECT:
                                    CheckBoardAccept(ProcessData);
                                    break;
                                case RECIPE_PARAMETER_DOWNLOAD_COMMAND:
                                    CheckBoardAcceptSubCode(ProcessData);
                                    break;
                                case ALARM_REPORT+_R:
                                case EQP_STATUS_REPORT+_R:
                                case JOB_RECEIVE_REPORT+_R:
                                case JOB_SEND_REPORT+_R:
                                case EDC_REPORT+_R:
                                case JOB_REMOVE_RECOVERY_REPORT+_R:
                                case OPERATOR_LOGIN_LOGOUT_REPORT+_R:
                                    CheckSend(ProcessData);
                                    break;
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
            bool OK = true;
            OK &= MesApp.Instance.JOJOMesConfig.IsEnableMes;
            try
            {
                OK &= socket.handler == null ? false : socket.handler.Connected;
            }
            catch (Exception)
            {
                OK = false;
            }
            return OK;
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
            dynamic.AddressInterface = ALARM_REPORT;
            IsAlarm = true;

            List<MesProcessData> Returndatas = new List<MesProcessData>();
            Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
            return MesModel.ReturnData(Returndatas);

        }
        public async Task<MesProcessData> ProcessParameters()
        {
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
        }
        private static SemaphoreSlim semaphore = new SemaphoreSlim(0);
        /// <summary>
        /// 过站检查
        /// </summary>
        /// <param name="BoardCode"></param>
        /// <returns></returns>
        public async Task<MesProcessData> CheckBoard(MesDynamic dynamic)
        {
            //清空信号量
            while (semaphore.CurrentCount > 0)
            {
                semaphore.Wait();
            }
            bool IsOK = true;
            foreach (string item in dynamic.Codes)
            {
                List<MesProcessData> datas = new List<MesProcessData>();
                datas.Add(new MesProcessData { MesName = EquipmentID, MesValue = MesApp.Instance.JOJOMesConfig.EquipmentID });
                datas.Add(new MesProcessData { MesName = JobID, MesValue = item });
                dynamic.AddressInterface = JOB_RECEIVE_REPORT;
                IsOK &= await MesSend(datas, dynamic);
            }

            if (await semaphore.WaitAsync(MesApp.Instance.JOJOMesConfig.MesTimeOut*MesApp.Instance.JOJOMesConfig.Mes1.ReNumber+100)&&IsOK)
            {
                List<MesProcessData> Returndatas = new List<MesProcessData>();
                Returndatas.Add(MesModel.SetResultOKEnable(NowBoardIsCheck));
                NowBoardIsCheck = false;
                return MesModel.ReturnData(Returndatas);
            }
            else
            {
                List<MesProcessData> Returndatas = new List<MesProcessData>();
                Returndatas.Add(MesModel.SetResultOKEnable(false));
                NowBoardIsCheck = false;
                return MesModel.ReturnData(Returndatas);
            }
        }
        /// <summary>
        /// 在过站检查中，发送条码信息后，需要等到条码校验的结果。是分开成2个命令
        /// </summary>
        /// <param name="data"></param>
        private void CheckBoardAccept(MesProcessData data)
        {
            bool Commond = false;
            data = data.ModifyValueByPath(new string[] { Header, MESSAGENAME }, NG_EJECT+_R);
            data = data.ModifyValueByPath(new string[] { Header, REPLYSUBJECTNAME }, MesApp.Instance.JOJOMesConfig.MesAddress + ":" + MesApp.Instance.JOJOMesConfig.Port);
            try
            {
                string input = data.FindValueByPath(new string[] { Body, "PCBQuality" }).ToString();
                if (input != null)
                {
                    if (input.Contains("1"))
                    {
                        NowBoardIsCheck = true;
                    }
                    else if (input.Contains("2"))
                    {
                        NowBoardIsCheck = false;
                    }
                    else
                    {
                        MesModel.SetMachinAlarm("Mes返回当前条码不在数据库中，需要人工干预");
                    }
                    Commond = true;
                    //接收到条码检验结果指令，通知条码校验可以继续执行
                    semaphore.Release();
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
                MesSend_Accept(data);
            }
            catch (Exception ex)
            {
                data = data.ModifyValueByPath(new string[] { MesReturn, ReturnCode }, NG);
                data = data.ModifyValueByPath(new string[] { MesReturn, ReturnMessage }, "指令执行失败");
                MesSend_Accept(data);
            }
        }
        private void CheckBoardAcceptSubCode(MesProcessData data)
        {

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
            StationInfo.Add(new MesProcessData { MesName = Station, MesValue = MesApp.Instance.JOJOMesConfig.Mes1.StatueName });
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
            ProcessData1.Add(new MesProcessData { MesName = Name, MesValue = Station });
            ProcessData1.Add(new MesProcessData { MesName = value, MesValue = MesApp.Instance.JOJOMesConfig.Mes1.StatueName.ToString() });

            List<MesProcessData> ProcessData2 = new List<MesProcessData>();
            ProcessData2.Add(new MesProcessData { MesName = Name, MesValue = "QWER" });
            ProcessData2.Add(new MesProcessData { MesName = value, MesValue = data.VerifiedBoardResult == "1" ? "PASS" : "FAIL" });

            List<MesProcessData> ProcessData3 = new List<MesProcessData>();
            ProcessData3.Add(new MesProcessData { MesName = Name, MesValue = "Operator" });
            ProcessData3.Add(new MesProcessData { MesName = value, MesValue = data.LoginName.ToString() });
            List<MesProcessData> ProcessData4 = new List<MesProcessData>();
            ProcessData4.Add(new MesProcessData { MesName = Name, MesValue = "ModelName" });
            ProcessData4.Add(new MesProcessData { MesName = value, MesValue = data.ProduceName.ToString() });

            List<MesProcessData> ProcessData5 = new List<MesProcessData>();
            ProcessData5.Add(new MesProcessData { MesName = Name, MesValue = "QTY" });
            ProcessData5.Add(new MesProcessData { MesName = value, MesValue = data.TPNumber.ToString() });

            List<MesProcessData> ProcessData6 = new List<MesProcessData>();
            ProcessData6.Add(new MesProcessData { MesName = Name, MesValue = "NGNum" });
            ProcessData6.Add(new MesProcessData { MesName = value, MesValue = data.NGTPNumber.ToString() });

            List<MesProcessData> ProcessData7 = new List<MesProcessData>();
            ProcessData7.Add(new MesProcessData { MesName = Name, MesValue = "BoardSide" });
            ProcessData7.Add(new MesProcessData { MesName = value, MesValue = data.BottomTop == MesConst.Top ? "T" : "B" });
            JArray Detailes = new JArray();
            foreach (var item in data.MachineSubBoardResult)
            {
                if (item.Value == "1")
                {
                    JObject Detailedata = new JObject
                    {
                        {"Code", Regex.Replace(item.Key, @"[^\d]", "")},
                        {"Results", "OK"},
                    };
                    Detailes.Add(Detailedata);
                }
            }
            foreach (var item in data.VerifiedSubBoardResult)
            {
                if (item.Value == "1")
                {
                    JObject Detailedata = new JObject
                    {
                        {"Code", Regex.Replace(item.Key, @"[^\d]", "")},
                        {"Results", "OK"},
                    };
                    Detailes.Add(Detailedata);
                }
            }
            foreach (var item in data.TPNGs)
            {
                string codeName = string.Join(",", item.NGCodeName);
                JObject Detailedata = new JObject
                    {
                        {"Code", Regex.Replace(item.SubBoardId, @"[^\d]", "")},
                        {"Results", "NG"},
                        {"TEST_ITEM", "N/A"},
                        {"Result", codeName},
                        {"PartName", item.PartNumber}
                    };
                Detailes.Add(Detailedata);
            }


            List<MesProcessData> ProcessData8 = new List<MesProcessData>();
            ProcessData8.Add(new MesProcessData { MesName = Name, MesValue = "Details" });
            ProcessData8.Add(new MesProcessData { MesName = value, MesValue = Detailes.ToString() });

            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData1.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData2.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData3.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData4.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData5.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData6.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData7.ToArray() });
            MesProcessDataList.Add(new MesProcessData { MesName = ProcessData, MesValue = ProcessData8.ToArray() });

            datas.Add(new MesProcessData { MesName = "ProcessDataList", MesValue = MesProcessDataList.ToArray() });

            MesDynamic dynamic = new MesDynamic
            {
                AddressInterface = EDC_REPORT
            };

            List<MesProcessData> Returndatas = new List<MesProcessData>();
            Returndatas.Add(MesModel.SetResultOKEnable(await MesSend(datas, dynamic)));
            return MesModel.ReturnData(Returndatas);
        }

        public async Task<MesProcessData> SwitchPrograms()
        {
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
        }
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
            Head.Add(new MesProcessData { MesName = REPLYSUBJECTNAME, MesValue = MesApp.Instance.JOJOMesConfig.MesAddress + ":" + MesApp.Instance.JOJOMesConfig.Port });
            Message.Add(new MesProcessData { MesName = Header, MesValue = Head.ToArray() });

            Message.Add(new MesProcessData { MesName = Body, MesValue = datas.ToArray() });

            List<MesProcessData> MessageReturn = new List<MesProcessData>();
            MessageReturn.Add(new MesProcessData { MesName = ReturnCode, MesValue = "" });
            MessageReturn.Add(new MesProcessData { MesName = ReturnMessage, MesValue = "" });
            Message.Add(new MesProcessData { MesName = MesReturn, MesValue = MessageReturn.ToArray() });


            MesProcessData Top = new MesProcessData();
            Top.MesName = "Message";
            Top.MesValue = Message.ToArray();
            MesSend_Accept(Top);
            return await AwaitReceive(Top);
        }

        private void AwaitSend()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (SendStrQueue.Count > 0)
                    {
                        socket.SendObject(SendStrQueue.Dequeue());
                        await Task.Delay(100);//每个发送间隔100ms
                    }
                    else
                        await Task.Delay(100);
                }
            });
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
                string ReceiveMESSAGEID = Data.FindValueByPath(new string[] { Header, MESSAGEID }).ToString();
                if (RecDic.ContainsKey(ReceiveMESSAGEID))
                {
                    RecDic[ReceiveMESSAGEID] = Mes1Rec;
                }
                else
                {
                    RecDic.Add(ReceiveMESSAGEID, Mes1Rec);
                }
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(MesApp.Instance.JOJOMesConfig.MesTimeOut), cts1.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            if (RecDic[ReceiveMESSAGEID].Result)
                            {
                                RecDic.Remove(ReceiveMESSAGEID);
                                MesLog.Info(ReceiveMESSAGEID + "return true");
                                Result = true;
                                return;
                            }
                            else
                            {
                                RecDic.Clear();
                                MesLog.Error("Mes返回信息3次失败。接口为：" + Data.FindValueByPath(new string[] { Header, MESSAGENAME }).ToString() + " 时间为：" + Time +
                                            "随机ID为：" + MESSAGEID);
                                MesModel.SetMachinAlarm("Mes返回信息3次失败");
                            }
                            return;
                        }
                        if (RecDic[MESSAGEID].ReSend++ >= MesApp.Instance.JOJOMesConfig.Mes1.ReNumber)
                        {
                            RecDic.Clear();
                            MesLog.Error("Mes重新发送3次失败。接口为：" + Data.FindValueByPath(new string[] { Header, MESSAGENAME }).ToString() + " 时间为：" + Time +
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
                    Result = false;
                    return;
                });

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
            string ReceiveMESSAGEID = data.FindValueByPath(new string[] { Header, MESSAGEID }).ToString();

            if (RecDic.ContainsKey(ReceiveMESSAGEID))
            {
                if (data.FindValueByPath(new string[] { MesReturn, ReturnCode }).ToString().Contains("2"))
                {
                    MesLog.Error("Mes执行失败。 接口为：" + data.FindValueByPath(new string[] { Header, MESSAGENAME }).ToString());
                    if (RecDic[ReceiveMESSAGEID].ReSend < 3)
                    {
                        RecDic[ReceiveMESSAGEID].ReSend++;
                        MesSend_Accept(RecDic[ReceiveMESSAGEID].Data);
                    }
                    else
                    {
                        RecDic[ReceiveMESSAGEID].Result = false;
                        RecDic[ReceiveMESSAGEID].Cts.Cancel();
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
            string MesInterface = Data.FindValueByPath(new string[] { Header, MESSAGENAME }, 0).ToString();
            if (!MesInterface.Contains(EAP_LinkTest_Request))
            {
                MesLog.Info("发送Mes数据:" + data);

            }
            SendStrQueue.Enqueue(data);
        }


        /// <summary>
        /// 接收心跳,反馈接收的心跳
        /// </summary>
        public void EAP_LinkTest_Request_Accept(MesProcessData Data)
        {
            HeartTime = DateTime.Now;

            Data = Data.ModifyValueByPath(new string[] { Header, MESSAGENAME }, EAP_LinkTest_Request+_R);
            Data = Data.ModifyValueByPath(new string[] { Body, EquipmentID }, MesApp.Instance.JOJOMesConfig.EquipmentID);
            Data = Data.ModifyValueByPath(new string[] { Header, REPLYSUBJECTNAME }, MesApp.Instance.JOJOMesConfig.MesAddress + ":" + MesApp.Instance.JOJOMesConfig.Port);
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
            data = data.ModifyValueByPath(new string[] { Header, REPLYSUBJECTNAME }, MesApp.Instance.JOJOMesConfig.MesAddress + ":" + MesApp.Instance.JOJOMesConfig.Port);
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
                        data = data.ModifyValueByPath(new string[] { MesReturn, ReturnCode }, NG);
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
                MesLog.Error("Mes控制设备开启暂停执行报错：" + ex.ToString());
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
            data = data.ModifyValueByPath(new string[] { Header, REPLYSUBJECTNAME }, MesApp.Instance.JOJOMesConfig.MesAddress + ":" + MesApp.Instance.JOJOMesConfig.Port);

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
            if (_uniqueNumbers.Count >= 9000)
            {
                _uniqueNumbers.Clear();
            }
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
            dynamic.AddressInterface = ALARM_REPORT;

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
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
        }

        public async Task<MesProcessData> Dynamic(MesDynamic dynamic)
        {
            List<MesProcessData> Returndata = new List<MesProcessData>();
            Returndata.Add(MesModel.SetResultOKEnable(false, false));
            return MesModel.ReturnData(Returndata);
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
