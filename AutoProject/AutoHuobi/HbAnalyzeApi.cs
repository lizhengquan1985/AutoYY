using AutoHuobi.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHuobi
{
    public class HbAnalyzeApi
    {
        public static ResponseKline kline(string symbol, string period, int size = 300)
        {
            var url = $"{HbConfig.baseUrl}/history/kline";
            url += $"?symbol={symbol}&period={period}&size={size}";

            int httpCode = 0;
            var result = HttpUtils.RequestDataSync(url, "GET", null, null, out httpCode);
            return JsonConvert.DeserializeObject<ResponseKline>(result);
        }
    }
}
