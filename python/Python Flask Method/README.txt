🔐 KeyMaster Python + Flask Web Integration

A lightweight, secure, and production-ready Python + Flask-based login system for authenticating users via the KeyMaster AGNI API.

This project demonstrates:

🧔 Username/Password authentication  
💻 Persistent HWID generation per user  
⚡ Beautiful and animated HTML/CSS frontend example  
🔁 Easy-to-customize Flask backend structure

------------------------------------------------------------

📁 Project Structure

your-project/
├── templates/
│   └── index.html           → Your login UI (Username/Password)
├── static/
│   └── style.css            → CSS styles and animations
├── app.py                   → Flask server for authentication logic
├── keymaster_auth.py        → KeyMaster API client & HWID logic

------------------------------------------------------------

🚀 Getting Started

✅ Prerequisites

Ensure Python 3.x is installed. Then install required packages:

pip install flask requests

------------------------------------------------------------

🛠️ Setup Instructions

1. Clone or download this project to your local machine.
2. Open app.py and replace:
   MY_KEYMASTER_ACCOUNT_UID = "YOUR-KEYMASTER-UID-HERE" with your actual KeyMaster UID.
3. Customize the UI in templates/index.html.
4. Modify animation or themes in static/style.css to fit your brand.

------------------------------------------------------------

▶️ Running the App

Start the Flask server:

python app.py

Then open your browser and navigate to:

http://localhost:5000

Login with your registered KeyMaster AGNI credentials.  
You will receive a success or failure message based on your authentication.

------------------------------------------------------------

🔐 Using KeyMaster API

Authenticate with Username and Password:

from keymaster_auth import authenticate_client_user

response = authenticate_client_user(master_user_id, username, password, app_version)

Authenticate with License Key (optional):

from keymaster_auth import authenticate_license_key

response = authenticate_license_key(master_user_id, license_key, hwid, app_version, username=None)

------------------------------------------------------------

💡 HWID Generation

HWID (Hardware ID) is used to uniquely identify a user's device.

from keymaster_auth import get_persistent_hwid

hwid = get_persistent_hwid("YourAppName")

This ensures licensing remains device-bound, increasing security and preventing key sharing.

------------------------------------------------------------

📌 Important Notes

- Fully supports Windows, Linux, and macOS
- Session/token system can be added on top
- Easily extend to include dashboards, file tools, or client portals
- Built to be modular, clean, and API-focused

------------------------------------------------------------

🧱 Built With

- Python 3.x  
- Flask  
- HTML/CSS (Responsive & Animated)  
- KeyMaster AGNI API

------------------------------------------------------------

📜 License

This project is under the MIT License – free to use, modify, and distribute.

------------------------------------------------------------

🙌 Credits

Crafted with ❤️ by Agniveer Corporation  
Official KeyMaster AGNI Python Web Client Integration
