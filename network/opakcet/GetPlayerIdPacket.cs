using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network.opacket {
    class GetPlayerIdPacket : DataPacket {

        public string name;

        public override void Decode() {
            throw new NotImplementedException();
        }

        public override void Encode() {
            this.Reset();
            PutByte(GetPacketId());
            Put(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            Put(Encoding.UTF8.GetBytes(this.name));
        }

        public override byte GetPacketId() {
            return 0x36;
        }

    }
}
