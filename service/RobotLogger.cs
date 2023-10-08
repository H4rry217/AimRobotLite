using AimRobot.Api;
using log4net;

namespace AimRobotLite.service {
    public class RobotLogger : IRobotLogger {

        private ILog log;

        public RobotLogger(ILog logger) {
            log = logger;
        }

        public void Debug(string s) {
            log.Debug(s);
        }

        public void Error(string s) {
            log.Error(s);
        }

        public void Fatal(string s) {
            log.Fatal(s);
        }

        public void Info(string s) {
            log.Info(s);
        }

        public void Warn(string s) {
            log.Warn(s);
        }
    }
}
