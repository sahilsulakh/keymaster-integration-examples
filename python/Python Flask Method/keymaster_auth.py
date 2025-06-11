import requests
import json
import uuid
import os
import platform

# --- KeyMaster API Endpoints ---
LICENSE_AUTH_API_URL = "https://keymaster-agni.vercel.app/api/authenticate-key"
CLIENT_USER_AUTH_API_URL = "https://keymaster-agni.vercel.app/api/authenticate-client-user"

def get_persistent_hwid(app_name="StreamerPanel"):
    """
    Generate and persist a unique HWID based on UUID and store it across sessions.
    """
    system = platform.system()
    if system == "Windows":
        base_dir = os.getenv('APPDATA') or os.path.join(os.path.expanduser("~"), 'AppData', 'Roaming')
    elif system == "Darwin":
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
            with open(hwid_file_path, "r", encoding="utf-8") as f_in:
                hwid = f_in.read().strip()
                if hwid:
                    return hwid
    except Exception:
        pass

    # If not found, generate new HWID
    new_hwid = str(uuid.uuid4()).upper()
    try:
        with open(hwid_file_path, "w", encoding="utf-8") as f_out:
            f_out.write(new_hwid)
    except Exception:
        pass

    return new_hwid


def authenticate_license_key(master_user_id, license_key, hwid, app_version, username=None):
    """
    Authenticate a license key using KeyMaster API.
    """
    payload = {
        "masterUserId": master_user_id,
        "licenseKey": license_key.upper(),
        "hwid": hwid,
        "appVersion": app_version
    }
    if username:
        payload["username"] = username
    return _send_request(LICENSE_AUTH_API_URL, payload)


def authenticate_client_user(master_user_id, username, password_plain_text, app_version):
    """
    Authenticate a client user (username/password) using KeyMaster API.
    """
    payload = {
        "masterUserId": master_user_id,
        "username": username,
        "passwordPlainText": password_plain_text,
        "appVersion": app_version
    }
    return _send_request(CLIENT_USER_AUTH_API_URL, payload)


def _send_request(api_url, payload):
    """
    Internal helper to send POST request with proper headers.
    """
    headers = {"Content-Type": "application/json"}
    try:
        response = requests.post(api_url, data=json.dumps(payload), headers=headers, timeout=30)
        return response.json()
    except Exception as e:
        return {"success": False, "message": f"Request failed: {str(e)}"}
