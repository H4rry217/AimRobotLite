using AimRobot.Api;
using AimRobot.Api.events;
using AimRobot.Api.events.ev;
using AimRobot.Api.game;
using EventHandler = AimRobot.Api.events.EventHandler;

namespace AimRobotLite.service.robotplugin
{
    public class AimRobotDefaultListener : IEventListener{

        public static Dictionary<string, int> KILL_STATISTIC = new Dictionary<string, int>();
        public static Dictionary<string, long> PLAYER_FIRST_KILL_STATISTIC = new Dictionary<string, long>();
        public static Dictionary<string, int> ROCKET_KILL_STATISTIC = new Dictionary<string, int>();
        public static long LAST_STATISTIC_UPDATE_TIMESTAMP = 0;

        public static Dictionary<string, int> PLAYER_KillSTREAK = new Dictionary<string, int>();

        [EventHandler]
        public void KillStatistic(PlayerDeathEvent playerEvent) {

            /*********************************************/

            Robot.GetInstance().GetGameContext().PlayerCheck(playerEvent.killerName);
            Robot.GetInstance().GetGameContext().PlayerCheck(playerEvent.playerName);

            /*********************************************/

            long curTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (curTime - LAST_STATISTIC_UPDATE_TIMESTAMP > 1000 * 50) {
                KILL_STATISTIC.Clear();
                ROCKET_KILL_STATISTIC.Clear();
                PLAYER_FIRST_KILL_STATISTIC.Clear();
                PLAYER_KillSTREAK.Clear();

                //dirty work
                ((DataContext)Robot.GetInstance().GetGameContext()).ClearCacheData();
                Program.Winform.textBox5.Text = "";
                Program.Winform.textBox6.Text = "";
                Program.Winform.richTextBox1.Text = "";

                AimRobotDefaultPlugin.instance.GetLogger().Debug("Clean kill statistic dict");
            }

            if (!PLAYER_FIRST_KILL_STATISTIC.ContainsKey(playerEvent.killerName)) {
                PLAYER_FIRST_KILL_STATISTIC[playerEvent.killerName] = curTime;
            }

            if (playerEvent.killerBy != GameConst.KILL_BY_SUICIDE) {
                KILL_STATISTIC[playerEvent.killerName] = (KILL_STATISTIC.TryGetValue(playerEvent.killerName, out int value1) ? value1 : 0) + 1;


                if (GameConst.RocketCodes.Contains(playerEvent.killerBy)) {
                    ROCKET_KILL_STATISTIC[playerEvent.killerName] = (ROCKET_KILL_STATISTIC.TryGetValue(playerEvent.killerName, out int value2) ? value2 : 0) + 1;
                }
            }

            KILL_STATISTIC[playerEvent.playerName] = 0;

            LAST_STATISTIC_UPDATE_TIMESTAMP = curTime;

        }

        [EventHandler]
        public void weaponBan(PlayerDeathEvent playerEvent) {
            if (Program.Winform.checkBox1.Checked && playerEvent.killerBy.Equals("Nambu_Type2A")) {
                if (Robot.GetInstance().GetGameContext().GetCurrentPlayerName().Equals(playerEvent.killerName)) {
                    AimRobotDefaultPlugin.instance.GetLogger().Warn("arl exit reason: curret player using type2a");
                    Application.Exit();
                    return;
                } else {
                    Robot.GetInstance().SendChat(
                        $"[{playerEvent.killerName}] will get banned by robot, reason: forbidden weapon Type 2A\n" +
                        $"[{playerEvent.killerName}] 将会被踢出游戏，原因：使用禁止的武器（二式冲锋枪）"
                        );
                    Robot.GetInstance().KickPlayer(playerEvent.killerName);
                }
            }
        }

        /***************************************************/

        private static Dictionary<string, long> PLAYER_CHAT_TIMESTAMP = new Dictionary<string, long>();
        private static Dictionary<string, int> PLAYER_CHAT_COUNT = new Dictionary<string, int>();

        [EventHandler]
        public void OnAntiFloodMessage(PlayerChatEvent chatEvent) {
            long curTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (Program.Winform.checkBox4.Checked) {
                if (!string.Equals(chatEvent.speaker, Robot.GetInstance().GetGameContext().GetCurrentPlayerName())) {

                    if (PLAYER_CHAT_TIMESTAMP.TryGetValue(chatEvent.speaker, out long lastChat)) {
                        //如果发言间隔小于1秒
                        if (curTime - lastChat < 1000) {
                            if (PLAYER_CHAT_COUNT.TryGetValue(chatEvent.speaker, out int chatCount)) {
                                int newChatCount = chatCount + 1;
                                PLAYER_CHAT_COUNT[chatEvent.speaker] = newChatCount;


                                if (newChatCount >= 5) {
                                    PLAYER_CHAT_COUNT[chatEvent.speaker] = 0;
                                    Robot.GetInstance().BanPlayer(chatEvent.speaker);
                                    Robot.GetInstance().SendChat($"\n\n\nPlayer [{chatEvent.speaker}] is flooding the chat box and will get banned. \n玩家 [{chatEvent.speaker}] 正在刷屏且将会被服务器屏蔽。");
                                    Robot.GetInstance().SendChat($"\n\n\nPlayer [{chatEvent.speaker}] is flooding the chat box and will get banned. \n玩家 [{chatEvent.speaker}] 正在刷屏且将会被服务器屏蔽。");
                                }
                            }

                        } else if (curTime - lastChat > 5000) {
                            PLAYER_CHAT_COUNT[chatEvent.speaker] = 1;
                        }


                    } else {
                        PLAYER_CHAT_COUNT[chatEvent.speaker] = 1;
                    }

                    PLAYER_CHAT_TIMESTAMP[chatEvent.speaker] = curTime;
                }
            }
        }

    }
}
