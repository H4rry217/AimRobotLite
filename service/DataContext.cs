using AimRobot.Api;
using AimRobot.Api.game;
using AimRobotLite.common;
using AimRobotLite.network;
using AimRobotLite.network.opacket;
using AimRobotLite.Properties;
using AimRobotLite.service.robotplugin;
using log4net;
using System.Net.Sockets;
using System.Text.Json;
using static AimRobot.Api.IGameContext;
using static AimRobotLite.service.BfbanAntiCheat;

namespace AimRobotLite.service {
    public class DataContext : IGameContext {

        private bool RunningState = true;

        private readonly ILog log = LogManager.GetLogger(typeof(DataContext));

        private Dictionary<long, bool> PlayerBfbanStatusDict = new Dictionary<long, bool>();

        //Gametool
        private Dictionary<long, PlayerStatInfo> PlayerInfoDict = new Dictionary<long, PlayerStatInfo>();
        private Dictionary<string, long> PlayerNameMapper = new Dictionary<string, long>();

        //Cache
        private Dictionary<string, long> PlayerCacheNameMapper = new Dictionary<string, long>();
        private Dictionary<long, string> PlayerCacheIdMapper = new Dictionary<long, string>();

        //queue
        private UniqueQueue<long> QueryStatByPidQueue = new UniqueQueue<long>();
        private UniqueQueue<string> QueryStatByNameQueue = new UniqueQueue<string>();
        private UniqueQueue<long> QueryBfbanQueue = new UniqueQueue<long>();

        //callbacks
        private Dictionary<string, StatCallBack> QueryStatByNameCallbacks = new Dictionary<string, StatCallBack>();
        private Dictionary<long, StatCallBack> QueryStatByPidCallbacks = new Dictionary<long, StatCallBack>();
        private Dictionary<long, BfbanCallback> QueryBfbanCallbacks = new Dictionary<long, BfbanCallback>();

        private UniqueQueue<long> PlayerWaitingCheckQueue = new UniqueQueue<long>();
        private ISet<long> PlayerCheckingSet = new HashSet<long>();

        private ISet<long> FairPlayers = new HashSet<long>();
        private ISet<long> AbnormalPlayers = new HashSet<long>();

        private ISet<IAntiCheat> AntiCheats = new HashSet<IAntiCheat>();

        private System.Threading.Timer _QueryTimer;
        private System.Threading.Timer _CheckTimer;
        private System.Threading.Timer _GameRoomTimer;
        private System.Threading.Timer _ContextTimer;
        private System.Threading.Timer _BroadcastTimer;

        /*****************************************************/
        private long CurrentGameId = 0;
        private string CurrentPlayerName = string.Empty;

        public delegate void _CurPlayerNameProc(string randomVal, string name);
        public _CurPlayerNameProc _CurrentPlayerProcDelegate;

        private RobotConnection RobotConnection;

        public DataContext(RobotConnection robotConnection) {
            RobotConnection = robotConnection;

            _ContextTimer = new System.Threading.Timer(_GameInterval, null, 0, 5 * 1000);
            _QueryTimer = new System.Threading.Timer(_QueryInterval, null, 0, 20 * 1000);
            _CheckTimer = new System.Threading.Timer(_CheckPlayers, null, 0, 15 * 1000);
            _GameRoomTimer = new System.Threading.Timer(_GameStatInterval, null, 0, 60 * 1000);
            _BroadcastTimer = new System.Threading.Timer(_Broadcast, null, 0, 45 * 1000);

            LoadLocalData();
            RegisterAntiCheat(new BfbanAntiCheat());
        }

        public void LoadLocalData() {
            var iniData = LocalDataFileHelper.GetData();

            long curTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            int count1 = 0, count2 = 0;

            foreach (var playData in iniData["bfban"]) {
                string[] vals = playData.Value.Split(',');
                if (vals.Length == 2) {
                    long dataCreateTime = long.Parse(vals[1]);
                    if (((curTime - dataCreateTime) / (float)86400) < 3) {
                        PlayerBfbanStatusDict[long.Parse(playData.KeyName)] = Convert.ToBoolean(int.Parse(vals[0]));
                        count1++;
                    }
                }
            }

            foreach (var playData in iniData["stat"]) {
                string[] vals = playData.Value.Split(',');
                if (vals.Length == 5) {
                    long dataCreateTime = long.Parse(vals[1]);
                    if (((curTime - dataCreateTime) / (float)86400) < 3) {
                        PlayerStatInfo player = new PlayerStatInfo();
                        player.id = long.Parse(playData.KeyName);
                        player.userName = vals[0];
                        player.rank = int.Parse(vals[2]);
                        player.killsPerMinute = double.Parse(vals[3]);
                        player.kills = int.Parse(vals[4]);

                        PlayerInfoDict[player.id] = player;
                        count2++;
                    }
                }
            }

            log.Info($"Loaded {count1} Bfban Data, {count2} player stat data from cache");
        }

