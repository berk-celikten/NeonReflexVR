# NeonReflexVR — Durum

## Amaç ve teknoloji
- Windows PC VR için 60 saniyelik hedef vurma arcade oyunu; Quest 3 + Link/Air Link.
- Ana hedef kullanıcı kararıyla Android standalone'dan PC VR'ye değiştirildi (12 Ekim 2026).
- Unity 6.3 LTS `6000.3.26f1`; C#; URP `17.3.0`; Windows x64 / Mono.
- XRI `3.3.2`; OpenXR `1.16.1`; XR Plug-in Management `4.6.1`; Input System `1.20.0`.
- Test Framework `1.6.0`; Unity CLI `1.0.0-beta.12`; Pipeline `0.8.0-exp.1` (mevcut CLI paketi).
- PUBLIC repository: https://github.com/berk-celikten/NeonReflexVR — branch `main`.

## Tamamlananlar
- Git/GitHub, ignore kuralları ve üçüncü taraf lisans bildirimleri hazır.
- GameScene build listesinde ilk; SampleScene, XR Origin, kamera, tracking/input ve meta dosyaları korundu.
- Zemin, üç test kübü, iki kontrolcü görseli; URP Unlit materyaller hazır.
- Windows OpenXR Loader, başlangıçta XR başlatma, Oculus Touch ve Single Pass Instanced açık.
- İki elde VRRaycastShooter; ayrı Left/Right Interaction/Activate action ve lazerler; basış başına tek atış.
- Kullanıcı Aşama 3 çift el fiziksel testindeki 7 kontrolün tamamını Quest 3 + Link ile doğruladı.
- Aşama 4: `Assets/Prefabs/Targets/Target.prefab` — 0,3 m sphere, collider, Unlit ve beyaz flash.
- TargetSpawner üç slot kullanır; başlangıç bakışını ilk LateUpdate'te sabitler; baş dönüşüyle taşımaz.
- Hedefler göz başlangıcından 2,5–3,5 m; yatay ±25°; göz yüksekliğine göre -0,5/+0,1 m.
- En az 0,65 m hedef aralığı; küp/zemin çakışması ve görüş hattı engelleri reddedilir.
- İlk isabet tek kez kabul edilir; 0,15 s beyaz flash sonrası hedef değiştirilir. Vurulmazsa 5 s'de yenilenir.
- Silinen hedef pasifleştirilip yok edilir; üç aktif hedef aşılmaz; coroutine/event/pooling eklenmedi.
- Bölge tamamen engelliyse güvenli yer bulunana kadar sınırlı aralıkla yeniden denenir; uyarı bir kez verilir.
- Mevcut küp flash davranışı korunur; yeni paket, skor, oturum sayacı, menü, ses/partikül eklenmedi.

## Mevcut aşama ve sıradaki görev
- Aşama 4 kod/prefab/sahne ve otomatik doğrulamalar tamam; fiziksel hedef testi bekliyor.
- Önceki çift el commit'i: `9101d94`; en güncel commit için `git log -1`.
- Sıradaki: Link ile 3 hedef, iki el isabet, eşzamanlı tek kabul, flash ve 5 s yenileme testi.
- Baş döndürünce sabit bölgeyi, konforlu yükseklik/mesafeyi, birikme olmadığını ve tracking'i kontrol et.
- Sonrasında Aşama 5: 60 saniyelik oyun döngüsü ve skor.
- Test EXE: `Builds/Windows-Targets/NeonReflexVR.exe`; yanındaki Data/DLL dosyalarıyla kullan.

## Doğrulama ve bilinen sorunlar
- 25/25 EditMode testi geçti: sahne/tracking, çift el atış, sınırlar, yaşam döngüsü ve gerçek sahne çakışmaları.
- Script derlemesi başarılı; Console son kontrol: 0 hata / 56 uyarı. Windows build: 0 hata / 20 uyarı.
- Uyarılar önceki Android→Windows Editor oturumundaki URP dizi boyutları; Editor yeniden başlatılması önerilir.
- Diğer build uyarıları: Player'da CLI Pipeline runtime kapalı; tam Starter Assets sample klasörü yok.
- Aşama 4 gerçek Quest 3 hedef oynanışı henüz kullanıcı tarafından doğrulanmadı.
- Göz yüksekliği/recenter/takip kaybından dönüş/uzun süreli kararlılık ölçümleri ayrıca takip edilir.
- Eski Android APK oluşturuldu fakat yüklenmedi; Android artık ana hedef değil.
- Builds, `.utmp`, `Assets/_Recovery` Git'e yüklenmez; kurtarma yedekleri korunur.
- [ROADMAP](ROADMAP.md); STATUS yalnızca milestone sonunda güncellenir.