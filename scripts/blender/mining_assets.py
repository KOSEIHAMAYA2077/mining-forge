"""Run in the existing localhost Blender MCP session, in two deliberate stages."""
import bpy, math, random
from pathlib import Path
from mathutils import Vector

ROOT = Path(r'C:/Users/kouha/Documents/ChatGPT/採掘系のゲーム作成？')
OUT = ROOT / 'game/Assets/Resources/MiningPrototype/Models'
SOURCE = ROOT / 'art/source/mining-prototype'

def setup():
    if bpy.data.scenes.get('MiningPrototypeAssets'):
        raise RuntimeError('Asset scene already exists; inspect before replacing')
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'tmp/blender-before-mining.blend'), copy=True)
    scene=bpy.data.scenes.new('MiningPrototypeAssets')
    bpy.context.window.scene=scene
    scene.unit_settings.system='METRIC'; scene.unit_settings.scale_length=1

def mat(name, color, metal=0, rough=.65):
    m=bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.diffuse_color=(*color,1); m.use_nodes=True
    bs=m.node_tree.nodes.get('Principled BSDF')
    bs.inputs['Base Color'].default_value=(*color,1)
    bs.inputs['Metallic'].default_value=metal;bs.inputs['Roughness'].default_value=rough
    return m

def mesh(name, verts, faces, material):
    data=bpy.data.meshes.new(name);data.from_pydata(verts,[],faces);data.update()
    ob=bpy.data.objects.new(name,data);bpy.context.scene.collection.objects.link(ob);ob.data.materials.append(material)
    return ob

def crystal(name, radius, height, material, location=(0,0,0)):
    v=[]
    for z,r in [(0,radius*.75),(height*.68,radius),(height*.87,radius*.7)]:
        for i in range(6):
            a=i*math.tau/6;v.append((math.cos(a)*r,math.sin(a)*r,z))
    v.append((radius*.1,0,height));f=[tuple(reversed(range(6)))]
    for row in range(2):
        for i in range(6):
            a=row*6+i;b=row*6+(i+1)%6;f.append((a,b,b+6,a+6))
    for i in range(6):f.append((12+i,12+(i+1)%6,18))
    ob=mesh(name,v,f,material);ob.location=location;return ob

def rock(name, radius, material, seed, location=(0,0,0), flatten=.65):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=radius,location=location)
    ob=bpy.context.object;ob.name=name;r=random.Random(seed)
    for v in ob.data.vertices:
        v.co*=r.uniform(.82,1.16);v.co.z=max(0,v.co.z*flatten+radius*flatten)
    ob.data.materials.append(material);return ob

def bar(name, start, end, radius, material, vertices=8):
    d=Vector(end)-Vector(start)
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=d.length,location=(Vector(start)+Vector(end))/2)
    ob=bpy.context.object;ob.name=name;ob.rotation_euler=d.to_track_quat('Z','Y').to_euler();ob.data.materials.append(material);return ob

def export(name, objects):
    bpy.ops.object.select_all(action='DESELECT')
    for ob in objects: ob.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,global_scale=1,mesh_smooth_type='OFF',use_mesh_modifiers=True,add_leaf_bones=False,bake_anim=False)
    for ob in objects:ob.hide_set(True)

def stage_one():
    setup()
    stone=mat('MP_Basalt',(.19,.24,.28)); cyan=mat('MP_Crystal',(.05,.65,.78),.2,.28)
    core=crystal('Core',.31,1.25,cyan,(0,0,.15))
    shell=rock('Shell',.59,stone,17,flatten=.46)
    export('OreNode',[core,shell])
    wood=mat('MP_Handle',(.29,.13,.045)); steel=mat('MP_Steel',(.38,.48,.55),.65,.32);gold=mat('MP_Grip',(.9,.53,.12),.3)
    objs=[bar('Handle',(0,0,-.13),(0,0,.79),.046,wood),bar('Grip',(0,0,-.13),(0,0,.19),.058,gold)]
    # Asymmetric pointed head; Unity Y-up after FBX conversion. Origin is the hand grip.
    profile=[(-.47,.69),(-.49,.8),(-.16,.89),(.14,.89),(.42,.78),(.58,.59),(.36,.69),(.11,.77),(-.16,.78)]
    verts=[(x,y,z) for y in [-.075,.075] for x,z in profile];n=len(profile)
    faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    objs.append(mesh('PickHead',verts,faces,steel))
    tip=bpy.data.objects.new('StrikeTip',None);bpy.context.scene.collection.objects.link(tip);tip.location=(.58,0,.59);objs.append(tip)
    export('Pickaxe',objs)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'mining-prototype.blend'))
    print('STAGE_ONE exported OreNode and Pickaxe; original Scene preserved')

