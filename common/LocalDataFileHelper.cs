using AimRobot.Api.game;
using IniParser;
using IniParser.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AimRobotLite.common {
    class LocalDataFileHelper {

        private static IniData iniData;
        private static string FilePath = Path.Combine(Application.StartupPath, "localdata");
        private static FileIniDataParser Parser = new FileIniDataParser();

        private static object lockObject = new object();
        private static int writeCount = 0;

        static LocalDataFileHelper() {
            if (!File.Exists(FilePath)) File.Create(FilePath).Close();

            ReadData();
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

        private static void IncreaseWriteCount(Action writeAction) {
            lock (lockObject) {
                writeAction();
                writeCount++;

                if (writeCount >= 1) {
                    WriteData();
                    writeCount = 0;
                }
            }
        }

        public static void SetBfbanStat(long playerId, bool isHacker) {
            IncreaseWriteCount(() => {
                iniData["bfban"][playerId.ToString()] = $"{(isHacker ? "1" : "0")},{DateTimeOffset.Now.ToUnixTimeSeconds()}";
            });
        }

        public static void SetPlayerInfo(PlayerStatInfo player) {
            IncreaseWriteCount(() => {
                iniData["stat"][player.id.ToString()] = $"{player.userName},{DateTimeOffset.Now.ToUnixTimeSeconds()},{player.rank},{player.killsPerMinute},{player.kills}";
            });
        }

    }
}
