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
- [x] Sağ kontrolcü ve resmî XRI Right Interaction/Activate trigger action bağlantısını kur.
- [x] Mevcut kontrolcü görselini koru; her trigger basışında tek Raycast atışı uygula.
- [x] Üç mevcut kübe kısa isabet geri bildirimi ve düşük maliyetli atış lazeri ekle.
- [x] Input, Raycast, engel, materyal koruma ve sahne referanslarını EditMode ile doğrula (18/18).
- [x] Kullanıcıyla Quest 3 / Link üzerinde sağ trigger, lazer ve küp renk geri bildirimini doğrula.
- [x] Aynı shooter ile sol Activate action'ını ve ayrı lazeri bağla; çift el input/isabet testlerini geçir.
- [x] Kullanıcı Aşama 3 çift el fiziksel testinin 7 kontrolünü Quest 3 / Link ile doğruladı.

## 4. Hedefler ve isabet sistemi
- [x] Sphere Target prefabı, Unlit görünüm ve başlangıç bakışına sabit güvenli bölgeyi oluştur.
- [x] Üç hedef; tek isabet kabulü, beyaz flash sonrası değiştirme ve 5 s yenilemeyi uygula.
- [x] Sayı/sınırlar/çakışmalar/isabet/eşzamanlı atış/yenileme ve temizliği test et; 25/25 EditMode.
- [x] Kullanıcı Aşama 4 rastgele hedef oynanışını Quest 3 + Link üzerinde doğruladı.
- [x] Bölge kilidini kaldır; doluluk/yakın kullanım ağırlıklı rastgele seçim ve farklı bölge tercihi uygula.
- [x] Geçerli HMD başlangıç/restart pose hizalaması; eski küpleri yalnız GameScene'den kaldır.
- [x] Serbest dağılım/denge, güvenli yenileme/retry, tracking ve oyun regresyon testlerini geçir; 43/43 EditMode.
- [x] Kullanıcı Windows-RandomFix bölge geçişi/denge, üç hedef sınırı ve sabit alanı Quest 3'te doğruladı.

## 5. 60 saniyelik oyun döngüsü
- [x] Ready/Playing/GameOver durumlarını oluştur; 6.1'de otomatik başlangıcı A ile menü başlangıcına dönüştür.
- [x] 60 saniye ve Target ilk isabeti başına +1 puan; test küpleri/eşzamanlı tekrarlar puansız.
- [x] Oturum sonu hedef temizliği, A ile restart ve değerlerin sıfırlanmasını test et; 36/36 EditMode.
- [x] Sade World Space skor/süre/GameOver/Final Score ve restart yönergesini ekle.
- [x] Kullanıcı Aşama 5 temel oyun döngüsünü Quest 3 / Link üzerinde çalışır buldu.
- [x] Kullanıcı RandomFix çift el/skor, 60 saniye, GameOver ve sağ A restart'ı doğruladı.

## 6. Arcade sunumu ve kullanılabilirlik
- [x] 6.1 — Ready menüsü, yalnız SCORE/TIME HUD ve GAME OVER/FINAL SCORE ekranını oluştur.
- [x] A ile başlat/yeniden oyna, HMD kontrolü, sade arcade UI ve 45 EditMode testini tamamla.
- [x] 6.1 — Kullanıcı Windows-UI menü/HUD okunabilirliği, hedef/skor, GameOver ve A akışını doğruladı.
- [x] 6.2 — Özgün cache'li Fire/Hit/Start/GameOver sesleri ve ilk dinamik isabette doğru el haptics ekle.
- [x] İdempotent sahne entegrasyonu, 54 EditMode testi ve Windows-Audio Development build'ini doğrula.
- [x] 6.2 — Kullanıcı Quest 3 + Link dört ses, doğru el titreşimi, isabet/ıskalama ve restart'ı doğruladı.
- [x] 6.3 — Koyu arena, ortak neon grid/çerçeve meshleri, HDR turuncu hedef ve ince çift el lazerlerini ekle.
- [x] Kontrollü URP Bloom, altı görsel/physics testi, 60/60 regresyon ve Windows-Neon build doğrulandı.
- [x] 6.3 — Kullanıcı Windows-Neon oynanış/HUD/skor/hedef/iki el/ses/haptic/restart'ı doğruladı; genel performans kabul edilebilir.
- [x] Grid aliasing düzeltmesi: tek quad, analitik AA, daha geniş/seyrek çizgiler, mesafe fade; 61/61 test ve NeonFix build.
- [x] Kullanıcı Windows-NeonFix cyan grid'in düzgün olduğunu ve diğer oyun sistemlerinin çalıştığını doğruladı.
- [x] Mor çizgi aliasing düzeltmesi: sonlu çizgi kapsam filtresi, SDR mor ve fade; 63/63 test, NeonLinesFix build.
- [x] Kullanıcı cyan grid ve Windows-NeonLinesFix mor dekor çizgilerini düzgün buldu; diğer oyun sistemleri çalışıyor.
- [x] Dış Side Rails: Z quad geometrisi, mevcut filtre shader'ı ve ayrı SDR materyal; 65/65 test ve Windows-RailsFix build.
- [x] 6.3 — Kullanıcı RailsFix cyan/mor/dış ray, baş hareketi, renk/parlaklık/fade, akıcılık ve tüm oynanışı fiziksel doğruladı.
- [x] Kullanıcı UI/ses dengesi, 6.3 renk/parlaklık/fade ve rahatsız edici çizgi titreşimi olmadığını doğruladı.
- [ ] Odağı kaybetme, uygulamaya geri dönme ve yeniden oynama akışlarını test et.

## 7. Performans ve yayın adayı
- [x] 7.1 — Development Player tanılama, FrameTiming/ProfilerRecorder/XR sayaçları ve bounded rapor altyapısı.
- [x] Odak/pause için sayaç/expiry durdurma; hesap/unavailable ve kararlılık dahil 73/73 EditMode; Windows-Perf build.
- [ ] 7.1 — Fiziksel FPS/p99/CPU/GPU/GC ve ölçüm overhead verilerini Quest 3 + Link ile al.
- [ ] Ready bekleme, focus/pause geri dönüşü, HMD kaybı/recenter ve 5–10 tur native kararlılığı cihazda doğrula.
- [ ] Windows PC ve Link oturumunda CPU/GPU, kare hızı ve bellek ölçümü yap.
- [ ] Ölçülen darboğazları gider; renderer, materyal ve hedef üretimini optimize et.
- [ ] Oturum, input ve cihaz regresyon testlerini tamamla.
- [ ] Son asset/lisans/gizli bilgi denetimi ve Windows release build doğrulamasını yap.

## 8. GitHub ve itch.io yayını
- [ ] Sürüm numarası, release notları ve kurulum/oynama yönergelerini hazırla.
- [ ] Ekran görüntüleri ve itch.io açıklamasını hazırla.
- [ ] PUBLIC GitHub release/tag ve indirilebilir Windows build paketini yayınla.
- [ ] itch.io sayfasını yayınla; indirme ve temiz Windows kurulumu akışını doğrula.
