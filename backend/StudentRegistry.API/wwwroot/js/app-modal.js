// Generic promise-based confirmation/input modal (replaces alert()/confirm()).
// variant: 'primary' | 'warning' | 'danger'. Pass focusInputId to collect a text value.
function showAppModal({ title, bodyHtml, confirmText = 'تأكيد', cancelText = 'إلغاء', variant = 'primary', focusInputId = null }) {
  return new Promise((resolve) => {
    const overlay = document.getElementById('app-modal-overlay');
    const box = document.getElementById('app-modal-box');
    const titleEl = document.getElementById('app-modal-title');
    const bodyEl = document.getElementById('app-modal-body');
    const confirmBtn = document.getElementById('app-modal-confirm');
    const cancelBtn = document.getElementById('app-modal-cancel');
    if (!overlay || !box || !titleEl || !bodyEl || !confirmBtn || !cancelBtn) {
      resolve({ confirmed: false, value: null });
      return;
    }

    titleEl.textContent = title;
    bodyEl.innerHTML = bodyHtml;
    confirmBtn.textContent = confirmText;
    cancelBtn.textContent = cancelText;
    box.className = 'app-modal-box app-modal-' + variant;

    overlay.style.display = 'flex';

    const cleanup = () => {
      overlay.style.display = 'none';
      confirmBtn.removeEventListener('click', onConfirm);
      cancelBtn.removeEventListener('click', onCancel);
      overlay.removeEventListener('click', onOverlayClick);
      document.removeEventListener('keydown', onKeydown);
    };

    const onConfirm = () => {
      const inputEl = focusInputId ? document.getElementById(focusInputId) : null;
      const value = inputEl ? inputEl.value : null;
      cleanup();
      resolve({ confirmed: true, value });
    };
    const onCancel = () => {
      cleanup();
      resolve({ confirmed: false, value: null });
    };
    const onOverlayClick = (e) => {
      if (e.target === overlay) onCancel();
    };
    const onKeydown = (e) => {
      if (e.key === 'Escape') onCancel();
      if (e.key === 'Enter' && focusInputId) onConfirm();
    };

    confirmBtn.addEventListener('click', onConfirm);
    cancelBtn.addEventListener('click', onCancel);
    overlay.addEventListener('click', onOverlayClick);
    document.addEventListener('keydown', onKeydown);

    if (focusInputId) {
      setTimeout(() => {
        const el = document.getElementById(focusInputId);
        if (el) el.focus();
      }, 50);
    }
  });
}

function showToast(message, variant = 'success') {
  const container = document.getElementById('app-toast-container');
  if (!container) return;
  const toast = document.createElement('div');
  toast.className = 'app-toast app-toast-' + variant;
  toast.textContent = message;
  container.appendChild(toast);
  setTimeout(() => {
    toast.classList.add('app-toast-hide');
    setTimeout(() => toast.remove(), 300);
  }, 3000);
}
