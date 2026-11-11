using System;
using System.Collections.Generic;
namespace WoolGame {
public static partial class WoolEndlessGenerator {
    // Early blocker colours keep part of their ammunition until the tail.
    // The legal exit witness remains available; extra slots/removal/sort/amplify
    // can relieve the resulting occupancy without an ability instruction.
    static void ApplyFiveSlotChallenge(WoolLevelDefinition data,int total) {
        var cars=data.vehicles;var order=data.tutorialOrder;
        var random=new Random(unchecked(data.number*104729+7919));
        // Choose among all currently legal exits, rather than following a column.
        var rules=new WoolLevel(data);var shuffledOrder=new List<int>(order.Length);
        for(int step=0;step<cars.Length;step++){
            var legal=new List<int>();
            for(int i=0;i<cars.Length;i++)if(cars[i].source==WoolVehicleSource.Board&&rules.CanSelect(i))legal.Add(i);
            if(legal.Count==0)break;
            int pick=legal[random.Next(legal.Count)];rules.Tiles[pick].selected=true;shuffledOrder.Add(pick);
        }
        foreach(int i in order)if(cars[i].source!=WoolVehicleSource.Board)shuffledOrder.Add(i);
        if(shuffledOrder.Count!=cars.Length)throw new InvalidOperationException("Random exit order is incomplete.");
        order=data.tutorialOrder=shuffledOrder.ToArray();
        // Three long-lived colours and two working colours create five-slot pressure.
        int held=3;
        int workingSlots=2;
        var palette=new List<int>();for(int c=0;c<10;c++)palette.Add(c);
        for(int i=palette.Count-1;i>0;i--){int j=random.Next(i+1);int c=palette[i];palette[i]=palette[j];palette[j]=c;}
        for(int i=0;i<held;i++)cars[order[i]].color=palette[i];
        int previous=-1;
        for(int i=held;i<order.Length;i++){
            int color;do{color=palette[held+random.Next(palette.Count-held)];}while(color==previous);
            cars[order[i]].color=color;previous=color;
        }
        var yarn=new List<int>(total);
        var heldRemaining=new int[held];
        for(int i=0;i<held;i++){
            AppendColorRun(yarn,cars[order[i]].color,2);
            heldRemaining[i]=cars[order[i]].ammo-2;
        }
        for(int start=held;start<order.Length;start+=workingSlots){
            int count=Math.Min(workingSlots,order.Length-start);var remaining=new int[count];
            for(int j=0;j<count;j++)remaining[j]=cars[order[start+j]].ammo;
            // Keep the rack occupied; extra slots give room to recover from a poor choice.
            for(int j=0;j<count;j++){AppendColorRun(yarn,cars[order[start+j]].color,2);remaining[j]-=2;}
            while(true){
                var pending=new List<int>();for(int j=0;j<count;j++)if(remaining[j]>0)pending.Add(j);
                if(pending.Count==0)break;
                int picked=pending[random.Next(pending.Count)];int take=2*(1+random.Next(Math.Min(2,remaining[picked]/2)));
                AppendColorRun(yarn,cars[order[start+picked]].color,take);remaining[picked]-=take;
                // Mix in occupied-bow colours without emptying their slots early.
                // Keep two shots for the tail so the player must plan around them.
                if(random.Next(100)<55){
                    var drips=new List<int>();for(int h=0;h<held;h++)if(heldRemaining[h]>2)drips.Add(h);
                    if(drips.Count>0){int h=drips[random.Next(drips.Count)];AppendColorRun(yarn,cars[order[h]].color,2);heldRemaining[h]-=2;}
                }
            }
        }
        // Release the long-lived colours in a shuffled order instead of a fixed cycle.
        var tailOrder=new List<int>();for(int i=0;i<held;i++)tailOrder.Add(i);
        while(tailOrder.Count>0){int pick=random.Next(tailOrder.Count);int h=tailOrder[pick];tailOrder.RemoveAt(pick);AppendColorRun(yarn,cars[order[h]].color,heldRemaining[h]);}
        data.yarn=yarn.ToArray();data.requiredSlots=held+workingSlots;data.openSlots=4;
        data.snakeBatchSize=3;data.tutorial=false;data.generatorRevision=14;data.difficultyNote=string.Empty;
    }
    static void AppendColorRun(List<int> output,int color,int count) {
        for(int i=0;i<count;i++)output.Add(color);
    }
    // Split every vehicle's even ammunition into visible even colour groups.
    static void BuildOpeningGroups(List<int> output,List<WoolTile> tiles,List<int> order,int batch,int number) {
        var random=new Random(unchecked(number*104729+17));
        for(int start=0;start<order.Count;start+=batch){
            int count=Math.Min(batch,order.Count-start);var remaining=new int[count];
            for(int j=0;j<count;j++){remaining[j]=tiles[order[start+j]].ammo;if(number>1){AppendColorRun(output,tiles[order[start+j]].color,2);remaining[j]-=2;}}
            bool pending=true;while(pending){pending=false;for(int j=0;j<count;j++)if(remaining[j]>0){
                int take=number==1?remaining[j]:2*(1+random.Next(remaining[j]/2));
                AppendColorRun(output,tiles[order[start+j]].color,take);
                remaining[j]-=take;if(remaining[j]>0)pending=true;
            }}
        }
    }
}
}
