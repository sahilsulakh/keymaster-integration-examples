# KeyMaster Integration Examples

This repository provides ready-to-use code files for integrating the KeyMaster SaaS license authentication system into your client applications. Use these files to quickly add license key validation, HWID (hardware ID) tracking, and secure authentication to your software.

## Supported Languages

- **C# (.NET/WinForms)**: [`/csharp`](https://github.com/sahilsulakh/keymaster-integration-examples/tree/main/csharp)
- **Python**: [`/python/python-auth-client.py`](https://github.com/sahilsulakh/keymaster-integration-examples/blob/main/python/python-auth-client.py)
- **Node.js (JavaScript/TypeScript)**: [`/nodejs/keymasterAuthClient.js`](https://github.com/sahilsulakh/keymaster-integration-examples/blob/main/nodejs/keymasterAuthClient.js)
- **Java**: [`/java`](https://github.com/sahilsulakh/keymaster-integration-examples/tree/main/java)

## How to Use

1. **Get Your KeyMaster Account UID**
   - Log in to your KeyMaster dashboard and copy your User ID (UID) from the Profile page.

2. **Choose Your Language**
   - Browse to the relevant folder above and download the code files for your platform.

3. **Replace Placeholders**
   - In each code sample, replace `"PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE"` with your actual UID.
   - Update any application-specific names (e.g., for HWID storage) as needed.

4. **Integrate and Test**
   - Add the code to your project.
   - Follow comments in the code for setup and usage.
   - Test authentication with your KeyMaster-generated license keys.

## Features

- Secure license key validation via KeyMaster API
- HWID (machine ID) generation and tracking
- Example error handling and user feedback
- Easy integration for desktop and backend apps

## More Resources

- [KeyMaster .NET Client Starter Guide](https://github.com/sahilsulakh/KeymasterDotNetStarterGuide) – Full WinForms example project
- [KeyMaster Web Dashboard](https://keymaster-agni.vercel.app/) – Manage your licenses and settings


---

For questions or support, please use the KeyMaster dashboard contact form or open an issue in this repository.
