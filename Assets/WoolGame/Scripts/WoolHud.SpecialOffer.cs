using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIEffects;

namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] WoolSpecialOffer specialOffer;
        void BindSpecialOffer()
        {
            if(!specialOffer)return;
            specialOffer.Bind(()=>Show("STORE UNAVAILABLE","Purchases are currently unavailable.\nNo payment has been taken.","BACK",Hide));
        }
        public void ShowSpecialOffer()
        {
            if(!specialOffer)return;
            CloseEngagement();Hide();if(profilePanel)profilePanel.SetActive(false);
            specialOffer.Show();
        }
#if UNITY_EDITOR
        public string AuthorSpecialOffer(Sprite panelSprite,Sprite greenButton)
        {
            if(Application.isPlaying)throw new System.InvalidOperationException("Author the offer in Edit Mode.");
            var existing=transform.Find("Special offer") as RectTransform;
            var overlay=existing?existing:Rect("Special offer",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            specialOffer=overlay.GetComponent<WoolSpecialOffer>();if(!specialOffer)specialOffer=overlay.gameObject.AddComponent<WoolSpecialOffer>();
            specialOffer.dimmer=FantasyImage("Special offer dimmer",overlay,null,0,0,1035,1536,false).GetComponent<Image>();
            specialOffer.dimmer.color=new Color(0,0,0,.8f);specialOffer.dimmer.raycastTarget=true;
            var card=FantasyImage("Special Offer card",overlay,panelSprite,0,0,1035,1536,false);
            specialOffer.card=card;
            var close=card.Find("Close offer");
            specialOffer.closeButton=close?close.GetComponent<Button>():ButtonAt("Close offer",card,"",new Vector2(150,150),Vector2.zero,null);
            var cr=(RectTransform)specialOffer.closeButton.transform;FantasyPlace(cr,860,90,150,150);
            var ci=specialOffer.closeButton.GetComponent<Image>();ci.sprite=skin[3];ci.color=Color.white;ci.type=Image.Type.Simple;ci.preserveAspect=true;
            foreach(var text in specialOffer.closeButton.GetComponentsInChildren<TMP_Text>(true))text.gameObject.SetActive(false);
            var buy=card.Find("Purchase offer");
            specialOffer.purchaseButton=buy?buy.GetComponent<Button>():ButtonAt("Purchase offer",card,"",new Vector2(630,205),Vector2.zero,null);
            FantasyPlace((RectTransform)specialOffer.purchaseButton.transform,202,1290,630,205);
            var bi=specialOffer.purchaseButton.GetComponent<Image>();bi.sprite=greenButton;bi.color=Color.white;bi.type=Image.Type.Simple;
            foreach(var text in specialOffer.purchaseButton.GetComponentsInChildren<TMP_Text>(true))text.gameObject.SetActive(false);
            specialOffer.priceLabel=FantasyText("Editable price",specialOffer.purchaseButton.transform,24,32,582,136,86,Color.white);
            var levelMaterial=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/WoolGame/Designer/UI/FantasyHome/LevelText.mat");
            if(levelMaterial)specialOffer.priceLabel.fontSharedMaterial=levelMaterial;
            specialOffer.rewardLabel=FantasyText("Editable bonus coins",card,150,1140,270,82,80,Color.white);
            var title=FantasyText("Special Offer title",card,210,42,620,116,96,Color.white);title.text="Special Offer";
            specialOffer.unlockLabel=FantasyText("Editable unlock count",card,495,1020,145,62,54,Color.white);
            specialOffer.removeLabel=FantasyText("Editable remove count",card,705,1020,145,62,54,Color.white);
            specialOffer.sortLabel=FantasyText("Editable sort count",card,495,1200,145,62,54,Color.white);
            specialOffer.amplifyLabel=FantasyText("Editable amplify count",card,705,1200,145,62,54,Color.white);
            const string textMaterialPath="Assets/WoolGame/Designer/UI/SpecialOffer/OfferText.mat";
            var offerMaterial=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(textMaterialPath);
            if(!offerMaterial)
            {
                offerMaterial=new Material(title.font.material);offerMaterial.name="Special Offer outlined text";
                offerMaterial.SetColor("_OutlineColor",new Color(.30f,.08f,.015f,1));
                offerMaterial.SetFloat("_OutlineWidth",.20f);
                UnityEditor.AssetDatabase.CreateAsset(offerMaterial,textMaterialPath);
            }
            offerMaterial.EnableKeyword("OUTLINE_ON");
            UnityEditor.EditorUtility.SetDirty(offerMaterial);
            foreach(var label in new[]{title,specialOffer.rewardLabel,specialOffer.unlockLabel,specialOffer.removeLabel,specialOffer.sortLabel,specialOffer.amplifyLabel})
                label.fontSharedMaterial=offerMaterial;
            var shiny=specialOffer.purchaseButton.GetComponent<UIShiny>();if(!shiny)shiny=specialOffer.purchaseButton.gameObject.AddComponent<UIShiny>();
            shiny.width=.10f;shiny.softness=.55f;shiny.brightness=.8f;shiny.gloss=.6f;shiny.rotation=25;
            shiny.effectPlayer.play=true;shiny.effectPlayer.loop=true;shiny.effectPlayer.duration=.65f;shiny.effectPlayer.loopDelay=1.35f;
            shiny.effectPlayer.initialPlayDelay=1.35f;shiny.effectPlayer.updateMode=AnimatorUpdateMode.UnscaledTime;
            BindSpecialOffer();ConfigureInputTargets();specialOffer.Fit();specialOffer.Hide();
            UnityEditor.EditorUtility.SetDirty(specialOffer);UnityEditor.EditorUtility.SetDirty(this);
            return "Special Offer authored: editable coins, four ability counts and price; 80% black dimmer, UIShiny and gentle price pulse.";
        }
#endif
    }
}


