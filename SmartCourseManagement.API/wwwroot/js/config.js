/**
 * config.js - API configuration and application constants
 */

const CONFIG = {
    API_BASE: '/api/v1',
    TOKEN_KEY: 'scmp_token',
    REFRESH_TOKEN_KEY: 'scmp_refresh_token',
    USER_KEY: 'scmp_user',
    THEME_KEY: 'scmp_theme',
    PAGE_SIZE: 10,
    DEBOUNCE_DELAY: 300,
    TOAST_DURATION: 4000,
};

// Freeze the config to prevent accidental mutations
Object.freeze(CONFIG);
