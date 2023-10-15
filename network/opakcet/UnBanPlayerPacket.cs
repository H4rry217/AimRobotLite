﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network.opacket {
    class UnBanPlayerPacket : DataPacket {

        public long playerId;

        public override void Decode() {
            throw new NotImplementedException();
        }

        public override void Encode(){
            this.Reset();
            PutByte(GetPacketId());
            Put(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            Put(BitConverter.GetBytes(this.playerId));
        }

        public override byte GetPacketId() {
            return 0x31;
        }

    }
}