        public long GetCurrentGameId() {
            return CurrentGameId;
        }

        public string GetCurrentPlayerName() {
            return CurrentPlayerName;
        }

        public long GetPlayerId(string name) {
            GetPlayerIdPacket packet = new GetPlayerIdPacket();
            packet.name = name;
            return RobotConnection.SendPacket(packet);
        }

        public void RegisterAntiCheat(IAntiCheat antiCheat) {
            AntiCheats.Add(antiCheat);
        }

        public void UnregisterAntiCheat(IAntiCheat antiCheat) {
            AntiCheats.Remove(antiCheat);
        }

        private void _GameInterval(object state) {
            try {
                CurrentGameId = _GetCurrentGameId();
                if (CurrentGameId != 0 && CurrentPlayerName.Length == 0 && Robot.GetInstance() != null) _ProcCurrentPlayerName();

                _RocketKilledBroadcast();
            } catch (SocketException ex) {

            }
        }

        private static int ROBOT_BROADCAST_STATE = 0;
        private static int ROBOT_CUSTOM_BROADCAST_STATE = 1;
        private void _Broadcast(object state) {
            if (RobotConnection.GetConnectionStatus()) {
                switch (ROBOT_BROADCAST_STATE) {
                    case 0:
                        //chn
                        Robot.GetInstance().SendChat(Resources.RobotBroadcast_text2);
                        break;
                    case 1:
                        //eng
                        Robot.GetInstance().SendChat(Resources.RobotBroadcast_text1);
                        break;
                    case 2:
                        Robot.GetInstance().SendChat(
                        $"当前机器人已检测了 {FairPlayers.Count} 名正常玩家并发现 {AbnormalPlayers.Count} 名异常数据玩家。\n" +
                            $"AIM ROBOT LITE has detected {FairPlayers.Count} fair players and find {AbnormalPlayers.Count} players with abnormal data");
                        break;
                    case 3:
                        //custom
                        var custom = SettingFileHelper.GetData()["setting"][$"broadcast.content{ROBOT_CUSTOM_BROADCAST_STATE}"];
                        if (custom.Length > 0) Robot.GetInstance().SendChat(SettingFileHelper.GetData()["setting"][$"broadcast.content{ROBOT_CUSTOM_BROADCAST_STATE}"]);

                        ROBOT_CUSTOM_BROADCAST_STATE++;
                        if (ROBOT_CUSTOM_BROADCAST_STATE > 4) {
                            ROBOT_CUSTOM_BROADCAST_STATE = 1;
                            ROBOT_BROADCAST_STATE = 0;
                        }

                        return;
                }

                ROBOT_BROADCAST_STATE++;
            }
        }

        private void _RocketKilledBroadcast() {
            if (Program.Winform.checkBox2.Checked) {
                foreach (string playerName in AimRobotDefaultListener.ROCKET_KILL_STATISTIC.Keys) {
                    int killedCount = AimRobotDefaultListener.ROCKET_KILL_STATISTIC[playerName];
                    log.Info("Rocket broadcast [playerName] [killedCount]");

                    if (killedCount > 10) {
                        Robot.GetInstance().SendChat($"王炸！！！还！有！谁！玩家 [{playerName}] 呼叫的火箭弹支援炸死了 {killedCount} 名敌人！！！");
                    } else if (killedCount > 5) {
                        Robot.GetInstance().SendChat($"好机会，这一波可以冲！玩家 [{playerName}] 呼叫了火箭弹支援，炸死了 {killedCount} 名敌人！");
                    } else if (killedCount > 1) {
                        Robot.GetInstance().SendChat($"[{playerName}] 召唤了火箭弹，炸死了 {killedCount} 名敌人");
                    } else {
                        Robot.GetInstance().SendChat($"好漂亮的烟花！玩家 [{playerName}] 呼叫了火箭弹支援，但只炸死一名敌人！");
                    }
                }
            }

            AimRobotDefaultListener.ROCKET_KILL_STATISTIC.Clear();
        }

