# Novella コマンドリファレンス

シナリオJSON/CSVの `commands` 配列に記述するコマンド一覧です。全43種。

---

## 目次

### テキスト・表示
- [say](#say--セリフ表示) / [ai_say](#ai_say--ai生成セリフ) / [show_title](#show_title--章タイトル表示)

### 選択肢・分岐
- [choice](#choice--選択肢表示) / [label](#label--ラベル定義) / [jump](#jump--無条件ジャンプ) / [jump_if](#jump_if--条件付きジャンプ真) / [jump_unless](#jump_unless--条件付きジャンプ偽)

### フラグ
- [set_flag](#set_flag--フラグセット) / [add_flag](#add_flag--フラグ加算)

### 背景・立ち絵
- [show_bg](#show_bg--背景表示) / [show_char](#show_char--立ち絵表示) / [hide_char](#hide_char--立ち絵非表示) / [move_char](#move_char--立ち絵移動)

### サウンド
- [play_bgm](#play_bgm--bgm再生) / [stop_bgm](#stop_bgm--bgm停止) / [fade_bgm](#fade_bgm--bgm音量フェード) / [play_se](#play_se--効果音再生) / [stop_se](#stop_se--se停止) / [play_voice](#play_voice--ボイス再生) / [stop_voice](#stop_voice--ボイス停止) / [set_volume](#set_volume--チャンネル音量設定)

### 演出・カメラ
- [shake](#shake--画面揺れ) / [flash](#flash--画面フラッシュ) / [fade](#fade--画面フェード) / [zoom](#zoom--画面ズーム) / [pan](#pan--画面パン) / [reset_camera](#reset_camera--カメラリセット) / [ken_burns](#ken_burns--ken-burnsエフェクト) / [stop_ken_burns](#stop_ken_burns--ken-burns停止)

### パーティクル
- [play_particle](#play_particle--エフェクト再生) / [stop_particle](#stop_particle--エフェクト停止)

### インタラクション
- [input_text](#input_text--テキスト入力)

### ローカライズ
- [set_language](#set_language--言語切替)

### 表示モード
- [set_mode](#set_mode--表示モード切替) / [clear](#clear--nvlテキストクリア)

### 計算
- [calc](#calc--変数計算)

### 制御
- [wait](#wait--ウェイト) / [next_script](#next_script--次スクリプト) / [end](#end--終了)

---

## 共通フィールド（ScriptCommand）

すべてのコマンドは以下のフィールドを持ちます。各コマンドが使用するフィールドのみ記述すれば十分です。

| フィールド | 型 | 説明 |
|---|---|---|
| `type` | string | コマンド種別（必須） |
| `character` | string | キャラクター名/ID |
| `text` | string | テキスト/プロンプト |
| `image` | string | 画像名（characterやclipのフォールバック） |
| `position` | string | 位置（`"left"` / `"center"` / `"right"`） |
| `expression` | string | 表情名 |
| `duration` | float | アニメーション秒数 |
| `label` | string | ラベル名/条件式 |
| `target` | string | ジャンプ先/スクリプトパス/フラグ名 |
| `clip` | string | 音声ファイル名 |
| `volume` | float | 音量（0.0〜1.0） |
| `value` | string | 汎用値（強度・色・方向等） |
| `order` | int | 表示レイヤー順（デフォルト: -1） |
| `choices` | ChoiceOption[] | 選択肢リスト |

---

## 条件式の書式

`jump_if` / `jump_unless` / `choice.condition` で使用する条件式:

| 書式 | 意味 | 例 |
|---|---|---|
| `flag_name` | フラグが truthy | `"met_alice"` |
| `!flag_name` | フラグが falsy | `"!met_alice"` |
| `flag==value` | 値一致 | `"route==good"` |
| `flag!=value` | 値不一致 | `"route!=bad"` |
| `flag>=N` | 数値以上 | `"affection>=10"` |
| `flag<=N` | 数値以下 | `"affection<=5"` |
| `flag>N` | 数値超過 | `"affection>10"` |
| `flag<N` | 数値未満 | `"affection<3"` |

---

## テキスト・表示

### say — セリフ表示

テキストを1文字ずつ表示し、ユーザー入力待ちになります。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `character` | - | - | 話者名 |
| `text` | **必須** | - | 表示するセリフ |
| `clip` | - | - | 同時再生するボイスファイル名 |
| `volume` | - | - | ボイス音量 |

```json
{ "type": "say", "character": "アリス", "text": "こんにちは！" }
```

```json
{ "type": "say", "character": "アリス", "text": "お元気ですか？", "clip": "alice_002", "volume": 0.8 }
```

ボイス付きの場合、バックログからの再聴にも対応します。

---

### ai_say — AI生成セリフ

Claude API にプロンプトを送り、生成されたテキストを表示します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `character` | - | - | 話者名 |
| `text` | **必須** | - | Claude APIへ送るプロンプト |

```json
{ "type": "ai_say", "character": "ナレーター", "text": "この世界の美しさについて詩的に一言述べてください。" }
```

> **⚠️ セキュリティ上の注意**: `NovellaEngine.AIApiKey`（Inspector）に設定したAPIキーはシーンにシリアライズされ、**ビルドに平文で埋め込まれます**。配布ビルドからキーを抽出される恐れがあるため、`ai_say`は**ローカルでの開発・プロトタイピング用途に限定**してください。配布作品でAI生成を使う場合は、キーを直接埋め込まず、自前のプロキシサーバー経由でAPIを呼ぶ構成にしてください。

---

### show_title — 章タイトル表示

画面中央に大きなテキストをフェードイン・フェードアウトで表示します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `text` | **必須** | - | タイトルテキスト |
| `duration` | - | 3.0 | 表示総時間（秒） |

フェードイン0.5秒 + 表示 + フェードアウト0.5秒で構成されます。

```json
{ "type": "show_title", "text": "第1章：はじまりの日", "duration": 4.0 }
```

---

## 選択肢・分岐

### choice — 選択肢表示

プレイヤーに選択肢を提示します。表示前にオートセーブが実行されます。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `choices` | **必須** | - | 選択肢の配列 |

**choices の各要素:**

| フィールド | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `text` | **必須** | - | 選択肢テキスト |
| `target` | - | - | 選択後ジャンプするラベル |
| `set_flag` | - | - | セットするフラグ名 |
| `flag_value` | - | `"true"` | フラグにセットする値 |
| `condition` | - | - | 表示条件式（条件を満たす選択肢のみ表示） |

```json
{
  "type": "choice",
  "choices": [
    { "text": "はい", "target": "yes_route", "set_flag": "agreed" },
    { "text": "いいえ", "target": "no_route" },
    { "text": "秘密のルート", "target": "secret", "condition": "affection>=10" }
  ]
}
```

---

### label — ラベル定義

ジャンプ先のマーカーです。実行時は何もせず次へ進みます。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `label` | **必須** | - | ラベル名 |

```json
{ "type": "label", "label": "good_end" }
```

---

### jump — 無条件ジャンプ

指定ラベルへ無条件でジャンプします。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `target` | **必須** | - | ジャンプ先ラベル |

```json
{ "type": "jump", "target": "good_end" }
```

---

### jump_if — 条件付きジャンプ（真）

条件式が真の場合に指定ラベルへジャンプします。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `label` | **必須** | - | 条件式 |
| `target` | **必須** | - | ジャンプ先ラベル |

```json
{ "type": "jump_if", "label": "affection>=10", "target": "good_route" }
```

```json
{ "type": "jump_if", "label": "met_alice", "target": "reunion_scene" }
```

---

### jump_unless — 条件付きジャンプ（偽）

条件式が偽の場合に指定ラベルへジャンプします（`jump_if` の逆）。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `label` | **必須** | - | 条件式 |
| `target` | **必須** | - | ジャンプ先ラベル |

```json
{ "type": "jump_unless", "label": "agreed", "target": "refuse_route" }
```

---

## フラグ

### set_flag — フラグセット

フラグに値をセットします。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `label` | **必須** | - | フラグ名 |
| `value` | - | `"true"` | セットする値 |

```json
{ "type": "set_flag", "label": "met_alice" }
```

```json
{ "type": "set_flag", "label": "route", "value": "good" }
```

---

### add_flag — フラグ加算

数値フラグに整数を加算します。負数で減算も可能です。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `target` | **必須** | - | フラグ名 |
| `value` | - | `"1"` | 加算する整数値 |

```json
{ "type": "add_flag", "target": "affection", "value": "5" }
```

```json
{ "type": "add_flag", "target": "affection", "value": "-3" }
```

---

## 背景・立ち絵

### show_bg — 背景表示

背景画像を表示します。CG回想にも自動記録されます。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `image` | **必須** | - | 背景画像名（Resources内） |
| `duration` | - | - | クロスフェード秒数 |

```json
{ "type": "show_bg", "image": "school_classroom", "duration": 0.5 }
```

---

### show_char — 立ち絵表示

キャラクターの立ち絵を表示します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `character` | **必須**\* | - | キャラクターID |
| `expression` | - | - | 表情名 |
| `position` | - | - | `"left"` / `"center"` / `"right"` |
| `duration` | - | - | フェードイン秒数 |
| `layer` | - | - | レイヤー制御: `"front"`（最前面）/ `"back"`（最背面）/ 数値 |
| `order` | - | -1 | 表示レイヤー順（レガシー。`layer` 推奨） |

\* `character` 未指定時は `image` がフォールバックとして使われます。

```json
{ "type": "show_char", "character": "alice", "expression": "smile", "position": "right", "layer": "front" }
```

同じキャラクターで `expression` だけ変えると表情クロスフェード（0.2秒）になります。

---

### hide_char — 立ち絵非表示

キャラクターの立ち絵を非表示にします。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `character` | **必須**\* | - | キャラクターID |

\* `character` 未指定時は `image` がフォールバック。

```json
{ "type": "hide_char", "character": "alice" }
```

---

### move_char — 立ち絵移動

表示中のキャラクターをスムーズに別のポジションへ移動します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `character` | **必須**\* | - | キャラクターID |
| `position` | **必須** | - | 移動先ポジション |
| `duration` | - | - | 移動アニメーション秒数 |
| `layer` | - | - | 移動後のレイヤー制御: `"front"` / `"back"` / 数値 |

```json
{ "type": "move_char", "character": "alice", "position": "left", "duration": 0.5, "layer": "front" }
```

---

## サウンド

### play_bgm — BGM再生

BGMを再生します。BGM回想にも自動記録されます。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `clip` | **必須**\* | - | BGMファイル名（Resources内） |
| `volume` | - | 1.0 | 音量（0.0〜1.0） |
| `value` | - | - | `"fade_in"` でフェードイン再生 |
| `duration` | - | 1.0 | フェードイン時間（`value: "fade_in"` 時） |

\* `clip` 未指定時は `image` がフォールバック。

```json
{ "type": "play_bgm", "clip": "bgm_school", "volume": 0.7 }
{ "type": "play_bgm", "clip": "bgm_night", "value": "fade_in", "duration": 2.0 }
```

---

### stop_bgm — BGM停止

BGMを停止します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `duration` | - | - | フェードアウト秒数 |

```json
{ "type": "stop_bgm", "duration": 1.0 }
```

---

### fade_bgm — BGM音量フェード

再生中のBGMを停止せずに音量をフェードします。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `value` | - | 1.0 | 目標音量（0.0〜1.0） |
| `duration` | - | 1.0 | フェード秒数 |

```json
{ "type": "fade_bgm", "value": "0.3", "duration": 2.0 }
{ "type": "fade_bgm", "value": "1.0", "duration": 1.0 }
```

---

### play_se — 効果音再生

効果音を1回再生します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `clip` | **必須**\* | - | SEファイル名 |
| `volume` | - | - | 音量 |

```json
{ "type": "play_se", "clip": "se_door_open", "volume": 1.0 }
```

---

### stop_se — SE停止

再生中のSEを即座に停止します。パラメータ不要。

```json
{ "type": "stop_se" }
```

---

### play_voice — ボイス再生

ボイスを単独で再生します（`say` コマンドとは独立して使用可）。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `clip` | **必須**\* | - | ボイスファイル名 |
| `volume` | - | - | 音量 |

```json
{ "type": "play_voice", "clip": "voice_scream", "volume": 0.9 }
```

---

### stop_voice — ボイス停止

再生中のボイスを即座に停止します。パラメータ不要。

```json
{ "type": "stop_voice" }
```

---

### set_volume — チャンネル音量設定

BGM/SE/Voiceチャンネルの音量をスクリプトから設定します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `target` | **必須** | - | チャンネル名: `bgm` / `se` / `voice` |
| `value` | - | 1.0 | 音量（0.0〜1.0）。`volume` パラメータでも可 |

```json
{ "type": "set_volume", "target": "bgm", "value": "0.5" }
{ "type": "set_volume", "target": "se", "value": "0.8" }
```

---

## 演出・カメラ

### shake — 画面揺れ

画面を揺らす演出です。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `duration` | - | 0.4 | 揺れ継続時間（秒） |
| `value` | - | `"10"` | 揺れの強度（ピクセル） |

```json
{ "type": "shake", "duration": 0.5, "value": "20" }
```

---

### flash — 画面フラッシュ

画面全体をフラッシュさせます。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `duration` | - | 0.3 | フラッシュ時間（秒） |
| `value` | - | `"white"` | 色: `"white"` / `"black"` |

```json
{ "type": "flash", "duration": 0.3, "value": "white" }
```

---

### fade — 画面フェード

画面を暗転（out）または明転（in）します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `value` | - | `"out"` | `"out"` で暗転 / `"in"` で明転 |
| `target` | - | `"black"` | フェード色: `"black"` / `"white"` |
| `duration` | - | 0.5 | フェード時間（秒） |

```json
{ "type": "fade", "value": "out", "duration": 1.0 }
```

```json
{ "type": "fade", "value": "in", "target": "black", "duration": 0.5 }
```

---

### zoom — 画面ズーム

画面をズームイン/アウトします。Canvas RectTransform のスケーリングで実現。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `value` | - | `"1"` | ズーム倍率（0.5〜3.0） |
| `duration` | - | 0.5 | アニメーション秒数 |
| `position` | - | `"center"` | ズーム中心: `"left"` / `"right"` / `"top"` / `"bottom"` / `"center"` |

```json
{ "type": "zoom", "value": "1.5", "position": "right", "duration": 0.8 }
```

---

### pan — 画面パン

画面をスライド移動します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `value` | **必須** | - | 移動量 `"X,Y"`（ピクセル） |
| `duration` | - | 0.5 | アニメーション秒数 |

```json
{ "type": "pan", "value": "100,0", "duration": 0.5 }
```

```json
{ "type": "pan", "value": "-50,30", "duration": 1.0 }
```

---

### reset_camera — カメラリセット

ズーム・パンをすべて元に戻します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `duration` | - | 0.3 | アニメーション秒数 |

```json
{ "type": "reset_camera", "duration": 0.5 }
```

---

### ken_burns — Ken Burnsエフェクト

背景画像をゆっくりズーム＆パンするスチル演出です。**非ブロッキング**（即座に次のコマンドへ進み、セリフと並行して動作）。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `value` | - | 1.2 | 目標ズーム倍率（0.5〜3.0） |
| `position` | - | center | ズーム先: `left` / `right` / `center` / `top` / `bottom` / `top_left` / `top_right` / `bottom_left` / `bottom_right` |
| `duration` | - | 5.0 | アニメーション秒数 |

```json
{ "type": "ken_burns", "value": "1.3", "position": "right", "duration": 8.0 }
```

---

### stop_ken_burns — Ken Burns停止

Ken Burnsエフェクトを停止し、背景を元の位置・サイズに戻します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `duration` | - | 0.5 | リセットアニメーション秒数 |

```json
{ "type": "stop_ken_burns", "duration": 1.0 }
```

---

## パーティクル

### play_particle — エフェクト再生

パーティクルエフェクトを再生します。プリセット5種と、カスタムPrefabに対応。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `value` | - | `"sakura"` | プリセット名またはPrefab名 |
| `duration` | - | 0（無限） | 持続時間（秒、0で無限再生） |

**プリセット一覧:**
- `sakura` — 桜吹雪（ピンクの花びら）
- `snow` — 雪（白い粒子）
- `rain` — 雨（青白い縦長粒子）
- `firefly` — 蛍（黄緑の光）
- `dust` — 塵（微かな浮遊粒子）

カスタムPrefabは `Resources/Particles/{name}` に配置。

```json
{ "type": "play_particle", "value": "sakura" }
```

```json
{ "type": "play_particle", "value": "snow", "duration": 10.0 }
```

---

### stop_particle — エフェクト停止

パーティクルエフェクトを停止します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `value` | - | - | 停止するエフェクト名（省略で全停止） |
| `duration` | - | 1.0 | フェードアウト時間（秒） |

```json
{ "type": "stop_particle", "value": "sakura", "duration": 2.0 }
```

```json
{ "type": "stop_particle" }
```

---

## インタラクション

### input_text — テキスト入力

テキスト入力ダイアログを表示し、入力結果をフラグに保存します。
保存されたフラグは `{flag_name}` で他コマンドのテキスト内に展開できます。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `target` | **必須** | - | 保存先フラグ名 |
| `text` | - | `"入力してください"` | プロンプト文 |
| `value` | - | - | デフォルト入力値 |

```json
{ "type": "input_text", "target": "player_name", "text": "あなたの名前を入力してください" }
```

```json
{ "type": "say", "text": "ようこそ、{player_name}さん！" }
```

---

## ローカライズ

### set_language — 言語切替

ゲームの表示言語を切り替えます。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `value` | **必須** | - | 言語コード（`Resources/Localization/` 内のファイル名） |

```json
{ "type": "set_language", "value": "en" }
```

テキスト中の `#key#` がローカライズファイルの値に置換されます。

```json
{ "type": "say", "text": "#greeting#、{player_name}さん！" }
```

ローカライズファイル例（`Resources/Localization/ja.json`）:
```json
{ "greeting": "こんにちは", "farewell": "さようなら" }
```

---

## テキスト内展開

テキスト内で以下の記法が使えます：

| 記法 | 説明 | 例 |
|---|---|---|
| `{flag_name}` | フラグ値に展開 | `{player_name}` |
| `#key#` | ローカライズキーに展開 | `#greeting#` |
| `{w:N}` | N秒ウェイト（インラインコマンド） | `{w:0.5}` |
| `{s:N}` | 文字速度変更（インラインコマンド） | `{s:20}` |
| `{sr}` | 文字速度リセット（インラインコマンド） | `{sr}` |

---

## 制御

### wait — ウェイト

指定秒数だけ待機します。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `duration` | - | 0.1 | 待機秒数 |

```json
{ "type": "wait", "duration": 2.0 }
```

---

### next_script — 次スクリプト

現在のスクリプトを終了し、次のスクリプトをロードして再生を続けます。チャプター完了として自動記録＆オートセーブされます。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `target` | **必須** | - | 次スクリプトのResourcesパス |

```json
{ "type": "next_script", "target": "Scripts/chapter02" }
```

---

### end — 終了

スクリプトの再生を終了します。シーン回想にも自動記録されます。

```json
{ "type": "end" }
```

---

## 表示モード

### set_mode — 表示モード切替

ADV/NVLモードを切り替えます。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `value` | **必須** | - | `"adv"` または `"nvl"` |

```json
{ "type": "set_mode", "value": "nvl" }
```

---

### clear — NVLテキストクリア

NVLモードで蓄積されたテキストをクリアします（ページ送り）。

```json
{ "type": "clear" }
```

---

## 計算

### calc — 変数計算

フラグ変数に対して四則演算を行います。

| パラメータ | 必須 | デフォルト | 説明 |
|---|---|---|---|
| `target` | **必須** | - | 対象フラグ名 |
| `value` | **必須** | - | 演算式（例: `"+5"`, `"-3"`, `"*2"`, `"/2"`） |

```json
{ "type": "calc", "target": "affection", "value": "+5" }
```

---

## リッチテキスト装飾

`say` コマンドの `text` 内で使えるインライン装飾タグです。TMProリッチテキストに自動変換されます。

| タグ | 変換先 | 例 |
|---|---|---|
| `{b}...{/b}` | `<b>...</b>` | `{b}太字{/b}` |
| `{i}...{/i}` | `<i>...</i>` | `{i}斜体{/i}` |
| `{c:色}...{/c}` | `<color=色>...</color>` | `{c:red}赤文字{/c}` / `{c:#FF6600}橙{/c}` |
| `{size:n}...{/size}` | `<size=n>...</size>` | `{size:60}大きい{/size}` |
| `{rb:漢字,かんじ}` | TMProルビ | `{rb:漢字,かんじ}` |
| `{w:n}` | n秒待機 | `{w:1.0}` |
| `{s:n}` | 表示速度変更 | `{s:20}` |
| `{sr}` | 表示速度リセット | `{sr}` |

複合も可能: `{b}{c:red}太字赤{/c}{/b}`

---

## CSV対応

JSONの代わりにCSV形式でもスクリプトを記述できます。列の順序:

```
type, character, text, image, position, expression, duration, label, target, clip, volume, value, order
```

CSVファイルは `Resources/Scripts/` に配置し、自動判定されます。

---

## シナリオ例

```json
{
  "commands": [
    { "type": "show_title", "text": "第1章：はじまりの日" },
    { "type": "show_bg", "image": "school_gate", "duration": 0.5 },
    { "type": "play_bgm", "clip": "bgm_morning", "volume": 0.6 },
    { "type": "say", "text": "春の朝。桜が舞い散る校門の前に立つ。" },
    { "type": "show_char", "character": "alice", "expression": "smile", "position": "center" },
    { "type": "say", "character": "アリス", "text": "おはよう！今日もいい天気だね。", "clip": "alice_001" },
    { "type": "choice", "choices": [
      { "text": "おはよう！", "target": "friendly", "set_flag": "friendly_greeting" },
      { "text": "...", "target": "silent" }
    ]},
    { "type": "label", "label": "friendly" },
    { "type": "add_flag", "target": "affection", "value": "1" },
    { "type": "say", "character": "アリス", "text": "えへへ、元気そうでよかった！" },
    { "type": "jump", "target": "continue" },
    { "type": "label", "label": "silent" },
    { "type": "say", "character": "アリス", "text": "...寝不足？大丈夫？" },
    { "type": "label", "label": "continue" },
    { "type": "fade", "value": "out", "duration": 1.0 },
    { "type": "next_script", "target": "Scripts/chapter02" }
  ]
}
```

---

## 回想モード（Scene Recollection）

SceneDefinition（ScriptableObject）でリプレイ可能なシーンを定義し、タイトル画面の「回想」から既読シーンを再プレイできます。

### SceneDefinition の設定

`Create > Novella > Scene Definition` で作成。

| フィールド | 説明 |
|---|---|
| `sceneId` | シーンの一意ID（記録キー） |
| `title` | 回想画面に表示するタイトル |
| `scriptPath` | スクリプトのResourcesパス（例: `Scripts/chapter01`） |
| `startLabel` | 開始ラベル（空なら先頭から） |
| `endLabel` | 終了ラベル（空なら `end` コマンドまで） |
| `thumbnail` | サムネイル画像（任意） |
| `sortOrder` | 表示順序（小さいほど先） |

### 自動記録

スクリプト実行中に `label` コマンドで `endLabel` に到達すると、そのシーンが自動的にクリア済みとして記録されます。

### リプレイ動作

- フラグは隔離されます（リプレイ中のフラグ変更は本編に影響しません）
- 終了ラベルに到達、または `end` コマンドで自動的にタイトル画面に戻ります
- 未クリアシーンはロック表示（"???"）になります

---

## 分岐フローマップ（Flowchart）

FlowchartDefinition（ScriptableObject）でストーリーの分岐構造を定義し、タイトル画面の「フローチャート」から閲覧できます。

### FlowchartDefinition の設定

`Create > Novella > Flowchart Definition` で作成。

#### FlowNode（ノード）

| フィールド | 説明 |
|---|---|
| `id` | ノードの一意ID |
| `title` | 表示タイトル |
| `type` | `Start` / `Scene` / `Choice` / `Ending` |
| `unlockKey` | 到達判定キー（SceneDefinitionのsceneId、エンディングラベル等） |
| `column` | グリッド上の列（0始まり） |
| `row` | グリッド上の行（0始まり） |

#### FlowEdge（エッジ）

| フィールド | 説明 |
|---|---|
| `fromId` | 接続元ノードID |
| `toId` | 接続先ノードID |
| `label` | エッジラベル（選択肢テキスト等、任意） |

### 表示ルール

- **Start** ノードは常にアンロック
- **Scene/Choice** ノードは `unlockKey` が SceneRecollectionManager に記録済みならアンロック
- **Ending** ノードは `unlockKey` が EndingManager に記録済みならアンロック
- 未到達ノードは暗転表示（"???"）
- ヘッダーに踏破率（到達ノード数/全ノード数）をパーセンテージ表示
