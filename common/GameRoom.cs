    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.common {
    class GameRoom {

        public ISet<RoomPlayer> TeamOne = new HashSet<RoomPlayer>();
        public ISet<RoomPlayer> TeamTwo = new HashSet<RoomPlayer>();

        public ISet<RoomPlayer> InQueue = new HashSet<RoomPlayer>();
        public ISet<RoomPlayer> InLoading = new HashSet<RoomPlayer>();

        public ISet<RoomPlayer> Spectators = new HashSet<RoomPlayer>();

        public IList<RoomPlayer> GetPlayers() {
            ISet<RoomPlayer> resultSet = new HashSet<RoomPlayer>();

            resultSet.UnionWith(TeamOne);
            resultSet.UnionWith(TeamTwo);
            resultSet.UnionWith(InQueue);
            resultSet.UnionWith(InLoading);
            resultSet.UnionWith(Spectators);

            return resultSet.ToList();
        }

    }
}
