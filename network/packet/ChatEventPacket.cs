using AimRobot.Api.events.ev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network.packet {
    public class ChatEventPacket : DataPacket {

        public PlayerChatEvent ev;

        public override void Decode() {
            throw new NotImplementedException();
        }

        public override void Encode() {
            this.Reset();
            PutByte(GetPacketId());
            PutString(this.ev.speaker);
            PutString(this.ev.message);
        }

        public override byte GetPacketId() {
            return Protocol.PACKET_EVENT_CHAT;
        }

    }
}
