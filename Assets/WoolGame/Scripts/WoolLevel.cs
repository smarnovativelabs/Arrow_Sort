using System;
using System.Collections.Generic;

namespace WoolGame
{
    public enum WoolDirection { Up, Right, Down, Left }
    public enum WoolSize { Small, Medium, Large }
    public enum WoolVehicleSource { Board, Rail, Container }

    [Serializable]
    public sealed class WoolTile
    {
        public int row, column, color, ammo;
        public float x,y,width=.72f,length=1.02f;
        public WoolDirection direction;
        public WoolSize size;
        public bool selected;
        public WoolVehicleSource source;
        public int sourceId,sourceOrder;
        [NonSerialized] public bool hidden;
    }

    // Deterministic level rules can be exercised without a scene or renderer.
    public sealed class WoolLevel
    {
        public readonly List<WoolTile> Tiles = new List<WoolTile>();
        public readonly List<int> Yarn = new List<int>();
        public readonly List<int> Solution = new List<int>();
        public int Number { get; private set; }
        public int Rows { get; private set; }
        readonly List<WoolTile> tunnelObstacles=new List<WoolTile>();
        public const int Columns = 5;
        public const int Slots = 8;
        public static int Capacity(WoolSize size) { return size == WoolSize.Small ? 4 : size == WoolSize.Medium ? 6 : 10; }

        public WoolLevel(WoolLevelDefinition definition)
        {
            Number=definition.number;Rows=0;
            foreach(var p in definition.containerPositions)tunnelObstacles.Add(new WoolTile{x=p.x,y=p.y,width=1.65f,length=1.75f,direction=WoolDirection.Up});
            foreach(var t in definition.vehicles)Tiles.Add(new WoolTile {row=t.row,column=t.column,color=t.color,ammo=t.ammo,direction=t.direction,size=t.size,x=t.x,y=t.y,width=t.width,length=t.length,source=t.source,sourceId=t.sourceId,sourceOrder=t.sourceOrder,hidden=t.source!=WoolVehicleSource.Board});
            Yarn.AddRange(definition.yarn);Solution.AddRange(definition.tutorialOrder);
            var remaining=new int[WoolArt.Palette.Length];
            foreach(var tile in Tiles){tile.ammo=Capacity(tile.size);remaining[tile.color]+=tile.ammo;}
            for(int i=0;i<Yarn.Count;){int color=Yarn[i];if(remaining[color]>0){remaining[color]--;i++;}else Yarn.RemoveAt(i);}
            for(int color=0;color<remaining.Length;color++)for(int n=0;n<remaining[color];n++)Yarn.Add(color);
        }

        public WoolLevel(int number)
        {
            Number = Math.Max(1, number);
            Rows = Number == 1 ? 2 : Number < 5 ? 3 : 4;
            if (Number == 1)
            {
                // User recording, 00:15: purple/blue/red with three piece sizes.
                // Capacities are tunable reconstruction values, not extracted numbers.
                AddTile(0, 1, 3, WoolSize.Large, WoolDirection.Up);
                AddTile(0, 2, 1, WoolSize.Medium, WoolDirection.Right);
                AddTile(0, 3, 0, WoolSize.Small, WoolDirection.Right);
                AddTile(1, 1, 1, WoolSize.Small, WoolDirection.Left);
                AddTile(1, 2, 0, WoolSize.Medium, WoolDirection.Left);
                AddTile(1, 3, 3, WoolSize.Large, WoolDirection.Down);

                Solution.AddRange(new[] { 5, 2, 1, 0, 3, 4 });
                BuildYarn();
                return;
            }

            int colors = Math.Min(8, 3 + (Number - 1) / 3);
            var random = new Random(unchecked(Number * 7919 + 37));
            for (int row = 0; row < Rows; row++)
                for (int col = 0; col < Columns; col++)
                {
                    int color = (col + row + random.Next(colors)) % colors;
                    var size = (WoolSize)random.Next(3);
                    AddTile(row, col, color, size, WoolDirection.Up);
                }
            BuildEscapePlan(random);
            BuildYarn();
        }

        void AddTile(int row, int column, int color, WoolSize size, WoolDirection direction)
        {
            Tiles.Add(new WoolTile { row = row, column = column, color = color, size = size, ammo = Capacity(size), direction = direction, x=(column-2)*1.48f,y=-3.65f-row*1.46f,width=.72f,length=size==WoolSize.Small?.72f:size==WoolSize.Medium?1.02f:1.32f });
        }

        void BuildEscapePlan(Random random)
        {
            var removed = new bool[Tiles.Count];
            var candidates = new List<int>();
            while (Solution.Count < Tiles.Count)
            {
                candidates.Clear();
                for (int i = 0; i < Tiles.Count; i++)
                {
                    if (removed[i]) continue;
                    for (int d = 0; d < 4; d++)
                    {
                        bool clear = true;
                        for (int j = 0; j < Tiles.Count; j++)
                            if (i != j && !removed[j] && BlocksPath(Tiles[i], Tiles[j], (WoolDirection)d))
                            { clear = false; break; }
                        if (clear) candidates.Add(i * 4 + d);
                    }
                }
                if (candidates.Count == 0) throw new InvalidOperationException("Board has no escape route.");
                int choice = candidates[random.Next(candidates.Count)];
                int index = choice / 4;
                Tiles[index].direction = (WoolDirection)(choice % 4);
                removed[index] = true;
                Solution.Add(index);
            }
        }

