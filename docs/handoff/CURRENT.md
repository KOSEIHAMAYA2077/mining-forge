# 次の担当への引き継ぎ

2026-09-07：ユーザーは現在の試作を棚上げし、ZIPにまとめてGitHubへ保存するよう指示した。開発の自動継続はしない。現在のブランチは `feat/mining-prototype-assets`。main未統合。

保存版の説明・起動・復元は [ARCHIVE_README](../../ARCHIVE_README.md)、現在地は [STATUS](../STATUS.md)。実行版は `builds/ForgeAssets-archive/MiningForge.exe`、Sceneは `game/Assets/Scenes/ForgeAssetStudy.unity`。ZIP内では `playable/ForgeAssets-archive/MiningForge.exe`。

準備→この地金を鍛える→たたく／とくぎ→部位→決定→しあげる→クリックまたはEnterで回収。マウス・矢印・Enter・Esc・一覧のTab切替。永続保存はない。

現在はBlenderの原石描画を使う盤面式試作。任意の3D切削は作っていない。原作の完全再現でもない。自動UI確認39項目合格は、手動試遊・聴感・原作一致の証明ではない。[検証記録](../verification/ForgeAssets/README.md)。

ZIP本体はGitの通常ファイルへ入れず、非公開リポジトリのReleaseへ添付。旧P0a-01/02/03と、不採用のMiningAssets-dev実行版も比較用に収録する。元作業フォルダは消さない。

今後の案は [採取・加工・販売の検討](../research/2026-09-07-extraction-cleaning-simulator.md)。採取ビーム、スキルを使う化石クリーニング、販売を段階ごとに検討する候補であり、すべて実装すると決定したわけではない。
