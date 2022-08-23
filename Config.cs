using System.IO;
using TShockAPI;
using Newtonsoft.Json;

namespace ChangeKey
{
    public class Config
    {
        //路径
        public static string ConfigPath = $"{TShock.SavePath}/keyChange.json";
        public bool 是否需要击败新三王;
        public bool 是否需要击败世纪之花;
        public bool 是否需要击败骷髅王;
        /// <summary>
        /// 确认配置文件存在，不存在则创建并填入默认值
        /// </summary>
        public static void EnsureFile()
        {
            if (!File.Exists(ConfigPath))
            {
                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(new Config(true,true,false)));
            }
        }
        public static Config ReadConfig()
        {
            //读取ConfigFile
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
        }
        public Config(bool downedBoss3,bool ThreeKings,bool Flower)
        {
            是否需要击败骷髅王 = downedBoss3;
            是否需要击败新三王 = ThreeKings;
            是否需要击败世纪之花 = Flower;
        }
    }
}
