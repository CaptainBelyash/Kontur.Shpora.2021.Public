using System;
using System.Collections.Generic;
using System.Threading;

namespace ReaderWriterLock
{
    public class RwLock : IRwLock
    {
        private object writerLocker = new();
        private HashSet<object> riderLockers = new();
        
        public void ReadLocked(Action action)
        {
            var riderLocker = new object();

            lock (riderLocker)
            {
                lock (writerLocker)
                {
                    riderLockers.Add(riderLocker);
                }
                action();
            }

            lock (writerLocker)
            {
                riderLockers.Remove(riderLocker);
            }
        }

        public void WriteLocked(Action action)
        {
            lock (writerLocker)
            {
                riderLockers.ForEach(locker => Monitor.Enter(locker));
                action();
                riderLockers.ForEach(locker => Monitor.Exit(locker));
            }
        }
    }
}