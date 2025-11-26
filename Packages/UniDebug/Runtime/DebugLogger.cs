using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UniDebug;
using UniDebug.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

public static partial class DebugLogger
{
    public static LogLevel ToLogLevel(LogType logType)
    {
        return logType switch
        {
            LogType.Assert => LogLevel.Error,
            LogType.Error => LogLevel.Error,
            LogType.Exception => LogLevel.Error,
            LogType.Warning => LogLevel.Warning,
            LogType.Log => LogLevel.Debug,
            _ => LogLevel.Debug,
        };
    }

    private static int GetConsoleLogMax()
    {
        return SystemInfo.systemMemorySize < 4000 ? 100 : 3000;
    }

    private static int _logNumber;

    private const string StackTraceLogKey = "UniDebug_StackTraceLog";

    public static StackTraceLogType StackTrackLogType
    {
        get => (StackTraceLogType)PlayerPrefs.GetInt(StackTraceLogKey, (int)StackTraceLogType.ScriptOnly);
        set
        {
            PlayerPrefs.SetInt(StackTraceLogKey, (int)value);
            SetStackTraceLogType(value);
        }
    }

    private static void SetStackTraceLogType(StackTraceLogType value)
    {
        Application.SetStackTraceLogType(LogType.Log, value);
        Application.SetStackTraceLogType(LogType.Warning, value);
    }

    public static void ClearSavedSettings()
    {
        PlayerPrefs.DeleteKey(StackTraceLogKey);

        // プロパティを通じてデフォルト値にリセット（Unity内部設定も一緒に更新）
        StackTrackLogType = StackTraceLogType.ScriptOnly;
    }

    public static ConsoleLogManager GlobalConsoleLogManager { get; private set; }
#if UNITY_EDITOR
    private static bool _isFirstTime;
#endif

    public static event Action<ConsoleLog> OnReceiveMessageEvent
    {
        add
        {
            if (GlobalConsoleLogManager != null) { GlobalConsoleLogManager.OnReceiveMessageEvent += value; }
        }
        remove
        {
            if (GlobalConsoleLogManager != null) { GlobalConsoleLogManager.OnReceiveMessageEvent -= value; }
        }
    }

    static DebugLogger()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GlobalConsoleLogManager = new ConsoleLogManager(GetConsoleLogMax());
#if UNITY_EDITOR
        _isFirstTime = true;
        // エディターモードでは次回起動時までログマネージャー終了
        Application.quitting += OnQuitting;
#endif

#endif

        _logNumber = 0;
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void InitializeOnLoad()
    {
        // エディターで2回目以降の実行時はコンストラクタが呼ばれないため代替処理
        // staticコンストラクタより遅く実行されるため注意（他のクラスのstaticコンストラクタのログは収集不可）
        if (_isFirstTime)
        {
            _isFirstTime = false;
            Log("InitializeOnLoad first time");
        }
        else
        {
            GlobalConsoleLogManager = new ConsoleLogManager(GetConsoleLogMax());
            Log("InitializeOnLoad reset GlobalConsoleLogManager");
        }

        SetStackTraceLogType(StackTrackLogType);
    }

    private static void OnQuitting()
    {
        GlobalConsoleLogManager = null;
        _logNumber = 0;
    }
#endif

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private static void AdjustTag(ref string tag)
    {
        if (tag.Length > 20)
        {
            // フルパスの場合２０文字以上になることが多いので決め打ち
            tag = System.IO.Path.GetFileNameWithoutExtension(tag);
        }
    }

    /// <summary>
    /// ログレベルに応じた区切り線カラーを返す
    /// </summary>
    private static string GetDividerColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => "#FFFFFF",    // 白色
            LogLevel.Warning => "#FFFF00",  // 黄色
            LogLevel.Error => "#FF0000",    // 赤色
            _ => "#FFFFFF"
        };
    }

    /// <summary>
    /// CallerLineNumberを受け取るためのオーバーロード追加
    /// </summary>
    private static string CreateElementWithLine(string message, string tag, LogLevel level, int lineNumber, string debugTagName = null)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 로그 레벨에 따라 트레이서 표시 여부 확인
        bool shouldShowTracer = level switch
        {
            LogLevel.Debug => LogSetting.ShowDebugTracer,
            LogLevel.Warning => LogSetting.ShowWarningTracer,
            LogLevel.Error => LogSetting.ShowErrorTracer,
            _ => true
        };

        // 로그 레벨에 따라 DebugTag 표시 여부 확인
        bool shouldShowDebugTag = !string.IsNullOrEmpty(debugTagName) && level switch
        {
            LogLevel.Debug => LogSetting.ShowDebugTag,
            LogLevel.Warning => LogSetting.ShowWarningTag,
            LogLevel.Error => LogSetting.ShowErrorTag,
            _ => false
        };

        // 태그 조정
        AdjustTag(ref tag);

        // 스레드 세이프 인크리먼트
        Interlocked.Increment(ref _logNumber);

        // 트레이서 또는 DebugTag가 표시될 경우 구분선 표시
        bool shouldShowDivider = shouldShowTracer || shouldShowDebugTag;

        var dividerColor = GetDividerColor(level);
        var tracerPart = shouldShowTracer ? $"({tag}:{lineNumber})" : "";
        var debugTagPart = shouldShowDebugTag ? $" [<color=#00FF00>{debugTagName}</color>]" : "";
        var dividerPart = shouldShowDivider ? $" <color={dividerColor}><b>|</b></color> " : "";

        return $"{tracerPart}{debugTagPart}{dividerPart}{message}";
#else
        return message;
