using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using Lunar;
using System.Text;
using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;

namespace chat_web_demo
{
    public class PickDayAgent
    {
        public ChatClient _chatClient { get; }
        string[] KnownActivities =  {"祭祀", "祈福", "求嗣", "开光", "塑绘", "齐醮", "斋醮", "沐浴", "酬神", "造庙", "祀灶", "焚香", "谢土", "出火", "雕刻", "嫁娶", "订婚", "纳采", "问名", "纳婿", "归宁", "安床", "合帐", "冠笄", "订盟", "进人口", "裁衣", "挽面", "开容", "修坟", "启钻", "破土", "安葬", "立碑", "成服", "除服", "开生坟", "合寿木", "入殓", "移柩", "普渡", "入宅", "安香", "安门", "修造", "起基", "动土", "上梁", "竖柱", "开井开池", "作陂放水", "拆卸", "破屋", "坏垣", "补垣", "伐木做梁", "作灶", "解除", "开柱眼", "穿屏扇架", "盖屋合脊", "开厕", "造仓", "塞穴", "平治道涂", "造桥", "作厕", "筑堤", "开池", "伐木", "开渠", "掘井", "扫舍", "放水", "造屋", "合脊", "造畜稠", "修门", "定磉", "作梁", "修饰垣墙", "架马", "开市", "挂匾", "纳财", "求财", "开仓", "买车", "置产", "雇佣", "出货财", "安机械", "造车器", "经络", "酝酿", "作染", "鼓铸", "造船", "割蜜", "栽种", "取渔", "结网", "牧养", "安碓磑", "习艺", "入学", "理发", "探病", "见贵", "乘船", "渡水", "针灸", "出行", "移徙", "分居", "剃头", "整手足甲", "纳畜", "捕捉", "畋猎", "教牛马", "会亲友", "赴任", "求医", "治病", "词讼", "起基动土", "破屋坏垣", "盖屋", "造仓库", "立券交易", "交易", "立券", "安机", "会友", "求医疗病", "诸事不宜", "馀事勿取", "行丧", "断蚁", "归岫", "无"};

        ChatClientAgent agent;
        public PickDayAgent(ChatClient chatClient)
        {
            _chatClient = chatClient;
            agent = _chatClient.AsAIAgent(
                instructions: $"""
                你是一個擇日小助手，依據使用者提供的日期範圍及活動類型，使用 ShowRecommendedActivitiesAsync 工具挑選適合的日子。
                - 若使用者指定相對日期區間（例如「未來一週」），使用 GetNowTimeAsync 工具取得現在的日期時間計算實際日期範圍。
                - 使用 ShowRecommendedActivitiesAsync 工具來獲取每一天的宜、忌活動。
                - 嘗試將活動歸類到以下項目之一: {string.Join(", ", KnownActivities)}等。
                - 根據使用者提供的活動類型，找出宜、忌該活動的日子，並顯示當日宜忌項目的完整原文。
                - 工具回傳結果可能包含簡體中文，請一律轉為繁體中文。
                """,
                tools: [ 
                    AIFunctionFactory.Create(GetNowTimeAsync),
                    AIFunctionFactory.Create(ShowRecommendedActivitiesAsync)
                ]
            );
        }

        [Description("根據使用者提供的日期範圍及活動類型，挑選適合的日子。")]
        public async Task<string> PickDayAsync(string question)
        {
            var response = await agent.RunAsync(question);
            return response.ToString() ?? string.Empty;
        }

        [Description("取得現在日期時間")]
        public async Task<DateTime> GetNowTimeAsync() => DateTime.Now;

        [Description("顯示指定日期範圍，每天的宜、忌活動")]
        public async Task<string> ShowRecommendedActivitiesAsync(
            [Description("開始日期")] DateTime startDate,
            [Description("結束日期")] DateTime endDate
        )        
        {
            if (startDate.CompareTo(endDate) > 0)
            {
                (startDate, endDate) = (endDate, startDate);
            }
            var date = startDate;
            var sb = new StringBuilder();
            while (date.CompareTo(endDate) <= 0)
            {
                var lunarDate =Lunar.Lunar.FromDate(date);
                var okActivities = string.Join(", ", lunarDate.GetDayYi());
                var avoidActivities = string.Join(", ", lunarDate.GetDayJi());
                var result = $"""
                【{date:yyyy-MM-dd}】
                    - 宜 {okActivities}
                    - 忌 {avoidActivities}
                """;
                Console.WriteLine(result);
                sb.AppendLine(result);   
                date = date.AddDays(1);
            }
            return sb.ToString();
        }        
    }
}