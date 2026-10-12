"""Original NR-7 plasma blaster. Run Blender -b --python this_file.py -- --project PATH.
Logical coordinates: X right, Y up, Z muzzle-forward, metres; converted to Blender.
No external assets, textures or add-ons. Separate named parts remain editable in .blend.
"""
import argparse, json, math, sys
from pathlib import Path
import bpy
from mathutils import Vector

args = argparse.ArgumentParser()
args.add_argument('--project', required=True)
opts = args.parse_args(sys.argv[sys.argv.index('--') + 1:])
root = Path(opts.project)
source = root / 'Tools/Blender'
preview = source / 'previews'
export = root / 'Assets/Art/Weapons/NeonBlaster.fbx'
preview.mkdir(parents=True, exist_ok=True)
export.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
bpy.context.scene.unit_settings.system = 'METRIC'
bpy.context.scene.unit_settings.scale_length = 1

def xyz(v): return (v[0], -v[2], v[1])
def material(name, color, metal, roughness, emission=0):
    m = bpy.data.materials.new(name)
    m.diffuse_color = (*color, 1)
    m.use_nodes = True
    p = m.node_tree.nodes.get('Principled BSDF')
    p.inputs['Base Color'].default_value = (*color, 1)
    p.inputs['Metallic'].default_value = metal
    p.inputs['Roughness'].default_value = roughness
    p.inputs['Emission Color'].default_value = (*color, 1)
    p.inputs['Emission Strength'].default_value = emission
    return m
mats = [material('NR_Body', (.018, .035, .063), .72, .32),
        material('NR_Alloy', (.21, .29, .38), .8, .26),
        material('NR_Cyan', (.025, .62, 1), .25, .25, 2),
        material('NR_Violet', (.32, .06, .7), .35, .28, .9)]
parts = []
def finish(obj, name, mat, bevel=0):
    obj.name = name
    obj.data.materials.append(mats[mat])
    if bevel:
        b = obj.modifiers.new('Machined edge bevel', 'BEVEL')
        b.width = bevel; b.segments = 1
        b.affect = 'EDGES'
        b = obj.modifiers.new('Weighted face normals', 'WEIGHTED_NORMAL')
        b.keep_sharp = True; b.weight = 40
    parts.append(obj)
    return obj

def prism(name, profile, width, mat=0, bevel=.0015, x=0):
    # Side profile is (forward Z, up Y). Closed, bevelled extrusion.
    verts = [xyz((x+s*width/2, y, z)) for s in (-1,1) for z,y in profile]
    n = len(profile)
    faces = [tuple(reversed(range(n))), tuple(range(n,2*n))]
    faces += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(verts, [], faces); mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
    bpy.ops.mesh.normals_make_consistent(inside=False); bpy.ops.object.mode_set(mode='OBJECT')
    obj.select_set(False)
    return finish(obj,name,mat,bevel)

def box(name, pos, size, mat=0, bevel=.001):
    bpy.ops.mesh.primitive_cube_add(size=1, location=xyz(pos))
    obj=bpy.context.object
    obj.scale=(size[0],size[2],size[1])
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.select_set(False)
    return finish(obj,name,mat,bevel)

def ring(name, z, outer, inner, depth, mat, n=24):
    verts=[]
    for zz,rr in [(z-depth/2,outer),(z+depth/2,outer),(z-depth/2,inner),(z+depth/2,inner)]:
        verts += [xyz((math.cos(i*2*math.pi/n)*rr, math.sin(i*2*math.pi/n)*rr, zz)) for i in range(n)]
    faces=[]
    for i in range(n):
        j=(i+1)%n
        faces += [(i,j,n+j,n+i),(2*n+j,2*n+i,3*n+i,3*n+j),
                  (n+i,n+j,3*n+j,3*n+i),(j,i,2*n+i,2*n+j)]
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    obj=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(obj)
    return finish(obj,name,mat,.0007)

