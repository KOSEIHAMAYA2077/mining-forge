# Mining Forge（仮称）

未知の採集物を、一手ずつ加減して取り出す。結晶で道具を作り、遺物で採り方を変え、魔力で技を覚え、次の探索へ向かう小さな一人用ゲーム。

**現在はUnity試作P0a-03を開発中です。** DQ11鍛冶のシステム再現を優先し、呼称は活性度を使用します。全技・必殺・地金効果などを実装しましたが、確率・品質係数の原作一致は未確認です。実際の起動場所・確認状況は [CURRENT](docs/handoff/CURRENT.md)。

[GitHubリポジトリ（非公開）](https://github.com/KOSEIHAMAYA2077/mining-forge) · [作業一覧](https://github.com/KOSEIHAMAYA2077/mining-forge/issues)

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

リポジトリルートをCodexの作業フォルダに指定します。Unityプロジェクトは `game/`、現在のソースは `game/Assets/Scripts/`。P0a-03は `feat/dq11-system-reference`、旧P0a-02は `feat/p0a-crystal` で管理し、mainへはまだ統合していません。

参考作品の比較は [Steam調査](docs/research/2026-09-06-steam-comparison.md)。最新の指示は [CR-003](docs/handoff/changes/CR-003.md)で、まずシステムを再現し、独自の採取や殻剥がしは後から追加します。使用素材と出典は [ASSETS](docs/ASSETS.md)。
