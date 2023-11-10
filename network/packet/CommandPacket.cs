using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network.packet {
    public class CommandPacket : DataPacket {

        public string command;

        public override void Decode() {
            Get(1);

            command = GetString();
        }

        public override void Encode() {
            throw new NotImplementedException();
        }

        public override byte GetPacketId() {
            return Protocol.PACKET_COMMAND;
        }

    }
}
