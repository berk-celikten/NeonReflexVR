# NeonReflexVR — Durum

## Amaç ve teknoloji
- Windows PC VR için 60 saniyelik hedef vurma arcade oyunu; Quest 3, Link/Air Link üzerinden kullanılır.
- Kullanıcı kararıyla ana hedef Android standalone'dan PC VR'ye değiştirildi (12 Ekim 2026).
- Unity 6.3 LTS `6000.3.26f1`; C#; URP `17.3.0`; Windows x64 / Mono.
- XRI `3.3.2`; OpenXR `1.16.1`; XR Plug-in Management `4.6.1`; Input System `1.20.0`.
- Test Framework `1.6.0`; Unity CLI `1.0.0-beta.12`; Pipeline `0.8.0-exp.1` (mevcut deneysel CLI paketi).
- PUBLIC repository: https://github.com/berk-celikten/NeonReflexVR — branch `main`.

## Tamamlananlar
- Git/GitHub, ignore kuralları ve üçüncü taraf lisans bildirimleri hazır.
- GameScene build listesinde ilk; SampleScene ve sahne meta dosyaları korundu.
- XR Origin / Camera Offset / tek kamera + Audio Listener / iki kontrolcü hazır.
- TrackedPoseDriver ve resmî XRI Head/Left/Right action referansları bağlı.
- Zemin, üç renkli küp ve kontrolcü görselleri; URP Unlit materyaller hazır.
- Windows OpenXR Loader, XR başlangıçta başlatma, Oculus Touch ve Single Pass Instanced açık.
- Bilgisayarda aktif OpenXR runtime: Meta Horizon Link; sahne/oyun kodu değiştirilmedi.
- GameScene-only Windows Development build başarılı; EXE başlatıldı; OpenXR oturumu FOCUSED.
- Kullanıcı gözlükte VR görüntü, baş ve iki kontrolcü tracking'inin çalıştığını doğruladı.

## Mevcut aşama ve sıradaki görev
- Aşama 2.4: PC VR build ve temel gerçek gözlük testi tamamlandı; kalan fiziksel kontroller bekliyor.
- Son sahne kurulumu commit'i: `8a66cffd07751e4b8082068eba0e0eef08d3c021`; en güncel Git değişikliği için `git log -1`.
- Sıradaki: göz yüksekliği, recenter, takip kaybından dönüş ve kararlılık/performance testi.
- Sonrasında Aşama 3: tetik girdisi ve ateş etme. Henüz oyun mekaniği/locomotion yok.
- EXE: `Builds/Windows/NeonReflexVR.exe`; uygulamayı aynı klasördeki Data/DLL dosyalarıyla birlikte kullan.

## Doğrulama ve bilinen sorunlar
- Windows build: 0 hata / 20 uyarı; Windows XR Project Validation: 0 sorun.
- URP dizi boyutu uyarıları Android→Windows geçişindeki Editor oturumuna ait; Editor yeniden başlatılması öneriliyor.
- Diğer build uyarıları: Player'da CLI Pipeline runtime kapalı; tam Starter Assets sample klasörü yok. Tracking testi çalıştı.
- Önceki sahne milestone'unda 7/7 EditMode testi geçti; sahne değiştirilmediği için tekrar koşulmadı.
- Göz yüksekliği/recenter/takip kaybından dönüş/uzun süreli kararlılık henüz kullanıcı tarafından doğrulanmadı.
- Eski Android Development APK başarılı oluşturuldu (min API 32, target 36, debug imza); ADB cihaz göstermedi, yüklenmedi.
- Android ayarları tarihsel olarak korunur; APK/ADB kurulumu artık ana geliştirme hedefi değildir.
- Builds, `.utmp` ve `Assets/_Recovery` yerelde tutulur, Git'e yüklenmez. Kurtarma yedekleri korunmuştur.
- [ROADMAP](ROADMAP.md); STATUS yalnızca milestone sonunda güncellenir.
