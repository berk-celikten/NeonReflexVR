"""Original sci-fi target set. Blender -b --python this_file.py -- --project PATH.
Logical coordinates: X right / Y up / Z depth, front faces -Z. Metres.
All geometry fits the existing radius-0.15 m target sphere. No external assets.
"""
import argparse, json, math, sys
from pathlib import Path
import bpy
from mathutils import Vector

p=argparse.ArgumentParser();p.add_argument('--project',required=True)
args=p.parse_args(sys.argv[sys.argv.index('--')+1:])
root=Path(args.project);source=root/'Tools/Blender';previews=source/'previews'
export=root/'Assets/Art/Targets';export.mkdir(parents=True,exist_ok=True);previews.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.context.scene.unit_settings.system='METRIC';bpy.context.scene.unit_settings.scale_length=1
bpy.context.preferences.filepaths.save_version=0

def xyz(v):return (v[0],-v[2],v[1])
def material(name,color,metal,rough,emission=0):
    m=bpy.data.materials.new(name);m.diffuse_color=(*color,1)
    p=m.node_tree.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value=(*color,1);p.inputs['Metallic'].default_value=metal
    p.inputs['Roughness'].default_value=rough
    p.inputs['Emission Color'].default_value=(*color,1);p.inputs['Emission Strength'].default_value=emission
    return m
mats=[material('Target_Armor',(.025,.055,.10),.68,.35),material('Target_Amber',(1,.23,.015),.2,.32,1.2)]
parts=[]
def finish(obj,name,mat,bevel=0):
    obj.name=name;obj.data.materials.append(mats[mat]);obj.select_set(False)
    if bevel:
        b=obj.modifiers.new('Machined bevel','BEVEL');b.width=bevel;b.segments=2
        n=obj.modifiers.new('Weighted normals','WEIGHTED_NORMAL');n.keep_sharp=True
    parts.append(obj);return obj