# Continuous receiver: angular shoulders and tapered nose instead of stacked boxes.
prism('01 monocoque receiver', [(-.070,-.018),(-.070,.024),(-.054,.038),(.074,.033),(.129,.021),(.137,.009),(.124,-.022),(.024,-.027),(-.035,-.024)],.047)
prism('02 upper floating armor', [(-.057,.037),(-.045,.046),(.067,.043),(.096,.031),(.055,.033),(-.046,.032)],.038,1,.0012)
prism('03 rear chamfered spine',[(-.070,-.012),(-.074,.023),(-.062,.034),(-.058,-.013)],.036,1,.001)
# Raked grip: palm swell, inset side pads, separated heel energy cell.
grip=[(-.048,-.018),(-.006,-.024),(-.020,-.064),(-.030,-.108),(-.043,-.116),(-.070,-.110),(-.074,-.098),(-.055,-.046)]
prism('04 ergonomic grip',grip,.034,0,.002)
for s in (-1,1):
    prism('05 grip inset '+str(s),[(-.051,-.040),(-.019,-.035),(-.030,-.096),(-.040,-.106),(-.063,-.101)],.002,1,.0008,x=s*.0172)
    for i in range(4):
        z=-.041-i*.0035;y=-.054-i*.011
        prism('06 grip traction '+str(s)+' '+str(i),[(z-.014,y),(z+.012,y+.004),(z+.012,y+.006),(z-.014,y+.002)],.0018,0,.0003,x=s*.0187)
prism('07 removable heel cell',[(-.074,-.110),(-.069,-.123),(-.037,-.127),(-.028,-.114),(-.032,-.107)],.039,0,.0012)
for s in (-1,1): box('08 heel violet indicator '+str(s),(s*.020,-.116,-.050),(.001,.003,.016),3,.0004)
# Open trigger cage. Beams leave the trigger cavity empty.
prism('09 trigger guard forward',[(.037,-.025),(.045,-.028),(.032,-.064),(.024,-.064)],.014,1,.001)
prism('10 trigger guard lower',[(-.019,-.060),(.031,-.064),(.031,-.071),(-.022,-.066)],.014,1,.001)
prism('11 recessed trigger',[(-.008,-.027),(.001,-.028),(-.004,-.048),(-.011,-.048)],.009,0,.001)
# Staggered receiver side panels, exposed cyan energy paths, vents and fasteners.
for s in (-1,1):
    prism('12 side armor '+str(s),[(-.049,-.016),(-.054,.013),(-.042,.026),(.046,.023),(.064,.012),(.049,-.013),(.004,-.019)],.004,1,.001,x=s*.023)
    prism('13 recessed energy bed '+str(s),[(-.043,.008),(-.036,.017),(.050,.014),(.074,.002),(.067,-.004),(.042,.007)],.0025,0,.0007,x=s*.0257)
    prism('14 cyan zigzag conduit '+str(s),[(-.040,.009),(-.035,.013),(.048,.010),(.071,-.002),(.069,-.005),(.047,.006),(-.037,.009)],.0015,2,.0004,x=s*.0272)
    prism('15 nose cheek '+str(s),[(.072,-.018),(.086,.023),(.111,.018),(.125,.005),(.117,-.020)],.003,0,.001,x=s*.0227)
    for i in range(3):
        z=.083+i*.010
        prism('16 nose cooling slot '+str(s)+' '+str(i),[(z,-.013),(z+.004,-.012),(z+.009,.009),(z+.005,.010)],.0015,1,.0004,x=s*.0246)
    for z,y in [(-.042,-.007),(.040,-.007)]:
        box('17 flush panel lock '+str(s)+str(z),(s*.0258,y,z),(.0015,.004,.005),0,.0005)
    box('18 rear violet cell '+str(s),(s*.024,.020,-.057),(.002,.004,.011),3,.0006)
box('19 dorsal energy channel',(0,.044,.010),(.007,.002,.073),2,.0006)
# Hollow muzzle; rings have an actual bore, no invisible nested barrel faces.
ring('20 faceted muzzle shroud',.138,.024,.011,.028,0)
ring('21 machined muzzle rim',.155,.023,.010,.008,1)
ring('22 cyan muzzle aperture',.161,.017,.009,.004,2)
ring('23 internal dark bore lip',.157,.010,.008,.008,0)
# A small non-emitting sight keeps the top silhouette mechanical.
box('24 rear sight',(0,.050,-.043),(.018,.007,.010),0,.001)
box('25 front sight',(0,.029,.113),(.008,.006,.008),1,.0008)

