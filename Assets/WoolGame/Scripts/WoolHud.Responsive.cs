using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] Texture2D landscapeMap;
        public void ConfigureInputTargets()
        {
            foreach(var text in GetComponentsInChildren<TMPro.TMP_Text>(true))text.raycastTarget=false;
            foreach(var graphic in GetComponentsInChildren<Graphic>(true))
            {
                if(graphic.GetComponent<Button>())continue;
                bool blocker=graphic.gameObject==modal||graphic.gameObject==menu||graphic.gameObject==launch||graphic.gameObject==profilePanel||graphic.gameObject==dailyPanel||graphic.gameObject==spinPanel;
                bool scroll=graphic.GetComponent<RectMask2D>()||graphic.GetComponent<Mask>()||graphic.GetComponent<ScrollRect>();
                graphic.raycastTarget=blocker||scroll||graphic.GetComponent<TMPro.TMP_InputField>()||(adPassOffer&&graphic==adPassOffer.dimmer)||(specialOffer&&graphic==specialOffer.dimmer)||(luckySpin&&graphic==luckySpin.dimmer)||(profileView&&graphic==profileView.dimmer);
            }
            foreach(var button in GetComponentsInChildren<Button>(true))
            {
                var graphic=button.GetComponent<Image>();if(!graphic)continue;
                if(!graphic.enabled){graphic.enabled=true;graphic.color=Color.clear;}
                graphic.raycastTarget=true;button.targetGraphic=graphic;
            }
            var events=FindFirstObjectByType<EventSystem>();
            if(events){var input=events.GetComponent<InputSystemUIInputModule>();if(input&&(!input.point||!input.leftClick))input.AssignDefaultActions();}
        }
        void ResponsiveMenus(float width,float height)
        {
            float menuScale=Mathf.Clamp(Mathf.Min(width/760f,height/1400f),.55f,1.6f);
            if(homePage)
            {
                foreach(string name in new[]{"TASKS","Lv.16","OFFERS","NO ADS","Play level","Game title","Rescue subtitle","Duck family art"})
                {var t=homePage.Find(name);if(t)t.localScale=Vector3.one*menuScale;}
                string[] badges={"TASKS","Lv.16","OFFERS","NO ADS"};
                for(int i=0;i<4;i++){var t=homePage.Find(badges[i]) as RectTransform;if(t)t.anchoredPosition=new Vector2((-240+160*i)*menuScale,0);}
            }
            if(shopPage)
            {
                var viewport=shopPage.Find("Shop viewport") as RectTransform;
                if(viewport){var content=viewport.Find("Shop products") as RectTransform;if(content)content.localScale=Vector3.one*Mathf.Min((width-48)/650f,1.6f);}
            }
            FitMapViewport();
            FitAuthoredPanels();
        }
        void FitMapViewport()
        {
            if(landscapeMap&&mapContent)
            {
                float w=mapScroll.viewport.rect.width;float h=mapScroll.viewport.rect.height;
                if(w<=0||h<=0)return;
                mapContent.sizeDelta=new Vector2(w,h);mapContent.localScale=Vector3.one;
                var art=mapContent.Find("Landscape map") as RectTransform;if(art){art.anchorMin=Vector2.zero;art.anchorMax=Vector2.one;art.offsetMin=art.offsetMax=Vector2.zero;}
                var positions=MapStops();
                for(int i=0;i<mapNodes.Length;i++)
                {var rt=mapNodes[i].GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=positions[i];rt.anchoredPosition=Vector2.zero;rt.sizeDelta=new Vector2(w*.09f,w*.07f);mapNodes[i].GetComponentInChildren<TMPro.TMP_Text>().fontSize=Mathf.Clamp(w*.027f,20,34);}
            }
        }
        static Vector2[] MapStops()=>new[]{new Vector2(.595f,.033f),new Vector2(.41f,.09f),new Vector2(.395f,.15f),new Vector2(.54f,.223f),new Vector2(.63f,.29f),new Vector2(.516f,.354f),new Vector2(.39f,.405f),new Vector2(.485f,.476f),new Vector2(.566f,.56f),new Vector2(.523f,.632f),new Vector2(.632f,.719f),new Vector2(.772f,.761f),new Vector2(.832f,.819f),new Vector2(.735f,.875f),new Vector2(.582f,.932f)};
#if UNITY_EDITOR
        public void AuthorResponsiveMap(Texture2D texture)
        {
            landscapeMap=texture;
            foreach(Transform child in mapContent)child.gameObject.SetActive(false);
            System.Array.Resize(ref mapNodes,15);System.Array.Resize(ref mapStates,15);System.Array.Resize(ref mapDifficulties,15);
            var existing=mapContent.Find("Landscape map");
            var art=existing?existing.GetComponent<RawImage>():new GameObject("Landscape map",typeof(RectTransform),typeof(RawImage)).GetComponent<RawImage>();
            art.transform.SetParent(mapContent,false);art.texture=texture;art.raycastTarget=false;art.transform.SetAsFirstSibling();art.gameObject.SetActive(true);
            foreach(var node in mapNodes){node.gameObject.SetActive(true);foreach(Transform child in node.transform)if(child.GetComponent<TMPro.TMP_Text>()&&child!=node.GetComponentInChildren<TMPro.TMP_Text>().transform)child.gameObject.SetActive(false);}
            for(int i=0;i<mapNodes.Length;i++){mapStates[i].gameObject.SetActive(false);mapDifficulties[i].gameObject.SetActive(false);}
            mapContent.anchorMin=mapContent.anchorMax=new Vector2(.5f,1);mapContent.pivot=new Vector2(.5f,1);
            mapScroll.movementType=ScrollRect.MovementType.Clamped;mapScroll.horizontal=false;mapScroll.vertical=true;
            ConfigureInputTargets();portraitWidth=0;ApplyPortraitFrame(Screen.width,Screen.height,Screen.safeArea);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
