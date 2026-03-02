using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace chat_web_demo
{
    public class PickNumberFunctions
    {
        static string[] KnownLotteries = new[] { "樂透", "大樂透", "威力彩", "黑大樂透" };

        [Description("為樂透、大樂透、威力彩、黑大樂透等彩券選擇號碼")]
        public Task<string> PickLotteryNumbersAsync(
            [Description("彩券名稱，包含 '樂透'、'大樂透'、'威力彩'、'黑大樂透'。")] string lotteryName)
        {
            if (!KnownLotteries.Contains(lotteryName))
            {
                throw new ArgumentException($"不支援 {lotteryName}. 支援的彩券有: {string.Join(", ", KnownLotteries)}");
            }
            int count, min, max;
            bool reqExtraNum = true;
            switch (lotteryName)
            {
                case "樂透":
                    count = 6;
                    min = 1;
                    max = 49;
                    reqExtraNum = false;
                    break;
                case "大樂透":
                    count = 6;
                    min = 1;
                    max = 49;
                    break;
                case "威力彩":
                    count = 6;
                    min = 1;
                    max = 38;
                    break;
                case "黑大樂透":
                    count = 8;
                    min = 0;
                    max = 31;
                    reqExtraNum = false;
                    break;
                default:
                    throw new NotImplementedException();
            }
            var random = new Random();
            var numbers = Enumerable.Range(min, max - min + 1)
                .OrderBy(_ => random.Next()).Take(count)
                .OrderBy(o => o).ToArray();
            if (reqExtraNum)
            {
                var extraNum = random.Next(1, 9); // 額外號碼在 1-8 之間
                return Task.FromResult($"號碼為 {string.Join(", ", numbers)}，特別號碼為 {extraNum}，祝您中獎！");
            }
            return Task.FromResult($"號碼為 {string.Join(", ", numbers)}，祝您中獎！");
        }
    }
}