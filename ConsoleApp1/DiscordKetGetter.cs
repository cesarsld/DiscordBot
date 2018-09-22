using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBot
{
    
    class DiscordKeyGetter
    {
        //private static readonly string keyPath = "..\\..\\..\\ShadBotKey.txt"; // server
        private static readonly string keyPath = "BotKey/ShadBotKey.txt"; // server
        private static readonly string testKeyPath = "TestKey/TestKey.txt"; //home
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
                using (StreamReader sr = new StreamReader(testKeyPath, Encoding.UTF8))
                {
                    string key = sr.ReadToEnd();
                    return key;
                }
            }
        }
    }
}
