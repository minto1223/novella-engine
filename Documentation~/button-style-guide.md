# ボタンスタイルガイド

Novella のボタンは **NovellaButtonStyle**（スタイルアセット）で見た目を管理します。
通常 / ホバー / 押下 / 無効 の4状態それぞれに色・装飾・画像を設定でき、コードを書かずにボタンデザインを差し替えられます。

## 仕組みの全体像

```
NovellaUITheme（テーマ）
  ├ PrimaryButtonStyle … タイトルメニュー・選択肢などの大きいボタン用
  ├ IconButtonStyle    … HUD（AUTO/SKIP等）の小さいボタン用
  └ DangerButtonStyle  … 終了・リセットなど危険操作用
        ↓ 実行時に自動割り当て
NovellaButton（ボタンに付くコンポーネント）
  └ Style … 個別指定するとテーマより優先される
```

- テーマのスタイル欄が**未設定**なら、従来どおりのフラット色（`TitleButtonColor` 等）で表示されます
- 特定のボタンだけ見た目を変えたい場合は、そのボタンの `NovellaButton > Style` にスタイルアセットを直接ドラッグします（テーマより優先）

## スタイルアセットの作り方

1. Projectビューで右クリック → **Create > Novella > Button Style**
2. 生成されたアセットを選択し、Inspectorで **Normal / Hover / Pressed / Disabled** の4状態を編集

各状態で設定できる項目:

| 項目 | 説明 |
|---|---|
| Sprite | この状態専用のボタン画像（未設定なら共通の Background Sprite → 色のみ描画の順でフォールバック） |
| Sprite Tint | 画像使用時の乗算色。**白＝画像そのままの色**。ホバーで画像をふんわり染めたい時などに使う |
| Background Color | 画像を使わない時の背景色 |
| Border Color | 枠線の色 |
| Text Color | ラベル文字色 |
| Show Corners | 四隅の角括弧装飾を表示（ホバー状態でONにするのが定番） |
| Play Sheen | 光が横切るエフェクトを再生 |
| Scale | ボタンの拡縮（押下時に 0.94〜0.98 で「押した感」が出る） |
| Enter SE | この状態に入った時に鳴らす効果音 |

共通設定:

| 項目 | 説明 |
|---|---|
| Transition Duration | 状態間のアニメーション秒数（0で即時切替） |
| Corner Color | 角括弧の色 |
| Show Border | 枠線を表示するか。**画像に枠が描き込んであるデザインではOFF** |
| Background Sprite | 全状態共通のボタン画像（9スライス推奨） |

## ボタンへの適用方法

### Button Builder で新規ボタンを作る場合

`Novella > Button Builder` の「スタイル（4状態演出）」欄で選択:

- **テーマにおまかせ（推奨）**: テーマのスタイルが自動適用される
- **カスタムスタイル指定**: 特定のスタイルアセットを指定
- **なし**: 従来のフラット色ボタン

### 既存ボタンに後付けする場合

1. ボタンのGameObjectに **NovellaButton** コンポーネントを追加
2. スタイルはテーマから自動割り当てされる（個別指定したい場合は `Style` 欄にドラッグ）

枠線・角括弧・sheenの装飾オブジェクトは実行時に自動生成されるので、手動でのオブジェクト配置は不要です。

## 自作画像（Photoshop等）への差し替え手順

1. **状態ごとにPNGを書き出す**（透過推奨。例: `btn_normal.png` / `btn_hover.png` / `btn_pressed.png` / `btn_disabled.png`。1枚を全状態で使い回してもOK）
2. Unityに取り込み、Inspectorで **Texture Type: Sprite (2D and UI)**、**Sprite Mode: Single** にする
3. 伸縮に耐えるデザインなら Sprite Editor で **Border を設定して9スライス化**（どのボタンサイズでも角が崩れなくなる）
4. スタイルアセットの各状態の **Sprite** 欄に対応する画像をドラッグ
5. 色は基本さわらなくてOK（画像使用時は Sprite Tint＝白でそのままの色が出る）
6. 画像に枠が描いてあるなら **Show Border をOFF**。角括弧・sheenが不要なら各状態の Show Corners / Play Sheen をOFF

これだけで、ホバーすると画像ごと切り替わるボタンになります（画像は状態切替時に即時差し替え、色・スケールはアニメーション）。

### 注意

- **AUTO / SKIP / Quick Load ボタンの文字色**はエンジン側（HUDController）がON/OFF表示のために直接制御しています。これらのボタンでは `NovellaButton > Control Label Color` をOFFにしてください（デモのシーンでは設定済み）
- ゲームパッド/キーボードで選択した時もホバーと同じ演出が表示されます
