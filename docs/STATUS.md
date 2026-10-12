# NeonReflexVR — Durum

## Amaç ve teknoloji
- Windows PC VR: 60 saniyelik hedef vurma arcade oyunu; Quest 3 + Link/Air Link.
- Unity 6.3 LTS `6000.3.26f1`; C#; URP `17.3.0`; Windows x64 / Mono.
- XRI `3.3.2`; OpenXR `1.16.1`; XR Management `4.6.1`; Input System `1.20.0`.
- Test Framework `1.6.0`; UGUI `2.0.0`; CLI `1.0.0-beta.12`; Pipeline `0.8.0-exp.1`.
- PUBLIC https://github.com/berk-celikten/NeonReflexVR — branch `main`.

## Tamamlananlar
- Git/ignore/lisanslar; GameScene ilk sahne; mevcut meta, SampleScene ve XR/tracking/input referansları korundu.
- İki elle tek basışlık Raycast/lazer; ilk dinamik hedefe +1 skor; en fazla üç hedef.
- Serbest ağırlıklı bölge dağılımı, güvenli mesafe/yükseklik/aralık/LOS ve retry; sabit tur anchor'ı.
- Hedef: 0,15 s beyaz flash/yenileme, vurulmazsa 5 s; GameOver temizliği.
- Ready/A başlangıç, 60 s tur, GameOver/final skor ve sağ A restart; geçerli HMD yönüne yeni tur hizalaması.
- World Space menü/HUD; değer değişiminde güncelleme; dört cache'li özgün ses ve ilk isabette doğru el haptics.
- Aşama 4/5 oynanış, 6.1 UI ve 6.2 ses/haptic/restart kullanıcı tarafından fiziksel olarak doğrulandı.
- 6.3 RailsFix kullanıcı onayı: cyan grid, mor çizgiler, dış alt/üst raylar düzgün; baş hareketinde rahatsız edici titreşim yok.
- Renk/parlaklık/fade kabul edilebilir; akıcılık ve hedef/iki el/skor/ses/haptic/restart fiziksel olarak doğrulandı.
- Grid/çizgi filtreli shader'ları, HDR turuncu hedef, collider'sız dekor ve kontrollü Quarter Bloom korunur.
- 7.1 PerformanceDiagnostics Development Player'da otomatik; Editor/release otomatik ölçülmez; normal HUD değişmedi.
- Sabit 8192 kare tamponu, sekiz spike; 30 s/durum/odak/çıkış raporu; per-frame log ve deep profiling yok.
- Player FPS/p99/eşdeğer düşük FPS, desteklenen FrameTiming CPU/GPU ve ProfilerRecorder Main Thread/GC/draw sayaçları.
- Desteksiz/geçersiz GPU/GC/XR değerleri unavailable; çalışan XR sağlayıcısı refresh/dropped bilgisi verir.
- FrameTimingStats etkin; raporlar persistentDataPath/Diagnostics ve Player.log; [kullanım/sınırlar](PERFORMANCE.md).
- Odak veya application pause'da Playing sayacı ve hedef expiry/retry saatleri durur; isabet/sesli atış puan kazandırmaz.
- Focus ve pause birlikte sona erince devam eder; Ready/GameOver'da odaksız start/restart engellenir.
- Dört eski build silindi: Audio/Neon/NeonFix/NeonLinesFix; yaklaşık 712,5 MiB kazanıldı; RailsFix korundu.

## Mevcut aşama ve sıradaki görev
- 7.1 ölçüm altyapısı/otomatik test/Windows Development build tamam; fiziksel FPS/CPU/GPU/kararlılık testi bekliyor.
- Yeni EXE: `Builds/Windows-Perf/NeonReflexVR.exe`; güncel commit için `git log -1`.
- Rapor: `%USERPROFILE%/AppData/LocalLow/DefaultCompany/NeonReflexVR/Diagnostics/perf-*.log`.
- Sıradaki: aynı Link ayarlarında Ready, 5–10 tur, odak/pause, tracking kaybı/geri dönüş ve recenter testi.
- Windows-RailsFix, Perf fiziksel olarak doğrulanana kadar korunur.

## Doğrulama ve bilinen sorunlar
- 73/73 EditMode geçti: 65 regresyon + hesap/unavailable/tampon/Editor koruması ve focus/pause/tracking testleri.
- 15 tur testinde üç hedef sınırı, tek AudioSource ve cache'li clip kimliği korunur; native ses/haptic birikimi cihazda ölçülmedi.
- Derleme başarılı; Console 0 hata / 26 bilinen URP uyarısı; Windows-Perf build 0 hata / 20 bilinen uyarı.
- P99 eşdeğeri gerçek worst-%1 ortalaması değil; Player FPS headset/compositor FPS değildir. Donanım metrikleri henüz alınmadı.
- Recorder Main Thread beklemeleri içerebilir; managed bellek toplam süreç/VRAM değil; rapor I/O etkisi ölçülmedi.
- Tracking kaybı tek başına saati durdurmaz; mevcut anchor/HUD sabit kalır; native recenter/Link event davranışı beklemede.
- Aday darboğazlar: Instantiate/Destroy + güvenli physics arama, UI güncellemesi, Bloom/transparan stereo fill ve URP kopyaları.
- Ölçümsüz optimizasyon/pooling veya yeni paket/MCP eklenmedi; görsel dönüşüm yok.
- Builds, Temp/Library, `.utmp`, `_Recovery` yüklenmez; [ROADMAP](ROADMAP.md).