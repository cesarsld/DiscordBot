using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Axie;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Axie.TournamentTool
{
    class TournamentUtility
    {

        public static Discord.EmbedBuilder GetPreBattleData(JObject script)
        {
            JArray fighterArray = JArray.FromObject(script["metadata"]["fighters"]);
            var entityArray = new BattleEntity[6];
            int i = 0;
            foreach (var fighter in fighterArray)
            {
                entityArray[i] = fighter.ToObject<BattleEntity>();
                i++;
            }
            var embed = new Discord.EmbedBuilder();
            embed.WithTitle("Pre battle stats");
            for (i = 0; i < 2; i++)
            {
                embed.AddField($"Team {i + 1}", "---------------");
                for (int j = 0; j < 3; j++)
                {
                    embed.AddInlineField($"Axie #{entityArray[j + (3 * i)].id}", entityArray[j + (3 * i)].GetRatings() + entityArray[j + (3 * i)].GetStats() + entityArray[j + (3 * i)].GetMoveset());
                }
            }
            embed.WithColor(Discord.Color.Red);
            return embed;
        }
    }
}
