using System;
using System.Linq;
using CoinMarketCap;
using System.IO;
using System.Threading.Tasks;
using CoinMarketCap.Entities;

namespace CoinMon
{
    public static class Program
    {
        public static readonly string CacheFilePath = "/tmp/coinmon.cache";
        public const int CacheMaxAgeMinutes = 60;
        public static bool IgnoreCache;

        public static int Left = Console.CursorLeft;
        public static int Top = Console.CursorTop;

        static async Task Main(string[] args)
        {
            Left = Console.CursorLeft;
            Top = Console.CursorTop;
            Console.WriteLine("Loading...");
            IgnoreCache = args.Contains("-i");
            string output;

            if (File.GetLastWriteTime(CacheFilePath).AddMinutes(CacheMaxAgeMinutes) < DateTime.Now || IgnoreCache)
            {
                var (btc, eth, xrp, trx) = await RequestCoinIfo();
                output = FormatOutput(btc, eth, xrp, trx);
                await File.WriteAllTextAsync(CacheFilePath, output);
            }
            else
            {
                output = await LoadAndFormatCache();
            }

            Console.SetCursorPosition(Left, Top);
            Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r");
            Console.SetCursorPosition(Left, Top);
            Console.WriteLine(output);
        }

        private static async Task<(TickerEntity btc, TickerEntity eth, TickerEntity xrp, TickerEntity trx)> RequestCoinIfo()
        {
            //This library shits itself if we try to await all of them concurrently (Task.WaitAll)
            //So we gotta do it sequencially, taking 4x as long ...
            var client = CoinMarketCapClient.GetInstance();
            var btc = await client.GetTickerAsync("bitcoin");
            var eth = await client.GetTickerAsync("ethereum");
            var xrp = await client.GetTickerAsync("ripple");
            var trx = await client.GetTickerAsync("tronix");
            return (btc, eth, xrp, trx);
        }

        private static string FormatOutput(TickerEntity btcTask, TickerEntity ethTask, TickerEntity xrpTask, TickerEntity trxTask)
        {
            var output = $"BTC: {btcTask.PriceUsd?.ToString("C").PadLeft(10, ' ')}\t\tETH: {ethTask.PriceUsd?.ToString("C").PadLeft(10, ' ')}\nXRP: {xrpTask.PriceUsd?.ToString("C").PadLeft(10, ' ')}\t\tTRX: {trxTask.PriceUsd?.ToString("C").PadLeft(10, ' ')}";
            var offset = Math.Max(0, Console.WindowWidth / 2 - output.Length / 2);
            output = $"{"".PadLeft(offset)}BTC: {btcTask.PriceUsd?.ToString("C").PadLeft(10, ' ')}\t\tETH: {ethTask.PriceUsd?.ToString("C").PadLeft(10, ' ')}\n{"".PadLeft(offset)}XRP: {xrpTask.PriceUsd?.ToString("C").PadLeft(10, ' ')}\t\tTRX: {trxTask.PriceUsd?.ToString("C").PadLeft(10, ' ')}";
            return output;
        }

        private static async Task<string> LoadAndFormatCache()
        {
            var output = await File.ReadAllTextAsync(CacheFilePath);
            var lines = output.Split('\n');
            lines[0] = lines[0].Trim();
            lines[1] = lines[1].Trim();
            var offset = Math.Max(0,Console.WindowWidth / 2 - lines[0].Length);
            lines[0] = "\t".PadLeft(offset) + lines[0];
            lines[1] = "\t".PadLeft(offset) + lines[1];
            output = lines[0] + Environment.NewLine + lines[1];
            return output;
        }
    }
}