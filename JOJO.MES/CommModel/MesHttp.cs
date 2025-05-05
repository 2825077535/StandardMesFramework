using JOJO.Mes.Log;
using JOJO.Mes.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JOJO.Mes.CommModel
{
    /// <summary>
    /// 仅使用Post和Get请求
    /// </summary>
    internal class MesHttp
    {
        MesModel MesModel=new MesModel();
        public string MesUrlAddress { get; set; } = @"http:\\Send";
        public string AccessInterface { get; set; } = "";
        public int MesUrlTimeOut { get; set; } = 5000;
        public bool UseToken { get; set; } = false;
        public string Token { get; set; } = "";
        private string url { get; set; } = "";
        HttpClient Client;
        public Queue<string> ResponseHttpQueue = new Queue<string>();

        public bool CreatHttpClient()
        {
            try
            {
                Client = new HttpClient();
                Client.Timeout = TimeSpan.FromMilliseconds(MesUrlTimeOut);
            }
            catch (Exception ex)
            {
                MesLog.Error("创建HTTP服务异常:"+ex.ToString());
            }
            return true;
        }
        /// <summary>
        /// 发送等待返回的数据
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="IsPost">1为Post请求，0为Get请求</param>
        /// <returns></returns>
        public async Task<string> MesUrlSendAndAccept(string obj,int IsPost=1)
        {
            try
            {
                string dataOut = "";
                StringContent content;
                switch (IsPost)
                {
                    case 1:
                        url = MesUrlAddress + "/" + AccessInterface;
                        MesLog.Info("当前访问的URL地址:" + url);
                        if (UseToken)
                        {
                            Client.DefaultRequestHeaders.Add("token", Token);
                        }
                        // 构造要发送的内容
                        content = new StringContent(obj, Encoding.UTF8);
                        MesLog.Info("MesUrl数据上传：" + obj);
                        // 发送POST请求
                        HttpResponseMessage response = await Client.PostAsync(url, content);
                        // 确保响应成功
                        if (response.IsSuccessStatusCode)
                        {
                            dataOut = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            MesLog.Info("Mes数据接受：" + dataOut);
                        }
                        content = null;
                        response = null;
                        break;

                    case 0:
                        url = MesUrlAddress + "/" + AccessInterface;
                        MesLog.Info("当前访问的URL地址:" + url);
                        if (UseToken)
                        {
                            Client.DefaultRequestHeaders.Add("token", Token);
                        }
                        // 发送Get请求
                        response = await Client.GetAsync(url);
                        // 确保响应成功
                        if (response.IsSuccessStatusCode)
                        {
                            dataOut = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            MesLog.Info("Mes数据接受：" + dataOut);
                        }
                        content = null;
                        response = null;
                        break;
                    default:
                        break;
                }
                return dataOut;
            }
            catch (TaskCanceledException ex)
            {
                MesLog.Error("发送Http相应超时:" + ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                MesLog.Error("发送Http数据失败:" + ex.ToString());
                return null;
            }

        }
        public void Close()
        {
            Client.Dispose();
        }
    }
}
