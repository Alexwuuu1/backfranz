"""Independent reader: validate ASE chunks and compare composited RGBA with exports.

Supports the RGB, normal-blend, image-layer sources authored in crear_assets.py.
This validates binary structure and pixels, not an Aseprite application launch.
"""
from pathlib import Path
import json
import struct
import zlib
from PIL import Image


def read_ase(path):
    data=path.read_bytes()
    size,magic,frames,w,h,depth=struct.unpack_from('<I5H',data)
    assert size==len(data) and magic==0xa5e0 and depth==32, path
    layers=[];result=[];position=128
    for index in range(frames):
        length,magic,old_count,duration,count=struct.unpack_from('<IHHH2xI',data,position)
        assert magic==0xf1fa and duration>0
        count=count or old_count
        cursor=position+16;render={}
        for _ in range(count):
            chunk_length,kind=struct.unpack_from('<IH',data,cursor)
            assert chunk_length>=6 and cursor+chunk_length<=position+length
            payload=data[cursor+6:cursor+chunk_length]
            if kind==0x2004:
                flags,typ,level,_,_,blend,opacity=struct.unpack_from('<6HB',payload)
                assert typ==0 and level==0 and blend==0 and opacity==255
                namelen=struct.unpack_from('<H',payload,16)[0]
                name=payload[18:18+namelen].decode('utf8')
                layers.append((flags,name))
            elif kind==0x2005:
                layer,x,y,opacity,typ,z=struct.unpack_from('<HhhBHh',payload)
                assert layer<len(layers) and typ==2 and opacity==255 and z==0
                cw,ch=struct.unpack_from('<HH',payload,16)
                raw=zlib.decompress(payload[20:])
                assert len(raw)==cw*ch*4
                if layers[layer][0]&1:
                    render[layer]=(x,y,Image.frombytes('RGBA',(cw,ch),raw))
            cursor+=chunk_length
        assert cursor==position+length
        canvas=Image.new('RGBA',(w,h))
        for layer in sorted(render):
            x,y,image=render[layer];canvas.alpha_composite(image,(x,y))
        result.append(canvas);position+=length
    assert position==len(data)
    return result,layers


def main():
    source=Path(__file__).resolve().parent;root=source.parents[1]
    manifest=json.loads((source/'assets.json').read_text(encoding='utf8'))
    total=0
    for entry in manifest:
        frames,layers=read_ase(root/entry['source'])
        assert len(frames)==len(entry['exports'])==entry['frames']
        assert len(layers)==entry['layers']
        for frame,filename in zip(frames,entry['exports']):
            export=Image.open(root/filename).convert('RGBA')
            # Invisible RGB in imported alpha-zero pixels is immaterial to rendering.
            for image in (frame,export):
                image.paste((0,0,0,0),mask=image.getchannel('A').point(lambda alpha:255 if alpha==0 else 0))
            assert frame.size==export.size and frame.tobytes()==export.tobytes(),filename
            total+=1
    print(f'PASS: {len(manifest)} archivos ASE válidos; {total} fotogramas coinciden exactamente con sus PNG.')


if __name__=='__main__':main()
