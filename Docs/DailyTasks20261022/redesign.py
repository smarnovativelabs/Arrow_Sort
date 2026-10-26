from pathlib import Path
p=Path('Docs/DailyTasks20261022/WoolDailyTasks.cs');s=p.read_text();lines=[]
for l in s.splitlines():
 if 'giftLabels[i].text=' in l:l='                giftLabels[i].text=((i+1)*25).ToString();'
 if 'claimLabels[i].text=' in l:l='                claimLabels[i].text=done?"Collected":count>=WoolProgress.DailyTargets[i]?"Collect":i==5?videoBusy?"Loading...":"Watch":"Go";'
 if 'videoBusy=true;message.text=' in l:l='            videoBusy=true;message.text="Waiting for reward video...";Refresh();bool settled=false;'
 if '},failure=>' in l:l='            },failure=>{if(settled)return;settled=true;if(!this)return;videoBusy=false;message.text="VIDEO UNAVAILABLE - "+failure;Refresh();});'
 lines.append(l)
p.write_text('\n'.join(lines)+'\n',encoding='utf-8')
p=Path('Docs/DailyTasks20261022/AuthorDailyTasks.cs');s=p.read_text()
s=s.replace('static Texture2D iconsTexture,duckTexture;','static Texture2D iconsTexture,duckTexture,giftTexture;')
s=s.replace('        var h=UnityEngine.Object.FindAnyObjectByType<WoolHud>();','        ImportGenerated(FindGenerated("GIFT_FILE"),dir+"/GiftStates.png");giftTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"/GiftStates.png");\n        ImportGenerated(FindGenerated("ROOM_FILE"),dir+"/StudyRoom.png");\n        var h=UnityEngine.Object.FindAnyObjectByType<WoolHud>();')
a=s.index('        Gradient("Purple backdrop"');b=s.index('        view.duckFrames=',a)
s=s[:a]+'        Picture("Study room",card,AssetDatabase.LoadAssetAtPath<Sprite>(dir+"/StudyRoom.png"),0,0,941,557);\n'+s[b:]
s=s.replace('26,143,375,410','15,62,395,488')
s=s.replace('401,139,486,371','409,83,481,415').replace('440,153,408,42','445,104,408,42').replace('"COLLECT CREDITS"','"CREDIT CHEST"').replace('446,196,397,57','450,150,397,57').replace('547,266,190,158','550,226,190,158').replace('481,415,322,53','483,397,322,53').replace('448,459,400,43','448,449,400,34')
a=s.index('        view.close=Button(');b=s.index('        Box("Reward band gold border"',a)
s=s[:a]+'''        view.close=Button("Close daily tasks",card,830,8,88,88,"",H("EF3325"));
        var slash1=Box("Cancel stroke 1",view.close.transform,21,36,48,14,"FFFFFF",false);slash1.localRotation=Quaternion.Euler(0,0,45);
        var slash2=Box("Cancel stroke 2",view.close.transform,21,36,48,14,"FFFFFF",false);slash2.localRotation=Quaternion.Euler(0,0,-45);
        view.info=null;
'''+s[b:]
s=s.replace('view.gifts=new Button[4];view.giftArt=new Image[4];view.giftLabels=new TMP_Text[4];','view.gifts=new Button[4];view.giftArt=new Image[4];view.giftLabels=new TMP_Text[4];view.giftClosed=new Sprite[4];view.giftOpen=new Sprite[4];view.giftGlows=new GameObject[4];')
s=s.replace('            int art=giftCells[i];','''            int art=giftCells[i];
            view.giftClosed[i]=Cell("GiftClosedState"+i,giftTexture,i,4,2);view.giftOpen[i]=Cell("GiftOpenState"+i,giftTexture,i+4,4,2);
            var glow=Rect("Collected gift shine",view.gifts[i].transform,-26,-28,166,166);glow.gameObject.AddComponent<WoolRewardGlow>().raycastTarget=false;view.giftGlows[i]=glow.gameObject;glow.gameObject.SetActive(false);''')
