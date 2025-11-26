#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;

namespace UniDebug.Editor
{
    /// <summary>
    /// UniDebug 패키지 초기화 - DebugTag.cs 및 asmdef 자동 생성
    /// </summary>
    [InitializeOnLoad]
    internal static class UniDebugInitializer
    {
        private const string UniDebugFolder = "Assets/UniDebug";
        private const string DebugTagPath = "Assets/UniDebug/DebugTag.cs";
        private const string AsmdefPath = "Assets/UniDebug/UniDebug.Tags.asmdef";

        static UniDebugInitializer()
        {
            // DebugTag.cs가 프로젝트 어디에도 없으면 기본 파일 생성
            EditorApplication.delayCall += EnsureDebugTagExists;
        }

        private static void EnsureDebugTagExists()
        {
            // 이미 Assets 폴더에 DebugTag.cs가 있는지 확인
            var guids = AssetDatabase.FindAssets("DebugTag t:script");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("DebugTag.cs") && path.StartsWith("Assets/"))
                {
                    // 이미 존재함 - asmdef도 있는지 확인
                    EnsureAsmdefExists();
                    return;
                }
            }

            // 없으면 기본 DebugTag.cs 및 asmdef 생성
            GenerateDefaultDebugTag();
            GenerateAsmdef();
        }

        private static void EnsureAsmdefExists()
        {
            if (!File.Exists(Path.GetFullPath(AsmdefPath)))
            {
                GenerateAsmdef();
            }
        }

        private static void GenerateDefaultDebugTag()
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
            sb.AppendLine("        Default = 1,");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            // 디렉토리 생성
            var fullPath = Path.GetFullPath(DebugTagPath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 파일 쓰기
            File.WriteAllText(fullPath, sb.ToString());
        }

        private static void GenerateAsmdef()
        {
            var asmdefContent = @"{
    ""name"": ""UniDebug.Tags"",
    ""rootNamespace"": ""UniDebug"",
    ""references"": [],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
            // 디렉토리 생성
            var fullPath = Path.GetFullPath(AsmdefPath);
            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, asmdefContent);
            AssetDatabase.Refresh();
        }
    }
}
#endif
