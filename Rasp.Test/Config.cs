using Simple.BotUtils.Data;
using Simple.BotUtils.Startup;

namespace Rasp.Test
{
    public class Config : ConfigBase
    {
        [ArgumentKey("-token")]
        public string TelegramToken { get; set; }
        [ArgumentKey("-admin")]
        public long TelegramAdmin { get; set; }

    }
}
