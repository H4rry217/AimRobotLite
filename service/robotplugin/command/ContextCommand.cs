using AimRobot.Api;
using AimRobot.Api.command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.service.robotplugin.command {
    public class ContextCommand : ICommandListener {

        public string GetCommandKeyword() {
            return "context";
        }

        public void OnCommand(CommandData commandHandler) {
            if(commandHandler.GetSender() == null) {
                if (Robot.GetInstance().GetGameContext().IsEnable()) {
                    Robot.GetInstance().GetGameContext().SetEnable(false);
                } else {
                    Robot.GetInstance().GetGameContext().SetEnable(true);
                }
            }
        }

    }
}