        private void _ProcCurrentPlayerName() {
            var random = new Random().Next(100000, 999999 + 1);
            _CurrentPlayerProcDelegate = (randomVal, playerName) => {
                if (string.Equals(random.ToString(), randomVal)) {
                    CurrentPlayerName = playerName;
                    _CurrentPlayerProcDelegate = null;
                }
            };

            Robot.GetInstance().SendChat($"{random}");
            Robot.GetInstance().SendChat($"{random}");
        }

        private long _GetCurrentGameId() {
            GetGameIdPacket packet = new GetGameIdPacket();
            return RobotConnection.SendPacket(packet);
        }

        private void _CheckPlayers(object state) {

            log.Debug($"Check Queue -> WaitingCheck: {PlayerWaitingCheckQueue.Count} Checking: {PlayerCheckingSet.Count}");
            log.Debug($"FairPlayer : {FairPlayers.Count} AbnormalPlayers: {AbnormalPlayers.Count}");

            //检测等待结果队列中的玩家，是否结果已出
            //ISet<long> playerIds = new HashSet<long>();
            foreach (long checkingPlayerId in PlayerCheckingSet) {
                if (!FairPlayers.Contains(checkingPlayerId)) {

                    if (AbnormalPlayers.Contains(checkingPlayerId)) {
                        GetPlayerStatInfo(checkingPlayerId, (statData) => {
                            Robot.GetInstance().SendChat(
                              $"[{statData.userName}] will get banned by robot, reason: Abnormal Player\n" +
                              $"[{statData.userName}] 将会被屏蔽出游戏，原因：异常玩家"
                              );

                            Robot.GetInstance().BanPlayer(checkingPlayerId);
                        });
                    } else {
                        FairPlayers.Add(checkingPlayerId);

                        foreach (var antiCheat in AntiCheats) {
                            antiCheat.IsAbnormalPlayer(checkingPlayerId, (isHacker, chs, eng) => {
                                if (isHacker && !AbnormalPlayers.Contains(checkingPlayerId)) {
                                    FairPlayers.Remove(checkingPlayerId);
                                    AbnormalPlayers.Add(checkingPlayerId);

                                    GetPlayerStatInfo(checkingPlayerId, (statData) => {
                                        Robot.GetInstance().SendChat(
                                          $"[{statData.userName}] will get banned by robot, reason: {eng}\n" +
                                          $"[{statData.userName}] 将会被屏蔽出游戏，原因：{chs}"
                                          );

                                        Robot.GetInstance().BanPlayer(checkingPlayerId);
                                    });

                                }
                            });
                        }
                    }

                }
            }

            PlayerCheckingSet.Clear();

            //添加查询队列
            int checkPerLimit = 24;

            while (PlayerWaitingCheckQueue.Count > 0 && checkPerLimit > 0) {
                //如果还有检测余量
                long checkPlayerId = PlayerWaitingCheckQueue.Dequeue();

                //如果不存在于等待结果队列,则查询数据
                if (!PlayerCheckingSet.Contains(checkPlayerId)) {
                    PlayerCheckingSet.Add(checkPlayerId);

                    QueryStatByPidQueue.Enqueue(checkPlayerId);
                    QueryBfbanQueue.Enqueue(checkPlayerId);
                } else {
                    checkPerLimit--;
                }
            }
        }