def stage_two():
    bpy.context.window.scene=bpy.data.scenes['MiningPrototypeAssets']
    stone=bpy.data.materials['MP_Basalt'];cyan=bpy.data.materials['MP_Crystal']
    dark=mat('MP_Crack',(.025,.035,.045));broken=[]
    for i in range(5):
        a=i*math.tau/5;broken.append(rock('Rubble'+str(i),.23,stone,i+40,(math.cos(a)*.34,math.sin(a)*.3,0),.45))
    export('BrokenRock',broken)
    export('RockChip',[rock('Chip',.09,stone,77,flatten=.8)])
    export('CrystalDrop',[crystal('Drop',.18,.46,cyan)])
    cracks=[]
    for i,pts in enumerate([[(-.24,-.28,.62),(-.07,-.32,.49),(-.14,-.4,.31),(.07,-.46,.21)],[(.31,-.19,.59),(.2,-.31,.45),(.28,-.38,.28)],[(0,-.32,.5),(.17,-.33,.52)]]):
        for j in range(len(pts)-1):cracks.append(bar('Crack'+str(i)+'_'+str(j),pts[j],pts[j+1],.015,dark,5))
    export('Cracks',cracks)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'mining-prototype.blend'))
    print('STAGE_TWO exported rubble, chip, drop, cracks')

def icons():
    s=bpy.data.scenes['MiningPrototypeAssets'];bpy.context.window.scene=s
    for ob in s.objects:ob.hide_render=True
    s.render.engine='BLENDER_EEVEE_NEXT';s.render.resolution_x=s.render.resolution_y=256;s.render.resolution_percentage=100;s.render.film_transparent=True
    s.render.image_settings.file_format='PNG';s.render.image_settings.color_mode='RGBA'
    s.world=bpy.data.worlds.new('AssetIconWorld');s.world.use_nodes=True;s.world.node_tree.nodes['Background'].inputs[0].default_value=(.25,.3,.38,1)
    camera=bpy.data.objects.new('AssetIconCamera',bpy.data.cameras.new('AssetIconCamera'));s.collection.objects.link(camera);s.camera=camera;camera.data.type='ORTHO'
    lamp=bpy.data.objects.new('AssetIconLight',bpy.data.lights.new('AssetIconLight','AREA'));s.collection.objects.link(lamp);lamp.location=(2,-3,5);lamp.data.energy=350;lamp.data.size=3
    folder=ROOT/'game/Assets/Resources/MiningPrototype/Icons';folder.mkdir(parents=True,exist_ok=True)
    for filename,names,target,scale in [('crystal',['Drop'],(0,0,.23),.62),('pickaxe',['Handle','Grip','PickHead'],(0,0,.39),1.32)]:
        visible=[s.objects[n] for n in names]
        for ob in visible:ob.hide_render=False;ob.hide_set(False)
        camera.location=Vector(target)+Vector((1,-3,1.3));camera.rotation_euler=(Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.ortho_scale=scale
        s.render.filepath=str(folder/(filename+'.png'));bpy.ops.render.render(write_still=True)
        for ob in visible:ob.hide_render=True;ob.hide_set(True)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'mining-prototype.blend'))
    print('Rendered crystal and pickaxe UI icons, transparent 256x256')

def board_models():
    bpy.context.window.scene=bpy.data.scenes['MiningPrototypeAssets']
    stone=mat('MP_BoardStone',(.25,.29,.35));edge=mat('MP_BoardEdge',(.12,.16,.22));vein=mat('MP_BoardVein',(.10,.48,.55),.35,.4)
    layouts={'ring':[(1,0),(1,1)],'blade':[(0,0),(1,0),(2,0)],'shield':[(0,0),(0,1),(1,0),(1,1)],'axe':[(0,0),(0,1),(1,0),(1,1),(2,0),(3,0)],'robe':[(r,c) for r in range(4) for c in range(2)],'cross':[(0,0),(0,1),(1,0),(2,0)]}
    for name,cells in layouts.items():
        verts=[];faces=[];slots=[];index={}
        def v(x,z,depth):
            key=(x,z,depth)
            if key not in index:
                index[key]=len(verts);verts.append((x,depth,z))
            return index[key]
        for r,c in cells:
            x=c-1;z=3-r
            # Shared top vertices make a continuous rock, with edges only at the outline.
            corners=[v(x,z,-.09),v(x+1,z,-.09),v(x+1,z+1,-.09),v(x,z+1,-.09)]
            center=v(x+.47,z+.54,-.24)
            for j in range(4):faces.append((corners[j],corners[(j+1)%4],center));slots.append(0 if j%2 else 1)
            for a,b,neighbor in [(0,1,(r+1,c)),(1,2,(r,c+1)),(2,3,(r-1,c)),(3,0,(r,c-1))]:
                if neighbor in cells:continue
                pa=verts[corners[a]];pb=verts[corners[b]]
                faces.append((corners[a],v(pa[0],pa[2],.18),v(pb[0],pb[2],.18),corners[b]));slots.append(1)
        ob=mesh('Board_'+name,verts,faces,stone);ob.data.materials.append(edge)
        for poly,slot in zip(ob.data.polygons,slots):poly.material_index=slot
        objects=[ob]
        for n,(r,c) in enumerate(cells):
            # Short, embedded chips and broad veins; no upright crystal pillars.
            x=c-.5;z=3.5-r
            chip=mesh('Vein_'+name+'_'+str(n),[(x-.29,-.255,z-.23),(x+.18,-.255,z-.32),(x+.32,-.255,z+.12),(x+.03,-.31,z+.32),(x-.23,-.255,z+.14)],[(0,1,3),(1,2,3),(3,4,0)],vein);objects.append(chip)
        export('Board_'+name,objects)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/'mining-prototype.blend'))
    print('Exported continuous board silhouettes matching the existing six recipe footprints')
