// UltraReader - Admin JavaScript Module

export function selectFolderForInput(inputId) {
    return new Promise((resolve) => {
        const input = document.createElement('input');
        input.type = 'file';
        input.multiple = true;
        input.accept = 'image/*';
        input.webkitdirectory = true;
        
        input.onchange = function(e) {
            const existingInput = document.getElementById(inputId);
            if (existingInput && input.files.length > 0) {
                // Transfer files to existing input
                const dt = new DataTransfer();
                for (let file of input.files) {
                    // Only add image files
                    if (file.type.startsWith('image/')) {
                        dt.items.add(file);
                    }
                }
                existingInput.files = dt.files;
                existingInput.dispatchEvent(new Event('change', { bubbles: true }));
            }
            resolve(input.files.length);
        };
        
        input.oncancel = function() {
            resolve(0);
        };
        
        input.click();
    });
}
