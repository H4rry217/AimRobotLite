using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network {
    public abstract class DataPacket : BinaryStream {

        public abstract byte GetPacketId();

        public abstract void Decode();

        public abstract void Encode();

    }
}
