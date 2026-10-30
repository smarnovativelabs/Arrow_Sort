using UnityEngine;
namespace WoolGame
{
    public static class WoolArt
    {
        public static readonly Color[] Palette = {
            Hex("E53935"), Hex("00D5E8"), Hex("FFD600"), Hex("7B28C9"), Hex("159447"), Hex("FF8C00"), Hex("FF76B5"), Hex("2454E6"), Hex("86451F"), Hex("30343B")
        };
        public static readonly string[] ColorNames = { "Red", "Cyan", "Yellow", "Purple", "Green", "Orange", "Pink", "Blue", "Brown", "Charcoal" };
        public static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#"+hex,out var color); return color; }
        public static Vector2 BlockSize(WoolSize size,WoolDirection direction)
        {
            float length=size==WoolSize.Small?.72f:size==WoolSize.Medium?1.02f:1.32f;
            return direction==WoolDirection.Up||direction==WoolDirection.Down?new Vector2(.72f,length):new Vector2(length,.72f);
        }
    }
}
