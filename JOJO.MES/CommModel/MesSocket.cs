using JOJO.Mes.Log;
using JOJO.Mes.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace JOJO.Mes.CommModel
{
    internal class MesSocket
    {
        private readonly byte[] buffer = new byte[1024 * 1024 * 100];
        MesModel MesModel = new MesModel();
        public Socket listener;
        public Socket handler;
        public int Point = 8888;
        public string Address = "192.168.8.88";
        public Queue<string> SocketQueue = new Queue<string>();

        public delegate void PropertyChangedEventHandler(object sender, SocketIsConnect e);
        public event PropertyChangedEventHandler ConnectChange;
        public bool CreatSocket()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(Address), Point);
                listener.Bind(localEndPoint);
                listener.Listen(10);
            }
            catch (FormatException)
            {
                return false;
            }
            MesLog.Info("Socket,等待客户端连接...");
            // 开始异步接受客户端连接
            listener.BeginAccept(AcceptCallback, listener);
            return true;
        }
        protected virtual void OnPropertyChanged(bool Connect)
        {
            ConnectChange?.Invoke(this, new SocketIsConnect(Connect));
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                if (listener == null)
                {
                    return;
                }

                Socket listener1 = (Socket)ar.AsyncState;
                // 完成接受客户端连接
                handler = listener1.EndAccept(ar);
                MesLog.Info($"连接Socket成功:" + handler.AddressFamily);
                OnPropertyChanged(true);
                // 开始异步接收数据
                handler.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, handler);
                //// 继续监听新的连接
                listener1.BeginAccept(AcceptCallback, listener1);
            }
            catch (Exception ex)
            {
                MesLog.Error(ex.ToString());
            }

        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (listener == null)
            {
                return;
            }
            Socket handler = (Socket)ar.AsyncState;
            if (handler == null)
            {
                return;
            }
            try
            {
                int bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0)
                {
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);
                    string message = System.Text.Encoding.UTF8.GetString(data);
                    SocketQueue.Enqueue(message);
                    handler.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, handler);
                }
            }
            catch (SocketException e)
            {
                MesLog.Warn($"接收Socket数据出错: {e.Message}");
            }
            finally
            {
                if (handler.Connected)
                {
                    handler.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, handler);

                }
            }
        }
        public bool SendObject(string SendString)
        {
            try
            {
                if (handler != null && !handler.Connected)
                {
                    MesModel.SetMachineLog("Mes所在的Socket端口未连接，无法发送数据");
                    return false;
                }
                byte[] SendBytes = Encoding.UTF8.GetBytes(SendString);
                Task.Run(() =>
                {
                    // 通过Socket发送数据
                    handler.Send(SendBytes, 0, SendBytes.Length, SocketFlags.None);
                });
            }
            catch (Exception ex)
            {
                MesLog.Error("发送不带反馈的Socket数据失败：" + ex.ToString());
                return false;
            }
            return true;
        }
        public void Close()
        {
            try
            {
                OnPropertyChanged(false);
                listener?.Close();
                listener = null;
                handler?.Shutdown(SocketShutdown.Both);
                handler?.Close();
                handler = null;
            }
            catch (Exception)
            {
            }
        }
    }
    public class SocketIsConnect : EventArgs
    {
        public bool Connect { get; }

        public SocketIsConnect(bool Connect)
        {
            this.Connect = Connect;
        }
    }

}
