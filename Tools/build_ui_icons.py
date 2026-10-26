"""Original, scalable symbol artwork for the four rescue abilities."""
from pathlib import Path
from PIL import Image, ImageDraw
import math

OUT = Path(__file__).resolve().parents[1] / 'Assets/WoolGame/Designer/UI/Release'
OUT.mkdir(parents=True, exist_ok=True)
S=4
INK='#59361E'
def make(name, painter):
    im=Image.new('RGBA',(512*S,512*S)); d=ImageDraw.Draw(im)
    def box(r,fill,radius=22,outline=INK,width=9):
        d.rounded_rectangle(tuple(int(x*S) for x in r),radius=radius*S,fill=fill,outline=outline,width=width*S)
    def line(points,fill,width=12):
        d.line([(int(x*S),int(y*S)) for x,y in points],fill=fill,width=width*S,joint='curve')
    def poly(points,fill,outline=INK,width=7):
        p=[(int(x*S),int(y*S)) for x,y in points];d.polygon(p,fill=fill)
        if outline:d.line(p+[p[0]],fill=outline,width=width*S,joint='curve')
    def circle(x,y,r,fill,outline=INK,width=7):
        d.ellipse(((x-r)*S,(y-r)*S,(x+r)*S,(y+r)*S),fill=fill,outline=outline,width=width*S)
    painter(box,line,poly,circle,d)
    im.resize((512,512),Image.Resampling.LANCZOS).save(OUT/(name+'.png'))

def unlock(b,l,p,c,d):
    b((66,325,446,436),'#F7C765',30);b((94,342,418,410),'#74513C',20,width=5)
    # Open shackle visibly clears the body on its right side.
    d.arc((154*S,57*S,346*S,251*S),155,354,fill=INK,width=40*S)
    d.arc((163*S,66*S,337*S,242*S),155,354,fill='#FFF0A4',width=22*S)
    b((135,186,377,360),'#72CE67',36);b((151,203,361,260),'#A5EC7B',24,outline=None,width=0)
    c(257,254,23,'#385C37',outline=None);b((245,258,269,306),'#385C37',7,outline=None,width=0)
    c(403,148,66,'#FFDC64');l([(403,110),(403,186)],'#FFFFFF',16);l([(365,148),(441,148)],'#FFFFFF',16)

def remove(b,l,p,c,d):
    # A vehicle lifting out of its parking footprint.
    b((72,363,440,445),'#A6C8D4',27);b((99,382,413,425),'#537D8D',15,width=5)
    b((126,216,386,350),'#FF9C65',36);b((169,174,343,270),'#FFC171',29)
    b((184,187,328,248),'#99E4EE',17,width=6);l([(198,200),(283,200)],'#E9FEFF',8)
    c(167,333,31,'#654B42');c(345,333,31,'#654B42');c(167,333,13,'#FFD46A',width=4);c(345,333,13,'#FFD46A',width=4)
    p([(255,49),(329,126),(284,126),(284,192),(226,192),(226,126),(181,126)],'#76DC90')
    l([(87,237),(87,299)],'#FFE59B',13);l([(425,235),(425,297)],'#FFE59B',13)

def sort(b,l,p,c,d):
    # Two opposing arrows exchange rows of colored snake segments.
    p([(80,125),(346,125),(346,86),(436,158),(346,228),(346,188),(80,188)],'#74DCE6')
    p([(432,324),(166,324),(166,285),(76,357),(166,429),(166,388),(432,388)],'#FFE16A')
    for x,col in [(156,'#B779EC'),(256,'#F59168'),(356,'#81D77D')]:
        c(x,255,47,col);d.arc(((x-30)*S,223*S,(x+30)*S,276*S),192,286,fill='#FFFFFFB0',width=7*S)

def fog(b,l,p,c,d):
    # A bright projectile passes all the way through a fog bank.
    for x,y,r in [(172,283,78),(246,227,91),(332,275,83)]:c(x,y,r,'#B6B6DF',width=8)
    b((100,270,406,356),'#B6B6DF',32,outline=None,width=0)
    l([(105,353),(404,353)],INK,8)
    p([(64,300),(293,200),(269,157),(456,153),(366,318),(342,273),(94,367)],'#FFE780')
    l([(112,306),(329,211)],'#FFFFD5',13)
    for x,y in [(414,86),(450,367),(85,190)]:
        p([(x,y-22),(x+7,y-7),(x+22,y),(x+7,y+7),(x,y+22),(x-7,y+7),(x-22,y),(x-7,y-7)],'#FFF3B3',outline=None)

for name,fn in [('AbilityUnlock',unlock),('AbilityRemove',remove),('AbilitySort',sort),('AbilityFog',fog)]:make(name,fn)

# Nine-sliceable surfaces: the corner artwork retains its radius at every screen ratio.
for name,top,bottom,border in [('CreamPanel',(255,249,225),(244,220,177),'#C89749'),('GreenButton',(139,222,91),(54,158,65),'#347843'),('BlueButton',(127,211,221),(55,133,164),'#3B718E')]:
    im=Image.new('RGBA',(512,256));mask=Image.new('L',im.size);ImageDraw.Draw(mask).rounded_rectangle((3,3,509,253),radius=42,fill=255)
    d=ImageDraw.Draw(im)
    for y in range(256):
        t=y/255;col=tuple(round(a*(1-t)+b*t) for a,b in zip(top,bottom));d.line((0,y,512,y),fill=col)
    im.putalpha(mask);d=ImageDraw.Draw(im);d.rounded_rectangle((4,4,508,252),radius=42,outline=border,width=8);d.rounded_rectangle((15,15,497,241),radius=32,outline='#FFF4CF',width=4)
    im.save(OUT/(name+'.png'))
im=Image.new('RGBA',(512,512));d=ImageDraw.Draw(im)
d.rounded_rectangle((12,22,500,506),radius=96,fill='#9D6A2E')
d.rounded_rectangle((8,6,504,490),radius=96,fill='#E9B65D',outline='#A96C26',width=10)
d.rounded_rectangle((21,17,491,473),radius=84,fill='#FFF3CF',outline='#FFFCE9',width=10)
d.rounded_rectangle((34,32,478,456),radius=73,outline='#F4D892',width=5)
im.save(OUT/'AbilityTile.png')
print(OUT)
