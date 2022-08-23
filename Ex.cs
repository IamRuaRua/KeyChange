using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace ChangeKey
{
    public static class Ex
    {
        //如果返回null  则继续往下判断 否则需要进行返回
        //
        public static bool? IfSendErrorMessage(this TSPlayer player,string message,bool fileConfigIf,bool NPCIf,string id,params string[] ids)
        {
            if (ids.Contains(id))
            {
                if (fileConfigIf)
                {
                    if (NPCIf)
                    {
                        return true;
                    }
                    else
                    {
                        player.SendErrorMessage(message);
                        return false;
                    }
                }
            }
            return null;
             
        }

    }
}
