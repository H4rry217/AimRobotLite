using AimRobot.Api.events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.events {
    public class MethodList {

        private List<EventMethod> eventMethods = new List<EventMethod>();

        public void InvokeAll(RobotEvent robotEvent) {
            foreach (var item in eventMethods) {
                if (item.GetPluginBase().IsEnable()) {
                    item.Invoke(robotEvent);
                }
            }
        }

        public void RegisterMethod(EventMethod eventMethod) {
            eventMethods.Add(eventMethod);
        }

        public void UnregisterMethod(IEventListener eventListener) {
            eventMethods.RemoveAll(item => item.GetListener() == eventListener);
        }

    }
}
