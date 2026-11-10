"""Pack the approved 32 poses: each directional row = idle, step A, step B, punch."""
from pathlib import Path
import uuid,re
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
source=Image.open(ROOT/'tools/unarmed-source.png').convert('RGBA')
out=ROOT/'unity/Assets/Resources/Characters/Unarmed'
out.mkdir(parents=True,exist_ok=True)
atlas=Image.new('RGBA',(1024,2048));idle=Image.new('RGBA',(1024,512))
for d in range(8):
 for phase in range(4):
  cell=source.crop((round(phase*source.width/4),round(d*source.height/8),round((phase+1)*source.width/4),round((d+1)*source.height/8)))
  box=cell.getchannel('A').point(lambda a:255 if a>96 else 0).getbbox()
  assert box
  upper=cell.getchannel('A').crop((0,box[1],cell.width,box[1]+70)).point(lambda a:255 if a>96 else 0).getbbox()
  center=(upper[0]+upper[2])/2
  frame=Image.new('RGBA',(256,256))
  frame.alpha_composite(cell,(round(128-center),16-box[1]))
  atlas.paste(frame,(phase*256,d*256))
  if phase==0:idle.paste(frame,(d%4*256,d//4*256))
for name,img in [('combat',atlas),('idle',idle)]:
 p=out/(name+'.png');img.save(p,optimize=True)
 meta=Path(str(p)+'.meta')
 if not meta.exists():
  template=(ROOT/'unity/Assets/Resources/Heroine/idle.png.meta').read_text()
  meta.write_text(re.sub(r'guid: .*','guid: '+uuid.uuid4().hex,template))
 print(p.relative_to(ROOT),p.stat().st_size)