        private void _QueryInterval(object state) {

            log.Debug($"Query Queue -> Name: {QueryStatByNameQueue.Count} Pid: {QueryStatByPidQueue.Count} Bfban: {QueryBfbanQueue.Count}");

            var statPerLimit = 8;
            while (QueryStatByNameQueue.Count > 0 && statPerLimit > 0) {
                var queryName = QueryStatByNameQueue.Dequeue();

                if (PlayerNameMapper.ContainsKey(queryName)) {
                    if (QueryStatByNameCallbacks.TryGetValue(queryName, out StatCallBack queryCallback)) {
                        queryCallback(PlayerInfoDict[PlayerNameMapper[queryName]]);
                        QueryStatByNameCallbacks.Remove(queryName);
                    }
                } else {
                    statPerLimit--;

                    DataApi.GetPlayerStatByName(
                        queryName, 
                        (data) => {
                            var json = ((JsonDocument)data).RootElement;
                            PlayerStatInfo statInfo = JsonSerializer.Deserialize<PlayerStatInfo>(json);

                            if (PlayerCacheIdMapper.TryGetValue(statInfo.id, out string plName)) statInfo.userName = plName;

                            if (statInfo.userName != null && !string.Equals(statInfo.userName, string.Empty)) {

                                PlayerInfoDict[statInfo.id] = statInfo;
                                PlayerNameMapper[statInfo.userName] = statInfo.id;

                                LocalDataFileHelper.SetPlayerInfo(statInfo);

                                if (QueryStatByNameCallbacks.TryGetValue(queryName, out StatCallBack queryCallback)) {
                                    queryCallback(statInfo);
                                    QueryStatByNameCallbacks.Remove(queryName);
                                }
                            }

                        }, 
                        () => {
                            QueryStatByNameQueue.Enqueue(queryName);
                        });
                }
            }

            while (QueryStatByPidQueue.Count > 0 && statPerLimit > 0) {
                var playerId = QueryStatByPidQueue.Dequeue();

                if (PlayerInfoDict.ContainsKey(playerId)) {
                    if (QueryStatByPidCallbacks.TryGetValue(playerId, out StatCallBack queryCallback)) {
                        queryCallback(PlayerInfoDict[playerId]);
                        QueryStatByPidCallbacks.Remove(playerId);
                    }
                } else {
                    statPerLimit--;

                    DataApi.GetPlayerStatByPlayerId(
                        playerId, 
                        (data) => {
                            var json = ((JsonDocument)data).RootElement;
                            PlayerStatInfo statInfo = JsonSerializer.Deserialize<PlayerStatInfo>(json); ;

                            if (PlayerCacheIdMapper.TryGetValue(playerId, out string plName)) statInfo.userName = plName;

                            if (statInfo.userName != null && !string.Equals(statInfo.userName, string.Empty)) {

                                PlayerInfoDict[statInfo.id] = statInfo;
                                PlayerNameMapper[statInfo.userName] = statInfo.id;

                                LocalDataFileHelper.SetPlayerInfo(statInfo);

                                if(QueryStatByPidCallbacks.TryGetValue(playerId, out StatCallBack queryCallback)) {
                                    queryCallback(statInfo);
                                    QueryStatByPidCallbacks.Remove(playerId);
                                }
                            }

                        },
                        () => {
                            QueryStatByPidQueue.Enqueue(playerId);
                        });
                }

            }

            /*query bfban*/
            var bfbanPerLimit = 32;
            ISet<long> bfbanQueryPidSet = new HashSet<long>();

            while (QueryBfbanQueue.Count > 0 && bfbanPerLimit > 0) {
                var playerId = QueryBfbanQueue.Dequeue();

                if (PlayerBfbanStatusDict.ContainsKey(playerId)) {
                    if(QueryBfbanCallbacks.TryGetValue(playerId, out BfbanCallback bfbanCallback)) {
                        bfbanCallback(PlayerBfbanStatusDict[playerId]);
                        QueryBfbanCallbacks.Remove(playerId);
                    }
                } else {
                    bfbanPerLimit--;
                    bfbanQueryPidSet.Add(playerId);
                }

                
            }

            if (bfbanQueryPidSet.Count > 0) {
                DataApi.GetBfbanStatusByPlayerIds(
                    bfbanQueryPidSet, 
                    (data) => {
                        var json = ((JsonDocument)data).RootElement;
                        var personaids = json.GetProperty("personaids").EnumerateObject();

                        foreach (var bfbanData in personaids) {
                            long playerId = long.Parse(bfbanData.Name);
                            PlayerBfbanStatusDict[playerId] = bfbanData.Value.GetProperty("hacker").GetBoolean();

                            LocalDataFileHelper.SetBfbanStat(playerId, PlayerBfbanStatusDict[playerId]);

                            if (QueryBfbanCallbacks.TryGetValue(playerId, out BfbanCallback bfbanCallback)) {
                                bfbanCallback(PlayerBfbanStatusDict[playerId]);
                                QueryBfbanCallbacks.Remove(playerId);
                            }
                        }

                    },
                    () => {
                        foreach (var item in bfbanQueryPidSet) QueryBfbanQueue.Enqueue(item);
                    });
            }
        }

