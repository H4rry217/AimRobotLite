using AimRobot.Api.events.ev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network.packet {
    public class DeathEventPacket : DataPacket {

        public PlayerDeathEvent ev;

        public override void Decode() {
            throw new NotImplementedException();
        }

        public override void Encode() {
            this.Reset();
            PutByte(GetPacketId());
            PutString(this.ev.killerPlatoon);
            PutString(this.ev.killerName);
            PutString(this.ev.killerBy);
            PutString(this.ev.playerPlatoon);
            PutString(this.ev.playerName);
        }

        public override byte GetPacketId() {
            return Protocol.PACKET_EVENT_DEATH;
        }

    }
}
