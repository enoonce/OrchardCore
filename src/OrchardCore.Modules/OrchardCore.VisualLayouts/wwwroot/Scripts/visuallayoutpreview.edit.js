function initializeVisualLayoutPreview(nameElement, stateElement) {

    var tokenElement = document.querySelector("[name='__RequestVerificationToken']");

    var sendFormData = function () {

        var formData = 'Name=' + encodeURIComponent(nameElement.value) +
            '&State=' + encodeURIComponent(stateElement.value) +
            '&__RequestVerificationToken=' + encodeURIComponent(tokenElement ? tokenElement.value : '');

        // store the form data to pass it in the event handler
        localStorage.setItem('OrchardCore.visuallayouts', JSON.stringify(formData));
    }

    window.addEventListener('storage', function (ev) {
        if (ev.key != 'OrchardCore.visuallayouts:ready') return; // ignore other keys

        // triggered by the preview window the first time it is loaded in order
        // to pre-render the view even if no visuallayouts:render is already sent
        sendFormData();
    }, false);

    stateElement.addEventListener('change', function () { sendFormData(); });
    stateElement.addEventListener('input', function () { sendFormData(); });

    window.addEventListener('unload', function () {
        localStorage.removeItem('OrchardCore.visuallayouts');
        // this will raise an event in the preview window to notify that the live preview is no longer active.
        localStorage.setItem('OrchardCore.visuallayouts:not-connected', '');
        localStorage.removeItem('OrchardCore.visuallayouts:not-connected');
    });
}
