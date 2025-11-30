// File picker interop for Blazor
window.filePickerInterop = {
    // Store dotnet references and data
    _dotnetRefs: {},
    _inputs: {},
    _files: {},

    // Open folder picker and return folder info
    openFolderPicker: function(pickerId, dotnetRef) {
        return new Promise((resolve) => {
            // Store reference
            this._dotnetRefs[pickerId] = dotnetRef;
            
            // Create dynamic input
            let input = this._inputs[pickerId];
            if (!input) {
                input = document.createElement('input');
                input.type = 'file';
                input.multiple = true;
                input.setAttribute('webkitdirectory', '');
                input.setAttribute('directory', '');
                input.style.display = 'none';
                document.body.appendChild(input);
                this._inputs[pickerId] = input;
            }
            
            // Handle change
            const handleChange = async () => {
                input.removeEventListener('change', handleChange);
                
                if (!input.files || input.files.length === 0) {
                    resolve(null);
                    return;
                }
                
                // Store files for later reading
                this._files[pickerId] = input.files;
                
                // Get folder info
                const firstFile = input.files[0];
                const relativePath = firstFile.webkitRelativePath;
                const folderName = relativePath.split('/')[0];
                
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
                
                resolve({
                    folderName: folderName,
                    files: files
                });
            };
            
            input.addEventListener('change', handleChange);
            input.click();
        });
    },

    // Read file as base64 by index
    readFileAsBase64: function (pickerId, index) {
        return new Promise((resolve) => {
            const files = this._files[pickerId];
            if (!files || index >= files.length) {
                resolve(null);
                return;
            }

            const file = files[index];
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

    // Cleanup
    disposeFolderPicker: function(pickerId) {
        const input = this._inputs[pickerId];
        if (input && input.parentNode) {
            input.parentNode.removeChild(input);
        }
        delete this._inputs[pickerId];
        delete this._files[pickerId];
        delete this._dotnetRefs[pickerId];
    }
};
