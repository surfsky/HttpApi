using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.HttpApi.Components
{
    /// <summary>IP 黑白名单设置</summary>
    internal class IPFilter
    {
        [T("名称")]     public string IP { get; set; }
        [T("封禁时间")] public DateTime? StartDt { get; set; }
        [T("解禁时间")] public DateTime? EndDt { get; set; }

        //-----------------------------------------------
        // 缓存及判断逻辑
        //-----------------------------------------------
        /// <summary>列表</summary>
        public static List<IPFilter> All = new List<IPFilter>();

        /// <summary>指定 IP 是否被禁止</summary>
        public static bool IsBanned(string ip)
        {
            foreach (var filter in All)
            {
                if (filter.IP == ip)
                {
                    var now = DateTime.Now;
                    if (filter.EndDt == null)    return true;  // 结束时间为空，则永远封禁
                    else if (filter.EndDt > now) return true;  // 尚未到解禁时间
                    return false;
                }
            }
            return false;
        }

        /// <summary>禁止指定IP访问网站</summary>
        /// <param name="minutes">封禁分钟数。如果为空，则永久封禁</param>
        public static void Ban(string ip, int? minutes)
        {
            var now = DateTime.Now;
            var filter = All.FirstOrDefault(t => t.IP == ip);
            if (filter == null)
            {
                filter = new IPFilter() { IP = ip };
            }
            filter.StartDt = now;
            filter.EndDt = (minutes == null) ? (DateTime?)null : now.AddMinutes(minutes.Value);
            All.Add(filter);
        }
    }
}
