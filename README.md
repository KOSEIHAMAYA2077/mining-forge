# Mining Forge（仮称）

未知の採集物を、一手ずつ加減して取り出す。結晶で道具を作り、遺物で採り方を変え、魔力で技を覚え、次の探索へ向かう小さな一人用ゲーム。

**現在は企画・設計段階です。Unityプロジェクト、実行可能なゲーム、プレイ検証結果はまだありません。** 製品名・舞台・最終的な撤退ルールは未決定です。

## 最初に読む

- [START_HERE](docs/START_HERE.md)：作業フォルダ、読み順、実装担当への渡し方
- [MASTER](docs/MASTER.md)：現在の企画の正本、決定と提案の区別
- [STATUS](docs/STATUS.md)：現在地と次の作業
- [GAME_DESIGN](docs/GAME_DESIGN.md)：採取・報酬・成長のつながり
- [DEV_NOTES](docs/DEV_NOTES.md)：3種類の採集物を広げる案と検証したい仮説
- [PROTOTYPE_0](docs/PROTOTYPE_0.md)：小さな試作の範囲・仮数値・完成条件
- [ROADMAP](docs/ROADMAP.md)：小規模作品として完成させる順序
- [WORKFLOW](docs/WORKFLOW.md)：GitHub、進捗、作業記録、端末間の引き継ぎ
- [DECISIONS](docs/DECISIONS.md)：決めた理由と再検討条件

## 正本と作業場所

共有の正本は、このGitリポジトリのGitHub上の `main` です。ローカルの未コミット変更や、まだpushしていないコミットは共有前の作業です。会話中の案は、文書に反映するまで確定仕様にしません。

リポジトリルートをCodexの作業フォルダに指定します。将来のUnityプロジェクトは `game/`、独自のゲーム素材・コードは `game/Assets/_Project/` に置く計画です。`game/` はまだ作成していません。

参考作品の比較は [Steam調査](docs/research/2026-09-06-steam-comparison.md) に保存しています。参考元の画面・文章・素材の複製は行わず、部位・技・会心・適正ゲージによる判断を独自の採取体験に組み直します。
