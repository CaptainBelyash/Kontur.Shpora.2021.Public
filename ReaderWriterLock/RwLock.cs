using System;
using System.Collections.Generic;
using System.Threading;

namespace ReaderWriterLock
{
    public class RwLock : IRwLock
    {
        private object writerLocker = new();
        private HashSet<Mutex> riderMutexes = new();
        
        public void ReadLocked(Action action)
        {
            var mutex = new Mutex();
            lock (writerLocker)
            {
                riderMutexes.Add(mutex);
                mutex.WaitOne();
            }
            action();
            mutex.ReleaseMutex();
        }

        public void WriteLocked(Action action)
        {
            lock (writerLocker)
            {
                riderMutexes.ForEach(m => m.WaitOne());
                action();
                riderMutexes.Clear();
            }
        }
    }
}