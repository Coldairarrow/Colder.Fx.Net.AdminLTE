namespace Coldairarrow.Util.WcfMS
{
    /// <summary>
    /// 配置信息
    /// </summary>
    public class WcfMSConfig
    {
        /// <summary>
        /// 项目名
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// Redis配置
        /// 注：默认为localhost:6379
        /// </summary>
        public string RedisConfig { get; set; } = "localhost:6379";

        /// <summary>
        /// 当前服务器Ip地址
        /// 注：若不传入则获取当前内网Ip地址
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 是否开启传输安全校验加密
        /// </summary>
        public bool OpenSecurity { get; set; } = false;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
    }
}
