using AimRobot.Api.events;
using AimRobot.Api.plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.events {
    public class EventMethod {

        private MethodInfo method;

        private IEventListener listener;

        private PluginBase plugin;

        public EventMethod(MethodInfo method, IEventListener listener, PluginBase pluginBase) {
            this.method = method;
            this.listener = listener;
            this.plugin = pluginBase;
        }

        public void Invoke(RobotEvent ev) {
            method.Invoke(listener, new object[] { ev });
        }

        public IEventListener GetListener() {
            return listener;
        }

        public PluginBase GetPluginBase() {
            return plugin;
        }

    }
}
