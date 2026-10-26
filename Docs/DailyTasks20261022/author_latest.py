from pathlib import Path
p=Path('Docs/DailyTasks20261022/AuthorDailyTasks.cs');s=p.read_text(encoding='utf-8')
s=s.replace('        Box("Navy background",card,0,0,941,1672,"192D55",false);','''        var priorBackground=panel.transform.Find("Daily full background");if(priorBackground)UnityEngine.Object.DestroyImmediate(priorBackground.gameObject);
        var full=Picture("Daily full background",panel.transform,AssetDatabase.LoadAssetAtPath<Sprite>(dir+"/StudyRoom.png"),0,0,941,1672);full.anchorMin=full.anchorMax=new Vector2(.5f,.5f);full.anchoredPosition=Vector2.zero;full.SetAsFirstSibling();view.fullBackground=full.GetComponent<Image>();
        Gradient("Task board frame",card,10,786,921,876,"6E4230","251924","DAA357",28,5);''')
s=s.replace('int[] taskCells={0,1,-1,2,3,4};','int[] taskCells={-1,1,0,3,-1,-1,-1,-1};')
s=s.replace('view.rows=new RectTransform[6];view.rowArt=new Graphic[6];view.fills=new Image[6];view.claims=new Button[6];view.counts=new TMP_Text[6];view.claimLabels=new TMP_Text[6];','int taskCount=WoolDailyTaskState.Titles.Length;view.rows=new RectTransform[taskCount];view.rowArt=new Graphic[taskCount];view.fills=new Image[taskCount];view.claims=new Button[taskCount];view.counts=new TMP_Text[taskCount];view.claimLabels=new TMP_Text[taskCount];')
s=s.replace('941,1108','941,1484')
s=s.replace('        for(int i=0;i<6;i++)','        for(int i=0;i<taskCount;i++)')
s=s.replace('var unlockIcon=System.Linq.Enumerable.First(unlockButton.GetComponentsInChildren<Image>(true),im=>im.name=="Icon 0").sprite;','''var abilityIcons=new Sprite[4];for(int kind=0;kind<4;kind++){var ability=System.Linq.Enumerable.First(h.GetComponentsInChildren<Button>(true),b=>b.name==WoolBoosters.Names[kind]);abilityIcons[kind]=System.Linq.Enumerable.First(ability.GetComponentsInChildren<Image>(true),im=>im.name=="Icon "+kind).sprite;}
        var loginButton=System.Linq.Enumerable.First(h.GetComponentsInChildren<Button>(true),b=>b.name=="Tasks");var loginIcon=System.Linq.Enumerable.First(loginButton.GetComponentsInChildren<Image>(true),im=>im.name=="Animated icon").sprite;''')
s=s.replace('i==2?unlockIcon:Cell("TaskIconFresh"+i,iconsTexture,taskCells[i],4,3)','i==0?loginIcon:i>=4?abilityIcons[i-4]:Cell("LatestTaskIcon"+i,iconsTexture,taskCells[i],4,3)')
s=s.replace('        EditorUtility.SetDirty(view);EditorUtility.SetDirty(h);panel.SetActive(false);','''        var badges=h.GetComponent<WoolHomeRewardBadges>();if(!badges)badges=h.gameObject.AddComponent<WoolHomeRewardBadges>();
        var home=(RectTransform)so.FindProperty("referenceHome").objectReferenceValue;
        var homeButtons=home.GetComponentsInChildren<Button>(true);
        badges.tasks=Badge(System.Linq.Enumerable.First(homeButtons,b=>b.name=="Tasks").transform,"Daily ready badge",28,false);
        badges.spin=Badge(System.Linq.Enumerable.First(homeButtons,b=>b.name=="Lucky Spin").transform,"Spin ready badge",30,true);
        badges.shop=Badge(System.Linq.Enumerable.First(homeButtons,b=>b.name=="Shop").transform,"Shop reward badge",36,true);
        var shop=h.GetComponentInChildren<WoolShopView>(true);if(shop&&shop.dailyClaim)badges.freeCoins=Badge(shop.dailyClaim.transform,"Free coin reward badge",34,true);
        badges.Refresh();h.ConfigureInputTargets();EditorUtility.SetDirty(badges);
        EditorUtility.SetDirty(view);EditorUtility.SetDirty(h);panel.SetActive(false);''')
a=s.index('    static string FindGenerated(')
s=s[:a]+'''    static GameObject Badge(Transform parent,string name,float size,bool mark)
    {
        var prior=parent.Find(name);if(prior)UnityEngine.Object.DestroyImmediate(prior.gameObject);
        var rt=Gradient(name,parent,0,0,size,size,"FF6256","E41325","FFF5DE",size*.5f,2);rt.anchorMin=rt.anchorMax=Vector2.one;rt.anchoredPosition=new Vector2(-12,-12);
        if(mark)Text("Alert",rt,"!",0,0,size,size,Mathf.RoundToInt(size*.8f),Color.white);return rt.gameObject;
    }
'''+s[a:]
p.write_text(s,encoding='utf-8')
