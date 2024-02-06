using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network.packet {
    public class ScreenshotPacket : DataPacket {

        public long timestamp;
        public Bitmap image;

        public override void Decode() {
            Get(1);
            this.timestamp = GetLong();
        }

        public override void Encode() {
            this.Reset();
            PutByte(GetPacketId());

            Put(Binary.LongToBytes(this.timestamp));

            byte[] bytes = null;
            using (MemoryStream stream = new MemoryStream()) {
                image.Save(stream, ImageFormat.Png);
                bytes = stream.ToArray();
            }

            Put(Binary.IntegerToBytes(bytes.Length));
            Put(bytes);
        }

        public override byte GetPacketId() {
            return Protocol.PACKET_SCREENSHOT;
        }

    }
}
