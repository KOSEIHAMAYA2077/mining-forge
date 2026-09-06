# 前作Lucent Crystal Kit V1の持ち込み

2026-09-06。ユーザー自身の前作 `KOSEIHAMAYA2077/one-board-incremental` から、見た目の改修用にコピー。元プロジェクトは変更していない。現在のmining-forgeゲームには未導入。

## 内容

- `LucentCrystalKit-v1.unitypackage`：前作の保存済み配布パッケージ。Prefab・Mesh・Material・表示コンポーネント。生成元は2026-09-05の成果物であり、現在HEADから再生成したものではない。
- `obj/Shape0.obj`〜`Shape7.obj`：四面体、八面体、星形、面取りPrism、16面体、32面体、多面球、滑らかな球。材質を含まない形状ファイル。
- `UPSTREAM_README.md`：元プロジェクトの説明を変更せず保存。内部の実行ファイル・フォルダ記述は元プロジェクト基準。
- `SHA256.json`：コピー元と一致を確認したパッケージとOBJのSHA-256。

## 出典と取り扱い

元フォルダ：`C:\Dev\one-board-incremental`。参照時HEAD：`778d892ede598c49f38145394de342ddcec91aaa`。

パッケージ元：`Artifacts/CrystalKit/20260905-162337-1350284/LucentCrystalKit-v1.unitypackage`。
OBJ元：`ArtExports/CrystalKitV1/`。説明元：`docs/LUCENT_CRYSTAL_KIT_V1.md`。

元説明はコード生成のOriginal Meshで、購入アセットや参考作品からの抽出ではないと記載。ユーザーの自作プロジェクト間での再利用として持ち込んだ。外部CC0素材として登録したり、独自にライセンスを付け替えたりしていない。

Unity 6000.3.18f1のBuilt-in Render Pipeline向け。現在のゲームはURP 17.3.0なので、材質を変換して確認する必要がある。パッケージをコピーしたこととURPで正しく描画できたことを混同しない。保存アーカイブのUnityインポート・依存関係の解決・表示は今回未検証。

CrystalAppearanceの外殻・稜線・コアと状態表示を参考に、結晶一つの表示から確認する。前作のゲーム進行、セーブ、弾、インクリメンタル経済は持ち込まない。必要なものだけを現在の`game/Assets/`へ導入する。
