using AimRobot.Api;
using AimRobot.Api.config;
using AimRobot.Api.plugin;
using AimRobotLite.service.robotplugin.command;

namespace AimRobotLite.service.robotplugin {
    public class AimRobotDefaultPlugin : PluginBase {

        public AutoSaveConfig Config { get; set; }

        public static PluginBase instance = null;

        public override string GetAuthor() {
            return "H4rry217";
        }

        public override string GetDescription() {
            return "AimRobotLite自带插件，禁用后将无法使用ARL程序自带的基础功能";
        }

        public override string GetPluginName() {
            return "AimRobotDefault";
        }

        public override Version GetVersion() {
            return new Version(1, 14, 514);
        }

        public override void OnDisable() {
            
        }

        public override void OnEnable() {
            
        }

        public override void OnLoad() {
            AutoSaveConfig config = new DefaultPluginAutoSaveConfig(this, "BfvRobot");
            Robot.GetInstance().GetPluginManager().ConfigAutoSave(config);

            this.Config = config;

            instance = this;
            Robot.GetInstance().GetPluginManager().RegisterCommandListener(this, new StatCommand());
            Robot.GetInstance().GetPluginManager().RegisterCommandListener(this, new ContextCommand());
            Robot.GetInstance().GetPluginManager().RegisterCommandListener(this, new UnBanCommand());
            Robot.GetInstance().GetPluginManager().RegisterListener(this, new AimRobotDefaultListener());
        }
    }
}
