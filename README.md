# NeonReflexVR

Meta Quest 3 üzerinde bağımsız (standalone) çalışması hedeflenen küçük bir VR arcade oyunu. Oyuncunun 60 saniye içinde hedefleri vurduğu bir oyun döngüsü geliştirilmesi planlanmaktadır. Tamamlanan projenin kaynak kodu GitHub'da, oynanabilir sürümü itch.io'da yayınlanacaktır.

## Teknolojiler ve hedef platform

- Unity 6.3 LTS: `6000.3.26f1` (ProjectSettings/ProjectVersion.txt).
- C# ve Universal Render Pipeline (URP): `17.3.0`.
- Input System: `1.20.0`.
- Unity Test Framework: `1.6.0`.
- Unity Pipeline: `com.unity.pipeline` sürümü `0.8.0-exp.1` (deneysel).
- Yerel Unity CLI: `1.0.0-beta.12` (`unity --version` ile doğrulandı).
- Hedef cihaz: Meta Quest 3; hedef platform: Android standalone VR.
- XR Plug-in Management: `4.6.1`; OpenXR Plugin: `1.16.1`; XR Interaction Toolkit: `3.3.2`. Sürümler Unity 6000.3.26f1 Package Manager recommended ve compatible bilgileriyle doğrulanmıştır.

Unity Pipeline, Unity CLI'nin Editor ile iletişimini sağlayan pakettir: [Unity belgeleri](https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package). Paketin projede bulunması, ayrı CLI aracının kurulmuş veya erişilebilir olduğunu tek başına doğrulamaz.

## Projeyi açma

Unity Hub üzerinden bu klasörü Unity `6000.3.26f1` ile açın. `Assets`, `Packages` ve `ProjectSettings` proje kökünde birlikte bulunmalıdır. Hedef cihaz derlemesi için Android araçlarının ve XR yapılandırmasının sonraki aşamada doğrulanması gerekmektedir.

## Mevcut aşama

Quest 3 standalone için Android XR altyapısı ve ilk VR test sahnesi GameScene hazırlanmıştır. Yeni oyun mekaniği eklenmemiştir. Mevcut SampleScene ve varlıklar korunmuştur. Quest 3 cihaz testi veya Android oyuncu derlemesi yapılmamıştır.

Mevcut Editor.log içinde C# derleme hatası görülmemiş ve son script derlemesinin başarılı olduğu kaydedilmiştir. Logda `Native extension for Android target not found` mesajı vardır; Android derleme desteği ayrıca incelenmelidir. AssetImportWorker loglarında `-noUpm` nedeniyle Package Manager başlatılamadığı ve `Curl error 3` URL hatası kayıtları bulunmaktadır. Bunların mevcut ana Editor oturumunu engellediği doğrulanmamıştır.

## Git düzeni

`Assets`, varlıkların `.meta` dosyaları, `Packages/manifest.json`, `Packages/packages-lock.json` ve `ProjectSettings` sürüm kontrolünde korunmalıdır. `Library`, `Temp`, `Obj`, `Logs`, `Builds`, kullanıcı ayarları ve üretilen IDE dosyaları `.gitignore` ile hariç tutulur.

