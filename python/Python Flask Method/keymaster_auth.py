import requests
import json
import uuid
import os
import platform
import hashlib
import time
import logging
from typing import Dict, Optional, Any

# Configure logging
logger = logging.getLogger(__name__)

# --- KeyMaster API Endpoints ---
LICENSE_AUTH_API_URL = "https://keymaster-agni.vercel.app/api/authenticate-key"
CLIENT_USER_AUTH_API_URL = "https://keymaster-agni.vercel.app/api/authenticate-client-user"

# Request timeout and retry settings
REQUEST_TIMEOUT = 30
MAX_RETRIES = 3
RETRY_DELAY = 1  # seconds

class AuthenticationError(Exception):
    """Custom exception for authentication errors"""
    pass

class NetworkError(Exception):
    """Custom exception for network-related errors"""
    pass

def get_persistent_hwid(app_name: str = "StreamerPanel") -> str:
    """
    Generate and persist a unique HWID based on UUID and store it across sessions.
    
    Args:
        app_name: Name of the application for storage directory
        
    Returns:
        Persistent hardware ID string
        
    Raises:
        OSError: If unable to create directories or files
    """
    try:
        system = platform.system()
        
        # Determine base directory based on OS
        if system == "Windows":
            base_dir = os.getenv('APPDATA') or os.path.join(os.path.expanduser("~"), 'AppData', 'Roaming')
        elif system == "Darwin":
            base_dir = os.path.join(os.path.expanduser("~"), 'Library', 'Application Support')
        else:  # Linux and other Unix-like systems
            base_dir = os.getenv('XDG_CONFIG_HOME') or os.path.join(os.path.expanduser("~"), '.config')
        
        # Create safe app name for directory
        safe_app_name = "".join(c if c.isalnum() else "_" for c in app_name)
        app_data_dir = os.path.join(base_dir, safe_app_name.lower())
        hwid_file_path = os.path.join(app_data_dir, "hwid.dat")
        
        # Try to create directory if it doesn't exist
        os.makedirs(app_data_dir, exist_ok=True)
        
        # Try to read existing HWID
        if os.path.exists(hwid_file_path):
            try:
                with open(hwid_file_path, "r", encoding="utf-8") as f_in:
                    hwid = f_in.read().strip()
                    if hwid and len(hwid) == 36:  # Valid UUID length
                        logger.debug(f"Retrieved existing HWID from {hwid_file_path}")
                        return hwid
            except (IOError, OSError) as e:
                logger.warning(f"Could not read existing HWID file: {e}")
        
        # Generate new HWID if not found or invalid
        new_hwid = str(uuid.uuid4()).upper()
        
        # Try to save new HWID
        try:
            with open(hwid_file_path, "w", encoding="utf-8") as f_out:
                f_out.write(new_hwid)
            logger.info(f"Generated and saved new HWID to {hwid_file_path}")
        except (IOError, OSError) as e:
            logger.warning(f"Could not save HWID to file: {e}")
        
        return new_hwid
        
    except Exception as e:
        logger.error(f"Error generating HWID: {e}")
        # Fallback: generate UUID based on system info
        system_info = f"{platform.system()}-{platform.node()}-{os.getlogin() if hasattr(os, 'getlogin') else 'unknown'}"
        fallback_hwid = str(uuid.uuid5(uuid.NAMESPACE_DNS, system_info)).upper()
        logger.warning(f"Using fallback HWID generation: {fallback_hwid}")
        return fallback_hwid

def authenticate_license_key(master_user_id: str, license_key: str, hwid: str, 
                           app_version: str, username: Optional[str] = None) -> Dict[str, Any]:
    """
    Authenticate a license key using KeyMaster API.
    
    Args:
        master_user_id: Master user ID from KeyMaster
        license_key: License key to authenticate
        hwid: Hardware ID
        app_version: Application version
        username: Optional username
        
    Returns:
        Dictionary containing authentication result
        
    Raises:
        AuthenticationError: If authentication fails
        NetworkError: If network request fails
    """
    if not all([master_user_id, license_key, hwid, app_version]):
        raise AuthenticationError("Missing required parameters for license authentication")
    
    payload = {
        "masterUserId": master_user_id.strip(),
        "licenseKey": license_key.upper().strip(),
        "hwid": hwid.strip(),
        "appVersion": app_version.strip()
    }
    
    if username:
        payload["username"] = username.strip()
    
    logger.info(f"Authenticating license key: {license_key[:8]}...")
    return _send_request(LICENSE_AUTH_API_URL, payload)

