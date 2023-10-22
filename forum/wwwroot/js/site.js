document.addEventListener('DOMContentLoaded', function () {

    const burgerBtn = document.getElementById("burger-btn");
    const navLinkList = document.getElementById("nav-links-container");

    // Toggles between opening and closing nav-link-list
    burgerBtn.addEventListener("click", () => {
        burgerBtn.classList.toggle("active");
        navLinkList.classList.toggle("active");
    })

    document.querySelectorAll(".reply-btn").forEach(function (button) {
        button.addEventListener("click", function () {
            // Toggle the "active" class on the related div
            this.nextElementSibling.classList.toggle("active");

            // Toggle button text between original and "Close"
            if (this.textContent === "Reply") {
                this.textContent = "Close";
            } else {
                this.textContent = "Reply";
            }
        });
    });

    document.querySelectorAll(".edit-btn").forEach(function (button) {
        button.addEventListener("click", function () {
            // Toggle the "active" class on the related div
            this.nextElementSibling.classList.toggle("active");

            // Toggle button text between original and "Close"
            if (this.textContent === "Edit") {
                this.textContent = "Close";
            } else {
                this.textContent = "Edit";
            }
        });
    });
})