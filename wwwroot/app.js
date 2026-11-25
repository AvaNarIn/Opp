const loginForm = document.querySelector("#login-form");
const passwordInput = document.querySelector("#password");
const togglePasswordButton = document.querySelector("#toggle-password");

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

if (loginForm) {
    loginForm.addEventListener("submit", login);
} else {
    loadCurrentUser();
}

document.querySelectorAll(".logout-link").forEach(function (link) {
    link.addEventListener("click", function (event) {
        event.preventDefault();

        sessionStorage.removeItem("access_token");
        window.location.replace("index.html");
    });
});

async function login(event) {
    event.preventDefault();

    const emailInput = document.querySelector("#email");
    const errorMessage = document.querySelector("#login-error");
    const submitButton = loginForm.querySelector('button[type="submit"]');

    errorMessage.hidden = true;
    submitButton.disabled = true;
    submitButton.textContent = "Входим…";

    try {
        const response = await fetch("/api/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                email: emailInput.value.trim(),
                password: passwordInput.value
            })
        });

        if (response.status === 401) {
            errorMessage.textContent = "Неверная почта или пароль.";
            errorMessage.hidden = false;
            return;
        }

        if (response.status === 400) {
            errorMessage.textContent = "Проверьте правильность заполнения полей.";
            errorMessage.hidden = false;
            return;
        }

        if (!response.ok) {
            errorMessage.textContent = "Не удалось войти. Попробуйте позже.";
            errorMessage.hidden = false;
            return;
        }

        const data = await response.json();

        if (!data.access_token) {
            errorMessage.textContent = "Сервер не вернул токен входа.";
            errorMessage.hidden = false;
            return;
        }

        sessionStorage.setItem("access_token", data.access_token);

        window.location.replace("projects.html");
    } catch {
        errorMessage.textContent = "Не удалось связаться с сервером.";
        errorMessage.hidden = false;
    } finally {
        submitButton.disabled = false;
        submitButton.textContent = "Войти";
    }
}

async function loadCurrentUser() {
    const token = sessionStorage.getItem("access_token");

    if (!token) {
        window.location.replace("index.html");
        return;
    }

    try {
        const response = await fetch("/api/me", {
            headers: {
                Authorization: `Bearer ${token}`
            }
        });

        if (response.status === 401) {
            sessionStorage.removeItem("access_token");
            window.location.replace("index.html");
            return;
        }

        if (!response.ok) {
            showUserError();
            return;
        }

        const user = await response.json();

        const userName = document.querySelector(".user-name");
        const avatar = document.querySelector(".avatar");
        const roleLabel = document.querySelector(".user-info .muted");

        if (userName) {
            userName.textContent = user.name;
        }

        if (avatar) {
            avatar.textContent = user.name
                .trim()
                .split(/\s+/)
                .slice(0, 2)
                .map(part => part.charAt(0))
                .join("")
                .toUpperCase();
        }

        if (roleLabel) {
            roleLabel.textContent = user.system_role === "Admin"
                ? "Администратор системы"
                : "Пользователь";
        }
    } catch {
        showUserError();
    }
}

function showUserError() {
    const container = document.querySelector(".page-content");

    if (container) {
        const message = document.createElement("p");
        message.className = "request-error";
        message.setAttribute("role", "alert");
        message.textContent =
            "Не удалось загрузить пользователя. Обновите страницу.";

        container.prepend(message);
    }
}