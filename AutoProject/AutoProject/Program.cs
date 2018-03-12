using AutoHuobi.Data;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoProject
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            ILog logger = LogManager.GetLogger(typeof(Program));
            logger.Error("-------------------------- begin ---------------------------------");

            Console.WriteLine($"{DateTime.Now}");
            foreach (var coin in CoinDataPools.coins)
            {
                Console.WriteLine($"{coin}");
                CoinDataPools.InitCoinData(coin);
            }
            Console.WriteLine($"{DateTime.Now}");

            // 推荐24小时内最低的
            Dictionary<string, decimal> coinLeanPercent = new Dictionary<string, decimal>();
            foreach(var coin in CoinDataPools.coins)
            {
                var analyzeData = CoinDataPools.GetAnalyzeData(coin);
                decimal nowLeanPercent = analyzeData.NowLeanPercent["1min"];
                coinLeanPercent.Add(coin, nowLeanPercent);
            }
            // 排序

            Console.ReadLine();
        }
    }
}
