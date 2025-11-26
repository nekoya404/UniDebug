using UnityEngine;

namespace UniDebug
{
    /// <summary>
    /// タグ設定を保存するScriptableObject
    /// </summary>
    public class TagSettings : ScriptableObject
    {
        /// <summary>
        /// 設定ファイル名
        /// </summary>
        public const string AssetName = "UniDebugTagSettings";

        /// <summary>
        /// パッケージ内Dataフォルダの相対パス
        /// </summary>
        public const string DataFolderPath = "Data";

        [SerializeField]
        private TagMask _displayedTags = TagMask.AllTags;

        /// <summary>
        /// 表示するタグマスク（変更時に即座にランタイムに適用）
        /// </summary>
        public TagMask DisplayedTags
        {
            get => _displayedTags;
            set
            {
                _displayedTags = value;
                // 値変更時に即座にランタイムに適用
                TagFilterInfo.DisplayedTags = value;
            }
        }

        /// <summary>
        /// 設定をランタイムに適用
        /// </summary>
        public void ApplySettings()
        {
            TagFilterInfo.DisplayedTags = _displayedTags;
        }
    }
}
