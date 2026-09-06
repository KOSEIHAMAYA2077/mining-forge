# Blender試作原本

mining-prototype.blend：最初の鉱石・ピッケルと破壊状態の試作。board-art.blend：盤面向け6形状、道具・結晶・破片の描画用。元の作業Sceneを残し、別Sceneで作業した。Blender 4.5.13 LTS、localhost MCPを使用。

再生成用スクリプトはscripts/blender/。出力はgame/Assets/Resources/MiningPrototype/。FBXはメートル、Unity Y-upへ変換。ピッケル原点は握り位置、StrikeTipは打点。初回モデルで読み込み・表示・Colliderを検証した。

現在のゲームはこれらのBlender描画を使う2D UI。表面を任意に削る3Dモデル変形ではない。素材は本プロジェクト制作、外部モデルや有料APIは使っていない。既存素材の出典はdocs/ASSETS.md。
