using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.Protocol.Compatibility
{
    public class ConcurrentQueue<T> : IEnumerable<T>
    {
        private readonly Queue<T> _queue;

        public ConcurrentQueue()
        {
            _queue = new Queue<T>();
        }

        public void Enqueue(T item)
        {
            lock (_queue)
            {
                _queue.Enqueue(item);
            }
        }

        public bool TryDequeue(out T item)
        {
            lock (_queue)
            {
                if (_queue != null && _queue.Count > 0)
                {
                    try
                    {
                        item = _queue.Dequeue();
                        return true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Suppress
                    }
                }
            }
            item = default(T);
            return false;
        }

        public bool IsEmpty
        {
            get
            {
                lock (_queue)
                {
                    return _queue.Count == 0;
                }
            }
        }

        public int Count {
            get
            {
                lock (_queue)
                {
                    return _queue.Count;
                }
            }
        }

        public IQueryable<T> AsQueryable
        {
            get
            {
                lock (_queue) {
                    return _queue.AsQueryable<T>();
                }
            }
        }


        public IEnumerator<T> GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _queue.GetEnumerator(); 
        }
    }

}
