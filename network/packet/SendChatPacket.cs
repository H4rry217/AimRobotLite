using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network.packet {
    class SendChatPacket : DataPacket {

        public string message;

        public override void Decode() {
            Get(1);

            message = GetString();
        }

        public override void Encode() {
            throw new NotImplementedException();
        }

        public override byte GetPacketId() {
            return Protocol.PACKET_SEND_CHAT;
        }
    }

}
