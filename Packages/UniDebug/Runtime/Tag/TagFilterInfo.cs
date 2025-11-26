using System.Collections.Generic;
using UnityEngine;

namespace UniDebug
{
    /// <summary>
    /// タグフィルタリング状態情報を管理する静的クラス
    /// </summary>
    public static class TagFilterInfo
    {
        /// <summary>
        /// 現在表示中のタグマスク
        /// </summary>
        public static TagMask DisplayedTags { get; set; } = TagMask.AllTags;

        /// <summary>
        /// 通過した（表示された）メッセージ数
        /// </summary>
        public static int PassedCount { get; private set; }

        /// <summary>
        /// 保留された（フィルタリングされた）メッセージ数
        /// </summary>
        public static int WithheldCount => _withheldMessages.Count;

        private static readonly List<WithheldMessage> _withheldMessages = new List<WithheldMessage>();

        /// <summary>
        /// 保留されたメッセージ情報
        /// </summary>
        public struct WithheldMessage
        {
            public string Message;
            public DebugTag Tag;
            public System.DateTime Time;
        }

        /// <summary>
        /// メッセージが表示されるべきか確認
        /// </summary>
        /// <param name="tag">確認するタグ</param>
        /// <returns>表示可否</returns>
        public static bool ShouldDisplay(DebugTag tag)
        {
            return DisplayedTags.Contains(tag);
        }

        /// <summary>
        /// メッセージ通過処理
        /// </summary>
        public static void MessagePassed()
        {
            PassedCount++;
        }

        /// <summary>
        /// メッセージ保留処理
        /// </summary>
        public static void MessageWithheld(string message, DebugTag tag)
        {
#if UNITY_EDITOR
            _withheldMessages.Add(new WithheldMessage
            {
                Message = message,
                Tag = tag,
                Time = System.DateTime.Now
            });
#endif
        }

        /// <summary>
        /// 特定タグの保留メッセージを出力
        /// </summary>
        public static void FlushWithheldMessages(DebugTag tag)
        {
#if UNITY_EDITOR
            var messages = _withheldMessages.FindAll(m => m.Tag == tag);
            if (messages.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"=== [{tag}] Withheld messages: {messages.Count} ===");
                foreach (var msg in messages)
                {
                    sb.AppendLine($"[{msg.Time:HH:mm:ss}] {msg.Message}");
                }
                Debug.Log(sb.ToString());

                _withheldMessages.RemoveAll(m => m.Tag == tag);
            }
#endif
        }

        /// <summary>
        /// カウンターをリセット
        /// </summary>
        public static void ResetCounters()
        {
            PassedCount = 0;
            _withheldMessages.Clear();
        }
    }
}
