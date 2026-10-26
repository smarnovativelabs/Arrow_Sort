from pathlib import Path
p=Path('Assets/WoolGame/Scripts/WoolSpriteSilhouette.cs');s=p.read_text()
s=s.replace('float w=s.rect.width/s.pixelsPerUnit,h=s.rect.height/s.pixelsPerUnit;','float w=s.rect.width,h=s.rect.height;\n            // OverrideGeometry expects pixels measured from the Sprite rect bottom-left.\n            var center=new Vector2(w*.5f,h*.5f);')
s=s.replace('var v=new Vector2[65];var t=new ushort[192];','var v=new Vector2[65];v[0]=center;var t=new ushort[192];')
s=s.replace('v[i+1]=new Vector2(Mathf.Cos(a)*w,Mathf.Sin(a)*h)*.485f;','v[i+1]=center+new Vector2(Mathf.Cos(a)*w,Mathf.Sin(a)*h)*.485f;')
s=s.replace('new Vector2(-w*.475f,h*.405f)','center+new Vector2(-w*.475f,h*.405f)').replace('new Vector2(w*.475f,h*.405f)','center+new Vector2(w*.475f,h*.405f)').replace('new Vector2(w*.45f,h*.19f)','center+new Vector2(w*.45f,h*.19f)').replace('new Vector2(w*.0875f,-h*.464f)','center+new Vector2(w*.0875f,-h*.464f)').replace('new Vector2(-w*.0875f,-h*.464f)','center+new Vector2(-w*.0875f,-h*.464f)').replace('new Vector2(-w*.45f,h*.19f)','center+new Vector2(-w*.45f,h*.19f)')
Path('Docs/DailyTasks20261022/WoolSpriteSilhouette.cs').write_text(s,encoding='utf-8')
