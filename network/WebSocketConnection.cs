using log4net;
using WebSocketSharp;
using AimRobotLite.network.packet;
using AimRobot.Api;

namespace AimRobotLite.network {
    public class WebSocketConnection {

        private WebSocket WebSocketClient;

        private ILog log = LogManager.GetLogger(typeof(WebSocketConnection));

        public void TryConnect() {
            string url = $"{Program.Winform.textBox9.Text}/ws/robot";

            log.Info($"try connect remote server {url}");

            try {
                WebSocketClient = new WebSocket($"{url}?serverId={Program.Winform.textBox10.Text}&token={Program.Winform.textBox11.Text}");
                WebSocketClient.OnOpen += WebSocketOnOpen;
                WebSocketClient.OnMessage += WebSocketOnBinary;
                WebSocketClient.OnError += WebSocketOnError;
                WebSocketClient.OnClose += WebSocketOnClose;

                log.Debug($"start connect websocket server...");
                WebSocketClient.Connect();
            } catch (Exception ex) {

            }
        }

        public void WebSocketOnOpen(object sender, EventArgs e) {
            log.Info("websocket server has connected");
        }

        public void WebSocketOnError(object sender, EventArgs e) {
            log.Error(sender);
            log.Error(((WebSocketSharp.ErrorEventArgs)e).Exception);
        }

        public void WebSocketOnBinary(object sender, MessageEventArgs e) {
            if (e.IsBinary) {
                byte[] rawData = e.RawData;
                DataPacket dataPacket = Protocol.GetPacket(rawData[0]);
                dataPacket.Put(rawData);

                dataPacket.Decode();
                handlePacket(dataPacket);

            }
        }

        private void WebSocketOnClose(object sender, EventArgs e) {
            log.Error("websocket server connection has closed");
        }

        public bool IsConnectionAlive() {
            return WebSocketClient != null && WebSocketClient.IsAlive;
        }

        public void Close() {
            if(WebSocketClient != null) WebSocketClient.Close();
        }

        public void SendRemote(DataPacket pk) {
            try {
                if (IsConnectionAlive()) {
                    pk.Encode();
                    WebSocketClient.Send(pk.GetBuffer());
                }
            } catch(Exception ex) {
                log.Error(ex);
            }
        }

        private static void handlePacket(DataPacket packet) {
            switch (packet.GetPacketId()) {
                case Protocol.PACKET_SEND_CHAT:
                    Robot.GetInstance().SendChat(((SendChatPacket)packet).message);
                    break;
                case Protocol.PACKET_BAN:
                    Robot.GetInstance().BanPlayer(((BanPlayerPacket)packet).playerId);
                    break;
                case Protocol.PACKET_BAN_BY_NAME:
                    var reason1 = ((BanPlayerByNamePacket)packet).reason;
                    Robot.GetInstance().BanPlayer(((BanPlayerByNamePacket)packet).playerName, reason1);
                    break;
                case Protocol.PACKET_COMMAND:
                    var cmd = ((CommandPacket)packet).command;
                    Robot.GetInstance().GetPluginManager().CheckCommand(null, cmd);
                    break;
            }
        }

    }
}
