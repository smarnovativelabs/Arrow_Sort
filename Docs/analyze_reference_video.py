"""Extract timestamped evidence frames from the user-supplied local recording."""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'Temp' / 'ReferenceTools'))
import cv2
from PIL import Image, ImageDraw

source = Path(r'C:\Users\Muneeb Nazir\Downloads\First Level Video.mp4')
out = Path(__file__).parent / 'ReferenceFrames'
out.mkdir(exist_ok=True)
cap = cv2.VideoCapture(str(source))
fps = cap.get(cv2.CAP_PROP_FPS)
count = cap.get(cv2.CAP_PROP_FRAME_COUNT)
duration = count / fps
print({'fps': fps, 'frames': count, 'seconds': duration, 'width': cap.get(3), 'height': cap.get(4)})
times = [float(x) for x in sys.argv[1:]] if len(sys.argv) > 1 else list(range(0, int(duration), 5))
thumbs = []
for sec in times:
    cap.set(cv2.CAP_PROP_POS_MSEC, sec * 1000)
    ok, frame = cap.read()
    if not ok:
        continue
    im = Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
    im.save(out / f'frame-{sec:07.2f}.jpg', quality=93)
    im.thumbnail((240, 420))
    tile = Image.new('RGB', (250, 450), '#172333')
    tile.paste(im, ((250-im.width)//2, 25))
    ImageDraw.Draw(tile).text((8, 5), f'{sec:.2f}s', fill='white')
    thumbs.append(tile)
for start in range(0, len(thumbs), 16):
    subset = thumbs[start:start+16]
    sheet = Image.new('RGB', (1000, ((len(subset)+3)//4)*450), '#172333')
    for idx, tile in enumerate(subset):
        sheet.paste(tile, ((idx%4)*250, (idx//4)*450))
    path = out / f'contact-{start//16:02d}.jpg'
    sheet.save(path, quality=93)
    print(path.resolve())
cap.release()