def prism(name,profile,depth,mat,z=0,bevel=.0015):
    n=len(profile);verts=[xyz((x,y,z+s*depth/2)) for s in (-1,1) for x,y in profile]
    faces=[tuple(reversed(range(n))),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active=obj;obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
    return finish(obj,name,mat,bevel)

def polygon(radius,n=6,rotation=math.pi/6):return [(math.cos(rotation+i*math.tau/n)*radius,math.sin(rotation+i*math.tau/n)*radius) for i in range(n)]
def plate(name,radius,depth,mat,z=0,n=24,bevel=.0015):return prism(name,polygon(radius,n),depth,mat,z,bevel)

def ring(name,outer,inner,depth,mat,z=0,n=24,bevel=.001):
    verts=[]
    for zz,r in [(z-depth/2,outer),(z+depth/2,outer),(z-depth/2,inner),(z+depth/2,inner)]:
        verts += [xyz((math.cos(i*math.tau/n)*r,math.sin(i*math.tau/n)*r,zz)) for i in range(n)]
    faces=[]
    for i in range(n):
        j=(i+1)%n;faces += [(i,j,n+j,n+i),(2*n+j,2*n+i,3*n+i,3*n+j),(n+i,n+j,3*n+j,3*n+i),(j,i,2*n+i,2*n+j)]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active=obj;obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
    return finish(obj,name,mat,bevel)

def box(name,pos,size,mat,bevel=.001):
    bpy.ops.mesh.primitive_cube_add(size=1,location=xyz(pos));obj=bpy.context.object
    obj.scale=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return finish(obj,name,mat,bevel)
def sphere(name,pos,scale,mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=12,radius=1,location=xyz(pos))
    obj=bpy.context.object;obj.scale=(scale[0],scale[2],scale[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    for face in obj.data.polygons:face.use_smooth=True
    return finish(obj,name,mat)

collections=[];stats=[]
def begin():parts.clear()
def complete(name,label):
    collection=bpy.data.collections.new(name+' editable parts');bpy.context.scene.collection.children.link(collection)
    for obj in parts:
        for col in list(obj.users_collection):col.objects.unlink(obj)
        collection.objects.link(obj)
    collections.append(collection)
    bpy.ops.object.select_all(action='DESELECT');copies=[]
    for obj in parts:
        dup=obj.copy();dup.data=obj.data.copy();bpy.context.collection.objects.link(dup)
        bpy.context.view_layer.objects.active=dup;dup.select_set(True)
        for m in list(dup.modifiers):bpy.ops.object.modifier_apply(modifier=m.name)
        dup.select_set(False);copies.append(dup)
    for obj in copies:obj.select_set(True)
    bpy.context.view_layer.objects.active=copies[0];bpy.ops.object.join();merged=bpy.context.object;merged.name=name
    bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    # Identical FBX handedness correction to the validated Blender blaster exporter.
    merged.rotation_euler[2]=math.pi;bpy.ops.object.transform_apply(location=False,rotation=True,scale=False)
    t=merged.modifiers.new('Game triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=t.name)
    merged.data.calc_loop_triangles();tri=len(merged.data.loop_triangles)
    radius=max(v.co.length for v in merged.data.vertices)
    assert tri<4000,(name,tri)
    assert radius<=.15,(name,radius)
    bpy.ops.export_scene.fbx(filepath=str(export/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',
        global_scale=1,apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_space_transform=True,
        use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,bake_anim=False)
    stats.append({'name':name,'design':label,'triangles':tri,'materials':2,'maximum_radius_m':round(radius,6),'editable_parts':len(parts)})
    bpy.data.objects.remove(merged,do_unlink=True)

begin()
plate('Hex reactor cage',.138,.038,0,n=6,bevel=.0025)
plate('Large amber hex strike face',.114,.007,1,z=-.024,n=6,bevel=.002)
plate('Rear amber energy plate',.102,.007,1,z=.024,n=6)
ring('Concentric dark aim bezel',.068,.053,.007,0,z=-.031,n=24)
plate('Center amber bullseye',.050,.010,1,z=-.035,n=24)
for i in range(6):
    a=math.pi/6+i*math.tau/6;c=math.cos(a);s=math.sin(a)
    obj=box('Armor spoke '+str(i),(c*.103,s*.103,-.030),(.030,.010,.008),0,.001)
    obj.rotation_euler[1]=-a
    plate('Center fastener '+str(i),.004,.003,0,z=-.029,n=8,bevel=.0004).location=xyz((c*.081,s*.081,0))
complete('Target_HexCore','Hexagonal energy reactor / clear circular strike center')

begin()
sphere('Amber reactor orb',(0,0,0),(.073,.073,.053),1)
ring('Dark iris surround',.078,.052,.035,0,z=-.028,n=24)
plate('Bright drone strike iris',.050,.008,1,z=-.052,n=24)
ring('Iris central aiming recess',.025,.019,.004,0,z=-.058,n=16,bevel=.0006)
plate('Amber aim center',.018,.004,1,z=-.060,n=16,bevel=.0005)
for sign in (-1,1):
    prism('Swept stabilizer '+str(sign),[(sign*.058,.022),(sign*.097,.046),(sign*.137,.018),(sign*.127,-.031),(sign*.093,-.047),(sign*.062,-.021)],.035,0,bevel=.002)
    prism('Amber wing strike surface '+str(sign),[(sign*.085,.022),(sign*.102,.027),(sign*.124,.010),(sign*.116,-.022),(sign*.094,-.028),(sign*.080,-.014)],.005,1,z=-.021,bevel=.001)
    prism('Rear wing energy '+str(sign),[(sign*.092,.016),(sign*.122,.004),(sign*.113,-.021),(sign*.093,-.024)],.004,1,z=.021,bevel=.001)
    for i in range(2):box('Wing cooling port '+str(sign)+' '+str(i),(sign*(.099+i*.010),-.010,-.025),(.004,.024,.003),0,.0005)
box('Upper amber status',(0,.082,.004),(.027,.010,.015),1,.001)
complete('Target_Drone','Compact hovering drone / amber orb and swept wings')

begin()
outline=[(-.073,-.099),(.073,-.099),(.094,-.078),(.094,.078),(.073,.099),(-.073,.099),(-.094,.078),(-.094,-.078)]
prism('Capsule angular housing',outline,.043,0,bevel=.0025)
face=[(-.061,-.083),(.061,-.083),(.077,-.067),(.077,.067),(.061,.083),(-.061,.083),(-.077,.067),(-.077,-.067)]
prism('Large amber panel',face,.007,1,z=-.027,bevel=.002)
prism('Rear amber panel',[(x*.86,y*.86) for x,y in face],.006,1,z=.026,bevel=.0015)
ring('Panel dark circular aim bezel',.056,.043,.009,0,z=-.034,n=24)
plate('Panel bullseye',.040,.008,1,z=-.041,n=24)
for sign in (-1,1):
    box('Top bottom armored rail '+str(sign),(0,sign*.070,-.033),(.088,.009,.006),0,.001)
    for side in (-1,1):
        box('Panel edge energy '+str(sign)+str(side),(side*.084,sign*.049,-.025),(.004,.030,.007),1,.0007)
        box('Capsule locking lug '+str(sign)+str(side),(side*.083,sign*.080,0),(.020,.025,.028),0,.0015)
complete('Target_Capsule','Armored neon strike capsule / large amber panel')

(source/'NeonTargets.stats.json').write_text(json.dumps({'blender_version':bpy.app.version_string,'collider_radius_m':.15,'models':stats},indent=2)+'\n')
print('TARGET_STATS '+json.dumps(stats))

# Studio images are separate from exported model geometry and game illumination.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=40;scene.cycles.use_denoising=True
scene.render.resolution_x=1100;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.world.color=(.025,.025,.035);scene.view_settings.view_transform='AgX'
def aim(obj,pos):obj.rotation_euler=(Vector(pos)-obj.location).to_track_quat('-Z','Y').to_euler()
for name,pos,power,color in [('Studio key',(.22,.32,.30),5,(.65,.8,1)),('Studio rim',(-.28,-.22,.12),7,(.18,.45,1)),('Studio fill',(.12,-.22,.18),2,(1,.55,.22))]:
    bpy.ops.object.light_add(type='AREA',location=pos);lamp=bpy.context.object;lamp.name=name;lamp.data.energy=power;lamp.data.size=.25;lamp.data.color=color;aim(lamp,(0,0,0))
bpy.ops.object.camera_add(location=(.19,.52,.17));camera=bpy.context.object;aim(camera,(0,0,0));camera.data.type='ORTHO';camera.data.ortho_scale=.36;scene.camera=camera
scene.render.image_settings.file_format='PNG'
for col in collections:col.hide_render=True
for col,stat in zip(collections,stats):
    col.hide_render=False;scene.render.filepath=str(previews/(stat['name']+'.png'));bpy.ops.render.render(write_still=True);col.hide_render=True
# Lay out source assemblies on a workbench for convenient editing; FBX pivots remain centered.
for i,col in enumerate(collections):
    col.hide_render=False
    for obj in col.objects:obj.location.x+=(i-1)*.36
camera.location=(.28,1.2,.38);aim(camera,(0,0,0));camera.data.ortho_scale=1.12
scene.render.resolution_x=1600;scene.render.resolution_y=800
scene.render.filepath=str(previews/'NeonTargets.png')
bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(source/'NeonTargets.blend'))
