from flask import Flask, render_template, request
from keymaster_auth import authenticate_client_user, get_persistent_hwid

app = Flask(__name__)

MY_KEYMASTER_ACCOUNT_UID = "ellRHrNSMrcAvtnKsvHVqyOvgbT2" <<<--- REPLACE WITH YOUR MASTER UID FROM THE KEYMASTER PROFILE
APP_VERSION = "1.0.0"

@app.route("/", methods=["GET"])
def login_page():
    return render_template("index.html")

@app.route("/auth", methods=["POST"])
def auth_user():
    username = request.form.get("username")
    password = request.form.get("password")
    hwid = get_persistent_hwid("StreamerPanel")
    response = authenticate_client_user(MY_KEYMASTER_ACCOUNT_UID, username, password, APP_VERSION)

    if response.get("success"):
        return f"<h2 style='color:lime;'>Login Successful</h2><p>{response.get('message')}</p>"
    else:
        return f"<h2 style='color:red;'>Login Failed</h2><p>{response.get('message')}</p>"

if __name__ == "__main__":
    app.run(debug=True)
