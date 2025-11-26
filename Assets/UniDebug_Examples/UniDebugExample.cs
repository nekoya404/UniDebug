using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// UniDebugの全機能をテストするサンプルスクリプト
/// UnityエディターのGameObjectに追加して使用
/// </summary>
public class UniDebugExample : MonoBehaviour
{
    [Header("テスト設定")]
    [SerializeField] private bool autoTest = false;
    [SerializeField] private float testInterval = 2f;

    private void Start()
    {
        if (autoTest)
        {
            StartCoroutine(AutoTestCoroutine());
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 800));
        GUILayout.Label("=== UniDebug テストサンプル ===", GUI.skin.box);

        if (GUILayout.Button("1. 基本ログテスト"))
        {
            TestBasicLogs();
        }

        if (GUILayout.Button("2. ログレベルテスト"))
        {
            TestLogLevels();
        }

        if (GUILayout.Button("3. Assertテスト"))
        {
            TestAsserts();
        }

        if (GUILayout.Button("4. AssertNotNullテスト"))
        {
            TestAssertNotNull();
        }

        if (GUILayout.Button("5. エラーログテスト"))
        {
            TestErrorLogs();
        }

        if (GUILayout.Button("6. Exceptionテスト"))
        {
            TestException();
        }

        if (GUILayout.Button("7. タグフィルタリングテスト"))
        {
            TestTagFiltering();
        }

        if (GUILayout.Button("8. マルチスレッドテスト"))
        {
            TestMultiThread();
        }

        if (GUILayout.Button("9. パフォーマンステスト (1000ログ)"))
        {
            TestPerformance();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("全テスト実行"))
        {
            StartCoroutine(RunAllTests());
        }

