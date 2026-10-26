using System;
using System.Collections.Generic;
using UnityEngine;

namespace WoolGame.Editor
{
    // Pure data authoring operations: callers own Undo, dirty flags and asset saves.
    // A departure witness proves arrow clearance, not timed four-slot solvability.
    public static class WoolManualLevelUtility
    {
        static int ColorCount => WoolArt.Palette.Length;

        public static List<string> Validate(WoolLevelDefinition data, int vehicleLimit = 70, int yarnLimit = 400)
        {
            var errors = new List<string>();
            if (!data) { errors.Add("No level definition is selected."); return errors; }
            if (data.number < 1) errors.Add("Level number must be at least one.");
            if(data.number%5==0&&data.difficulty!=WoolDifficulty.Hard)errors.Add("Every fifth level must be Hard.");
            if(!Finite(data.advanceSpeed)||data.advanceSpeed<=0||!Finite(data.windingRate)||data.windingRate<=0)errors.Add("Dragon and winding speeds must be positive and finite.");
            if(!Finite(data.fogAbove))errors.Add("Fog boundary must be finite.");
            if (data.openSlots != 4) errors.Add("Every level must start with four working slots.");
            if (!Finite(data.boardRotation)) errors.Add("Board rotation must be finite.");
            if (!Finite(data.boardScale) || data.boardScale <= 0) errors.Add("Board scale must be positive and finite.");
            if (data.snakeBatchSize < 1) errors.Add("Snake batch size must be at least one.");
            if (data.vehicles == null || data.vehicles.Length < 50) errors.Add("Every level requires at least 50 vehicles.");
            else if (data.vehicles.Length > vehicleLimit) errors.Add("Vehicle count exceeds the pool limit of " + vehicleLimit + ".");
            if (data.yarn == null || data.yarn.Length < 250) errors.Add("Every dragon requires at least 250 segments.");
            else if (data.yarn.Length > yarnLimit) errors.Add("Wool count exceeds the pool limit of " + yarnLimit + ".");

            bool geometryValid = data.vehicles != null && data.vehicles.Length > 0;
            var required = new long[ColorCount];
            if (data.vehicles != null)
            {
                for (int i = 0; i < data.vehicles.Length; i++)
                {
                    var tile = data.vehicles[i];
                    if (tile == null) { errors.Add("Vehicle " + i + " is missing."); geometryValid = false; continue; }
                    if (!ValidSize(tile.size)) errors.Add("Vehicle " + i + " has an invalid size.");
                    else if (tile.ammo != WoolLevel.Capacity(tile.size)) errors.Add("Vehicle " + i + " must have capacity " + WoolLevel.Capacity(tile.size) + ".");
                    if (!ValidColor(tile.color)) errors.Add("Vehicle " + i + " has an invalid color.");
                    else required[tile.color] += tile.ammo;
                    if (!ValidDirection(tile.direction)) { errors.Add("Vehicle " + i + " has an invalid arrow direction."); geometryValid = false; }
                    if (!Finite(tile.x) || !Finite(tile.y)) { errors.Add("Vehicle " + i + " has a non-finite position."); geometryValid = false; }
                    if (!Finite(tile.width) || !Finite(tile.length) || tile.width <= 0 || tile.length <= 0)
                    { errors.Add("Vehicle " + i + " must have a positive, finite footprint."); geometryValid = false; }
                    if (tile.selected) errors.Add("Vehicle " + i + " is saved as already selected.");
                    if(Finite(tile.x)&&Finite(tile.y)&&Finite(data.boardRotation)&&Finite(data.boardScale))
                    {
                        bool vertical=tile.direction==WoolDirection.Up||tile.direction==WoolDirection.Down;
                        float w=vertical?tile.width:tile.length,h=vertical?tile.length:tile.width;
                        for(int corner=0;corner<4;corner++)
                        {
                            var p=new Vector3(0,-3.2f,0)+Quaternion.Euler(0,0,data.boardRotation)*new Vector3(tile.x+(corner%2==0?-1:1)*w*.5f,tile.y+3.2f+(corner<2?-1:1)*h*.5f,0)*data.boardScale;
                            if(p.x < -4.59f||p.x>4.59f||p.y<(data.tutorial?-12.25f:-12.25f)||p.y>(data.tutorial?-2.36f:.49f)){errors.Add("Vehicle "+i+" extends outside the departure borders. Move it inward or reduce board scale.");break;}
                        }
                    }
                }
            }
            var actual = new long[ColorCount];
            if (data.yarn != null)
                for (int i = 0; i < data.yarn.Length; i++)
                {
                    int color = data.yarn[i];
                    if (!ValidColor(color)) errors.Add("Wool unit " + i + " has an invalid color.");
                    else actual[color]++;
                }
            for (int color = 0; color < ColorCount; color++)
                if (required[color] != actual[color]) errors.Add("Color " + color + " needs " + required[color] + " wool units but has " + actual[color] + ".");

            if (data.route == null || data.route.Length < 2) errors.Add("The route needs at least two points.");
            else
            {
                bool distinct = false;
                for (int i = 0; i < data.route.Length; i++)
                {
                    var p = data.route[i];
                    if (!Finite(p.x) || !Finite(p.y) || !Finite(p.z)) errors.Add("Route point " + i + " is not finite.");
                    if (i > 0 && (p - data.route[0]).sqrMagnitude > .000001f) distinct = true;
                }
                if (!distinct) errors.Add("The route needs at least two distinct points.");
            }
            if (geometryValid)
            {
                for (int i = 0; i < data.vehicles.Length; i++)
                    for (int j = i + 1; j < data.vehicles.Length; j++)
                        if (data.vehicles[i].source==WoolVehicleSource.Board&&data.vehicles[j].source==WoolVehicleSource.Board&&Overlap(data.vehicles[i], data.vehicles[j])) errors.Add("Vehicles " + i + " and " + j + " overlap.");
                int count = DepartureOrder(data).Length;
                if (count != data.vehicles.Length) errors.Add("Arrow layout is blocked: only " + count + " of " + data.vehicles.Length + " vehicles can escape.");
            }
            return errors;
        }

