using AutoHuobi;
using AutoHuobi.Data;
using log4net;
using log4net.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoProject
{
    class Program
    {
        static ILog logger = null;
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            logger = LogManager.GetLogger(typeof(Program));
            logger.Error("-------------------------- begin ---------------------------------");

            while (true)
            {
                logFlex();
                Thread.Sleep(1000 * 60 * 60);
            }
            
            // 排序
            // 1. 最近5天偏高,0.95
            // 2. 最近5天偏低,0.05
            // 3. 拐点, 是否普遍拐点,
            // 4. 最近1小时的速度

            Console.ReadLine();
        }

        private static void logFlex()
        {
            Console.WriteLine($"{DateTime.Now}");
            foreach (var coin in CoinDataPools.coins)
            {
                Console.WriteLine($"{coin}");
                CoinDataPools.InitCoinData(coin);
            }
            Console.WriteLine($"{DateTime.Now}");

            // 推荐24小时内最低的
            Dictionary<string, decimal> coinLeanPercent = new Dictionary<string, decimal>();
            Dictionary<string, Dictionary<string, int>> countData = new Dictionary<string, Dictionary<string, int>>();
            foreach (var coin in CoinDataPools.coins)
            {
                var analyzeData = CoinDataPools.GetAnalyzeData(coin);
                decimal nowLeanPercent = analyzeData.NowLeanPercent["1min"];
                coinLeanPercent.Add(coin, nowLeanPercent);

                if (!countData.ContainsKey(coin))
                {
                    countData.Add(coin, new Dictionary<string, int>());
                }

                countData[coin].Add("1.015", log(coin, analyzeData, "1.015"));
                countData[coin].Add("1.02", log(coin, analyzeData, "1.02"));
                countData[coin].Add("1.03", log(coin, analyzeData, "1.03"));
                countData[coin].Add("1.035", log(coin, analyzeData, "1.035"));
                countData[coin].Add("1.04", log(coin, analyzeData, "1.04"));
                countData[coin].Add("1.045", log(coin, analyzeData, "1.045"));
                countData[coin].Add("1.05", log(coin, analyzeData, "1.05"));
                countData[coin].Add("1.055", log(coin, analyzeData, "1.055"));
                countData[coin].Add("1.06", log(coin, analyzeData, "1.06"));
                countData[coin].Add("1.07", log(coin, analyzeData, "1.07"));
            }

            logger.Error($"--->           1.015  1.02  1.03  1.035   1.04  1.045   1.05  1.055   1.06   1.07");
            int[] countArr = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (var coin in countData.Keys)
            {
                var countStr = "";
                int index = 0;
                foreach (var percent in countData[coin].Keys)
                {
                    countStr += ("" + countData[coin][percent]).PadLeft(7, ' ');
                    countArr[index] += countData[coin][percent];
                    index++;
                }
                logger.Error($"--->  {coin.PadLeft(6, ' ')}  {countStr}");
            }
            var countStrPrint = "";
            foreach (var c in countArr)
            {
                countStrPrint += c.ToString().PadLeft(7, ' ');
            }
            logger.Error($"--->          {countStrPrint}");
            logger.Error($"");
            logger.Error($"");
        }

        private static int log(string coin, AnalyzeData analyzeData, string percent)
        {
            logger.Error($"--->  {coin}  {percent}");
            foreach (var item in analyzeData.FlexPoint[percent])
            {
                logger.Error($"isHigh:{item.isHigh}   open:{item.open} date:{Utils.GetDateById(item.id).ToString("yyyy-MM-dd HH:mm:ss")}");
            }
            //logger.Error($"");
            //logger.Error($"");
            //logger.Error($"");
            return analyzeData.FlexPoint[percent].Count;
        }
    }
}
