using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Discord;
using Discord.Commands;
namespace DiscordBot.Axie.QoLListHandler
{
    public class QolListHandler
    {
        private string qolListPath;
        public QolListHandler(string path)
        {
            qolListPath = path;
        }

        private async Task<List<Suggestion>> GetQoLList()
        {
            List<Suggestion> list = new List<Suggestion>();
            if (File.Exists(qolListPath))
            {
                string fileData = "";
                using (StreamReader sr = new StreamReader(qolListPath))
                {
                    fileData = await sr.ReadToEndAsync();
                }
                if (fileData.Length != 0)
                {
                    string[] jsonFiles = Regex.Split(fileData, "\r\n|\r|\n");
                    foreach (var json in jsonFiles)
                    {
                        Suggestion obj = JObject.Parse(json).ToObject<Suggestion>();
                        if (obj != null) list.Add(obj);
                    }
                }
            }
            return list;
        }

        private async Task SetQoLList(List<Suggestion> list)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                stringBuilder.Append(JsonConvert.SerializeObject(list[i]));
                if (i != list.Count - 1)
                    stringBuilder.AppendLine();
            }

            using (StreamWriter sw = new StreamWriter(qolListPath))
            {
                await sw.WriteAsync(stringBuilder.ToString());
            }
        }

        public async Task AddSuggestion(string type, string description, ICommandContext context)
        {
            var list = await GetQoLList();
            var newSuggestion = new Suggestion(GetQoLType(type), description);
            list.Add(newSuggestion);
            await SetQoLList(list);
            await context.Message.AddReactionAsync(new Emoji("✅"));
        }

        public async Task RemoveSuggestion(int index, ICommandContext context)
        {
            var list = await GetQoLList();
            if (index >= 0 && index < list.Count)
            {
                list.RemoveAt(index);
                await SetQoLList(list);
                await context.Message.AddReactionAsync(new Emoji("✅"));
            }
        }

        public async Task<EmbedBuilder> GetEmbedQoLData()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Suggestion List");
            var list = await GetQoLList();
            int index = 0;
            foreach(var qol in list)
            {
                index++;
                embed.AddField($"{qol.type}",$"#{index} - " + qol.description);
            }
            embed.WithColor(Color.Purple);
            return embed;
        }

        private QoLType GetQoLType(string type)
        {
            type = type.ToLower();
            switch (type)
            {
                case "ui":
                    return QoLType.UI;
                case "animation":
                    return QoLType.Animation;
                case "core":
                    return QoLType.Core;
                case "gameplay":
                    return QoLType.Gameplay;
                default:
                    return QoLType.General;
            }
        }

        public static EmbedBuilder GetTypeEmbed()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Suggestion types");
            embed.WithDescription($"{QoLType.Animation} | {QoLType.Core} | {QoLType.UI} | {QoLType.Gameplay} | {QoLType.General}");
            embed.WithColor(Color.DarkBlue);
            return embed;
        }

    }

    public class Suggestion
    {
        public QoLType type;
        public string description;

        public Suggestion(QoLType _type, string data)
        {
            type = _type;
            description = data;
        }

    }
    public enum QoLType
    {
        UI,
        Animation,
        Core,
        Gameplay,
        General
    }
}
