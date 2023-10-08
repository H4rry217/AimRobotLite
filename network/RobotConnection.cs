using AimRobot.Api;
using AimRobot.Api.events.ev;
using AimRobotLite.service;
using log4net;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AimRobotLite.network{
    public class RobotConnection {

		private ILog log = LogManager.GetLogger(typeof(RobotConnection));

		private EndPoint RemoteEndPoint = new IPEndPoint(IPAddress.Loopback, 12307);
		private Socket OutSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
		private Socket InSocket;
		private bool Closed = true;
		private Thread SocketThread;

		private bool ConnectStatus = false;

        public RobotConnection() {
			SocketThread = new Thread(new ThreadStart(ReceiveData));
		}

		public void StartListen() {
			if (Closed) {
                Closed = false;
                SocketThread.Start();
            }
		}

		public void Close() {
            Closed = true;
			OutSocket.Shutdown(SocketShutdown.Both);
            OutSocket.Close();

            InSocket.Shutdown(SocketShutdown.Both);
            InSocket.Close();
		}

		public long SendPacket(DataPacket dataPacket) {
			dataPacket.Encode();
			return BitConverter.ToInt64(Send(dataPacket.GetBuffer()), 8);
		}

		private byte[] Send(byte[] data) {
			OutSocket.SendTo(data, RemoteEndPoint);

			var receiveBuffer = new byte[256];

			int size = OutSocket.ReceiveFrom(receiveBuffer, ref RemoteEndPoint);
			byte[] byteData = new byte[size];

			Array.Copy(receiveBuffer, byteData, size);
			return byteData;
		}

		private void ReceiveData() {
			InSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);
			InSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
			InSocket.Bind(new IPEndPoint(IPAddress.Loopback, 12306));

			InSocket.ReceiveBufferSize = 61440;
			InSocket.SendBufferSize = 61440;

			try {
				while (!Closed) {
					byte[] receive = new byte[InSocket.ReceiveBufferSize];
					EndPoint remoteEP = new IPEndPoint(0L, 0);

					int receiveSize = InSocket.ReceiveFrom(receive, ref remoteEP);
					byte[] buffer = new byte[receiveSize];

					Array.Copy(receive, 0, buffer, 0, receiveSize);

					byte[] eventTypeBytes = buffer.Take(32).ToArray();
					byte[] killerDataBytes = buffer.Skip(32).Take(32).ToArray();
					byte[] playerDataBytes = buffer.Skip(64).ToArray();

					string eventType = Encoding.ASCII.GetString(eventTypeBytes, 0, GetStringEndPos(eventTypeBytes));
					string firstPart = Encoding.ASCII.GetString(killerDataBytes, 0, GetStringEndPos(killerDataBytes));
					string secPart = Encoding.UTF8.GetString(playerDataBytes, 0, GetStringEndPos(playerDataBytes));

					if (eventType == "onKillLog") {
						string[] playerDatas = secPart.Split('#');
						string playerName = playerDatas[0];
						string killType = playerDatas[1];

						string killerPlatoons = string.Empty;
						string playerPlatoons = string.Empty;

						if (firstPart.StartsWith("[")) {
							killerPlatoons = firstPart.Substring(1, firstPart.IndexOf("]") - 1);
							firstPart = firstPart.Substring(firstPart.IndexOf("]") + 1);
						}

						if (playerName.StartsWith("[")) {
							playerPlatoons = playerName.Substring(1, playerName.IndexOf("]") - 1);
							playerName = playerName.Substring(playerName.IndexOf("]") + 1);
						}

						var platoon1 = string.Equals(killerPlatoons, string.Empty) ? "" : $"[{killerPlatoons}]";
						var platoon2 = string.Equals(playerPlatoons, string.Empty) ? "" : $"[{playerPlatoons}]";

						log.Info($"DeathEvent: {platoon1}{firstPart} ->{killType}-> {platoon2}{playerName}");

						PlayerDeathEvent playerDeathEvent = new PlayerDeathEvent();
						playerDeathEvent.killerPlatoon = killerPlatoons;
						playerDeathEvent.killerName = firstPart;
						playerDeathEvent.killerBy = killType;
						playerDeathEvent.playerPlatoon = playerPlatoons;
						playerDeathEvent.playerName = playerName;

						Robot.GetInstance().GetPluginManager().CallEvent(playerDeathEvent);

					} else if (eventType == "onChatMessage") {
						log.Info($"ChatEvent: {firstPart} => {secPart}");

						PlayerChatEvent playerChatEvent = new PlayerChatEvent();
						playerChatEvent.speaker = firstPart;
						playerChatEvent.message = secPart;

                        Robot.GetInstance().GetPluginManager().CallEvent(playerChatEvent);

                        //dirty work
                        if (((DataContext)Robot.GetInstance().GetGameContext())._CurrentPlayerProcDelegate != null) {
                            ((DataContext)Robot.GetInstance().GetGameContext())._CurrentPlayerProcDelegate(playerChatEvent.message, playerChatEvent.speaker);
						}

                    }

					ConnectStatus = true;
				}

			} catch (SocketException ex) {
				ConnectStatus = false;

				//RETRY CONNECTION TODO
			}

		}

		public bool GetConnectionStatus() {
			return ConnectStatus;
		}

		private int GetStringEndPos(byte[] b) {
			for (int i = 0; i < b.Length; i++) {
				if (b[i] == 0) return i;
            }
			return -1;
		}


	}
}
