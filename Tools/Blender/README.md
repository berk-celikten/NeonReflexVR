# NR-7 Neon Blaster — özgün Blender kaynağı

NeonReflexVR Aşama 7.3A için sıfırdan, Blender `bpy` ile oluşturulmuş kompakt bilimkurgu lazer tabancası. Harici model, texture, marka tasarımı, ücretli asset veya yeni Unity paketi kullanılmaz.

- Blender 5.2.0 LTS; tek FBX mesh'i, **3.724 üçgen / 4 materyal**, 23,7 cm uzunluk.
- Düzenlenebilir `.blend`: 47 isimli parça, bevel ve weighted normal modifier'ları; preview kamera/ışıkları yalnız Blender kaynağında.
- Lacivert metal gövde, alaşım panel/vent/namlu halkası, cyan enerji yolları ve az mor hücre vurgusu.
- Tek mesh dört submesh içerir: bir renderer, el başına dört materyal çizimi; tek draw call olduğu varsayılmaz.

## Dosyalar

- `Tools/Blender/create_neon_blaster.py`: yeniden üretim scripti.
- `Tools/Blender/NeonBlaster.blend`: düzenlenebilir kaynak.
- `Tools/Blender/NeonBlaster.stats.json`: üretim sayımları.
- `Tools/Blender/previews/NeonBlaster.png`: Blender studio render'ı.
- `Tools/Blender/previews/NeonBlaster-Unity.png`: Unity URP materyalleriyle Editor preview; stereo/cihaz görüntüsü değildir.
- `Assets/Art/Weapons/NeonBlaster.fbx`: Unity modeli.
- `Assets/Art/Weapons/Materials/`: dört kalıcı, ortak URP/Lit materyali.
- `Assets/Art/Weapons/Prefabs/NeonBlaster.prefab`: collider ve runtime script içermeyen ortak prefab.
- `Assets/Editor/Setup/BlenderGunSceneSetup.cs`: import/prefab/sahne/build CLI girişleri.

## Yeniden üretim

Proje kökünde PowerShell:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' -b --python Tools/Blender/create_neon_blaster.py -- --project (Get-Location).Path
```

Unity açıkken mevcut Unity CLI:

```powershell
unity recompile --project-path (Get-Location).Path
unity command eval 'NeonReflexVR.Editor.BlenderGunSceneSetup.Configure(); return "Complete";' --timeout 90
unity command eval 'NeonReflexVR.Editor.BlenderGunSceneSetup.RenderPreview(); return "Preview complete";' --timeout 60
unity command run_tests --mode editor --async_tests true
unity command test_status
unity command build --target StandaloneWindows64 --outputPath Builds/Windows-BlenderGun/NeonReflexVR.exe --options '["Development","DetailedBuildReport"]' --scenes '["Assets/Scenes/GameScene.unity"]' --confirm true
unity command build_status
```

Editor kapalıyken `Unity.exe -batchmode -quit -projectPath <proje> -executeMethod NeonReflexVR.Editor.BlenderGunSceneSetup.Configure` desteklenir. Aynı proje açıkken ikinci batchmode süreci çalıştırılmaz. `BuildWindows` ayrı Development build girişidir. Önce sahneleri kaydedin ve Play Mode'u durdurun.

## Eksen ve lazer bağlantısı

Blender kaynak assembly'sinde -Y ileri / Z yukarı; export kopyası Unity FBX handedness dönüşümü için 180° Z dönüşünü bake eder. Unity importer `bakeAxisConversion=true`, ölçek 1, animasyon/kamera/ışık/collider importu kapalı. İçe aktarılan mesh +Z ileri / +Y yukarıdır; pivot kontrolcü kökünde. Her elin prefab local position/rotation değerleri sıfır, scale 1; namlu `(0, 0, 0.163)` metredir. Fiziksel kabza rahatlığı onaylanmadı; cihaz testinden sonra yalnız görsel offset gerekebilir.

`VRRaycastShooter` raycast başlangıcı kontrolcü + 0,06 m, menzil 20 m, mevcut yön/hit/skor/ses/haptic akışı aynıdır. `visualMuzzle` sadece lazerin ilk çizgi noktasını değiştirir; referans yoksa önceki davranış korunur. Namlu öncesindeki bir engel raycast'e hâlâ takılır; çizgi geri çizilmez, hit noktasında sıfır uzunluğa daralır. Tüm prefab nesneleri Ignore Raycast (2), collider yok. İki el aynı mesh ve dört materyali paylaşır; tracking kökleri ve input bağlantıları değişmez.

URP materyalleri Blender materyal isimlerine göre ayrıca oluşturulur ve bağlanır; otomatik FBX materyal eşdeğerliği varsayılmaz. Cyan emission maksimum 1,6, mor 0,56; mevcut Quarter Bloom ayarı değiştirilmez. Yeni runtime animasyonu, fizik, kamera veya ışık eklenmez. `PerformanceDiagnostics` yalnız Development Player davranışını korur; Release otomatik tanılaması açılmaz.

## Fiziksel onay bekliyor

Quest 3 + Link'te iki el kabza hizası/tracking, ileri namlu ve lazer çıkışı, yakın mesafe/parlama/stereo konforu, HUD/hedef görünürlüğü, çift trigger/score/ses/haptic/60 s/restart ve iki tur FPS/CPU/GPU karşılaştırması kullanıcı tarafından doğrulanmalıdır. Otomatik testler bu onayın yerine geçmez. Windows-Perf ve Windows-RailsFix korunur.
