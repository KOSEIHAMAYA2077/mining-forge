# Mining Forge 試作保存版 — 2026-09-07

DQ11「ふしぎな鍛冶」の仕組みを再現し、別の要素を足してみようとしたプロジェクト。元の仕組みの完成度が高く、納得できる追加要素を見出せなかったため、いったん棚上げして保存する。原作との数値一致・作品の完成を宣言するアーカイブではない。

## ZIPの中身と起動

ZIPは展開して使用する。最新の保存対象は `playable/ForgeAssets-archive/MiningForge.exe`。隣のDataフォルダとDLLも必要なので、実行ファイルだけを取り出さない。準備→「この地金を鍛える」→たたく／とくぎ→部位→決定。仕上げ後に原石をクリックまたはEnterで回収。マウス、矢印、Enter、Esc、一覧の左右切替はTab。

- `source/`：Unityプロジェクト、設計、作業記録、Blender原本、制作スクリプト、ライセンス。
- `playable/ForgeAssets-archive/`：DQ11型の盤面を維持し、Blender描画・破片・回収表示を組み込んだ最終保存対象。
- `playable/P0a-01-final/`、`P0a-02-final/`、`P0a-03-final/`：過去の3つの区切り。最終仕様ではなく比較用。
- `playable/MiningAssets-dev/`：ユーザーが差し戻した旧3D画面への組み込み案。**不採用の参考記録**。新しい企画の土台にはしない。
- `history/repository.bundle`：保存時点のローカルGit参照を含む履歴。`.git/config`や認証情報は含まない。
- `MANIFEST.json`：収録全ファイルのサイズ・SHA256、アーカイブ対象のコミット。

バイナリZIPはGit履歴へ直接格納せず、非公開GitHubリポジトリのReleaseに添付する。UnityのLibrary・Temp、認証情報、Blenderの自動バックアップ、重複する途中ビルドは含めない。元の作業フォルダは削除していない。

## 編集再開

Unity 6000.3.18f1で `source/game` を開く。現在の保存対象Sceneは `Assets/Scenes/ForgeAssetStudy.unity`。`ForgeStudy`は前の鍛冶版、`BoardStudy`・`CrystalStudy`はさらに古い比較用。

Blender原本は `source/art/source/mining-prototype/board-art.blend` と `mining-prototype.blend`。Blender 4.5.13 LTSのlocalhost MCPで制作した。現在のゲーム表示はBlenderで描画した2D画像とUnityのUI判定であり、任意に体積を削る3Dシステムは未実装。

保存対象の再ビルドは `source/scripts/build-mining-assets.ps1 -OutputFolder ForgeAssets-archive`。出力のSOURCE.txtに実際のビルド元を記録する。Git履歴を復元する場合は `git clone history/repository.bundle restored-repo` を使用し、`feat/mining-prototype-assets`を選ぶ。

## 確認と限界

保存前の描写版では自動UI確認39項目合格。手動のウィンドウ操作はユーザーのEscで停止し、完走確認はしていない。音声信号は検出したが、聴感・長時間・他PCの検証は未了。表示の満足度と原作一致を自動テスト合格に読み替えない。最終保存ビルドの検証結果は `source/docs/verification/ForgeAssets/README.md`。

会心率・品質などの仮係数は残る。所持品の永続保存はない。仕上げで確定した1品を回収演出で表示し、回収演出で追加の品を発行しない。ゲームと素材の来歴は `source/docs/ASSETS.md`。

次の「3D採取→磨く・取り出す→売る／使う」は企画候補として別メモに保存しただけで、このZIPの実装内容には含まない。
