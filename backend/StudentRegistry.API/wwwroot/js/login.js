// Login page controller.
//
// Security notes:
// - Username/password are read directly from input.value and never re-rendered into the DOM
//   (no innerHTML anywhere in this file) — the only user-visible output is the server's own
//   plain-text message, written via textContent.
// - Credentials are never written to localStorage/sessionStorage, never put in the URL/query
//   string, never logged to the console, and only ever sent in a POST request body.
// - No client-side validation or comparison of the username/password is performed here — that is
//   entirely the backend's responsibility once real authentication is implemented.

// Thin, swappable API client. Everything else in this file talks to auth *through* this object,
// so replacing the placeholder backend later (real endpoint, tokens, MFA challenge, etc.) means
// changing only this function — not the form-handling code below.
const AuthApiClient = {
  async login(credentials) {
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]');

    const response = await fetch('/api/auth/login', {
      method: 'POST',
      credentials: 'same-origin',
      headers: {
        'Content-Type': 'application/json',
        ...(csrfToken ? { 'RequestVerificationToken': csrfToken.value } : {})
      },
      body: JSON.stringify(credentials)
    });

    let data = null;
    try {
      data = await response.json();
    } catch {
      data = null;
    }

    return { status: response.status, data };
  }
};

function showLoginAlert(message, type) {
  const alertEl = document.getElementById('login-alert');
  if (!alertEl) return;
  alertEl.textContent = message;
  alertEl.className = 'alert ' + (type === 'success' ? 'alert-success' : 'alert-danger');
  alertEl.style.display = 'flex';
}

function hideLoginAlert() {
  const alertEl = document.getElementById('login-alert');
  if (!alertEl) return;
  alertEl.style.display = 'none';
}

function initLoginForm() {
  const form = document.getElementById('login-form');
  const usernameInput = document.getElementById('login-username');
  const passwordInput = document.getElementById('login-password');
  const submitBtn = document.getElementById('login-submit-btn');
  if (!form || !usernameInput || !passwordInput || !submitBtn) return;

  form.addEventListener('submit', async function (e) {
    e.preventDefault();
    hideLoginAlert();

    // Collected as-is — untrusted data, forwarded to the backend without any local checks.
    const credentials = {
      username: usernameInput.value,
      password: passwordInput.value
    };

    const originalText = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.textContent = 'جاري تسجيل الدخول...';

    try {
      const { status, data } = await AuthApiClient.login(credentials);

      if (status === 501) {
        // Expected while the real backend isn't wired up yet.
        showLoginAlert((data && data.message) || 'تسجيل الدخول غير مفعل بعد.', 'danger');
      } else if (data && data.success) {
        showLoginAlert(data.message || 'تم تسجيل الدخول بنجاح.', 'success');
      } else {
        showLoginAlert((data && data.message) || 'تعذر تسجيل الدخول. الرجاء المحاولة مرة أخرى.', 'danger');
      }
    } catch {
      showLoginAlert('تعذر الاتصال بالخادم. الرجاء المحاولة مرة أخرى.', 'danger');
    } finally {
      submitBtn.disabled = false;
      submitBtn.textContent = originalText;
      passwordInput.value = '';
    }
  });
}

document.addEventListener('DOMContentLoaded', initLoginForm);
