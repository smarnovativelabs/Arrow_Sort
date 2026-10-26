using UnityEngine;
namespace WoolGame
{
    public static class WoolArt
    {
        public static readonly Color[] Palette = {
            Hex("B51224"), Hex("007C9E"), Hex("B88808"), Hex("7621AE"), Hex("007642"), Hex("C44D08"), Hex("A70E70"), Hex("1944AF"), Hex("50800A"), Hex("BC3C52")
        };
        public static readonly string[] ColorNames = { "Red", "Sky", "Honey", "Purple", "Mint", "Peach", "Pink", "Blue", "Lime", "Coral" };
        public static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#"+hex,out var color); return color; }
        public static Vector2 BlockSize(WoolSize size,WoolDirection direction)
        {
            float length=size==WoolSize.Small?.72f:size==WoolSize.Medium?1.02f:1.32f;
            return direction==WoolDirection.Up||direction==WoolDirection.Down?new Vector2(.72f,length):new Vector2(length,.72f);
        }
    }
}