        public static int[] DepartureOrder(WoolLevelDefinition data)
        {
            if (!HasGeometry(data)) return Array.Empty<int>();
            var rules = CreateRules(data);
            var order = new List<int>(rules.Tiles.Count);
            bool changed;
            do
            {
                changed = false;
                for (int i = 0; i < rules.Tiles.Count; i++)
                {
                    if (!rules.CanSelect(i)) continue;
                    rules.Tiles[i].selected = true;
                    order.Add(i);
                    changed = true;
                }
            } while (changed && order.Count < rules.Tiles.Count);
            // Source queues become available as their visible front leaves. Their
            // timing is a runtime concern, not a simultaneous overlap on the board.
            if(order.Count==Array.FindAll(data.vehicles,t=>t.source==WoolVehicleSource.Board).Length)
                for(int i=0;i<data.vehicles.Length;i++)if(data.vehicles[i].source!=WoolVehicleSource.Board)order.Add(i);
            return order.ToArray();
        }

        public static void BalanceYarn(WoolLevelDefinition data)
        {
            int[] required = RequiredCounts(data);
            var result = new List<int>(data.yarn ?? Array.Empty<int>());
            var actual = new int[ColorCount];
            foreach (int color in result) if (ValidColor(color)) actual[color]++;
            // Remove the last surplus occurrence, retaining the longest possible
            // original prefix and the relative order of all retained wool units.
            for (int i = result.Count - 1; i >= 0; i--)
            {
                int color = result[i];
                if (!ValidColor(color)) { result.RemoveAt(i); continue; }
                if (actual[color] <= required[color]) continue;
                actual[color]--; result.RemoveAt(i);
            }
            // Append deficits in the authored vehicle color order.
            foreach (var tile in data.vehicles)
                while (actual[tile.color] < required[tile.color]) { result.Add(tile.color); actual[tile.color]++; }
            data.yarn = result.ToArray();
        }

        public static void RebuildYarn(WoolLevelDefinition data)
        {
            RequiredCounts(data); // Validate source data before changing any field.
            int[] order = DepartureOrder(data);
            if (order.Length != data.vehicles.Length)
                throw new InvalidOperationException("Cannot rebuild wool while some arrows have no escape route.");
            // A valid authored witness is more specific than a new greedy witness.
            if (ValidWitness(data, data.tutorialOrder)) order = (int[])data.tutorialOrder.Clone();
            int batch = Math.Max(1, Math.Min(data.snakeBatchSize, Math.Max(1, order.Length)));
            var result = new List<int>();
            for (int offset = 0; offset < order.Length; offset += batch)
            {
                int count = Math.Min(batch, order.Length - offset);
                var remaining = new int[count];
                for (int i = 0; i < count; i++) remaining[i] = WoolLevel.Capacity(data.vehicles[order[offset + i]].size);
                if (batch > 1)
                    for (int i = 0; i < count; i++)
                    {
                        int take = Math.Min(2, remaining[i]); remaining[i] -= take;
                        for (int unit = 0; unit < take; unit++) result.Add(data.vehicles[order[offset + i]].color);
                    }
                for (int step = 0; step < count; step++)
                {
                    int i = batch <= 3 ? step : count - 1 - step;
                    for (int unit = 0; unit < remaining[i]; unit++) result.Add(data.vehicles[order[offset + i]].color);
                }
            }
            data.yarn = result.ToArray();
            data.tutorialOrder = order;
        }

