
const MY_KEYMASTER_ACCOUNT_UID = "ellRHrNSMrcAvtnKsvHVqyOvgbT2"; // <<<--- REPLACE WITH YOUR ACTUAL MASTER ID FROM THE KEYMASTER PROFILE
const MY_APP_VERSION = "1.0.0";
const APP_NAME = "MyCoolWebApp";

// API Endpoints
const LICENSE_AUTH_API_URL = "https://keymaster-agni.vercel.app/api/authenticate-key";
const CLIENT_USER_AUTH_API_URL = "https://keymaster-agni.vercel.app/api/authenticate-client-user";

// Generate and store persistent HWID (similar to Python version)
function getPersistentHWID(appName) {
    const storageKey = `hwid_${appName.toLowerCase().replace(/[^a-z0-9]/g, '_')}`;
    let hwid = localStorage.getItem(storageKey);
    
    if (!hwid) {
        hwid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            const r = Math.random() * 16 | 0;
            const v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        }).toUpperCase();
        
        localStorage.setItem(storageKey, hwid);
    }
    
    return hwid;
}

// Initialize HWID
const myHWID = getPersistentHWID(APP_NAME);
document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('hwid-display').textContent = myHWID;
});

// Tab switching functionality
function switchTab(tabType) {
    // Update tab buttons
    document.querySelectorAll('.tab-button').forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');
    
    // Update tab content
    document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
    document.getElementById(tabType + '-tab').classList.add('active');
    
    // Clear any previous messages
    hideMessage();
}

// API request helper function
async function sendRequest(apiUrl, payload) {
    try {
        const response = await fetch(apiUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(payload)
        });
        
        return await response.json();
    } catch (error) {
        return {
            success: false,
            message: `Network error: ${error.message}`
        };
    }
}

// License key authentication
async function authenticateLicenseKey(masterUserId, licenseKey, hwid, appVersion, username = null) {
    const payload = {
        masterUserId: masterUserId,
        licenseKey: licenseKey.toUpperCase(),
        hwid: hwid,
        appVersion: appVersion
    };
    
    if (username && username.trim()) {
        payload.username = username.trim();
    }
    
    return await sendRequest(LICENSE_AUTH_API_URL, payload);
}

// Username/password authentication
async function authenticateClientUser(masterUserId, username, passwordPlainText, appVersion) {
    const payload = {
        masterUserId: masterUserId,
        username: username,
        passwordPlainText: passwordPlainText,
        appVersion: appVersion
    };
    
    return await sendRequest(CLIENT_USER_AUTH_API_URL, payload);
}

// Show loading state
function showLoading(button) {
    button.disabled = true;
    button.querySelector('.loading-spinner').style.display = 'inline-block';
    button.querySelector('.btn-text').textContent = 'Authenticating...';
}

// Hide loading state
function hideLoading(button, originalText) {
    button.disabled = false;
    button.querySelector('.loading-spinner').style.display = 'none';
    button.querySelector('.btn-text').textContent = originalText;
}

// Show message
function showMessage(message, isSuccess) {
    const messageEl = document.getElementById('message');
    messageEl.textContent = message;
    messageEl.className = `message ${isSuccess ? 'success' : 'error'}`;
    messageEl.style.display = 'block';
}

// Hide message
function hideMessage() {
    document.getElementById('message').style.display = 'none';
}

// Handle successful authentication
function handleAuthSuccess(response, authType) {
    showMessage(`${authType} Success! ${response.message}`, true);
    
    // Here you can redirect the user or perform other actions
    // Example: window.location.href = '/dashboard';
    
    console.log('Authentication successful:', response);
}

// Handle failed authentication
function handleAuthFailure(response, authType) {
    showMessage(`${authType} Failed: ${response.message || 'Unknown error'}`, false);
    console.error('Authentication failed:', response);
}

// License form submission
document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('license-form').addEventListener('submit', async function(e) {
        e.preventDefault();
        
        const button = this.querySelector('.login-btn');
        const originalText = 'Authenticate with License Key';
        
        const licenseKey = document.getElementById('license-key').value.trim();
        const username = document.getElementById('license-username').value.trim() || null;
        
        if (!licenseKey) {
            showMessage('Please enter a license key', false);
            return;
        }
        
        showLoading(button);
        hideMessage();
        
        try {
            const response = await authenticateLicenseKey(
                MY_KEYMASTER_ACCOUNT_UID,
                licenseKey,
                myHWID,
                MY_APP_VERSION,
                username
            );
            
            if (response && response.success) {
                handleAuthSuccess(response, 'License Key Authentication');
            } else {
                handleAuthFailure(response, 'License Key Authentication');
            }
        } catch (error) {
            handleAuthFailure({ message: error.message }, 'License Key Authentication');
        } finally {
            hideLoading(button, originalText);
        }
    });

    // User form submission
    document.getElementById('user-form').addEventListener('submit', async function(e) {
        e.preventDefault();
        
        const button = this.querySelector('.login-btn');
        const originalText = 'Login with Username/Password';
        
        const username = document.getElementById('user-username').value.trim();
        const password = document.getElementById('user-password').value;
        
        if (!username || !password) {
            showMessage('Please enter both username and password', false);
            return;
        }
        
        showLoading(button);
        hideMessage();
        
        try {
            const response = await authenticateClientUser(
                MY_KEYMASTER_ACCOUNT_UID,
                username,
                password,
                MY_APP_VERSION
            );
            
            if (response && response.success) {
                handleAuthSuccess(response, 'User Authentication');
            } else {
                handleAuthFailure(response, 'User Authentication');
            }
        } catch (error) {
            handleAuthFailure({ message: error.message }, 'User Authentication');
        } finally {
            hideLoading(button, originalText);
        }
    });

    // Initialize the page
    console.log('KeyMaster Authentication System Initialized');
    console.log('Hardware ID:', myHWID);
    
    // Check if KeyMaster account UID is configured
    if (MY_KEYMASTER_ACCOUNT_UID === "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE")     // <<<-- DO NOT CHANGE IT!! IT IS FOR CHECKING PURPOSES.
    {
        showMessage('Please configure your KeyMaster Account UID in the script', false);
    }
});
