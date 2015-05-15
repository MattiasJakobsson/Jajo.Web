using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SuperGlue.Diagnostics
{
    internal class ConcurrentLruLSet<T> : IEnumerable<T>
    {
        private LinkedList<T> _items = new LinkedList<T>();
        private readonly int _maxCapacity;
        private readonly Action<T> _onDrop;

        public ConcurrentLruLSet(int maxCapacity, Action<T> onDrop = null)
        {
            _maxCapacity = maxCapacity;
            _onDrop = onDrop;
        }

        public T FirstOrDefault(Func<T, bool> predicate)
        {
            return _items.FirstOrDefault(predicate);
        }

        public IEnumerable<T> Where(Func<T, bool> predicate)
        {
            return _items.Where(predicate);
        }

        public void Push(T item)
        {
            LinkedList<T> comparand;
            LinkedList<T> linkedList;
            LinkedListNode<T> linkedListNode;
            do
            {
                comparand = _items;
                linkedList = new LinkedList<T>(comparand);
                linkedList.Remove(item);
                linkedList.AddLast(item);
                linkedListNode = null;
                if (linkedList.Count > _maxCapacity)
                {
                    linkedListNode = linkedList.First;
                    linkedList.RemoveFirst();
                }
            }
            while (Interlocked.CompareExchange(ref _items, linkedList, comparand) != comparand);
            if (_onDrop == null || linkedListNode == null)
                return;
            _onDrop(linkedListNode.Value);
        }

        public void Clear()
        {
            _items = new LinkedList<T>();
        }

        public void ClearHalf()
        {
            LinkedList<T> comparand;
            do
            {
                comparand = _items;
            }
            while (Interlocked.CompareExchange(ref _items, new LinkedList<T>(comparand.Skip(comparand.Count / 2)), comparand) != comparand);
            if (_onDrop == null)
                return;
            foreach (var obj in comparand.Take(comparand.Count / 2))
                _onDrop(obj);
        }

        public void Remove(T val)
        {
            _items.Remove(val);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}