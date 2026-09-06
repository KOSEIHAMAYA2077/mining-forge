using System;
using UnityEngine;
using UnityEngine.UI;

namespace MiningForge
{
    // Authored vector artwork. Each cell reveals its own pieces; gameplay RNG is never used.
    public class OreCellGraphic:MaskableGraphic
    {
        public int Index,TopRow,RightColumn;
        public float Progress;
        public bool Damaged,Broken;
        static readonly Color[] stone={Hex(0x526069),Hex(0x647079),Hex(0x788084),Hex(0x424f59),Hex(0x8c9290)};
        public static Color Hex(int c)=>new Color(((c>>16)&255)/255f,((c>>8)&255)/255f,(c&255)/255f);
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var rng=new System.Random(735+Index*91);float w=rectTransform.rect.width,h=rectTransform.rect.height;
            // Shared world-space crystals cross cell boundaries, rather than repeating an icon six times.
            Crystal(vh,52,18,56,476,Hex(0x368d98),Hex(0x83dccb));
            Crystal(vh,94,43,62,419,Hex(0x287986),Hex(0x54c2b5));
            Crystal(vh,24,151,35,271,Hex(0x226875),Hex(0x41a99b));
            Crystal(vh,192,8,55,211,Hex(0x4792a0),Hex(0x8ce9d2));
            Crystal(vh,244,51,47,174,Hex(0x286d86),Hex(0x60bfc5));
            Crystal(vh,145,91,36,148,Hex(0x2d8085),Hex(0x75d5bd));
            // Irregular rock fragments disappear from a stable ordering, exposing the core locally.
            for(int n=0;n<32;n++)
            {
                rng=new System.Random(735+Index*91+n*157);
                float x=3+(n%5)*29+(float)rng.NextDouble()*19-8,y=-3-(n/5)*20-(float)rng.NextDouble()*17;
                float radius=16+(float)rng.NextDouble()*13, threshold=(float)((n*13+Index*7)%32)/32f;
                float visibility=Mathf.Clamp01((1-Progress)-threshold+.10f);

                Vector2 center=new Vector2(x,y);var points=new Vector2[6];
                for(int k=0;k<6;k++){float a=(k*60+15)*Mathf.Deg2Rad;float rr=radius*(.78f+(float)rng.NextDouble()*.22f);points[k]=new Vector2(Mathf.Clamp(center.x+Mathf.Cos(a)*rr,0,w),Mathf.Clamp(center.y+Mathf.Sin(a)*rr,-h,0));}
                if(visibility<=0)continue;
                Color baseColor=stone[n%stone.Length];baseColor.a=Mathf.Clamp01(visibility*12);
                Poly(vh,baseColor,points);Color shade=Hex(0x2a3644);shade.a=baseColor.a;Poly(vh,shade,center,points[3],points[4],points[5]);
                Line(vh,points[1],points[2],2,new Color(.66f,.7f,.68f,baseColor.a*.7f));
                if(n%4==0)Poly(vh,new Color(.71f,.58f,.34f,baseColor.a),center,center+new Vector2(8,-3),center+new Vector2(4,-9));
            }
            if(Damaged)
            {
                Color crack=Broken?Hex(0x310c20):Hex(0x153141);
                Vector2[] path={new Vector2(27,-3),new Vector2(63,-33),new Vector2(47,-52),new Vector2(78,-72),new Vector2(54,-118)};
                for(int n=0;n<path.Length-1;n++)Line(vh,path[n],path[n+1],Broken?5:3,crack);
                Line(vh,path[2],new Vector2(7,-71),3,crack);Line(vh,path[3],new Vector2(129,-88),3,crack);
                if(Broken){Line(vh,new Vector2(14,-15),new Vector2(115,-104),3,Hex(0xd65e53));Line(vh,new Vector2(114,-18),new Vector2(18,-102),3,Hex(0xd65e53));}
            }
        }
        void Crystal(VertexHelper vh,float x,float top,float width,float length,Color leftColor,Color rightColor)
        {
            float l=x-width*.5f,r=x+width*.5f,shoulder=top+width*.65f,bottom=top+length;
            Facet(vh,Hex(0xb4f0da),new Vector2(x,top),new Vector2(r,shoulder),new Vector2(x,shoulder+12),new Vector2(l,shoulder));
            Facet(vh,leftColor,new Vector2(l,shoulder),new Vector2(x,shoulder+12),new Vector2(x,bottom),new Vector2(l+7,bottom-28));
            Facet(vh,rightColor,new Vector2(x,shoulder+12),new Vector2(r,shoulder),new Vector2(r-7,bottom-28),new Vector2(x,bottom));
            Facet(vh,new Color(.85f,1,.90f,.6f),new Vector2(x+3,shoulder+22),new Vector2(x+5,shoulder+22),new Vector2(x+5,bottom-38),new Vector2(x+3,bottom-35));
        }
        // Clip each shared facet to this cell; only its mother rock and damage change with progress.
        void Facet(VertexHelper vh,Color color,params Vector2[] global)
        {
            var points=new System.Collections.Generic.List<Vector2>();foreach(var p in global)points.Add(new Vector2(p.x-RightColumn*140,p.y-TopRow*128));
            for(int edge=0;edge<4;edge++)
            {
                var output=new System.Collections.Generic.List<Vector2>();if(points.Count==0)return;
                Vector2 a=points[points.Count-1];float da=Distance(a,edge);
                foreach(var b in points){float db=Distance(b,edge);if((da>=0)!=(db>=0))output.Add(Vector2.LerpUnclamped(a,b,da/(da-db)));if(db>=0)output.Add(b);a=b;da=db;}points=output;
            }
            for(int i=0;i<points.Count;i++)points[i]=new Vector2(points[i].x,-points[i].y);Poly(vh,color,points.ToArray());
        }
        float Distance(Vector2 p,int edge)=>edge==0?p.x+4:edge==1?136-p.x:edge==2?p.y+4:124-p.y;
        public static void Poly(VertexHelper vh,Color color,params Vector2[] points)
        {int start=vh.currentVertCount;foreach(var p in points)vh.AddVert(p,color,Vector2.zero);for(int i=1;i<points.Length-1;i++)vh.AddTriangle(start,start+i,start+i+1);}
        public static void Line(VertexHelper vh,Vector2 from,Vector2 to,float width,Color color)
        {Vector2 normal=new Vector2(-(to-from).y,(to-from).x).normalized*width*.5f;Poly(vh,color,from+normal,to+normal,to-normal,from-normal);}
    }
    public class OreOutlineGraphic:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();Color dark=OreCellGraphic.Hex(0x172933);
            OreCellGraphic.Poly(vh,dark,new Vector2(0,-18),new Vector2(31,0),new Vector2(270,0),new Vector2(286,-32),new Vector2(286,-248),new Vector2(270,-268),new Vector2(0,-268));
            OreCellGraphic.Poly(vh,dark,new Vector2(0,-230),new Vector2(144,-230),new Vector2(152,-470),new Vector2(110,-520),new Vector2(34,-515),new Vector2(0,-475));
        }
    }
}
