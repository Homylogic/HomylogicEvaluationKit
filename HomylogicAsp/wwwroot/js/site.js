// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


/* -- BODY CONTAINER HEIGHT -- */
var Height_MenuMain;
var Height_Footer;
$(document).ready(function () {
    // Sets default height of body container.
    Height_MenuMain = document.getElementById("menu-main").clientHeight;
    Height_Footer = document.getElementById("footer").clientHeight;
    OnWindowResize();
    window.addEventListener('resize', OnWindowResize);
});

function OnWindowResize()
{
    // Set content body container height.
    var targetHeight = window.innerHeight - Height_MenuMain - Height_Footer - 30;
    var bodyContainer = document.getElementById("body-container");
    bodyContainer.style.height = targetHeight + "px";
}




/* --- DEFAULT NAVIGATION BACK --- */
$("#navigate-back").on("click", function (e) {
    e.preventDefault();
    window.history.back();
});




/* --- CONTROLS --- */

// Clicked on expandation panel by html element ID.
// Expands or collapse html element by changing display style - none or expandStyle argument.
function OnClickExpander(elementId) {
    var expander = document.getElementById(elementId);
    if (expander.style.display === "none") {
        expander.style.display = "block";
    } else {
        expander.style.display = "none";
    }
}
