// File picker interop for Blazor
window.filePickerInterop = {
    // Get folder info after selection
    getFolderInfo: function (inputId) {
        const input = document.getElementById(inputId);
        if (!input || !input.files || input.files.length === 0) {
            return null;
        }

        // Get the folder path from the first file
        const firstFile = input.files[0];
        const relativePath = firstFile.webkitRelativePath;
        const folderName = relativePath.split('/')[0];
        
        // Collect all files info
        const files = [];
        for (let i = 0; i < input.files.length; i++) {
            const file = input.files[i];
            files.push({
                name: file.name,
                path: file.webkitRelativePath,
                type: file.type,
                size: file.size,
                index: i
            });
        }
        
        return {
            folderName: folderName,
            files: files
        };
    },

    // Read file as base64 by index
    readFileAsBase64: function (inputId, index) {
        return new Promise((resolve) => {
            const input = document.getElementById(inputId);
            if (!input || !input.files || index >= input.files.length) {
                resolve(null);
                return;
            }

            const file = input.files[index];
            const reader = new FileReader();
            reader.onload = function () {
                resolve(reader.result);
            };
            reader.onerror = function () {
                resolve(null);
            };
            reader.readAsDataURL(file);
        });
    },
    
    // Click the folder picker
    clickFolderPicker: function(inputId) {
        const input = document.getElementById(inputId);
        if (input) {
            input.click();
        }
    }
};
