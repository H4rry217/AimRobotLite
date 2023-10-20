using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network.opakcet {
    public class JoinGamePacket : DataPacket {

        public long gameId;

        public override void Decode() {
            throw new NotImplementedException();
        }

        public override void Encode() {
            this.Reset();
            PutByte(GetPacketId());
            Put(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            Put(BitConverter.GetBytes(this.gameId));
        }

        public override byte GetPacketId() {
            return 0x32;
        }

    }
}
