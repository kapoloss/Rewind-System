using System;

namespace RewindSystem.Core
{
    /// <summary>
    /// A ring buffer (circular buffer) implementation for storing struct-based snapshots.
    /// </summary>
    public class StructRingBuffer<T> : IRewindBuffer<T> where T : struct
    {
        private readonly T[] _buffer;
        private int _head;
        private int _tail;
        private int _count;

        /// <inheritdoc />
        public bool IsEmpty => _count == 0;
        
        /// <inheritdoc />
        public int Count => _count;

        /// <summary>
        /// Creates a new StructRingBuffer with the given capacity.
        /// </summary>
        public StructRingBuffer(int capacity)
        {
            _buffer = new T[capacity];
            _head = 0;
            _tail = -1;
            _count = 0;
        }

        /// <inheritdoc />
        public ref T GetSlot()
        {
            ref T slot = ref _buffer[_head];
            _tail = _head;
            _head = (_head + 1) % _buffer.Length;

            if (_count < _buffer.Length)
                _count++;

            return ref slot;
        }

        /// <inheritdoc />
        public T GetLatest()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Buffer is empty.");
            return _buffer[_tail];
        }

        /// <inheritdoc />
        public ref T GetLatestRef()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Buffer is empty.");
            return ref _buffer[_tail];
        }

        /// <inheritdoc />
        public bool TryRemoveLatest(Action onBeforeBorn)
        {
            if (IsEmpty)
                return false;

            _tail = (_tail - 1 + _buffer.Length) % _buffer.Length;
            _head = (_head - 1 + _buffer.Length) % _buffer.Length;
            _count--;

            if (_count == 0)
                onBeforeBorn?.Invoke();

            return true;
        }

        /// <inheritdoc />
        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _head = 0;
            _tail = -1;
            _count = 0;
        }
    }
}