using System;
using System.Collections.Generic;
using UnityEngine;
namespace WoolGame
{
    public static partial class WoolEndlessGenerator
    {
        // Rebuild only generated definitions owned by the running attempt. Vehicle count,
        // mechanic unlocks and campaign progress remain independent of presentation.
        public static void ReflowForPortrait(WoolLevelDefinition data,float screenAspect)
        {
            if(data.tutorial)return;
            var queued=new List<WoolTile>();int parked=0;
            foreach(var tile in data.vehicles)if(tile.source==WoolVehicleSource.Board)parked++;else queued.Add(tile);
            int width=screenAspect>2.4f?18:screenAspect>2?20:screenAspect>1.55f?22:24;
            int containerCount=data.containerPositions.Length;
            int height=Mathf.Max(8,Mathf.CeilToInt((parked*6.8f+containerCount*38)/width/2)*2);
            var random=new System.Random(unchecked(data.number*73129+22613));
            List<WoolTile> cars=null,witness=null;Vector2[] depots=null;
            for(int attempt=0;attempt<48;attempt++)
            {
                if(attempt>0&&attempt%8==0)height+=2;
                depots=new Vector2[containerCount];
                for(int c=0;c<containerCount;c++)depots[c]=new Vector2(containerCount==1?0:c==0?-2.35f:2.35f,.35f-height*Cell*(containerCount==1?.52f:c==0?.43f:.68f));
                cars=new List<WoolTile>();int sx=width>=22?12:8,sy=Mathf.Max(2,(height/2)/2*2);
                Pack(cars,random,0,0,sx,sy,height);Pack(cars,random,sx,0,width-sx,sy,height);
                Pack(cars,random,0,sy,sx,height-sy,height);Pack(cars,random,sx,sy,width-sx,height-sy,height);
                float center=(width*.5f-9)*Cell;
                foreach(var tile in cars)tile.x-=center;
                foreach(var depot in depots)
                    cars.RemoveAll(t=>Mathf.Abs(t.x-depot.x)<1.05f+((int)t.direction%2==0?t.width:t.length)*.5f&&Mathf.Abs(t.y-depot.y-.45f)<1.75f+((int)t.direction%2==0?t.length:t.width)*.5f);
                cars.RemoveAll(t=>TunnelBlocks(t,t.direction,depots)&&TunnelBlocks(t,(WoolDirection)(((int)t.direction+2)%4),depots));
                if(cars.Count<parked)continue;
                while(cars.Count>parked)cars.RemoveAt(random.Next(cars.Count));
                if(data.hasRail)
                {
                    float low=float.MaxValue;
                    foreach(var tile in cars)low=Mathf.Min(low,tile.y-((int)tile.direction%2==0?tile.length:tile.width)*.5f);
                    float shift=RailY+2.1f-low;foreach(var tile in cars)tile.y+=shift;
                }
                try{witness=MakeWitness(cars,random,data.difficulty,data.hasRail,depots);break;}
                catch(InvalidOperationException){witness=null;}
            }
            if(witness==null)throw new InvalidOperationException("No portrait exit plan for level "+data.number);
            var order=new List<int>();foreach(var tile in witness)order.Add(cars.IndexOf(tile));
            for(int i=0;i<cars.Count;i++)cars[i].color=Palette[(i*3+data.number)%Mathf.Max(3,data.colorCount)];
            foreach(var tile in queued)
            {
                if(tile.source==WoolVehicleSource.Container)
                {var p=depots[tile.sourceId]+Vector2.up*1.15f;tile.x=p.x;tile.y=p.y;}
                cars.Add(tile);order.Add(cars.Count-1);
            }
            data.vehicles=cars.ToArray();data.tutorialOrder=order.ToArray();data.containerPositions=depots;data.boardRotation=0;
            if(data.number<=10)
            {
                for(int i=0;i<order.Count;i++)cars[order[i]].color=Palette[(i+data.number)%data.colorCount];
                var colors=new List<int>();BuildOpeningGroups(colors,cars,order,data.snakeBatchSize,data.number);data.yarn=colors.ToArray();
            }
            else ApplyChallenge(data);
            data.initialLegalMoves=0;var rules=new WoolLevel(data);
            for(int i=0;i<rules.Tiles.Count;i++)if(rules.CanSelect(i))data.initialLegalMoves++;
        }
    }
}
