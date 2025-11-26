#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniDebug.Editor
{
    /// <summary>
    /// TagSettings ScriptableObjectの管理ユーティリティ
    /// </summary>
    [InitializeOnLoad]
    public static class TagSettingsManager
    {
        /// <summary>
        /// Assetsフォルダ内のUniDebug設定パス（Packagesフォルダは読み取り専用のためAssetsに保存）
        /// </summary>
        private const string SettingsFolder = "Assets/UniDebug";
        private const string SettingsPath = "Assets/UniDebug/TagSettings.asset";

        private static TagSettings _cachedSettings;

        /// <summary>
        /// エディタロード時またはPlayMode開始時に設定を自動適用
        /// </summary>
        static TagSettingsManager()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            // エディタ起動時にも設定を適用
            EditorApplication.delayCall += ApplySettingsOnLoad;
        }

        /// <summary>
        /// エディタロード直後に設定を適用
        /// </summary>
        private static void ApplySettingsOnLoad()
        {
            var settings = GetOrCreateSettings();
            settings.ApplySettings();
        }

        /// <summary>
        /// PlayMode状態変更時に設定を適用
        /// </summary>
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // PlayMode開始時に保存された設定を適用
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                var settings = GetOrCreateSettings();
                settings.ApplySettings();
            }
        }

        /// <summary>
        /// 設定を取得（なければ作成）
        /// </summary>
        public static TagSettings GetOrCreateSettings()
        {
            if (_cachedSettings != null)
            {
                return _cachedSettings;
            }

            // Assetsフォルダから検索
            var settings = AssetDatabase.LoadAssetAtPath<TagSettings>(SettingsPath);

            if (settings == null)
            {
                // 新規作成
                settings = CreateSettings();
            }

            _cachedSettings = settings;
            return settings;
        }

        /// <summary>
        /// 設定ファイルを作成
        /// </summary>
        private static TagSettings CreateSettings()
        {
            var settings = ScriptableObject.CreateInstance<TagSettings>();

            // Assets/UniDebugフォルダの確認および作成
            if (!AssetDatabase.IsValidFolder(SettingsFolder))
            {
                AssetDatabase.CreateFolder("Assets", "UniDebug");
            }

            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return settings;
        }

        /// <summary>
        /// 設定を保存
        /// </summary>
        public static void SaveSettings(TagSettings settings)
        {
            if (settings != null)
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                settings.ApplySettings();
            }
        }
    }
}
#endif
