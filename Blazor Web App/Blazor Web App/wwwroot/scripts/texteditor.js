window.setEditorContent = (editorRef, content) => {
    if (editorRef) {
        editorRef.innerHTML = content;
    }
};

window.getEditorContent = (editorRef) => {
    return editorRef.innerHTML;
};

window.execCommandOnContent = (command) => {
    document.execCommand(command, false, null);
};