        void BuildYarn()
        {
            // Match wool arrival order to a legal escape order. Following this
            // witness always clears each crossbow before another slot is required.
            foreach (int index in Solution)
                for (int k = 0; k < Tiles[index].ammo; k++) Yarn.Add(Tiles[index].color);
        }

        public bool CanSelect(int index)
        {
            if (index < 0 || index >= Tiles.Count || Tiles[index].selected||Tiles[index].hidden) return false;
            WoolTile tile = Tiles[index];
            for(int i=0;i<tunnelObstacles.Count;i++)
                if(!(tile.source==WoolVehicleSource.Container&&tile.sourceId==i)&&BlocksPath(tile,tunnelObstacles[i],tile.direction))return false;
            for (int i = 0; i < Tiles.Count; i++)
                if (i != index && !Tiles[i].selected && !Tiles[i].hidden && BlocksPath(tile, Tiles[i], tile.direction))
                    return false;
            return true;
        }

        // Full swept AABB, including partial lane overlaps. Direction belongs to
        // the tested move; the other tile keeps its actual oriented footprint.
        public static bool BlocksPath(WoolTile tile,WoolTile other,WoolDirection direction)
        {
            bool vertical=direction==WoolDirection.Up||direction==WoolDirection.Down;
            bool otherVertical=other.direction==WoolDirection.Up||other.direction==WoolDirection.Down;
            float ax=vertical?tile.width:tile.length,ay=vertical?tile.length:tile.width;
            float bx=otherVertical?other.width:other.length,by=otherVertical?other.length:other.width;
            const float epsilon=.001f;
            if(vertical)
            {
                if(System.Math.Abs(tile.x-other.x)>=(ax+bx)*.5f-epsilon)return false;
                return direction==WoolDirection.Up?other.y+by*.5f>tile.y-ay*.5f+epsilon:other.y-by*.5f<tile.y+ay*.5f-epsilon;
            }
            if(System.Math.Abs(tile.y-other.y)>=(ay+by)*.5f-epsilon)return false;
            return direction==WoolDirection.Right?other.x+bx*.5f>tile.x+ax*.5f-epsilon:other.x-bx*.5f<tile.x-ax*.5f+epsilon;
        }

        public int RemainingAmmo(int color)
        {
            int total = 0;
            foreach (var tile in Tiles) if (tile.color == color && !tile.selected) total += tile.ammo;
            return total;
        }

        public bool RemoveBox(int index,List<float> remainingAmounts=null)
        {
            if(remainingAmounts!=null&&remainingAmounts.Count!=Yarn.Count)
                throw new ArgumentException("Wool colors and remaining amounts must have matching lengths.",nameof(remainingAmounts));
            if(index<0||index>=Tiles.Count||Tiles[index].selected)return false;
            var tile=Tiles[index];
            float available=0;
            for(int i=0;i<Yarn.Count;i++)if(Yarn[i]==tile.color)available+=remainingAmounts==null?1:remainingAmounts[i];
            if(available+.0001f<tile.ammo)return false;
            tile.selected=true;float remaining=tile.ammo;
            for(int i=Yarn.Count-1;i>=0&&remaining>.00001f;i--)
            {
                if(Yarn[i]!=tile.color)continue;
                float amount=remainingAmounts==null?1:remainingAmounts[i];
                float take=Math.Min(remaining,amount);remaining-=take;amount-=take;
                if(amount<=.00001f){Yarn.RemoveAt(i);if(remainingAmounts!=null)remainingAmounts.RemoveAt(i);}
                else if(remainingAmounts!=null)remainingAmounts[i]=amount;
            }
            return true;
        }

        public void SortColors(int use,List<float> remainingAmounts=null)
        {
            if(remainingAmounts!=null&&remainingAmounts.Count!=Yarn.Count)
                throw new ArgumentException("Wool colors and remaining amounts must have matching lengths.",nameof(remainingAmounts));
            var random=new Random(unchecked(Number*7919+use*104729));
            for(int i=Yarn.Count-1;i>0;i--)
            {
                int j=random.Next(i+1),value=Yarn[i];Yarn[i]=Yarn[j];Yarn[j]=value;
                // A partially wound section retains its exact color and quantity.
                if(remainingAmounts!=null){float amount=remainingAmounts[i];remainingAmounts[i]=remainingAmounts[j];remainingAmounts[j]=amount;}
            }
        }

    }

    public static class WoolBoosters
    {
        public static readonly string[] Names={"Unlock","Remove","Sort","Amplify"};
        public static readonly int[] Prices={800,600,700,500};
        public static readonly int[] UnlockLevels={2,4,6,8};
        public static bool Available(int kind,int level)=>kind>=0&&kind<4&&level>=UnlockLevels[kind];
        public static readonly string[] Descriptions={"Unlock a new slot","Remove any box","Refresh the dragon body colors","Collect hidden dragon wool for a short time"};
    }
}

