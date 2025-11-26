# UniDebug - Unity用デバッグライブラリ

Unity用のカスタムデバッグログシステムです。

## 📦 インストール

### Package Manager経由でインストール

1. Unity EditorでWindow > Package Managerを開く
2. 左上の「+」ボタンをクリック
3. 「Add package from git URL...」を選択
4. 以下のURLを入力:
```
https://github.com/nekoya404/UniDebug.git?path=Packages/UniDebug
```

バージョンを指定する場合:
```
https://github.com/nekoya404/UniDebug.git?path=Packages/UniDebug#1.0.0
```

### または manifest.json に直接追加

`Packages/manifest.json`を開いて以下を追加:
```json
{
  "dependencies": {
    "com.nekoya404.unidebug": "https://github.com/nekoya404/UniDebug.git?path=Packages/UniDebug"
  }
}
```

バージョンを指定する場合:
```json
{
  "dependencies": {
    "com.nekoya404.unidebug": "https://github.com/nekoya404/UniDebug.git?path=Packages/UniDebug#1.0.0"
  }
}
```

## 🎮 サンプルプロジェクト

このリポジトリには、UniDebugライブラリの使い方を示すサンプルUnityプロジェクトが含まれています。

1. リポジトリ全体をクローン:
   ```bash
   git clone https://github.com/nekoya404/UniDebug.git
   ```
2. クローンしたリポジトリをUnityプロジェクトとして開く（リポジトリ自体がUnityプロジェクトです）
3. `Assets/UniDebug_Examples/` にあるサンプルシーンを開いて実行
```

## 📖 使用方法

エディターで`UniDebug/Debug Window`メニューから設定ウィンドウを開いて設定できます。

### 基本的なログ出力

```csharp
DebugLogger.Log("デバッグメッセージ");
DebugLogger.LogWarning("警告メッセージ");
DebugLogger.LogError("エラーメッセージ");
```

### カスタムタグを使用

```csharp
DebugLogger.Log("[Battle] 戦闘開始");
DebugLogger.Log("戦闘開始", "Battle");
```

### Assertion

```csharp
DebugLogger.Assert(player != null, "プレイヤーがnullです");
DebugLogger.AssertNotNull(gameObject, "GameObjectがnullです");
```

## 📄 ライセンス

MIT License
