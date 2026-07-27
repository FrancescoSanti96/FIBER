let editor;
ClassicEditor
    .create(document.querySelector('#editor'), {
        toolbar: [
            'undo', 'redo', '|',
            'heading', '|',
            'bold', 'italic', '|',
            'bulletedList', 'numberedList', '|',
        ]
    })
    .then(newEditor => {
        editor = newEditor;
    })
    .catch(error => {
        console.error(error);
    });
