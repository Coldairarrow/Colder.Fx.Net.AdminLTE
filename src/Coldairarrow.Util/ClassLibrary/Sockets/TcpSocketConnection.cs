using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Coldairarrow.Util.Sockets
{
    /// <summary>
    /// Socket连接,双向通信
    /// </summary>
    public class TcpSocketConnection
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket">维护的Socket对象</param>
        /// <param name="server">维护此连接的服务对象</param>
        /// <param name="recLength">接受缓冲区大小</param>
        public TcpSocketConnection(Socket socket, TcpSocketServer server, int recLength)
        {
            _socket = socket;
            _server = server;
            _recLength = recLength;
            _sendCheckMsg = server.GetPropertyValue("_sendCheckMsg") as SendCheckMsg;
        }

        #endregion

        #region 私有成员

        private Socket _socket { get; }
        private bool _isRec { get; set; } = true;
        private TcpSocketServer _server { get; set; } = null;
        private bool _isClosed { get; set; } = false;
        private string _connectionId { get; set; } = Guid.NewGuid().ToString();
        private SendCheckMsg _sendCheckMsg { get; } = new SendCheckMsg();

        /// <summary>
        /// 接收区大小,单位:字节
        /// </summary>
        private int _recLength { get; set; }
        private void AccessException(Exception ex)
        {
            if (!(ex is ObjectDisposedException))
            {
                try
                {
                    HandleException?.Invoke(ex);
                }
                catch
                {

                }
            }
        }

        #endregion

        #region 外部接口

        /// <summary>
        /// 开始接受客户端消息
        /// </summary>
        public void StartRecMsg()
        {
            try
            {
                byte[] container = new byte[_recLength];
                _socket.BeginReceive(container, 0, container.Length, SocketFlags.None, asyncResult =>
                {
                    try
                    {
                        int length = _socket.EndReceive(asyncResult);

                        //马上进行下一轮接受，增加吞吐量
                        if (length > 0 && _isRec && IsSocketConnected() && (!_isClosed))
                            StartRecMsg();

                        if (length > 0)
                        {
                            byte[] recBytes = new byte[length];
                            Array.Copy(container, 0, recBytes, 0, length);
                            HandleRecMsg?.Invoke(_server, this, recBytes);
                        }
                        else
                            Close();
                    }
                    catch (Exception ex)
                    {
                        AccessException(ex);
                        Close();
                    }
                }, null);
            }
            catch (Exception ex)
            {
                AccessException(ex);
                Close();
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes">数据字节</param>
        public void Send(byte[] bytes)
        {
            try
            {
                _socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, asyncResult =>
                {
                    try
                    {
                        int length = _socket.EndSend(asyncResult);
                        HandleSendMsg?.Invoke(_server, this, bytes);
                    }
                    catch (Exception ex)
                    {
                        AccessException(ex);
                    }
                }, null);
            }
            catch (Exception ex)
            {
                AccessException(ex);
            }
        }

        /// <summary>
        /// 发送字符串（默认使用UTF-8编码）
        /// </summary>
        /// <param name="msgStr">字符串</param>
        public void Send(string msgStr)
        {
            Send(Encoding.UTF8.GetBytes(msgStr));
        }

        /// <summary>
        /// 发送字符串（使用自定义编码）
        /// </summary>
        /// <param name="msgStr">字符串消息</param>
        /// <param name="encoding">使用的编码</param>
        public void Send(string msgStr, Encoding encoding)
        {
            Send(encoding.GetBytes(msgStr));
        }

        /// <summary>
        /// 发送数据并校验返回结果
        /// </summary>
        /// <param name="bytes">发送的数据</param>
        /// <param name="check">确认具体确认方法</param>
        /// <param name="timeOut">发送数据到收到确认消息的时间间隔,单位为毫秒</param>
        /// <returns></returns>
        public Task<bool> SendWithCheck(byte[] bytes, Func<string, byte[], bool> check, int timeOut)
        {
            Send(bytes);
            var waitHandle = _sendCheckMsg.SendMsg(ConnectionId, bytes, check, timeOut);
            return Task.Run(() =>
            {
                var res = Task.Run(() => { waitHandle.WaitOne(); }).Wait(timeOut);
                return res;
            });
        }

        /// <summary>
        /// 连接标识Id
        /// 注:用于标识与客户端的连接
        /// </summary>
        public string ConnectionId
        {
            get
            {
                return _connectionId;
            }
            set
            {
                string oldConnectionId = _connectionId;
                _connectionId = value;
                _server?.SetConnectionId(this, oldConnectionId, value);
            }
        }

        ///// <summary>
        ///// 关闭当前连接
        ///// </summary>
        //public void Close()
        //{
        //    if (_isClosed)
        //        return;
        //    try
        //    {
        //        _isClosed = true;
        //        _server.RemoveConnection(this);

        //        _isRec = false;
        //        if (IsSocketConnected())
        //        {
        //            _socket.Disconnect(false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AccessException(ex);
        //    }
        //    finally
        //    {
        //        try
        //        {
        //            _socket?.Dispose();
        //            HandleClientClose?.Invoke(_server, this);
        //        }
        //        catch (Exception ex)
        //        {
        //            AccessException(ex);
        //        }
        //    }
        //}
        /// <summary>
        /// 关闭当前连接
        /// </summary>
        public void Close()
        {
            if (_isClosed)
                return;
            try
            {
                _isClosed = true;
                _server.RemoveConnection(this);

                _isRec = false;
                _socket.BeginDisconnect(false, (asyncCallback) =>
                {
                    try
                    {
                        _socket.EndDisconnect(asyncCallback);
                    }
                    catch (Exception ex)
                    {
                        HandleException?.Invoke(ex);
                    }
                    finally
                    {
                        _socket.Dispose();
                    }
                }, null);
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(ex);
            }
            finally
            {
                try
                {
                    HandleClientClose?.Invoke(_server, this);
                }
                catch (Exception ex)
                {
                    HandleException?.Invoke(ex);
                }
            }
        }

        /// <summary>
        /// 判断是否处于已连接状态
        /// </summary>
        /// <returns></returns>
        public bool IsSocketConnected()
        {
            return !((_socket.Poll(1000, SelectMode.SelectRead) && (_socket.Available == 0)) || !_socket.Connected);
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 客户端连接接受新的消息后调用
        /// </summary>
        public Action<TcpSocketServer, TcpSocketConnection, byte[]> HandleRecMsg { get; set; }

        /// <summary>
        /// 客户端连接发送消息后回调
        /// </summary>
        public Action<TcpSocketServer, TcpSocketConnection, byte[]> HandleSendMsg { get; set; }

        /// <summary>
        /// 客户端连接关闭后回调
        /// </summary>
        public Action<TcpSocketServer, TcpSocketConnection> HandleClientClose { get; set; }

        /// <summary>
        /// 异常处理程序
        /// </summary>
        public Action<Exception> HandleException { get; set; }

        #endregion
    }
}
