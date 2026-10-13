# NeonReflexVR — Durum

## Amaç ve teknoloji
- Windows PC VR: 60 saniyelik arcade hedef vurma; Quest 3 + Quest Link/Air Link. Standalone APK değildir.
- Unity 6000.3.26f1 / URP 17.3.0 / Windows x64 / Mono; release candidate 1.0.0-rc1.
- XRI 3.3.2; OpenXR 1.16.1; XR Management 4.6.1; Input System 1.20.0.
- Test Framework 1.6.0; UGUI 2.0.0; CLI 1.0.0-beta.12; Pipeline 0.8.0-exp.1.
- PUBLIC https://github.com/berk-celikten/NeonReflexVR — main; commit için git log -1.

## Tamamlananlar
- Tek XR kamera/AudioListener; resmî input/tracking; iki Blender silahı/trigger/Raycast/lazer.
- Ready/sağ A, 60 s/skor, GameOver/temizlik/sağ A restart; ses ve doğru el haptics.
- Üç hedef; serbest ağırlıklı bölgeler/LOS/mesafe/height/retry; sabit HMD tur anchor'ı.
- HexCore/Drone/Capsule Blender hedefleri, basit hitbox; 0,15 s beyaz flash/5 s yenileme.
- Özgün model kaynakları Tools/Blender; varlıklar Assets/Art/Weapons ve Assets/Art/Targets.
- HUD rakamları: panel/metin 24 mm ayrımı; font/Bold/64/tasarım korunur; 02a8584 commit'i mevcut.
- Hedef entegrasyonu 915d6a4 commit'i mevcut; mevcut meta/GUID/SampleScene korunur.
- Koyu sci-fi arena; filtreli cyan grid/mor/raylar ve kontrollü Quarter Bloom.
- Focus/pause saat durdurma; başlangıç/restart geçerli HMD ister.
- 7.1 Development diagnostics kaynakta kalır; release otomatik FPS/CPU/GPU/log toplamaz.
- Kullanıcı Windows-TargetVariants'ta üç model/görünürlük/merkez-kenar isabet/flash/yenilemeyi doğruladı.
- Aynı fiziksel testte HUD rakamları, iki silah/lazer, ses/haptic, 60 s/GameOver/restart başarılı.
- Görsel konfor/genel akıcılık kabul edildi; bu onay ayrıntılı benchmark veya uzun kararlılık testi değildir.
- 7.4 release: Development/Script Debugging/Autoconnect Profiler/Deep Profiling OFF; GameScene tek build sahnesi.
- Compiler path remap yalnız release süresince; DLL kişisel PDB yolları kaldırıldı, orijinal compiler argümanları geri alındı.
- Oynanış/sahne/modeller/input/XR/backend değişmedi; kalıcı PlayerSettings değişikliği yalnız rc1 sürüm numarası.
- 10 doğrulanmış eski perf logu (27.254 byte) silindi; Diagnostics ana dizini/Player.log/uygulama verileri korundu.
- Taşınabilir ZIP: 227 dosya; runtime/Data/DLL/Mono/D3D12/OpenXR, kurulum/notlar ve tam Unity/XRI bildirimleri.
- 20 runtime paketinin lisans/bildirimleri ve tam Apache metni pakette; özgün içeriğe yeni açık kaynak lisansı atanmadı.

## Mevcut aşama ve sıradaki görev
- 7.4 Windows release candidate hazır; fiziksel Quest 3 release onayı bekleniyor.
- EXE: Builds/Windows-ReleaseCandidate/NeonReflexVR.exe.
- ZIP: Builds/Packages/NeonReflexVR-Windows-PCVR-v1.0.0-rc1.zip; yaklaşık 40,1 MiB; yanındaki SHA256.
- Tüm eski Development build'ler korundu: TargetVariants/HUDFix/BlenderGun/Perf/RailsFix/VisualOverhaul.
- README, RELEASE_NOTES, docs/WINDOWS_INSTALL güncel; yerel yeniden üretim Tools/Release/README.md.
- Henüz GitHub Release/itch.io/tag/final v1.0.0 yayını yapılmadı.
- Sıradaki: ZIP'ten temiz kurulumla Quest 3 release testi; ardından 5–10 tur/focus/tracking/recenter onayı.

## Doğrulama ve sınırlar
- 108/108 son EditMode regresyon geçti; script compilation başarılı; Console 0 hata / 2 bilinen uyarı.
- Son release build 0 hata / 2 bilinen uyarı: Pipeline runtime config yokluğu ve sample-cache.
- ZIP yeni klasöre çıkarıldı; her dosya SHA256 ile kaynakla karşılaştırıldı; debug/source dosyaları hariç.
- ZIP girdilerinde token/anahtar/kişisel yol eşleşmesi yok; .pdb/DoNotShip geliştirme çıktısı pakete alınmadı.
- Çıkarılmış release 35 s responsive; Mono/engine/sahne/OpenXR sağlayıcıları yüklendi; eksik bağımlılık exception yok.
- Smoke boyunca ve kapanışta perf dosyası/[PERF] kaydı 0; normal Unity loglama korunur.
- Smoke temel PC açılışıdır; fiziksel stereo/oynanış/recenter/tracking veya uzun performans başarısı değildir.
- 72 Hz eski Perf raporu Player 40–60 FPS; CPU frame 16,7–25 ms/GPU 2,7–3,3 ms; beklemeler dahil olabilir.
- İleri optimizasyon ertelendi; p99/GC/overhead ve 5–10 tur native focus/tracking/recenter hâlâ bekliyor.
- Builds/_Recovery/Library/Temp/.utmp yüklenmez; [ROADMAP](ROADMAP.md), [PERFORMANCE](PERFORMANCE.md).
