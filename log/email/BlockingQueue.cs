using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EckyStudio.M.BaseModel.log.email
{
    public class BlockingQueue<T>
    {
        readonly Queue<T> mQueue = new Queue<T>();
        public T Take() {
            lock (mQueue)
            {
                while (mQueue.Count == 0) {
                    Monitor.Wait(mQueue);
                }
                
                T t = mQueue.Dequeue();
                //return default(T);
                return t;
            }
        }

        public void Put(T t) {
            lock(mQueue){
                mQueue.Enqueue(t);
                Monitor.Pulse(mQueue);
            }
        }
    }
}
