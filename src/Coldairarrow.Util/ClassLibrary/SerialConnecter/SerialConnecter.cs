using System;
using System.IO.Ports;

namespace Coldairarrow.Util.Serial
{
    /// <summary>
    /// 串口通信
    /// </summary>
    public class SerialConnecter
    {
        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="portName">串口名，列如COM1,COM2等</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        public SerialConnecter(string portName, int baudRate = 9600, int dataBits = 8, int stopBits = 1)
        {
            _sp = new SerialPort
            {
                PortName = portName,
                BaudRate = baudRate,
                DataBits = dataBits,
                StopBits = (StopBits)stopBits
            };
        }

        #endregion

        #region 内部成员

        private SerialPort _sp { get; }

        #endregion

        #region 外部接口

        /// <summary>
        /// 开始串口
        /// </summary>
        public void Start()
        {
            try
            {
                if (!_sp.IsOpen)
                {
                    _sp.DataReceived += new SerialDataReceivedEventHandler((a, b) =>
                    {
                        int length = _sp.BytesToRead;

                        byte[] buffer = new byte[length];
                        _sp.Read(buffer, 0, buffer.Length);

                        HandleReceiveData?.BeginInvoke(buffer, null, null);
                    });

                    _sp.Open();
                    HandleStarted?.Invoke();
                }
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(ex);
            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        public void Stop()
        {
            try
            {
                _sp.Close();
                HandleStopped?.Invoke();
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(ex);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="bytes"></param>
        public void Send(byte[] bytes)
        {
            try
            {
                _sp.Write(bytes, 0, bytes.Length);
                HandleSendData?.Invoke(bytes);
            }
            catch (Exception ex)
            {
                HandleException?.Invoke(ex);
            }
        }

        /// <summary>
        /// 串口开始后处理事件
        /// </summary>
        public Action HandleStarted { get; set; }

        /// <summary>
        /// 串口停止后处理事件
        /// </summary>
        public Action HandleStopped { get; set; }

        /// <summary>
        /// 回调事件
        /// 接收到新的数据
        /// </summary>
        public Action<byte[]> HandleReceiveData { get; set; }

        /// <summary>
        /// 数据发送后处理的事件
        /// </summary>
        public Action<byte[]> HandleSendData { get; set; }

        /// <summary>
        /// 异常处理事件
        /// </summary>
        public Action<Exception> HandleException { get; set; }

        #endregion
    }
}
