using AimRobot.Api;
using AimRobot.Api.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.service.robotplugin.command {
    public class UnBanCommand : ICommandListener {

        public string GetCommandKeyword() {
            return "unban";
        }

        public void OnCommand(CommandData commandHandler) {
            if(commandHandler.GetSender() == null) {
                Robot.GetInstance().UnBanPlayer(long.Parse(commandHandler.GetValue<string>("id")));
                Robot.GetInstance().GetLogger().Info($"Unban player by playerid {long.Parse(commandHandler.GetValue<string>("id"))}");
            }
        }

    }
}
