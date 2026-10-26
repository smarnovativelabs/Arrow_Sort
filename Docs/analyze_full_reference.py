"""Decode the complete local reference, retaining a one-second visual timeline."""
import sys, json
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / '.reference-tools'))
import cv2
from PIL import Image, ImageDraw

source=Path(r'C:\Users\Muneeb Nazir\Downloads\Screen_Recording_20261007_112414.mp4')
out=Path(__file__).parent/'Reference20261007'
out.mkdir(exist_ok=True)
cap=cv2.VideoCapture(str(source))
fps=cap.get(cv2.CAP_PROP_FPS)
metadata={'source':str(source),'fps':fps,'frame_count':int(cap.get(cv2.CAP_PROP_FRAME_COUNT)),'width':int(cap.get(3)),'height':int(cap.get(4))}
tiles=[];next_second=0;frame_index=0;timeline=[];previous=None
while True:
    ok,frame=cap.read()
    if not ok:break
    second=cap.get(cv2.CAP_PROP_POS_MSEC)/1000
    if second<0:second=frame_index/fps
    gray=cv2.resize(cv2.cvtColor(frame,cv2.COLOR_BGR2GRAY),(72,162))
    motion=float(cv2.absdiff(gray,previous).mean()) if previous is not None else 0
    previous=gray
    timeline.append({'frame':frame_index,'time':round(second,3),'motion':round(motion,3)})
    if second>=next_second:
        image=Image.fromarray(cv2.cvtColor(frame,cv2.COLOR_BGR2RGB));image.thumbnail((142,310))
        tile=Image.new('RGB',(150,338),'#142334');tile.paste(image,((150-image.width)//2,23))
        ImageDraw.Draw(tile).text((6,5),f'{int(second)//60:02}:{int(second)%60:02}',fill='white');tiles.append(tile)
        next_second=int(second)+1
    frame_index+=1
cap.release()
for start in range(0,len(tiles),48):
    chunk=tiles[start:start+48];sheet=Image.new('RGB',(1200,((len(chunk)+7)//8)*338),'#142334')
    for i,tile in enumerate(chunk):sheet.paste(tile,((i%8)*150,(i//8)*338))
    sheet.save(out/f'timeline-{start//48:02}.jpg',quality=94)
metadata.update({'decoded_frames':frame_index,'last_timestamp':timeline[-1]['time'],'timeline_samples':len(tiles),'contact_sheets':(len(tiles)+47)//48})
(out/'metadata.json').write_text(json.dumps(metadata,indent=2))
(out/'frame-timeline.json').write_text(json.dumps(timeline))
print(json.dumps(metadata,indent=2))
