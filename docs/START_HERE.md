# 作業の入口

更新日：2026-09-06

## このWindowsで指定する場所

Codexに渡す「作業領域」はファイルではなく、次の**フォルダ**です。

```text
C:\Users\kouha\Documents\ChatGPT\採掘系のゲーム作成？
```

最初に読む案内ファイル：

```text
C:\Users\kouha\Documents\ChatGPT\採掘系のゲーム作成？\docs\START_HERE.md
```

ルートの `AGENTS.md` がエージェント向けの作業規則です。`MASTER.md` などが名前だけで自動的に読まれることを前提にせず、AGENTS.mdと依頼プロンプトで読み順を指定しています。

## 別の端末で作業するとき

GitHubのこのリポジトリを任意の場所へcloneし、**clone先のルートフォルダ**をCodexに指定します。このWindowsの絶対パスをMacなどにコピーする必要はありません。文書内リンクと作業規則はリポジトリ基準の相対パスを使います。

保存先：[KOSEIHAMAYA2077/mining-forge](https://github.com/KOSEIHAMAYA2077/mining-forge)（非公開）。アクセスできるGitHubアカウントで認証してから取得します。

```sh
git clone https://github.com/KOSEIHAMAYA2077/mining-forge.git
```

Unityで開く場所は `<clone先>/game/` です。現在のP0a-03は `feat/dq11-system-reference` ブランチ、旧P0a-02は `feat/p0a-crystal`。Unity 6000.3.18f1を使い、詳しい起動手順は [game/README](../game/README.md) と [CURRENT](handoff/CURRENT.md) を参照してください。最新の実装要件はCR-003です。

## 読み順と次の依頼

1. `AGENTS.md`
2. `docs/MASTER.md`：決定と未決事項
3. `docs/STATUS.md`：完了済み・未着手
4. `docs/handoff/CURRENT.md`：次の一件と引き継ぎ
5. 試作を依頼する場合だけ `docs/PROTOTYPE_0.md` と `docs/handoff/IMPLEMENTATION_PROMPT.md`

2026-09-06の追加依頼で、Issue #1のP0a（結晶1個を採る）実装と試遊版の引き渡しへ進んでいます。今回P0bの実装は含めません。

## 文書の使い分け

| 文書 | 答えること |
| --- | --- |
| MASTER | 今、どんなゲームを作る方針か |
| GAME_DESIGN | 採取と3種類の報酬がどうつながるか |
| DEV_NOTES | 広げるならどんな案があるか、何を試すか |
| PROTOTYPE_0 | 最初の実装の具体的な範囲と判定条件 |
| ROADMAP | 完成まで何をどの順に積み上げるか |
| DECISIONS | なぜ決めたか、いつ再検討するか |
| STATUS / handoff/CURRENT | どこまでできていて、次に何をするか |
| sessions | その作業で変えたこと・根拠・検証・残件 |
| GitHub Issues / PR | 個別の作業単位、関連する差分と確認 |

運用の詳細は [WORKFLOW](WORKFLOW.md)。履歴は残しつつ、次の担当が読む現在地は短く保ちます。

実際に試作を依頼して遊び、感想から修正を戻す手順は [PLAYTEST_LOOP](PLAYTEST_LOOP.md)。初回はP0aで一度プレイを挟む依頼文を用意しています。
