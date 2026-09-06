# 使用素材と出典

更新日：2026-09-06。P0a-02は2Dの盤面を使用。導入済みと調査候補を以下で区別する。

## P0a-02で実際に使うもの

| 素材・出典 | 配置場所 | 実際の使用・条件 |
| --- | --- | --- |
| [Kenney / Roguelike Caves & Dungeons](https://kenney.nl/assets/roguelike-caves-dungeons) | `game/Assets/Resources/BoardArt/roguelikeDungeon_transparent.png` | 公式zipのスプライトシート。背景の床と洞窟小物。16pxタイル＋1px間隔、左上を0として列8〜10・行10、列0〜3・行0を切り出し、拡大・着色して使用。2026-09-06に公式CC0表記・同梱文書を確認。`game/Assets/ThirdParty/Licenses/Kenney-Caves.txt` を保存 |
| Kenney / Impact Sounds 1.0 | `game/Assets/Resources/Audio/impactMining_001.ogg`, `impactMining_004.ogg` | 通常／範囲打撃と強打。既存のCC0素材、音量・ピッチ調整。出典とライセンスは下の旧版表 |
| Kenney / Particle Pack 1.1 | `game/Assets/Resources/Art/smoke_01.png`, `spark_01.png` | 打撃位置の粉、会心、回収の粒子。既存CC0素材を2D描画に使用 |
| Noto Sans CJK JP Regular | `game/Assets/Resources/Fonts/NotoSansCJKjp-Regular.otf` | 日本語UI。既存OFLフォントを変更せず使用 |
| 本プロジェクト制作の2D晶脈・母岩・亀裂・つるはし | `game/Assets/Scripts/OreCellGraphic.cs`, `BoardEffects.cs` | 連続する結晶の面をマスで区切って描画し、局所的に母岩を減らす。外部鉱石画像を取り込んだものではない。UIの枠も本プロジェクトのコードで作成 |
| 本プロジェクト制作の短い合成音 | `game/Assets/Scripts/BoardEffects.cs` | 繊細な削り、会心、適正帯、帯越え、破損、回収、活性調整。打撃後の状態音は破損→帯越え→会心→適正帯の順で一つだけ再生 |

Kenney背景の元シートとPreviewを実見し、ピクセル座標を保つインポート設定（非2冪への自動リサイズなし、Point、MipMapなし）にした。晶脈は6位置に同じ画像を繰り返す方式を避け、位置をまたぐ結晶と母岩を描く。DQの画像・音・装飾は使用していない。Nature Kitの3Dモデルは旧版用に保持しているが、BoardStudyの画面では使用しない。

KDRNとgreatdocbrownの公式配布ページは再確認した。前者の写真調アイコンは形状に沿った部位差分がなく、後者は小さな報酬向けの候補として残した。配布プレビューの取得はツールで失敗し、実ファイルは未取得・未導入。候補の全実物を比較済みとは扱わず、今回の盤面に対応した結晶をコードで制作した。効果音ラボからも今回は音源を取り込んでいない。

## P0a-01に導入した素材

| 素材名・版 | 配布元 | 取得日 | ライセンス文書 | ファイルと使用箇所・改変 |
| --- | --- | --- | --- | --- |
| Nature Kit 2.1（同梱表記） | [Kenney公式](https://kenney.nl/assets/nature-kit) | 2026-09-06 | `game/Assets/ThirdParty/Licenses/Kenney-Nature.txt` / CC0 | `Art/rock_largeA.fbx`, `rock_largeC.fbx`。母岩と背景。縮尺・配置・マテリアル変更 |
| Impact Sounds 1.0 | [Kenney公式](https://kenney.nl/assets/impact-sounds) | 2026-09-06 | `game/Assets/ThirdParty/Licenses/Kenney-Impact.txt` / CC0 | `Audio/impactMining_000`〜`004.ogg`。打撃音。再生時の音程・音量を技別に変更 |
| Particle Pack 1.1（同梱表記） | [Kenney公式](https://kenney.nl/assets/particle-pack) | 2026-09-06 | `game/Assets/ThirdParty/Licenses/Kenney-Particle.txt` / CC0 | `Art/spark_01.png`, `smoke_01.png`。打撃・会心・回収の粒子。色・大きさ・移動・フェード追加 |
| Noto Sans CJK JP Regular | [配布元](https://github.com/notofonts/noto-cjk/tree/main/Sans) | 2026-09-06 | `game/Assets/ThirdParty/Licenses/Noto-OFL.txt` / SIL OFL 1.1 | `Fonts/NotoSansCJKjp-Regular.otf`。日本語UIと部位番号。フォント改変なし |

ファイル位置は `game/Assets/Resources/` 基準。必要な素材とライセンスだけをGitへ格納。最大ファイルはフォント約16.5 MBで、今回Git LFSは使わない。

結晶メッシュ、簡単なランプ・つるはし、UI、品質到達・会心・結果の短い音階は本プロジェクトのコードで生成。参考作品からの画像・音の抜き出しはない。

## 候補

2026-09-06の追加調査：無料の鉱石・宝石素材は見つかった。以下は未取得の候補。導入済み表とは区別する。[調査と用途の整理](research/2026-09-06-forging-reference-and-ore-assets.md)。

| 素材 | 確認した内容 | 用途候補 |
| --- | --- | --- |
| [KDRN / Ores and Minerals - Mini-pack (24)](https://kdrn.itch.io/free-ores-and-minerals-minipack-24) | CC0表記、24種、256×256個別アイコンと縮小前画像、ゲーム利用・表記不要の説明 | 主役の鉱石または報酬画像。写真調と2D背景の統一、部位差分は別に調整 |
| [greatdocbrown / Coins & Gems & Chests & More](https://greatdocbrown.itch.io/coins-gems-etc) | CC0表記、多くが16×16、宝石・箱・コイン、PNGとAseprite | ピクセル調の報酬と小物。大きな採取対象は別途用意 |

主役は鉱石画像を置くだけにせず、盤面に対応する母岩・亀裂・露出マスクを組み合わせる。適した素材がない場合は結晶1点と段階差分の自作を候補とする。この鉱石候補2件は未取得。P0a-02の導入・自作は冒頭の表を参照。

2026-09-06の初回感想後は2Dを第一候補にする。下記は当時の候補。KenneyはP0a-02で導入済み、効果音ラボは未導入。

| 配布元・素材 | 確認した条件 | 検討用途と採用時の確認 |
| --- | --- | --- |
| Kenney [Roguelike Caves & Dungeons](https://kenney.nl/assets/roguelike-caves-dungeons) | 公式に2D・520点・CC0と表記（2026-09-06確認） | 洞窟背景の第一候補。採取物の拡大表現に十分か、タイル粒度・縮尺・日本語UIとの調和、導入する個別ファイルと同梱ライセンスを確認 |
| [効果音ラボ](https://soundeffect-lab.info/agreement/) | 商用利用無料、クレジット任意、改変利用可。素材自体の再配布・改変素材の再配布・音源直リンク・AI学習は禁止。アプリへの操作音組み込みは再配布でない例として記載（2026-09-06確認） | 打撃・削り・会心・回収等の候補。採用する音の個別ページと当日の条件を確認し、ゲーム組み込みとソース共有の扱いを分ける。CC0として扱わず、音源を無条件にGitへ格納しない |

ほかのフリー3D素材も利用条件に沿えば候補にできるというユーザー許可あり。特定素材は未選定。採用時に商用利用、改変、必要な表記、ゲーム配布とGitでのソース／素材共有の可否を個別に記録する。

以下は従来候補。

| 配布元 | 素材 | 用途候補 | 導入時の確認 |
| --- | --- | --- | --- |
| Kenney | [Nature Kit](https://kenney.nl/assets/nature-kit) | 岩、採集場の背景 | 形式、縮尺、使用するモデル |
| Kenney | [Modular Cave Kit](https://kenney.nl/assets/modular-cave-kit) | 洞窟を採用した場合の背景 | 舞台との適合、組み合わせ |
| Kenney | [Particle Pack](https://kenney.nl/assets/particle-pack) | 粉、きらめきのテクスチャ | 完成したUnityエフェクトではなく素材として利用 |
| Kenney | [Impact Sounds](https://kenney.nl/assets/impact-sounds) | 打撃、石片、損傷 | 聞き分け、音量、繰り返し |
| Kenney | [Interface Sounds](https://kenney.nl/assets/interface-sounds) | 選択、確定、帰還 | 操作音の統一 |
| Kenney | [UI Pack](https://kenney.nl/assets/ui-pack) | パネル、ボタン、表示 | 日本語文字の余白、視認性 |

冒頭または旧版の導入表にない候補は未使用。追加導入時は配布ページと同梱ライセンスを再確認する。

## 実際に導入したら記録する項目

| 素材名・版 | 配布元URL | 取得日 | ライセンス・同梱文書の場所 | リポジトリ内の場所 | 使用箇所・改変 |
| --- | --- | --- | --- | --- | --- |

必要な素材だけを取り込み、元のライセンス文書を一緒に保持する。購入素材や再配布制限のある素材は、GitHubへの格納可否を確認する。参考作品から画像・音・UIを抜き出して使わない。
