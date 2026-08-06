# 条件式・calc の回帰テスト

`jump_if` / `jump_unless` / `choice.condition` / `calc` が使う式評価の回帰テストです。
Unityを起動せずに実行できます。

```
dotnet run
```

（.NET SDK 8 以降が必要です。終了コードは全件成功で 0、失敗があれば 1。）

## 何をしているか

`ExpressionTests.csproj` がパッケージ本体のソース

- `Runtime/Core/NovellaExpression.cs`
- `Runtime/Core/FlagManager.cs`
- `Runtime/Commands/CalcCommandHandler.cs`

を直接コンパイルします。テスト用のコピーは持たないので、本体を編集したら次の実行でそのまま検証されます。
`Stubs.cs` は `UnityEngine.Debug` と、`CalcCommandHandler` が触る範囲の `NovellaEngine` / `ScriptCommand` を
最小限だけ再現したものです。

## 見るべき出力

- **`fail=0`** — 全ケース期待どおり
- **`従来記法での新旧差異=0`** — 1章のケースで、新しい式パーサと旧評価方式の結果が一致している

旧評価方式（`FlagManager.EvaluateLegacy`）は、式として解釈できない条件が来たときのフォールバックとして
本体に残っています。1章はそこを新旧比較して後方互換を担保する意図のケース群です。

意図的に変えた挙動は5章に差分として出力されます（テストの成否には数えません）。

## 注意

`Tools~` は末尾のチルダによりUnityのインポート対象外です。ここに置いたものはパッケージ利用者には届きません。
