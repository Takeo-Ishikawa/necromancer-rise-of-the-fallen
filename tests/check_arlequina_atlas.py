"""Pixel regression checks for sprite replacement; run with Pillow installed."""
import runpy
from pathlib import Path
from PIL import Image, ImageChops
root=Path(__file__).resolve().parents[1]
packer=runpy.run_path(str(root/'tools/prepare_arlequina.py'))
# A new transparent pose must erase the old silhouette, not blend over it.
atlas=Image.new('RGBA',(1024,512),(255,0,0,255))
pose=Image.new('RGBA',(256,256))
pose.putpixel((128,128),(0,255,0,255))
packer['put'](atlas,5,pose)
assert atlas.crop((256,256,512,512)).tobytes()==pose.tobytes()
assert atlas.getpixel((0,0))==(255,0,0,255), 'Adjacent cell changed'
folder=root/'unity/Assets/Resources/Characters/Arlequina'
idle=Image.open(folder/'idle.png').convert('RGBA')
walk=Image.open(folder/'spectral.png').convert('RGBA')
def cell(sheet,i):
 return sheet.crop((i%4*256,i//4*256,i%4*256+256,i//4*256+256))
for direction in range(8):
 a,b=cell(walk,direction),cell(walk,16+direction)
 assert a.tobytes()!=b.tobytes(), ('identical strides',direction)
 for frame in (a,b):
  assert frame.tobytes()!=cell(idle,direction).tobytes(), ('idle used as contact',direction)
  box=frame.getchannel('A').point(lambda v:255 if v>24 else 0).getbbox()
  assert box and box[0]>0 and box[1]>0 and box[2]<256 and box[3]<=242,(direction,box)
print('Arlequina: full RGBA replacement, cell isolation and 8 walking pairs OK')
