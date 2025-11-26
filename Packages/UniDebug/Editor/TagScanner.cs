#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UniDebug.Editor
{
    /// <summary>
    /// 프로젝트 내 스크립트에서 [태그] 패턴을 자동으로 스캔하는 유틸리티
    /// </summary>
    public static class TagScanner
    {
        // Debug.Log, DebugLogger.Log 등에서 "[Tag]" 패턴을 찾는 정규식
        // 문자열 리터럴 내의 [태그] 패턴 검색
        private static readonly Regex TagPattern = new Regex(
            @"(?:Debug\.Log|Debug\.LogWarning|Debug\.LogError|DebugLogger\.Log|DebugLogger\.LogWarning|DebugLogger\.LogError)\s*\(\s*""?\[([A-Za-z_][A-Za-z0-9_]*)\]",
            RegexOptions.Compiled | RegexOptions.Multiline
        );

        // $"[Tag]..." 형식의 보간 문자열도 검색
        private static readonly Regex InterpolatedTagPattern = new Regex(
            @"(?:Debug\.Log|Debug\.LogWarning|Debug\.LogError|DebugLogger\.Log|DebugLogger\.LogWarning|DebugLogger\.LogError)\s*\(\s*\$""?\[([A-Za-z_][A-Za-z0-9_]*)\]",
            RegexOptions.Compiled | RegexOptions.Multiline
        );

        /// <summary>
        /// 프로젝트 내 모든 스크립트에서 [태그] 패턴을 스캔
        /// </summary>
        /// <returns>발견된 태그 목록 (중복 제거됨)</returns>
        public static List<string> ScanProjectForTags()
        {
            var foundTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 현재 등록된 태그 목록 가져오기
            var existingTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DebugTag tag in Enum.GetValues(typeof(DebugTag)))
            {
                existingTags.Add(tag.ToString());
            }

            // Assets 폴더 내 모든 .cs 파일 검색
            var scriptGuids = AssetDatabase.FindAssets("t:Script", new[] { "Assets" });
            var totalFiles = scriptGuids.Length;
            var processedFiles = 0;

            try
            {
                foreach (var guid in scriptGuids)
                {
                    processedFiles++;

                    // 진행률 표시
                    if (processedFiles % 50 == 0)
                    {
                        EditorUtility.DisplayProgressBar(
                            "Scanning for Tags",
                            $"Scanning scripts... ({processedFiles}/{totalFiles})",
                            (float)processedFiles / totalFiles
                        );
                    }

                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!path.EndsWith(".cs"))
                    {
                        continue;
                    }

                    // UniDebug 패키지 자체는 스킵
                    if (path.Contains("UniDebug") && path.Contains("Packages"))
                    {
                        continue;
                    }

                    try
                    {
                        var content = File.ReadAllText(path);
                        ScanContent(content, foundTags);
                    }
                    catch (Exception)
                    {
                        // 파일 읽기 실패 시 무시
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            // 이미 등록된 태그 제외
            var newTags = foundTags
                .Where(tag => !existingTags.Contains(tag))
                .OrderBy(tag => tag)
                .ToList();

            return newTags;
        }

        /// <summary>
        /// 스크립트 내용에서 태그 패턴 검색
        /// </summary>
        private static void ScanContent(string content, HashSet<string> foundTags)
        {
            // 일반 문자열에서 검색
            var matches = TagPattern.Matches(content);
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    var tagName = match.Groups[1].Value;
                    if (IsValidTagName(tagName))
                    {
                        foundTags.Add(tagName);
                    }
                }
            }

            // 보간 문자열에서 검색
            var interpolatedMatches = InterpolatedTagPattern.Matches(content);
            foreach (Match match in interpolatedMatches)
            {
                if (match.Groups.Count > 1)
                {
                    var tagName = match.Groups[1].Value;
                    if (IsValidTagName(tagName))
                    {
                        foundTags.Add(tagName);
                    }
                }
            }
        }

        /// <summary>
        /// 유효한 태그 이름인지 검사
        /// </summary>
        private static bool IsValidTagName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 50)
            {
                return false;
            }

            // C# 식별자 규칙 검사
            if (!char.IsLetter(name[0]) && name[0] != '_')
            {
                return false;
            }

            for (int i = 1; i < name.Length; i++)
            {
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_')
                {
                    return false;
                }
            }

            // 예약어 또는 일반적인 단어 제외
            var excludedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "if", "else", "for", "while", "do", "switch", "case",
                "break", "continue", "return", "null", "true", "false",
                "int", "float", "string", "bool", "void", "var",
                "public", "private", "protected", "internal", "static",
                "class", "struct", "enum", "interface", "namespace"
            };

            return !excludedWords.Contains(name);
        }

        /// <summary>
        /// 발견된 태그들을 DebugTag enum에 추가
        /// </summary>
        public static void AddFoundTags(List<string> newTags)
        {
            if (newTags == null || newTags.Count == 0)
            {
                return;
            }

            // 현재 태그 목록 가져오기
            var currentTags = new List<string>();
            foreach (DebugTag tag in Enum.GetValues(typeof(DebugTag)))
            {
                currentTags.Add(tag.ToString());
            }

            // Default가 없으면 추가
            if (currentTags.Count == 0 || currentTags[0] != "Default")
            {
                currentTags.Insert(0, "Default");
            }

            // 새 태그 추가 (중복 체크)
            var existingSet = new HashSet<string>(currentTags, StringComparer.OrdinalIgnoreCase);
            foreach (var tag in newTags)
            {
                if (!existingSet.Contains(tag))
                {
                    currentTags.Add(tag);
                    existingSet.Add(tag);
                }
            }

            // DebugTag.cs 파일 재생성
            GenerateDebugTagFile(currentTags);
        }

        /// <summary>
        /// DebugTag.cs 파일 생성
        /// </summary>
        private static void GenerateDebugTagFile(List<string> tags)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// This script is auto-generated by UniDebug editor window.");
            sb.AppendLine("// Do not modify directly!");
            sb.AppendLine("// To change tags, click 'Tags...' button in UniDebug/Debug Window.");
            sb.AppendLine();
            sb.AppendLine("namespace UniDebug");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Debug tag enum for log filtering");
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

            // 파일 경로 찾기
            var guids = AssetDatabase.FindAssets("DebugTag t:script");
            string filePath = null;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("DebugTag.cs") && path.Contains("UniDebug"))
                {
                    filePath = path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(filePath))
            {
                filePath = "Packages/UniDebug/Runtime/Tag/DebugTag.cs";
            }

            // 파일 쓰기
            var fullPath = Path.GetFullPath(filePath);
            File.WriteAllText(fullPath, sb.ToString());

            AssetDatabase.Refresh();
            Debug.Log($"DebugTag.cs updated. Added {tags.Count - 1} tags (excluding Default).");
        }
    }
}
#endif
