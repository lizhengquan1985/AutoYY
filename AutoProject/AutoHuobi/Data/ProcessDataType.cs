using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoHuobi.Data
{
    /// <summary>
    /// 拐点数据
    /// </summary>
    public class FlexPoint
    {
        public bool isHigh { get; set; }
        public decimal open { get; set; }
        public long id { get; set; }
    }

    public class AnalyzeData
    {
        // 目前值， 5分钟最高值， 5分钟最低值，目前值得5分钟偏向（以最低值为准）,5分钟列表，1分钟列表
        /// <summary>
        /// 目前价位
        /// </summary>
        public decimal NowPrice { get; set; }
        /// <summary>
        /// key :1min, 5min, 15min, 30min, 60min, 1day, 1mon, 1week, 1year
        /// 目前值范围的一个最大值
        /// </summary>
        public Dictionary<string, decimal> HighestPrice { get; set; }
        public Dictionary<string, decimal> LowestPrice { get; set; }
        /// <summary>
        /// key :1min, 5min, 15min, 30min, 60min, 1day, 1mon, 1week, 1year
        /// 目前值的偏向（以最低值为准）目前值-最低值/最高值-最低值
        /// </summary>
        public Dictionary<string, decimal> NowLeanPercent { get; set; }
        /// <summary>
        /// key :1min, 5min, 15min, 30min, 60min, 1day, 1mon, 1week, 1year
        /// </summary>
        public Dictionary<string, List<KlineData>> KlineData { get; set; }
        /// <summary>
        /// 拐点数据， 基于各种百分比, 1,2,3,4,5,6,7,8,9,10
        /// </summary>
        public Dictionary<string, List<FlexPoint>> FlexPoint { get; set; }
        // 分析一段时间内的最高点，最低点。 今天，24小时，2天，3天，4天，一周，半个月，1个月，3个月。一年等
        // 分析短时间内的上涨速度，下降速度，让计算机感受
        // 整体行情的分析，整体数据的趋势，可能影响单个的趋势。
    }

    public class CoinDataPools
    {
        public static List<string> coins = new List<string> {
            "btc","bch","eth","etc","ltc",
            "eos","xrp","omg","dash","zec",
            // 创新
            "trx","mds","ela",
            "itc","nas","ruff","zil","dta",
            "let","ht","theta","hsr","qtum",
            "snt","iost","neo","storj","gnt",
            "cvc","smt","ven","elf","xem",
        };

        static Dictionary<string, AnalyzeData> analyzeDataDic = new Dictionary<string, AnalyzeData>();

        public static void InitCoinData(string coin)
        {
            if (!analyzeDataDic.ContainsKey(coin))
            {
                analyzeDataDic.Add(coin, new AnalyzeData());
            }
            else
            {
                analyzeDataDic[coin] = new AnalyzeData();
            }

            InitKline(coin, "1min");
            InitKline(coin, "5min");
            InitKline(coin, "15min");
            InitKline(coin, "30min");

            InitFlexlist(coin, (decimal)1.02);
            InitFlexlist(coin, (decimal)1.025);
            InitFlexlist(coin, (decimal)1.03);
            InitFlexlist(coin, (decimal)1.04);
            InitFlexlist(coin, (decimal)1.05);
            InitFlexlist(coin, (decimal)1.06);
            InitFlexlist(coin, (decimal)1.07);
            InitFlexlist(coin, (decimal)1.08);
            InitFlexlist(coin, (decimal)1.09);
            InitFlexlist(coin, (decimal)1.10);
        }

        private static void InitKline(string coin, string period)
        {
            ResponseKline res = HbAnalyzeApi.kline(coin + "usdt", period, 1440);
            if (analyzeDataDic[coin].KlineData == null)
            {
                analyzeDataDic[coin].KlineData = new Dictionary<string, List<KlineData>>();
            }
            analyzeDataDic[coin].KlineData.Add(period, res.data);

            if (analyzeDataDic[coin].NowPrice <= (decimal)0.000001)
            {
                analyzeDataDic[coin].NowPrice = res.data[0].open;
            }
            // 生产其他数据
            HighOrLowPrice(coin, period);
            InitNowLeanPercent(coin, period);
        }

        private static void HighOrLowPrice(string coin, string period)
        {
            var klineData = analyzeDataDic[coin].KlineData[period];
            var high = (decimal)0;
            var low = (decimal)9999999;
            foreach (var item in klineData)
            {
                if (high < item.open)
                {
                    high = item.open;
                }
                if (low > item.open)
                {
                    low = item.open;
                }
            }
            if (analyzeDataDic[coin].HighestPrice == null)
            {
                analyzeDataDic[coin].HighestPrice = new Dictionary<string, decimal>();
            }

            if (analyzeDataDic[coin].HighestPrice.ContainsKey(period))
            {
                analyzeDataDic[coin].HighestPrice[period] = high;
            }
            else
            {
                analyzeDataDic[coin].HighestPrice.Add(period, high);
            }

            if (analyzeDataDic[coin].LowestPrice == null)
            {
                analyzeDataDic[coin].LowestPrice = new Dictionary<string, decimal>();
            }

            if (analyzeDataDic[coin].LowestPrice.ContainsKey(period))
            {
                analyzeDataDic[coin].LowestPrice[period] = high;
            }
            else
            {
                analyzeDataDic[coin].LowestPrice.Add(period, high);
            }
        }

        private static void InitNowLeanPercent(string coin, string period)
        {
            if (analyzeDataDic[coin].NowLeanPercent == null)
            {
                analyzeDataDic[coin].NowLeanPercent = new Dictionary<string, decimal>();
            }

            var high = analyzeDataDic[coin].HighestPrice[period];
            var low = analyzeDataDic[coin].LowestPrice[period];
            var nowPrice = analyzeDataDic[coin].NowPrice;

            if (analyzeDataDic[coin].NowLeanPercent.ContainsKey(period))
            {
                analyzeDataDic[coin].NowLeanPercent[period] = (nowPrice - low) / (high - low);
            }
            else
            {
                analyzeDataDic[coin].NowLeanPercent.Add(period, (nowPrice - low) / (high - low));
            }
        }

        private static void InitFlexlist(string coin, decimal percent)
        {
            var oneMinKline = analyzeDataDic[coin].KlineData["1min"];

            List<FlexPoint> flexPointList = new List<FlexPoint>();

            decimal openHigh = oneMinKline[0].open;
            decimal openLow = oneMinKline[0].open;
            long idHigh = oneMinKline[0].id;
            long idLow = oneMinKline[0].id;
            int lastHighOrLow = 0; // 1 high, -1: low
            foreach (var item in oneMinKline)
            {
                if (item.open > openHigh)
                {
                    openHigh = item.open;
                    idHigh = item.id;
                }
                if (item.open < openLow)
                {
                    openLow = item.open;
                    idLow = item.id;
                }

                if (openHigh >= openLow * percent)
                {
                    var dtHigh = Utils.GetDateById(idHigh);
                    var dtLow = Utils.GetDateById(idLow);
                    // 说明是一个节点了。
                    if (idHigh > idLow && lastHighOrLow != 1)
                    {
                        flexPointList.Add(new FlexPoint() { isHigh = true, open = openHigh, id = idHigh });
                        lastHighOrLow = 1;
                        openHigh = openLow;
                        idHigh = idLow;
                    }
                    else if (idHigh < idLow && lastHighOrLow != -1)
                    {
                        flexPointList.Add(new FlexPoint() { isHigh = false, open = openLow, id = idLow });
                        lastHighOrLow = -1;
                        openLow = openHigh;
                        idLow = idHigh;
                    }
                    else if (lastHighOrLow == 1)
                    {

                    }
                }
            }

            if (analyzeDataDic[coin].FlexPoint == null)
            {
                analyzeDataDic[coin].FlexPoint = new Dictionary<string, List<FlexPoint>>();
            }

            if (analyzeDataDic[coin].FlexPoint.ContainsKey(percent.ToString()))
            {
                analyzeDataDic[coin].FlexPoint[percent.ToString()] = flexPointList;
            }
            else
            {
                analyzeDataDic[coin].NowLeanPercent.Add(percent.ToString(), flexPointList);
            }
        }
    }

    // 策略， 如果普涨， 等平稳，找到涨的厉害的卖
}
