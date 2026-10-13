# NeonReflexVR — Windows PC VR kurulumu (v1.0.0-rc1)

Bu paket Windows x64 PC VR içindir. Meta Quest 3, Quest Link/Air Link bağlantısıyla kullanılır; gözlüğe kurulacak standalone APK değildir.

## Hazırlık ve kurulum
1. Windows bilgisayarda Meta Horizon Link'i kurun ve bilgisayar/gözlük bağlantısını uygulama üzerinden hazırlayın.
2. Meta Horizon Link'i bilgisayarın etkin OpenXR runtime'ı olarak ayarlayın.
3. Quest 3'te Quest Link veya Air Link'i açıp bilgisayarın Link ana ekranına geçin.
4. **NeonReflexVR-Windows-PCVR-v1.0.0-rc1.zip** için Windows'ta **Tümünü ayıkla** seçeneğini kullanın.
5. Çıkarılan klasörde **NeonReflexVR.exe** çalıştırın. ZIP içinden doğrudan başlatmayın.
6. NeonReflexVR_Data, UnityPlayer.dll, MonoBleedingEdge ve diğer dosyalar aynı paket yapısında kalmalıdır. Yalnız EXE yeterli değildir.

## Oynama
- Sağ A: Ready menüsünden başlat; GameOver ekranından yeniden oyna.
- Her iki kontrolcünün trigger'ı: tek basışla tek atış; basılı tutmak sürekli ateş etmez.
- 60 saniyede mümkün olduğunca çok hedef vurun. İlk başarılı hedef isabeti +1 puandır.
- HexCore, Drone ve Capsule hedefleri rastgele seçilir; aynı anda en fazla üç hedef bulunur.
- İsabette beyaz flash, ses ve atışı yapan kontrolcüde hafif titreşim; hedefin yerine yenisi gelir.
- Tur bitince final skor gösterilir. Sağ A yeni turu sıfır skor/60 saniyeyle başlatır.
- Tracking hazır değilse tur başlamaz. Rahat başlangıç yönünü seçin; turdaki hedef alanı başınızı takip ederek dönmez.

## Sorun giderme
- Görüntü VR'ye geçmiyorsa Link bağlantısını ve aktif OpenXR runtime'ı kontrol edin.
- Eksik DLL/Data hatasında ZIP'i tamamen yeniden çıkarın; dosyaları tek tek taşımayın.
- Normal release otomatik FPS/CPU/GPU tanılaması veya perf logu üretmez. Normal Unity hata loglaması korunur.
- Bu bir yayın adayıdır. Release'in gerçek Quest 3 testi ve uzun kararlılık/recenter/tracking/focus kontrolleri henüz onaylanmadı.

RELEASE_NOTES.md, THIRD_PARTY_NOTICES.md ve THIRD_PARTY_LICENSES/ pakete eşlik eder.
