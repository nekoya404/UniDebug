#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UniDebug.Editor
{
    /// <summary>
    /// 태그를 추가/삭제하는 팝업 윈도우
    /// </summary>
    internal class EditTagWindow : EditorWindow
    {
        // 현재 열린 EditTagWindow 인스턴스 (UniDebugWindow와 함께 닫히도록 추적)
        private static EditTagWindow _currentInstance;

        [SerializeField] private List<string> _tags = new List<string>();

        private const float OutlineThickness = 2;
        private const float Margin = OutlineThickness + 10;
        private const float ButtonHeight = 25;
        private static readonly Color OutlineColor = new Color(0.1f, 0.1f, 0.1f);

        private Vector2 _scrollPosition;

        /// <summary>
        /// 팝업 윈도우 생성
        /// </summary>
        public static void ShowWindow()
        {
            // 이미 열린 윈도우가 있으면 닫기 (중복 방지)
            if (_currentInstance != null)
            {
                _currentInstance.Close();
                _currentInstance = null;
            }

            var window = CreateInstance<EditTagWindow>();
            window.titleContent = new GUIContent("Edit Tags");
            window._tags = ReadCurrentTags();

            // 윈도우 크기 설정
            var rect = new Rect
            {
                width = 300,
                height = 450,
                position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition) + new Vector2(20, -225)
            };
            window.position = rect;
            window.ShowPopup();

            // 현재 인스턴스 등록
            _currentInstance = window;
        }

        /// <summary>
        /// 열려있는 EditTagWindow 닫기 (UniDebugWindow가 닫힐 때 호출됨)
        /// </summary>
        public static void CloseIfOpen()
        {
            if (_currentInstance != null)
            {
                _currentInstance.Close();
                _currentInstance = null;
            }
        }

        private void OnDestroy()
        {
            // 인스턴스 참조 해제
            if (_currentInstance == this)
            {
                _currentInstance = null;
            }
        }

        private void OnGUI()
        {
            // 테두리 그리기
            DrawOutline();

            var contentRect = new Rect(Margin, Margin, position.width - 2 * Margin, position.height - 2 * Margin);

            GUILayout.BeginArea(contentRect);

            EditorGUILayout.LabelField("DebugTag List", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 스크롤 영역
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(position.height - 160));

            for (int i = 0; i < _tags.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"Element {i}", GUILayout.Width(70));
                    _tags[i] = EditorGUILayout.TextField(_tags[i]);

                    // Default 태그는 삭제 불가
                    GUI.enabled = i > 0;
                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        _tags.RemoveAt(i);
                        i--;
                    }
                    GUI.enabled = true;
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(5);

            // 추가/삭제 버튼
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    _tags.Add($"NewTag{_tags.Count}");
                }
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    if (_tags.Count > 1) // Default는 유지
                    {
                        _tags.RemoveAt(_tags.Count - 1);
                    }
                }
            }

            EditorGUILayout.Space(5);

            // Tag Scan all Script 버튼
            if (GUILayout.Button("Tag Scan all Script", GUILayout.Height(ButtonHeight)))
            {
                var foundTags = TagScanner.ScanProjectForTags();
                if (foundTags.Count > 0)
                {
                    var message = $"Found {foundTags.Count} tag(s):\n" + string.Join(", ", foundTags);
                    if (EditorUtility.DisplayDialog("Tag Scan Result",
                        message + "\n\nDo you want to add these tags?",
                        "Add Tags", "Cancel"))
                    {
                        // 현재 태그 리스트에 추가
                        var existingSet = new HashSet<string>(_tags, StringComparer.OrdinalIgnoreCase);
                        foreach (var tag in foundTags)
                        {
                            if (!existingSet.Contains(tag))
                            {
                                _tags.Add(tag);
                                existingSet.Add(tag);
                            }
                        }
                        Repaint();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Tag Scan Result",
                        "No [Tag] patterns found in project scripts.", "OK");
                }
            }

            EditorGUILayout.Space(5);

            // Apply button
            if (GUILayout.Button("Apply Tags", GUILayout.Height(ButtonHeight)))
            {
                ApplyTags();
                Close();
            }

            GUILayout.EndArea();
        }

        private void DrawOutline()
        {
            // 왼쪽
            EditorGUI.DrawRect(new Rect(0, 0, OutlineThickness, position.height), OutlineColor);
            // 오른쪽
            EditorGUI.DrawRect(new Rect(position.width - OutlineThickness, 0, OutlineThickness, position.height), OutlineColor);
            // 위
            EditorGUI.DrawRect(new Rect(0, 0, position.width, OutlineThickness), OutlineColor);
            // 아래
            EditorGUI.DrawRect(new Rect(0, position.height - OutlineThickness, position.width, OutlineThickness), OutlineColor);
        }

        /// <summary>
        /// 현재 DebugTag.cs에서 태그 읽기
        /// </summary>
        private static List<string> ReadCurrentTags()
        {
            var tags = new List<string>();

            foreach (DebugTag tag in Enum.GetValues(typeof(DebugTag)))
            {
                tags.Add(tag.ToString());
            }

            // Default가 없으면 추가
            if (tags.Count == 0 || tags[0] != "Default")
            {
                tags.Insert(0, "Default");
            }

            return tags;
        }

        /// <summary>
        /// 태그 적용 (DebugTag.cs 파일 재생성)
        /// </summary>
        private void ApplyTags()
        {
            // 태그 이름 유효성 검사 및 중복 제거
            var validTags = new List<string>();
            var seenTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var tag in _tags)
            {
                var trimmedTag = tag.Trim();
                if (string.IsNullOrEmpty(trimmedTag))
                {
                    continue;
                }

                // C# 식별자 유효성 검사
                if (!IsValidIdentifier(trimmedTag))
                {
                    Debug.LogWarning($"'{trimmedTag}'는 유효한 C# 식별자가 아닙니다. 건너뜁니다.");
                    continue;
                }

                // 중복 검사
                if (seenTags.Contains(trimmedTag))
                {
                    Debug.LogWarning($"'{trimmedTag}' 태그가 중복됩니다. 건너뜁니다.");
                    continue;
                }

                seenTags.Add(trimmedTag);
                validTags.Add(trimmedTag);
            }

            // Default가 맨 앞에 있도록 보장
            if (validTags.Count == 0 || validTags[0] != "Default")
            {
                validTags.Remove("Default");
                validTags.Insert(0, "Default");
            }

            // DebugTag.cs 파일 생성
            GenerateDebugTagFile(validTags);
        }

        private static bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            // 첫 글자는 문자 또는 언더스코어
            if (!char.IsLetter(name[0]) && name[0] != '_')
            {
                return false;
            }

            // 나머지는 문자, 숫자 또는 언더스코어
            for (int i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// DebugTag.cs 파일 생성
        /// </summary>
        private static void GenerateDebugTagFile(List<string> tags)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// 이 스크립트는 UniDebug 에디터 윈도우에서 자동 생성됩니다.");
            sb.AppendLine("// 직접 수정하지 마세요!");
            sb.AppendLine("// 태그를 변경하려면 UniDebug/Debug Window에서 'Tags...' 버튼을 클릭하세요.");
            sb.AppendLine();
            sb.AppendLine("namespace UniDebug");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// 디버그 로그에 사용할 수 있는 태그 enum");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public enum DebugTag");
            sb.AppendLine("    {");

            for (int i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                var enumValue = i + 1; // 1부터 시작
                sb.AppendLine($"        {tag} = {enumValue},");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            // 파일 경로 찾기: Assets 폴더 내의 기존 DebugTag.cs 우선 검색
            var guids = AssetDatabase.FindAssets("DebugTag t:script");
            string filePath = null;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                // Assets 폴더 내의 DebugTag.cs를 우선 사용 (패키지 폴더는 읽기 전용일 수 있음)
                if (path.EndsWith("DebugTag.cs") && path.StartsWith("Assets/"))
                {
                    filePath = path;
                    break;
                }
            }

            // Assets 폴더에 없으면 새로 생성
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = "Assets/UniDebug/DebugTag.cs";
            }

            // 디렉토리가 없으면 생성
            var fullPath = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 파일 쓰기
            File.WriteAllText(fullPath, sb.ToString());

            AssetDatabase.Refresh();
            Debug.Log($"DebugTag.cs가 업데이트되었습니다. 경로: {filePath}, 태그 수: {tags.Count}");
        }
    }
}
#endif
