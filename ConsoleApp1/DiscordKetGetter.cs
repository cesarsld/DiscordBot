using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace BHungerGaemsBot
{
    
    class DiscordKeyGetter
    {
        //private static readonly string keyPath = "..\\..\\..\\ShadBotKey.txt"; // server
        private static readonly string keyPath = "BotKey/ShadBotKey.txt"; // server
        //private static readonly string keyPath = "C:/Users/Cesar Jaimes/Documents/GitHub/DiscrodBot/ShadBotKey.txt"; // home
        public static string GetKey()
        {
            if (File.Exists(keyPath))
            {
                using (StreamReader sr = new StreamReader(keyPath, Encoding.UTF8))
                {
                    string key = sr.ReadToEnd();
                    return key;
                }
            }
            else
            {
                return "NDQ5NjY5OTIyMjIyNzY4MTMz.DeoDfg.RY3tz6u3tY_S_KBwthcy511L4w4";
            }
        }
    }
}
