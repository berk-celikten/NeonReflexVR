# NeonReflexVR

## 🎮 [itch.io’dan indir ve oyna](https://berk-celikten.itch.io/neonreflexvr)

NeonReflexVR, Windows bilgisayarda çalışan 60 saniyelik bir PC VR hedef vurma oyunudur. Meta Quest 3, **Quest Link veya Air Link** üzerinden kullanılır. **Standalone Quest APK değildir.**

## Oyna
Windows bilgisayarda Meta Horizon Link kurulu ve etkin OpenXR runtime olarak seçili olmalıdır. Gözlükten bilgisayarın Quest Link/Air Link ana ekranına bağlanın.

1. **NeonReflexVR-Windows-PCVR-v1.0.0-rc1.zip** dosyasını tamamen çıkarın.
2. Çıkarılan klasördeki **NeonReflexVR.exe** uygulamasını çalıştırın.
3. EXE, NeonReflexVR_Data, UnityPlayer.dll, MonoBleedingEdge ve diğer runtime dosyalarını birlikte tutun. EXE'yi tek başına taşımayın; ZIP içinden doğrudan çalıştırmayın.
4. Gözlüğü takıp iki kontrolcüyü hazır edin. Ana menüde sağ **A** tuşuna basarak turu başlatın.

[Windows kurulum yönergesi](docs/WINDOWS_INSTALL.md) · [Release notları](RELEASE_NOTES.md)

## Kontroller ve amaç
- Sol veya sağ **trigger**: her basışta tek atış; iki el birlikte kullanılabilir.
- Sağ **A**: ana menüde başlat; GameOver'da yeni tur başlat.
- Tur **60 saniye** sürer. Bir hedefin ilk başarılı isabeti **1 puan** verir; aynı hedefe çift isabet ikinci puan vermez.
- Üç özgün Blender hedefi bulunur: **HexCore, Drone, Capsule**. Aynı anda en fazla üç hedef vardır.
- Hedefler başlangıç bakışının önündeki güvenli alanda rastgele oluşur. Vurulan hedef beyaz flash sonrası yenilenir; vurulmayan hedef yaklaşık 5 saniyede değişir.
- HUD skor/süreyi, GameOver ekranı son skoru gösterir. Süre dolunca hedefler temizlenir.
- Çift silah, lazer, ses ve başarılı isabette ilgili kontrolcüde hafif titreşim bulunur. Teleport veya smooth locomotion yoktur.

Oyun alanını rahat ve güvenli tutun. Headset tracking hazır değilken A ile tur başlamaz.

## Yayın adayı durumu
**v1.0.0-rc1** ilk Windows yayın adayıdır; henüz final v1.0.0 değildir.

Kullanıcı Windows-TargetVariants Development sürümünde üç hedefi, merkez/kenar isabetlerini, beyaz flash/yenilemeyi, HUD rakamlarını, iki silah/lazeri, ses/haptic, 60 saniye ve GameOver/restart'ı Quest 3 + Link ile doğruladı. Genel akıcılık ve görsel konfor kabul edildi.

Release Candidate'ın fiziksel cihaz testi bekliyor. 5–10 tur uzun kararlılık, gerçek recenter/tracking kaybı ve focus dönüşü ayrıca doğrulanmalıdır. Genel akıcılık onayı ayrıntılı CPU/GPU benchmark sonucu değildir.

Normal release'de otomatik performans raporu oluşturulmaz; Unity'nin normal Player.log mekanizması korunur.

## Geliştirme
Unity **6.3 LTS 6000.3.26f1**, C#, URP **17.3.0**, Windows x64 / Mono. XRI **3.3.2**, OpenXR **1.16.1**, XR Management **4.6.1**, Input System **1.20.0**, Test Framework **1.6.0** kullanılır.

Unity Hub üzerinden belirtilen sürümle açın; başlangıç sahnesi Assets/Scenes/GameScene.unity olmalıdır. Mevcut XR/OpenXR ve input ayarlarını koruyun. Development diagnostics yönergeleri [PERFORMANCE](docs/PERFORMANCE.md) içindedir; bunlar normal oyuncu başlangıç talimatı değildir.

Yeni görevde önce kısa [STATUS](docs/STATUS.md), sonra ilgili [ROADMAP](docs/ROADMAP.md) bölümünü okuyun. Kurallar [AGENTS.md](AGENTS.md). Blender silah/hedeflerinin özgün üretim kaynakları Tools/Blender/ altındadır.

## Lisanslar ve dağıtım
[PUBLIC kaynak repository](https://github.com/berk-celikten/NeonReflexVR). Build/ZIP çıktıları kaynak Git repository'sine commit edilmez.

Unity şablon ve resmî XRI input asset'inin telif/lisans bildirimleri ile Unity Companion License'ın tam metni [THIRD_PARTY_NOTICES](THIRD_PARTY_NOTICES.md) içindedir. Dağıtım ZIP'i ayrıca runtime paketlerinin bildirimlerini THIRD_PARTY_LICENSES/ altında taşır. Unity/XRI lisansları bütün özgün NeonReflexVR içeriğine atanmış değildir.

Blender silah ve hedefleri bu proje için üretilmiş özgün içeriktir. Özgün kod/içerik için yeni bir açık kaynak lisansı atanmadı; PUBLIC olması yeniden kullanım lisansı verildiği anlamına gelmez.
