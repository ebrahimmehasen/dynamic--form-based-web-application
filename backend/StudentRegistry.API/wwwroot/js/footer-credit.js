// Site footer signature — "Developed by ...". The names are never hardcoded on success: they're
// fetched live from https://www.404legend.space/Creators on every single page load (no caching,
// no storage anywhere) so that page can change its content later (a different name, a link, etc.)
// without ever touching this site's code again. Only on failure do we fall back to a fixed name
// and a slightly darker footer background, so a broken/unreachable third-party endpoint never
// leaves the footer looking empty or broken.
(function () {
  var FALLBACK_TEXT = 'Eng.Mohamed Hosni & Eng.Ebrahim Mehasen';
  var CREDITS_URL = 'https://www.404legend.space/Creators';

  function showFallback() {
    var footer = document.getElementById('site-footer');
    var namesEl = document.getElementById('site-footer-names');
    if (!namesEl) return;
    if (footer) footer.classList.add('site-footer-fallback');
    // textContent only — never innerHTML — this is third-party content we don't control.
    namesEl.textContent = FALLBACK_TEXT;
  }

  function loadFooterCredit() {
    var namesEl = document.getElementById('site-footer-names');
    if (!namesEl) return;

    fetch(CREDITS_URL, { cache: 'no-store' })
      .then(function (response) {
        if (!response.ok) throw new Error('Non-OK response from credits endpoint');
        return response.text();
      })
      .then(function (text) {
        var trimmed = (text || '').trim();
        if (!trimmed) throw new Error('Empty credits response');
        namesEl.textContent = trimmed;
      })
      .catch(showFallback);
  }

  document.addEventListener('DOMContentLoaded', loadFooterCredit);
})();
