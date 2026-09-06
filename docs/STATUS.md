# 現在の進捗

2026-09-07。**棚上げ・アーカイブ保存**。ブランチ `feat/mining-prototype-assets`。mainへは統合しない。

DQ11「ふしぎな鍛冶」を再現して別要素を足す実験だったが、元の仕組みの完成度が高く、納得できる追加要素を見出せず、ユーザーは一旦しまうことを決定した。これは企画上の振り返りであり、原作の完全再現達成を意味しない。

## 保存対象

P0a-03の盤面・ルールに、Blenderで制作した6形状の原石描画、ピッケル、ひび・破片・回収演出を加えたForge Asset Study。原本・FBX・画像・Unity設定を保存する。旧3D画面への組み込み案は差し戻され、現行コードでは取り消した。比較用実行版だけを不採用と明記して保管する。

ゲーム表示は事前描画した画像とUI。自由に削る3Dシステムは未実装。原作の確率・品質係数には未確認の仮値が残り、永続保存もない。

## 確認

描写版の自動UI確認39項目に合格。回収演出が品を二重発行しないこと、再加工、素材参照、複数盤面を確認。OS入力による手動試遊は中断し、聴感・他PC・長時間・見た目の満足度は未検証。ソース `d22929a` から保存用ビルドを作り、同じ39項目に再度合格。[検証記録](verification/ForgeAssets/README.md)。

## 保存と次の一手

[保存版ガイド](../ARCHIVE_README.md)。ZIPにはソース、Blender原本、5つの実行版、Git bundle、全収録ファイルのSHA256を含める。元フォルダは保持。ZIP自体を [GitHub Release](https://github.com/KOSEIHAMAYA2077/mining-forge/releases/tag/archive-dq11-prototype-2026-09-07) に添付する。

次案は [3D採取→磨く・取り出す→販売の検討](research/2026-09-07-extraction-cleaning-simulator.md)。まだ実装指示ではない。旧CR-003の完全再現作業も、追加指示があるまで再開しない。


## GitHubへの保存完了

[Release・ZIP](https://github.com/KOSEIHAMAYA2077/mining-forge/releases/tag/archive-dq11-prototype-2026-09-07) にアップロード済み。ZIPは374,631,134 bytes、1,209項目。全収録ファイルを読み戻してサイズ・SHA256・CRCを確認し、GitHub側のSHA256とも一致した。

ZIP対象コミット：`f3b76d7cdc57858bf89949b60ad56621316243e3`。実行版ソース：`d22929a3418603d4b874b92e0c5b79b30ddf1a66`。保存作業の [Draft PR #8](https://github.com/KOSEIHAMAYA2077/mining-forge/pull/8) はmain未統合。リポジトリ紹介文・README・Releaseに棚上げの経緯を反映した。アップロード確認の記録は `docs/archives/2026-09-07-receipt.json`。この確認記録のコミットはZIP作成後のため、ZIP本体には含まない。