Repository [GitHub üzerinde PUBLIC](https://github.com/berk-celikten/NeonReflexVR) olarak yayınlanmaktadır. Geliştirme kuralları için `AGENTS.md` dosyasını okuyun.

## Üçüncü taraf lisans bildirimleri

Unity URP şablonundan gelen içeriklerin telif bildirimleri ve Unity Companion License'ın tam metni [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) dosyasındadır. Bu lisans ilgili Unity içeriği ve türevleri için geçerlidir; NeonReflexVR için geliştireceğimiz özgün oyun koduna ve projenin tamamına uygulanmaz. Özgün içerik için henüz proje genelinde bir açık kaynak lisansı seçilmemiştir. Paketler kendi lisans koşullarına tabidir.

## Quest 3 XR yapılandırması — Aşama 1.6

- Android Build Support, SDK/NDK Tools ve OpenJDK kurulumu Unity CLI ile doğrulandı. Aktif build hedefi Android.
- Android'de OpenXR Loader ve başlangıçta XR başlatma etkin.
- Meta Quest Support açık; hedef cihaz Quest 3 (`eureka`). Oculus Touch Controller Profile açık.
- Scripting backend IL2CPP, mimari ARM64, minimum Android API 32; Target API Level Unity'nin Automatic ayarında. [Meta Android manifest gereksinimleri](https://developers.meta.com/vr/documentation/native/android/mobile-native-manifest/).
- Input System etkin; OpenXR için InputSystem.XR.PoseControl ve StickControl thumbstick tanımları eklendi. Latency Optimization, Prioritize Input Polling olarak ayarlandı; Run In Background açık.
- OpenXR render modu Single Pass Instanced / Android Multiview. Android yalnızca Mobile kalite seviyesini ve `Mobile_RPAsset` / `Mobile_Renderer` yolunu kullanır.
- UPM tarafından çözümlenen dolaylı XR bağımlılıkları: XR Core Utilities `2.6.0`, XR Legacy Input Helpers `2.1.13`. Meta XR All-in-One SDK, ek Meta OpenXR paketi ve deneysel XR paketi eklenmedi; paket sample'ları içe aktarılmadı.
- Script derlemesi başarılı, güncel Console kontrolünde 0 hata ve 0 uyarı. XR Project Validation'da 0 hata ve 1 engelleyici olmayan SSAO performans uyarısı var. Bu uyarı PC renderer içindeki SSAO'dan kaynaklanır; PC kalite seviyesi Android'de hariç tutulur ve Mobile renderer'da SSAO bulunmaz. Mevcut render varlıkları değiştirilmemiştir.
- Android hedef geçişindeki geçmiş CLI kaydında bir ana iş parçacığı zaman aşımı; shader içe aktarımında 18 D3D11 hassasiyet uyarısı vardır. Bunlar güncel Console/derleme hatası değildir; geçiş ve derleme sonrasında kontrol edilmiştir.

APK build ve cihazda kontrolcü/performans testi sonraki görevlerin kapsamındadır. İlk APK aşamasında otomatik Target API seçimi ve gerekli SDK platformu ayrıca doğrulanmalıdır.

## İlk VR sahnesi — Aşama 2.1–2.3

`Assets/Scenes/GameScene.unity`, build scene listesinde ilk sıradadır; mevcut SampleScene listede korunur.

- `XR Origin (VR)` altında `Camera Offset`, tek bir `Main Camera`, `Left Controller` ve `Right Controller` bulunur. Origin başlangıcı sıfır konum/rotasyondur; Floor tracking istenir ve desteklenmeyen cihazlar için 1,6 m kamera yüksekliği tanımlıdır.
- Kamera ve iki kontrolcü modern Input System `TrackedPoseDriver` kullanır: Position, Rotation ve Tracking State referansları resmî XRI Head / Left / Right action map'lerine bağlıdır. Güncelleme modu Update And Before Render'dır. Tek Audio Listener kameradadır.
- Resmî XRI 3.3.2 Starter Assets input action dosyası ve `.meta` dosyası `Assets/Input/` altına değiştirilmeden alınmıştır. `InputActionManager`, action asset'ini etkinleştirir. Telif bildirimi `THIRD_PARTY_NOTICES.md` içindedir.
- Kontrolcüler için iki renkli basit kutu model; ortamda düz zemin, kırmızı/yeşil/mavi küpler ve gölgesiz yönlü ışık vardır. Materyaller URP Unlit kullanır.
- Locomotion, teleport, silah, lazer, ateş etme ve hedef vurma davranışı yoktur. HMD/kontrolcü pose tracking yapılandırması fiziksel cihazda henüz doğrulanmamıştır.

Editor otomasyonu `Assets/Editor/Setup/GameSceneSetup.cs` altındadır. Menü: `NeonReflexVR > Create Initial GameScene`. Mevcut çıktılar varsa üzerine yazmayı reddeder. EditMode testleri `Assets/Tests/EditMode/GameSceneTests.cs` altında kamera/hiyerarşi, kalıcı input referansları, kontrolcü görselleri, materyaller, eksik scriptler, build listesi ve locomotion bulunmamasını kontrol eder.

Quest 3 üzerinde sonraki kontrolde göz yüksekliği ve recenter, baş hareketi, sol/sağ kontrolcü konum/rotasyonu, takip kaybı/toparlanması, uygulama odağı, renklerin/stereo görüntünün doğruluğu ve performans doğrulanmalıdır.
