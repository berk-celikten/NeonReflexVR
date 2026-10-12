# NeonReflexVR — Durum

## Amaç ve teknoloji
- Windows PC VR için 60 saniyelik hedef vurma arcade oyunu; Quest 3, Link/Air Link üzerinden kullanılır.
- Ana hedef kullanıcı kararıyla Android standalone'dan PC VR'ye değiştirildi (12 Ekim 2026).
- Unity 6.3 LTS `6000.3.26f1`; C#; URP `17.3.0`; Windows x64 / Mono.
- XRI `3.3.2`; OpenXR `1.16.1`; XR Plug-in Management `4.6.1`; Input System `1.20.0`.
- Test Framework `1.6.0`; Unity CLI `1.0.0-beta.12`; Pipeline `0.8.0-exp.1` (mevcut CLI paketi).
- PUBLIC repository: https://github.com/berk-celikten/NeonReflexVR — branch `main`.

## Tamamlananlar
- Git/GitHub, ignore kuralları ve üçüncü taraf lisans bildirimleri hazır.
- GameScene build listesinde ilk; SampleScene ve mevcut meta dosyaları korundu.
- XR Origin / Camera Offset / tek kamera + Audio Listener / iki kontrolcü hazır.
- TrackedPoseDriver ve resmî XRI Head/Left/Right action referansları bağlı.
- Zemin, üç renkli küp, basit kontrolcü görselleri ve URP Unlit materyaller hazır.
- Windows OpenXR Loader, başlangıçta XR başlatma, Oculus Touch ve Single Pass Instanced açık.
- Kullanıcı önceki PC VR build'inde VR görüntü, baş ve iki kontrolcü tracking'ini doğruladı.
- Aşama 3: iki kontrolcüde aynı VRRaycastShooter; Left/Right Interaction/Activate action'ları ayrı.
- Atış menzili 20 m; 0,08 s iki noktalı lazer; üç mevcut küpte 0,15 s beyaz isabet geri bildirimi.
- Materyal asset'leri değiştirilmez; property block geri yüklenir. Tracking/kamera/input asset korundu.
- Yeni paket, skor, zamanlayıcı, hedef üretimi ve locomotion eklenmedi.

## Mevcut aşama ve sıradaki görev
- Aşama 3: çift el uygulaması ve otomatik testler tamam; sol/eşzamanlı fiziksel test bekliyor.
- Önceki tek el ateş etme commit'i: `24e543a`; güncel commit için `git log -1`.
- Sıradaki: Link ile her iki trigger, aynı anda/ayrı atış, basılı tutma ve aynı kübe hızlı isabet testi.
- Baş/iki kontrolcü tracking regresyonunu da kontrol et; kullanıcı onayı olmadan fiziksel testi tamamlama.
- Sonrasında Aşama 4: hedef prefabı/üretimi ve isabet yaşam döngüsü.
- Yeni test EXE'si: `Builds/Windows-DualShooting/NeonReflexVR.exe`; Data/DLL dosyalarıyla birlikte kullan.
- Önceki Windows-Shooting build'i yalnızca sağ elle ateş eder; çift el testi için yeni EXE'yi kullan.

## Doğrulama ve bilinen sorunlar
- Çift el: 18/18 EditMode testi geçti; bağımsız/eşzamanlı input, tek atış, isabet ve renk geri yükleme.
- Script derlemesi başarılı; Console son kontrol: 0 hata / 26 uyarı. Windows build: 0 hata / 20 uyarı.
- Uyarılar: eski Android→Windows Editor oturumundaki URP dizi boyutları; Editor yeniden başlatılması önerilir.
- Diğer build uyarıları: Player'da CLI Pipeline runtime kapalı; tam Starter Assets sample klasörü yok.
- Kullanıcı sağ elle trigger, lazer Raycast ve küp renk geri bildirimini fiziksel olarak doğruladı.
- Sol el ve çift el eşzamanlı ateş etme/geri bildirim fiziksel olarak henüz doğrulanmadı.
- Göz yüksekliği/recenter/takip kaybından dönüş/uzun süreli kararlılık kontrolleri de bekliyor.
- Eski Android APK oluşturuldu fakat ADB cihaz göstermedi; yüklenmedi. Android artık ana hedef değil.
- Builds, `.utmp`, `Assets/_Recovery` Git'e yüklenmez; kurtarma yedekleri korunur.
- [ROADMAP](ROADMAP.md); STATUS yalnızca milestone sonunda güncellenir.