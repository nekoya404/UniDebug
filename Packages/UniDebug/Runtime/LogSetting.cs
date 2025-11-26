using System;
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

namespace UniDebug
{
    /// <summary>
    /// 로그 표시 설정 관리 클래스
    /// </summary>
    [Serializable]
    public class LogSetting
    {
        private const string UniDebugLogKey = "UniDebugLog";

        public static readonly LogSetting _instance = new();

        [SerializeField]
        private bool _enableAssertionStop = true;

        [SerializeField]
        private bool _showDebugTag = true;

        [SerializeField]
        private bool _showWarningTag = true;

        [SerializeField]
        private bool _showErrorTag = true;

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

        public static void ClearSavedSettings()
        {
            PlayerPrefs.DeleteKey(UniDebugLogKey);

            // メモリ上のインスタンスも初期化
            _instance._enableAssertionStop = false;
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
