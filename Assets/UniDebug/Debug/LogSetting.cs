using System;
using System.Collections.Generic;
using UniDebug.Utils;
using UnityEngine;

public partial class DebugLogger
{
    // 場所を移すと大量ファイルに変更が出るので残す
    public enum LogLevel
    {
        Debug, // デバッグ用情報（通常時は表示せず、タグを明示的に指定することで表示する想定）
        Warning, // 警告
        Error // エラー
    }
}

// TODO: デバッグビルドでのみ含まれるように設定が必要
namespace UniDebug
{
    [Serializable]
    public class LogSetting
    {
        private const string UniDebugLogKey = "UniDebugLog";

        public static readonly LogSetting _instance = new();

        [SerializeField]
        private bool _enableAssertionStop = false;

        // タグ保存: keyは小文字、valueは有効化するかどうか
        [SerializeField]
        private SerializableDictionary<string, bool> _tags = new();

        // 表示用タグ名保存: keyは小文字、valueは元の表示名
        [SerializeField]
        private SerializableDictionary<string, string> _tagDisplayNames = new();

        [SerializeField]
        private bool _showDebugTag = false;

        [SerializeField]
        private bool _showWarningTag = false;

        [SerializeField]
        private bool _showErrorTag = false;

        [SerializeField]
        private bool _showDebugTracer = true;

        [SerializeField]
        private bool _showWarningTracer = true;

        [SerializeField]
        private bool _showErrorTracer = true;

        public static bool EnableAssertionStop
        {
            get => _instance._enableAssertionStop;
            set
            {
                if (_instance._enableAssertionStop != value)
                {
                    _instance._enableAssertionStop = value;
                    Save();
                }
            }
        }

        public static IReadOnlyDictionary<string, bool> Tags => _instance._tags;

        public static bool ShowDebugTag
        {
            get => _instance._showDebugTag;
            set
            {
                if (_instance._showDebugTag != value)
                {
                    _instance._showDebugTag = value;
                    Save();
                }
            }
        }

        public static bool ShowWarningTag
        {
            get => _instance._showWarningTag;
            set
            {
                if (_instance._showWarningTag != value)
                {
                    _instance._showWarningTag = value;
                    Save();
                }
            }
        }

        public static bool ShowErrorTag
        {
            get => _instance._showErrorTag;
            set
            {
                if (_instance._showErrorTag != value)
                {
                    _instance._showErrorTag = value;
                    Save();
                }
            }
        }

        public static bool ShowDebugTracer
        {
            get => _instance._showDebugTracer;
            set
            {
                if (_instance._showDebugTracer != value)
                {
                    _instance._showDebugTracer = value;
                    Save();
                }
            }
        }

        public static bool ShowWarningTracer
        {
            get => _instance._showWarningTracer;
            set
            {
                if (_instance._showWarningTracer != value)
                {
                    _instance._showWarningTracer = value;
                    Save();
                }
            }
        }

        public static bool ShowErrorTracer
        {
            get => _instance._showErrorTracer;
            set
            {
                if (_instance._showErrorTracer != value)
                {
                    _instance._showErrorTracer = value;
                    Save();
                }
            }
        }

        public static event Action OnChanged;

        static LogSetting()
        {
            var json = PlayerPrefs.GetString(UniDebugLogKey);
            try
            {
                JsonUtility.FromJsonOverwrite(json, _instance);
            }
            catch (Exception)
            {
                PlayerPrefs.DeleteKey(UniDebugLogKey);
            }
        }

        private LogSetting()
        {
        }

        /// <summary>
        /// タグ設定（大文字小文字を区別しない）
        /// </summary>
        /// <param name="key">タグ名（元の形式）</param>
        /// <param name="enabled">有効化するかどうか</param>
        public static void SetTag(string key, bool enabled = true)
        {
            var lowerKey = key.ToLower();
            var hasKey = _instance._tags.TryGetValue(lowerKey, out var current);
            if ((hasKey && current != enabled) || (!hasKey && enabled))
            {
                _instance._tags[lowerKey] = enabled;
                // 表示用の名前を保存（初回登録時または既存にない場合）
                if (!_instance._tagDisplayNames.ContainsKey(lowerKey))
                {
                    _instance._tagDisplayNames[lowerKey] = key;
                }
                Save();
            }
        }

        /// <summary>
        /// タグ削除（大文字小文字を区別しない）
        /// </summary>
        public static void RemoveTag(string key)
        {
            var lowerKey = key.ToLower();
            var removed = _instance._tags.Remove(lowerKey);
            if (removed)
            {
                _instance._tagDisplayNames.Remove(lowerKey);
                Save();
            }
        }

        /// <summary>
        /// タグの有効化状態を確認（大文字小文字を区別しない）
        /// </summary>
        public static bool CheckTag(string key)
        {
            var lowerKey = key.ToLower();
            return _instance._tags.TryGetValue(lowerKey, out var current) && current;
        }

        /// <summary>
        /// タグの表示名を取得
        /// </summary>
        public static string GetTagDisplayName(string key)
        {
            var lowerKey = key.ToLower();
            return _instance._tagDisplayNames.TryGetValue(lowerKey, out var displayName) ? displayName : key;
        }

        /// <summary>
        /// カスタムタグがある場合のフィルタリング可否を確認（大文字小文字を区別しない）
        /// - タグが登録されていない場合はtrue（表示）
        /// - タグが登録されていて有効化されている場合はtrue（表示）
        /// - タグが登録されているが無効化されている場合はfalse（非表示）
        /// </summary>
        public static bool ShouldDisplayCustomTag(string customTag)
        {
            // カスタムタグがない場合は常に表示
            if (string.IsNullOrEmpty(customTag))
            {
                return true;
            }

            var lowerTag = customTag.ToLower();

            // 登録されていないタグは常に表示（フィルタリングしない）
            if (!_instance._tags.ContainsKey(lowerTag))
            {
                return true;
            }

            // 登録されたタグは有効化状態に応じてフィルタリング
            return CheckTag(customTag);
        }

        public static void ClearSavedSettings()
        {
            PlayerPrefs.DeleteKey(UniDebugLogKey);

            // メモリ上のインスタンスも初期化
            _instance._enableAssertionStop = false;
            _instance._tags.Clear();
            _instance._tagDisplayNames.Clear();
            _instance._showDebugTag = false;
            _instance._showWarningTag = false;
            _instance._showErrorTag = false;
            _instance._showDebugTracer = true;
            _instance._showWarningTracer = true;
            _instance._showErrorTracer = true;

            OnChanged?.Invoke();
        }

        private static void Save()
        {
            var json = JsonUtility.ToJson(_instance);
            PlayerPrefs.SetString(UniDebugLogKey, json);

            OnChanged?.Invoke();
        }
    }
}
