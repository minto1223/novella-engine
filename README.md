# Novella Visual Novel Engine

Unity 6 向けのビジュアルノベルエンジンです。JSON/CSV スクリプトでシナリオを記述し、43種のコマンドで演出を制御できます。

## 目次

- [特徴](#特徴)
- [動作要件](#動作要件)
- [インストール](#インストール)
- [クイックスタート](#クイックスタート)
- [コマンド一覧](#コマンド一覧)
- [サンプル](#サンプル)
- [Editorメニュー（Novella）](#editorメニューnovella) — 全29項目の早見表と詳細
- [ライセンス](#ライセンス)

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
https://github.com/minto1223/novella-engine.git
```

特定バージョンを指定する場合:

```
https://github.com/minto1223/novella-engine.git#v1.1.0
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

## Editorメニュー（Novella）

Unity Editor上部のメニューバーに **Novella** メニューが表示されます。各項目はUI要素の自動生成・配線を行うビルダーや、開発支援ツールです。

### 早見表

| カテゴリ | メニュー項目 | 用途 |
|---------|------------|------|
| UI構築系 | [Build HUD](#novella--build-hud) | ミニHUDパネル生成 |
| | [Rebuild Backlog Prefab](#novella--rebuild-backlog-prefab) | バックログカードPrefab再構築 |
| | [Rebuild Backlog Search Bar](#novella--rebuild-backlog-search-bar) | バックログ検索バー生成 |
| | [Rebuild Settings Panel](#novella--rebuild-settings-panel) | 設定パネル再構築 |
| | [Build CG Gallery](#novella--build-cg-gallery) | CGギャラリーパネル生成 |
| | [Build BGM Gallery](#novella--build-bgm-gallery) | BGM回想パネル生成 |
| | [Build Scene Recollection](#novella--build-scene-recollection) | シーン回想パネル生成 |
| | [Build Chapter Select](#novella--build-chapter-select) | チャプター選択パネル生成 |
| | [Create Chapter List](#novella--create-chapter-list) | ChapterListアセット自動生成 |
| | [Rebuild Save Panels](#novella--rebuild-save-panels) | セーブ/ロードパネル再構築 |
| | [Build Ending List](#novella--build-ending-list) | エンディングリストパネル生成 |
| | [Button Builder](#novella--button-builder) | ボタン自由追加ウィンドウ |
| UI修正・パッチ系 | [Add Save Panel Paging](#novella--add-save-panel-paging) | セーブ/ロードにページ切替追加 |
| | [Patch Title: Add Reset Button](#novella--patch-title-add-reset-button) | タイトルにリセットボタン追加 |
| | [Patch Menu: Add Title Button](#novella--patch-menu-add-title-button) | メニューにタイトル戻るボタン追加 |
| 開発ツール系 | [Script Editor](#novella--script-editor) | JSONシナリオ編集ウィンドウ |
| | [Flag Debug Window](#novella--flag-debug-window) | フラグ確認・編集ウィンドウ |
| | [Flowchart](#novella--flowchart) | 分岐構造の可視化ウィンドウ |
| | [Auto Import Assets](#novella--auto-import-assets) | アセットインポート設定一括最適化 |
| | [Convert CSV to JSON](#novella--convert-csv-to-json) | CSV→JSON変換 |
| | [Convert JSON to CSV](#novella--convert-json-to-csv) | JSON→CSV変換 |
| | [Validate Scripts](#novella--validate-scripts) | シナリオ一括検証 |
| ビルド系 | [Build Windows](#novella--build-windows) | Windowsスタンドアロンビルド |
| | [Configure WebGL Settings](#novella--configure-webgl-settings) | WebGL設定自動化 |
| | [Build WebGL](#novella--build-webgl) | WebGLビルド |
| | [Configure Android Settings](#novella--configure-android-settings) | Android設定自動化 |
| | [Build Android](#novella--build-android) | Androidビルド |
| | [Configure iOS Settings](#novella--configure-ios-settings) | iOS設定自動化 |
| | [Build iOS](#novella--build-ios) | iOS用Xcodeプロジェクト生成 |

### UI構築系

これらのメニューはUIパネルやボタンをコードで自動生成し、必要なコンポーネントの参照を自動配線します。UIが壊れた場合や初期構築時に使用します。

#### Novella > Build HUD

**対象シーン:** SampleScene

SampleScene内の `NovellaCanvas` 直下に、画面右下に配置されるミニHUDパネル（`HUDPanel`）を生成します。

- 8つのボタンを水平配置: QS（クイックセーブ）, QL（クイックロード）, SAVE, LOAD, AUTO, SKIP, LOG（バックログ）, MENU
- `NovellaManager` に `HUDController` コンポーネントを追加し、全ボタンの参照を自動配線
- `MenuUIController` から `SaveUI` / `LoadUI` の参照も自動取得して配線
- 既存の `HUDPanel` がある場合は削除してから再生成

#### Novella > Rebuild Backlog Prefab

**対象シーン:** SampleScene

`Assets/Novella/Prefabs/BacklogEntry.prefab` を再構築します。バックログの各エントリ（カード）のレイアウトを定義するPrefabです。

- カード型レイアウト: 左にテキスト列（キャラ名 + セリフ）、右にジャンプボタン（`>`）
- 半透明の暗い背景、ContentSizeFitterでテキスト量に応じた自動高さ調整
- シーン内の `BacklogUIController` の `EntryContainer` のスペーシング（24px間隔、左右60px余白）も自動更新
- 既存Prefabからフォント設定を引き継ぎ

#### Novella > Rebuild Backlog Search Bar

**対象シーン:** SampleScene

`BacklogPanel` の最上部に検索バー（`SearchBar`）を生成します。

- `TMP_InputField` によるリアルタイム検索入力フィールド（プレースホルダー: 「検索...」）
- 検索アイコン（Q）+ 入力エリアの水平レイアウト
- `BacklogUIController` の `_searchInput` に自動配線
- 既存の `SearchBar` があれば削除してから再生成

#### Novella > Rebuild Settings Panel

**対象シーン:** SampleScene

`NovellaCanvas` 内の `SettingsPanel > SettingsCard` の中身を全削除して再構築します。

- **テキストセクション:** テキスト速度、オート待ち時間、フォントサイズ（スライダー）、未読スキップ、選択肢後スキップ続行、オートセーブ（トグル）
- **サウンドセクション:** BGM音量、SE音量、ボイス音量（スライダー）
- **表示セクション:** ウィンドウ透明度（スライダー）、フルスクリーン（トグル）
- 各セクションに色付きヘッダーと下線
- スクロール対応（項目が多いため `ScrollRect` で構成）
- 「初期化」ボタン（全設定をデフォルトに戻す）と「閉じる」ボタン
- `SettingsUIController` の全スライダー・トグル・ボタン参照を自動配線

#### Novella > Build CG Gallery

**対象シーン:** TitleScene

`TitleCanvas` にCGギャラリーパネル（`GalleryPanel`）を生成します。

- ゲーム中に `show_bg` で表示した背景/CG画像を一覧表示
- スクロール可能なリスト + 閉じるボタン
- `TitleManager` に `CGGalleryUIController` をアタッチし、自動配線
- タイトル画面に「Gallery」ボタンを追加（CG記録がない場合はボタン無効化）

#### Novella > Build BGM Gallery

**対象シーン:** TitleScene

`TitleCanvas` にBGM回想パネル（`BGMGalleryPanel`）を生成します。

- ゲーム中に `play_bgm` で再生したBGMの一覧を表示
- 「Now Playing: ---」ラベルで現在再生中のBGM名を表示
- スクロール可能なリスト + 閉じるボタン
- `TitleManager` に `BGMGalleryUIController` をアタッチし、自動配線
- タイトル画面に「BGM」ボタンを追加（緑系の色、BGM記録がない場合は無効化）

#### Novella > Build Scene Recollection

**対象シーン:** TitleScene

`TitleCanvas` にシーン回想パネル（`RecollectionPanel`）を生成します。

- `end` / `next_script` コマンドでクリア済みのシーンを一覧表示
- 選択するとそのシーンを最初から再プレイ可能
- スクロール可能なリスト + 閉じるボタン
- `TitleManager` に `SceneRecollectionUIController` をアタッチし、自動配線
- タイトル画面に「Scene」ボタンを追加（紫系の色）

#### Novella > Build Chapter Select

**対象シーン:** TitleScene

`TitleCanvas` にチャプター選択パネル（`ChapterSelectPanel`）を生成します。

- `ChapterList` ScriptableObjectに定義されたチャプターを一覧表示
- 前の章をクリアすると次の章が解放される進行管理
- スクロール可能なリスト + 閉じるボタン
- `TitleManager` に `ChapterSelectUIController` をアタッチし、自動配線
- タイトル画面に「Chapter」ボタンを追加（青系の色）
- 実行後、`ChapterList` ScriptableObjectを別途作成して `ChapterSelectUIController` に設定する必要あり（または `Create Chapter List` で自動生成）

#### Novella > Create Chapter List

**対象シーン:** TitleScene

`Resources/Scripts/` 内の `chapter*.json` ファイルを自動検索して `ChapterList` ScriptableObjectを生成します。

- `chapter01.json`, `chapter02.json` ... の命名規則にマッチするファイルを自動取得（`chapter01_csv` 等は除外）
- 各JSONのタイトルフィールドを読み取ってチャプター名に使用
- ファイル名順にソートして `ChapterList.Chapters` 配列を構成
- 生成先: `Assets/Novella/Resources/ChapterList.asset`
- 既存ファイルがある場合は上書き
- シーン内に `ChapterSelectUIController` がある場合は `_chapterList` に自動配線
- 生成後、Projectウィンドウで生成アセットを選択状態にする

> `Build Chapter Select` でUIを構築した後にこのメニューを実行するのが標準手順です。

#### Novella > Rebuild Save Panels

**対象シーン:** SampleScene

`SaveSlot.prefab` を再生成し、`SaveCard` / `LoadCard` をGridLayoutGroup構造に再構築します。

- **SaveSlot.prefab** を自動生成（`Assets/Novella/Prefabs/SaveSlot.prefab`）
  - 2列グリッドレイアウト（912px × 140px / スロット）
  - スロット全体をボタン化（クリックでセーブ/ロード）
  - サムネイル・スロット番号・日時・スクリプト名を表示
  - ホバー/プレス時のカラー変化
- `SaveCard` / `LoadCard` の中身を再構築してScrollRect + GridLayoutGroupに置き換え
- `SaveUIController` に生成したPrefabを自動配線
- 既存の内容は全削除して再生成するため、UI上のカスタマイズは消える点に注意

> セーブUIが壊れた場合や初期構築時に使用します。スロット数変更後はこのメニューで再構築してください。

#### Novella > Build Ending List

**対象シーン:** TitleScene

`TitleCanvas` にエンディングリストパネル（`EndingListPanel`）を生成します。

- `end` コマンドの `label` で指定されたエンディング名の到達記録を一覧表示
- 到達済みエンディングはエンディング名を表示、未到達は「???」で表示
- `_allEndings` リストで全エンディング名を定義可能（定義しない場合は到達済みのみ表示）
- 進捗表示（例: `Endings: 3 / 5`）
- スクロール可能なリスト + 閉じるボタン
- `TitleManager` に `EndingListUIController` をアタッチし、自動配線
- タイトル画面に「Endings」ボタンを追加（橙系の色、到達記録がない場合は無効化）

使い方:
```json
{ "type": "end", "label": "True End" }
{ "type": "end", "label": "Bad End" }
{ "type": "end" }
```
`label` を指定した `end` コマンドが実行されるとエンディングが記録されます。`label` 省略時はエンディングリストに記録されません（通常のシナリオ終了）。

#### Novella > Button Builder

**対象シーン:** TitleScene / SampleScene（自動検知）

タイトル画面またはゲームプレイ画面のボタンを自由に追加できるEditorWindowです。`Novella > Button Builder` で開きます。

- **タブ自動切替:** 現在開いているシーンに応じて `Title` / `Game` タブが自動選択される
- **機能選択:** ドロップダウンで追加するボタンの機能を選択
  - **Title タブ:** New Game / Continue / Quit / Reset / CG Gallery / BGM Gallery / Scene Recollection / Chapter Select / Ending List / Flowchart
  - **Game タブ:** Quick Save / Quick Load / Save / Load / Auto / Skip / Backlog / Menu
- **配置先:** GameObjectをドラッグして指定（空欄の場合は自動で `ButtonRow` / `HUDPanel` に配置）
- **見た目モード（ラジオボタンで切り替え）:**
  - **テキストモード:** 表示文字・文字色・フォントサイズを入力
  - **画像モード:** Spriteをドラッグして指定（テキスト非表示）
- **ボタン色:** カラーピッカーで背景色を指定
- **自動配線:** `TitleManager` または `HUDController` の対応フィールドに自動で配線

### UI修正・パッチ系

既存のUIに追加パーツを取り付けるメニューです。

#### Novella > Add Save Panel Paging

**対象シーン:** SampleScene

`SavePanel` と `LoadPanel` の両方にページ切替バー（`PageBar`）を追加します。

- `< 1/5 >` 形式のページナビゲーション（前ページ・次ページボタン + ページ番号表示）
- 10スロット x 5ページ = 50スロットのページ切替を実現
- 既存の `PageBar` があれば削除してから再生成
- シーンを自動保存

#### Novella > Patch Title: Add Reset Button

**対象シーン:** TitleScene

タイトル画面の `ButtonRow` にデータリセットボタン（`RESET`、赤系）を追加します。

- セーブファイル全消去（通常スロット・クイックセーブ・オートセーブ・サムネイル）
- CGギャラリー / BGM回想 / シーン回想 / エンディング記録を消去
- 既読管理 / 全PlayerPrefsを消去
- ボタン追加後、コンティニュー・ギャラリー等のボタンを自動で無効化
- `TitleManager._resetButton` に自動配線

> Button Builderからも同様のボタンを追加できます。

#### Novella > Patch Menu: Add Title Button

**対象シーン:** SampleScene

ゲーム中メニュー（`NovellaCanvas/MenuPanel/MenuCard`）に「TITLE」ボタンを追加します。

- 赤系の背景色で、Closeボタンの直前に配置
- 既存ボタンのサイズに合わせた高さ
- `MenuUIController` の `_titleButton` に自動配線
- クリックするとタイトル画面（TitleScene）に戻る

### 開発ツール系

#### Novella > Script Editor

Unity Editor内でJSONシナリオを直接編集できる専用ウィンドウを開きます。

- **3ペイン構成:**
  - 左ペイン: `Resources/Scripts/` 内のJSONファイル一覧
  - 中央ペイン: 選択したスクリプトのコマンドリスト（タイプとプレビュー表示）
  - 右ペイン: 選択したコマンドの詳細フィールド編集
- **ドラッグリサイズ:** ペイン間の境界をドラッグして幅調整可能
- **コマンド操作:** 追加（全33種から選択）・複製・削除・上下移動
- **タイプ別フィールド:** コマンドタイプに応じて必要なフィールドのみ表示
- **選択肢編集:** `choice` コマンドのchoices配列をインライン編集（テキスト・ジャンプ先・フラグ・条件）
- **プレビュー:** 「▶ Preview」ボタンで選択中のコマンド位置からSampleSceneをPlay Mode開始。未保存の場合は自動保存してから起動。EditorPrefsでスクリプトパスと開始位置を受け渡し
- **保存:** JSON書き出し → ホットリロードで即座にゲームに反映
- **未保存警告:** 変更がある状態でスクリプトを切り替えると確認ダイアログ表示

#### Novella > Flag Debug Window

プレイモード中にフラグの状態をリアルタイムで確認・操作できるEditorウィンドウを開きます。

- **表示情報:** 現在のスクリプトパス、コマンドインデックス、全フラグの名前と値
- **編集:** フラグの値をテキストフィールドで直接編集可能
- **削除:** 各フラグの横の「X」ボタンで個別削除
- **追加:** ウィンドウ下部でフラグ名と値を指定して新規追加
- **全クリア:** 「Clear All Flags」ボタンで全フラグを一括削除
- **自動更新:** 毎フレーム Repaint するのでリアルタイムに状態が反映
- プレイモード以外では「Play mode only.」と表示

#### Novella > Flowchart

シナリオの分岐構造をノードグラフで視覚化するEditorウィンドウを開きます。

- **スクリプト選択:** ウィンドウ上部のドロップダウンで `Resources/Scripts/` 内のJSONファイルを選択
- **ノード表示:** `label` ごとにブロック分割し、各ブロックをノードとして配置。ブロック内のコマンドをサマリー表示（最大4行 + 残件数）
- **エッジ（接続線）表示:**
  - `jump` → 無条件ジャンプ先へ接続
  - `jump_if` / `jump_unless` → 条件付きジャンプ先へ接続（条件式をラベル表示）
  - `choice` → 各選択肢のジャンプ先へ接続（選択肢テキストをラベル表示）
  - ブロック末尾にジャンプ/end がない場合は次のブロックへフォールスルー接続
- **操作:** ズーム（マウスホイール）、パン（ドラッグ）、ノード選択・移動
- **ダブルクリック:** ノードをダブルクリックするとScript Editorが開き、該当コマンド位置にジャンプ
- **Refresh:** ボタンでスクリプト一覧を再読み込み

> シナリオの全体像を俯瞰でき、ラベルの接続ミスや到達不能なブロックを視覚的に発見できます。

#### Novella > Auto Import Assets

`Resources/` 以下の全アセットのインポート設定を一括で最適化します。新しく画像や音声を追加した後に実行してください。

| フォルダ | 設定内容 |
|---------|---------|
| `Resources/Backgrounds/` | Sprite (Single)、ピボット Center |
| `Resources/Characters/` | Sprite (Single)、ピボット **Bottom Center**（立ち絵の足元基準配置用） |
| `Resources/Audio/BGM/` | AudioClip、**Streaming**読み込み（メモリ節約） |
| `Resources/Audio/SE/` | AudioClip、DecompressOnLoad（低レイテンシ） |
| `Resources/Audio/Voice/` | AudioClip、DecompressOnLoad |
| `Resources/Movies/` | 存在確認のみ（Unityが自動インポート） |

- 既に正しい設定のアセットはスキップされるので、何度実行しても安全
- 変更があったアセットのみ再インポート（SaveAndReimport）
- 完了後、更新件数をConsoleに表示

> 画像を大量に追加した際に1枚ずつ設定する手間を省けます。特にキャラクター画像のピボット設定忘れを防止できます。

#### Novella > Convert CSV to JSON

CSVシナリオファイルをJSON形式に変換します。

- ファイル選択ダイアログで `Assets/Novella/Resources/Scripts/` 内のCSVファイルを指定
- 既存のCSVパーサー（RFC 4180準拠）でパース後、JSONとして出力
- 出力先は元ファイルと同じフォルダに `.json` 拡張子で生成
- 同名ファイルが存在する場合は上書き確認ダイアログを表示
- `choice` 行（連続行でグループ化）も正しく1つの `choice` コマンドに変換

#### Novella > Convert JSON to CSV

JSONシナリオファイルをCSV形式に変換します。

- ファイル選択ダイアログで `Assets/Novella/Resources/Scripts/` 内のJSONファイルを指定
- 出力CSVはヘッダー行付き（`type,character,text,image,...`）
- `choice` コマンドの各選択肢は個別の `choice` 行に展開（CSVの慣例に従う）
- タイトルがある場合は `#title,タイトル名` 行を先頭に出力
- テキスト内のカンマ・改行・ダブルクォートはRFC 4180に準拠して適切にエスケープ

> CSVはExcelやGoogleスプレッドシートで開けるため、大量のセリフ編集やチーム作業時に便利です。CSVで編集→JSONに変換→ゲームで使用というワークフローが可能です。

#### Novella > Validate Scripts

全シナリオファイル（`Assets/Novella/Resources/Scripts/*.json`）を一括チェックし、問題をConsoleに出力します。

**チェック項目（エラー）:**
- コマンドタイプが空
- 未知のコマンドタイプ（typo検出）
- `jump` / `jump_if` / `jump_unless` が存在しないラベルを参照
- `choice` の `target` が未定義のラベルを参照

**チェック項目（警告）:**
- 重複するラベル定義
- 定義されているが参照されていない未使用ラベル
- 存在しない背景画像の参照（`show_bg` → `Resources/Backgrounds/`）
- 存在しないキャラクター画像の参照（`show_char` → `Resources/Characters/`）
- 存在しないBGMの参照（`play_bgm` → `Resources/Audio/BGM/`）
- 存在しないSEの参照（`play_se` → `Resources/Audio/SE/`）
- 存在しないボイスの参照（`play_voice` → `Resources/Audio/Voice/`）

出力例:
```
[Novella Validator] demo.json:5 - Unknown command type: "sya"
[Novella Validator] demo.json: Jump to undefined label: "route_c"
[Novella Validator] demo.json:12 - BGM not found: Audio/BGM/missing_bgm
[Novella Validator] demo.json: Unused label: "unused_branch"
[Novella Validator] All 3 scripts are valid.
```

> シナリオを書き終えた後や、リリース前のチェックとして実行することを推奨します。

### ビルド系

#### Novella > Build Windows

Windowsスタンドアロンビルド（64bit）を実行します。

- PlayerSettingsを自動設定: 解像度1920x1080、ウィンドウモード、リサイズ可
- ビルド対象シーン: TitleScene → SampleScene
- 出力先: `Builds/Windows/NovellaGame.exe`
- ビルド完了後、ファイルサイズ・所要時間をConsoleに出力
- 失敗時はステップごとのエラー詳細をConsoleに表示

#### Novella > Configure WebGL Settings

WebGLビルド用のPlayerSettingsを自動設定します（Build WebGL実行時にも自動で呼ばれます）。

- 解像度: 1920x1080
- 圧縮: Gzip（decompressionFallback有効でサーバー非対応でも動作）
- メモリ: 初期64MB、最大512MB、Geometric成長モード
- Data Caching有効

#### Novella > Build WebGL

WebGLビルドを実行します。

- `Configure WebGL Settings` を自動実行した後、ビルド開始
- ビルド対象シーン: TitleScene → SampleScene
- 出力先: `Builds/WebGL/`
- ビルド完了後、サイズ・所要時間をConsoleに出力
- 失敗時はエラー詳細をConsoleに表示

#### Novella > Configure Android Settings

Androidビルド用のPlayerSettingsを自動設定します。

- 画面向き: 横画面（Landscape Left/Right のみ）
- スクリプトバックエンド: IL2CPP
- ターゲットアーキテクチャ: ARM64
- 最小APIレベル: Android 7.0 (API 24)
- ターゲットAPIレベル: API 33

#### Novella > Build Android

Androidビルド（APK）を実行します。

- `Configure Android Settings` を自動実行した後、ビルド開始
- ビルド対象シーン: TitleScene → SampleScene
- 出力先: `Builds/Android/Novella.apk`
- ビルド完了後、ファイルサイズをConsoleに出力

> Android SDK/NDK が Unity Hub 経由でインストールされている必要があります。

#### Novella > Configure iOS Settings

iOSビルド用のPlayerSettingsを自動設定します。

- 画面向き: 横画面（Landscape Left/Right のみ）
- スクリプトバックエンド: IL2CPP
- ターゲットデバイス: iPhone + iPad

#### Novella > Build iOS

iOS用のXcodeプロジェクトを生成します。

- `Configure iOS Settings` を自動実行した後、ビルド開始
- ビルド対象シーン: TitleScene → SampleScene
- 出力先: `Builds/iOS/`
- 生成されたXcodeプロジェクトをMacで開いて実機ビルドが可能

> macOS + Xcode が必要です。Windows環境ではiOSビルドは実行できません。

## ライセンス

MIT License
