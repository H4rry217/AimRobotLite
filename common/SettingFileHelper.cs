using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AimRobotLite.common {
    class SettingFileHelper {

        private static IniData iniData;
        private static string FilePath = Path.Combine(Application.StartupPath, "config.ini");
        private static FileIniDataParser Parser = new FileIniDataParser();

        static SettingFileHelper() {
            if (!File.Exists(FilePath)) File.Create(FilePath).Close();

            var data = ReadData();
            var setting = data["setting"];

            if (setting["broadcast.content1"] == null) setting["broadcast.content1"] = "自定义广播1";
            if (setting["broadcast.content2"] == null) setting["broadcast.content2"] = "自定义广播2";
            if (setting["broadcast.content3"] == null) setting["broadcast.content3"] = "自定义广播3";
            if (setting["broadcast.content4"] == null) setting["broadcast.content4"] = "";

            if (setting["broadcast.rocketkill"] == null) setting["broadcast.rocketkill"] = "True";

            if (setting["banplayer.type2a"] == null) setting["banplayer.type2a"] = "False";
            if (setting["banplayer.floodmsg"] == null) setting["banplayer.floodmsg"] = "True";
            if (setting["antiafkkick"] == null) setting["antiafkkick"] = "True";

            if (setting["remoteserver.wsurl"] == null) setting["remoteserver.wsurl"] = "ws://127.0.0.1";
            if (setting["remoteserver.serverid"] == null) setting["remoteserver.serverid"] = "serverid";
            if (setting["remoteserver.token"] == null) setting["remoteserver.token"] = "token";
            if (setting["remoteserver.autoconnect"] == null) setting["remoteserver.autoconnect"] = "False";

            if (setting["manage.enable"] == null) setting["manage.enable"] = "False";
            if (setting["manage.runpath"] == null) setting["manage.runpath"] = "steam://rungameid/1238810";
            if (setting["manage.ocrurl"] == null) setting["manage.ocrurl"] = "http://127.0.0.1:11452";
            if (setting["manage.screenw"] == null) setting["manage.screenw"] = "1500";
            if (setting["manage.screenh"] == null) setting["manage.screenh"] = "1000";

            WriteData();
        }

        public static void WriteData() {
            Parser.WriteFile(FilePath, iniData, Encoding.Default);
        }

        private static IniData ReadData() {
            iniData = Parser.ReadFile(FilePath, Encoding.Default);
            return GetData();
        }

        public static IniData GetData() {
            return iniData;
        }

    }
}
