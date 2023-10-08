using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AimRobotLite.common {
    struct RoomPlayer {

        [JsonPropertyName("player_id")]
        public long id { get; set; }

        [JsonPropertyName("user_id")]
        public long userId { get; set; }
        public string name { get; set; }
        public int slot { get; set; }

    }
}
