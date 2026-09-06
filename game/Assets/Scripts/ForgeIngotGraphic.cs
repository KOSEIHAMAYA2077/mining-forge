using UnityEngine;
using UnityEngine.UI;

namespace MiningForge
{
    public sealed class ForgeIngotGraphic : MaskableGraphic
    {
        public string Shape;
        Texture2D metal;
        public override Texture mainTexture => metal ? metal : Texture2D.whiteTexture;
        protected override void OnEnable(){base.OnEnable();metal=Resources.Load<Texture2D>("ForgeArt/hot-metal");SetMaterialDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(Shape=="ring")
            {
                for(int i=0;i<48;i++)
                {
                    float a=i*Mathf.PI*2/48,b=(i+1)*Mathf.PI*2/48;
                    Quad(vh,new Vector2(132+116*Mathf.Cos(a),180+52*Mathf.Sin(a)),new Vector2(132+116*Mathf.Cos(b),180+52*Mathf.Sin(b)),new Vector2(132+88*Mathf.Cos(b),180+27*Mathf.Sin(b)),new Vector2(132+88*Mathf.Cos(a),180+27*Mathf.Sin(a)),color);
                }
            }
            else if(Shape=="blade")
            {
                Piece(vh,new[]{new Vector2(65,6),new Vector2(110,71),new Vector2(92,234),new Vector2(38,234),new Vector2(20,71)});
                Piece(vh,new[]{new Vector2(12,226),new Vector2(118,226),new Vector2(122,247),new Vector2(8,247)});
                Piece(vh,new[]{new Vector2(50,244),new Vector2(80,244),new Vector2(85,350),new Vector2(45,350)});
            }
            else if(Shape=="shield")
                Piece(vh,new[]{new Vector2(29,8),new Vector2(235,8),new Vector2(259,43),new Vector2(230,168),new Vector2(132,233),new Vector2(34,168),new Vector2(5,43)});
            else if(Shape=="cross")
            {
                Piece(vh,new[]{new Vector2(45,55),new Vector2(85,55),new Vector2(89,349),new Vector2(42,357)});
                Piece(vh,new[]{new Vector2(24,12),new Vector2(237,12),new Vector2(257,61),new Vector2(237,109),new Vector2(24,109),new Vector2(7,61)});
            }
            else if(Shape=="axe")
            {
                Piece(vh,new[]{new Vector2(45,173),new Vector2(85,173),new Vector2(89,469),new Vector2(42,477)});
                Piece(vh,new[]{new Vector2(19,38),new Vector2(79,17),new Vector2(109,79),new Vector2(109,160),new Vector2(78,229),new Vector2(19,208)});
                Piece(vh,new[]{new Vector2(89,64),new Vector2(166,48),new Vector2(232,7),new Vector2(258,75),new Vector2(261,162),new Vector2(239,231),new Vector2(162,192),new Vector2(89,176)});
            }
            else
                Piece(vh,new[]{new Vector2(27,8),new Vector2(236,8),new Vector2(258,35),new Vector2(258,448),new Vector2(228,475),new Vector2(36,475),new Vector2(7,448),new Vector2(7,35)});
        }
        void Piece(VertexHelper vh,Vector2[] points)
        {
            Vector2 center=Vector2.zero;foreach(var point in points)center+=point;center/=points.Length;
            for(int i=0;i<points.Length;i++)
            {
                Vector2 a=points[i],b=points[(i+1)%points.Length],innerA=Vector2.Lerp(a,center,.055f),innerB=Vector2.Lerp(b,center,.055f);
                Quad(vh,a+new Vector2(5,8),b+new Vector2(5,8),innerB+new Vector2(5,8),innerA+new Vector2(5,8),new Color(.08f,.06f,.04f,.9f));
                Quad(vh,a,b,innerB,innerA,color*(i%3==0?1.25f:.58f));
                Triangle(vh,center,innerA,innerB,color);
            }
        }
        void Vertex(VertexHelper vh,Vector2 p,Color tint)=>vh.AddVert(new Vector3(p.x,-p.y),tint,new Vector2(p.x/264,1-p.y/480));
        void Triangle(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color tint)
        {int n=vh.currentVertCount;Vertex(vh,a,tint);Vertex(vh,b,tint);Vertex(vh,c,tint);vh.AddTriangle(n,n+1,n+2);}
        void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color tint)
        {int n=vh.currentVertCount;Vertex(vh,a,tint);Vertex(vh,b,tint);Vertex(vh,c,tint);Vertex(vh,d,tint);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);}
    }
}
