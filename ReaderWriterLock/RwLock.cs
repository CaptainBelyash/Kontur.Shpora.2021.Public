using System;
using System.Collections.Generic;
using System.Threading;

namespace ReaderWriterLock
{
    public class RwLock : IRwLock
    {
        private const int CriticalUsedCount = 10;
        
        private object writerLocker = new();
        private object cleanerLocker = new();
        private HashSet<object> riderLockers = new();
        private HashSet<object> usedLockers = new();
        
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
                lock (cleanerLocker)
                {
                    usedLockers.Add(riderLocker);
                    
                    if (usedLockers.Count > CriticalUsedCount)
                        new Thread(() => CleanLockers()).Start();
                }
            }
        }

        public void WriteLocked(Action action)
        {
            lock (writerLocker)
            {
                riderLockers.ForEach(locker => Monitor.Enter(locker));
                action();
                riderLockers.ForEach(locker => Monitor.Exit(locker));
                riderLockers.Clear();
                usedLockers.Clear();
            }
        }

        private void CleanLockers()
        {
            lock (writerLocker)
            {
                lock (cleanerLocker)
                {
                    riderLockers.ExceptWith(usedLockers);
                    usedLockers.Clear();
                }
            }
        }
    }
}