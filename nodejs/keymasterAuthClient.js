// Node.js KeyMaster Auth Client
// (full code, see integration guide for details)
const axios = require('axios');
const crypto = require('crypto');
const fs = require('fs').promises;
const path = require('path');
const os = require('os');
const API_BASE_URL = "https://keymaster-agni.vercel.app/api/authenticate-key";
async function getPersistentHwid(appName = "YourNodeApp") {
    let baseDir;
    const platform = os.platform();
    if (platform === 'win32') {
        baseDir = process.env.APPDATA || path.join(os.homedir(), 'AppData', 'Roaming');
    } else if (platform === 'darwin') {
        baseDir = path.join(os.homedir(), 'Library', 'Application Support');
    } else {
        baseDir = process.env.XDG_CONFIG_HOME || path.join(os.homedir(), '.config');
    }
    const safeAppName = appName.replace(/[^a-zA-Z0-9]/g, '_');
    const appDataDir = path.join(baseDir, safeAppName.toLowerCase());
    const hwidFilePath = path.join(appDataDir, "hwid.dat");
    try {
        await fs.mkdir(appDataDir, { recursive: true });
        const hwid = await fs.readFile(hwidFilePath, 'utf-8');
        if (hwid.trim()) return hwid.trim();
    } catch (readError) {
        if (readError.code !== 'ENOENT') console.error("Error reading HWID file:", readError.message);
    }
    const newHwid = crypto.randomUUID().toUpperCase();
    try {
        await fs.writeFile(hwidFilePath, newHwid);
    } catch (writeError) {
        console.error("Error writing HWID file:", writeError.message);
    }
    return newHwid;
}
async function authenticateLicense(masterUserId, licenseKey, hwid, appVersion, username = null) {
    const placeholderUid = "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE";
    if (!masterUserId || masterUserId === placeholderUid) {
        console.error("Configuration Error: MasterUserId is not set. Replace placeholder.");
        return { success: false, message: "Configuration Error: MasterUserId is not set. Replace placeholder.", errorCode: "CLIENT_MISSING_MASTER_USER_ID" };
    }
    if (!licenseKey) {
        return { success: false, message: "License Key cannot be empty.", errorCode: "CLIENT_MISSING_LICENSE_KEY" };
    }
    const payload = { masterUserId, licenseKey: licenseKey.toUpperCase(), hwid, appVersion };
    if (username) payload.username = username;
    console.log(`Sending to KeyMaster: ${JSON.stringify(payload)}`);
    try {
        const response = await axios.post(API_BASE_URL, payload, {
            headers: { 'Content-Type': 'application/json' },
            timeout: 30000,
        });
        console.log(`KeyMaster Response Status: ${response.status}, Body: ${JSON.stringify(response.data)}`);
        return response.data;
    } catch (error) {
        if (error.response) {
            console.error(`KeyMaster API Error (${error.response.status}):`, error.response.data || error.response.statusText);
            return error.response.data || { success: false, message: `API Error: ${error.response.status} - ${error.response.statusText}`, errorCode: `HTTP_${error.response.status}` };
        } else if (error.request) {
            console.error("Network Error (no response from KeyMaster):", error.message);
            return { success: false, message: "Network error: No response from KeyMaster. Please check your internet connection and the API URL.", errorCode: "NETWORK_NO_RESPONSE" };
        } else {
            console.error("Error setting up KeyMaster request:", error.message);
            return { success: false, message: "Request setup error: " + error.message, errorCode: "CLIENT_REQUEST_SETUP_ERROR" };
        }
    }
}
async function main() {
    const MY_KEYMASTER_ACCOUNT_UID = "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE";
    if (MY_KEYMASTER_ACCOUNT_UID === "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE") {
        console.error("CRITICAL CONFIGURATION ERROR: MY_KEYMASTER_ACCOUNT_UID is not set.");
        console.error("Please open this script and replace the placeholder with your actual KeyMaster User ID.");
        return;
    }
    const myLicenseKey = process.argv[2];
    const myUsername = process.argv[3] || null;
    if (!myLicenseKey) {
        console.log("Usage: node keymasterAuthClient.js <YOUR_LICENSE_KEY> [YOUR_USERNAME]");
        return;
    }
    const myAppVersion = "1.0.0";
    const myHwid = await getPersistentHwid("MyCoolNodeApp");
    console.log(`\nUsing MasterUserId: ${MY_KEYMASTER_ACCOUNT_UID}, HWID: ${myHwid}`);
    console.log(`Authenticating key: ${myLicenseKey} for App v${myAppVersion}...`);
    const result = await authenticateLicense(MY_KEYMASTER_ACCOUNT_UID, myLicenseKey, myHwid, myAppVersion, myUsername);
    console.log("\n--- Authentication Result ---");
    if (result && result.success) {
        console.log(`Status: Successful, Message: ${result.message}, User Status: ${result.userStatus || 'N/A'}`);
        console.log("\nApplication features unlocked!");
    } else {
        console.log(`Status: Failed, Message: ${result.message || 'N/A'}, Error Code: ${result.errorCode || 'N/A'}, User Status: ${result.userStatus || 'N/A'}`);
        console.log("\nApplication access denied.");
    }
}
if (require.main === module) {
  main();
}
module.exports = { authenticateLicense, getPersistentHwid };

