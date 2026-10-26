using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    // Shared by Home, Shop and Map. Only the selected-state artwork changes.
    public sealed class WoolMenuNavigation:MonoBehaviour
    {
        public WoolHud owner;
        public Image surface;
        public Button[] tabs;
        public Sprite[] states;
        public RectTransform shopViewport;
        [SerializeField] int selected=1;
        Vector2 lastParent;
        public string SelectedTab=>selected==0?"Shop":selected==2?"Map":"Home";
        public float Height=>((RectTransform)transform).rect.height;
        void Bind()
        {
            if(tabs==null)return;
            for(int i=0;i<tabs.Length;i++)
            {
                int index=i;tabs[i].onClick.RemoveAllListeners();
                tabs[i].onClick.AddListener(()=>
                {
                    if(index==selected)return;
                    if(index==0)owner.SkywayShop();else if(index==1)owner.SkywayHome();else owner.ShowMap();
                });
            }
        }
        public void Select(string tab)
        {
            selected=tab=="Shop"?0:tab=="Map"?2:1;surface.sprite=states[selected];
            gameObject.SetActive(true);transform.SetAsLastSibling();Bind();lastParent=Vector2.zero;Fit();
        }
        public void SetVisible(bool visible){gameObject.SetActive(visible);if(visible)Fit();}
        void LateUpdate()=>Fit();
        public void Fit()
        {
            var rt=(RectTransform)transform;var parent=rt.parent as RectTransform;
            if(!parent||!surface||!surface.sprite||parent.rect.width<=0)return;
            if(lastParent==parent.rect.size)return;lastParent=parent.rect.size;
            float w=Mathf.Min(parent.rect.width-24,1280),h=w*surface.sprite.rect.height/surface.sprite.rect.width;
            rt.anchorMin=rt.anchorMax=new Vector2(.5f,0);rt.pivot=new Vector2(.5f,0);
            rt.localScale=Vector3.one;rt.sizeDelta=new Vector2(w,h);rt.anchoredPosition=new Vector2(0,12);
            if(shopViewport){var bottom=shopViewport.offsetMin;bottom.y=h+34;shopViewport.offsetMin=bottom;}
        }
#if UNITY_EDITOR
        public void Author(WoolHud hud,Sprite[] sprites,RectTransform shopView)
        {
            owner=hud;states=sprites;shopViewport=shopView;
            var rt=new GameObject("Navigation artwork",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)).GetComponent<RectTransform>();
            rt.SetParent(transform,false);rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;
            surface=rt.GetComponent<Image>();surface.sprite=states[1];surface.preserveAspect=true;surface.raycastTarget=false;
            tabs=new Button[3];string[] names={"Shop","Home","Map"};
            for(int i=0;i<3;i++)
            {
                var b=new GameObject("Navigate "+names[i],typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(Button));
                var br=(RectTransform)b.transform;br.SetParent(transform,false);br.anchorMin=new Vector2(i/3f,0);br.anchorMax=new Vector2((i+1)/3f,1);br.offsetMin=br.offsetMax=Vector2.zero;
                var image=b.GetComponent<Image>();image.color=Color.clear;image.raycastTarget=true;
                tabs[i]=b.GetComponent<Button>();tabs[i].targetGraphic=image;tabs[i].transition=Selectable.Transition.None;
            }
            selected=1;Fit();UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
