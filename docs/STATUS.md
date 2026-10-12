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
- GameScene build listesinde ilk; SampleScene, XR Origin, kamera, tracking/input ve mevcut meta dosyaları korundu.
- İki elde tek basışlık Raycast ve ayrı lazer; mevcut üç test kübü ve flash davranışı korunur.
- Kullanıcı Aşama 3 çift el testinin 7 kontrolünü ve Aşama 4 hedef oynanışını Quest 3 + Link ile doğruladı.
- Sphere Target prefabı: 0,3 m, collider, Unlit; üç hedef, ilk isabet sonrası 0,15 s flash ve yenileme.
- Vurulmayan hedef 5 s'de yenilenir; başlangıç bakışına sabit güvenli bölge, 2,5–3,5 m mesafe.
- Üç slot; engel/çakışma kontrolü; pooling/coroutine/event birikimi yok.
- Aşama 5: GameManager Ready → Playing → GameOver; ilk LateUpdate'te otomatik oyun başlangıcı.
- Playing: 60 s (Time.time), üç hedef ve yalnızca dinamik Target ilk isabeti için +1 puan.
- GameOver: süre 0, son skor sabit; spawner Stop ile tüm hedefler temizlenir; skor artmaz.
- Sağ A: mevcut resmî XRI Right Locomotion/Jump action referansı; yalnız GameOver'da yeni basışla restart.
- Restart sahne yüklemez; skor 0/süre 60/üç yeni hedef; iki shooter aynı manager'a bağlı.
- Sade World Space Canvas/Text: Score, Time, Game Over, Final Score, Press A to Restart.
- HUD başlangıç bakışının 2 m önünde ve gözden 0,55 m yukarıda; oturum boyunca sabit.
- Input asset, Target.cs, prefab, XR/tracking ve paketler değiştirilmedi; yeni paket/ses/neon/partikül yok.

## Mevcut aşama ve sıradaki görev
- Aşama 5 uygulaması ve otomatik doğrulamalar tamam; fiziksel tam tur/restart/UI testi bekliyor.
- Önceki hedef sistemi commit'i: `86c3b67`; güncel commit için `git log -1`.
- Sıradaki: 60 s tur, iki el skor, küplerin puan kazandırmaması, GameOver temizliği ve A ile restart testi.
- UI okunabilirliğini ve tekrar turlarda hedef birikmediğini kontrol et; onaysız fiziksel testi tamamlama.
- Sonrasında Aşama 6: VR sunumu/menü ve görsel-ses kullanılabilirlik.
- Test EXE: `Builds/Windows-GameLoop/NeonReflexVR.exe`; yanındaki Data/DLL dosyalarıyla kullan.

## Doğrulama ve bilinen sorunlar
- 36/36 EditMode testi geçti (önceki 25 + 11 oyun döngüsü/skor/restart/UI testi).
- Script derlemesi başarılı; Console son kontrol: 0 hata / 26 uyarı. Windows build: 0 hata / 20 uyarı.
- Uyarılar önceki Android→Windows oturumunun URP dizi boyutları; Editor yeniden başlatılması önerilir.
- Diğer build uyarıları: Player'da CLI Pipeline runtime kapalı; tam Starter Assets sample klasörü yok.
- Aşama 5 gerçek Quest 3 tur/restart/HUD okunabilirliği henüz kullanıcı tarafından doğrulanmadı.
- Göz yüksekliği/recenter/takip kaybından dönüş/uzun süreli kararlılık ölçümleri ayrıca takip edilir.
- Android artık ana hedef değil. Builds, `.utmp`, `Assets/_Recovery` Git'e yüklenmez.
- [ROADMAP](ROADMAP.md); STATUS yalnızca milestone sonunda güncellenir.