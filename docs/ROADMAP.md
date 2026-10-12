# NeonReflexVR — 8 aşamalı yol haritası

Yalnızca doğrulanmış işler `[x]` olarak işaretlenir. Sahne kurulumu, cihaz testi ve yayın ayrı milestone'lardır. Güncel başlangıç noktası: [STATUS](STATUS.md).

Ana hedef Windows PC VR / Quest Link-Air Link'tir. Android APK/ADB dağıtımı tarihsel olarak bırakılmıştır; önceki doğrulanmış altyapı maddeleri korunur.

## 1. Proje ve XR altyapısı
- [x] Unity sürümü, proje yapısı ve CLI bağlantısını doğrula.
- [x] Git ignore kuralları, PUBLIC GitHub repository ve lisans bildirimlerini hazırla.
- [x] Android Build Support, SDK/NDK ve OpenJDK kurulumunu doğrula.
- [x] Kararlı, UPM tarafından doğrulanan XR paketlerini kur ve bağımlılıkları çöz.
- [x] Android OpenXR, Meta Quest Support, Oculus Touch, IL2CPP/ARM64 ve Input System ayarlarını doğrula.

## 2. İlk VR sahnesi ve cihaz doğrulaması
- [x] 2.1 — GameScene oluştur; SampleScene'i koru; build listesine ekle.
- [x] 2.2 — XR Origin, tek VR kamera/Audio Listener ve kontrolcü tracking referanslarını kur.
- [x] 2.3 — Resmî input actions, zemin, üç küp ve basit kontrolcü görsellerini ekle.
- [x] Sahneyi yeniden açıp referansları doğrula; 7 EditMode testini geçir; Console'u kontrol et.
- [x] 2.4 — Windows x64 OpenXR Loader, Oculus Touch ve aktif Meta Horizon Link runtime'ını doğrula.
- [x] GameScene-only Windows Development build oluştur ve PC uygulamasını başlat.
- [x] Kullanıcı ile fiziksel gözlükte Link üzerinden VR görüntü ve baş hareketini doğrula.
- [x] Kullanıcı ile sol/sağ kontrolcü tracking'ini doğrula.
- [ ] Göz yüksekliği ve recenter davranışını doğrula.
- [ ] Takip kaybı ve yeniden takip davranışını doğrula.
- [ ] Uygulama odağı ve başlangıç performansını cihazda kontrol et.

## 3. Kontrolcü girdisi ve ateş etme
- [ ] Kullanılacak el/kontrolcü ve tetik input davranışını belirle.
- [ ] Basit silah/görsel ve tek atış mekanizmasını oluştur.
- [ ] İsabet sorgusu, atış geri bildirimi ve atış aralığını uygula.
- [ ] Input ve atış davranışını test et; Quest 3 / Link üzerinde doğrula.

## 4. Hedefler ve isabet sistemi
- [ ] Hedef prefabı, görünüm ve güvenli konumlandırma alanını oluştur.
- [ ] Hedef üretimi, vurulma ve yeniden üretim davranışını uygula.
- [ ] İsabet geri bildirimi ve nesne yaşam döngüsünü test et.
- [ ] Hedef okunabilirliğini ve erişilebilirliğini cihazda doğrula.

## 5. 60 saniyelik oyun döngüsü
- [ ] Başlat, oynanıyor ve sonuç durumlarını oluştur.
- [ ] 60 saniyelik sayaç, skor ve isabet sayımını uygula.
- [ ] Oturum bitişi, yeniden başlatma ve sıfırlamayı test et.
- [ ] Quest 3 / Link üzerinde tam oturum oynanışını doğrula.

## 6. Arcade sunumu ve kullanılabilirlik
- [ ] VR menü, sayaç/skor göstergesi ve sonuç ekranını oluştur.
- [ ] Neon görsel stil, ses ve uygun haptik geri bildirimi ekle.
- [ ] Okunabilirlik, ses dengesi, kullanıcı konforu ve sınırları doğrula.
- [ ] Odağı kaybetme, uygulamaya geri dönme ve yeniden oynama akışlarını test et.

## 7. Performans ve yayın adayı
- [ ] Windows PC ve Link oturumunda CPU/GPU, kare hızı ve bellek ölçümü yap.
- [ ] Ölçülen darboğazları gider; renderer, materyal ve hedef üretimini optimize et.
- [ ] Oturum, input ve cihaz regresyon testlerini tamamla.
- [ ] Son asset/lisans/gizli bilgi denetimi ve Windows release build doğrulamasını yap.

## 8. GitHub ve itch.io yayını
- [ ] Sürüm numarası, release notları ve kurulum/oynama yönergelerini hazırla.
- [ ] Ekran görüntüleri ve itch.io açıklamasını hazırla.
- [ ] PUBLIC GitHub release/tag ve indirilebilir Windows build paketini yayınla.
- [ ] itch.io sayfasını yayınla; indirme ve temiz Windows kurulumu akışını doğrula.
