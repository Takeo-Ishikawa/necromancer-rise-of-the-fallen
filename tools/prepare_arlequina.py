"""Repack the approved preview into Unity's 256px cell contract. Requires Pillow."""
from pathlib import Path
from PIL import Image, ImageOps
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
def put(sheet,index,frame): sheet.alpha_composite(frame,(index%4*256,index//4*256))
for direction,original in enumerate(order):
    put(idle,direction,frames[original])
    put(spectral,direction,frames[original])
    put(spectral,16+direction,frames[original])
# Two approved diagonal walking poses; other directions use a subtle idle step.
for index,frame in [(7,frames[8]),(23,frames[9]),(1,ImageOps.mirror(frames[8])),(17,ImageOps.mirror(frames[9]))]:
    put(spectral,index,frame)
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
