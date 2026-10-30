using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WoolGame
{
    public sealed class WoolSpecialOffer : MonoBehaviour
    {
        [Header("Editable offer copy")]
        [SerializeField,Min(0)] int bonusCoins=2000;
        [SerializeField] string displayPrice="Rs450.00";
        [SerializeField,Min(0)] int unlockCount=1,removeCount=1,sortCount=1,amplifyCount=1;
        [SerializeField,Range(0,1f)] float backdropAlpha=.8f;
        [Header("Authored UI")]
        public RectTransform card;
        public Image dimmer;
        public TMP_Text rewardLabel,priceLabel;
        public TMP_Text unlockLabel,removeLabel,sortLabel,amplifyLabel;
        public Button closeButton,purchaseButton;
        Action purchase;
        RectTransform root,canvasRect;
        Vector2 lastSize,lastCanvasSize;
        Rect lastSafe;float refreshAt;
        public int BonusCoins {get=>bonusCoins;set{bonusCoins=Mathf.Max(0,value);RefreshCopy();}}
        public string DisplayPrice {get=>displayPrice;set{displayPrice=value??"";RefreshCopy();}}
        public bool Visible=>gameObject.activeSelf;
        void Awake(){BindButtons();}
        void OnEnable(){RefreshCopy();Fit();}
        void OnValidate(){RefreshCopy();}
        public void Bind(Action onPurchase){purchase=onPurchase;BindButtons();}
        void BindButtons()
        {
            if(closeButton){closeButton.onClick.RemoveAllListeners();closeButton.onClick.AddListener(Hide);}
            if(purchaseButton){purchaseButton.onClick.RemoveAllListeners();purchaseButton.onClick.AddListener(()=>purchase?.Invoke());}
        }
        public void Show(){gameObject.SetActive(true);transform.SetAsLastSibling();RefreshCopy();Fit();}
        public void Hide(){gameObject.SetActive(false);}
        void RefreshCopy()
        {
            var shared=WoolIapCatalog.Find(WoolIapCatalog.Intro);
            if(shared!=null){bonusCoins=shared.coins;displayPrice=shared.displayPrice;unlockCount=removeCount=sortCount=amplifyCount=shared.powers;}
            // Cream reward cells need dark lettering even after a global font refresh.
            foreach(var label in new[]{rewardLabel,unlockLabel,removeLabel,sortLabel,amplifyLabel})
                if(label){label.color=new Color(.28f,.12f,.035f,1);label.fontStyle=FontStyles.Normal;}
            if(card)
            {
                var heading=card.Find("Special Offer title");
                if(heading)
                {
                    var label=heading.GetComponent<TMP_Text>();
                    if(label){label.text="Special <color=#FFE56E>Offer</color>";label.color=Color.white;label.fontSize=98;label.fontSizeMax=98;label.fontSizeMin=72;label.enableAutoSizing=true;label.characterSpacing=0;}
                }
            }
            if(rewardLabel)rewardLabel.text=WoolCoinText.Format(bonusCoins);
            if(unlockLabel)unlockLabel.text="x"+Mathf.Max(0,unlockCount);
            if(removeLabel)removeLabel.text="x"+Mathf.Max(0,removeCount);
            if(sortLabel)sortLabel.text="x"+Mathf.Max(0,sortCount);
            if(amplifyLabel)amplifyLabel.text="x"+Mathf.Max(0,amplifyCount);
            bool owned=Application.isPlaying&&WoolProgress.OwnsProduct(WoolIapCatalog.Intro);if(purchaseButton)purchaseButton.interactable=!owned;if(priceLabel)priceLabel.text=owned?"OWNED":WoolPurchases.Current?WoolPurchases.Current.Price(WoolIapCatalog.Intro)??displayPrice:displayPrice;
            if(dimmer)dimmer.color=new Color(0,0,0,backdropAlpha);
        }
        public void Fit()
        {
            if(!card||!dimmer)return;
            if(!root)root=(RectTransform)transform;
            if(!canvasRect){var canvas=GetComponentInParent<Canvas>();if(canvas)canvasRect=(RectTransform)canvas.transform;}
            var size=root.rect.size;if(size.x<1||size.y<1)return;
            float scale=Mathf.Min(size.x*.9f/1035f,size.y*.9f/1536f);
            card.anchorMin=card.anchorMax=new Vector2(.5f,.5f);card.anchoredPosition=Vector2.zero;
            card.sizeDelta=new Vector2(1035,1536);card.localScale=Vector3.one*scale;
            if(canvasRect)
            {
                dimmer.rectTransform.anchorMin=dimmer.rectTransform.anchorMax=new Vector2(.5f,.5f);
                dimmer.rectTransform.position=canvasRect.TransformPoint(canvasRect.rect.center);
                dimmer.rectTransform.sizeDelta=canvasRect.rect.size;dimmer.rectTransform.localScale=Vector3.one;
                lastCanvasSize=canvasRect.rect.size;
            }
            lastSize=size;lastSafe=Screen.safeArea;
        }
        void LateUpdate()
        {
            if(Time.unscaledTime>=refreshAt){refreshAt=Time.unscaledTime+1;RefreshCopy();}
            if(root&&(root.rect.size!=lastSize||lastSafe!=Screen.safeArea||(canvasRect&&canvasRect.rect.size!=lastCanvasSize)))Fit();
            if(purchaseButton)
            {
                float pulse=1f+.017f*Mathf.Sin(Time.unscaledTime*2.6f);
                purchaseButton.transform.localScale=Vector3.one*pulse;
                if(priceLabel)priceLabel.transform.localScale=Vector3.one/pulse;
            }
        }
    }
}


