(function () {
  function setMode(mode) {
    var tabs = document.querySelectorAll('[data-auth-tab]');
    var panels = document.querySelectorAll('[data-auth-panel]');

    tabs.forEach(function (t) {
      var isActive = t.getAttribute('data-auth-tab') === mode;
      t.classList.toggle('is-active', isActive);
      t.setAttribute('aria-selected', isActive ? 'true' : 'false');
    });

    panels.forEach(function (p) {
      var isActive = p.getAttribute('data-auth-panel') === mode;
      p.classList.toggle('is-active', isActive);
    });
  }

  document.addEventListener('click', function (e) {
    var tab = e.target && e.target.closest && e.target.closest('[data-auth-tab]');
    if (tab) {
      e.preventDefault();
      setMode(tab.getAttribute('data-auth-tab'));
      return;
    }

    var link = e.target && e.target.closest && e.target.closest('[data-auth-switch]');
    if (link) {
      e.preventDefault();
      setMode(link.getAttribute('data-auth-switch'));
    }
  });
})();
