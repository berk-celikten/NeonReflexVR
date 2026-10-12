# NeonReflexVR — Durum

## Amaç ve teknoloji
- Windows PC VR: 60 saniyelik hedef vurma arcade oyunu; Quest 3 + Link/Air Link.
- Unity 6.3 LTS `6000.3.26f1`; C#; URP `17.3.0`; Windows x64 / Mono.
- XRI `3.3.2`; OpenXR `1.16.1`; XR Management `4.6.1`; Input System `1.20.0`.
- Test Framework `1.6.0`; UGUI `2.0.0`; Unity CLI `1.0.0-beta.12`; Pipeline `0.8.0-exp.1`.
- PUBLIC https://github.com/berk-celikten/NeonReflexVR — branch `main`.

## Tamamlananlar
- Git/ignore/lisans bildirimleri hazır; GameScene ilk build sahnesi; SampleScene ve meta dosyaları korunur.
- XR Origin, tek kamera/Audio Listener, HMD ve iki kontrolcü tracking/input bağlantıları hazır.
- İki elle tek basışlık Raycast, ayrı lazer; dinamik Target ilk isabetine +1 puan.
- Üç sphere hedef; 0,15 s isabet flash/yenileme, vurulmazsa 5 s yenileme; GameOver temizliği.
- Serbest bölge seçimi: doluluk/yakın kullanıma ağırlık; farklı bölge tercihi; güvenli fallback/retry.
- 2,5–3,5 m mesafe, konforlu yükseklik, 0,65 m aralık ve collider/görüş hattı kontrolleri korunur.
- Başlangıç/restart geçerli HMD pose'una hizalanır; alan tur boyunca başı takip etmez.
- Kullanıcı RandomFix ile bölge geçişi/denge, en fazla 3 hedef, sabit alan, çift el/skor, 60 s/GameOver/A restart'ı doğruladı.
- Aşama 4 ve 5 ilgili fiziksel testleri tamamlandı.
- 6.1: otomatik başlangıç kaldırıldı; Ready'de hedef yok, skor 0 ve süre 60'ta bekler.
- Ready menüsü: NEON REFLEX VR; yönergeler; Press A to Start.
- Sağ A: mevcut XRI Right Locomotion/Jump; yeni basış Ready'yi başlatır veya GameOver'dan yeni tur açar.
- HMD hazır değilse A başlatamaz; basılı A tracking geri geldiğinde kendiliğinden tur başlatmaz.
- Playing HUD yalnız SCORE/TIME; 60 s tur ve önceki skor/atış/hedef davranışları korunur.
- GameOver: GAME OVER / FINAL SCORE / Press A to Play Again; hedef yok, skor sabit.
- Mevcut World Space Canvas/Text yeniden kullanıldı; koyu panel, cyan/turuncu vurgu, yerleşik font.
- Menü/sonuç göz hizasında 2 m önde; HUD gözden 0,6 m yukarıda, hedeflerin üstünde; turda sabit.
- UI metni değer/durum değişince güncellenir; sürekli panel taşıma/UI rebuild yok.
- Yeni paket/font/dış asset/ses/haptik/partikül/oyun modu eklenmedi.
- Windows-GameLoop ve Windows-SpawnFix temizlendi: yaklaşık 356,7 MiB; Windows-RandomFix korundu.

## Mevcut aşama ve sıradaki görev
- Aşama 6.1 uygulaması/test/build tamam; Quest 3 fiziksel UI testi bekliyor.
- Önceki RandomFix commit'i: `ed17af4`; güncel commit için `git log -1`.
- Sıradaki: Ready'de bekleme, A ile başlama, HUD okunabilirliği/engel oluşturmama, sonuç ve A ile yeni tur testi.
- Sonrasında Aşama 6.2 sunum iyileştirmeleri; ses/haptik bu görevde eklenmedi.
- Yeni EXE: `Builds/Windows-UI/NeonReflexVR.exe`; Data/DLL dosyalarıyla birlikte kullan.
- Windows-RandomFix, Windows-UI fiziksel onayına kadar silinmez.

## Doğrulama ve bilinen sorunlar
- 45/45 EditMode testi geçti: Ready/A/HMD engeli, HUD/sonuç metinleri, yerleşim ve önceki oyun/dağılım regresyonları.
- Ready/HUD/GameOver Unity Game View görsel preview'ları kontrol edildi; metin kesilmesi görülmedi.
- Script derlemesi başarılı; Console: 0 hata / 32 uyarı; Windows-UI build: 0 hata / 20 uyarı.
- Bilinen uyarılar: eski Editor oturumunun URP dizi boyutları; Player'da CLI runtime kapalı; Samples klasörü yok.
- 6.1 stereo UI okunabilirliği, başlangıç/sonuç ekranı ve A akışı henüz fiziksel olarak doğrulanmadı.
- Recenter/takip kaybından dönüş/uzun süreli kararlılık ölçümleri ayrıca takip edilir.
- Builds, Temp/Library, `.utmp`, `Assets/_Recovery` Git'e yüklenmez; [ROADMAP](ROADMAP.md).