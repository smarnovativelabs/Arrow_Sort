using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace WoolGame
{
    public sealed partial class WoolHud
    {
        [SerializeField] RectTransform mapPage,mapContent;
        [SerializeField] ScrollRect mapScroll;
        [SerializeField] Button[] mapNodes;
        [SerializeField] TMP_Text[] mapStates,mapDifficulties;
        [SerializeField] TMP_Text mapSummary,launchMotivation,homeDifficulty,gameDifficulty;
        [SerializeField] Image launchFill;
        [SerializeField] WoolLevelDefinition[] campaignLevels;
        public event Action<int> LevelSelected;
        int mapStart=1;
        Button previousMap,nextMap;
        bool mapSliding;
        RectTransform departingMap;
        Sprite mapMarkerSprite;
        public void SetLaunchProgress(float value,string message)
        {
            if(!launch)return;launch.SetActive(true);launch.transform.SetAsLastSibling();
            var loadingView=launch.GetComponent<WoolLoadingScreen>();if(loadingView)loadingView.SetProgress(value);
            if(launchFill)launchFill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,480*Mathf.Clamp01(value));
            if(launchMotivation)launchMotivation.text=message;
            launchProgress.text=Mathf.RoundToInt(Mathf.Clamp01(value)*100)+"%";
        }
        public void HideLaunch()=>EndLaunch();
        void BindCampaignUI()
        {
            if(mapNodes!=null)for(int i=0;i<mapNodes.Length;i++)
            {
                int index=i;mapNodes[i].onClick.RemoveAllListeners();mapNodes[i].onClick.AddListener(()=>{int number=mapStart+index;if(WoolProgress.IsUnlocked(number))LevelSelected?.Invoke(number);});
            }
            var nav=menu.transform.Find("Bottom navigation");if(nav)
            {
                BindNav(nav,"Shop",ShowShop);BindNav(nav,"Home",()=>Navigate(2));BindNav(nav,"Map",ShowMap);
            }
        }
        void BindNav(Transform root,string name,Action callback){var t=root.Find(name);if(!t)return;var b=t.GetComponent<Button>();b.onClick.RemoveAllListeners();b.onClick.AddListener(()=>callback());}
        public void ShowMap()
        {
            if(skywayMap){ShowSkywayMap();return;}
            if(!mapPage)return;CloseEngagement();Hide();if(profilePanel)profilePanel.SetActive(false);
            homePage.gameObject.SetActive(false);shopPage.gameObject.SetActive(false);mapPage.gameObject.SetActive(true);mapStart=((WoolProgress.CurrentLevel-1)/mapNodes.Length)*mapNodes.Length+1;EnsureMapPaging();RefreshMap();Canvas.ForceUpdateCanvases();FitMapViewport();
            if(mapScroll&&mapContent)
            {
                float height=Mathf.Max(1,mapContent.rect.height-mapScroll.viewport.rect.height);
                mapScroll.verticalNormalizedPosition=landscapeMap?Mathf.Clamp01((MapStops()[Mathf.Clamp(WoolProgress.CurrentLevel-mapStart,0,mapNodes.Length-1)].y*mapContent.rect.height-mapScroll.viewport.rect.height*.5f)/height):1-Mathf.Clamp01(((WoolProgress.CurrentLevel-mapStart)*166-160)/height);
            }
            SelectTab("Map");
        }
        void RefreshMap()
        {
            if(mapNodes==null)return;
            mapSummary.text="Levels "+mapStart+"–"+(mapStart+mapNodes.Length-1)+"  ·  "+WoolProgress.CompletedCount+" rescued";
            if(previousMap)previousMap.interactable=!mapSliding&&mapStart>1;if(nextMap)nextMap.interactable=!mapSliding&&mapStart+mapNodes.Length<=WoolProgress.TotalLevels;
            for(int i=0;i<mapNodes.Length;i++)
            {
                bool complete=WoolProgress.HasCompleted(mapStart+i),current=mapStart+i==WoolProgress.CurrentLevel,unlocked=WoolProgress.IsUnlocked(mapStart+i);
                var difficulty=LevelDifficulty(mapStart+i);mapNodes[i].GetComponentInChildren<TMP_Text>().text=(mapStart+i).ToString();var image=mapNodes[i].GetComponent<Image>();
                Paint(image,difficulty==WoolDifficulty.Hard?8:difficulty==WoolDifficulty.Medium?2:1);image.type=Image.Type.Sliced;
                mapNodes[i].interactable=unlocked;image.color=unlocked?Color.white:new Color(.63f,.72f,.79f,.85f);
                mapStates[i].text=complete?"Complete":current?"Play":"Locked";mapStates[i].color=unlocked?WoolArt.Hex("704322"):WoolArt.Hex("536A79");
                mapDifficulties[i].text=difficulty.ToString().ToUpperInvariant();
                if(landscapeMap)
                {
                    image.sprite=MapMarker();image.type=Image.Type.Simple;
                    image.color=complete?WoolArt.Hex("39B94D"):current?WoolArt.Hex("FFCA55"):WoolArt.Hex("B7B2A5");
                    var colors=mapNodes[i].colors;colors.disabledColor=Color.white;mapNodes[i].colors=colors;
                    var label=mapNodes[i].GetComponentInChildren<TMP_Text>();label.text=unlocked?(mapStart+i).ToString():"";
                    label.color=complete?Color.white:WoolArt.Hex("493923");label.outlineWidth=0;
                    var lockIcon=mapNodes[i].transform.Find("Level lock");
                    if(!lockIcon)lockIcon=CreateMapLock(mapNodes[i].transform);
                    lockIcon.gameObject.SetActive(!unlocked);
                    mapStates[i].gameObject.SetActive(false);mapDifficulties[i].gameObject.SetActive(false);
                }
            }
        }
        WoolDifficulty LevelDifficulty(int number)
        {
            if(campaignLevels!=null&&number<=campaignLevels.Length&&campaignLevels[number-1]&&(number<=2||campaignLevels[number-1].manuallyAuthored))return campaignLevels[number-1].difficulty;
            return WoolEndlessGenerator.DifficultyFor(number);
        }
        Sprite MapMarker()
        {
            if(mapMarkerSprite)return mapMarkerSprite;
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false);
            texture.name="Map marker";texture.wrapMode=TextureWrapMode.Clamp;
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {
                float distance=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(32,32));
                float shade=distance>28?.72f:1;
                texture.SetPixel(x,y,new Color(shade,shade,shade,Mathf.Clamp01(32-distance)));
            }
            texture.Apply();mapMarkerSprite=Sprite.Create(texture,new Rect(0,0,64,64),new Vector2(.5f,.5f),100);
            return mapMarkerSprite;
        }
        Transform CreateMapLock(Transform parent)
        {
            var root=new GameObject("Level lock",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(parent,false);
            root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            void Part(string name,Vector2 min,Vector2 max,Color color,bool round)
            {
                var image=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image)).GetComponent<UnityEngine.UI.Image>();
                image.transform.SetParent(root,false);image.rectTransform.anchorMin=min;image.rectTransform.anchorMax=max;
                image.rectTransform.offsetMin=image.rectTransform.offsetMax=Vector2.zero;image.color=color;image.raycastTarget=false;
                if(round)image.sprite=MapMarker();
            }
            var ink=WoolArt.Hex("564D40");
            Part("Shackle",new Vector2(.35f,.37f),new Vector2(.65f,.84f),ink,true);
            Part("Shackle opening",new Vector2(.42f,.43f),new Vector2(.58f,.74f),WoolArt.Hex("B7B2A5"),true);
            Part("Body",new Vector2(.29f,.19f),new Vector2(.71f,.56f),ink,false);
            Part("Keyhole",new Vector2(.47f,.28f),new Vector2(.53f,.43f),Color.white,true);
            return root;
        }
        void EnsureMapPaging()
        {
            var banner=mapPage.Find("Map header");if(banner)banner.gameObject.SetActive(false);
            if(mapSummary)mapSummary.gameObject.SetActive(false);
            mapScroll.viewport.offsetMax=Vector2.zero;
            mapScroll.viewport.offsetMin=new Vector2(0,185);
            if(previousMap)return;
            previousMap=MenuButton("Previous map page",mapPage,"<",new Vector2(104,104),Vector2.zero,()=>PageMap(-1),Color.white);
            nextMap=MenuButton("Next map page",mapPage,">",new Vector2(104,104),Vector2.zero,()=>PageMap(1),Color.white);
            foreach(var button in new[]{previousMap,nextMap})
            {
                var image=button.GetComponent<Image>();image.sprite=MapMarker();image.type=Image.Type.Simple;image.color=WoolArt.Hex("FFCA55");
                var label=button.GetComponentInChildren<TMP_Text>();label.color=WoolArt.Hex("493923");label.rectTransform.sizeDelta=new Vector2(90,90);label.fontSize=64;
            }
            Place(previousMap.GetComponent<RectTransform>(),new Vector2(0,.5f),new Vector2(64,0));
            Place(nextMap.GetComponent<RectTransform>(),new Vector2(1,.5f),new Vector2(-64,0));
        }
        void PageMap(int direction)
        {
            if(mapSliding)return;
            int target=mapStart+direction*mapNodes.Length;
            if(target<1||target>WoolProgress.TotalLevels)return;
            StartCoroutine(SlideMap(target,direction));
        }
        System.Collections.IEnumerator SlideMap(int target,int direction)
        {
            mapSliding=true;mapScroll.StopMovement();mapScroll.enabled=false;
            var origin=mapContent.anchoredPosition;
            departingMap=Instantiate(mapContent,mapContent.parent);departingMap.name="Departing map panel";
            foreach(var graphic in departingMap.GetComponentsInChildren<Graphic>())graphic.raycastTarget=false;
            mapStart=target;RefreshMap();
            float width=mapScroll.viewport.rect.width;
            float elapsed=0;
            while(elapsed<.38f&&mapPage.gameObject.activeInHierarchy)
            {
                elapsed+=Mathf.Min(Time.unscaledDeltaTime,.05f);float t=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.38f));
                departingMap.anchoredPosition=origin+Vector2.left*(direction*width*t);
                mapContent.anchoredPosition=origin+Vector2.right*(direction*width*(1-t));
                yield return null;
            }
            mapContent.anchoredPosition=origin;Destroy(departingMap.gameObject);departingMap=null;
            mapScroll.enabled=true;mapSliding=false;RefreshMap();
        }
        void SelectTab(string name)
        {
            FitReferencePages();
            if(menuNavigation){menuNavigation.Select(name);return;}
            var nav=menu.transform.Find("Bottom navigation");if(!nav)return;
            foreach(Transform child in nav)
            {
                var b=child.GetComponent<Button>();if(!b)continue;
                bool selected=child.name==name;var rect=(RectTransform)child;
                var p=rect.anchoredPosition;p.y=selected?16:-5;rect.anchoredPosition=p;
                rect.localScale=Vector3.one*(selected?1.08f:.94f);
                var image=b.GetComponent<Image>();if(image)image.color=selected?WoolArt.Hex("8DE4D5"):Color.white;
                var label=b.GetComponentInChildren<TMP_Text>();if(label){label.color=WoolArt.Hex("164F59");label.fontStyle=selected?FontStyles.Bold:FontStyles.Normal;}
            }
        }
        void RefreshDifficulty(int number)
        {
            if(campaignLevels==null||campaignLevels.Length==0)return;
            var d=LevelDifficulty(number);
            if(homeDifficulty){homeDifficulty.text=d==WoolDifficulty.Hard?"HARD LEVEL":d==WoolDifficulty.Medium?"A fresh challenge":"A gentle adventure";homeDifficulty.color=d==WoolDifficulty.Hard?WoolArt.Hex("FFEFBC"):Color.white;homeDifficulty.outlineColor=WoolArt.Hex("43200F");homeDifficulty.outlineWidth=.2f;}
            if(gameDifficulty){gameDifficulty.text=d.ToString().ToUpperInvariant();gameDifficulty.color=d==WoolDifficulty.Hard?WoolArt.Hex("E44837"):WoolArt.Hex("7C633F");}
        }
    }
}