def authenticate_client_user(master_user_id: str, username: str, 
                           password_plain_text: str, app_version: str) -> Dict[str, Any]:
    """
    Authenticate a client user (username/password) using KeyMaster API.
    
    Args:
        master_user_id: Master user ID from KeyMaster
        username: Username to authenticate
        password_plain_text: Plain text password
        app_version: Application version
        
    Returns:
        Dictionary containing authentication result
        
    Raises:
        AuthenticationError: If authentication fails
        NetworkError: If network request fails
    """
    if not all([master_user_id, username, password_plain_text, app_version]):
        raise AuthenticationError("Missing required parameters for user authentication")
    
    # Input validation
    if len(username) > 50:
        raise AuthenticationError("Username too long")
    
    if len(password_plain_text) > 100:
        raise AuthenticationError("Password too long")
    
    payload = {
        "masterUserId": master_user_id.strip(),
        "username": username.strip(),
        "passwordPlainText": password_plain_text,
        "appVersion": app_version.strip()
    }
    
    logger.info(f"Authenticating user: {username}")
    return _send_request(CLIENT_USER_AUTH_API_URL, payload)

def _send_request(api_url: str, payload: Dict[str, Any]) -> Dict[str, Any]:
    """
    Internal helper to send POST request with proper headers and retry logic.
    
    Args:
        api_url: API endpoint URL
        payload: Request payload
        
    Returns:
        Response dictionary
        
    Raises:
        NetworkError: If all retry attempts fail
    """
    headers = {
        "Content-Type": "application/json",
        "User-Agent": "KeyMaster-Client/1.0",
        "Accept": "application/json"
    }
    
    for attempt in range(MAX_RETRIES):
        try:
            logger.debug(f"Sending request to {api_url} (attempt {attempt + 1}/{MAX_RETRIES})")
            
            response = requests.post(
                api_url, 
                data=json.dumps(payload), 
                headers=headers, 
                timeout=REQUEST_TIMEOUT,
                verify=True  # Verify SSL certificates
            )
            
            # Log response status
            logger.debug(f"Response status: {response.status_code}")
            
            # Check if response is JSON
            try:
                result = response.json()
            except json.JSONDecodeError:
                logger.error(f"Invalid JSON response from API: {response.text}")
                raise NetworkError("Invalid response format from authentication server")
            
            # Check for HTTP errors
            if response.status_code >= 400:
                error_msg = result.get('message', f'HTTP {response.status_code} error')
                logger.warning(f"API returned error: {error_msg}")
                return {"success": False, "message": error_msg}
            
            return result
            
        except requests.exceptions.Timeout:
            logger.warning(f"Request timeout (attempt {attempt + 1}/{MAX_RETRIES})")
            if attempt == MAX_RETRIES - 1:
                raise NetworkError("Authentication server timeout")
                
        except requests.exceptions.ConnectionError:
            logger.warning(f"Connection error (attempt {attempt + 1}/{MAX_RETRIES})")
            if attempt == MAX_RETRIES - 1:
                raise NetworkError("Cannot connect to authentication server")
                
        except requests.exceptions.RequestException as e:
            logger.error(f"Request failed: {str(e)}")
            if attempt == MAX_RETRIES - 1:
                raise NetworkError(f"Request failed: {str(e)}")
                
        except Exception as e:
            logger.error(f"Unexpected error during request: {str(e)}")
            if attempt == MAX_RETRIES - 1:
                raise NetworkError(f"Unexpected error: {str(e)}")
        
        # Wait before retry
        if attempt < MAX_RETRIES - 1:
            time.sleep(RETRY_DELAY * (attempt + 1))  # Exponential backoff
    
    raise NetworkError("All retry attempts failed")

def get_system_info() -> Dict[str, str]:
    """
    Get system information for debugging purposes.
    
    Returns:
        Dictionary containing system information
    """
    return {
        "system": platform.system(),
        "platform": platform.platform(),
        "architecture": platform.architecture()[0],
        "python_version": platform.python_version(),
        "node": platform.node()
    }
