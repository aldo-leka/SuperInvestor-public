export function initialize(textAreaId) {
    const textarea = document.getElementById(textAreaId);
    if (textarea) {
        textarea.style.height = 'auto';
        textarea.style.height = textarea.scrollHeight + 'px';
    }
}