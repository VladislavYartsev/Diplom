namespace OnlineAPI.wwwroot.js
{
    document.addEventListener("DOMContentLoaded", function () {
        const form = document.getElementById("codeForm");
        if (!form) return;

        form.addEventListener("submit", function (e) {
            e.preventDefault();

            const code = document.getElementById("codeInput").value;

            fetch("/Auth/CheckCode", {
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded"
                },
                body: "code=" + encodeURIComponent(code)
            })
                .then(r => {
                    if (!r.ok) throw new Error("HTTP " + r.status);
                    return r.json();
                })
                .then(d => {
                    if (!d.success) {
                        document.getElementById("codeError").innerText = d.message;
                        return;
                    }

                    document.getElementById("title").innerText = "Регистрация";
                    document.getElementById("codeForm").style.display = "none";
                    document.getElementById("registerForm").style.display = "block";
                    document.getElementById("hiddenCode").value = code;
                })
                .catch(err => {
                    console.error(err);
                    document.getElementById("codeError").innerText = "Ошибка проверки кода";
                });
        });
    });

}
