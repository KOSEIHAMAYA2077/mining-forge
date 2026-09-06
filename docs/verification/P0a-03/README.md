# P0a-03 正式保存ビルドの検証

2026-09-07。ソース `a58174972dedd5ce2a02d5c20b9ded0b5d2a9a28`。Unity 6000.3.18f1、Windows x64。ソース保存後に `scripts/build-forge.ps1 -OutputFolder P0a-03-final` を実行。ビルド前後でgameの未コミット変更なし。

起動：`C:\Users\kouha\Documents\ChatGPT\採掘系のゲーム作成？\builds\P0a-03-final\MiningForge.exe`

## 確認したこと

- ソースのルール試験68チェック合格。[ログ](rules-report.txt)。新旧テストの中間出力を分離してから再実行。
- 旧試作も境界33チェック・13,173アサーション合格、20seed×2方策の結果を再確認。これは旧版の回帰検証。
- 正式Windows実行版を `--forge-smoke` で起動し、終了コード0、28チェック合格。[実行報告](runtime-report.txt)。通常実行ではチャージ確率を強制しない。
- 準備、3ページの技、範囲選択、打撃と音声出力、会心必殺、地金効果、無料の評価、仕上げ、所持品、うちなおし、二重仕上げ拒否を確認。
- 1280×720と1920×1080で15枚の実行画面を取得。代表7枚を保存。日本語表示・レイアウト・地金形状・結果表示を目視。
- 実行ログのException／エラー／smoke失敗／ReadPixelsエラーは0件。ビルドと実行の全ログはローカルtmp内。
- [ソース記録](SOURCE.txt)、[配布フォルダ全ファイルのSHA256](build-sha256.json)。バイナリはGit管理外。

## この検証が証明しないこと

原作との数値一致ではない。会心率・品質・乱数・節目順序などは [未確認台帳](../../handoff/changes/CR-003-REFERENCE.md) に残っている。試験用地金は原作レシピデータではない。

自動試験はUI処理を呼ぶ方式で、OSのマウス・キー入力を実際に送っていない。音声ピークを検出したが聴感は未評価。ユーザーが見た目と操作を気に入ったか、面白いかは未評価。他PC、長時間、16:9以外も未検証。永続保存は未実装。

## 実際の画面

[準備](01-preparation.png)・[技一覧](04-skills-page2.png)・[範囲選択](06-range-target.png)・[地金効果](09-material-effect.png)・[結果](12-result.png)・[6部位](14-six-cell-1920.png)・[8部位](15-eight-cell-1920.png)
