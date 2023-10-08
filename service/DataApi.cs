using AimRobotLite.common;
using AimRobotLite.Properties;
using log4net;
using System.Text.Json;

namespace AimRobotLite.service {
    class DataApi {

        public delegate void DataCallback(object param);

        private static readonly ILog log = LogManager.GetLogger(typeof(DataApi));

        public static async void GetPlayerStatByPlayerId(long pid, DataCallback callback, Action errorCallback) {
            string path = "bfv/stats";
            string data = await RequestUtils.Get(
                Resources.DataApiFromGametool + path,
                new Dictionary<string, object> {
                    {"playerid", pid }
                });

            if (!string.Equals(string.Empty, data)) {
                callback(JsonDocument.Parse(data));
            } else {
                log.Error($"request error on bfv/stats {pid}");
                errorCallback();
            }
        }

        public static async void GetPlayerStatByName(string name, DataCallback callback, Action errorCallback) {
            string path = "bfv/stats";
            string data = await RequestUtils.Get(
                Resources.DataApiFromGametool + path,
                new Dictionary<string, object> {
                    {"name", name }
                });

            if (!string.Equals(string.Empty, data)) {
                callback(JsonDocument.Parse(data));
            } else {
                log.Error($"request error on bfv/stats param {name}");
                errorCallback();
            }
        }

        public static async void GetDetailServer(long gameId, DataCallback callback) {
            string path = "bfv/detailedserver";
            string data = await RequestUtils.Get(
                Resources.DataApiFromGametool + path,
                new Dictionary<string, object> {
                    {"gameid", gameId }
                });

            if (!string.Equals(string.Empty, data)) {
                callback(JsonDocument.Parse(data));
            } else {
                log.Error($"request error on bfv/detailedserver {gameId}");
            }
        }

        public static async void GetBfbanStatusByPlayerIds(ICollection<long> ids, DataCallback callback, Action errorCallback) {
            string path = "bfban/checkban";
            string data = await RequestUtils.Get(
                $"{Resources.DataApiFromGametool}{path}?personaids={string.Join("%2C", ids)}",
                new Dictionary<string, object> { });

            if (!string.Equals(string.Empty, data)) {
                callback(JsonDocument.Parse(data));
            } else {
                log.Error($"request error on bfban/checkban");
                errorCallback();
            }
        }

        public static async void GetGameRoomData(long gameId, DataCallback callback) {
            log.Debug("GetGameRoomData- Start");
            GameRoom room = new GameRoom();

            string dataGT = await RequestUtils.Get(
                Resources.DataApiFromGametool + "bfv/players",
                new Dictionary<string, object> {
                    {"gameid", gameId }
                });

            if (!string.Equals(string.Empty, dataGT)) {
                var dataJsonFromGT = JsonDocument.Parse(dataGT).RootElement;
                foreach (var teamElement in dataJsonFromGT.GetProperty("teams").EnumerateArray()) {
                    if (string.Equals(teamElement.GetProperty("teamid").GetString(), "teamOne")) {

                        foreach (var playerElement in teamElement.GetProperty("players").EnumerateArray()) {
                            room.TeamOne.Add(JsonSerializer.Deserialize<RoomPlayer>(playerElement));
                        }

                    } else if (string.Equals(teamElement.GetProperty("teamid").GetString(), "teamTwo")) {

                        foreach (var playerElement in teamElement.GetProperty("players").EnumerateArray()) {
                            room.TeamTwo.Add(JsonSerializer.Deserialize<RoomPlayer>(playerElement));
                        }

                    }
                }

                foreach (var loadingPlayer in dataJsonFromGT.GetProperty("loading").EnumerateArray()) {
                    room.InLoading.Add(JsonSerializer.Deserialize<RoomPlayer>(loadingPlayer));
                }

                foreach (var queuePlayer in dataJsonFromGT.GetProperty("que").EnumerateArray()) {
                    room.InQueue.Add(JsonSerializer.Deserialize<RoomPlayer>(queuePlayer));
                }

            } else {
                log.Error($"request error on bfv/players {gameId}");
            }

            log.Debug("GetGameRoomData- end to callback function\n" +
                $"InQueue: {room.InQueue.Count} InLoading: {room.InLoading.Count} PlayerCount: {room.TeamOne.Count + room.TeamTwo.Count}");

            callback(room);

        }

        public static async void GetNewestVersion(DataCallback callback) {
            string data = await RequestUtils.Get(
                "https://raw.githubusercontent.com/H4rry217/AimRobotLite/master/_resources/newestversion.txt",
                new Dictionary<string, object> {}
                );

            if (!string.Equals(string.Empty, data)) {
                callback(data);
            }
        }

    }
}
