// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

window.addEventListener('DOMContentLoaded', () => {
    const messageBox = document.getElementById('success-message');
    if (messageBox) {
        setTimeout(() => {
            messageBox.style.transition = "opacity 1s ease-out";
            messageBox.style.opacity = '0';
            setTimeout(() => {
                messageBox.style.display = 'none';
            }, 1000);  // Μετά από το fade, το εξαφανίζει τελείως
        }, 3000); // Εμφανίζεται για 3 δευτερόλεπτα
    }
});
