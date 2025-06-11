KeyMaster Python + Flask Web Integration

A secure, production-ready Python + Flask-based login system for authenticating users via the KeyMaster AGNI API.

This project features:

- 🧔 Secure username/password authentication
- 💻 Persistent HWID generation per user
- ⚡ Responsive, animated HTML/CSS frontend
- 🔒 Hardened Flask backend with security best practices
- 🛡️ Security headers, session management, and logging
- 🔁 All-in-one login and dashboard UI (no separate dashboard/auth_result files)

------------------------------------------------------------

## 📁 Project Structure

```
your-project/
├── templates/
│   └── index.html           → Login UI & dashboard logic (all-in-one)
├── static/
│   └── style.css            → CSS styles and animations
├── app.py                   → Flask server with production security
├── keymaster_auth.py        → KeyMaster API client & HWID logic
├── gunicon.conf.py          → Gunicorn config for production deployment
├── .env.local               → Environment variables (secrets, config)
├── requirements.txt         → Python dependencies
```

------------------------------------------------------------

## 🚀 Getting Started

### Prerequisites

- Python 3.x
- pip (Python package manager)

Install required packages:

```
pip install -r requirements.txt
```

------------------------------------------------------------

## 🛠️ Setup Instructions

1. Clone or download this project to your local machine.
2. Copy `.env.local` and set your secrets and config (see sample in repo).
3. Ensure your KeyMaster UID is set in `.env.local` as `KEYMASTER_ACCOUNT_UID`.
4. Customize the UI in `templates/index.html` as needed.
5. Edit `static/style.css` for your branding.

------------------------------------------------------------

## ▶️ Running the App

Start the Flask server:

```
python app.py
```

By default, the app runs on:

    http://localhost:3000

Login with your registered KeyMaster AGNI credentials. You will see a success or failure message, and on success, the dashboard will be shown on the same page.

------------------------------------------------------------

## 🔐 Using KeyMaster API

Authenticate with Username and Password:

```python
from keymaster_auth import authenticate_client_user
response = authenticate_client_user(master_user_id, username, password, app_version)
```

Authenticate with License Key (optional):

```python
from keymaster_auth import authenticate_license_key
response = authenticate_license_key(master_user_id, license_key, hwid, app_version, username=None)
```

------------------------------------------------------------

## 💡 HWID Generation

HWID (Hardware ID) is used to uniquely identify a user's device.

```python
from keymaster_auth import get_persistent_hwid
hwid = get_persistent_hwid("YourAppName")
```

This ensures licensing remains device-bound, increasing security and preventing key sharing.

------------------------------------------------------------

## 🔑 Environment Configuration

Before running the app, you must set up your environment variables in `.env.local` (or `.env`).

Example `.env.local`:

```
# KeyMaster Configuration
KEYMASTER_ACCOUNT_UID=your_keymaster_uid_here
APP_VERSION=1.0.0

# Flask Configuration
SECRET_KEY=your_super_secret_key_here
FLASK_ENV=production

# Server Configuration
PORT=3000
HOST=0.0.0.0

# Security Settings
SESSION_TIMEOUT=1800

# Logging
LOG_LEVEL=INFO
```

### What to change:
- `KEYMASTER_ACCOUNT_UID`: Get this from your KeyMaster AGNI dashboard or admin panel. This is your unique account identifier for API authentication.
- `SECRET_KEY`: Generate a strong random string (e.g., using `python -c "import secrets; print(secrets.token_hex(32))"`) and keep it secret. Used for Flask session security.
- Adjust other values as needed for your deployment (port, session timeout, etc).

**Never commit your `.env.local` or secrets to public repositories!**

------------------------------------------------------------

## 📌 Production Notes

- Secure session and cookie settings enabled
- Security headers and rate limiting in place
- Logging to file and console for auditability
- Environment variables loaded from `.env.local` (use `python-dotenv`)
- All authentication and dashboard logic handled in `index.html`
- No separate dashboard or auth_result templates needed
- Modular, clean, and API-focused codebase

------------------------------------------------------------

## 🧱 Built With

- Python 3.x
- Flask
- HTML/CSS (Responsive & Animated)
- KeyMaster AGNI API

------------------------------------------------------------

## 📜 License

This project is under the MIT License – free to use, modify, and distribute.

------------------------------------------------------------

## 🙌 Credits

Crafted with ❤️ by Agniveer Corporation  
Official KeyMaster AGNI Python Web Client Integration
