# UniDebug - Unity用デバッグライブラリ

Unity用のカスタムデバッグログシステムです。

## 📦 インストール

### Package Manager経由でインストール

1. Unity EditorでWindow > Package Managerを開く
2. 左上の「+」ボタンをクリック
3. 「Add package from git URL...」を選択
4. 以下のURLを入力:
```
https://github.com/nekoya404/UniDebug.git?path=Assets/UniDebug
```

### または manifest.json に直接追加

`Packages/manifest.json`を開いて以下を追加:
```json
{
  "dependencies": {
    "com.nekoya404.unidebug": "https://github.com/nekoya404/UniDebug.git?path=Assets/UniDebug"
  }
}
```
