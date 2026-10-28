

(() => {
    const form = document.getElementById("emailForm");
    if (!form) return;

    const status = document.getElementById("emailFormStatus");
    const fallbackLink = document.getElementById("emailFallbackLink");
    const submitButton = document.getElementById("sendEmailButton");
    const defaultButtonHtml = submitButton.innerHTML;

    const EMAILJS_PUBLIC_KEY = "IZG6gUVaufWZ5a6ll";
    const EMAILJS_SERVICE_ID = "service_pb1dtfx";
    const EMAILJS_TEMPLATE_ID = "template_3sn7g48";
    const ADMIN_EMAIL = "kerilesaziz12@gmail.com";

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        if (!form.reportValidity()) return;

        submitButton.disabled = true;
        submitButton.textContent = "Sending...";
        status.textContent = "Sending your message...";
        status.dataset.state = "sending";
        fallbackLink.hidden = true;

        try {
            if (!window.emailjs) {
                throw new Error("EmailJS did not load. Check your internet connection or ad blocker.");
            }

            await window.emailjs.sendForm(
                EMAILJS_SERVICE_ID,
                EMAILJS_TEMPLATE_ID,
                form,
                { publicKey: EMAILJS_PUBLIC_KEY }
            );

            status.textContent = "Message sent successfully!";
            status.dataset.state = "success";
            form.reset();
        } catch (error) {
            console.error("EmailJS complaint send failed:", error?.text || error?.message || error);
            status.textContent = "We could not send the message. You can use your email app instead.";
            status.dataset.state = "error";

            const data = new FormData(form);
            const body = `From: ${data.get("name")}\nReply-to: ${data.get("email")}\n\n${data.get("message")}`;
            fallbackLink.href = `mailto:${ADMIN_EMAIL}?subject=${encodeURIComponent(data.get("subject") || "")}&body=${encodeURIComponent(body)}`;
            fallbackLink.hidden = false;
        } finally {
            submitButton.disabled = false;
            submitButton.innerHTML = defaultButtonHtml;
        }
    });
})();