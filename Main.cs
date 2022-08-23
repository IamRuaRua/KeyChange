using System;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Terraria.Localization;
using Rests;
using Newtonsoft.Json.Linq;

namespace ChangeKey
{
    [ApiVersion(2, 1)]
    public class KeyChange : TerrariaPlugin
    {
        Config configFile;
        private int keyID;
        private int giveItemID;

        #region Plugin Info
        public override string Author => "Rua";
        public override string Description => "钥匙交换物品";
        public override string Name => "KeyChange";
        public override Version Version => new Version(1,0,0,1);
        public KeyChange(Main game) : base(game) { }
        #endregion

        public Action<string> CommanAction { get; set; }
        public readonly Random random = new Random();

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(permissions: "ChangeKey", cmd: this.ChangeKey, "keychange", "kc"));
            Config.EnsureFile();//读取配置文件
            configFile = Config.ReadConfig();
        }
        void OnServerCommand(CommandEventArgs args)
        {
            Console.WriteLine(args.Command);
        }

       
        void ChangeKey(CommandArgs args)
        {
            var player = args.Player;
            if (player == null) return;
            if (!Main.ServerSideCharacter)
            {
                player.SendWarningMessage("SSC未开启,此插件无效");
                return;
            }
            if (args.Parameters.Count < 1)
            {
                args.Player.SendInfoMessage("缺少参数,你可以输入/kc help获取帮助");
                return;
            }
            string arg2 = args.Parameters[0];
            if (args.Parameters.Count > 1)
            {
                arg2 = string.Join(" ", args.Parameters);
            }
            Console.WriteLine(arg2);

            switch (arg2)
            {
                case "help":
                    player.SendInfoMessage("/kc list        列出所有可交换的钥匙" +
                        "\n/kc 数字       可交换指定钥匙" +
                        "\n/kc help        获取帮助");
                    return; 
                case "list":
                    player.SendInfoMessage("/kc 数字可交换钥匙");
                    player.SendInfoMessage("腐化钥匙(1)\n血腥钥匙(2)\n沙漠钥匙(3)\n丛林钥匙(4)\n神圣钥匙(5)\n冰霜钥匙(6)\n金钥匙(7)\n暗影钥匙(8)");
                    return; 
                case "1":
                case "腐化钥匙":
                    keyID = 1534;
                    giveItemID = 1571;
                    break;
                case "2":
                case "血腥钥匙":
                    keyID = 1535;
                    giveItemID = 1569;
                    break;
                case "3":
                case "沙漠钥匙":
                    keyID = 4714;
                    giveItemID = 4607;
                    break;
                case "4":
                case "丛林钥匙":
                    keyID = 1533;
                    giveItemID = 1156;
                    break;
                case "5":
                case "神圣钥匙":
                    keyID = 1536;
                    giveItemID = 1260;
                    break;
                case "6":
                case "冰霜钥匙":
                    keyID = 1537;
                    giveItemID = 1572;
                    break;
                case "7":
                case "金钥匙":
                    keyID = 327;
                    giveItemID = getItemIDFromKeyID(keyID);

                    break;
                case "8":
                case "暗影钥匙":
                    keyID = 329;
                    giveItemID = getItemIDFromKeyID(keyID);
                    break;
                default:
                    player.SendInfoMessage("无此参数,输入/kc help获取帮助");
                    return; 

            }

            if (!CheckCanChange(player, arg2))
            {
                return;
            }
            Change(keyID, player, giveItemID);
        }
        int getItemIDFromKeyID(int keyID)
        {
            int[] gold = { 113, 155, 156, Main.remixWorld?2623:157, 163, 164, 329, 3317 };//金钥匙=327
            int[] shadow = { 274, 218, Main.remixWorld?683:112, 220, 3019, 5010, 5010 };//暗影钥匙=329
            if (keyID == 327)
            {
                return gold[random.Next(0, gold.Length)];
            }
            else if (keyID == 329)
            {
                return shadow[random.Next(0, shadow.Length)];
            }

            int itemID = 1;
            return itemID;
        }


        void Change(int keyID, TSPlayer player, int giveItemID)
        {
            Item item;
            Item keyItem = TShock.Utils.GetItemById(keyID);
            string keyName = keyItem.Name;
            for (int i = 0; i < 50; i++)
            {
                item = player.TPlayer.inventory[i];
                // Loops through the player's inventory
                if (item.netID == keyID)
                {
                    // Found the item, checking for available slots
                    if (!player.InventorySlotAvailable)
                    {
                        player.SendErrorMessage("背包已满,无法交换!");
                        return;
                    }
                    if (item.stack >= 1)
                    {
                        player.TPlayer.inventory[i].stack--;
                        UpdatePlayerSlot(player, keyItem, i);
                        Item gotItem = TShock.Utils.GetItemById(giveItemID);
                        player.GiveItem(giveItemID, 1, 0);
                        player.SendSuccessMessage("使用" + keyName + "交换" + gotItem.Name + "成功!");
                        return;
                    }
                }
                // return;
            }
            player.SendErrorMessage("你没有" + keyName + ",无法交换!");

        }
        public static void UpdatePlayerSlot(TSPlayer player, Item item, int slotIndex)
        {
            NetMessage.SendData((int)PacketTypes.PlayerSlot, -1, -1, NetworkText.FromLiteral(item.Name), player.Index, slotIndex, (float)item.prefix);
            NetMessage.SendData((int)PacketTypes.PlayerSlot, player.Index, -1, NetworkText.FromLiteral(item.Name), player.Index, slotIndex, (float)item.prefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="keyNumID"></param>
        /// <returns></returns>
        bool CheckCanChange(TSPlayer player, string keyNumID)
        {
            if (player.IfSendErrorMessage("需要击败 骷髅王 才可兑换", configFile.是否需要击败骷髅王, NPC.downedBoss3, keyNumID, "7", "8") is bool a1)
            {
                return a1;
            }
            if (player.IfSendErrorMessage("需要击败 新三王 才可兑换", configFile.是否需要击败新三王, NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3, keyNumID, "1", "2", "3", "4", "5", "6") is bool a2)
            {
                return a2;
            }
            if (player.IfSendErrorMessage("需要击败 世纪之花 才可兑换", configFile.是否需要击败世纪之花, NPC.downedPlantBoss, keyNumID, "1", "2", "3", "4", "5", "6") is bool a3)
            {
                return a3;
            }
            return true;

        }

        public static void ExecuteRawCmd(TSPlayer player, string rawCmd)
        {
            if (string.IsNullOrEmpty(rawCmd))
            {
                player.SendInfoMessage("指令内容为空");
                return;
            }

            player.tempGroup = new SuperAdminGroup();
            Commands.HandleCommand(player, rawCmd);
            player.tempGroup = null;
        }
        void RunCMDOnTshock(string command)
        {
            CommandEventArgs args = new CommandEventArgs();
            var prop = args.GetType().GetProperty("Command");
            prop.SetValue(args, command);
            ServerApi.Hooks.ServerCommand.Invoke(args);
        }


    }
}
