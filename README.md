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
- Planlanan XR altyapısı: OpenXR ve XR Interaction Toolkit. 12 Ekim 2026 incelemesinde bu iki paket manifest ve paket kilit dosyasında bulunmamaktadır; henüz kurulmamıştır.

Unity Pipeline, Unity CLI'nin Editor ile iletişimini sağlayan pakettir: [Unity belgeleri](https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package). Paketin projede bulunması, ayrı CLI aracının kurulmuş veya erişilebilir olduğunu tek başına doğrulamaz.

## Projeyi açma

Unity Hub üzerinden bu klasörü Unity `6000.3.26f1` ile açın. `Assets`, `Packages` ve `ProjectSettings` proje kökünde birlikte bulunmalıdır. Hedef cihaz derlemesi için Android araçlarının ve XR yapılandırmasının sonraki aşamada doğrulanması gerekmektedir.

## Mevcut aşama

İlk görev yalnızca altyapı incelemesi ve yerel Git hazırlığıdır. Oyun mekanikleri bu görev kapsamında geliştirilmemiştir. Paketler ve sahneler değiştirilmemiştir. Quest 3 cihaz testi veya Android oyuncu derlemesi yapılmamıştır.

Mevcut Editor.log içinde C# derleme hatası görülmemiş ve son script derlemesinin başarılı olduğu kaydedilmiştir. Logda `Native extension for Android target not found` mesajı vardır; Android derleme desteği ayrıca incelenmelidir. AssetImportWorker loglarında `-noUpm` nedeniyle Package Manager başlatılamadığı ve `Curl error 3` URL hatası kayıtları bulunmaktadır. Bunların mevcut ana Editor oturumunu engellediği doğrulanmamıştır.

## Git düzeni

`Assets`, varlıkların `.meta` dosyaları, `Packages/manifest.json`, `Packages/packages-lock.json` ve `ProjectSettings` sürüm kontrolünde korunmalıdır. `Library`, `Temp`, `Obj`, `Logs`, `Builds`, kullanıcı ayarları ve üretilen IDE dosyaları `.gitignore` ile hariç tutulur.

GitHub bağlantısı ve ilk push sonraki aşamada birlikte yapılacaktır. Geliştirme kuralları için `AGENTS.md` dosyasını okuyun.

## Üçüncü taraf lisans bildirimleri

Unity URP şablonundan gelen içeriklerin telif bildirimleri ve Unity Companion License'ın tam metni [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) dosyasındadır. Bu lisans ilgili Unity içeriği ve türevleri için geçerlidir; NeonReflexVR için geliştireceğimiz özgün oyun koduna ve projenin tamamına uygulanmaz. Özgün içerik için henüz proje genelinde bir açık kaynak lisansı seçilmemiştir. Paketler kendi lisans koşullarına tabidir.
