# NeonReflexVR — Durum

## Amaç ve teknoloji
- Windows PC VR için 60 saniyelik hedef vurma arcade oyunu; Quest 3 + Link/Air Link.
- Ana hedef Android standalone'dan PC VR'ye değiştirildi (12 Ekim 2026).
- Unity 6.3 LTS `6000.3.26f1`; C#; URP `17.3.0`; Windows x64 / Mono.
- XRI `3.3.2`; OpenXR `1.16.1`; XR Plug-in Management `4.6.1`; Input System `1.20.0`.
- Test Framework `1.6.0`; UGUI `2.0.0`; Unity CLI `1.0.0-beta.12`; Pipeline `0.8.0-exp.1`.
- PUBLIC repository: https://github.com/berk-celikten/NeonReflexVR — branch `main`.

## Tamamlananlar
- Git/GitHub, ignore kuralları ve üçüncü taraf lisans bildirimleri hazır.
- GameScene ilk build sahnesi; SampleScene, XR Origin, kamera, tracking/input ve meta dosyaları korundu.
- İki elde tek basışlık Raycast ve ayrı lazer; kullanıcı çift el ve Aşama 4 hedef testlerini doğruladı.
- Sphere Target: 0,3 m, collider, Unlit; ilk isabet sonrası 0,15 s flash/yenileme, vurulmazsa 5 s yenileme.
- Ready/Playing/GameOver; 60 s tur, dinamik hedef ilk isabetine +1 puan; oyun sonunda hedef temizliği.
- Sağ A ile GameOver'da restart: mevcut XRI Right Locomotion/Jump; skor 0/süre 60/üç yeni hedef.
- Sade World Space skor/süre/GameOver/Final Score/restart UI; tur boyunca sabit.
- Kullanıcı Aşama 5 temel oyun döngüsünü çalışır buldu; hedeflerin sola yığılması gözlemlendi.
- Spawn fix: sabit üç slot, her sol/orta/sağ bölgede bir hedef; yeniden üretim aynı slot bölgesinde.
- Açı sınırları: sol -25/-8,33°, orta -8,33/+8,33°, sağ +8,33/+25°.
- Mesafe 2,5–3,5 m; gözden -0,5/+0,1 m yükseklik; hedefler arası en az 0,65 m.
- Collider/görüş hattı kontrolleri korunur; 64 rastgele + 27 sınırlı fallback adayı; engelde 0,5 s retry.
- HMD isTracked, konum+rotasyon ve geçerli merkez göz pose'u beklenir; taze input tracking-space'ten dünyaya çevrilir.
- İlk tur ve restart valid HMD yönüne hizalanır; süre beklerken başlamaz, alan baş dönüşünü takip etmez.
- Eski üç renkli küp yalnız GameScene'den kaldırıldı; materyal/prefab ve diğer kaynak dosyaları silinmedi.
- Yeni paket/oyun modu/ses/partikül/pooling eklenmedi; atış, skor, süre, GameOver ve UI korundu.

## Mevcut aşama ve sıradaki görev
- Hedef dağılımı düzeltmesi kod/test/build olarak hazır; Quest 3 fiziksel onayı bekliyor.
- Önceki oyun döngüsü commit'i: `fc9b9c2`; güncel commit için `git log -1`.
- Sıradaki: farklı başlangıç yönleriyle sol/orta/sağ birer hedef, kendi bölgelerinde yenileme ve sabit alan testi.
- Skor, 60 s/GameOver/A restart, iki el ve HUD regresyonunu cihazda kontrol et.
- Yeni EXE: `Builds/Windows-SpawnFix/NeonReflexVR.exe`; Data/DLL dosyalarıyla kullan.
- `Builds/Windows-GameLoop` korunur; SpawnFix fiziksel onayından sonra eski build temizlenebilir.
- Sonrasında Aşama 6: VR sunumu/menü ve görsel-ses kullanılabilirlik.

## Doğrulama ve bilinen sorunlar
- 42/42 EditMode testi geçti; bölge dağılımı/yenileme, engelli retry, HMD hazır olma/hizalama ve önceki sistemler.
- Script derlemesi başarılı; Console: 0 hata / 26 uyarı. Windows SpawnFix build: 0 hata / 20 uyarı.
- Uyarılar önceki Android→Windows oturumunun URP dizi boyutları; Editor yeniden başlatılması önerilir.
- Diğer build uyarıları: Player'da CLI Pipeline runtime kapalı; tam Starter Assets sample klasörü yok.
- Yeni dağılım/hizalama, kapsamlı UI okunabilirliği ve takip/recenter/kararlılık ölçümleri fiziksel test bekliyor.
- Android artık ana hedef değil. Builds, `.utmp`, `Assets/_Recovery` Git'e yüklenmez.
- [ROADMAP](ROADMAP.md); STATUS yalnızca milestone sonunda güncellenir.