# Novella Visual Novel Engine

Unity 6 向けのビジュアルノベルエンジンです。JSON/CSV スクリプトでシナリオを記述し、41種のコマンドで演出を制御できます。

## 目次

- [特徴](#特徴)
- [動作要件](#動作要件)
- [インストール](#インストール)
- [クイックスタート](#クイックスタート)
- [コマンド一覧](#コマンド一覧)
- [サンプル](#サンプル)
- [Editorメニュー（Novella）](#editorメニューnovella) — 全30項目の早見表と詳細
- [付録: シーンを手動でセットアップする](#付録-シーンを手動でセットアップする)
- [ライセンス](#ライセンス)

## 特徴

- **JSON/CSV 両対応** — スクリプトをテキストエディタで書ける
- **41種のコマンド** — テキスト表示、立ち絵、背景、BGM/SE/ボイス、選択肢、フラグ、カメラ演出、パーティクル等
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
https://github.com/minto1223/novella-engine.git
```

特定バージョンを指定する場合:

```
https://github.com/minto1223/novella-engine.git#v1.1.0
```

導入後、**続けて `Demo Project` サンプルのインポートが必要です**（シーンはパッケージを追加しただけでは `Assets` に入りません）。手順は下記クイックスタートを参照してください。

## クイックスタート

### 1. Demo Project をインポートする

**パッケージを追加しただけでは `Assets` にシーンが入りません。** まず下記の手順でサンプルをインポートしてください。動作するタイトル画面とゲーム画面が手に入り、そのまま再生できます。

1. Unity エディタで `Window > Package Manager` を開く
2. 左側のパッケージ一覧から `Novella Visual Novel Engine` を選択
3. 右側パネル上部の `Samples` タブをクリック
4. `Demo Project` の行にある `Import` ボタンをクリック
5. `Assets/Samples/Novella Visual Novel Engine/<version>/Demo Project/` にシナリオ・立ち絵・BGM/ボイス一式と `Scenes/TitleScene.unity` / `Scenes/SampleScene.unity` がコピーされる
6. `Scenes/TitleScene.unity` をダブルクリックで開き、そのまま Play するとデモが再生される

> Import はパッケージキャッシュ内のファイルをコピーするだけなので、追加のダウンロードは発生しません。

**独自のゲームを作る場合も、この `TitleScene` / `SampleScene` を複製して中身を差し替えるのが最短です。** タイトル画面（`TitleManager`）とゲーム画面（`NovellaEngine`）は Inspector 上の参照配線が多く、複製して使うほうが確実です。ボタンの追加・削除は `Novella > Button Builder`、各種パネルの再生成は `Novella > Rebuild ...` メニューから行えます。

シーンを一から組みたい場合は[付録: シーンを手動でセットアップする](#付録-シーンを手動でセットアップする)を参照してください。

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

## ボタンデザインのカスタマイズ

ボタンは4状態（通常/ホバー/押下/無効）のスタイルアセットで管理され、自作画像への差し替えもコード不要で行えます。詳細は [Documentation~/button-style-guide.md](Documentation~/button-style-guide.md) を参照してください。

## コマンド一覧

全41コマンドの詳細は [Documentation~/command-reference.md](Documentation~/command-reference.md) を参照してください。

| カテゴリ | コマンド |
|---------|---------|
| テキスト | `say`, `set_mode`, `clear` |
| 立ち絵 | `show_char`, `hide_char`, `move_char` |
| 背景 | `show_bg` |
| カメラ | `zoom`, `pan`, `reset_camera`, `ken_burns`, `stop_ken_burns` |
| 音声 | `play_bgm`, `stop_bgm`, `fade_bgm`, `play_se`, `stop_se`, `play_voice`, `stop_voice`, `set_volume` |
| 演出 | `shake`, `flash`, `fade`, `show_title`, `play_particle`, `stop_particle` |
| フラグ/変数 | `set_flag`, `add_flag`, `calc` |
| 分岐 | `choice`, `jump`, `jump_if`, `jump_unless`, `label` |
| 入力 | `input_text` |
| システム | `wait`, `next_script`, `set_language`, `play_movie`, `stop_movie`, `end` |

### 条件式

`jump_if` / `jump_unless` / `choice.condition` / `calc` では、括弧・論理演算・比較・整数の四則演算を組み合わせた式が書けます。比較や計算の両辺にフラグを置けます。

```json
{ "type": "jump_if", "label": "(rich OR noble) AND !wanted", "target": "vip_route" }
{ "type": "calc", "target": "damage", "value": "=atk * 2 - def" }
```

書式の詳細は [Documentation~/command-reference.md](Documentation~/command-reference.md#条件式の書式) を参照してください。

## サンプル

`Demo Project` には主要30コマンドを通しで確認できるデモシナリオ一式が入っています。

| 内容 | 場所（インポート後） |
|------|--------------------|
| タイトル画面・ゲーム画面のシーン | `Scenes/TitleScene.unity` / `Scenes/SampleScene.unity` |
| デモシナリオ（JSON） | `Resources/Scripts/demo_showcase.json` |
| 立ち絵・背景 | `Resources/Characters/` / `Resources/Backgrounds/` |
| BGM・SE・ボイス | `Resources/Audio/` |
| キャラクター定義・UIテーマ・ボタンスタイル | `Data/` |

インポート手順は[クイックスタート](#1-demo-project-をインポートする)を参照してください。

## Editorメニュー（Novella）

Unity Editor上部のメニューバーに **Novella** メニューが表示されます。各項目はUI要素の自動生成・配線を行うビルダーや、開発支援ツールです。全30項目の詳細は [Documentation~/editor-menu-reference.md](Documentation~/editor-menu-reference.md) を参照してください。

| カテゴリ | メニュー項目 | 用途 |
|---------|------------|------|
| UI構築系 | [Build HUD](Documentation~/editor-menu-reference.md#novella--build-hud) | ミニHUDパネル生成 |
| | [Rebuild Backlog Prefab](Documentation~/editor-menu-reference.md#novella--rebuild-backlog-prefab) | バックログカードPrefab再構築 |
| | [Rebuild Backlog Search Bar](Documentation~/editor-menu-reference.md#novella--rebuild-backlog-search-bar) | バックログ検索バー生成 |
| | [Rebuild Settings Panel](Documentation~/editor-menu-reference.md#novella--rebuild-settings-panel) | 設定パネル再構築 |
| | [Rebuild Title Settings Panel](Documentation~/editor-menu-reference.md#novella--rebuild-title-settings-panel) | タイトル画面用設定パネル構築 |
| | [Build CG Gallery](Documentation~/editor-menu-reference.md#novella--build-cg-gallery) | CGギャラリーパネル生成 |
| | [Build BGM Gallery](Documentation~/editor-menu-reference.md#novella--build-bgm-gallery) | BGM回想パネル生成 |
| | [Build Scene Recollection](Documentation~/editor-menu-reference.md#novella--build-scene-recollection) | シーン回想パネル生成 |
| | [Build Chapter Select](Documentation~/editor-menu-reference.md#novella--build-chapter-select) | チャプター選択パネル生成 |
| | [Create Chapter List](Documentation~/editor-menu-reference.md#novella--create-chapter-list) | ChapterListアセット自動生成 |
| | [Rebuild Save Panels](Documentation~/editor-menu-reference.md#novella--rebuild-save-panels) | セーブ/ロードパネル再構築 |
| | [Build Ending List](Documentation~/editor-menu-reference.md#novella--build-ending-list) | エンディングリストパネル生成 |
| | [Button Builder](Documentation~/editor-menu-reference.md#novella--button-builder) | ボタン自由追加ウィンドウ |
| UI修正・パッチ系 | [Add Save Panel Paging](Documentation~/editor-menu-reference.md#novella--add-save-panel-paging) | セーブ/ロードにページ切替追加 |
| | [Patch Title: Add Reset Button](Documentation~/editor-menu-reference.md#novella--patch-title-add-reset-button) | タイトルにリセットボタン追加 |
| | [Patch Menu: Add Title Button](Documentation~/editor-menu-reference.md#novella--patch-menu-add-title-button) | メニューにタイトル戻るボタン追加 |
| 開発ツール系 | [Script Editor](Documentation~/editor-menu-reference.md#novella--script-editor) | JSONシナリオ編集ウィンドウ |
| | [Flag Debug Window](Documentation~/editor-menu-reference.md#novella--flag-debug-window) | フラグ確認・編集ウィンドウ |
| | [Flowchart](Documentation~/editor-menu-reference.md#novella--flowchart) | 分岐構造の可視化ウィンドウ |
| | [Auto Import Assets](Documentation~/editor-menu-reference.md#novella--auto-import-assets) | アセットインポート設定一括最適化 |
| | [Convert CSV to JSON](Documentation~/editor-menu-reference.md#novella--convert-csv-to-json) | CSV→JSON変換 |
| | [Convert JSON to CSV](Documentation~/editor-menu-reference.md#novella--convert-json-to-csv) | JSON→CSV変換 |
| | [Validate Scripts](Documentation~/editor-menu-reference.md#novella--validate-scripts) | シナリオ一括検証 |
| ビルド系 | [Build Windows](Documentation~/editor-menu-reference.md#novella--build-windows) | Windowsスタンドアロンビルド |
| | [Configure WebGL Settings](Documentation~/editor-menu-reference.md#novella--configure-webgl-settings) | WebGL設定自動化 |
| | [Build WebGL](Documentation~/editor-menu-reference.md#novella--build-webgl) | WebGLビルド |
| | [Configure Android Settings](Documentation~/editor-menu-reference.md#novella--configure-android-settings) | Android設定自動化 |
| | [Build Android](Documentation~/editor-menu-reference.md#novella--build-android) | Androidビルド |
| | [Configure iOS Settings](Documentation~/editor-menu-reference.md#novella--configure-ios-settings) | iOS設定自動化 |
| | [Build iOS](Documentation~/editor-menu-reference.md#novella--build-ios) | iOS用Xcodeプロジェクト生成 |

## 付録: シーンを手動でセットアップする

`Demo Project` のシーンを複製せず、ゲーム画面を一から組む場合の手順です。

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
6. Canvas を作成し、`Novella > Build HUD` などのビルダーメニューで UI を生成

なお**タイトル画面（`TitleManager`）を一から生成するメニューは用意していません。** タイトル画面が必要な場合は `Demo Project` の `TitleScene` を複製してご利用ください。

## ライセンス

MIT License
