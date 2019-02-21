using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 电信IOT物联网平台接口对接SKD
    /// </summary>
    public class CTCC_IOT_ApiSDK
    {
        public CTCC_IOT_ApiSDK(string appId, string appSecret, string apiRootUrl, X509Certificate2 cerFile)
        {
            _appId = appId;
            _appSecret = appSecret;
            _apiRootUrl = apiRootUrl;
            _cerFile = cerFile;
        }
        private string _appId { get; }
        private string _appSecret { get; }
        private string _apiRootUrl { get; }
        private X509Certificate _cerFile { get; }
        private KeyValuePair<string, DateTime> _accessTokenEndTime { get; set; } = new KeyValuePair<string, DateTime>("1", DateTime.MinValue);
        private string _accessToken
        {
            get
            {
                if (DateTime.Now > _accessTokenEndTime.Value)
                {
                    string resData = GetAccessToken();
                    JObject obj = JsonConvert.DeserializeObject<JObject>(resData);
                    string accessToken = obj["accessToken"]?.ToString();
                    int seconds = (int)obj["expiresIn"] / 2;
                    DateTime endTime = DateTime.Now.AddSeconds(seconds);
                    _accessTokenEndTime = new KeyValuePair<string, DateTime>(accessToken, endTime);
                }

                return _accessTokenEndTime.Key;
            }
        }
        private string GetAccessToken()
        {
            string url = $"{_apiRootUrl}/iocm/app/sec/v1.1.0/login";
            Dictionary<string, object> paramters = new Dictionary<string, object>();
            paramters.Add("appId", _appId);
            paramters.Add("secret", _appSecret);

            return HttpHelper.PostData(url, paramters, null, ContentType.Form, _cerFile);
        }

        /// <summary>
        /// 注册直连设备
        /// </summary>
        /// <param name="IMEI">设备IMEI号码</param>
        /// <returns>设备Id</returns>
        public string RegisterDevice(string IMEI)
        {
            string url = $"{_apiRootUrl}/iocm/app/reg/v1.2.0/devices?appId={_appId}";
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("app_key", _appId);
            headers.Add("Authorization", $"Bearer {_accessToken}");

            Dictionary<string, object> paramters = new Dictionary<string, object>();
            paramters.Add("appId", _appId);
            paramters.Add("verifyCode", IMEI);
            paramters.Add("nodeId", IMEI);
            paramters.Add("timeout", 0);

            string resData = HttpHelper.PostData(url, paramters, headers, ContentType.Json, _cerFile);
            string deviceId = JsonConvert.DeserializeObject<JObject>(resData)["deviceId"]?.ToString();

            UpdateDeviceInfoReqDTO deviceInfo = new UpdateDeviceInfoReqDTO
            {
                name = IMEI,
                deviceType = "CarLock",
                model = "CarLockNBIOT",
                protocolType = "CoAP",
                manufacturerId = "wuxiang",
                manufacturerName = "wuxiang"
            };

            ChangeDeviceInfo(deviceId, deviceInfo);

            return deviceId;
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="deviceId">设备Id</param>
        /// <returns></returns>
        public string DeleteDevice(string deviceId)
        {
            string url = $"{_apiRootUrl}/iocm/app/dm/v1.1.0/devices/{deviceId}";
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("app_key", _appId);
            headers.Add("Authorization", $"Bearer {_accessToken}");

            Dictionary<string, object> paramters = new Dictionary<string, object>();
            paramters.Add("deviceId", deviceId);
            paramters.Add("appId", _appId);

            string resStr = HttpHelper.RequestData(HttpMethod.Delete, url, paramters, headers, ContentType.Json, _cerFile);

            return resStr;
        }

        /// <summary>
        /// 修改设备信息
        /// </summary>
        /// <param name="deviceId">设备ID</param>
        /// <param name="deviceInfo">设备信息</param>
        public void ChangeDeviceInfo(string deviceId, UpdateDeviceInfoReqDTO deviceInfo)
        {
            string url = $"{_apiRootUrl}/iocm/app/dm/v1.2.0/devices/{deviceId}";
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("app_key", _appId);
            headers.Add("Authorization", $"Bearer {_accessToken}");

            HttpHelper.RequestData("PUT", url, JsonConvert.SerializeObject(deviceInfo), "application/json", headers, _cerFile);
        }

        public string SubscribeData(string notifyType, string callBackUrl)
        {
            string url = $"{_apiRootUrl}/iocm/app/sub/v1.2.0/subscribe";
            var data = new
            {
                notifyType,
                callbackurl = callBackUrl
            };

            return RequestData("POST", url, data.ToJson());
        }

        private string RequestData(string method, string url, string body)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("app_key", _appId);
            headers.Add("Authorization", $"Bearer {_accessToken}");

            return HttpHelper.RequestData(method, url, body, "application/json", headers, _cerFile);
        }

        public void SendCmd(string deviceId, byte[] bytes)
        {
            string url = $"{_apiRootUrl}/iocm/app/cmd/v1.4.0/deviceCommands?appId={_appId}";

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("app_key", _appId);
            headers.Add("Authorization", $"Bearer {_accessToken}");

            Dictionary<string, int> paramters = new Dictionary<string, int>();
            for (int i = 0; i < bytes.Length; i++)
            {
                paramters.Add($"Data{i}", bytes[i]);
            }
            var reqData = new
            {
                deviceId,
                expireTime = 0,
                command = new
                {
                    serviceId = "LockData",
                    method = "Control",
                    paras = paramters
                }
            };

            HttpHelper.RequestData("POST", url, JsonConvert.SerializeObject(reqData), "application/json", headers, _cerFile);
        }

        public void MoveLockUp(string deviceId)
        {

        }

        /// <summary>
        /// 设备信息
        /// </summary>
        public class UpdateDeviceInfoReqDTO
        {
            /// <summary>
            /// 设备名
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// 设备类型
            /// </summary>
            public string deviceType { get; set; }

            /// <summary>
            /// 设备型号
            /// </summary>
            public string model { get; set; }

            /// <summary>
            /// 协议类型
            /// </summary>
            public string protocolType { get; set; }

            /// <summary>
            /// 厂商ID
            /// </summary>
            public string manufacturerId { get; set; }

            /// <summary>
            /// 厂商名
            /// </summary>
            public string manufacturerName { get; set; }

            /// <summary>
            /// 是否冻结
            /// </summary>
            public string mute { get; set; } = "FALSE";
        }
    }
}
