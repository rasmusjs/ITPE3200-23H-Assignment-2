document.addEventListener("DOMContentLoaded", function () {
    const burgerBtn = document.getElementById("burger-btn");
    const ul = document.getElementById("nav-link-list");

    burgerBtn.addEventListener("click", function () {
        ul.classList.toggle("open");
        burgerBtn.classList.toggle("active");
    });
});