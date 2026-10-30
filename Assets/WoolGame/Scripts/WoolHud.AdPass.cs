using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIEffects;

namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] WoolAdPassOffer adPassOffer;
        void BindAdPassOffer()
        {
            if(!adPassOffer)return;
            adPassOffer.Bind(()=>BuyShopProduct(WoolIapCatalog.RemoveAds));
        }
        public void ShowAdPassOffer()
        {
            if(!adPassOffer)return;
            CloseEngagement();Hide();if(profilePanel)profilePanel.SetActive(false);
            adPassOffer.Show();
        }
#if UNITY_EDITOR
        public string AuthorAdPassOffer(Sprite panelSprite,Sprite greenButton)
        {
            if(Application.isPlaying)throw new System.InvalidOperationException("Author the offer in Edit Mode.");
            var existing=transform.Find("Ad pass offer") as RectTransform;
            var overlay=existing?existing:Rect("Ad pass offer",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            adPassOffer=overlay.GetComponent<WoolAdPassOffer>();if(!adPassOffer)adPassOffer=overlay.gameObject.AddComponent<WoolAdPassOffer>();
            adPassOffer.dimmer=FantasyImage("Ad pass dimmer",overlay,null,0,0,1034,1521,false).GetComponent<Image>();
            adPassOffer.dimmer.color=new Color(0,0,0,.32f);adPassOffer.dimmer.raycastTarget=true;
            var card=FantasyImage("Remove Ads card",overlay,panelSprite,0,0,1034,1521,false);
            adPassOffer.card=card;
            var close=card.Find("Close offer");
            adPassOffer.closeButton=close?close.GetComponent<Button>():ButtonAt("Close offer",card,"",new Vector2(150,150),Vector2.zero,null);
            var cr=(RectTransform)adPassOffer.closeButton.transform;FantasyPlace(cr,860,88,150,150);
            var ci=adPassOffer.closeButton.GetComponent<Image>();ci.sprite=skin[3];ci.color=Color.white;ci.type=Image.Type.Simple;ci.preserveAspect=true;
            foreach(var text in adPassOffer.closeButton.GetComponentsInChildren<TMP_Text>(true))text.gameObject.SetActive(false);
            var buy=card.Find("Purchase offer");
            adPassOffer.purchaseButton=buy?buy.GetComponent<Button>():ButtonAt("Purchase offer",card,"",new Vector2(630,205),Vector2.zero,null);
            FantasyPlace((RectTransform)adPassOffer.purchaseButton.transform,202,1272,630,205);
            var bi=adPassOffer.purchaseButton.GetComponent<Image>();bi.sprite=greenButton;bi.color=Color.white;bi.type=Image.Type.Simple;
            foreach(var text in adPassOffer.purchaseButton.GetComponentsInChildren<TMP_Text>(true))text.gameObject.SetActive(false);
            adPassOffer.priceLabel=FantasyText("Editable price",adPassOffer.purchaseButton.transform,24,32,582,136,86,Color.white);
            var levelMaterial=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/FantasyHome/LevelText.mat");
            if(levelMaterial)adPassOffer.priceLabel.fontSharedMaterial=levelMaterial;
            adPassOffer.rewardLabel=FantasyText("Editable bonus coins",card,205,1145,254,92,75,Color.white);
            if(levelMaterial)adPassOffer.rewardLabel.fontSharedMaterial=levelMaterial;
            var shiny=adPassOffer.purchaseButton.GetComponent<UIShiny>();if(!shiny)shiny=adPassOffer.purchaseButton.gameObject.AddComponent<UIShiny>();
            shiny.width=.10f;shiny.softness=.55f;shiny.brightness=.8f;shiny.gloss=.6f;shiny.rotation=25;
            shiny.effectPlayer.play=true;shiny.effectPlayer.loop=true;shiny.effectPlayer.duration=.65f;shiny.effectPlayer.loopDelay=1.35f;
            shiny.effectPlayer.initialPlayDelay=1.35f;shiny.effectPlayer.updateMode=AnimatorUpdateMode.UnscaledTime;
            BindAdPassOffer();ConfigureInputTargets();adPassOffer.Fit();adPassOffer.Hide();
            UnityEditor.EditorUtility.SetDirty(adPassOffer);UnityEditor.EditorUtility.SetDirty(this);
            return "Ad Pass offer authored: editable bonus/price, 32% black dimmer, independent close/purchase buttons and two-second UIShiny.";
        }
#endif
    }
}

