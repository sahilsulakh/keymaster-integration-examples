# Python KeyMaster Auth Client
# (full code, see integration guide for details)
import requests
import json
import uuid
import os
import platform
API_BASE_URL = "https://keymaster-agni.vercel.app/api/authenticate-key"
def get_persistent_hwid(app_name="YourPythonApp"):
    if platform.system() == "Windows":
        base_dir = os.getenv('APPDATA') or os.path.join(os.path.expanduser("~"), 'AppData', 'Roaming')
    elif platform.system() == "Darwin":
        base_dir = os.path.join(os.path.expanduser("~"), 'Library', 'Application Support')
    else:
        base_dir = os.getenv('XDG_CONFIG_HOME') or os.path.join(os.path.expanduser("~"), '.config')
    safe_app_name = "".join(c if c.isalnum() else "_" for c in app_name)
    app_data_dir = os.path.join(base_dir, safe_app_name.lower())
    hwid_file_path = os.path.join(app_data_dir, "hwid.dat")
    try:
        if not os.path.exists(app_data_dir):
            os.makedirs(app_data_dir, exist_ok=True)
        if os.path.exists(hwid_file_path):
            with open(hwid_file_path, "r") as f_in:
                hwid = f_in.read().strip()
                if hwid:
                    return hwid
    except Exception as e:
        print(f"Error reading HWID file: {e}")
    new_hwid = str(uuid.uuid4()).upper()
    try:
        with open(hwid_file_path, "w") as f_out:
            f_out.write(new_hwid)
    except Exception as e:
        print(f"Error writing HWID file: {e}")
    return new_hwid
def authenticate_license(master_user_id, license_key, hwid, app_version, username=None):
    placeholder_uid = "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE"
    if not master_user_id or master_user_id == placeholder_uid:
        return {"success": False, "message": "Configuration Error: MasterUserId is not set in the Python application. This ID must be set by the application developer by replacing the placeholder.", "errorCode": "CLIENT_MISSING_MASTER_USER_ID"}
    if not license_key:
        return {"success": False, "message": "License Key cannot be empty.", "errorCode": "CLIENT_MISSING_LICENSE_KEY"}
    payload = {
        "masterUserId": master_user_id,
        "licenseKey": license_key.upper(),
        "hwid": hwid,
        "appVersion": app_version
    }
    if username:
        payload["username"] = username
    headers = {"Content-Type": "application/json"}
    print(f"Sending to KeyMaster: {json.dumps(payload)}")
    try:
        response = requests.post(API_BASE_URL, data=json.dumps(payload), headers=headers, timeout=30)
        print(f"KeyMaster Response Status: {response.status_code}")
        print(f"KeyMaster Response Body: {response.text}")
        return response.json()
    except requests.exceptions.Timeout:
        return {"success": False, "message": "The request to KeyMaster timed out.", "errorCode": "TIMEOUT_ERROR"}
    except requests.exceptions.JSONDecodeError:
        return {"success": False, "message": f"Error decoding KeyMaster API response. Status: {response.status_code}, Body: {response.text[:200]}...", "errorCode": "JSON_DECODE_ERROR"}
    except requests.exceptions.RequestException as e:
        return {"success": False, "message": f"Network or request error: {e}", "errorCode": "CLIENT_REQUEST_EXCEPTION"}
if __name__ == "__main__":
    MY_KEYMASTER_ACCOUNT_UID = "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE"
    if MY_KEYMASTER_ACCOUNT_UID == "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE":
        print("CRITICAL CONFIGURATION ERROR: MY_KEYMASTER_ACCOUNT_UID is not set.")
        print("Please open this script and replace the placeholder with your actual KeyMaster User ID.")
    else:
        my_license_key = input("Enter License Key (e.g., XXXX-XXXX-XXXX-XXXX): ").strip()
        my_app_version = "1.0.0"
        my_hwid = get_persistent_hwid("MyGreatPythonApp")
        my_username = input("Enter Username (optional, press Enter to skip): ").strip() or None
        print(f"\nUsing MasterUserId: {MY_KEYMASTER_ACCOUNT_UID}")
        print(f"Using HWID: {my_hwid}")
        print(f"Authenticating key: {my_license_key} for App v{my_app_version}...")
        result = authenticate_license(MY_KEYMASTER_ACCOUNT_UID, my_license_key, my_hwid, my_app_version, my_username)
        print("\n--- Authentication Result ---")
        if result and result.get("success"):
            print(f"Status: Successful, Message: {result.get('message')}, User Status: {result.get('userStatus', 'N/A')}")
            print("\nApplication features unlocked!")
        else:
            print(f"Status: Failed, Message: {result.get('message', 'N/A')}, Error Code: {result.get('errorCode', 'N/A')}, User Status: {result.get('userStatus', 'N/A')}")
            print("\nApplication access denied.")

