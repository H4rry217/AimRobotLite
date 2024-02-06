using AimRobot.Api;
using AimRobot.Api.plugin;
using AimRobotLite.network;
using AimRobotLite.network.opacket;
using AimRobotLite.network.opakcet;
using AimRobotLite.plugin;
using AimRobotLite.service;
using AimRobotLite.service.automanage;
using log4net;

namespace AimRobotLite{
    public class AimRobotLite : Robot {

        private RobotLogger logger;

        private RobotConnection RobotConnection;

        private WebSocketConnection WebsocketConnection;

        private PluginManager PluginManager;

        private IGameContext GameContext;

        private string CurrentDir;

        private GameWindow GameWindow;

        public static void run() {
            Init(new AimRobotLite());
            ((RobotPluginManager)GetInstance().GetPluginManager()).LoadPlugins();
        }

        public AimRobotLite(){
            ILog log = LogManager.GetLogger(typeof(AimRobotLite));
            logger = new RobotLogger(log);

            RobotConnection = new RobotConnection();
            RobotConnection.StartListen();

            WebsocketConnection = new WebSocketConnection();

            GameContext = new DataContext(RobotConnection);
            GameWindow = new GameWindow(int.Parse(Program.Winform.textBox14.Text), int.Parse(Program.Winform.textBox15.Text));

            CurrentDir = Directory.GetCurrentDirectory();
            string pluginsDirectory = Path.Combine(CurrentDir, "plugins");
            if (!Directory.Exists(pluginsDirectory)) Directory.CreateDirectory(pluginsDirectory);

            string configssDirectory = Path.Combine(CurrentDir, "configs");
            if (!Directory.Exists(configssDirectory)) Directory.CreateDirectory(configssDirectory);

            PluginManager = new RobotPluginManager(this);
        }

        public override string GetDirectory() {
            return CurrentDir;
        }

        public override IRobotLogger GetLogger(){
            return logger;
        }

        public override PluginManager GetPluginManager(){
            return PluginManager;
        }

        public override IGameContext GetGameContext(){
            return GameContext;
        }

        public RobotConnection GetRobotConnection(){
            return RobotConnection;
        }

        public WebSocketConnection GetWebSocketConnection() {
            return WebsocketConnection;
        }

        public GameWindow GetWindow() {
            return GameWindow;
        }

        public void TryConnectRemoteServer() {
            if (WebsocketConnection.IsConnectionAlive()) WebsocketConnection.Close();

            WebsocketConnection.TryConnect();
        }

        public override void SendChat(string message){
            SendChatPacket packet = new SendChatPacket();
            packet.message = message;
            RobotConnection.SendPacket(packet);
        }

        public override void BanPlayer(long playerId){
            BanPlayerPacket packet = new BanPlayerPacket();
            packet.playerId = playerId;
            RobotConnection.SendPacket(packet);
        }

        public override void BanPlayer(string name) {
            var playerId = GameContext.GetPlayerId(name);
            if (playerId != 0) BanPlayer(playerId);
        }

        public override void BanPlayer(string name, string reason){
            var playerId = GameContext.GetPlayerId(name);
            if (playerId != 0){

                if (reason != null && !string.Empty.Equals(reason)){
                    SendChat(
                       $"[{name}] will get banned by robot, reason: {reason}\n" +
                       $"[{name}] 将会被屏蔽出游戏，原因：{reason}"
                       );
                }

                BanPlayer(playerId);

                network.packet.BanPlayerByNamePacket banLogPacket = new network.packet.BanPlayerByNamePacket();
                banLogPacket.playerName = name;
                banLogPacket.reason = reason;

                GetWebSocketConnection().SendRemote(banLogPacket);
            }
            
        }

        public override void UnBanPlayer(long playerId) {
            UnBanPlayerPacket packet = new UnBanPlayerPacket();
            packet.playerId = playerId;
            RobotConnection.SendPacket(packet);
        }

        public override void UnBanPlayer(string name) {
            GetGameContext().GetPlayerStatInfo(name, (stat) => {
                if (stat.id != 0) UnBanPlayer(stat.id);
            });
        }

        public override void KickPlayer(long playerId){
            BanPlayer(playerId);
            UnBanPlayer(playerId);
        }

        public override void KickPlayer(string name) {
            var playerId = GameContext.GetPlayerId(name);
            if (playerId != 0) KickPlayer(playerId);
        }

        public override void KickPlayer(string name, string reason){
            var playerId = GameContext.GetPlayerId(name);
            if (playerId != 0){

                if (reason != null && !string.Empty.Equals(reason)){
                    SendChat(
                       $"[{name}] will get banned by robot, reason: {reason}\n" +
                       $"[{name}] 将会被踢出游戏，原因：{reason}"
                       );
                }

                KickPlayer(playerId);
            }
        }

        public override bool IsEnable() {
            return GameContext.GetCurrentPlayerName() != null && GameContext.GetCurrentPlayerName().Length > 0;
        }

        public override void JoinGame(long gameId) {
            JoinGamePacket packet = new JoinGamePacket();
            packet.gameId = gameId;
            RobotConnection.SendPacket(packet);
        }

        public override IConnection GetConnection() {
            return this.WebsocketConnection;
        }
    }
}
