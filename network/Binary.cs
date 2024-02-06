using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network {
    class Binary {

        public static short ReadShort(byte[] bytes) {
            return 
                (short)((
                (bytes[0] & 0xff) << 8) + 
                (bytes[1] & 0xff));
        }

        public static long ReadLong(byte[] bytes) {
            return 
                (((long)bytes[0] << 56) +
                ((long)(bytes[1] & 0xff) << 48) +
                ((long)(bytes[2] & 0xff) << 40) +
                ((long)(bytes[3] & 0xff) << 32) +
                ((long)(bytes[4] & 0xff) << 24) +
                ((bytes[5] & 0xff) << 16) +
                ((bytes[6] & 0xff) << 8) +
                ((bytes[7] & 0xff)));
        }

        public static byte[] ShortToBytes(short s) {
            return new byte[2] { 
                (byte)((s >> 8) & 0xff),
                (byte)(s & 0xff)
            };
        }

        public static byte[] IntegerToBytes(int i) {
            return new byte[4] {
                (byte)((i >> 24) & 0xff),
                (byte)((i >> 16) & 0xff),
                (byte)((i >> 8) & 0xff),
                (byte)(i & 0xff)
            };
        }

        public static byte[] LongToBytes(long i) {
            return new byte[8] {
                (byte)((i >> 56) & 0xff),
                (byte)((i >> 48) & 0xff),
                (byte)((i >> 40) & 0xff),
                (byte)((i >> 32) & 0xff),
                (byte)((i >> 24) & 0xff),
                (byte)((i >> 16) & 0xff),
                (byte)((i >> 8) & 0xff),
                (byte)(i & 0xff)
            };
        }

    }
}
