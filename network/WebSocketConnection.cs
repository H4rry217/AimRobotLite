using log4net;
using WebSocketSharp;
using AimRobotLite.network.packet;
using AimRobot.Api;

namespace AimRobotLite.network {
    public class WebSocketConnection : IConnection {

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
            pk.Encode();
            SendBuffer(pk.GetBuffer());
        }

        public void SendBuffer(byte[] buffer) {
            try {
                if (IsConnectionAlive()) {
                    WebSocketClient.Send(buffer);
                }
            } catch (Exception ex) {
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
                case Protocol.PACKET_SCREENSHOT:
                    new Thread(() => {
                        ((AimRobotLite)Robot.GetInstance()).GetWindow().GetBfvHandle();

                        ScreenshotPacket pk = (ScreenshotPacket)packet;
                        ScreenshotPacket pak = new ScreenshotPacket();
                        pak.timestamp = pk.timestamp;
                        pak.image = ((AimRobotLite)Robot.GetInstance()).GetWindow().GetScreenShot();

                        ((AimRobotLite)Robot.GetInstance()).GetWebSocketConnection().SendRemote(pak);
                    }).Start();

                    break;
                case Protocol.PACKET_RUNTASK:
                    if (Program.Winform.checkBox6.Checked) {
                        ((AimRobotLite)Robot.GetInstance()).GetWindow().RunTask(((RunTaskPacket)packet).task);
                    }
                    break;
            }
        }

    }
}
