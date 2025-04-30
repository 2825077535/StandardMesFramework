using JOJO.Mes.Log;
using JOJO.Mes.Model;
using System;
using System.Collections.Generic;
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

        public string MesUrlAcceptAddress { get; set; } = @"http:\\Accept";
        public int MesUrlTimeOut { get; set; } = 5000;
        public bool UseToken { get; set; } = false;
        public string Token { get; set; } = "";
        private string url { get; set; } = "";
        HttpClient Client;
        public Queue<string> GetHttpQueue = new Queue<string>();
        public Queue<string> ResponseHttpQueue = new Queue<string>();

        public bool CreatHttpClient()
        {
            Client = new HttpClient();
            Client.Timeout = TimeSpan.FromSeconds(MesUrlTimeOut);
            return true;
        }

        public async Task<string> MesUrlSendAndAccept(string obj)
        {
            try
            {
                string dataOut = "";
                url = MesUrlAddress + "/" + AccessInterface;
                MesLog.Info("当前访问的URL地址:" + url);

                // 创建 HTTP 客户端实例
                if (UseToken)
                {
                    Client.DefaultRequestHeaders.Add("token", Token);
                }
                // 构造要发送的内容
                var content = new StringContent(obj, Encoding.UTF8, "application/json");
                MesLog.Info("MesUrl数据上传：" + obj);
                // 发送POST请求
                var response = await Client.PostAsync(url, content);

                // 确保响应成功
                if (response.IsSuccessStatusCode)
                {
                    dataOut = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    MesLog.Info("Mes数据接受：" + dataOut);
                }
                content = null;
                response = null;

                return dataOut;
            }
            catch (Exception ex)
            {
                MesLog.Error("发送Http数据失败:" + ex.ToString());
                return null;
            }

        }

        public async void AcceptHttp()
        {
            while (true)
            {
                HttpListenerContext context = await Listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "POST")
                {
                    using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string requestContent = await reader.ReadToEndAsync();
                        GetHttpQueue.Enqueue(requestContent);

                        string responseString = "";

                        try
                        {
                            Task ResponseTask = Task.Run(async () =>
                            {
                                while (true)
                                {
                                    if (ResponseHttpQueue.Count > 0)
                                    {
                                        responseString = ResponseHttpQueue.Dequeue();
                                        break;
                                    }
                                    await Task.Delay(10);
                                }
                            }, new CancellationTokenSource(CtsTimeOut).Token);
                        }
                        catch (Exception ex)
                        {
                            MesLog.Error("Http接收响应失败：" + ex.ToString());
                            MesModel.SetMachineLog("Http接收响应失败");
                            return;
                        }


                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        using (Stream output = response.OutputStream)
                        {
                            await output.WriteAsync(buffer, 0, buffer.Length);
                        }
                    }
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    response.Close();
                }
            }
        }

        public void Close()
        {
            Client.Dispose();
        }
    }
}
