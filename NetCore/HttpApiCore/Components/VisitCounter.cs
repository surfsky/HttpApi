using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.HttpApi.Components
{
    // 访问频率检测
    // 假设实现：1分钟内超过200次访问就判定是攻击
    // 思路1：每个IP保留最近的200次记录，每次访问时判断头尾时间差，若低于1分钟判定是攻击。
    // 思路2：保留首次记录时间、访问次数；若达到限定时间，若访问次数超过200次判定是攻击。（采用）
    /// <summary>
    /// 访问计数器
    /// </summary>
    internal class VisitCounter
    {
        /// <summary>访问记录</summary>
        internal class Visit
        {
            public string IP { get; set; }
            public string URL { get; set; }
            public DateTime StartDt { get; set; }
            public long Cnt { get; set; }

            public override string ToString()
            {
                return $"IP={IP}, URL={URL}, StartDt={StartDt:yyyy-MM-dd HH:mm:ss}, Cnt={Cnt}";
            }
        }

        // 访问列表及线程安全保护锁
        static List<Visit> _visits = new List<Visit>();
        static object _lock = new object();

        /// <summary>访问是否过于密集</summary>
        /// <param name="seconds">检测周期（秒）</param>
        /// <param name="max">最大值</param>
        /// <example>
        /// protected void Application_BeginRequest(object sender, EventArgs e)
        /// {
        ///     var ip = ....;
        ///     var url = ...;
        ///     if (VisitCounter.IsOverFreqency(ip, url, 10, 100))
        ///     {
        ///         HttpContext.Current.Request.Abort();
        ///     }
        /// }
        /// </example>
        public static bool IsHeavy(string ip, string url, int seconds, int max)
        {
            if (max <= 0 || seconds <= 0)
                return false;

            lock (_lock)
            {
                var now = DateTime.Now;
                var visit = _visits.FirstOrDefault(t => t.IP == ip && t.URL == url);
                // 首次做记录
                if (visit == null)
                {
                    visit = new Visit();
                    visit.IP = ip;
                    visit.URL = url;
                    visit.StartDt = now;
                    visit.Cnt = 1;
                    _visits.Add(visit);
                    Log("visit new : " + visit.ToString());
                    return false;
                }
                // 超时了重新开始记录
                if (now > visit.StartDt.AddSeconds(seconds))
                {
                    visit.StartDt = now;
                    visit.Cnt = 1;
                    Log("visit reboot : " + visit.ToString());
                    return false;
                }
                // 在计算周期内，则计数值加一
                else
                {
                    visit.Cnt++;
                    Log("visit " + visit.ToString());
                    if (visit.Cnt >= max)
                    {
                        Log("visit over frequency : " + visit.ToString());
                        visit.StartDt = now;
                        visit.Cnt = 1;
                        return true;
                    }
                }
                return false;
            }
        }

        //
        static void Log(string text)
        {
            System.Diagnostics.Trace.WriteLine(text);
        }
    }
}
