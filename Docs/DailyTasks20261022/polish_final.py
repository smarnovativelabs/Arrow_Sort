from pathlib import Path
p=Path('Docs/DailyTasks20261022/AuthorDailyTasks.cs');s=p.read_text()
s=s.replace('static Texture2D iconsTexture,duckTexture;','static Texture2D iconsTexture,duckTexture;\n    static Color32[] iconPixels;')
s=s.replace('duckTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"/DuckWriting.png");','duckTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"/DuckWriting.png");\n        var cpu=new Texture2D(2,2);cpu.LoadImage(File.ReadAllBytes(dir+"/DailyIcons.png"));iconPixels=cpu.GetPixels32();UnityEngine.Object.DestroyImmediate(cpu);')
s=s.replace('"×"','"X"').replace('"\ufffd"','"X"')
s=s.replace('view.close.GetComponentInChildren<TMP_Text>().fontSize=100;','var closeText=view.close.GetComponentInChildren<TMP_Text>();closeText.fontSizeMax=closeText.fontSize=85;closeText.fontSizeMin=60;')
s=s.replace('view.info.GetComponentInChildren<TMP_Text>().fontSize=82;','var infoText=view.info.GetComponentInChildren<TMP_Text>();infoText.fontSizeMax=infoText.fontSize=82;infoText.fontSizeMin=60;')
s=s.replace('string path=dir+"/"+name+".asset";var existing','string path=dir+"/"+name+(tex==iconsTexture?"-Tight":"")+".asset";var existing')
s=s.replace('var sprite=Sprite.Create(tex,new Rect((cell%columns)*w,tex.height-(cell/columns+1)*h,w,h),','var region=new Rect((cell%columns)*w,tex.height-(cell/columns+1)*h,w,h);if(tex==iconsTexture)region=LargestIconBounds(region);\n        var sprite=Sprite.Create(tex,region,')
a=s.index('    static RectTransform Rect(')
s=s[:a]+'''    static Rect LargestIconBounds(Rect region)
    {
        int x0=(int)region.x,y0=(int)region.y,w=(int)region.width,h=(int)region.height;var seen=new bool[w*h];var queue=new int[w*h];int best=0,bx=0,by=0,bw=w,bh=h;
        for(int index=0;index<w*h;index++)
        {
            if(seen[index]||iconPixels[(y0+index/w)*iconsTexture.width+x0+index%w].a<100)continue;
            int head=0,tail=1,minX=w,minY=h,maxX=0,maxY=0;queue[0]=index;seen[index]=true;
            while(head<tail)
            {
                int n=queue[head++],x=n%w,y=n/w;minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);
                for(int d=0;d<4;d++){int nx=x+(d==0?-1:d==1?1:0),ny=y+(d==2?-1:d==3?1:0);if(nx<0||nx>=w||ny<0||ny>=h)continue;int next=ny*w+nx;if(seen[next])continue;seen[next]=true;if(iconPixels[(y0+ny)*iconsTexture.width+x0+nx].a>=100)queue[tail++]=next;}
            }
            if(tail>best){best=tail;bx=minX;by=minY;bw=maxX-minX+1;bh=maxY-minY+1;}
        }
        return new Rect(x0+Math.Max(0,bx-1),y0+Math.Max(0,by-1),Math.Min(w-bx,bw+2),Math.Min(h-by,bh+2));
    }
'''+s[a:]
s=s.replace('static void Outline(TMP_Text t,Color c){t.fontMaterial=new Material(t.fontSharedMaterial);t.outlineColor=c;t.outlineWidth=.16f;}','''static void Outline(TMP_Text t,Color c)
    {string path=dir+"/Text-"+t.name.Replace(" ","")+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(t.font.material);AssetDatabase.CreateAsset(m,path);}m.SetColor("_OutlineColor",c);m.SetFloat("_OutlineWidth",.16f);m.EnableKeyword("OUTLINE_ON");t.fontSharedMaterial=m;EditorUtility.SetDirty(m);}''')
p.write_text(s,encoding='utf-8')
p=Path('Docs/DailyTasks20261022/WoolDailyTasks.cs');s=p.read_text().replace('"✓"','"OK"').replace('Loading…','Loading...');p.write_text(s,encoding='utf-8')