s=s.replace('Cell("DailyGift"+i,iconsTexture,art,4,3)','view.giftClosed[i]')
a=s.index('        view.rows=new RectTransform[6]')
s=s[:a]+'''        var scrollRoot=Rect("Task scroll view",card,0,800,941,856);view.taskScroll=scrollRoot.gameObject.AddComponent<ScrollRect>();
        var viewport=Rect("Viewport",scrollRoot,0,0,941,856);var viewportImage=viewport.gameObject.AddComponent<Image>();viewportImage.sprite=rounded;viewportImage.type=Image.Type.Sliced;
        var mask=viewport.gameObject.AddComponent<Mask>();mask.showMaskGraphic=false;
        var content=Rect("Task content",viewport,0,0,941,1108);content.anchorMin=content.anchorMax=new Vector2(.5f,1);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
        view.taskLayout=content.gameObject.AddComponent<VerticalLayoutGroup>();view.taskLayout.padding=new RectOffset(24,24,0,0);view.taskLayout.spacing=20;view.taskLayout.childControlWidth=true;view.taskLayout.childControlHeight=true;view.taskLayout.childForceExpandWidth=true;view.taskLayout.childForceExpandHeight=false;
        var fitter=content.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        view.taskScroll.viewport=viewport;view.taskScroll.content=content;view.taskScroll.horizontal=false;view.taskScroll.vertical=true;view.taskScroll.movementType=ScrollRect.MovementType.Clamped;view.taskScroll.scrollSensitivity=45;
'''+s[a:]
s=s.replace('var row=Rect("Daily task "+i,card,24,800+i*143,893,132);','var row=Rect("Daily task "+i,content,24,i*188,893,168);var rowSize=row.gameObject.AddComponent<LayoutElement>();rowSize.preferredHeight=168;')
s=s.replace('row,0,0,893,132','row,0,0,893,168').replace('16,8,135,116','14,13,139,139').replace('165,7,482,52,36','169,18,470,59,42').replace('176,80,350,31','176,109,350,34').replace('176,76,350,39,29','176,106,350,41,32').replace('row,653,0,240,132','row,653,0,240,168').replace('680,5,57,55','679,14,61,61').replace('746,5,130,55,40','746,17,130,55,43').replace('684,67,191,57','680,92,198,62')
a=s.index('        var shield=view.preview.AddComponent<Image>()');b=s.index('        view.preview.SetActive(false);',a)
s=s[:a]+'''        view.previewCard=Surface("Reward preview card",view.preview.transform,0,0,174,180,WoolSpinSurface.Kind.CreamCard);view.previewCard.anchorMin=view.previewCard.anchorMax=new Vector2(0,1);
        var pointer=Box("Reward bubble pointer",view.previewCard,70,164,34,34,"FFD776",false);pointer.localRotation=Quaternion.Euler(0,0,45);pointer.SetAsFirstSibling();
        view.coinSprite=Cell("DailyCoinStack",iconsTexture,1,4,3);Picture("Reward coins",view.previewCard,view.coinSprite,46,15,84,66);
        view.previewAmount=Text("Reward amount",view.previewCard,"x20",13,81,148,51,43,H("713217"));Outline(view.previewAmount,H("FFFFFF"));
        view.previewState=Text("Reward status",view.previewCard,"Tap to collect",7,135,160,32,22,H("713217"));
        view.previewCollect=Button("Collect gift",view.previewCard,0,0,174,180,"",null);
        view.previewClose=Button("Close gift preview",view.previewCard,143,-9,39,39,"X",H("EF5B2C"));
'''+s[b:]
s=s.replace('h.ConfigureInputTargets();shield.raycastTarget=true;','h.ConfigureInputTargets();Canvas.ForceUpdateCanvases();view.taskScroll.verticalNormalizedPosition=1;')
s=s.replace('    static void ImportGenerated(','''    static string FindGenerated(string file)=>@"C:\\Users\\Muneeb Nazir\\.codex\\generated_images\\01a16219-77bf-7a63-ab37-39101461f6b0\\"+file;
    static void ImportGenerated(''')
p.write_text(s,encoding='utf-8')
