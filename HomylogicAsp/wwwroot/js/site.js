// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


/* --- DEFAULT NAVIGATION BACK --- */
$("#navigate-back").on("click", function (e) {
    e.preventDefault();
    window.history.back();
});




/* --- CONTROLS --- */

// Clicked on expandation panel by html element ID.
// Expands or collapse html element by changing display style - none or expandStyle argument.
function OnClickExpander(elementId) {
    var x = document.getElementById(elementId);
    if (x.style.display === "none") {
        x.style.display = "block";
    } else {
        x.style.display = "none";
    }
}
