using System;
using System.Collections;
using System.Collections.Generic;

namespace UniDebug.Utils
{
    /// <summary>
    /// 固定サイズのリングバッファ実装
    /// 最大容量を超えると最も古い項目を上書き
    /// </summary>
    public class RingBuffer<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;
        private int _head; // 次の書き込み位置
        private int _count;
        private readonly object _lock = new object();

        public int Capacity { get; }

        /// <summary>
        /// 現在バッファに保存されている項目数
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _count;
                }
            }
        }

        /// <summary>
        /// 指定された容量でRingBufferを生成
        /// </summary>
        public RingBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0", nameof(capacity));

            Capacity = capacity;
            _buffer = new T[capacity];
            _head = 0;
            _count = 0;
        }

        /// <summary>
        /// バッファに項目を追加
        /// バッファが満杯の場合、最も古い項目を上書き
        /// </summary>
        public void Add(T item)
        {
            lock (_lock)
            {
                _buffer[_head] = item;
                _head = (_head + 1) % Capacity;

                if (_count < Capacity)
                {
                    _count++;
                }
            }
        }

        /// <summary>
        /// インデックスで項目にアクセス（0が最も古い項目）
        /// </summary>
        public T this[int index]
        {
            get
            {
                lock (_lock)
                {
                    if (index < 0 || index >= _count)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    int actualIndex = (_head - _count + index + Capacity) % Capacity;
                    return _buffer[actualIndex];
                }
            }
        }

        /// <summary>
        /// バッファの全項目を削除
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                Array.Clear(_buffer, 0, _buffer.Length);
                _head = 0;
                _count = 0;
            }
        }

        /// <summary>
        /// 条件を満たす項目を順回
        /// </summary>
        public void ForEach(Func<T, bool> predicate, Action<T, int> action)
        {
            lock (_lock)
            {
                int itemCount = 0;
                for (int i = 0; i < _count; i++)
                {
                    T item = this[i];
                    if (predicate(item))
                    {
                        action(item, itemCount);
                        itemCount++;
                    }
                }
            }
        }

        /// <summary>
        /// 逆順で条件を満たす項目を順回
        /// </summary>
        public void ReverseForEach(Func<T, int, bool> predicate)
        {
            lock (_lock)
            {
                int itemCount = 0;
                for (int i = _count - 1; i >= 0; i--)
                {
                    T item = this[i];
                    if (!predicate(item, itemCount))
                        break;
                    itemCount++;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
            {
                var snapshot = new T[_count];
                for (int i = 0; i < _count; i++)
                {
                    snapshot[i] = this[i];
                }

                foreach (var item in snapshot)
                {
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
