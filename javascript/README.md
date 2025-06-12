# KeyMaster Authentication System

A modern, secure web-based authentication system that supports both license key authentication and username/password authentication. Built with vanilla HTML, CSS, and JavaScript for maximum compatibility and performance.

## ✨ Features

- **Dual Authentication Methods**
  - License Key Authentication with optional username
  - Traditional Username/Password Authentication
- **Modern UI/UX**
  - Glassmorphism design with gradient backgrounds
  - Smooth animations and hover effects
  - Responsive design for all devices
  - Tab-based interface switching
- **Security Features**
  - Hardware ID (HWID) generation and validation
  - Persistent device identification
  - Secure API communication
- **User Experience**
  - Loading states with spinners
  - Success/error message display
  - Form validation
  - Clean, intuitive interface

## 📁 File Structure

```
keymaster-auth/
├── index.html          # Main HTML structure
├── styles.css          # All CSS styling and animations
├── script.js           # JavaScript functionality and API calls
└── README.md           # This file
```

## 🛠️ Setup and Installation

### Prerequisites
- A modern web browser
- KeyMaster account and API access
- Web server (for local development)

### Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/keymaster-auth.git
   cd keymaster-auth
   ```

2. **Configure your KeyMaster credentials**
   
   Open `script.js` and update the configuration variables:
   ```javascript
   const MY_KEYMASTER_ACCOUNT_UID = "your-keymaster-account-uid-here";
   const MY_APP_VERSION = "1.0.0";
   const APP_NAME = "YourAppName";
   ```

3. **Serve the files**
   
   Using Python (simple method):
   ```bash
   # Python 3
   python -m http.server 8000
   
   # Python 2
   python -m SimpleHTTPServer 8000
   ```
   
   Using Node.js:
   ```bash
   npx serve .
   ```

4. **Open in browser**
   ```
   http://localhost:8000
   ```

## ⚙️ Configuration

### KeyMaster Account Setup

1. Sign up at [KeyMaster](https://keymaster-agni.vercel.app)
2. Get your Account UID from the profile page
3. Create license keys or client users as needed
4. Update the `MY_KEYMASTER_ACCOUNT_UID` in `script.js`

### Customization Options

**Application Settings** (in `script.js`):
```javascript
const MY_KEYMASTER_ACCOUNT_UID = "your-uid";    // Your KeyMaster account UID
const MY_APP_VERSION = "1.0.0";                // Your app version
const APP_NAME = "YourAppName";                 // Your application name
```

**API Endpoints** (in `script.js`):
```javascript
const LICENSE_AUTH_API_URL = "https://keymaster-agni.vercel.app/api/authenticate-key";
const CLIENT_USER_AUTH_API_URL = "https://keymaster-agni.vercel.app/api/authenticate-client-user";
```

## 🎨 Styling Customization

The application uses CSS custom properties for easy theming. Main colors can be changed in `styles.css`:

```css
:root {
  --primary-gradient: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  --accent-color: #667eea;
  --success-color: #28a745;
  --error-color: #dc3545;
}
```

## 📚 API Reference

### License Key Authentication

**Endpoint**: `POST /api/authenticate-key`

**Payload**:
```json
{
  "masterUserId": "string",
  "licenseKey": "string",
  "hwid": "string",
  "appVersion": "string",
  "username": "string" // optional
}
```

### Username/Password Authentication

**Endpoint**: `POST /api/authenticate-client-user`

**Payload**:
```json
{
  "masterUserId": "string",
  "username": "string",
  "passwordPlainText": "string",
  "appVersion": "string"
}
```

## 🔧 Hardware ID (HWID) System

The application generates a persistent Hardware ID for device identification:

- Generated using UUID4-like algorithm
- Stored in browser's localStorage
- Unique per application name
- Used for license validation and device tracking

## 🌐 Browser Compatibility

- ✅ Chrome 60+
- ✅ Firefox 55+
- ✅ Safari 12+
- ✅ Edge 79+
- ✅ Opera 47+

## 📱 Mobile Support

Fully responsive design that works on:
- iOS Safari
- Android Chrome
- Mobile Firefox
- Samsung Internet

## 🔒 Security Considerations

- **HTTPS Required**: Always use HTTPS in production
- **API Keys**: Never expose sensitive API keys in client-side code
- **Input Validation**: All inputs are validated on both client and server
- **HWID Persistence**: Hardware IDs are stored locally and persist across sessions

## 🚨 Common Issues

### Authentication Fails
- Verify your KeyMaster Account UID is correct
- Check that license keys are active and not expired
- Ensure API endpoints are accessible

### HWID Issues
- Clear localStorage if HWID needs to be regenerated
- Check browser compatibility for localStorage support

### Styling Issues
- Ensure all three files (HTML, CSS, JS) are in the same directory
- Verify file paths in HTML are correct
- Check browser console for any loading errors

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🛟 Support

- **Email**: agniveer171103@gmail.com

## 🙋 FAQ

**Q: Can I use this without KeyMaster?**
A: No, this authentication system is specifically designed to work with the KeyMaster API service.

**Q: Is this suitable for production use?**
A: Yes, but ensure you implement proper security measures like HTTPS and secure your API endpoints.

**Q: Can I customize the design?**
A: Absolutely! All styling is in `styles.css` and can be easily modified.

**Q: Does this work offline?**
A: The UI works offline, but authentication requires an internet connection to reach the KeyMaster API.

## 🔄 Version History

- **v1.0.0** - Initial release with dual authentication support
- **v1.1.0** - Added responsive design improvements
- **v1.2.0** - Enhanced error handling and user feedback

---

Crafted with ❤️ by Agniveer Corporation

**⭐ Star this repository if you find it helpful!**
