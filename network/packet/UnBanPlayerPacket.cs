using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network.packet {
    class UnBanPlayerPacket : DataPacket {

        //TODO

        public override void Decode() {
            throw new NotImplementedException();
        }

        public override void Encode() {
            throw new NotImplementedException();
        }

        public override byte GetPacketId() {
            return Protocol.PACKET_UNBAN;
        }

    }
}
