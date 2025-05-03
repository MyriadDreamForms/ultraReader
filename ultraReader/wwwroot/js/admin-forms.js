// Admin formları için yardımcı fonksiyonlar
$(document).ready(function () {
    // Form gönderimlerinde ek kontroller
    $("form").on("submit", function (e) {
        // Form kontrollerini manuel tetikle
        if (!$(this).valid()) {
            e.preventDefault();
            console.log("Form doğrulama başarısız oldu");
            return false;
        }
        
        // Dosya yükleme kontrolü
        var fileInputs = $(this).find('input[type="file"]');
        if (fileInputs.length > 0) {
            var fileValid = true;
            fileInputs.each(function() {
                if ($(this).prop('required') && !$(this).val()) {
                    // Hata mesajı göster
                    var errorSpan = $('<span class="text-danger">Bu alan gereklidir.</span>');
                    $(this).after(errorSpan);
                    fileValid = false;
                }
            });
            
            if (!fileValid) {
                e.preventDefault();
                console.log("Dosya yükleme doğrulaması başarısız oldu");
                return false;
            }
        }

        // Her şey yolunda, formu gönder
        console.log("Form gönderiliyor...");
        return true;
    });

    // Kapak görseli önizleme
    $('#coverImage').on('change', function() {
        var input = this;
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            
            reader.onload = function(e) {
                var previewDiv = $('<div class="mt-2"><p><strong>Seçilen Görsel:</strong></p><img src="' + e.target.result + '" class="img-thumbnail" style="max-height: 200px;"></div>');
                
                // Önceki önizlemeyi kaldır ve yenisini ekle
                $('#coverPreview').remove();
                $(input).after($('<div id="coverPreview"></div>').append(previewDiv));
            }
            
            reader.readAsDataURL(input.files[0]);
        }
    });

    // Bölüm görselleri önizleme
    $('input[name="Images"], input[name="files"]').on('change', function() {
        var input = this;
        var files = input.files;
        
        if (files && files.length > 0) {
            var previewDiv = $('<div id="imagesPreview" class="mt-3"><p><strong>Seçilen Görseller:</strong></p><div class="d-flex flex-wrap"></div></div>');
            var previewContainer = previewDiv.find('.d-flex');
            
            var maxPreview = Math.min(files.length, 5); // En fazla 5 önizleme göster
            
            for (var i = 0; i < maxPreview; i++) {
                var reader = new FileReader();
                
                reader.onload = (function(file, index) {
                    return function(e) {
                        var imgPreview = $('<div class="p-1"><img src="' + e.target.result + '" class="img-thumbnail" style="max-height: 100px; max-width: 100px;"></div>');
                        previewContainer.append(imgPreview);
                    }
                })(files[i], i);
                
                reader.readAsDataURL(files[i]);
            }
            
            if (files.length > maxPreview) {
                previewContainer.append('<div class="p-1 d-flex align-items-center"><span class="badge bg-secondary">+' + (files.length - maxPreview) + ' daha</span></div>');
            }
            
            // Önceki önizlemeyi kaldır ve yenisini ekle
            $('#imagesPreview').remove();
            $(input).after(previewDiv);
        }
    });
}); 