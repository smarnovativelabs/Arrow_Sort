using System;
using System.Collections.Generic;
namespace WoolGame {
public static partial class WoolEndlessGenerator {
    // Early blocker colours keep part of their ammunition until the tail.
    // The legal exit witness remains available; extra slots/removal/sort/amplify
    // can relieve the resulting occupancy without an ability instruction.
    static void ApplyFiveSlotChallenge(WoolLevelDefinition data,int total) {
        var cars=data.vehicles;var order=data.tutorialOrder;
        int held=data.number<5?1:data.number<10?3:4;
        int first=(int)((uint)data.number*7u%10u);
        var reserved=new HashSet<int>();
        for(int i=0;i<held;i++){int color=(first+i*3)%10;reserved.Add(color);cars[order[i]].color=color;}
        var regular=new List<int>();for(int c=0;c<10;c++)if(!reserved.Contains(c))regular.Add(c);
        for(int i=held;i<order.Length;i++)
            cars[order[i]].color=regular[(int)(((uint)(i-held)+(uint)data.number)%(uint)regular.Count)];
        var yarn=new List<int>(total);
        for(int i=0;i<held;i++)AppendColorRun(yarn,cars[order[i]].color,2);
        var random=new Random(unchecked(data.number*104729+653));
        for(int start=held;start<order.Length;start+=3) {
            int count=Math.Min(3,order.Length-start);var remaining=new int[count];
            for(int j=0;j<count;j++)remaining[j]=cars[order[start+j]].ammo;
            // Open all three bows before finishing their ammunition.
            for(int j=0;j<count;j++){AppendColorRun(yarn,cars[order[start+j]].color,2);remaining[j]-=2;}
            bool pending=true;
            while(pending){pending=false;for(int j=0;j<count;j++)if(remaining[j]>0){
                int take=2*(1+random.Next(remaining[j]/2));
                AppendColorRun(yarn,cars[order[start+j]].color,take);
                remaining[j]-=take;if(remaining[j]>0)pending=true;
            }}
        }
        for(int i=0;i<held;i++)AppendColorRun(yarn,cars[order[i]].color,cars[order[i]].ammo-2);
        data.yarn=yarn.ToArray();data.requiredSlots=held+3;data.openSlots=4;
        data.snakeBatchSize=3;data.tutorial=false;data.generatorRevision=9;data.difficultyNote=string.Empty;
    }
    static void AppendColorRun(List<int> output,int color,int count) {
        for(int i=0;i<count;i++)output.Add(color);
    }
    // Split every vehicle's even ammunition into visible even colour groups.
    static void BuildOpeningGroups(List<int> output,List<WoolTile> tiles,List<int> order,int batch,int number) {
        var random=new Random(unchecked(number*104729+17));
        for(int start=0;start<order.Count;start+=batch){
            int count=Math.Min(batch,order.Count-start);var remaining=new int[count];
            for(int j=0;j<count;j++)remaining[j]=tiles[order[start+j]].ammo;
            bool pending=true;while(pending){pending=false;for(int j=0;j<count;j++)if(remaining[j]>0){
                int take=number==1?remaining[j]:2*(1+random.Next(remaining[j]/2));
                AppendColorRun(output,tiles[order[start+j]].color,take);
                remaining[j]-=take;if(remaining[j]>0)pending=true;
            }}
        }
    }
}
}
