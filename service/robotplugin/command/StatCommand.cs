using AimRobot.Api;
using AimRobot.Api.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.service.robotplugin.command {
    public class StatCommand : ICommandListener {

        public string GetCommandKeyword() {
            return "stat";
        }

        public void OnCommand(CommandData commandHandler) {
            string sender = commandHandler.GetSender() == null? Robot.GetInstance().GetGameContext().GetCurrentPlayerName(): commandHandler.GetSender();

            long startTime = AimRobotDefaultListener.PLAYER_FIRST_KILL_STATISTIC.TryGetValue(sender, out long value) ? value : 0;
            float kpm;

            if (startTime == 0) {
                kpm = 0;
            } else {
                kpm = ((float)(AimRobotDefaultListener.KILL_STATISTIC.TryGetValue(sender, out int killCount1) ? killCount1 : 0f))
                    / ((float)((DateTimeOffset.Now.ToUnixTimeMilliseconds() - (AimRobotDefaultListener.PLAYER_FIRST_KILL_STATISTIC.TryGetValue(commandHandler.GetSender(), out long time) ? time : 0)) / (1000 * 60)));
            }

            Robot.GetInstance().SendChat(
                $"[{sender}] " +
                $"KPM {kpm} " +
                $"KILL {(AimRobotDefaultListener.KILL_STATISTIC.TryGetValue(sender, out int killCount) ? killCount : 0)} " +
                $"KILLSTREAK {(AimRobotDefaultListener.PLAYER_KillSTREAK.TryGetValue(sender, out int killStreakCount) ? killStreakCount : 0)}");
        }

    }
}
