# 使用素材と出典

更新日：2026-09-06。P0a-01へ下記素材を導入。公式ページのCC0表記と同梱ライセンスを確認済み。

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

| 配布元 | 素材 | 用途候補 | 導入時の確認 |
| --- | --- | --- | --- |
| Kenney | [Nature Kit](https://kenney.nl/assets/nature-kit) | 岩、採集場の背景 | 形式、縮尺、使用するモデル |
| Kenney | [Modular Cave Kit](https://kenney.nl/assets/modular-cave-kit) | 洞窟を採用した場合の背景 | 舞台との適合、組み合わせ |
| Kenney | [Particle Pack](https://kenney.nl/assets/particle-pack) | 粉、きらめきのテクスチャ | 完成したUnityエフェクトではなく素材として利用 |
| Kenney | [Impact Sounds](https://kenney.nl/assets/impact-sounds) | 打撃、石片、損傷 | 聞き分け、音量、繰り返し |
| Kenney | [Interface Sounds](https://kenney.nl/assets/interface-sounds) | 選択、確定、帰還 | 操作音の統一 |
| Kenney | [UI Pack](https://kenney.nl/assets/ui-pack) | パネル、ボタン、表示 | 日本語文字の余白、視認性 |

候補のうち上の導入表にないものは未使用。追加導入時は配布ページと同梱ライセンスを再確認する。

## 実際に導入したら記録する項目

| 素材名・版 | 配布元URL | 取得日 | ライセンス・同梱文書の場所 | リポジトリ内の場所 | 使用箇所・改変 |
| --- | --- | --- | --- | --- | --- |

必要な素材だけを取り込み、元のライセンス文書を一緒に保持する。購入素材や再配布制限のある素材は、GitHubへの格納可否を確認する。参考作品から画像・音・UIを抜き出して使わない。
