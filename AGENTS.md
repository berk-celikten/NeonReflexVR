# NeonReflexVR çalışma kuralları

- Önce `docs/STATUS.md` dosyasını ve kısa Git durumunu oku. Uzun sohbet geçmişine bağımlı kalma.
- Her görevde tüm repository'yi tarama. Yalnızca görevle ilgili dosyaları ve gerektiğinde `docs/ROADMAP.md` içindeki ilgili aşamayı incele.
- Değişiklikleri küçük ve görev kapsamıyla sınırlı tut; kullanıcının mevcut değişikliklerini koru.
- Açık görev talebi olmadan Unity sürümünü, paketleri, sahneleri, prefabları veya proje ayarlarını değiştirme. Gereksiz silme, taşıma ve toplu yeniden serileştirme yapma.
- `.meta` dosyalarını ve GUID'leri koru; varlık taşınırsa `.meta` dosyasını birlikte taşı.
- Unity işlemlerinde mümkün olduğunca Unity CLI ve desteklenen Editor API'lerini kullan; gerekli derleme, Console ve ilgili test kontrollerini yap.
- Belge değişikliklerinde diff ve dosya kontrolleri yeterlidir; gereksiz Unity/test çalıştırma. Yeni değişiklik veya çözülmemiş hata olmadıkça testleri tekrarlama.
- Doğrulanmamış işlemleri tamamlandı gösterme. Editor yapılandırmasını fiziksel Quest 3 testiyle karıştırma; yapılmayan kontrolleri belirt.
- Gereksiz uzun raporlar, tekrar eden açıklamalar ve log dökümleri üretme. Sonuçları kısa ve Türkçe raporla.
- Git öncesinde kök, status ve ilgili diff'i kontrol et; yalnızca amaçlanan dosyaları stage et. Geçici çıktıları, kişisel dosyaları, tokenları ve signing anahtarlarını yükleme; yeni üçüncü taraf içerikte lisans bildirimlerini koru.
- Git geçmişini/ayarlarını sıfırlama; `reset --hard`, `clean`, geçmiş yeniden yazma ve force push kullanma. Kullanıcı değişikliklerinin üzerine yazma.
- Önemli milestone sonunda STATUS ve ilgili ROADMAP maddelerini güncelle, uygun commit ve normal push yap. Repository PUBLIC kalır; yeni remote/repository oluşturma veya görünürlüğü değiştirme.
- `docs/STATUS.md` dosyasını her küçük işlemde güncelleme; milestone tamamlandığında güncelle. CheckBox'ları yalnızca gerçekten tamamlanan işler için işaretle.
