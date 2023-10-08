using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AimRobotLite.network.packet {
    class BanPlayerPacket : DataPacket {

        public long playerId;

        public string reason;

        public override void Decode() {
            Get(1);

            playerId = GetLong();
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
