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
- GameManager/oynanış, Blender varlıkları, input/tracking, Bloom/MSAA/render çözünürlüğü/XR/Link ayarları değiştirilmedi.

## Mevcut aşama ve sıradaki görev
- 7.3B uygulama/otomatik test/Development build tamam; HUD fiziksel Quest 3 onayı bekliyor.
- EXE: Builds/Windows-HUDFix/NeonReflexVR.exe; güncel commit için git log -1.
- Windows-BlenderGun korundu; önceki Perf/RailsFix/VisualOverhaul build'leri de silinmedi.
- Sıradaki: 0–9/10/59/60, skor/saniye değişimi, iki göz ve yavaş baş hareketi; Ready/GameOver/A akışı.

## Doğrulama ve bilinen sınırlar
- 94/94 EditMode: mevcut 89 + 5 HUD; glyph atlası/alan/clipping/derinlik/GPU örtülme/durum geçişleri.
- Derleme başarılı; Console 0 hata / 38 bilinen uyarı; HUDFix build 0 hata / 20 bilinen uyarı.
- Uyarılar: URP array cache, beklenen engelli-spawn testi, Pipeline runtime config ve sample-cache.
- Yakın plan önce/sonra: Assets/_Recovery/HUD-{coplanar,digits}-2.png; 0–9 ve iki basamaklı rakamlar.
- Kamera/iki göz offset simülasyonu offscreen Editor ölçümüdür; gerçek stereo/fiziksel titreme başarısı değildir.
- 72 Hz kullanıcı Perf raporu: Player 40–60 FPS, CPU frame 16,7–25 ms/GPU 2,7–3,3 ms; spike >33,3 ms yok/dropped 0.
- İleri optimizasyon ertelendi; ayrıntılı p99/GC/overhead, 5–10 tur native focus/tracking/recenter kontrolleri bekliyor.
- Legacy font örnekleme aliasing'i tamamen ortadan kaldırıldığı iddia edilmez; cihaz testinde kalan sorun varsa ayrıca değerlendirilir.
- Builds/_Recovery/Library/Temp/.utmp yüklenmez; [ROADMAP](ROADMAP.md).