        public static void UpdateDerivedData(WoolLevelDefinition data)
        {
            if (!data) throw new ArgumentNullException(nameof(data));
            data.openSlots = 4;
            var colors = new HashSet<int>();
            if (data.vehicles != null)
                foreach (var tile in data.vehicles)
                {
                    if (tile == null) continue;
                    if (ValidSize(tile.size)) tile.ammo = WoolLevel.Capacity(tile.size);
                    tile.selected = false;
                    if (ValidColor(tile.color)) colors.Add(tile.color);
                }
            data.colorCount = colors.Count;
            data.initialLegalMoves = 0;
            if (!HasGeometry(data)) return;
            var rules = CreateRules(data);
            for (int i = 0; i < rules.Tiles.Count; i++) if (rules.CanSelect(i)) data.initialLegalMoves++;
            if (ValidWitness(data, data.tutorialOrder)) return;
            int[] order = DepartureOrder(data);
            if (order.Length == data.vehicles.Length) data.tutorialOrder = order;
        }

        static int[] RequiredCounts(WoolLevelDefinition data)
        {
            if (!data) throw new ArgumentNullException(nameof(data));
            if (data.vehicles == null || data.vehicles.Length == 0)
                throw new InvalidOperationException("Add vehicles before building the wool sequence.");
            var required = new int[ColorCount];
            foreach (var tile in data.vehicles)
            {
                if (tile == null || !ValidColor(tile.color) || !ValidSize(tile.size))
                    throw new InvalidOperationException("All vehicles need valid colors and sizes before building wool.");
                checked { required[tile.color] += WoolLevel.Capacity(tile.size); }
            }
            return required;
        }

        static bool ValidWitness(WoolLevelDefinition data, int[] order)
        {
            if (!HasGeometry(data) || order == null || order.Length != data.vehicles.Length) return false;
            var rules = CreateRules(data);
            foreach (int index in order)
            {
                if(index<0||index>=rules.Tiles.Count)return false;
                if(rules.Tiles[index].source!=WoolVehicleSource.Board)continue;
                if (!rules.CanSelect(index)) return false;
                rules.Tiles[index].selected = true;
            }
            return true;
        }

        static WoolLevel CreateRules(WoolLevelDefinition data)
        {
            // The rule constructor copies every tile. Missing unrelated arrays
            // should not prevent geometry inspection while an asset is being edited.
            var temporary = ScriptableObject.CreateInstance<WoolLevelDefinition>();
            try
            {
                temporary.number = data.number;
                temporary.vehicles = data.vehicles;
                temporary.yarn = Array.Empty<int>();
                temporary.tutorialOrder = Array.Empty<int>();
                return new WoolLevel(temporary);
            }
            finally { UnityEngine.Object.DestroyImmediate(temporary); }
        }

        static bool HasGeometry(WoolLevelDefinition data)
        {
            if (!data || data.vehicles == null || data.vehicles.Length == 0) return false;
            foreach (var tile in data.vehicles)
                if (tile == null || !ValidDirection(tile.direction) || !Finite(tile.x) || !Finite(tile.y)
                    || !Finite(tile.width) || !Finite(tile.length) || tile.width <= 0 || tile.length <= 0) return false;
            return true;
        }

        static bool Overlap(WoolTile a, WoolTile b)
        {
            bool av = a.direction == WoolDirection.Up || a.direction == WoolDirection.Down;
            bool bv = b.direction == WoolDirection.Up || b.direction == WoolDirection.Down;
            float aw = av ? a.width : a.length, ah = av ? a.length : a.width;
            float bw = bv ? b.width : b.length, bh = bv ? b.length : b.width;
            return Math.Abs(a.x - b.x) < (aw + bw) * .5f - .001f && Math.Abs(a.y - b.y) < (ah + bh) * .5f - .001f;
        }

        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        static bool ValidColor(int value) => value >= 0 && value < ColorCount;
        static bool ValidSize(WoolSize value) => value >= WoolSize.Small && value <= WoolSize.Large;
        static bool ValidDirection(WoolDirection value) => value >= WoolDirection.Up && value <= WoolDirection.Left;
    }
}
