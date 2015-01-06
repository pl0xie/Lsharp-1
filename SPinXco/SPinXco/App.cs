using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Platform.Summoner;

namespace SPinXco
{
    class App
    {
        private static PVPNetConnection pvp;
        
        public void AppTest()
        {
            pvp = new PVPNetConnection();
            pvp.Connect("tryme123xxx1", "13XXXX6xx", Region.NA, "4.21.14");

            pvp.OnLogin += pvp_OnLogin;
            Console.ReadLine();

        }

        private async static void pvp_OnLogin(object sender, string username, string ipAddress)
        {
            PublicSummoner summoner = await pvp.GetSummonerByName("KING TRICK");
            Console.WriteLine(summoner.SummonerId);
        }
        

        
    }
}
