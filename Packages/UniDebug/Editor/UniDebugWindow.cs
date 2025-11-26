#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UniDebug;
using UniDebug.Utils;
using UnityEditor;
using UnityEngine;

namespace UniDebug.Editor
{
    /// <summary>
    /// UniDebugライブラリ用エディタウィンドウ
    /// ログ設定と基本ユーティリティ機能を提供
    /// </summary>
    public class UniDebugWindow : EditorWindow
    {
        [MenuItem("UniDebug/Debug Window")]
        private static void ShowWindow()
        {
            var window = GetWindow<UniDebugWindow>("UniDebug");
            window.Show();
        }

        private Vector2 _scrollPosition;

        // ヘルプテキスト表示フラグ
        private bool _showLogSettingsHelp = false;
        private bool _showLogDisplayHelp = false;

        // TagSettings 캐시
        private TagSettings _tagSettings;

        private void OnEnable()
        {
            _tagSettings = TagSettingsManager.GetOrCreateSettings();
        }

        private void OnDestroy()
        {
            // UniDebugWindow가 닫힐 때 EditTagWindow도 함께 닫기
            EditTagWindow.CloseIfOpen();
        }

        /// <summary>
        /// 背景テクスチャ生成ヘルパーメソッド
        /// </summary>
        private Texture2D MakeBackgroundTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void OnGUI()
        {
            using var scrollScope = new EditorGUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scrollScope.scrollPosition;

            DrawTagFilterSection();
            DrawLogSettings();
            DrawUtilities();
        }

        /// <summary>
        /// 태그 필터 섹션 (Console Filter Pro 스타일)
        /// </summary>
        private void DrawTagFilterSection()
        {
            EditorGUILayout.Space(5);

            // displayed 드롭다운
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("displayed", GUILayout.Width(60));

                if (_tagSettings != null)
                {
                    var tagNames = Enum.GetNames(typeof(DebugTag));

                    EditorGUI.BeginChangeCheck();
                    // MaskField는 0부터 시작하므로 >> 1 로 조정
                    int currentMask = _tagSettings.DisplayedTags.Value >> 1;
                    int newMask = EditorGUILayout.MaskField(currentMask, tagNames);

                    if (EditorGUI.EndChangeCheck())
                    {
                        _tagSettings.DisplayedTags = new TagMask(newMask << 1);
                        TagSettingsManager.SaveSettings(_tagSettings);
                    }
                }
            }

