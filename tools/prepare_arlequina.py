"""Repack the approved preview into Unity's 256px cell contract. Requires Pillow."""
from pathlib import Path
from PIL import Image
import uuid
ROOT=Path(__file__).resolve().parents[1]
source=Image.open(ROOT/'tools/arlequina-source.png').convert('RGBA')
out=ROOT/'unity/Assets/Resources/Characters/Arlequina'
out.mkdir(parents=True,exist_ok=True)
frames=[]
for i in range(12):
    x,y=i%4*362,i//4*362
    cell=source.crop((x,y,x+362,y+362))
    bbox=cell.getchannel('A').point(lambda a:255 if a>24 else 0).getbbox()
    crop=cell.crop(bbox)
    # Same scale for all frames, never stretch a pose to fit its cell.
    crop=crop.resize((round(crop.width*.66),round(crop.height*.66)),Image.Resampling.LANCZOS)
    frame=Image.new('RGBA',(256,256))
    frame.alpha_composite(crop,((256-crop.width)//2,242-crop.height))
    frames.append(frame)
# Runtime DirectionFrom: S, SW, W, NW, N, NE, E, SE.
order=[0,7,6,5,4,3,2,1]
idle=Image.new('RGBA',(1024,512))
spectral=Image.new('RGBA',(1024,2048))
def put(sheet,index,frame):
    # Replace ALL RGBA pixels, including transparency. Alpha-compositing a new
    # pose onto an occupied cell leaves the previous legs/arms visible.
    assert frame.size==(256,256)
    sheet.paste(frame,(index%4*256,index//4*256))
for direction,original in enumerate(order):
    put(idle,direction,frames[original])
    put(spectral,direction,frames[original])
    put(spectral,16+direction,frames[original])
# Two contact poses for every direction from the dedicated walking sheet.
walk=Image.open(ROOT/'tools/arlequina-walk-source.png').convert('RGBA')
for direction in range(8):
    for phase in range(2):
        i=direction*2+phase
        x0,x1=round(i%4*walk.width/4),round((i%4+1)*walk.width/4)
        y0,y1=round(i//4*walk.height/4),round((i//4+1)*walk.height/4)
        cell=walk.crop((x0,y0,x1,y1))
        bbox=cell.getchannel('A').point(lambda a:255 if a>24 else 0).getbbox()
        assert bbox is not None
        crop=cell.crop(bbox)
        crop=crop.resize((round(crop.width*.70),round(crop.height*.70)),Image.Resampling.LANCZOS)
        frame=Image.new('RGBA',(256,256))
        frame.alpha_composite(crop,((256-crop.width)//2,242-crop.height))
        put(spectral,direction+phase*16,frame)
for i in range(8,12): put(spectral,i,frames[11])
# Keep the existing projectile art/UV region independently of the character.
legacy=Image.open(ROOT/'unity/Assets/Resources/Heroine/spectral.png').convert('RGBA')
put(spectral,12,legacy.crop((0,768,256,1024)))
put(spectral,13,frames[10])
for name,sheet in [('idle',idle),('spectral',spectral)]:
    path=out/(name+'.png');sheet.save(path,optimize=True)
    meta=Path(str(path)+'.meta')
    if not meta.exists():
        template=(ROOT/'unity/Assets/Resources/Heroine/idle.png.meta').read_text()
        import re
        meta.write_text(re.sub(r'guid: .*', 'guid: '+uuid.uuid4().hex,template))
    print(path.relative_to(ROOT),path.stat().st_size)
