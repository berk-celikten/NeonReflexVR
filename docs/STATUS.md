# NeonReflexVR — Durum

## Amaç ve teknoloji
- Windows PC VR: 60 saniyelik hedef vurma arcade oyunu; Quest 3 + Link/Air Link.
- Unity 6.3 LTS 6000.3.26f1; C#; URP 17.3.0; Windows x64 / Mono.
- XRI 3.3.2; OpenXR 1.16.1; XR Management 4.6.1; Input System 1.20.0.
- Test Framework 1.6.0; UGUI 2.0.0; CLI 1.0.0-beta.12; Pipeline 0.8.0-exp.1.
- PUBLIC https://github.com/berk-celikten/NeonReflexVR — main.

## Tamamlananlar
- Git/ignore/lisanslar, GameScene; SampleScene ve mevcut meta/GUID'ler korundu.
- XR Origin/tek kamera/Audio Listener; resmî HMD ve iki kontrolcü input/tracking bağlantıları.
- İki elle tek basışlık Raycast/lazer; ilk dinamik hedef isabetinde +1 skor.
- Üç hedef; serbest ağırlıklı bölge seçimi, güvenli mesafe/yükseklik/aralık/LOS/retry.
- Tur başında geçerli HMD yönüne sabit anchor; 0,15 s beyaz isabet ve 5 s yenileme.
- Ready/A başlangıç, 60 s, GameOver/temizlik/final skor ve sağ A restart.
- Değer değişiminde güncellenen World Space UI; dört özgün cache'li ses ve doğru el haptics.
- Aşama 4/5, 6.1 UI ve 6.2 ses/haptic kullanıcı tarafından Quest 3 + Link'te doğrulandı.
- 6.3 RailsFix cyan grid/mor/dış ray, renk/fade/baş hareketi ve tüm oynanış fiziksel olarak doğrulandı.
- 7.1 Development tanılama: Player FPS/p99, geçerli FrameTiming CPU/GPU, Recorder GC/Main Thread ve XR sayaçları.
- Desteksiz metrikler unavailable; Editor FPS cihaz ölçümü değildir; [kullanım/sınırlar](PERFORMANCE.md).
- Focus/pause'da sayaç ve hedef saatleri durur; odaksız start/restart/isabet engellenir.
- Kullanıcı Windows-Perf ile iki tur/72 Hz HUD ölçümü yaptı; ileri optimizasyon şimdilik öncelik değil.
- 7.3: koyu lacivert kapalı chamber, birleşik duvar/tavan portalları, combat lane ve uzak altıgen detaylar.
- Beş yeni ortak dekor mesh/renderer; collider, runtime script, ek ışık veya yeni paket yok.
- Turuncu sphere'ın shader'ı yenilendi; aynı hitbox ve beyaz flash; iki lazerde çekirdek/yumuşak glow.
- Aynı UI metinleri/akışı; kalın metin, köşe/amber çerçeve ve dekorun metne taşmasını önleyen sade opak panel.
- Doğrulanmış grid/mor/rail shader/materyalleri ve Quarter Bloom aynen korundu.
- 7.3 görsel aşamasında oynanış/input/tracking/ses/haptic/focus ve diagnostics C# kodları değiştirilmedi.
- 7.3A — Blender 5.2.0 LTS CLI/bpy ile özgün NR-7 lazer tabancası: 3.724 üçgen, dört ortak URP/Lit materyali, 23,7 cm.
- Kaynak: Tools/Blender/create_neon_blaster.py ve NeonBlaster.blend; FBX: Assets/Art/Weapons/NeonBlaster.fbx.
- Blender/Unity preview'ları ve yeniden üretim: Tools/Blender/previews/ ve Tools/Blender/README.md.
- Aynı collider'sız Ignore Raycast prefabı iki tracked controller altında; +Z ileri, ölçek 1, namlu (0,0,0.163) m.
- VRRaycastShooter yalnız visualMuzzle çizgi başlangıcını kullanır; eski 0,06 m ray origin/20 m range/isabet/score/ses/haptic korunur.
- Namlu öncesi engelde raycast devam eder, ters lazer çizgisi oluşmaz; URP doğrulamasında emissive keyword korunur.

## Mevcut aşama ve sıradaki görev
- 7.3A silah modeli/CLI entegrasyonu/test/Windows Development build tamam; Quest 3 fiziksel onayı bekliyor.
- EXE: Builds/Windows-BlenderGun/NeonReflexVR.exe; güncel commit için git log -1.
- Windows-Perf, Windows-RailsFix ve Windows-VisualOverhaul korundu; eski build'ler silinmedi.
- Sıradaki: iki el kabza/hizalama/tracking, namlu-lazer doğrultusu, stereo konfor/Bloom ve HUD/hedef görünürlüğü.
- Çift trigger/skor/ses/haptic/60 s/GameOver/A restart ve aynı Link/72 Hz ayarlarıyla iki tur performans karşılaştırması kullanıcı testi bekliyor.
- 7.3 arena görsellerinin önceki fiziksel onay bekleme durumu da devam ediyor.

## Doğrulama ve bilinen sınırlar
- 89/89 EditMode geçti: önceki 73 + 9 görsel + 7 silah testi; çift el/prefab/transform/katman/collider/URP emission ve yakın engel regresyonu.
- Import/derleme başarılı (0 compile hata/uyarı); Console 0 hata; Windows-BlenderGun build 0 hata / 20 bilinen uyarı.
- Uyarılar: URP array cache, beklenen engelli-spawn test kaydı, Pipeline runtime config ve sample-cache.
- Ready/Playing/GameOver ile lazer/beyaz flash Editor önizlemesi yapıldı; gerçek VR görsel onayı değildir.
- Kullanıcının Perf raporu: Player 40–60 FPS, CPU frame 16,7–25 ms, GPU 2,7–3,3 ms; >33,3 ms spike yok, XR dropped 0.
- Main Thread/CPU frame beklemeleri içerebilir; bu sonuç tek başına CPU darboğazı kanıtı değil.
- 5–10 tur, native tracking kaybı/recenter, focus geri dönüşü ve ölçüm overhead kontrolleri hâlâ bekliyor.
- Yeni arena ve silahların gerçek stereo konforu, kabza ergonomisi, aliasing/Bloom ve FPS/CPU/GPU etkisi kullanıcı testi bekliyor.
- Builds/_Recovery/Library/Temp/.utmp yüklenmez; [ROADMAP](ROADMAP.md).
