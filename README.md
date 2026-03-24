# Novella Visual Novel Engine

Unity 6 向けのビジュアルノベルエンジンです。JSON/CSV スクリプトでシナリオを記述し、43種のコマンドで演出を制御できます。

## 特徴

- **JSON/CSV 両対応** — スクリプトをテキストエディタで書ける
- **43種のコマンド** — テキスト表示、立ち絵、背景、BGM/SE/ボイス、選択肢、フラグ、カメラ演出、パーティクル等
- **セーブ/ロード** — クイックセーブ、オートセーブ、複数スロット対応
- **バックログ** — ボイスリプレイ付き履歴表示
- **ADV/NVL モード** — ウィンドウ表示と全画面テキストを切り替え
- **立ち絵表情差分** — expression パラメータで差分を管理
- **リッチテキスト** — 太字、斜体、カラー、サイズ、ルビ（振り仮名）
- **ギャラリー** — CG回想、BGM鑑賞、エンディング一覧、シーン回想
- **分岐フローマップ** — プレイヤー向けの進行度マップ
- **多言語対応** — JSON ベースのローカライズ
- **Ken Burns エフェクト** — 背景ズーム・パンアニメーション
- **UIテーマ** — ScriptableObject でタイトル画面やボタンを一括カスタマイズ
- **カスタムUI拡張** — インターフェースを実装して独自のメッセージウィンドウや立ち絵表示に差し替え可能
- **モバイル対応** — SafeArea 対応、タッチ操作

## 動作要件

- Unity 6000.0 以降（Unity 6 LTS）
- Universal Render Pipeline (URP) 推奨
- TextMeshPro
- Newtonsoft.Json（com.unity.nuget.newtonsoft-json）

## インストール

### Unity Package Manager (Git URL)

1. Unity エディタで `Window > Package Manager` を開く
2. 左上の `+` ボタン → `Add package from git URL...`
3. 以下の URL を入力:

```
https://github.com/<your-username>/novella-engine.git
```

特定バージョンを指定する場合:

```
https://github.com/<your-username>/novella-engine.git#v1.0.0
```

## クイックスタート

### 1. シーンのセットアップ

1. 新しいシーンを作成
2. 空の GameObject を作成し `NovellaManager` と名前を付ける
3. `NovellaEngine` コンポーネントをアタッチ
4. 同じ GameObject に以下のコンポーネントもアタッチ:
   - `MessageWindowController`
   - `BackgroundController`
   - `CharacterDisplayController`
   - `AudioController`
   - `ChoiceUIController`
   - `BacklogUIController`
   - `MenuUIController`
5. NovellaEngine の各フィールドにコンポーネントを設定

### 2. スクリプトの作成

`Assets/Resources/Scripts/` にJSONファイルを配置:

```json
{
  "title": "My First Story",
  "commands": [
    { "type": "show_bg", "image": "bg_room" },
    { "type": "say", "character": "Alice", "text": "Hello, world!" },
    { "type": "show_char", "character": "alice", "expression": "smile", "position": "center" },
    { "type": "say", "character": "Alice", "text": "Welcome to Novella Engine!" },
    { "type": "end" }
  ]
}
```

### 3. リソースの配置

```
Assets/
  Resources/
    Scripts/       ← シナリオ JSON/CSV
    Backgrounds/   ← 背景画像（Sprite）
    Characters/    ← 立ち絵画像（Sprite）
    Audio/
      BGM/         ← BGM（AudioClip）
      SE/          ← SE（AudioClip）
      Voice/       ← ボイス（AudioClip）
```

## コマンド一覧

全43コマンドの詳細は [Documentation~/command-reference.md](Documentation~/command-reference.md) を参照してください。

| カテゴリ | コマンド |
|---------|---------|
| テキスト | `say`, `ai_say`, `set_mode`, `clear` |
| 立ち絵 | `show_char`, `hide_char`, `move_char` |
| 背景 | `show_bg` |
| カメラ | `zoom`, `pan`, `reset_camera`, `ken_burns`, `stop_ken_burns` |
| 音声 | `play_bgm`, `stop_bgm`, `fade_bgm`, `play_se`, `stop_se`, `play_voice`, `stop_voice`, `set_volume` |
| 演出 | `shake`, `flash`, `fade`, `show_title`, `play_particle`, `stop_particle` |
| フラグ/変数 | `set_flag`, `add_flag`, `calc` |
| 分岐 | `choice`, `jump`, `jump_if`, `jump_unless`, `label` |
| 入力 | `input_text` |
| システム | `wait`, `next_script`, `set_language`, `play_movie`, `stop_movie`, `end` |

## サンプル

Package Manager からインストール後、`Samples` タブの `Demo Project` をインポートすると、全コマンドを使用したデモシナリオが追加されます。

## ライセンス

MIT License
