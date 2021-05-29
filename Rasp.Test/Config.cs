using System;
using Simple.BotUtils.Data;

namespace Rasp.Test
{
    public class Config
    {
        public string TelegramToken { get; set; }
        public long TelegramAdmin { get; set; }

        public void Save()
        {
            XmlSerializer.ToFile("config.xml", this);
        }
        public static Config Load()
        {
            var cfg = XmlSerializer.LoadOrCreate("config.xml", new Config());
            return cfg;
        }
    }
}
