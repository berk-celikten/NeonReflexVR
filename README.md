# NeonReflexVR

Windows bilgisayarda çalışan, Meta Quest 3'ün Quest Link veya Air Link üzerinden kullanıldığı küçük bir PC VR arcade oyunu. Hedef, 60 saniyelik hedef vurma oyun döngüsüdür. Ana platform kullanıcı kararıyla Android standalone'dan Windows PC VR'ye değiştirilmiştir.

## Teknolojiler

Unity 6.3 LTS `6000.3.26f1`, C#, URP `17.3.0`, XR Interaction Toolkit `3.3.2`, OpenXR Plugin `1.16.1`, XR Plug-in Management `4.6.1`, Input System `1.20.0`, Test Framework `1.6.0`.

Yerel Unity CLI `1.0.0-beta.12`, mevcut `com.unity.pipeline` `0.8.0-exp.1` paketiyle Editor'a bağlanır. Yeni deneysel XR veya Meta All-in-One paketi eklenmemiştir.

## Başlangıç noktası

Yeni oturumda önce [docs/STATUS.md](docs/STATUS.md), ardından yalnızca görevle ilgili [ROADMAP aşaması](docs/ROADMAP.md) okunur. Çalışma kuralları [AGENTS.md](AGENTS.md) içindedir.

Unity Hub üzerinden projeyi `6000.3.26f1` ile açın. PC VR hedefi Windows x64 / Mono'dur. Windows XR ayarlarında OpenXR Loader, başlangıçta XR başlatma ve Oculus Touch Controller Profile etkindir.

PC'de Meta Horizon Link'i aktif OpenXR runtime yapın ve gözlükten Quest Link/Air Link bağlantısını başlatın. Ardından `Assets/Scenes/GameScene.unity` ile Unity Play Mode'u veya `Builds/Windows-RailsFix/NeonReflexVR.exe` uygulamasını kullanın. EXE, yanındaki Data klasörü ve DLL'lerle birlikte tutulmalıdır. Temel PC VR kullanımında gözlüğe APK yükleme/ADB yetkilendirmesi yapılmaz. [Meta Link yönergesi](https://developers.meta.com/vr/documentation/unity/unity-link/).

## Mevcut durum

GameScene'de XR Origin, tek Main Camera/Audio Listener, iki tracked kontrolcü, zemin ve çalışma sırasında üretilen üç küresel hedef vardır. Eski test küpleri yalnız sahneden kaldırılmıştır; kaynakları korunur. Input System TrackedPoseDriver bileşenleri resmî XRI Head/Left/Right action referanslarını kullanır. Materyaller URP Unlit'tir. SampleScene korunur ve GameScene build listesinde ilk sıradadır.

Windows Development build başarılıdır; kullanıcı Link üzerinden VR görüntü, baş ve iki kontrolcü takibini fiziksel gözlükte doğrulamıştır. Göz yüksekliği, recenter, tracking kaybından dönüş ve uzun süreli kararlılık testleri beklemektedir. Her iki trigger ile bağımsız tek Raycast atışı, ayrı kısa lazerler ve hedeflerde kısa beyaz isabet geri bildirimi hazırdır. Çift el fiziksel testinin 7 kontrolü kullanıcı tarafından doğrulandı. TargetSpawner geçerli HMD başlangıç bakışının önündeki alanda üç rastgele sphere hedef oluşturur. Slotlar bölgeye kilitlenmez; az dolu/az kullanılan bölgelere ağırlık verilir ve yenilemede farklı bölge tercih edilir; tek isabet sonrası beyaz flash ile, vurulmazsa 5 saniye sonra yeniler. Rastgele hedef oynanışı kullanıcı tarafından fiziksel olarak doğrulandı. GameManager Ready menüsünde sağ A ile başlayan 60 saniyelik tur, dinamik hedef başına +1 puan, GameOver temizliği ve sağ A ile restart yönetir; test küpleri puan kazandırmaz. World Space UI başlangıç yönergeleri, SCORE/TIME HUD ve GAME OVER/FINAL SCORE gösterir; koyu panel, cyan/turuncu vurgu ve yerleşik font kullanılır. RandomFix hedef dağılımı, çift el/skor, 60 saniye ve A restart kullanıcı tarafından fiziksel olarak doğrulandı. 6.1 menü/HUD ve A akışı kullanıcı tarafından fiziksel olarak doğrulandı. Dört düşük seviyeli özgün prosedürel ses ve başarılı dinamik isabette yalnız ateş eden kontrolcü için hafif haptics eklendi; 6.2 ses/titreşim kullanıcı tarafından fiziksel olarak doğrulandı; koyu neon arena, cyan/mavi/mor grid ve çerçeveler, HDR turuncu hedefler, ince lazerler ve kontrollü Bloom eklendi; 6.3 genel oynanış kullanıcı tarafından fiziksel olarak doğrulandı; uzak grid için analitik anti-aliasing ve mesafe fade eklendi, cyan grid fiziksel olarak doğrulandı; mor dekoratif çizgilere kapsam filtresi ve fade eklendi, mor çizgiler de fiziksel olarak doğrulandı; dış Side Rails çizgilerine filtreleme ve uygun Z quad geometrisi uygulandı, Windows-RailsFix fiziksel testi beklemektedir; locomotion yoktur.

Sahne ve ilk ateş etme sistemi için 65/65 EditMode testi geçti. Windows build 0 hata / 20 uyarıyla tamamlandı; URP geçiş uyarıları ve diğer bilinen durumlar STATUS'ta kayıtlıdır. Kurulum otomasyonu `Assets/Editor/Setup/GameSceneSetup.cs`, ateş etme ekleme otomasyonu `Assets/Editor/Setup/ShootingSceneSetup.cs`; testler `Assets/Tests/EditMode` altındadır.

## Git ve lisanslar

Repository [GitHub üzerinde PUBLIC](https://github.com/berk-celikten/NeonReflexVR) kalır. Yayın hedefi itch.io için Windows build paketidir. `Assets`, `.meta`, `Packages` ve `ProjectSettings` korunur; build çıktıları, geçici dosyalar ve kurtarma yedekleri Git'e yüklenmez.

Unity şablon ve resmî XRI input action içeriğinin telif bildirimleri ve Unity Companion License'ın tam metni [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) içindedir. Bu lisans ilgili Unity içeriği ve türevlerine uygulanır; özgün NeonReflexVR kodunun tamamına atanmaz. Proje genelinde özgün içerik için açık kaynak lisansı henüz seçilmemiştir.

Eski Android XR ayarları korunmuştur. İlk Android APK build başarılı olmuş, cihaz ADB'de görünmediği için yükleme/test yapılmamıştır; bu dağıtım yolu ana hedeften çıkarılmıştır.
