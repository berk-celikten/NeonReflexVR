# NeonReflexVR çalışma kuralları

- Değişiklikleri küçük, kontrollü ve görev kapsamıyla sınırlı tut. İşe başlamadan mevcut dosyaları ve Git durumunu incele.
- Unity sürümünü `ProjectSettings/ProjectVersion.txt` üzerinden doğrula; sürüm yükseltmesini ayrı bir karar olarak ele al.
- Açık talep olmadan oyun mekaniği ekleme, Unity paketlerini yükleme/kaldırma veya proje ayarlarını değiştirme.
- Unity sahnelerini (`.unity`), prefabları ve diğer varlıkları koru. Açıkça istenmeyen sahne düzenlemesi veya toplu yeniden serileştirme yapma.
- Varlıkların `.meta` dosyalarını ve GUID değerlerini koru. Bir varlığı taşıman gerekiyorsa ilgili `.meta` dosyasını birlikte taşı; `.meta` dosyalarını silerek yeniden üretme.
- Mevcut dosyaları gereksiz yere silme veya yeniden adlandırma. Kullanıcının değişikliklerinin üzerine yazma.
- Değişiklikleri uygun kapsamda doğrula: belge/Git hazırlığında diff ve ignore kurallarını; kod değişikliğinde derleme ve ilgili testleri; VR davranışında gerekirse Quest 3 cihaz testini kontrol et.
- Yapılmayan testleri veya erişilemeyen logları açıkça raporla. Eski bir Editor logunu yeni bir build ya da cihaz testi olarak sunma.
- Git işlemlerinden önce repository kökünü, status ve diff sonuçlarını incele; yalnızca amaçlanan dosyaları stage et. Üretilen dosyaları, yerel kullanıcı ayarlarını ve gizli bilgileri commit etme.
- Var olan Git repository veya ayarlarını sıfırlama. Kullanıcı açıkça istemedikçe `reset --hard`, `clean`, geçmiş yeniden yazma veya force push yapma.
- Commit, remote ekleme, GitHub push ve yayın işlemlerini yalnızca ilgili kullanıcı talebi kapsamında yap. Bu ilk görevde commit, remote bağlantısı ve push yapılmaz.
- Sonuçları, yapılan kontrolleri ve tespit edilen sorunları Türkçe raporla.
