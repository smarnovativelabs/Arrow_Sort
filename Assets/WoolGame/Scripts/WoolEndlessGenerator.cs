using System;
using System.Collections.Generic;
using UnityEngine;
namespace WoolGame
{
    // Bounded work: one requested board, 50-70 vehicles and 250-400 wool units.
    public static partial class WoolEndlessGenerator
    {
        public const int MaxLevel=int.MaxValue-1;
        public const int MaxVehicles=70;
        public const float RailY=-12.2f;
        const float Cell=.48f;
        static readonly int[] Palette={3,1,4,8,6,0,2,5,7,9};
        public static int DifficultyTier(int number)=>Math.Min(14,Math.Max(0,(number-1)/5));
        public static WoolDifficulty DifficultyFor(int number)=>number%5==0||DifficultyTier(number)>=2?WoolDifficulty.Hard:DifficultyTier(number)>=1||number>=2?WoolDifficulty.Medium:WoolDifficulty.Easy;
        public static WoolLevelDefinition Generate(int number,Vector3[] referenceRoute)
        {
            if(number<1||number>MaxLevel)throw new ArgumentOutOfRangeException(nameof(number));
            if(number<=3){var opening=GenerateOpening(number,referenceRoute);ConfigureAbilityLesson(opening);return opening;}
            var data=ScriptableObject.CreateInstance<WoolLevelDefinition>();data.hideFlags=HideFlags.DontSave;
            data.generatorRevision=5;data.number=number;data.name="Generated level "+number;data.difficulty=DifficultyFor(number);
            data.openSlots=4;data.boardScale=1;data.boardRotation=0;data.colorCount=10;
            data.hasRail=number>=21&&(number==21||number%4==0||number%7==0);
            int containers=number>=16&&!data.hasRail&&(number==16||number%3==0||number%5==0)?2:0;
            data.containerPositions=containers>0?new[]{new Vector2(-2.35f,-4.2f),new Vector2(2.35f,-7.4f)}:Array.Empty<Vector2>();

            int rows=data.hasRail?22:24;
            var random=new System.Random(unchecked(number*73129+9049));
            var list=new List<WoolTile>(MaxVehicles);
            int split=4+2*random.Next(2);
            Pack(list,random,0,0,8,split,rows);
            Pack(list,random,8,0,10,split,rows);
            Pack(list,random,0,split,8,rows-split,rows);
            Pack(list,random,8,split,10,rows-split,rows);
            // Reserve the full tunnel footprint before finding a legal exit sequence.
            foreach(var p in data.containerPositions)list.RemoveAll(t=>Mathf.Abs(t.x-p.x)<(t.direction==WoolDirection.Up?t.width:t.length)*.5f+.85f&&Mathf.Abs(t.y-p.y-.45f)<(t.direction==WoolDirection.Up?t.length:t.width)*.5f+1.4f);
            list.RemoveAll(t=>TunnelBlocks(t,t.direction,data.containerPositions)&&TunnelBlocks(t,(WoolDirection)(((int)t.direction+2)%4),data.containerPositions));
            // A few deliberate gaps change the silhouette without shrinking every piece.
            int goal=Math.Min(70,50+Math.Min(14,DifficultyTier(number)*2)+(int)((uint)number*13u%7u))-(data.hasRail?10:containers*6);
            while(list.Count>goal)list.RemoveAt(random.Next(list.Count));
            var witness=MakeWitness(list,random,data.difficulty,data.hasRail,data.containerPositions);
            if(data.hasRail)
            {
                for(int i=0;i<10;i++)list.Add(SourceTile(random,WoolVehicleSource.Rail,0,i,new Vector2(-3.65f+(i%6)*1.46f,RailY)));
            }
            for(int c=0;c<containers;c++)for(int i=0;i<6;i++)list.Add(SourceTile(random,WoolVehicleSource.Container,c,i,data.containerPositions[c]+Vector2.up*1.15f));
            if(list.Count<50||list.Count>MaxVehicles)throw new InvalidOperationException("Generator exceeded vehicle pool.");
            // Deterministic color variation without storing an ever-growing level history.
            list.Sort((a,b)=>{int v=a.source.CompareTo(b.source);if(v!=0)return v;v=a.sourceId.CompareTo(b.sourceId);if(v!=0)return v;v=a.y.CompareTo(b.y);if(v!=0)return v;v=a.x.CompareTo(b.x);return v!=0?v:a.sourceOrder.CompareTo(b.sourceOrder);});
            uint signature=unchecked((uint)number*2654435761u);
            for(int i=0;i<list.Count;i++){int digit=i<10?(int)(signature%10):random.Next(10);if(i<10)signature/=10;list[i].color=Palette[(digit+i*3)%10];}
            // Use the three designer vehicle sizes while balancing ammunition and footprints.
            var capacityOrder=new List<WoolTile>(list);capacityOrder.Sort((a,b)=>a.length.CompareTo(b.length));
            for(int i=0;i<capacityOrder.Count;i++)
            {
                var size=i<capacityOrder.Count*.50f?WoolSize.Small:i<capacityOrder.Count*.85f?WoolSize.Medium:WoolSize.Large;
                capacityOrder[i].size=size;capacityOrder[i].ammo=WoolLevel.Capacity(size);
            }
            data.vehicles=list.ToArray();
            var order=new List<int>();foreach(var tile in witness)order.Add(list.IndexOf(tile));
            for(int i=0;i<list.Count;i++)if(list[i].source!=WoolVehicleSource.Board)order.Add(i);
            data.tutorialOrder=order.ToArray();
            data.initialLegalMoves=0;var rules=new WoolLevel(data);
            for(int i=0;i<list.Count;i++)if(rules.CanSelect(i))data.initialLegalMoves++;
            ApplyChallenge(data);data.route=Route(number,referenceRoute);
            data.advanceSpeed=.32f+Math.Min(14,DifficultyTier(number))*.012f;data.windingRate=4.2f;data.initialHeadProgress=.35f;data.fogAbove=7;
            data.layoutStyle=data.hasRail?"Moving shuttle":containers>0?"Thread depots":"Patchwork junction";
            data.displayName=data.layoutStyle+" "+number;data.chapterName="Rescue Trail";

            data.evidence="Original on-demand layout. Shared meshes and a fixed pool. Scarce colors reserve slot capacity; authored route length controls time pressure.";
            ConfigureAbilityLesson(data);return data;
        }
        static WoolTile SourceTile(System.Random r,WoolVehicleSource source,int id,int order,Vector2 p)
        {
            var size=(WoolSize)r.Next(3);
            return new WoolTile{x=p.x,y=p.y,width=.78f,length=size==WoolSize.Small?.78f:size==WoolSize.Medium?1.02f:1.26f,size=size,ammo=WoolLevel.Capacity(size),direction=WoolDirection.Up,source=source,sourceId=id,sourceOrder=order};
        }
        static bool TunnelBlocks(WoolTile tile,WoolDirection direction,Vector2[] tunnels)
        {
            foreach(var p in tunnels)if(WoolLevel.BlocksPath(tile,new WoolTile{x=p.x,y=p.y+.45f,width=1.7f,length=2.8f,direction=WoolDirection.Up},direction))return true;
            return false;
        }
        static void Pack(List<WoolTile> result,System.Random random,int x,int y,int width,int height,int rows)
        {
            bool vertical=random.Next(2)==0;
            int lanes=(vertical?width:height)/2,extent=vertical?height:width;
            for(int lane=0;lane<lanes;lane++)for(int at=0;at<extent;)
            {
                int remain=extent-at;if(remain<2)break;
                int length=Math.Min(remain,2+random.Next(2));if(remain-length==1)length=length>2?length-1:length+1;
                int cx=x+(vertical?lane*2:at),cy=y+(vertical?at:lane*2);
                int w=vertical?2:length,h=vertical?length:2;
                var size=length==2?WoolSize.Small:length==3?WoolSize.Medium:WoolSize.Large;
                result.Add(new WoolTile{x=(cx+w*.5f-9)*Cell,y=.35f-(cy+h*.5f)*Cell,width=Cell*2-.09f,length=Cell*length-.09f,size=size,ammo=WoolLevel.Capacity(size),direction=vertical?WoolDirection.Up:WoolDirection.Right,row=cy,column=cx});at+=length;
            }
        }
        static List<WoolTile> MakeWitness(List<WoolTile> tiles,System.Random r,WoolDifficulty difficulty,bool sources,Vector2[] tunnels)
        {
            var gone=new bool[tiles.Count];var result=new List<WoolTile>(tiles.Count);
            for(int step=0;step<tiles.Count;step++)
            {
                int winner=-1;WoolDirection direction=0;float best=-1;
                for(int i=0;i<tiles.Count;i++)if(!gone[i])for(int d=0;d<4;d++)
                {
                    if(d%2!=(int)tiles[i].direction%2 || (sources&&d==2))continue;
                    var dir=(WoolDirection)d;bool clear=true;int removedBlockers=0;
                    foreach(var p in tunnels)if(WoolLevel.BlocksPath(tiles[i],new WoolTile{x=p.x,y=p.y+.45f,width=1.7f,length=2.8f,direction=WoolDirection.Up},dir)){clear=false;break;}
                    if(!clear)continue;
                    for(int j=0;j<tiles.Count;j++)if(j!=i&&WoolLevel.BlocksPath(tiles[i],tiles[j],dir)){if(!gone[j]){clear=false;break;}removedBlockers++;}
                    if(!clear)continue;
                    float score=(float)r.NextDouble()+removedBlockers*(difficulty==WoolDifficulty.Hard?8:difficulty==WoolDifficulty.Medium?5:3);
                    if(score>best){best=score;winner=i;direction=dir;}
                }
                if(winner<0)throw new InvalidOperationException("Partition layout has no exit witness.");
                tiles[winner].direction=direction;gone[winner]=true;result.Add(tiles[winner]);
            }
            return result;
        }
        static Vector3[] Route(int number,Vector3[] reference)
        {
            var result=(Vector3[])reference.Clone();float bend=(number%13-6)*.045f;
            for(int i=0;i<result.Length;i++){if((number&1)!=0)result[i].x=-result[i].x;result[i].x=Mathf.Clamp(result[i].x+Mathf.Sin(i*1.7f+number%19)*bend,-4.15f,4.15f);}
            return result;
        }
        public static void ApplyChallenge(WoolLevelDefinition data)
        {
            var cars=data.vehicles;var order=data.tutorialOrder;
            if(cars.Length<50||cars.Length>MaxVehicles||order.Length!=cars.Length)throw new InvalidOperationException("Challenge levels need 50-70 vehicles and a complete exit order.");
            int total=0;foreach(var car in cars){car.ammo=WoolLevel.Capacity(car.size);total+=car.ammo;}
            for(int i=0;total<250;i=(i+1)%cars.Length)
                if(cars[i].size<WoolSize.Large){int old=cars[i].ammo;cars[i].size++;cars[i].ammo=WoolLevel.Capacity(cars[i].size);total+=cars[i].ammo-old;}
            for(int i=cars.Length-1;total>400;i=(i+cars.Length-1)%cars.Length)
                if(cars[i].size>WoolSize.Small){int old=cars[i].ammo;cars[i].size--;cars[i].ammo=WoolLevel.Capacity(cars[i].size);total+=cars[i].ammo-old;}
            int scarce=(data.number%10)*7%10,count=data.difficulty==WoolDifficulty.Hard?3:2;
            foreach(var car in cars)if(car.color==scarce)car.color=(scarce+1+(data.number%8))%10;
            cars[order[0]].color=scarce;
            for(int i=1;i<count;i++)cars[order[order.Length-i]].color=scarce;
            var regular=new List<int>();int rareAmmo=0;
            foreach(int index in order){if(cars[index].color==scarce)rareAmmo+=cars[index].ammo;else regular.Add(index);}
            var result=new List<int>(total);
            // A held scarce bow leaves three usable slots; reserve room for later scarce cars too.
            int batch=4-count;data.snakeBatchSize=batch;
            for(int start=0;start<regular.Count;start+=batch)
            {
                int end=Math.Min(regular.Count,start+batch);
                for(int i=start;i<end;i++)result.Add(cars[regular[i]].color);
                for(int i=end-1;i>=start;i--)for(int a=1;a<cars[regular[i]].ammo;a++)result.Add(cars[regular[i]].color);
            }
            // One early unit and one middle unit cannot exhaust even the smallest scarce bow.
            result.Insert(Math.Min(2,result.Count),scarce);
            result.Insert(result.Count/2,scarce);
            for(int i=2;i<rareAmmo;i++)result.Add(scarce);
            data.yarn=result.ToArray();data.tutorial=false;data.generatorRevision=5;
            data.difficultyNote="A scarce color uses "+count+" vehicles, with wool at the front, middle and tail. Plan around its occupied slot.";
        }
    }
}
