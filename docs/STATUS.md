# NeonReflexVR — Durum

## Amaç ve teknoloji
- Meta Quest 3 standalone Android için 60 saniyelik hedef vurma VR arcade oyunu.
- Unity 6.3 LTS `6000.3.26f1`; C#; URP `17.3.0`.
- XRI `3.3.2`; OpenXR `1.16.1`; XR Plug-in Management `4.6.1`.
- Input System `1.20.0`; Test Framework `1.6.0`.
- Unity CLI `1.0.0-beta.12`; Pipeline `0.8.0-exp.1` (mevcut deneysel CLI paketi).
- PUBLIC repository: https://github.com/berk-celikten/NeonReflexVR — branch `main`.

## Tamamlananlar
- Git/GitHub, Unity ignore kuralları ve Unity lisans bildirimleri hazır.
- Android Build Support, SDK/NDK ve OpenJDK doğrulandı.
- Android OpenXR, Meta Quest Support, Oculus Touch, IL2CPP/ARM64 hazır.
- Minimum API 32; Target API Automatic; Input System uyumlu; XR başlangıçta açık.
- GameScene build listesinde ilk; SampleScene ve mevcut varlıklar korundu.
- XR Origin / Camera Offset / tek Main Camera + Audio Listener kuruldu.
- HMD ve sol/sağ kontrolcü için TrackedPoseDriver ve resmî XRI action referansları bağlı.
- Zemin, üç renkli küp ve basit kontrolcü görselleri; URP Unlit materyaller hazır.

## Mevcut aşama ve sıradaki görev
- Aşama 2.1–2.3 tamamlandı: ilk VR sahnesi ve tracking yapılandırması hazır.
- Son işlevsel milestone: `8a66cffd07751e4b8082068eba0e0eef08d3c021`.
- Sıradaki görev: Aşama 2.4 — ilk APK ve Quest 3 cihaz doğrulaması.
- Önce Target API/SDK, Developer Mode ve cihaz bağlantısını doğrula; ardından build/test.
- Henüz silah, lazer, hedef, skor, 60 saniyelik oturum veya locomotion mekaniği yok.

## Son milestone doğrulaması ve bilinen sorunlar
- Script derlemesi başarılı; 7/7 EditMode testi geçti; Console 0 hata / 0 uyarı.
- Sahne yeniden açıldığında input referansları korundu; eksik script yok.
- XR Validation: 0 hata, 1 engelleyici olmayan SSAO uyarısı; PC renderer Android'de hariç, Mobile renderer'da SSAO yok.
- Editor mevcut URP asset'ini otomatik yükseltebilir; ilgisiz asset değişikliklerini diff'te kontrol et.
- APK build ve fiziksel Quest 3 testi henüz yapılmadı; gerçek kamera/kontrolcü tracking'i doğrulanmadı.
- Testler: `Assets/Tests/EditMode/GameSceneTests.cs`; kurulum: `Assets/Editor/Setup/GameSceneSetup.cs` (mevcut çıktıları ezmez).
- İlgili plan: [ROADMAP](ROADMAP.md). Bu belgeyi küçük işlemlerde değil, milestone sonunda güncelle.
