(function (window) {
    'use strict';

    var STORAGE_KEY = 'cm-theme';

    function getPreference() {
        var stored = localStorage.getItem(STORAGE_KEY);
        return stored === 'light' || stored === 'dark' || stored === 'auto' ? stored : 'auto';
    }

    function resolveTheme(preference) {
        if (preference === 'dark' || preference === 'light')
            return preference;
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    function applyResolvedTheme(resolved) {
        document.documentElement.setAttribute('data-theme', resolved);
        document.documentElement.style.colorScheme = resolved;
    }

    function applyPreference(preference) {
        var resolved = resolveTheme(preference);
        applyResolvedTheme(resolved);
        return resolved;
    }

    function setPreference(preference) {
        if (preference !== 'light' && preference !== 'dark' && preference !== 'auto')
            preference = 'auto';
        localStorage.setItem(STORAGE_KEY, preference);
        return applyPreference(preference);
    }

    function init() {
        var preference = getPreference();
        applyPreference(preference);

        if (window.matchMedia) {
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function () {
                if (getPreference() === 'auto')
                    applyPreference('auto');
            });
        }
    }

    window.CloudMexTheme = {
        getPreference: getPreference,
        setPreference: setPreference,
        resolveTheme: resolveTheme,
        applyPreference: applyPreference,
        init: init
    };

    if (document.readyState === 'loading')
        document.addEventListener('DOMContentLoaded', init);
    else
        init();
})(window);
