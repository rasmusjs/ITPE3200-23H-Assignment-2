document.addEventListener('DOMContentLoaded', function() {
    console.log("loaded");
    const burgerBtn = document.getElementById("burger-btn");
    const navLinkList = document.getElementById("nav-link-list");
    
    burgerBtn.addEventListener("click", () => {
        burgerBtn.classList.toggle("active");
        navLinkList.classList.toggle("active");
    })
})