        private void _GameStatInterval(object state) {
            if (CurrentGameId != 0) {
                log.Debug("Try Get Room Data to check");
                DataApi.GetGameRoomData(CurrentGameId, (gameRoom) => {
                    var list = ((GameRoom)gameRoom).GetPlayers();
                    foreach (var player in list) {
                        PlayerCheck(player.id);
                    }

                });
            }

        }

        public void ClearCacheData() {
            PlayerCacheIdMapper.Clear();
            PlayerCacheNameMapper.Clear();
        }

        public void PlayerCheck(long playerId) {
            if (!AbnormalPlayers.Contains(playerId) && !FairPlayers.Contains(playerId) && !PlayerCheckingSet.Contains(playerId)) {
                PlayerWaitingCheckQueue.Enqueue(playerId);
            }
        }

        public void PlayerCheck(string playerName) {
            if (PlayerNameMapper.ContainsKey(playerName)) {
                PlayerCheck(PlayerNameMapper[playerName]);
            } else { 
                long playerId = Robot.GetInstance().GetGameContext().GetPlayerId(playerName);
                if(playerId == 0) {
                    GetPlayerStatInfo(playerName, (data) => {
                        PlayerCheck(data.id);
                    });
                } else {
                    PlayerCacheIdMapper[playerId] = playerName;
                    PlayerNameMapper[playerName] = playerId;
                    PlayerCheck(playerId);
                }

            }
        }

        public void GetPlayerStatInfo(long playerId, StatCallBack callBack) {
            if (PlayerInfoDict.ContainsKey(playerId)) {
                callBack(PlayerInfoDict[playerId]);
                return;
            }

            QueryStatByPidQueue.Enqueue(playerId);
            
            if(QueryStatByPidCallbacks.TryGetValue(playerId, out StatCallBack statCallBack)) {
                statCallBack += callBack;
                QueryStatByPidCallbacks[playerId] = statCallBack;
            } else {
                QueryStatByPidCallbacks.Add(playerId, callBack);
            }

        }

        public void GetPlayerStatInfo(string playerName, StatCallBack callBack) {
            if (PlayerNameMapper.ContainsKey(playerName)) {
                callBack(PlayerInfoDict[PlayerNameMapper[playerName]]);
                return;
            }

            QueryStatByNameQueue.Enqueue(playerName);

            if (QueryStatByNameCallbacks.TryGetValue(playerName, out StatCallBack statCallBack)) {
                statCallBack += callBack;
                QueryStatByNameCallbacks[playerName] = statCallBack;
            } else {
                QueryStatByNameCallbacks.Add(playerName, callBack);
            }

        }

        public void GetBfbanStatusInfo(long playerId, BfbanCallback callBack) {
            if (PlayerBfbanStatusDict.ContainsKey(playerId)) {
                callBack(PlayerBfbanStatusDict[playerId]);
                return;
            }

            QueryBfbanQueue.Enqueue(playerId);

            if (QueryBfbanCallbacks.TryGetValue(playerId, out BfbanCallback bfbanCallBack)) {
                bfbanCallBack += callBack;
                QueryBfbanCallbacks[playerId] = bfbanCallBack;
            } else {
                QueryBfbanCallbacks.Add(playerId, callBack);
            }

        }

        //private System.Threading.Timer _QueryTimer;
        //private System.Threading.Timer _CheckTimer;
        //private System.Threading.Timer _GameRoomTimer;
        //private System.Threading.Timer _ContextTimer;
        //private System.Threading.Timer _BroadcastTimer;

        public void SetEnable(bool state) {
            lock (this) {
                if (state && !RunningState) {
                    _QueryTimer.Change(0, 5 * 1000);
                    _CheckTimer.Change(0, 20 * 1000);
                    _GameRoomTimer.Change(0, 15 * 1000);
                    _ContextTimer.Change(0, 60 * 1000);
                    _BroadcastTimer.Change(0, 45 * 1000);

                    Robot.GetInstance().GetLogger().Warn("Context 启动");
                } else if(!state && RunningState){
                    _QueryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _CheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _GameRoomTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _ContextTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _BroadcastTimer.Change(Timeout.Infinite, Timeout.Infinite);

                    Robot.GetInstance().GetLogger().Warn("Context 停止");
                }

                RunningState = state;
            }
        }

        public bool IsEnable() {
            return RunningState;
        }
    }
}
