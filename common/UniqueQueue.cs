using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.common {
    public class UniqueQueue<T> : Queue<T> {

        private Queue<T> queue = new Queue<T>();
        private ISet<T> set = new HashSet<T>();
        private object lockObject = new object();

        public new int Count {
            get { return this.GetCount(); }
        }

        public UniqueQueue() {

        }

        public T Dequeue() {
            T val;
            lock (lockObject) {
                val = queue.Dequeue();
                if (set.Contains(val)) set.Remove(val);
            }

            return val;
        }

        public void Enqueue(T item) {
            lock (lockObject) {
                if (!set.Contains(item)) {
                    this.queue.Enqueue(item);
                    this.set.Add(item);
                }
            }
        }

        public int GetCount() {
            return this.queue.Count;
        }

    }
}