# Source assembly retains meaningful part names and bevel modifiers.
assembly=bpy.data.collections.new('NR-7 editable parts')
bpy.context.scene.collection.children.link(assembly)
for obj in parts:
    for col in list(obj.users_collection): col.objects.unlink(obj)
    assembly.objects.link(obj)

# Evaluate bevels into a single four-submesh export; keep normals and source modifiers.
bpy.ops.object.select_all(action='DESELECT')
copies=[]
for obj in parts:
    dup=obj.copy(); dup.data=obj.data.copy(); bpy.context.collection.objects.link(dup)
    bpy.context.view_layer.objects.active=dup;dup.select_set(True)
    for modifier in list(dup.modifiers): bpy.ops.object.modifier_apply(modifier=modifier.name)
    dup.select_set(False);copies.append(dup)
for obj in copies: obj.select_set(True)
bpy.context.view_layer.objects.active=copies[0]
bpy.ops.object.join()
merged=bpy.context.object;merged.name='NeonBlasterMesh'
# Reset pivot to tracked hand origin without moving geometry.
bpy.context.scene.cursor.location=(0,0,0)
bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
# FBX/Unity handedness maps Blender -Y to Unity -Z with baked conversion.
# Rotate the export copy only so imported mesh coordinates are +Z forward.
merged.rotation_euler[2] = math.pi
bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
tri=merged.modifiers.new('Explicit game triangles','TRIANGULATE')
bpy.ops.object.modifier_apply(modifier=tri.name)
merged.data.calc_loop_triangles()
triangles=len(merged.data.loop_triangles)
assert triangles < 5000, triangles
bpy.ops.export_scene.fbx(filepath=str(export),use_selection=True,object_types={'MESH'},
    axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,
    apply_scale_options='FBX_SCALE_UNITS',bake_space_transform=True,
    use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,bake_anim=False)
bpy.data.objects.remove(merged,do_unlink=True)
stats={'triangles':triangles,'materials':len(mats),'length_metres':.237,
       'unity_muzzle_local':[0,0,.163],'unity_forward':'+Z','blender_forward':'-Y',
       'source_parts':len(parts),'blender_version':bpy.app.version_string}
(source/'NeonBlaster.stats.json').write_text(json.dumps(stats,indent=2)+'\n')
print('NEON_BLASTER_STATS '+json.dumps(stats))

# Studio preview only: camera/lights never enter the FBX or the Unity scene.
scene=bpy.context.scene
scene.render.engine='CYCLES';scene.cycles.samples=48
scene.cycles.use_denoising=True
scene.render.resolution_x=1400;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.world.color=(.12,.12,.12)
scene.view_settings.view_transform='AgX'
def aim(obj,point): obj.rotation_euler=(Vector(point)-obj.location).to_track_quat('-Z','Y').to_euler()
for name, pos, power, size, color in [
    ('Studio key',(.23,-.15,.32),4,.25,(.7,.85,1)),
    ('Studio rim',(-.18,.10,.16),5,.20,(.22,.5,1)),
    ('Studio fill',(.08,.27,.05),2.5,.18,(.8,.55,1))]:
    bpy.ops.object.light_add(type='AREA',location=pos)
    lamp=bpy.context.object;lamp.name=name;lamp.data.energy=power;lamp.data.shape='DISK';lamp.data.size=size;lamp.data.color=color
    aim(lamp,(0,-.03,-.02))
bpy.ops.object.camera_add(location=(.32,-.40,.22))
camera=bpy.context.object;aim(camera,(0,-.035,-.035));camera.data.type='ORTHO';camera.data.ortho_scale=.36
scene.camera=camera
scene.render.image_settings.file_format='PNG'
scene.render.filepath=str(preview/'NeonBlaster.png')
bpy.ops.wm.save_as_mainfile(filepath=str(source/'NeonBlaster.blend'))
bpy.ops.render.render(write_still=True)
