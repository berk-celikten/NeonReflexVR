# NeonReflexVR — Durum

## Amaç ve teknoloji
- Windows PC VR: 60 saniyelik hedef vurma arcade oyunu; Quest 3 + Link/Air Link.
- Unity 6.3 LTS 6000.3.26f1; C#; URP 17.3.0; Windows x64 / Mono.
- XRI 3.3.2; OpenXR 1.16.1; XR Management 4.6.1; Input System 1.20.0.
- Test Framework 1.6.0; UGUI 2.0.0; CLI 1.0.0-beta.12; Pipeline 0.8.0-exp.1.
- PUBLIC https://github.com/berk-celikten/NeonReflexVR — main.

## Tamamlananlar
- Git/ignore/lisanslar; GameScene, SampleScene ve mevcut meta/GUID'ler korundu.
- Tek XR kamera/AudioListener; resmî HMD ve iki kontrolcü tracking/input.
- İki elle tek basışlık Raycast; ilk hedef isabetinde +1 skor; lazer/ses/doğru el haptics.
- Üç hedef, serbest ağırlıklı bölgeler; güvenli mesafe/yükseklik/aralık/LOS/retry.
- Sabit HMD tur anchor'ı; 0,15 s beyaz hit flash ve 5 s yenileme.
- Ready/A başlangıç; 60 s, GameOver/son skor/hedef temizliği; sağ A restart.
- Değer değişiminde World Space UI güncellemesi; dört cache'li özgün ses.
- Focus/pause'da sayaç ve hedef saatleri durur; odaksız start/restart/isabet engellenir.
- 7.1 Development diagnostics: FPS/p99, desteklenen FTM CPU/GPU/GC/XR; eksik veri unavailable.
- Editor FPS cihaz ölçümü değildir; [kullanım ve sınırlar](PERFORMANCE.md).
- Koyu sci-fi chamber, combat lane/altıgen detay; filtreli grid/mor/raylar ve kontrollü Quarter Bloom.
- 7.3A özgün NR-7 Blender silahı: 3.724 üçgen/dört ortak URP Lit materyali/23,7 cm.
- Kaynak Tools/Blender; FBX Assets/Art/Weapons/NeonBlaster.fbx; aynı collider'sız prefab iki elde.
- Silah namlusu yalnız görsel lazer başlangıcına bağlı; 0,06 m ray origin/20 m range ve isabet akışı korunur.
- Kullanıcı Windows-BlenderGun'da arena/silah/lazer/hedef/ses/haptic/oyun döngüsü ve genel performansı başarılı buldu.
- SCORE/TIME karakterlerinde aralıklı eksiklik/titreme bildirildi; HUD fiziksel onayı ayrı takip edilir.
- 7.3B: aynı düzlemdeki ZWrite panel/metin sorunu Editor GPU karşılaştırmasında yeniden üretildi.
- 2° bakışta eski düzende beyaz glyph pikselleri 17.370/19.075; ayrılmış düzende 19.086/19.086.
- Metin -8, panel +8 Canvas birimi: 0,0015 ölçekte 24 mm ayrım; çerçeve panelin 3 mm önünde.
- Font/Bold/64 rich-text boyutu, Canvas ölçüsü/alanı, UI metinleri ve tasarımı korundu; ZWrite kalır, ZTest LEqual açıkça belirtilir.
- Ortak UI kurulum otomasyonları aynı derinlik ayrımını korur; yeni runtime Update/mesh rebuild döngüsü eklenmedi.
- 7.3B'de GameManager/oynanış, Blender varlıkları, input/tracking, Bloom/MSAA/render çözünürlüğü/XR/Link ayarları değiştirilmedi.
- 7.3C: Blender 5.2.0 LTS CLI/bpy ile HexCore (3.320), Drone (3.372), Capsule (2.600 üçgen) özgün hedefleri; iki ortak materyal.
- Kaynak: Tools/Blender/create_target_variants.py ve NeonTargets.blend; FBX/prefab/materyaller: Assets/Art/Targets/.
- Preview'lar: Tools/Blender/previews/Target_*.png ve NeonTargets{,-Unity}.png; yeniden üretim: TargetVariants-README.md.
- Her model radius 0,15 m hitbox içinde; tek renderer/iki submesh, root Default layer/SphereCollider; karmaşık collider/animasyon/ışık/VFX yok.
- TargetSpawner yalnız opsiyonel üç prefab seçimi ve spawn anındaki sabit anchor yönünü ekler; konum/ağırlık/LOS/retry/üç slot korunur.
- Eski Target.prefab fallback olarak değişmeden durur; Target/TestTargetFeedback kodları, 5 s / 0,15 s ve skor/oyun akışı aynıdır.
- Yeni URP target shader'ı mevcut renderer-wide _BaseColor flash'ını iki submesh'te de saf beyaz gösterir; materyal kopyası yok.
- Silah/HUD/arena/skor/süre/start/restart/ses/haptic/diagnostics/XR/Link varlıkları korundu; HUD fix 02a8584 mevcut commit zincirinde tutuldu.

## Mevcut aşama ve sıradaki görev
- 7.3C model/CLI entegrasyon/test/Development build tamam; yeni hedefler için fiziksel Quest 3 onayı bekliyor.
- EXE: Builds/Windows-TargetVariants/NeonReflexVR.exe; güncel commit için git log -1.
- Windows-HUDFix ve Windows-BlenderGun korundu; önceki Perf/RailsFix/VisualOverhaul build'leri de silinmedi.
- Sıradaki: üç hedefin uzaktan seçilmesi, merkez/kenar isabetleri, beyaz flash/yenileme, Bloom/stereo konfor ve iki tur performans karşılaştırması.
- 7.3B HUD fiziksel onayı hâlâ bekliyor: iki göz/rakamlar/saniye-skor geçişleri/yavaş baş hareketi/Ready/GameOver/A.

## Doğrulama ve bilinen sınırlar
- 108/108 EditMode: önceki 94 + 14 target; referans/mesh/material/hitbox/raycast/tüm varyant seçimi/yenileme-pause/limit/anchor/fallback/GPU beyaz flash.
- Import/script compilation başarılı (0 compile hata/uyarı); son temiz Console 0 hata; TargetVariants build 0 hata / 20 bilinen uyarı.
- Uyarılar: URP array cache, beklenen engelli-spawn testi, Pipeline runtime config ve sample-cache.
- Yakın plan önce/sonra: Assets/_Recovery/HUD-{coplanar,digits}-2.png; 0–9 ve iki basamaklı rakamlar.
- HUD kamera/iki göz ve target preview/GPU flash kontrolleri offscreen Editor ölçümüdür; gerçek stereo/fiziksel cihaz başarısı değildir.
- 72 Hz kullanıcı Perf raporu: Player 40–60 FPS, CPU frame 16,7–25 ms/GPU 2,7–3,3 ms; spike >33,3 ms yok/dropped 0.
- İleri optimizasyon ertelendi; ayrıntılı p99/GC/overhead, 5–10 tur native focus/tracking/recenter kontrolleri bekliyor.
- Legacy font örnekleme aliasing'i tamamen ortadan kaldırıldığı iddia edilmez; cihaz testinde kalan sorun varsa ayrıca değerlendirilir.
- Builds/_Recovery/Library/Temp/.utmp yüklenmez; [ROADMAP](ROADMAP.md).
