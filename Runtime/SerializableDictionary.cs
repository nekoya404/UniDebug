using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniDebug.Utils
{
    /// <summary>
    /// Unity Inspectorでシリアライズ可能なDictionary実装
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> _keys = new List<TKey>();

        [SerializeField]
        private List<TValue> _values = new List<TValue>();

        /// <summary>
        /// シリアライズ前に呼び出され、Dictionary内容をListに変換
        /// </summary>
        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var kvp in this)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }

        /// <summary>
        /// デシリアライズ後に呼び出され、ListをDictionaryに復元
        /// </summary>
        public void OnAfterDeserialize()
        {
            Clear();

            int count = Mathf.Min(_keys.Count, _values.Count);
            for (int i = 0; i < count; i++)
            {
                if (_keys[i] != null)
                {
                    this[_keys[i]] = _values[i];
                }
            }
        }
    }
}
