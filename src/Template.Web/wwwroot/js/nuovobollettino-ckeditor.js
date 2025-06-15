$(document).ready(function() {
    ClassicEditor
        .create( document.querySelector( '#contenutoBollettino' ), {
            toolbar: [
                'undo', 'redo', '|',
                'heading', '|',
                'bold', 'italic',  '|',
                'bulletedList', 'numberedList', '|',             
            ]
        } )
        .then( editor => {
            window.editor = editor;
        } )
        .catch( error => {
            console.error( error );
        } );
}); 