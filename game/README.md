# Mining Forge — P0a-02「晶脈の標本」

形のある6マスを、単体・範囲技と活性の調整で仕上げる2D一画面の試作。Unity **6000.3.18f1** / URP **17.3.0** / Windows x64（Mono）。

## 起動・操作

正式な起動場所とソースIDは [CURRENT](../docs/handoff/CURRENT.md)。ローカルの `builds/P0a-02-final/MiningForge.exe` を起動する。隣接するDataとDLLも必要。初期ウィンドウ1280×720。

- 左の「たたく」または「特技」→技→中央の位置→「決定して実行」。範囲は右隣・下隣へ広がる。空きや盤面外を含む位置は実行できない。
- マウスでボタンと位置を選択できる。矢印でメニュー／位置選択、Enterで決定、Escで一段戻る。位置を選んだだけでは叩かない。
- 黄線と数値は通常の到達範囲、水色線と10%表記は会心時の到達範囲。緑の帯を狙い、赤い破損線に注意する。
- 活性が高いほど大きく削れ、打撃後に50下がる。「活性を上げる／鎮める」で調整。「詳しく見る」は無料。
- 「採取する」で結果を確認して終了。6部位が灰色の分離線以上なら原石1個、未分離が残れば報酬なし。破損しても続けられるが品質は最大39。
- 閉じる／Escの中断は状態を保持。放棄と試遊リセットには確認を表示。アプリ終了で進行と結果は消える。
- 設定で音量±、画面揺れ（弱／なし）。結果から同じseedか別seedで再試遊。

工房、探索、敵、持ち込みスキル、経済、永続保存は今回含まない。旧P0a-01のビルドは別フォルダに保持。

## Unityと検証

Unity Hubで `game` を開き、`Assets/Scenes/BoardStudy.unity` をPlay。旧CrystalStudyシーンは旧版の記録。

ルートで `powershell -ExecutionPolicy Bypass -File scripts/build-windows.ps1 -OutputFolder P0a-02-final`。出力の `SOURCE.txt` で出典を確認。ビルド一式はGit管理外。他端末では同じエディタで再ビルドするか、出力フォルダ全体を渡す。

- `dotnet run --project tests/MiningRules.Tests.csproj`：採取計算の境界、トランザクション、複数seedの回収手順比較。
- `MiningForge.exe --smoke-test -logFile <絶対ログパス>`：UI処理を呼ぶ自動試遊、描画・日本語の高さ・音声出力のチェック。画像は `artifacts/P0a-02`。OS入力試験とは区別する。
- `MiningForge.exe --fixed-activity --seed 1701`：開発用比較。活性1000固定、調整不可。同じ盤面・技・seedで比較できる。通常メニューにこの切替は置かない。
- `Assets/Resources/BoardConfig.json`：6マスの配置・中心・帯・技の量と消費・活性・会心率。小さな固定構造。
- ルール本体は `BoardRules.cs`。UI、演出、スモークテストは `BoardGame` の分割ファイル。表示用乱数は採取乱数から独立。
- [素材とライセンス](../docs/ASSETS.md)、[現在仕様](../docs/handoff/changes/CR-001-SPEC.md)。
