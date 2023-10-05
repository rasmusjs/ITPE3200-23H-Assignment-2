document.addEventListener('DOMContentLoaded', function () {

    const burgerBtn = document.getElementById("burger-btn");
    const navLinkList = document.getElementById("nav-links-container");

    // Toggles between opening and closing nav-link-list
    burgerBtn.addEventListener("click", () => {
        burgerBtn.classList.toggle("active");
        navLinkList.classList.toggle("active");
    })
})