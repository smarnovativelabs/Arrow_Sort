using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIEffects;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] WoolLuckySpin luckySpin;
        void BindLuckySpin(){if(luckySpin)luckySpin.Bind(RefreshWalletLabels);}
#if UNITY_EDITOR
        public string AuthorLuckySpin(Sprite coins,Sprite unlock,Sprite green,Sprite pointer,Sprite originalHub,Sprite noAds,Sprite originalInfo)
        {
            var old=transform.Find("Lucky Spin popup") as RectTransform;
            var overlay=old?old:Rect("Lucky Spin popup",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            luckySpin=overlay.GetComponent<WoolLuckySpin>();if(!luckySpin)luckySpin=overlay.gameObject.AddComponent<WoolLuckySpin>();
            // Retain the overlay, hide the previous composition for recoverability.
            foreach(Transform child in overlay)child.gameObject.SetActive(false);
            luckySpin.dimmer=FantasyImage("Black background",overlay,null,0,0,1000,1740,false).GetComponent<Image>();
            luckySpin.dimmer.color=new Color(0,0,0,.78f);luckySpin.dimmer.raycastTarget=true;
            var card=overlay.Find("Clean Lucky Spin layout") as RectTransform;
            if(!card)card=new GameObject("Clean Lucky Spin layout",typeof(RectTransform)).GetComponent<RectTransform>();
            card.SetParent(overlay,false);card.gameObject.SetActive(true);card.sizeDelta=new Vector2(1000,1740);luckySpin.card=card;
            RectTransform Surface(string name,Transform parent,WoolSpinSurface.Kind kind,float x,float y,float w,float h)
            {
                var rt=parent.Find(name) as RectTransform;
                if(!rt)rt=new GameObject(name,typeof(RectTransform),typeof(WoolSpinSurface)).GetComponent<RectTransform>();
                rt.SetParent(parent,false);rt.gameObject.SetActive(true);FantasyPlace(rt,x,y,w,h);
                var g=rt.GetComponent<WoolSpinSurface>();g.kind=kind;g.raycastTarget=false;g.SetVerticesDirty();return rt;
            }
            var brown=WoolArt.Hex("68320C");
            var outline=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/SpecialOffer/OfferText.mat");
            Surface("Orange header",card,WoolSpinSurface.Kind.Banner,120,20,760,148);
            var title=FantasyText("Title",card,150,42,700,96,69,Color.white);title.text="Lucky Spin";if(outline)title.fontSharedMaterial=outline;
            luckySpin.close=FantasyButton("Close",card,skin[3],885,45,90,90);
            luckySpin.info=FantasyButton("Information",card,originalInfo,25,45,90,90);luckySpin.info.GetComponent<Image>().useSpriteMesh=true;
            Surface("Golden wheel rim",card,WoolSpinSurface.Kind.GoldRing,60,210,880,880);
            luckySpin.wheel=Surface("Rotating wheel",card,WoolSpinSurface.Kind.PrizeDisk,95,245,810,810);
            for(int i=0;i<8;i++)
            {
                float a=i*45*Mathf.Deg2Rad;
                float x=405+Mathf.Sin(a)*250,y=405-Mathf.Cos(a)*250;
                if(i==0)FantasyImage("No Ads reward",luckySpin.wheel,noAds,x-64,y-54,128,128,false).GetComponent<Image>().useSpriteMesh=true;
                else if(i==4)FantasyImage("Unlock Slot reward",luckySpin.wheel,unlock,x-77,y-65,154,145,true).GetComponent<Image>().useSpriteMesh=true;
                else if(i!=5)FantasyImage("Duck coins "+i,luckySpin.wheel,coins,x-95,y-57,190,127,true);
                float r=i==5?250:337;float tx=405+Mathf.Sin(a)*r,ty=405-Mathf.Cos(a)*r;
                var label=FantasyText("Prize label "+i,luckySpin.wheel,tx-76,ty-34,152,68,i==0||i==4||i==5?31:45,i==0?Color.white:brown);
                label.text=i==0?"NO ADS\n24H":i==4?"UNLOCK x1":i==5?"TRY\nAGAIN":WoolSpinState.Coins[i].ToString();
                if(i==0&&outline)label.fontSharedMaterial=outline;
            }
            FantasyImage("Original flower hub",luckySpin.wheel,originalHub,330,330,150,150,false).GetComponent<Image>().useSpriteMesh=true;
            FantasyImage("Fixed pointer",card,pointer,455,185,90,110,false).GetComponent<Image>().useSpriteMesh=true;
            luckySpin.lamps=new WoolSpinLamp[12];
            for(int i=0;i<12;i++)
            {
                float a=(15+i*30)*Mathf.Deg2Rad;float x=500+Mathf.Sin(a)*424,y=650-Mathf.Cos(a)*424;
                var lamp=card.Find("Light "+i) as RectTransform;
                if(!lamp)lamp=new GameObject("Light "+i,typeof(RectTransform),typeof(WoolSpinLamp)).GetComponent<RectTransform>();
                lamp.SetParent(card,false);FantasyPlace(lamp,x-18,y-18,36,36);luckySpin.lamps[i]=lamp.GetComponent<WoolSpinLamp>();luckySpin.lamps[i].raycastTarget=false;luckySpin.lamps[i].SetLit(i%2==0);
            }
            Surface("Countdown card",card,WoolSpinSurface.Kind.CreamCard,85,1120,830,130);
            var timerTitle=FantasyText("Timer title",card,120,1135,390,46,30,brown);timerTitle.text="Next daily spin";
            var reset=FantasyText("UTC reset",card,120,1180,390,42,25,brown);reset.text="Resets at local midnight";
            luckySpin.timerLabel=FantasyText("Verified countdown",card,505,1147,365,72,52,brown);
            Surface("Streak card",card,WoolSpinSurface.Kind.CreamCard,85,1270,830,126);
            luckySpin.streakLabel=FantasyText("Daily streak",card,120,1283,760,46,36,brown);
            luckySpin.guaranteeLabel=FantasyText("30 day guarantee",card,120,1332,760,43,29,WoolArt.Hex("8A3AB2"));
            luckySpin.spin=FantasyButton("Spin",card,green,200,1420,600,170);luckySpin.spin.transition=Selectable.Transition.None;
            luckySpin.buttonLabel=FantasyText("Available spins",luckySpin.spin.transform,28,25,544,116,76,Color.white);
            var greenMaterial=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/FantasyHome/LevelText.mat");if(greenMaterial)luckySpin.buttonLabel.fontSharedMaterial=greenMaterial;
            var shiny=luckySpin.spin.GetComponent<UIShiny>();if(!shiny)shiny=luckySpin.spin.gameObject.AddComponent<UIShiny>();
            shiny.width=.10f;shiny.softness=.55f;shiny.brightness=.8f;shiny.gloss=.6f;shiny.rotation=25;
            shiny.effectPlayer.play=shiny.effectPlayer.loop=true;shiny.effectPlayer.duration=.65f;shiny.effectPlayer.loopDelay=1.35f;shiny.effectPlayer.initialPlayDelay=1.35f;shiny.effectPlayer.updateMode=AnimatorUpdateMode.UnscaledTime;
            luckySpin.progressLabel=FantasyText("Level progress",card,90,1610,820,47,30,Color.white);
            luckySpin.resultLabel=FantasyText("Spin result",card,90,1668,820,60,30,Color.white);
            BindLuckySpin();ConfigureInputTargets();luckySpin.Fit();overlay.gameObject.SetActive(false);
            UnityEditor.EditorUtility.SetDirty(luckySpin);UnityEditor.EditorUtility.SetDirty(this);
            return "Clean cream-and-gold Lucky Spin layout saved: 12 alternating lights, independent prize icons, timer/streak cards and large green button.";
        }
#endif
    }
}
