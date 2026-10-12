# Neon Targets — Blender kaynakları / Aşama 7.3C

Sıfırdan `bpy` ile üretilmiş üç özgün hedef. Harici asset/texture, gerçek ürün veya telifli oyun tasarımı kopyası ve yeni paket kullanılmaz. Blender 5.2.0 LTS background CLI ile doğrulandı.

| Varyant | Tasarım | Üçgen | Materyal | En uzak vertex |
|---|---|---:|---:|---:|
| Target_HexCore | Altıgen enerji kafesi ve dairesel isabet merkezi | 3.320 | 2 | 0,138335 m |
| Target_Drone | Amber reaktör/iris ve iki süpürülmüş stabilizer | 3.372 | 2 | 0,138465 m |
| Target_Capsule | Zırhlı neon panel/kapsül ve belirgin merkez | 2.600 | 2 | 0,131145 m |

Her mesh mevcut 0,15 m yarıçaplı hitbox'ın içinde kalır. İki ortak materyal bütün varyantlarda paylaşılır; tek mesh/renderer, iki submesh/çizim. Görsel boşluklar dahil hitbox önceki sphere kadar toleranslıdır; siluetle birebir mesh hitbox olduğu iddia edilmez.

## Dosyalar

- `Tools/Blender/create_target_variants.py`: yeniden üretilebilir kaynak scripti.
- `Tools/Blender/NeonTargets.blend`: 48 isimli parçanın bevel/normal modifier'larıyla düzenlenebilir kaynağı; üç assembly workbench üzerinde yan yana.
- `Tools/Blender/NeonTargets.stats.json`: üretim sayımları.
- `Tools/Blender/previews/Target_{HexCore,Drone,Capsule}.png`: bireysel Blender render'ları.
- `Tools/Blender/previews/NeonTargets.png`: Blender toplu önizleme.
- `Tools/Blender/previews/NeonTargets-Unity.png`: gerçek Unity shader/materyalleriyle offscreen Editor preview.
- `Assets/Art/Targets/Target_{HexCore,Drone,Capsule}.fbx`: tek mesh, merkez pivot, metre ölçeği.
- `Assets/Art/Targets/Materials/Target_{Armor,Amber}.mat`: iki ortak materyal.
- `Assets/Art/Targets/Prefabs/`: üç Target prefabı.
- `Assets/Shaders/TargetVariantSurface.shader`: URP opaque, tek pass, stereo/instancing uyumlu yüzey.
- `Assets/Editor/Setup/TargetVariantsSceneSetup.cs`: CLI import/prefab/sahne/preview otomasyonu.
- `Assets/Tests/EditMode/TargetVariantTests.cs`: 14 ek kontrol.

## Yeniden üretim

Proje kökünde PowerShell; önce sahneleri kaydedin ve Play Mode'u durdurun:

```powershell
& 'C:\Program Files\Blender Foundation\Blender 5.2\blender.exe' -b --python Tools/Blender/create_target_variants.py -- --project (Get-Location).Path
unity recompile --project-path (Get-Location).Path
unity command eval 'NeonReflexVR.Editor.TargetVariantsSceneSetup.Configure(); return "Complete";' 60000 --timeout 90
unity command eval 'NeonReflexVR.Editor.TargetVariantsSceneSetup.RenderPreview(); return "Preview complete";' 30000 --timeout 60
unity command run_tests --mode editor --async_tests true
unity command test_status
unity command build --target StandaloneWindows64 --outputPath Builds/Windows-TargetVariants/NeonReflexVR.exe --options '["Development","DetailedBuildReport"]' --scenes '["Assets/Scenes/GameScene.unity"]' --confirm true
unity command build_status
```

`eval` kodundan sonraki sayı C# eval için milisaniye timeout'udur; CLI `--timeout` saniyedir. Çoklu FBX importunda varsayılan 5 saniye yetmeyebilir. Preview, import/GPU upload tamamlandıktan sonra ayrı komutla çalıştırılır. Editor kapalıyken `Unity.exe -batchmode -quit -projectPath <proje> -executeMethod NeonReflexVR.Editor.TargetVariantsSceneSetup.Configure` kullanılabilir; aynı proje açıkken ikinci batchmode Editor açılmaz.

## Entegrasyon ve korunmuş davranış

`TargetSpawner.targetVariants` üç kalıcı prefab referansını tutar; her başarılı spawn'da rastgele bir görsel seçilir. Varyant dizisi boşsa veya seçilen slot null ise eski `Assets/Prefabs/Targets/Target.prefab` kullanılır. Bu orijinal prefab, materyali ve GUID'leri değiştirilmez/silinmez. Slotlar, konum seçimi/ağırlıkları/LOS/retry, 3 hedef sınırı ve gameplay saatleri aynı kalır. Boş varyant dizisinde eski seeded konum davranışı korunur.

Modelin yerel -Z yüzü tur başlangıcındaki sabit HMD anchor'ına yalnız spawn anında çevrilir; runtime billboard/animasyon yok. FBX scale 1, +Y yukarı, merkez pivot; Blender export kopyası blaster'da doğrulanan handedness dönüşümünü kullanır. Prefabın transform scale'i 1 ve SphereCollider radius'u 0,15 m: eski 0,3 scale × 0,5 radius ile aynı dünya hitbox'ı. Root layer Default (0), collider trigger değil. Mesh collider, child collider, Animator, Rigidbody, yeni ışık veya particle yok.

`Target` ve `TestTargetFeedback` değişmez. Root MeshRenderer'ın renderer-wide `_BaseColor` property block'u iki submesh'e de uygulanır. Shader tam beyaz override'ı saf beyaz verir; materyal kopyası oluşturulmaz. Shader energy yüzeyleri amber/turuncu ve okunaklı; armor lacivert yardımcı yüzeydir. Mevcut Bloom ayarı değiştirilmez.

HUD 7.3B düzeltmesi, Blender silahları, arena, skor/süre/start/restart/ses/haptic/diagnostics/XR/Link ayarları korunur. 108/108 EditMode: önceki 94 + 14 hedef testi; mesh/material/hitbox, raycast/tekrar isabet, bütün varyantların seçilmesi, 5 s / 0,15 s / pause kaydırması, üç slot, anchor yönü, fallback ve GPU beyaz flash.

## Fiziksel cihaz onayı bekliyor

Quest 3 + Link'te üç siluetin uzaktan ayırt edilmesi, merkez ve kenar isabetleri, beyaz flash/yenileme, amber-Bloom/stereo konfor, üç hedef/skor/iki el/ses/haptic/60 s/restart ve HUD rakamlarının korunması kontrol edilmelidir. Aynı Link çözünürlüğü/72 Hz ile iki tur FPS/CPU/GPU karşılaştırması yapın. Editor preview ve GPU testleri gerçek stereo cihaz onayı değildir. Windows-HUDFix, Windows-BlenderGun ve eski build'ler silinmez; uygulama kullanıcı onayı olmadan cihazda başlatılmaz.
