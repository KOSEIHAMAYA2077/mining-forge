# 次の担当への引き継ぎ

2026-09-07：ユーザーは現在の試作を棚上げし、ZIPにまとめてGitHubへ保存するよう指示した。開発の自動継続はしない。現在のブランチは `feat/mining-prototype-assets`。main未統合。

保存版の説明・起動・復元は [ARCHIVE_README](../../ARCHIVE_README.md)、現在地は [STATUS](../STATUS.md)。実行版は `builds/ForgeAssets-archive/MiningForge.exe`、Sceneは `game/Assets/Scenes/ForgeAssetStudy.unity`。ZIP内では `playable/ForgeAssets-archive/MiningForge.exe`。

準備→この地金を鍛える→たたく／とくぎ→部位→決定→しあげる→クリックまたはEnterで回収。マウス・矢印・Enter・Esc・一覧のTab切替。永続保存はない。

現在はBlenderの原石描画を使う盤面式試作。任意の3D切削は作っていない。原作の完全再現でもない。自動UI確認39項目合格は、手動試遊・聴感・原作一致の証明ではない。[検証記録](../verification/ForgeAssets/README.md)。

ZIP本体はGitの通常ファイルへ入れず、非公開リポジトリのReleaseへ添付。旧P0a-01/02/03と、不採用のMiningAssets-dev実行版も比較用に収録する。元作業フォルダは消さない。

今後の案は [採取・加工・販売の検討](../research/2026-09-07-extraction-cleaning-simulator.md)。採取ビーム、スキルを使う化石クリーニング、販売を段階ごとに検討する候補であり、すべて実装すると決定したわけではない。


## GitHubへの保存完了

[Release・ZIP](https://github.com/KOSEIHAMAYA2077/mining-forge/releases/tag/archive-dq11-prototype-2026-09-07) にアップロード済み。ZIPは374,631,134 bytes、1,209項目。全収録ファイルを読み戻してサイズ・SHA256・CRCを確認し、GitHub側のSHA256とも一致した。

ZIP対象コミット：`f3b76d7cdc57858bf89949b60ad56621316243e3`。実行版ソース：`d22929a3418603d4b874b92e0c5b79b30ddf1a66`。保存作業の [Draft PR #8](https://github.com/KOSEIHAMAYA2077/mining-forge/pull/8) はmain未統合。リポジトリ紹介文・README・Releaseに棚上げの経緯を反映した。アップロード確認の記録は `docs/archives/2026-09-07-receipt.json`。この確認記録のコミットはZIP作成後のため、ZIP本体には含まない。
