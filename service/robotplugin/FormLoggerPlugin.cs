using AimRobot.Api;
using AimRobot.Api.events;
using AimRobot.Api.events.ev;
using AimRobot.Api.plugin;
using EventHandler = AimRobot.Api.events.EventHandler;

namespace AimRobotLite.service.robotplugin {
    public class FormLoggerPlugin : PluginBase, IEventListener {

        public override string GetAuthor() {
            return "H4rry217";
        }

        public override string GetDescription() {
            return "AimRobotLite自带插件，禁用后将无法在程序中直接在“游戏日志”选项卡中查看游戏日志";
        }

        public override string GetPluginName() {
            return "AimRobotLogger";
        }

        public override Version GetVersion() {
            return new Version(1, 14, 514);
        }

        public override void OnDisable() {

        }

        public override void OnEnable() {

        }

        public override void OnLoad() {
            Robot.GetInstance().GetPluginManager().RegisterListener(this, this);
        }

        [EventHandler]
        public void OnChat(PlayerChatEvent playerEvent) {
            Program.Winform.ChatLogTextBoxAppend($"{playerEvent.speaker}: {playerEvent.message}");
        }

        [EventHandler]
        public void onDeath(PlayerDeathEvent playerEvent) {
            Program.Winform.KillLogTextBoxAppend($"{playerEvent.killerName} => {playerEvent.killerBy} => {playerEvent.playerName}");
        }
    }
}
