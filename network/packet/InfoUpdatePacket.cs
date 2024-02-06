using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AimRobotLite.service.automanage.GameWindow;

namespace AimRobotLite.network.packet {
    public class InfoUpdatePacket : DataPacket {

        public WindowInfo info;
        public long timestamp;

        public override void Decode() {
            throw new NotImplementedException();
        }

        public override void Encode() {
            this.Reset();
            PutByte(GetPacketId());
            PutLong(this.timestamp);
            PutString(this.info.RunTask.ToString());
            PutString(this.info.State.ToString());
            PutShort((short)this.info.ErrorCount);
        }

        public override byte GetPacketId() {
            return Protocol.PACKET_WINDOWINFO;
        }

    }
}
