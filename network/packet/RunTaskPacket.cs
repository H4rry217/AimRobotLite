using AimRobotLite.service.automanage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network.packet {
    public class RunTaskPacket : DataPacket {

        public GameWindow.Task task;

        public override void Decode() {
            this.Get(1);
            string s = this.GetString();
            this.task = (GameWindow.Task)Enum.Parse(typeof(GameWindow.Task), s);
        }

        public override void Encode() {
            throw new NotImplementedException();
        }

        public override byte GetPacketId() {
            return Protocol.PACKET_RUNTASK;
        }

    }
}
