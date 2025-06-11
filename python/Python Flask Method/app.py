import os
import logging
from flask import Flask, render_template, request, flash, redirect, url_for, session
from werkzeug.middleware.proxy_fix import ProxyFix
from keymaster_auth import authenticate_client_user, get_persistent_hwid
from dotenv import load_dotenv
load_dotenv('.env.local')
import secrets

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('app.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

def create_app():
    app = Flask(__name__)
    
    # Security configurations
    app.config['SECRET_KEY'] = os.environ.get('SECRET_KEY', secrets.token_hex(32))
    app.config['SESSION_COOKIE_SECURE'] = True
    app.config['SESSION_COOKIE_HTTPONLY'] = True
    app.config['SESSION_COOKIE_SAMESITE'] = 'Lax'
    app.config['PERMANENT_SESSION_LIFETIME'] = 1800  # 30 minutes
    
    # Rate limiting and security headers
    app.config['MAX_CONTENT_LENGTH'] = 1 * 1024 * 1024  # 1MB max file size
    
    # Trust proxy headers if behind reverse proxy
    app.wsgi_app = ProxyFix(app.wsgi_app, x_for=1, x_proto=1, x_host=1, x_prefix=1)
    
    return app

app = create_app()

# Configuration
MY_KEYMASTER_ACCOUNT_UID = os.environ.get("KEYMASTER_ACCOUNT_UID")
if not MY_KEYMASTER_ACCOUNT_UID:
    raise ValueError("KEYMASTER_ACCOUNT_UID environment variable is required")
APP_VERSION = os.environ.get("APP_VERSION", "1.0.0")

@app.before_request
def security_headers():
    """Add security headers to all responses"""
    pass

@app.after_request
def add_security_headers(response):
    """Add security headers"""
    response.headers['X-Content-Type-Options'] = 'nosniff'
    response.headers['X-Frame-Options'] = 'DENY'
    response.headers['X-XSS-Protection'] = '1; mode=block'
    response.headers['Strict-Transport-Security'] = 'max-age=31536000; includeSubDomains'
    response.headers['Content-Security-Policy'] = "default-src 'self'; style-src 'self' 'unsafe-inline'; script-src 'self'"
    return response

@app.errorhandler(404)
def not_found(error):
    logger.warning(f"404 error: {request.url}")
    return render_template('error.html', error_code=404, error_message="Page not found"), 404

@app.errorhandler(500)
def internal_error(error):
    logger.error(f"500 error: {str(error)}")
    return render_template('error.html', error_code=500, error_message="Internal server error"), 500

@app.errorhandler(413)
def too_large(error):
    logger.warning(f"413 error: Request too large from {request.remote_addr}")
    return render_template('error.html', error_code=413, error_message="Request too large"), 413

@app.route("/", methods=["GET"])
def login_page():
    """Render login page"""
    try:
        return render_template("index.html")
    except Exception as e:
        logger.error(f"Error rendering login page: {str(e)}")
        return "Server error", 500

@app.route("/auth", methods=["POST"])
def auth_user():
    """Authenticate user credentials"""
    try:
        # Input validation
        username = request.form.get("username", "").strip()
        password = request.form.get("password", "")
        
        if not username or not password:
            logger.warning(f"Invalid login attempt - missing credentials from {request.remote_addr}")
            return render_template("index.html", 
                                   auth_result=False, 
                                   message="Username and password are required")
        
        # Length validation
        if len(username) > 50 or len(password) > 100:
            logger.warning(f"Invalid login attempt - credentials too long from {request.remote_addr}")
            return render_template("index.html", 
                                   auth_result=False, 
                                   message="Invalid credentials")
        
        # Get hardware ID
        hwid = get_persistent_hwid("StreamerPanel")
        
        # Log authentication attempt
        logger.info(f"Authentication attempt for user: {username} from {request.remote_addr}")
        
        # Authenticate with KeyMaster
        response = authenticate_client_user(
            MY_KEYMASTER_ACCOUNT_UID, 
            username, 
            password, 
            APP_VERSION
        )
        
        if response.get("success"):
            logger.info(f"Successful authentication for user: {username}")
            session['authenticated'] = True
            session['username'] = username
            session.permanent = True
            
            return render_template("index.html", 
                                   auth_result=True, 
                                   message=response.get('message', 'Login successful'),
                                   username=username)
        else:
            logger.warning(f"Failed authentication for user: {username} from {request.remote_addr}")
            return render_template("index.html", 
                                   auth_result=False, 
                                   message=response.get('message', 'Authentication failed'))
    except Exception as e:
        logger.error(f"Authentication error: {str(e)}")
        return render_template("index.html", 
                               auth_result=False, 
                               message="An error occurred during authentication")

if __name__ == "__main__":
    app.run(debug=True, port=3000)
