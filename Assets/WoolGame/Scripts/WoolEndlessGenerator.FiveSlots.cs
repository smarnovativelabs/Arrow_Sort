using System;
using System.Collections.Generic;
namespace WoolGame {
public static partial class WoolEndlessGenerator {
    // Two early bows keep ammunition until the tail. Each following group
    // needs three distinct bows, then releases all three before the next group.
    // Following the legal vehicle witness therefore uses at most five slots.
    static void ApplyFiveSlotChallenge(WoolLevelDefinition data,int total){
        var cars=data.vehicles;var order=data.tutorialOrder;
        int first=(int)((uint)data.number*7u%10u),second=(first+3)%10;
        var regularColors=new List<int>(8);for(int c=0;c<10;c++)if(c!=first&&c!=second)regularColors.Add(c);
        cars[order[0]].color=first;cars[order[1]].color=second;
        for(int i=2;i<order.Length;i++)cars[order[i]].color=regularColors[(int)(((uint)(i-2)*3u+(uint)data.number)%8u)];
        var yarn=new List<int>(total){first,second};
        for(int start=2;start<order.Length;start+=3){
            int end=Math.Min(start+3,order.Length),ammo=0;
            for(int i=start;i<end;i++)ammo=Math.Max(ammo,cars[order[i]].ammo);
            for(int unit=0;unit<ammo;unit++)for(int i=start;i<end;i++)if(unit<cars[order[i]].ammo)yarn.Add(cars[order[i]].color);
        }
        for(int a=1;a<cars[order[0]].ammo;a++)yarn.Add(first);
        for(int a=1;a<cars[order[1]].ammo;a++)yarn.Add(second);
        data.yarn=yarn.ToArray();data.requiredSlots=5;data.openSlots=4;data.snakeBatchSize=3;data.tutorial=false;data.generatorRevision=8;
        data.difficultyNote=string.Empty;
    }
}
}

