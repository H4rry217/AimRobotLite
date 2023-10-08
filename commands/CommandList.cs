using AimRobot.Api.command;
using AimRobot.Api.plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.commands {
    public class CommandList {

        private PluginBase plugin;
        private ISet<ICommandListener> commandListeners;

        public CommandList(PluginBase pluginBase) {
            this.plugin = pluginBase;
            this.commandListeners = new HashSet<ICommandListener>();
        }

        public void RegisterCommand(ICommandListener commandListener) {
            commandListeners.Add(commandListener);
        }

        public void UnregisterCommand(ICommandListener commandListener) {
            commandListeners.Remove(commandListener);
        }

        public void InvokeCommand(string keyword, CommandData commandData) {
            if (plugin.IsEnable()) {
                foreach (var item in commandListeners){
                    if (string.Equals(keyword, item.GetCommandKeyword())) {
                        item.OnCommand(commandData);
                    }
                }
            }
        }

    }
}
