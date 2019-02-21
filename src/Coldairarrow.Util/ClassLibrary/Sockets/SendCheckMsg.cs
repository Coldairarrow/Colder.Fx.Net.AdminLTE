using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coldairarrow.Util.Sockets
{
    class SendCheckMsg
    {
        private ConcurrentDictionary<string, SynchronizedCollection<MsgDto>> _msgList { get; set; } = new ConcurrentDictionary<string, SynchronizedCollection<MsgDto>>();
        public EventWaitHandle SendMsg(string connectionId, byte[] sendBytes, Func<string, byte[], bool> check, int timeOut)
        {
            MsgDto newMsg = new MsgDto
            {
                SendBytes = sendBytes,
                SendTime = DateTime.Now,
                WaitHandle = new AutoResetEvent(false),
                Check = check,
                TimeOut = timeOut
            };

            Task.Run(() =>
            {
                if (!_msgList.ContainsKey(connectionId))
                    _msgList.TryAdd(connectionId, new SynchronizedCollection<MsgDto>());
                var sendList = _msgList[connectionId];
                sendList.Add(newMsg);
                Task.Run(() =>
                {
                    Thread.Sleep(timeOut);
                    sendList.Remove(newMsg);
                });
            });

            return newMsg.WaitHandle;
        }

        public void RecMsg(string conectionId, byte[] recBytes)
        {
            DateTime time = DateTime.Now;
            if (!_msgList.ContainsKey(conectionId))
                return;

            var sendList = _msgList[conectionId];
            var theMsg = sendList.Where(x => x.Check(conectionId, recBytes)).OrderBy(x => x.SendTime).FirstOrDefault();
            if (theMsg != null)
            {
                theMsg.WaitHandle.Set();
                DeleteMsg(conectionId, theMsg);
            }
        }

        private void DeleteMsg(string connectionId, MsgDto msg)
        {
            if (!_msgList.ContainsKey(connectionId))
                _msgList.TryAdd(connectionId,new SynchronizedCollection<MsgDto>());
            var sendList = _msgList[connectionId];
            sendList.Remove(msg);
        }
        class MsgDto
        {
            public byte[] SendBytes { get; set; }
            public DateTime SendTime { get; set; }
            public EventWaitHandle WaitHandle { get; set; }
            public Func<string, byte[], bool> Check { get; set; }
            public int TimeOut { get; set; }
        }
    }
}
