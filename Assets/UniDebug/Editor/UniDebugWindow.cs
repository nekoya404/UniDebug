#if UNITY_EDITOR
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
        private string _newTagName = "";

        // ヘルプテキスト表示フラグ
        private bool _showLogSettingsHelp = false;
        private bool _showLogDisplayHelp = false;
        private bool _showTagFilterHelp = false;

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

            DrawLogSettings();
            DrawCustomTagFilter();
            DrawUtilities();
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
        /// カスタムタグフィルターUI
        /// </summary>
        private void DrawCustomTagFilter()
        {
            EditorGUILayout.Space(15);

            // Tag Filterタイトルとヘルプボタン
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Tag Filter", EditorStyles.boldLabel, GUILayout.Width(65));

                var helpButtonStyle = _showTagFilterHelp ? new GUIStyle(GUI.skin.button)
                {
                    normal = {
                        textColor = Color.green,
                        background = MakeBackgroundTexture(new Color(0.2f, 0.8f, 0.2f, 0.3f))
                    },
                    fontStyle = FontStyle.Bold
                } : GUI.skin.button;

                if (GUILayout.Button("?", helpButtonStyle, GUILayout.Width(25), GUILayout.Height(18)))
                {
                    _showTagFilterHelp = !_showTagFilterHelp;
                }

                GUILayout.FlexibleSpace();
            }

            if (_showTagFilterHelp)
            {
                EditorGUILayout.HelpBox(
                    "登録されたタグのみフィルタリングされます。\n" +
                    "- ON: 該当タグの表示を許可\n" +
                    "- OFF: 該当タグを非表示\n" +
                    "- 登録されていないタグは常に表示されます。\n" +
                    "- タグは大文字小文字を区別しません。（例：Testとtestは同一）",
                    MessageType.Info);
            }

            EditorGUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                // タグ追加UI
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Add New Tag", GUILayout.Width(100));
                    _newTagName = EditorGUILayout.TextField(_newTagName);

                    GUI.enabled = !string.IsNullOrWhiteSpace(_newTagName);
                    if (GUILayout.Button("Add", GUILayout.Width(60)))
                    {
                        var trimmedTag = _newTagName.Trim();
                        var lowerTag = trimmedTag.ToLower();

                        // 大文字小文字を区別せずに重複チェック
                        if (!LogSetting.Tags.ContainsKey(lowerTag))
                        {
                            LogSetting.SetTag(trimmedTag, true);
                            _newTagName = "";
                            Repaint();
                        }
                        else
                        {
                            var existingDisplayName = LogSetting.GetTagDisplayName(lowerTag);
                            EditorUtility.DisplayDialog("タグが既に存在します",
                                $"タグ '{existingDisplayName}' が既にフィルタリストに存在します。\n\n" +
                                $"入力したタグ: '{trimmedTag}'\n" +
                                $"既存のタグ: '{existingDisplayName}'\n\n" +
                                $"タグは大文字小文字を区別しないため、同一のタグとして判定されます。",
                                "確認");
                        }
                    }
                    GUI.enabled = true;
                }

                EditorGUILayout.Space(10);

                // 登録されたタグリスト
                if (LogSetting.Tags.Count > 0)
                {
                    EditorGUILayout.LabelField("Registered Tags", EditorStyles.boldLabel);

                    var tagsList = LogSetting.Tags.ToList();
                    string tagToRemove = null;

                    foreach (var tag in tagsList)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            // タグ名（表示名を使用）
                            var displayName = LogSetting.GetTagDisplayName(tag.Key);
                            EditorGUILayout.LabelField(displayName, GUILayout.Width(150));

                            GUILayout.FlexibleSpace();

                            // ON/OFFトグルボタン
                            var buttonStyle = tag.Value ? new GUIStyle(GUI.skin.button)
                            {
                                normal = { textColor = Color.green },
                                fontStyle = FontStyle.Bold
                            } : GUI.skin.button;

                            if (GUILayout.Button(tag.Value ? "ON" : "OFF", buttonStyle, GUILayout.Width(60)))
                            {
                                LogSetting.SetTag(tag.Key, !tag.Value);
                                Repaint();
                            }

                            // 削除ボタン
                            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
                            if (GUILayout.Button("Remove", GUILayout.Width(70)))
                            {
                                if (EditorUtility.DisplayDialog("Remove Tag",
                                    $"Remove tag '{displayName}' from filter list?",
                                    "Yes", "No"))
                                {
                                    tagToRemove = tag.Key;
                                }
                            }
                            GUI.backgroundColor = Color.white;
                        }
                    }

                    // 削除処理（foreachの外で処理）
                    if (!string.IsNullOrEmpty(tagToRemove))
                    {
                        LogSetting.RemoveTag(tagToRemove);
                        Repaint();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No tags registered. Add a tag above to start filtering.", MessageType.None);
                }
            }

            EditorGUILayout.Space(15);
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