#endif
    }

    public static string RemoveElement(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        // "[tag]<level> message" からmessageを抽出する
        if (text[0] == '[')
        {
            var endIndex = text.IndexOf('>');
            if (endIndex > 0)
            {
                text = text.Substring(endIndex + 2);
            }
        }

        return text;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message, [CallerFilePath] string tag = "", [CallerLineNumber] int line = 0)
    {
        LogImplWithLine(message, tag, LogLevel.Debug, line);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(string message, [CallerFilePath] string tag = "", [CallerLineNumber] int line = 0)
    {
        LogImplWithLine(message, tag, LogLevel.Warning, line);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Assert(bool assertion, string message, [CallerFilePath] string tag = "", [CallerLineNumber] int line = 0)
    {
        AssertImplWithLine(assertion, message, tag, LogLevel.Error, line);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void AssertNotNull<T>(T obj, string message = "", [CallerFilePath] string tag = "", [CallerLineNumber] int line = 0)
        where T : class // AssertNotNull(obj != null) のような書き間違いを防ぐため制約を付ける
    {
        if (message == "") message = "Object is null";
        var isNotNull = obj is Object unityObj ? (bool)unityObj : obj != null;
        AssertImplWithLine(isNotNull, message, tag, LogLevel.Error, line);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void AssertNotNull<T>(T? obj, string message = "", [CallerFilePath] string tag = "", [CallerLineNumber] int line = 0)
        where T : struct // AssertNotNull(obj != null) のような書き間違いを防ぐため制約を付ける
    {
        if (message == "") message = "Object is null";
        AssertImplWithLine(obj != null, message, tag, LogLevel.Error, line);
    }

    /// <summary>
    /// 에러 로그 (개발 빌드에서만 실행)
    /// </summary>
    /// <param name="message">로그 메시지</param>
    /// <param name="tag">파일 경로 (자동 취득)</param>
    /// <param name="line">행 번호 (자동 취득)</param>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(string message, [CallerFilePath] string tag = "", [CallerLineNumber] int line = 0)
    {
        LogImplWithLine(message, tag, LogLevel.Error, line);
    }

    #region DebugTag 기반 로깅 메서드

    /// <summary>
    /// DebugTag를 사용한 로그 출력 (Console Filter Pro 스타일)
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string message, UniDebug.DebugTag debugTag, [CallerFilePath] string tag = "", [CallerLineNumber] int line = 0)
    {
        LogWithDebugTag(message, debugTag, tag, LogLevel.Debug, line);
    }

    /// <summary>
    /// DebugTag를 사용한 경고 로그 출력
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogWarning(string message, UniDebug.DebugTag debugTag, [CallerFilePath] string tag = "", [CallerLineNumber] int line = 0)
    {
        LogWithDebugTag(message, debugTag, tag, LogLevel.Warning, line);
    }

    /// <summary>
    /// DebugTag를 사용한 에러 로그 출력
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void LogError(string message, UniDebug.DebugTag debugTag, [CallerFilePath] string tag = "", [CallerLineNumber] int line = 0)
    {
        LogWithDebugTag(message, debugTag, tag, LogLevel.Error, line);
    }

    /// <summary>
    /// DebugTag 기반 로깅 구현
    /// </summary>
    private static void LogWithDebugTag(string message, UniDebug.DebugTag debugTag, string filePath, LogLevel level, int lineNumber)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // 태그 필터링 확인
        if (!UniDebug.TagFilterInfo.ShouldDisplay(debugTag))
        {
            UniDebug.TagFilterInfo.MessageWithheld(message, debugTag);
            return;
        }

        UniDebug.TagFilterInfo.MessagePassed();

        // 태그 이름 추출
        var tagName = debugTag.ToString();

        // 기존 로직 활용
        LogImplWithLine(message, filePath, level, lineNumber, tagName);
#endif
    }

    #endregion

    /// <summary>
    /// エラーログに関してはSentryなどに送信したいので常に機能するようにする
    /// </summary>
    /// <param name="exception"></param>
    public static void LogException(Exception exception)
    {
        Debug.LogException(exception);
    }

    /// <summary>
    /// 행 번호를 포함한 로그 구현
    /// </summary>
    private static void LogImplWithLine(string message, string tag, LogLevel level, int lineNumber, string debugTagName = null)
    {
        var element = CreateElementWithLine(message, tag, level, lineNumber, debugTagName);
        if (element == null) { return; }

        switch (level)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            case LogLevel.Debug:
                Debug.Log(element);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(element);
                break;
#endif
            case LogLevel.Error:
                Debug.LogError(element);
                break;
        }
    }

    /// <summary>
    /// 行番号を含むAssert実装
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    private static void AssertImplWithLine(bool assertion, string message, string tag, LogLevel level, int lineNumber)
    {
        if (assertion) { return; }
        LogImplWithLine(message, tag, level, lineNumber);
        AssertionStop();
    }

    private static void AssertionStop()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!LogSetting.EnableAssertionStop) { return; }

#if UNITY_EDITOR
        if (Thread.CurrentThread.ManagedThreadId != 1)
        {
            // メインスレッドでない場合は警告ログのみ出力
            Debug.LogWarning($"一時停止処理はメインスレッドでのみ可能です [ThreadId: {Thread.CurrentThread.ManagedThreadId}]");
            return;
        }
        // エディター一時停止
        Debug.LogWarning("エディターを一時停止します");
        UnityEditor.EditorApplication.isPaused = true;
#else
        // 実機ではタイムスケールを0に設定
        UnityEngine.Time.timeScale = 0;
#endif
#endif
    }
}
