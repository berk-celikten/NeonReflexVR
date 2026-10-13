# Windows release candidate — yeniden üretim

Bu araçlar yalnız yerel yayın adayını hazırlar; GitHub Release, itch.io yükleme veya tag oluşturmaz.

1. Temiz/saved GameScene ve Windows x64 / Mono ile Unity CLI bağlantısını doğrula.
2. CLI eval üzerinden NeonReflexVR.Editor.ReleaseCandidateBuild.Prepare() çağır.
3. Compiler ayarı Editor derlemesini tetikleyebilir. Script compilation tamamen bitip Console temizlenmeden build başlatma.
4. CLI build: StandaloneWindows64, yalnız Assets/Scenes/GameScene.unity, outputPath Builds/Windows-ReleaseCandidate/NeonReflexVR.exe, options yalnız DetailedBuildReport.
5. BuildReport'ta Development/AllowDebugging/ConnectWithProfiler/EnableDeepProfilingSupport false olmalı; scripting backend Mono kalmalı.
6. Build başarılı veya başarısız bitince ReleaseCandidateBuild.RestoreCompilerArguments() çağır; geçici -pathmap ayarı geliştirme tercihlerine bırakılmamalı.
7. Mevcut runtime paketlerinin LICENSE/Third Party Notices dosyalarını Builds/RC1-Licenses altında paket isimleriyle hazırla. THIRD_PARTY_NOTICES.md içindeki tam Unity Companion metni korunur.
8. OpenXR loader için Apache-2.0.txt tam metni resmî https://www.apache.org/licenses/LICENSE-2.0.txt kaynağından eklenir.
9. Tools/Release/PackageWindows.ps1 çalıştır. ZIP zaten varsa araç üzerine yazmaz.
10. Araç debug/source dosyalarını dışarıda tutar, bağımlılık listesini kontrol eder, yeni klasöre çıkarır ve tüm dosyaları SHA-256 ile karşılaştırır.
11. ZIP/extracted dosyalarında token/anahtar ve kişisel derleme yollarını tara. Compiler path remap eklenmesinin nedeni managed DLL CodeView/PDB kayıtlarındaki kişisel yol sızıntısıdır.
12. Çıkarılmış EXE'yi kısa smoke testte çalıştır; Diagnostics/perf-*.log ve [PERF] başlangıcı olmamalı. Normal Player.log kapatılmaz.

Paket bilgisi ve checksum Builds/Packages altında tutulur; bu çıktılar Git'e eklenmez.
Bu aday 1.0.0-rc1'e sabitlenmiştir; yeni adayda sürüm/adları bilinçli olarak güncellenir.
Fiziksel Quest 3 release onayı ve uzun kararlılık/recenter/tracking/focus testleri bu araçlar tarafından tamamlanmış sayılmaz.

Ek compiler argümanı API'si: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/PlayerSettings.SetAdditionalCompilerArguments.html
