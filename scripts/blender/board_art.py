"""Original local Blender art. No downloaded meshes, textures or paid services."""
import bpy, math, random, json
from pathlib import Path
from mathutils import Vector
from bpy_extras.object_utils import world_to_camera_view
ROOT=Path(r'C:/Users/kouha/Documents/ChatGPT/採掘系のゲーム作成？')
OUT=ROOT/'game/Assets/Resources/MiningPrototype'
LAYOUTS={'ring':[(1,0),(1,1)],'blade':[(0,0),(1,0),(2,0)],'shield':[(0,0),(0,1),(1,0),(1,1)],'axe':[(0,0),(0,1),(1,0),(1,1),(2,0),(3,0)],'robe':[(r,c) for r in range(4) for c in range(2)],'cross':[(0,0),(0,1),(1,0),(2,0)]}

def material(name,color,metal=.0,rough=.5,bump=False):
    m=bpy.data.materials.get(name) or bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True
    n=m.node_tree.nodes;n.clear();out=n.new('ShaderNodeOutputMaterial');p=n.new('ShaderNodeBsdfPrincipled');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough;m.node_tree.links.new(p.outputs[0],out.inputs[0])
    if bump:
        tex=n.new('ShaderNodeTexNoise');tex.inputs['Scale'].default_value=42;tex.inputs['Detail'].default_value=3
        b=n.new('ShaderNodeBump');b.inputs['Strength'].default_value=.28;b.inputs['Distance'].default_value=.018;m.node_tree.links.new(tex.outputs['Fac'],b.inputs['Height']);m.node_tree.links.new(b.outputs[0],p.inputs['Normal'])
        ramp=n.new('ShaderNodeValToRGB');ramp.color_ramp.elements[0].position=.25;ramp.color_ramp.elements[0].color=(*(v*.4 for v in color),1);ramp.color_ramp.elements[1].position=.8;ramp.color_ramp.elements[1].color=(*(v*1.25 for v in color),1);m.node_tree.links.new(tex.outputs['Fac'],ramp.inputs[0]);m.node_tree.links.new(ramp.outputs[0],p.inputs['Base Color'])
    if 'Mineral' in name:
        p.inputs['Coat Weight'].default_value=.4;p.inputs['Coat Roughness'].default_value=.12;p.inputs['Transmission Weight'].default_value=.16;p.inputs['IOR'].default_value=1.55
    return m

