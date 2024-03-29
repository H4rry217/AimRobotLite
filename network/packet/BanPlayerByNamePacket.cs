﻿using System;
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
            this.Reset();
            PutByte(GetPacketId());
            PutString(this.playerName);
            PutString(this.reason);
        }

        public override byte GetPacketId() {
            return Protocol.PACKET_BAN_BY_NAME;
        }

    }
}
