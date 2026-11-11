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
            data.generatorRevision=9;data.route=(Vector3[])route.Clone();
            data.openSlots=4;data.colorCount=number==1?3:Mathf.Min(6,3+(number-2)/2);data.difficulty=DifficultyFor(number);
            data.boardRotation=0;data.boardScale=1;data.hasRail=false;data.containerPositions=Array.Empty<Vector2>();
            data.advanceSpeed=number==1?.26f:.28f+(number-2)*.014f;data.startingGraceSeconds=20;
            data.windingRate=4.2f;data.initialHeadProgress=.35f;data.fogAbove=1000;data.snakeBatchSize=1;
            var tiles=new List<WoolTile>();var order=new List<int>();
            if(number==1)
            {
                var lesson=new WoolLevel(1);tiles.AddRange(lesson.Tiles);order.AddRange(lesson.Solution);
                foreach(var tile in tiles)
                {
                    tile.x*=1.25f;tile.y=tile.row==0?-3.6f:-5.9f;
                    tile.width=1.05f;tile.length=tile.size==WoolSize.Small?1.05f:tile.size==WoolSize.Medium?1.45f:1.85f;
                }
                data.tutorial=true;data.displayName="Your first rescue";data.layoutStyle="Guided crossbow rescue";
                data.difficultyNote="Learn color matching, direction, ammunition, and rescuing the duck family.";
            }
            else
            {
                int target=Math.Min(60,20+(number-2)*5);
                var random=new System.Random(number*73129+9049);
                int compactRows=24;int split=compactRows/2;
                Pack(tiles,random,0,0,8,split,compactRows);Pack(tiles,random,8,0,10,split,compactRows);
                Pack(tiles,random,0,split,8,compactRows-split,compactRows);Pack(tiles,random,8,split,10,compactRows-split,compactRows);
                while(tiles.Count>target)tiles.RemoveAt(random.Next(tiles.Count));
                if(tiles.Count!=target)throw new InvalidOperationException("Opening board vehicle count mismatch.");
                var witness=MakeWitness(tiles,random,WoolDifficulty.Easy,false,Array.Empty<Vector2>());
                foreach(var tile in witness)order.Add(tiles.IndexOf(tile));
                for(int i=0;i<order.Count;i++)tiles[order[i]].color=Palette[(i+number)%data.colorCount];
                data.displayName=number<=5?"Practice your rescue":"Growing confidence";
                data.layoutStyle="Opening practice";data.difficultyNote="A solvable "+target+"-vehicle board without depots or rail.";
            }
            var colors=new List<int>();
            foreach(int i in order)tiles[i].ammo=WoolLevel.Capacity(tiles[i].size);
            int batch=number==1?1:data.difficulty==WoolDifficulty.Hard?4:number>=7?3:2;data.snakeBatchSize=batch;data.requiredSlots=batch;
            BuildOpeningGroups(colors,tiles,order,batch,number);
            data.vehicles=tiles.ToArray();data.tutorialOrder=order.ToArray();data.yarn=colors.ToArray();
            var rules=new WoolLevel(data);for(int i=0;i<tiles.Count;i++)if(rules.CanSelect(i))data.initialLegalMoves++;
            data.chapterName="Rescue Trail";return data;
        }
    }
}