const loginForm = document.querySelector("#login-form");
const passwordInput = document.querySelector("#password");
const togglePasswordButton = document.querySelector("#toggle-password");

if (loginForm) {
    loginForm.addEventListener("submit", function (event) {
        event.preventDefault();

        window.location.href = "projects.html";
    });
}

if (togglePasswordButton && passwordInput) {
    togglePasswordButton.addEventListener("click", function () {
        const showPassword = passwordInput.type === "password";

        passwordInput.type = showPassword ? "text" : "password";
        togglePasswordButton.textContent = showPassword
            ? "Скрыть"
            : "Показать";

        togglePasswordButton.setAttribute(
            "aria-pressed",
            String(showPassword)
        );
    });
}