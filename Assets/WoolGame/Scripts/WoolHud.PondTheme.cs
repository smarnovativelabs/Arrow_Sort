#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        public void AuthorPondTheme()
        {
            var cream=PondSurface("Cream","FFFDF1","F3E9D2");
            var mint=PondSurface("Mint","70EBC2","168C83");
            var blue=PondSurface("Aqua","70D5F0","2686AF");
            var coral=PondSurface("Coral","FF9E83","D65362");
            var pale=PondSurface("Pale","F2FFFB","C8EAE2");
            var gold=PondSurface("Honey","FFEAA0","EDB854");
            var old=skin.ToArray();
            skin=new[]{cream,mint,blue,coral,pale,old[5],coral,cream,pale};
            var ink=WoolArt.Hex("164F59");
            foreach(var im in GetComponentsInChildren<Image>(true))
            {
                int i=im.sprite?System.Array.IndexOf(old,im.sprite):-1;
                if(i>=0&&i!=5){im.sprite=skin[i];im.type=Image.Type.Sliced;im.color=Color.white;}
                if(im.name=="Daily card"||im.name=="Spin card"||im.name=="Profile card"||im.name.StartsWith("Task ")&&!im.name.Contains("progress")){im.sprite=cream;im.type=Image.Type.Sliced;im.color=Color.white;}
                if(im.name=="Profile title"||im.name=="Shop awning"||im.name=="Map header"){im.sprite=blue;im.type=Image.Type.Sliced;}
            }
            foreach(var t in GetComponentsInChildren<TMP_Text>(true))
            {
                t.outlineWidth=0;
                if(t.color.a>.8f)t.color=ink;
            }
            foreach(var b in GetComponentsInChildren<Button>(true))
            {
                bool action=b.name=="Primary"||b.name=="Play level"||b.name=="Spin"||b.name=="Claim"||b.name=="Purchase";
                bool cross=b.name=="Close"||b.name.StartsWith("Close ");
                var im=b.GetComponent<Image>();if(im){im.sprite=cross?coral:action?mint:pale;im.color=Color.white;im.type=Image.Type.Sliced;}
                var text=b.GetComponentInChildren<TMP_Text>(true);
                if(text){text.color=action||cross?Color.white:ink;if(cross){text.text="×";text.fontSize=36;}if(action){text.outlineWidth=.12f;text.outlineColor=WoolArt.Hex("146B63");}}
            }
            var picture=homePage.Find("Sailing with friends").GetComponent<RawImage>();
            var pond=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/WoolGame/Designer/UI/PondHome.png");
            if(pond){picture.texture=pond;picture.uvRect=new Rect(0,0,1,1);picture.GetComponent<AspectRatioFitter>().aspectRatio=(float)pond.width/pond.height;}
            var banner=homePage.Find("Game title").GetComponent<RectTransform>();
            banner.GetComponent<Image>().enabled=false;Place(banner,new Vector2(.5f,.80f),Vector2.zero);banner.sizeDelta=new Vector2(670,165);
            var title=banner.GetComponentInChildren<TMP_Text>();title.text="WOOL\nCRUSH";title.fontSize=78;title.lineSpacing=-24;title.rectTransform.sizeDelta=new Vector2(650,190);title.color=WoolArt.Hex("FFF5D2");title.outlineColor=ink;title.outlineWidth=.22f;
            var subtitle=homePage.Find("Rescue subtitle").GetComponent<TMP_Text>();subtitle.text="LITTLE DUCKS. BIG ADVENTURE.";subtitle.fontSize=22;subtitle.color=ink;Place(subtitle.rectTransform,new Vector2(.5f,.705f),Vector2.zero);
            var hero=homePage.Find("Duck family art").GetComponent<RectTransform>();Place(hero,new Vector2(.5f,.53f),Vector2.zero);hero.sizeDelta=new Vector2(830,595);
            var idle=hero.GetComponent<WoolHeroMotion>();if(!idle)hero.gameObject.AddComponent<WoolHeroMotion>();
            string[] names={"TASKS","Lv.16","OFFERS","NO ADS"};
            string[] captions={"DAILY\nTASKS","LUCKY\nSPIN","RESCUE\nSHOP","AD\nPASS"};
            for(int i=0;i<names.Length;i++)
            {
                var b=homePage.Find(names[i]);if(!b)continue;
                var rt=b.GetComponent<RectTransform>();Place(rt,new Vector2(.5f,.31f),new Vector2(-240+160*i,0));rt.sizeDelta=new Vector2(140,120);
                b.GetComponent<Image>().sprite=i==1?gold:cream;
                var text=b.GetComponentInChildren<TMP_Text>();text.text=captions[i];text.fontSize=20;text.rectTransform.sizeDelta=new Vector2(130,60);text.rectTransform.anchoredPosition=new Vector2(0,-20);
                foreach(var im in b.GetComponentsInChildren<Image>())if(im.transform!=b){im.rectTransform.sizeDelta=new Vector2(48,48);im.rectTransform.anchoredPosition=new Vector2(0,30);}
            }
            var play=homePage.Find("Play level").GetComponent<RectTransform>();Place(play,new Vector2(.5f,.17f),Vector2.zero);play.sizeDelta=new Vector2(440,106);homeLevel.fontSize=43;
            var nav=menu.transform.Find("Bottom navigation");nav.GetComponent<Image>().sprite=cream;
            foreach(var text in nav.GetComponentsInChildren<TMP_Text>(true))text.color=ink;
            foreach(var page in new[]{shopPage,cardsPage,mapPage})if(page&&page.GetComponent<Image>())page.GetComponent<Image>().color=WoolArt.Hex("D9EFEB");
            foreach(var text in shopPage.Find("Shop awning").GetComponentsInChildren<TMP_Text>())text.color=Color.white;
            var content=shopPage.Find("Shop viewport/Shop products");
            foreach(Transform card in content)
            {
                card.GetComponent<Image>().sprite=cream;
                var label=card.Find("Product ribbon/Name").GetComponent<TMP_Text>();label.color=ink;
            }
            if(launch)launch.GetComponent<Image>().color=WoolArt.Hex("238E93");
            modal.GetComponent<Image>().color=new Color(.025f,.12f,.16f,.72f);
            ApplyModalSkin();Tutorial(false);ConfigureInputTargets();EditorUtility.SetDirty(this);
        }
        static Sprite PondSurface(string name,string top,string bottom)
        {
            string path="Assets/WoolGame/Designer/UI/Pond"+name+".png";
            const int n=128;var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);var pixels=new Color[n*n];
            Color a=WoolArt.Hex(top),b=WoolArt.Hex(bottom);
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                var q=new Vector2(Mathf.Abs(x-63.5f)-39,Mathf.Abs(y-63.5f)-39);
                float distance=new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude+Mathf.Min(Mathf.Max(q.x,q.y),0)-23;
                var color=Color.Lerp(b,a,Mathf.Clamp01(y/110f));
                if(distance>-4)color=Color.Lerp(color,Color.white,.68f);
                else if(distance>-8)color=Color.Lerp(color,y>64?Color.white:b,y>64?.28f:.2f);
                color.a=Mathf.Clamp01(-distance);pixels[y*n+x]=color;
            }
            tex.SetPixels(pixels);tex.Apply();System.IO.File.WriteAllBytes(path,tex.EncodeToPNG());DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spriteBorder=new Vector4(36,36,36,36);importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
#endif