            // passed / withheld 카운터
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"passed: {TagFilterInfo.PassedCount}", GUILayout.Width(100));
                EditorGUILayout.LabelField($"withheld: {TagFilterInfo.WithheldCount}", GUILayout.Width(100));
            }

            // Tags... 버튼
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Tags...", GUILayout.Width(80)))
                {
                    EditTagWindow.ShowWindow();
                }
            }

            EditorGUILayout.Space(10);

            // 구분선
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

            EditorGUILayout.Space(5);
        }

        /// <summary>
        /// ログ設定UI
        /// </summary>
        private void DrawLogSettings()
        {
            // タイトルとヘルプボタン
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Log", EditorStyles.boldLabel, GUILayout.Width(30));

                var helpButtonStyle = _showLogSettingsHelp ? new GUIStyle(GUI.skin.button)
                {
                    normal = {
                        textColor = Color.green,
                        background = MakeBackgroundTexture(new Color(0.2f, 0.8f, 0.2f, 0.3f))
                    },
                    fontStyle = FontStyle.Bold
                } : GUI.skin.button;

                if (GUILayout.Button("?", helpButtonStyle, GUILayout.Width(25), GUILayout.Height(18)))
                {
                    _showLogSettingsHelp = !_showLogSettingsHelp;
                }

                GUILayout.FlexibleSpace();
            }

            if (_showLogSettingsHelp)
            {
                EditorGUILayout.HelpBox(
                    "基本的なログ動作を設定します。\n" +
                    "- Pause on Assert: Assert失敗時にエディタを一時停止するかどうか\n" +
                    "- Stack Trace: ログに含まれるスタックトレースの詳細レベル\n" +
                    "  • None: スタックトレースなし\n" +
                    "  • Script Only: スクリプトコードのみ表示（推奨）\n" +
                    "  • Full: エンジン内部コードまですべて表示",
                    MessageType.Info);
            }

            EditorGUILayout.Space(5);

            // Assert発生時の一時停止
            LogSetting.EnableAssertionStop = EditorGUILayout.Toggle("Pause on Assert", LogSetting.EnableAssertionStop);

            // スタックトレース設定
            var stackTraceLogType = (StackTraceLogType)EditorGUILayout.EnumPopup("Stack Trace", DebugLogger.StackTrackLogType);
            if (stackTraceLogType != DebugLogger.StackTrackLogType)
            {
                DebugLogger.StackTrackLogType = stackTraceLogType;
            }

            EditorGUILayout.Space(10);

            // Log Displayタイトルとヘルプボタン
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Log Display", EditorStyles.boldLabel, GUILayout.Width(80));

                var helpButtonStyle = _showLogDisplayHelp ? new GUIStyle(GUI.skin.button)
                {
                    normal = {
                        textColor = Color.green,
                        background = MakeBackgroundTexture(new Color(0.2f, 0.8f, 0.2f, 0.3f))
                    },
                    fontStyle = FontStyle.Bold
                } : GUI.skin.button;

                if (GUILayout.Button("?", helpButtonStyle, GUILayout.Width(25), GUILayout.Height(18)))
                {
                    _showLogDisplayHelp = !_showLogDisplayHelp;
                }

                GUILayout.FlexibleSpace();
            }

            if (_showLogDisplayHelp)
            {
                EditorGUILayout.HelpBox(
                    "ログに表示される情報を設定します。\n" +
                    "- Tag: ログメッセージにカスタムタグ（[タグ名]）を表示するかどうか\n" +
                    "- Tracer: ログが発生したファイルと行番号を表示するかどうか\n",
                    MessageType.Info);
            }
            using (new EditorGUILayout.VerticalScope("box"))
            {
                // ヘッダー行
                DrawTagDisplayHeader();

                var (debugTag, debugTracer) = DrawTagDisplayRow("Debug", LogSetting.ShowDebugTag, LogSetting.ShowDebugTracer);
                LogSetting.ShowDebugTag = debugTag;
                LogSetting.ShowDebugTracer = debugTracer;

                var (warningTag, warningTracer) = DrawTagDisplayRow("Warning", LogSetting.ShowWarningTag, LogSetting.ShowWarningTracer);
                LogSetting.ShowWarningTag = warningTag;
                LogSetting.ShowWarningTracer = warningTracer;

                var (errorTag, errorTracer) = DrawTagDisplayRow("Error", LogSetting.ShowErrorTag, LogSetting.ShowErrorTracer);
                LogSetting.ShowErrorTag = errorTag;
                LogSetting.ShowErrorTracer = errorTracer;
            }

            EditorGUILayout.Space(15);
        }

        /// <summary>
        /// タグ表示ヘッダー描画
        /// </summary>
        private void DrawTagDisplayHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // 空白スペース（ラベル領域）
                EditorGUILayout.LabelField("", GUILayout.Width(80));

                GUILayout.FlexibleSpace();

                // タグヘッダー
                var headerStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };
                EditorGUILayout.LabelField("Tag", headerStyle, GUILayout.Width(80));

                // トレーサーヘッダー
                EditorGUILayout.LabelField("Tracer", headerStyle, GUILayout.Width(80));
            }
        }

        /// <summary>
        /// タグ表示行描画
        /// </summary>
        private (bool tagEnabled, bool tracerEnabled) DrawTagDisplayRow(string label, bool tagEnabled, bool tracerEnabled)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // ラベル（左揃え、固定幅）
                EditorGUILayout.LabelField(label, GUILayout.Width(80));

                GUILayout.FlexibleSpace();

                // タグON/OFFボタン
                var tagButtonStyle = tagEnabled ? new GUIStyle(GUI.skin.button)
                {
                    normal = { textColor = Color.green },
                    fontStyle = FontStyle.Bold
                } : GUI.skin.button;

                if (GUILayout.Button(tagEnabled ? "ON" : "OFF", tagButtonStyle, GUILayout.Width(80)))
                {
                    tagEnabled = !tagEnabled;
                }

                // トレーサーON/OFFボタン
                var tracerButtonStyle = tracerEnabled ? new GUIStyle(GUI.skin.button)
                {
                    normal = { textColor = Color.green },
                    fontStyle = FontStyle.Bold
                } : GUI.skin.button;

                if (GUILayout.Button(tracerEnabled ? "ON" : "OFF", tracerButtonStyle, GUILayout.Width(80)))
                {
                    tracerEnabled = !tracerEnabled;
                }
            }

            return (tagEnabled, tracerEnabled);
        }

        /// <summary>
        /// ユーティリティ機能UI
        /// </summary>
        private void DrawUtilities()
        {
            EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // ローカルデータ全削除
            GUI.backgroundColor = new Color(1f, 0.7f, 0.7f); // 警告色
            if (GUILayout.Button("Reset UniDebug Settings"))
            {
                if (EditorUtility.DisplayDialog(
                    "Reset UniDebug Settings",
                    "Are you sure you want to reset all UniDebug settings?\nThis will not affect other PlayerPrefs data.",
                    "Yes, Reset",
                    "Cancel"))
                {
                    LogSetting.ClearSavedSettings();
                    DebugLogger.ClearSavedSettings();
                    Debug.Log("UniDebug settings have been reset.");
                    Repaint(); // UI強制リフレッシュ
                }
            }
            GUI.backgroundColor = Color.white;
        }
    }
}
#endif