        GUILayout.EndArea();
    }

    #region 1. 基本ログテスト

    /// <summary>
    /// 基本ログ出力機能テスト
    /// </summary>
    private void TestBasicLogs()
    {
        DebugLogger.Log("基本Debugログです");
        DebugLogger.LogWarning("基本Warningログです");
        DebugLogger.LogError("基本Errorログです");
    }

    #endregion

    #region 2. ログレベルテスト

    /// <summary>
    /// 様々なログレベルテスト
    /// </summary>
    private void TestLogLevels()
    {
        DebugLogger.Log("DEBUGレベルメッセージ");
        DebugLogger.LogWarning("WARNINGレベルメッセージ");
        DebugLogger.LogError("ERRORレベルメッセージ");
    }

    #endregion

    #region 3. Assertテスト

    /// <summary>
    /// Assert機能テスト
    /// </summary>
    private void TestAsserts()
    {
        // 成功するAssert（何も出力されない）
        DebugLogger.Assert(true, "このメッセージは表示されません");

        // 失敗するAssert（エラー） - 注意: エディター一時停止される可能性あり
        // DebugLogger.Assert(false, "Assert Errorテスト - 条件がfalseです");
    }

    #endregion

    #region 4. AssertNotNullテスト

    /// <summary>
    /// Nullチェック Assert テスト
    /// </summary>
    private void TestAssertNotNull()
    {
        // 参照型テスト
        GameObject validObject = gameObject;
        DebugLogger.AssertNotNull(validObject, "GameObjectはnullではありません");

        GameObject nullObject = null;
        DebugLogger.AssertNotNull(nullObject, "GameObjectがnullです！");

        // 値型テスト
        int? validValue = 123;
        DebugLogger.AssertNotNull(validValue, "int?値が存在します");

        int? nullValue = null;
        DebugLogger.AssertNotNull(nullValue, "int?値がnullです！");
    }

    #endregion

    #region 5. エラーログテスト

    /// <summary>
    /// エラーログテスト
    /// </summary>
    private void TestErrorLogs()
    {
        DebugLogger.LogError("エラーメッセージ（開発ビルドでのみ表示）");
    }

    #endregion

    #region 6. Exceptionテスト

    /// <summary>
    /// 例外処理テスト
    /// </summary>
    private void TestException()
    {
        try
        {
            throw new InvalidOperationException("テスト用例外です");
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex);
        }
    }

    #endregion

    #region 7. タグフィルタリングテスト

    /// <summary>
    /// DebugTagベースフィルタリングテスト
    /// </summary>
    private void TestTagFiltering()
    {
        // 基本タグ（CallerFilePathで自動設定 - ファイル名）
        DebugLogger.Log("基本ログ出力 - タグなし");
        DebugLogger.LogWarning("Warningレベル - タグなし");
        DebugLogger.LogError("Errorレベル - タグなし");

        // DebugTagを使用したログ出力
        DebugLogger.Log("Defaultタグでログ出力", UniDebug.DebugTag.Default);
        DebugLogger.LogWarning("DefaultタグでWarning出力", UniDebug.DebugTag.Default);
        DebugLogger.LogError("DefaultタグでError出力", UniDebug.DebugTag.Default);
    }

    #endregion

    #region 8. マルチスレッドテスト

    /// <summary>
    /// マルチスレッド環境でログテスト
    /// </summary>
    private void TestMultiThread()
    {
        DebugLogger.Log("メインスレッドでログ");

        // バックグラウンドスレッドでログ
        System.Threading.Tasks.Task.Run(() =>
        {
            DebugLogger.Log("バックグラウンドスレッドでログ");
        });
    }

    #endregion

    #region 9. パフォーマンステスト

    /// <summary>
    /// 大量ログパフォーマンステスト
    /// </summary>
    private void TestPerformance()
    {
        var startTime = Time.realtimeSinceStartup;

        for (int i = 0; i < 1000; i++)
        {
            DebugLogger.Log($"パフォーマンステストログ #{i}");
        }

        var endTime = Time.realtimeSinceStartup;
        var elapsed = (endTime - startTime) * 1000f;

        DebugLogger.Log($"1000ログ出力完了 - 所要時間: {elapsed:F2}ms");
    }

    #endregion

    #region 自動テスト

    /// <summary>
    /// 全テストを順次実行
    /// </summary>
    private IEnumerator RunAllTests()
    {
        DebugLogger.Log("=== 全テスト開始 ===");

        DebugLogger.Log("テスト1: 基本ログ");
        TestBasicLogs();
        yield return new WaitForSeconds(1f);

        DebugLogger.Log("テスト2: ログレベル");
        TestLogLevels();
        yield return new WaitForSeconds(1f);

        DebugLogger.Log("テスト3: Assert");
        TestAsserts();
        yield return new WaitForSeconds(1f);

        DebugLogger.Log("テスト4: AssertNotNull");
        TestAssertNotNull();
        yield return new WaitForSeconds(1f);

        DebugLogger.Log("テスト5: エラーログ");
        TestErrorLogs();
        yield return new WaitForSeconds(1f);

        DebugLogger.Log("テスト6: Exception");
        TestException();
        yield return new WaitForSeconds(1f);

        DebugLogger.Log("テスト7: タグフィルタリング");
        TestTagFiltering();
        yield return new WaitForSeconds(1f);

        DebugLogger.Log("テスト8: マルチスレッド");
        TestMultiThread();
        yield return new WaitForSeconds(1f);

        DebugLogger.Log("テスト9: パフォーマンステスト");
        TestPerformance();

        DebugLogger.Log("=== 全テスト完了 ===");
    }

    /// <summary>
    /// 自動テストコルーチン
    /// </summary>
    private IEnumerator AutoTestCoroutine()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            TestBasicLogs();
            yield return new WaitForSeconds(testInterval);

            TestLogLevels();
            yield return new WaitForSeconds(testInterval);
        }
    }

    #endregion

    #region ユーティリティメソッド

    /// <summary>
    /// ConsoleLogManager 情報出力
    /// </summary>
    [ContextMenu("ConsoleLogManager 情報出力")]
    private void PrintConsoleLogManagerInfo()
    {
        if (DebugLogger.GlobalConsoleLogManager != null)
        {
            DebugLogger.Log("[Test]ConsoleLogManagerが有効化されています", UniDebug.DebugTag.hehe);
            DebugLogger.Log("[hehe]ConsoleLogManagerが有効化されています");
            DebugLogger.Log("ConsoleLogManagerが有効化されています");
        }
        else
        {
            DebugLogger.LogWarning("ConsoleLogManagerが無効化されています");
        }
    }

    #endregion
}
