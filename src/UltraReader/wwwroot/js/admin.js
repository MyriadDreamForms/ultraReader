// UltraReader - Admin JavaScript Module

// Natural sort comparison for filenames (handles numbers correctly)
function naturalCompare(a, b) {
    const ax = [], bx = [];
    
    a.replace(/(\d+)|(\D+)/g, function(_, $1, $2) { ax.push([$1 || Infinity, $2 || ""]) });
    b.replace(/(\d+)|(\D+)/g, function(_, $1, $2) { bx.push([$1 || Infinity, $2 || ""]) });
    
    while(ax.length && bx.length) {
        const an = ax.shift();
        const bn = bx.shift();
        const nn = (an[0] - bn[0]) || an[1].localeCompare(bn[1]);
        if(nn) return nn;
    }
    
    return ax.length - bx.length;
}

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
                // Get image files and sort them naturally by filename
                const imageFiles = Array.from(input.files)
                    .filter(file => file.type.startsWith('image/'))
                    .sort((a, b) => naturalCompare(a.name, b.name));
                
                // Transfer sorted files to existing input
                const dt = new DataTransfer();
                for (let file of imageFiles) {
                    dt.items.add(file);
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
