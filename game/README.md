# Mining Forge — P0a-01

結晶1個を3部位から分離するWindows試作。Unity **6000.3.18f1** / URP **17.3.0** / C# / Windows x64（Mono）。

## 遊ぶ

リポジトリルートの `builds/P0a-01/MiningForge.exe` を起動する。隣接する `MiningForge_Data` なども必要。初期表示は1280×720のウィンドウ。

- 結晶または右側の部位欄をクリックして選択。キー1・2・3でも選択可能。
- Q：通常打撃、W：強打、E：そっと削る、R：均等打ち。画面のボタンでも操作可能。
- 緑の帯が最適。細い白い線は中心、灰色の線は分離条件、赤い線は致命傷。各部位の数値と状態を併記する。
- 全部位を分離ラインまで進めると「原石を回収」が有効になる。帯まで仕上げると高品質を狙える。
- 「閉じる」またはEscは同じ結晶を保持。「放棄」は2回押して終了。打撃中は重複操作を受け付けない。
- 結果画面で「同じ条件でもう一度」は同じseedから再試遊。「別の乱数で試す」はseedを1つ進める。
- 音量±と画面揺れ（弱／なし）は画面左下。終了ボタンでアプリを閉じる。

今回の結果は試遊用で、終了時に消える。工房、倉庫、3系統、経済、永続セーブはP0b以降。

## Unityで開く・再ビルドする

Unity Hubからこの `game` フォルダを開く。`Assets/Scenes/CrystalStudy.unity` を開いてPlay。シーンは小さな起点で、3D背景とUIは実行時に生成する。

リポジトリルートで `powershell -ExecutionPolicy Bypass -File scripts/build-windows.ps1`。ビルドの出典は `builds/P0a-01/SOURCE.txt`。`-OutputFolder P0a-01-final` で別フォルダへ生成可能。今回の正式な引き渡し先は [CURRENT](../docs/handoff/CURRENT.md) を参照。ビルド一式はGit管理外なので、他端末では同じエディタで再ビルドするか、出力フォルダ全体を渡す。

## 検証と調整

- `dotnet run --project tests/MiningRules.Tests.csproj`（.NET SDK 10）：共有する実際の採取計算を境界・取引・乱数の再現性で検証。
- `MiningForge.exe --smoke-test -logFile <絶対ログパス>`：描画を伴う実行確認。UIボタンの処理を呼び出して採取・回収・損傷・開閉を確認し、2解像度の画面を `artifacts/P0a-01` に保存する。OSからの実クリック／キーボード試験とは区別する。
- `Assets/Resources/MiningConfig.json`：集中力、中心、帯、技の範囲と消費、会心率。構造は3部位・4技固定。
- 描画用乱数は採取乱数から独立。同じseed・同じ操作列なら同じ採取結果。
- 素材とライセンスは `../docs/ASSETS.md`。
