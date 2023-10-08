using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network.packet {
    class BanPlayerByNamePacket : DataPacket {

        public string playerName;

        public string reason;

        public override void Decode() {
            Get(1);

            playerName = GetString();
            reason = GetString();
        }

        public override void Encode() {
            throw new NotImplementedException();
        }

        public override byte GetPacketId() {
            return Protocol.PACKET_BAN;
        }

    }
}
