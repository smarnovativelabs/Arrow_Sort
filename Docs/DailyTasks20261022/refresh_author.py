from pathlib import Path
p=Path('Docs/DailyTasks20261022/AuthorDailyTasks.cs')
s=p.read_text()
s=s.replace('static Texture2D texture;','static Texture2D texture,iconsTexture,duckTexture;')
a=s.index('        string image=dir+')
b=s.index('        var h=UnityEngine.Object.FindAnyObjectByType',a)
s=s[:a]+'''        ImportGenerated(@"C:\\Users\\Muneeb Nazir\\.codex\\generated_images\\01a16219-77bf-7a63-ab37-39101461f6b0\\exec-3628ec6b-9d9f-44d8-a5de-dcf7083c6be3.png",dir+"/DailyIcons.png");
        ImportGenerated(@"C:\\Users\\Muneeb Nazir\\.codex\\generated_images\\01a16219-77bf-7a63-ab37-39101461f6b0\\exec-584af201-dbfe-4b1b-8df1-c27a92995cf2.png",dir+"/DuckWriting.png");
        iconsTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"/DailyIcons.png");duckTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"/DuckWriting.png");
'''+s[b:]
a=s.index('        Picture("Duck and header artwork"')
b=s.index('        // The original two-chest board',a)
s=s[:a]+'''        Gradient("Purple backdrop",card,0,0,941,556,"693A9C","352268","3A246B",16,0);
        for(int i=0;i<8;i++){var dot=Box("Backdrop sparkle "+i,card,45+i*114,160+(i%3)*115,30+i%2*18,30+i%2*18,"7945AB",false);dot.localRotation=Quaternion.Euler(0,0,35);}
        Surface("Gold title banner",card,22,3,897,132,WoolSpinSurface.Kind.Banner);
        var title=Text("Daily Tasks heading",card,"Daily Tasks",175,6,590,117,92,Color.white);Outline(title,H("6F2809"));
        view.duckFrames=new Sprite[4];for(int i=0;i<4;i++)view.duckFrames[i]=Cell("DuckFrame"+i,duckTexture,i,2,2);
        view.duck=Picture("Duck clerk",card,view.duckFrames[0],26,143,375,410);view.duckImage=view.duck.GetComponent<Image>();view.duckImage.preserveAspect=true;
'''+s[b:]
s=s.replace('Picture("One chest",view.chest.transform,Crop("Chest",475,318,144,105),0,0,190,139);','view.chestClosed=Cell("GrandChest",iconsTexture,10,4,3);view.chestOpen=Cell("GrandChestOpen",iconsTexture,11,4,3);\n        view.chestImage=Picture("One chest",view.chest.transform,view.chestClosed,-5,-9,200,158).GetComponent<Image>();view.chestImage.preserveAspect=true;')
a=s.index('        // Writing overlay:')
b=s.index('        Box("Reward band gold border"',a)
s=s[:a]+'''        view.pencil=null;view.writingHand=null;view.notebook=null;view.ink=new Image[0];
        view.close=Button("Close daily tasks",card,790,21,112,112,"×",H("EF3325"));
        view.close.GetComponentInChildren<TMP_Text>().fontSize=100;
        view.info=Button("Daily tasks information",card,52,24,104,104,"i",H("00A6F6"));
        view.info.GetComponentInChildren<TMP_Text>().fontSize=82;
'''+s[b:]
s=s.replace('Picture("Daily Reward title",card,Crop("RewardTitle",263,516,418,76),263,516,418,76);','Gradient("Daily Reward title",card,263,516,418,76,"009DFF","005DBD","FFDD3D",38,7);\n        var rewardTitle=Text("Daily Reward heading",card,"Daily Reward",278,522,388,60,46,Color.white);Outline(rewardTitle,H("133A76"));')
s=s.replace('view.creditSprite=Crop("CleanCreditCrown2",720,805,65,60);','view.creditSprite=Cell("CrownCredit",iconsTexture,5,4,3);')
s=s.replace('var creditMat=KeyMaterial("CreditIcon",new Color(.45f,.28f,.96f),.25f);','Material creditMat=null;')
s=s.replace('int[] giftX={400,586,789};int[] giftW={88,88,91};','int[] giftCells={6,7,8,9};')
s=s.replace('int art=i==0?0:i-1;','int art=giftCells[i];')
s=s.replace('Crop("CleanGiftArt"+art,giftX[art],592,giftW[art],72)','Cell("DailyGift"+i,iconsTexture,art,4,3)')
s=s.replace('g.GetComponent<Image>().material=KeyMaterial("GiftIcon",new Color(.005f,.39f,.73f),.20f);','g.GetComponent<Image>().preserveAspect=true;')
s=s.replace('int[] ys={802,941,1081,1223,1371,1519};','int[] taskCells={0,1,-1,2,3,4};\n        var unlockButton=System.Linq.Enumerable.First(h.GetComponentsInChildren<Button>(true),b=>b.name==WoolBoosters.Names[0]);\n        var unlockIcon=System.Linq.Enumerable.First(unlockButton.GetComponentsInChildren<Image>(true),im=>im.name=="Icon 0").sprite;')
s=s.replace('Picture("Task icon",row,Crop("TaskIcon"+i,37,ys[i]+5,142,112),12,9,142,112);','var taskIcon=Picture("Task icon",row,i==2?unlockIcon:Cell("TaskIconFresh"+i,iconsTexture,taskCells[i],4,3),16,8,135,116).GetComponent<Image>();taskIcon.preserveAspect=true;')
s=s.replace('view.coinSprite=Crop("Coins",48,950,117,98);','view.coinSprite=Cell("DailyCoinStack",iconsTexture,1,4,3);')
a=s.index('    static Sprite Crop(')
b=s.index('    static RectTransform Rect(',a)
s=s[:a]+'''    static void ImportGenerated(string source,string path)
    {
        if(!File.Exists(path))File.Copy(source,path);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.SaveAndReimport();
    }
    static Sprite Cell(string name,Texture2D tex,int cell,int columns,int rows)
    {
        string path=dir+"/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(existing)return existing;
        float w=tex.width/(float)columns,h=tex.height/(float)rows;
        var sprite=Sprite.Create(tex,new Rect((cell%columns)*w,tex.height-(cell/columns+1)*h,w,h),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
'''+s[b:]
s=s.replace('var g=Gradient("Glossy button face",rt,0,0,w,h,"FFD33B","F68B06","FFF09D",20,3);g.SetAsFirstSibling();','string top="FFD33B",bottom="F68B06";float radius=20;var c=color.Value;if(c.g>.6f&&c.r<.5f&&c.b<.5f){top="9EF241";bottom="359C07";}else if(c.r>.7f&&c.g<.4f){top="FF6863";bottom="CF071F";radius=50;}else if(c.b>.8f&&c.r<.2f){top="24D2FF";bottom="0074D4";radius=50;}var g=Gradient("Glossy button face",rt,0,0,w,h,top,bottom,"FFF09D",radius,4);g.SetAsFirstSibling();')
p.write_text(s)
