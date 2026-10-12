# Performans ve VR kararlılık testi — 7.1

## Kullanım
- Quest Link açıkken `Builds/Windows-Perf/NeonReflexVR.exe` Development Player'ını başlat.
- Tanılama otomatik çalışır; Editor ve normal release Player'da otomatik başlamaz. HUD'a ek bilgi koymaz.
- Rapor: `%USERPROFILE%/AppData/LocalLow/DefaultCompany/NeonReflexVR/Diagnostics/perf-*.log`.
- Aynı `[PERF]` satırları `Player.log` içinde de bulunur; başlangıç satırı kesin rapor yolunu bildirir.
- Yaklaşık 30 saniyede, durum/odak/pause değişiminde ve normal çıkışta rapor yazılır. Zorla kapanış son pencereyi kaybettirebilir.
- Ready, Playing ve GameOver pencereleri ayrıdır; geçişte gecikmeli native sayaçlar bir önceki kareyi temsil edebilir.
- Raporda veri yoksa oyunun/Link'in odağını ve başlangıç logunu kontrol et. Dosya yazılamazsa Player.log kullanılır.
- Başlangıç/dönüşte zaman tabanı için ilk kare atlanır; odaksız/pause boşlukları aktif FPS'e katılmaz.

## Metrikler ve sınırlar
- `avgPlayerFps`: aktif Player LateUpdate aralıklarının ortalaması. Gözlük/compositor FPS'i veya Link ağ gecikmesi değildir.
- `p99Ms`: nearest-rank %99 kare zamanı. `p99EquivalentFps=1000/p99Ms`; en kötü %1 karelerin ortalaması değildir.
- `worstMs`, `spikesOver33ms`, `topSpikes`: pencerenin en kötü sekiz karesi, zamanları ve oyun durumu; eşik 33,3 ms.
- `cpuFrameFTMms`, `gpuFrameFTMms`: FrameTimingManager'ın benzersiz timestamp'li, pozitif/geçerli örneklerinin ortalaması.
- GPU timer desteği yoksa, API veri vermiyorsa veya geçerli örnek yoksa `unavailable`; CPU'dan GPU değeri türetilmez.
- FrameTiming gecikmeli/asenkron ölçümdür; `cpu/gpuSamples` veri kapsamını gösterir. CPU frame süreleri beklemeleri içerebilir.
- `mainThreadInclusiveRecorderMs`: ProfilerRecorder Main Thread süresi; CPU busy-time olarak yorumlama.
- `gcAllocMeanBytes`, `gcAllocPeakBytes`, `gcUsedMB`, `gcReservedMB`, `gc0Collections`: desteklenen yönetilen bellek/GC sayaçları.
- Bu bellek değerleri toplam süreç belleği veya VRAM değildir; sayaçlar bir kare gecikebilir; eksikler `unavailable` olur.
- `drawCallsLatest`, `trianglesLatest`: son sayaç örneği; pencere ortalaması değil.
- `xrRefreshHz`, `xrDroppedCumulative`: çalışan XRDisplaySubsystem sağlayıcısı başarılı yanıt verirse kaydedilir; eksikler `unavailable`.
- `focusLosses`, `trackingLosses` oturum boyunca birikir. Tracking logu en fazla 1 Hz; çok kısa kayıplar device bulma örneklemesine takılabilir.
- 8192 karelik sabit tampon ve sekiz spike kaydı vardır; yalnız raporlama anında sıralama/metin/dosya I/O yapılır.
- Deep profiling ve per-frame log yok. Raporun kendi allocation/I/O etkisi sonraki karede ölçülür; ölçüm overhead'i cihazda henüz ölçülmedi.

## Mevcut davranış / aday maliyetler
- Odak kaybı veya application pause: Playing sayacı ve hedef expiry/retry saatleri durur; puan/sesli atış kabul edilmez.
- Odak ve pause birlikte biterse devam eder; otomatik yeni tur başlamaz. Ready/GameOver'da odaksız A başlangıcı engellenir.
- Tracking kaybı tek başına turu durdurmaz; start/restart geçerli HMD ister. Mevcut tur anchor/HUD sabittir; yeni tur güncel pose alır.
- Native recenter/odak olaylarının Link tarafından gerçekten nasıl iletildiği cihaz testini bekler; yeni recenter sistemi eklenmedi.
- Hedef Instantiate/Destroy ve engelli alanda her yeni hedefte en çok 3x91 aday ve aday başına en çok iki physics sorgusu spike adayıdır; pooling henüz gerekçelendirilmedi.
- Skor/saniye değişiminde UI metin allocation/rebuild; isabette native ses/haptik çağrıları ölçülmeli. Clip/material cache'leri korunur.
- GPU adayları: mevcut Forward+ URP depth/opaque kopyaları, Bloom (Quarter, 4 iterasyon), iki transparan dekor renderer'ı ve stereo fill-rate.
- Üç hedef sınırı, collider'sız dekor, değişmeyen shader/materyaller ve tek AudioSource korunur; gerekçesiz optimizasyon yapılmadı.

## Quest 3 test sırası (henüz tamamlanmadı)
1. RailsFix ile aynı Link bağlantısı/çözünürlük/refresh hızında 30–60 s Ready bekle; hedef/sayaç hareket etmemeli.
2. 5–10 tur normal çift el ateşle oyna; raporların Playing FPS/p99, CPU/GPU, GC ve spike zamanlarını karşılaştır.
3. Tur ortasında kısa Alt-Tab/odak kaybı ve geri dönüş: süre/puan/üç hedef durmalı; kalan süreyle devam etmeli.
4. HMD tracking kaybı/geri kazanımı ve recenter: görüntü/eller toparlanmalı; mevcut tur alanı kaymamalı; yeni tur güncel yöne hizalanmalı.
5. GameOver'da 30 s bekle; hedef/puan artmamalı. A restart tek tur başlatmalı; ses/haptic/nesne birikmesi olmamalı.
6. Normal çıkıştan sonra raporları sakla. GPU `unavailable` ise bunu eksik ölçüm olarak bırak; tahminle başarı işaretleme.