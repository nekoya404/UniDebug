using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniDebug
{
    /// <summary>
    /// 여러 태그를 비트마스크로 효율적으로 관리하는 구조체
    /// </summary>
    [Serializable]
    public struct TagMask : IEquatable<TagMask>
    {
        [SerializeField]
        private int _value;

        public int Value => _value;

        #region Construction

        public TagMask(int mask)
        {
            if (mask > AllTags._value)
            {
                int valuesAmount = Enum.GetValues(typeof(DebugTag)).Length;
                Debug.LogError($"마스크 값이 너무 큽니다. 최대값: {AllTags._value} (태그 개수: {valuesAmount})");
                _value = mask & AllTags._value;
                return;
            }
            _value = mask;
        }

        public TagMask(DebugTag tag)
        {
            _value = TagToMask(tag);
        }

        public TagMask(params DebugTag[] tags)
        {
            _value = 0;
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    _value |= TagToMask(tag);
                }
            }
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// 가능한 최대 마스크 값
        /// </summary>
        private static int MaxValue
        {
            get
            {
                int shift = Enum.GetValues(typeof(DebugTag)).Length + 1;
                Debug.Assert(shift < sizeof(int) * 8 - 1, "등록된 태그가 너무 많습니다");
                return (1 << shift) - 1;
            }
        }

        /// <summary>
        /// 모든 태그가 포함된 마스크
        /// </summary>
        public static TagMask AllTags => new TagMask { _value = MaxValue };

        /// <summary>
        /// 태그가 없는 빈 마스크
        /// </summary>
        public static TagMask None => new TagMask { _value = 0 };

        #endregion

        #region Methods

        /// <summary>
        /// 태그 enum 값을 비트마스크로 변환
        /// </summary>
        public static int TagToMask(DebugTag tag)
        {
            int maxShift = sizeof(int) * 8;
            int shift = (int)tag;
            if (shift < maxShift)
            {
                return 1 << shift;
            }
            else
            {
                Debug.LogError($"태그 {tag}가 유효 범위를 벗어났습니다. 등록된 태그가 너무 많을 수 있습니다.");
                return 0;
            }
        }

        /// <summary>
        /// 현재 마스크에 포함된 모든 태그를 반환
        /// </summary>
        public IEnumerable<DebugTag> GetTags()
        {
            foreach (DebugTag tag in Enum.GetValues(typeof(DebugTag)))
            {
                if ((TagToMask(tag) & _value) != 0)
                {
                    yield return tag;
                }
            }
        }

        /// <summary>
        /// 특정 태그가 포함되어 있는지 확인
        /// </summary>
        public bool Contains(DebugTag tag) => (_value & TagToMask(tag)) != 0;

        /// <summary>
        /// 다른 마스크와 하나라도 겹치는 태그가 있는지 확인
        /// </summary>
        public bool ContainsAny(TagMask other) => (_value & other._value) != 0;

        /// <summary>
        /// 다른 마스크의 모든 태그를 포함하는지 확인
        /// </summary>
        public bool ContainsAll(TagMask other) => (_value & other._value) == other._value;

        #endregion

        #region Operators

        public static TagMask operator &(TagMask left, TagMask right)
            => new TagMask(left._value & right._value);

        public static TagMask operator |(TagMask left, TagMask right)
            => new TagMask(left._value | right._value);

        public static TagMask operator ^(TagMask left, TagMask right)
            => new TagMask(left._value ^ right._value);

        public static TagMask operator ~(TagMask mask)
            => new TagMask(~mask._value & AllTags._value);

        public static bool operator ==(TagMask left, TagMask right)
            => left._value == right._value;

        public static bool operator !=(TagMask left, TagMask right)
            => left._value != right._value;

        #endregion

        #region Equality

        public bool Equals(TagMask other) => _value == other._value;

        public override bool Equals(object obj)
        {
            if (obj is TagMask other)
            {
                return _value == other._value;
            }
            return false;
        }

        public override int GetHashCode() => _value.GetHashCode();

        public override string ToString()
        {
            var tags = new List<string>();
            foreach (var tag in GetTags())
            {
                tags.Add(tag.ToString());
            }
            return tags.Count > 0 ? string.Join(", ", tags) : "None";
        }

        #endregion
    }
}