def setup_art():
    if bpy.data.scenes.get('BoardArt'):raise RuntimeError('BoardArt already exists; do not overwrite')
    s=bpy.data.scenes.new('BoardArt');bpy.context.window.scene=s
    s.render.engine='CYCLES';s.cycles.samples=48;s.cycles.use_denoising=True
    s.render.resolution_x=600;s.render.resolution_y=1200;s.render.resolution_percentage=100;s.render.film_transparent=True;s.render.image_settings.file_format='PNG';s.render.image_settings.color_mode='RGBA'
    s.view_settings.view_transform='AgX';s.view_settings.look='AgX - Medium High Contrast'
    s.world=bpy.data.worlds.new('BoardArt World');s.world.use_nodes=True;s.world.node_tree.nodes['Background'].inputs[0].default_value=(.24,.30,.38,1);s.world.node_tree.nodes['Background'].inputs[1].default_value=.35
    cam=bpy.data.objects.new('BoardArt Camera',bpy.data.cameras.new('BoardArt Camera'));s.collection.objects.link(cam);cam.location=(0,-9,2);cam.rotation_euler=(Vector((0,0,2))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.sensor_fit='VERTICAL';cam.data.ortho_scale=4.4;s.camera=cam
    for name,pos,power,color,size in [('Key',(-3,-4,6),500,(.73,.87,1),3),('Warm rim',(3,0,5),850,(1,.53,.2),2),('Soft fill',(2,-4,0),140,(.35,.8,1),4)]:
        ob=bpy.data.objects.new('BoardArt '+name,bpy.data.lights.new(name,'AREA'));s.collection.objects.link(ob);ob.location=pos;ob.rotation_euler=(Vector((0,0,2))-ob.location).to_track_quat('-Z','Y').to_euler();ob.data.energy=power;ob.data.color=color;ob.data.shape='DISK';ob.data.size=size
    material('MA_Slate',(.07,.085,.10),.2,.64,True);material('MA_Edge',(.03,.045,.06),.12,.7,True)
    material('MA_Mineral',(.008,.095,.14),.35,.20);material('MA_MineralLight',(.025,.26,.30),.3,.18);material('MA_Copper',(.36,.20,.095),.7,.36,True)
    (OUT/'BoardArt').mkdir(parents=True,exist_ok=True)

def make_mesh(name,verts,faces,mats,slots=None):
    data=bpy.data.meshes.new(name);data.from_pydata(verts,[],faces);data.update();ob=bpy.data.objects.new(name,data);bpy.context.scene.collection.objects.link(ob)
    for m in mats:ob.data.materials.append(bpy.data.materials[m])
    if slots:
        for p,m in zip(ob.data.polygons,slots):p.material_index=m
    return ob

def build_shape(name):
    s=bpy.data.scenes['BoardArt'];bpy.context.window.scene=s;rng=random.Random(917+len(name));cells=LAYOUTS[name]
    for ob in list(s.objects):
        if ob.name.startswith('Art_'+name+'_'):bpy.data.objects.remove(ob,do_unlink=True)
    vertices=[];faces=[];slots=[];corners={}
    def corner(x,z,front=True):
        key=(x,z,front)
        if key not in corners:
            jitter=random.Random(int((x+3)*99+(z+1)*17));dx=jitter.uniform(-.095,.095);dz=jitter.uniform(-.08,.08)
            corners[key]=len(vertices);vertices.append((x+dx,-.065 if front else .19,z+dz))
        return corners[key]
    for row,col in cells:
        x=col-1;z=3-row;c=[corner(x,z),corner(x+1,z),corner(x+1,z+1),corner(x,z+1)];center=len(vertices);vertices.append((x+.48,-rng.uniform(.10,.15),z+.5))
        for j in range(4):faces.append((c[j],c[(j+1)%4],center));slots.append(0)
        for a,b,nb in [(0,1,(row+1,col)),(1,2,(row,col+1)),(2,3,(row-1,col)),(3,0,(row,col-1))]:
            if nb in cells:continue
            ca=[(x,z),(x+1,z),(x+1,z+1),(x,z+1)]
            faces.append((c[a],corner(*ca[a],False),corner(*ca[b],False),c[b]));slots.append(1)
    base=make_mesh('Art_'+name+'_stone',vertices,faces,['MA_Slate','MA_Edge'],slots)
    bevel=base.modifiers.new('Chipped edge light','BEVEL');bevel.width=.026;bevel.segments=2
    objects=[base]
    for n,(row,col) in enumerate(cells):
        x=col-.5;z=3.5-row
        for j in range(3):
            angle=rng.uniform(-.45,.45);cx=x+(j-1)*.19;cz=z+(j-1)*.12
            radius=rng.uniform(.12,.23);length=rng.uniform(.25,.44)
            outline=[(-radius,-length*.75),(radius*.35,-length),(radius,length*.18),(radius*.3,length),(-radius*.7,length*.7)]
            v=[(cx+a*math.cos(angle)-b*math.sin(angle),-.18,cz+a*math.sin(angle)+b*math.cos(angle)) for a,b in outline];v.append((cx+.02,-rng.uniform(.24,.34),cz+.06))
            f=[(i,(i+1)%5,5) for i in range(5)];ob=make_mesh('Art_'+name+'_mineral_'+str(n)+'_'+str(j),v,f,['MA_Mineral','MA_MineralLight'],[0,1,0,0,1]);bevel=ob.modifiers.new('Mineral edge','BEVEL');bevel.width=.008;bevel.segments=2;objects.append(ob)
        # Copper inclusions connect the mineral pocket to surrounding stone.
        for j in range(18):
            cx=x+rng.uniform(-.42,.42);cz=z+rng.uniform(-.42,.42);w=rng.uniform(.015,.038)
            ob=make_mesh('Art_'+name+'_inclusion_'+str(n)+'_'+str(j),[(cx-w,-.155,cz-.08),(cx+w,-.155,cz-.04),(cx+w*.6,-.17,cz+.12)],[(0,1,2)],['MA_Copper']);objects.append(ob)
    for ob in s.objects:
        if ob.type=='MESH':ob.hide_render=ob not in objects
    bpy.ops.object.select_all(action='DESELECT')
    for ob in objects:ob.select_set(True)
    bpy.context.view_layer.objects.active=base
    bpy.ops.export_scene.fbx(filepath=str(OUT/'Models'/('Board_'+name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,add_leaf_bones=False,bake_anim=False)
    print('Built art model',name,len(objects))

def render_shape(name):
    s=bpy.data.scenes['BoardArt'];bpy.context.window.scene=s
    for ob in s.objects:
        if ob.type=='MESH':ob.hide_render=not ob.name.startswith('Art_'+name+'_')
    s.render.filepath=str(OUT/'BoardArt'/(name+'.png'));bpy.ops.render.render(write_still=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'art/source/mining-prototype/board-art.blend'))
    print('Rendered',name)

def render_tools():
    s=bpy.data.scenes['BoardArt'];bpy.context.window.scene=s
    material('MA_Steel',(.16,.20,.25),.8,.29,True);material('MA_Wood',(.16,.063,.018),.0,.56,True);material('MA_Leather',(.055,.025,.013),.0,.8,True)
    src=bpy.data.scenes['MiningPrototypeAssets'];objects=[]
    for name,material_name in [('Handle','MA_Wood'),('Grip','MA_Leather'),('PickHead','MA_Steel')]:
        ob=src.objects[name].copy();ob.data=src.objects[name].data.copy();s.collection.objects.link(ob);ob.name='ArtTool_'+name;ob.hide_set(False);ob.hide_render=False;ob.data.materials.clear();ob.data.materials.append(bpy.data.materials[material_name]);bevel=ob.modifiers.new('Worn edges','BEVEL');bevel.width=.012;bevel.segments=3;objects.append(ob)
    # Visible leather wrapping around the hand grip.
    for i in range(7):
        bpy.ops.mesh.primitive_torus_add(major_segments=12,minor_segments=4,location=(0,0,-.10+i*.043),major_radius=.057,minor_radius=.006)
        ob=bpy.context.object;ob.name='ArtTool_Wrap'+str(i);ob.data.materials.append(bpy.data.materials['MA_Wood']);objects.append(ob)
    tip=src.objects['StrikeTip'].copy();s.collection.objects.link(tip);tip.name='StrikeTip';objects.append(tip)
    bpy.ops.object.select_all(action='DESELECT')
    for ob in objects:ob.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    bpy.ops.export_scene.fbx(filepath=str(OUT/'Models/Pickaxe.fbx'),use_selection=True,object_types={'MESH','EMPTY'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,add_leaf_bones=False,bake_anim=False)
    for ob in s.objects:
        if ob.type=='MESH':ob.hide_render=ob not in objects
    cam=s.camera;cam.location=(0,-5,.38);cam.rotation_euler=(Vector((0,0,.38))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=1.22;s.render.resolution_x=s.render.resolution_y=512
    s.render.filepath=str(OUT/'BoardArt/pickaxe.png');bpy.ops.render.render(write_still=True)
    point=world_to_camera_view(s,cam,tip.matrix_world.translation);(OUT/'BoardArt/pick-pivot.json').write_text(json.dumps({'x':point.x,'y':point.y}),encoding='utf-8')
    for filename,source in [('crystal','Drop'),('chip','Chip')]:
        for ob in s.objects:
            if ob.type=='MESH':ob.hide_render=True
        ob=src.objects[source].copy();ob.data=src.objects[source].data.copy();s.collection.objects.link(ob);ob.name='ArtIcon_'+filename;ob.hide_set(False);ob.hide_render=False;ob.data.materials.clear();ob.data.materials.append(bpy.data.materials['MA_Mineral'] if filename=='crystal' else bpy.data.materials['MA_Slate'])
        target=Vector((0,0,.23 if filename=='crystal' else .08));cam.location=target+Vector((1,-3,1));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=.65 if filename=='crystal' else .25
        s.render.filepath=str(OUT/'BoardArt'/(filename+'.png'));bpy.ops.render.render(write_still=True)
    cam.location=(0,-9,2);cam.rotation_euler=(Vector((0,0,2))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=4.4;s.render.resolution_x=600;s.render.resolution_y=1200
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'art/source/mining-prototype/board-art.blend'))
    print('Detailed pickaxe, strike-pivot metadata, crystal and rock-chip images saved')
