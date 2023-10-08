using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.network {
    public class BinaryStream {


        private int offset;

        private byte[] buffer = new byte[32];

        private int count;

        private const int MAX_ARRAY_SIZE = int.MaxValue - 8;

        public BinaryStream() {
            this.buffer = new byte[32];
            this.offset = 0;
            this.count = 0;
        }

        public BinaryStream(byte[] buffer) : this(buffer, 0) {

        }

        public BinaryStream(byte[] buffer, int offset) {
            this.buffer = buffer;
            this.offset = offset;
            this.count = buffer.Length;
        }

        public void Reset() {
            this.buffer = new byte[32];
            this.offset = 0;
            this.count = 0;
        }

        public void SetBuffer(byte[] buffer) {
            this.buffer = buffer;
            this.count = buffer == null ? -1 : buffer.Length;
        }

        public void SetBuffer(byte[] buffer, int offset) {
            this.SetBuffer(buffer);
            this.SetOffset(offset);
        }

        public int GetOffset() {
            return this.offset;
        }

        public void SetOffset(int offset) {
            this.offset = offset;
        }

        public byte[] GetBuffer() {
            byte[] copyArray = new byte[count];
            Array.Copy(buffer, copyArray, this.count);

            return copyArray;
        }

        public int GetCount() {
            return this.count;
        }

        public byte[] Get() {
            return this.Get(this.count - this.offset);
        }

        public byte[] Get(int len) {
            if (len < 0) {
                this.offset = this.count - 1;
                return new byte[0];
            }

            len = Math.Min(len, this.GetCount() - this.offset);
            this.offset += len;


            byte[] copyArray = new byte[len];
            Array.Copy(this.buffer, this.offset - len, copyArray, 0, len);
            return copyArray;
        }

        public void Put(byte[] bytes) {
            if (bytes == null) {
                return;
            }

            this.EnsureCapacity(this.count + bytes.Length);

            Array.Copy(bytes, 0, buffer, count, bytes.Length);
            this.count += bytes.Length;
        }

        public void PutByte(byte b) {
            this.Put(new byte[] { b });
        }

        private void EnsureCapacity(int minCapacity) {
            // overflow-conscious code
            if (minCapacity - this.buffer.Length > 0) {
                Grow(minCapacity);
            }
        }

        private void Grow(int minCapacity) {
            int oldCapacity = this.buffer.Length;
            int newCapacity = oldCapacity << 1;

            if (newCapacity - minCapacity < 0) {
                newCapacity = minCapacity;
            }

            if (newCapacity - MAX_ARRAY_SIZE > 0) {
                newCapacity = HugeCapacity(minCapacity);
            }

            Array.Resize(ref buffer, newCapacity);
        }

        private static int HugeCapacity(int minCapacity) {
            if (minCapacity < 0) throw new OutOfMemoryException();

            return (minCapacity > MAX_ARRAY_SIZE) ? int.MaxValue : MAX_ARRAY_SIZE;
        }

        public short GetShort() {
            return Binary.ReadShort(Get(2));
        }

        public string GetString(Encoding encoding) {
            short len = GetShort();
            return encoding.GetString(Get(len));
        }

        public string GetString() {
            return GetString(Encoding.UTF8);
        }

        public long GetLong() {
            return Binary.ReadLong(Get(8));
        }

    }
}
