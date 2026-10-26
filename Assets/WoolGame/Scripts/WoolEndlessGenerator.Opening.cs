using System;
using System.Collections.Generic;
using UnityEngine;
namespace WoolGame
{
    public static partial class WoolEndlessGenerator
    {
        static WoolLevelDefinition GenerateOpening(int number,Vector3[] route)
        {
            var data=ScriptableObject.CreateInstance<WoolLevelDefinition>();
            data.hideFlags=HideFlags.DontSave;data.name="Opening level "+number;data.number=number;
            data.generatorRevision=6;data.route=(Vector3[])route.Clone();
            data.openSlots=4;data.colorCount=number==1?3:number==2?5:6;data.difficulty=DifficultyFor(number);
            data.boardRotation=0;data.boardScale=1;data.hasRail=false;data.containerPositions=Array.Empty<Vector2>();
            data.advanceSpeed=number==1?.18f:number==2?.22f:.27f;
            data.windingRate=4.2f;data.initialHeadProgress=.35f;data.fogAbove=7;data.snakeBatchSize=1;
            var tiles=new List<WoolTile>();var order=new List<int>();
            if(number==1)
            {
                var lesson=new WoolLevel(1);tiles.AddRange(lesson.Tiles);order.AddRange(lesson.Solution);
                foreach(var tile in tiles)
                {
                    tile.x*=1.25f;tile.y=tile.row==0?-3.6f:-6.1f;
                    tile.width=1.05f;tile.length=tile.size==WoolSize.Small?1.05f:tile.size==WoolSize.Medium?1.45f:1.85f;
                }
                data.tutorial=true;data.displayName="Your first rescue";data.layoutStyle="Guided crossbow rescue";
                data.difficultyNote="Learn color matching, direction, ammunition, and rescuing the duck family.";
            }
            else
            {
                int target=number==2?30:40;
                var random=new System.Random(number*73129+9049);
                Pack(tiles,random,0,0,8,10,24);Pack(tiles,random,8,0,10,10,24);
                Pack(tiles,random,0,10,8,14,24);Pack(tiles,random,8,10,10,14,24);
                while(tiles.Count>target)tiles.RemoveAt(random.Next(tiles.Count));
                if(tiles.Count!=target)throw new InvalidOperationException("Opening board vehicle count mismatch.");
                var witness=MakeWitness(tiles,random,WoolDifficulty.Easy,false,Array.Empty<Vector2>());
                foreach(var tile in witness)order.Add(tiles.IndexOf(tile));
                for(int i=0;i<tiles.Count;i++)tiles[i].color=Palette[(i+number)%data.colorCount];
                data.displayName=number==2?"Practice your rescue":"Ready for a challenge";
                data.layoutStyle="Opening practice";data.difficultyNote="A solvable "+target+"-vehicle board without depots or rail.";
            }
            var colors=new List<int>();
            foreach(int i in order)tiles[i].ammo=WoolLevel.Capacity(tiles[i].size);
            int batch=number==1?1:3;data.snakeBatchSize=batch;
            for(int start=0;start<order.Count;start+=batch){int end=Math.Min(order.Count,start+batch);int max=0;for(int j=start;j<end;j++)max=Math.Max(max,tiles[order[j]].ammo);for(int n=0;n<max;n++)for(int j=start;j<end;j++)if(n<tiles[order[j]].ammo)colors.Add(tiles[order[j]].color);}
            data.vehicles=tiles.ToArray();data.tutorialOrder=order.ToArray();data.yarn=colors.ToArray();
            var rules=new WoolLevel(data);for(int i=0;i<tiles.Count;i++)if(rules.CanSelect(i))data.initialLegalMoves++;
            data.chapterName="Rescue Trail";return data;
        }
    }
}