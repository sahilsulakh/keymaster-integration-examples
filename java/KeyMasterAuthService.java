// Java KeyMasterAuthService.java
// (full code, see integration guide for details)
package com.example.keymasterclient;
import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;
public class KeyMasterAuthService {
    private static final String API_BASE_URL = "https://keymaster-agni.vercel.app/api/authenticate-key";
    private final HttpClient httpClient;
    private final ObjectMapper objectMapper;
    public KeyMasterAuthService() {
        this.httpClient = HttpClient.newBuilder()
                .version(HttpClient.Version.HTTP_1_1)
                .connectTimeout(Duration.ofSeconds(15))
                .build();
        this.objectMapper = new ObjectMapper();
        this.objectMapper.setPropertyNamingStrategy(PropertyNamingStrategies.LOWER_CAMEL_CASE);
        this.objectMapper.configure(com.fasterxml.jackson.databind.DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, false);
        this.objectMapper.setSerializationInclusion(com.fasterxml.jackson.annotation.JsonInclude.Include.NON_NULL);
    }
    public AuthModels.AuthResponse authenticateLicense(AuthModels.AuthRequest requestPayload) {
        final String PLACEHOLDER_UID = "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE";
        if (requestPayload.masterUserId == null || requestPayload.masterUserId.isEmpty() || requestPayload.masterUserId.equals(PLACEHOLDER_UID)) {
            AuthModels.AuthResponse err = new AuthModels.AuthResponse();
            err.success = false;
            err.message = "Configuration Error: MasterUserId is not set in the Java application. Replace placeholder.";
            err.errorCode = "CLIENT_MISSING_MASTER_USER_ID";
            return err;
        }
        if (requestPayload.licenseKey == null || requestPayload.licenseKey.isEmpty()) {
             AuthModels.AuthResponse err = new AuthModels.AuthResponse();
            err.success = false;
            err.message = "License Key cannot be empty.";
            err.errorCode = "CLIENT_MISSING_LICENSE_KEY";
            return err;
        }
        try {
            String jsonPayload = objectMapper.writeValueAsString(requestPayload);
            System.out.println("Sending JSON to KeyMaster: " + jsonPayload);
            HttpRequest request = HttpRequest.newBuilder()
                    .uri(URI.create(API_BASE_URL))
                    .timeout(Duration.ofSeconds(30))
                    .header("Content-Type", "application/json")
                    .POST(HttpRequest.BodyPublishers.ofString(jsonPayload))
                    .build();
            HttpResponse<String> response = httpClient.send(request, HttpResponse.BodyHandlers.ofString());
            System.out.println("KeyMaster Response Status: " + response.statusCode() + ", Body: " + response.body());
            return objectMapper.readValue(response.body(), AuthModels.AuthResponse.class);
        } catch (java.io.IOException | InterruptedException e) {
            System.err.println("Exception in authenticateLicense: " + e.getMessage());
            AuthModels.AuthResponse err = new AuthModels.AuthResponse();
            err.success = false;
            err.message = "Error communicating with KeyMaster: " + e.getMessage();
            if (e instanceof java.net.http.HttpTimeoutException) {
                 err.errorCode = "TIMEOUT_ERROR";
            } else if (e instanceof java.net.ConnectException) {
                 err.errorCode = "NETWORK_ERROR";
            } else {
                 err.errorCode = "CLIENT_REQUEST_ERROR";
            }
            return err;
        }
    }
    public static void main(String[] args) {
        String myKeyMasterAccountUid = "PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE";
        if (myKeyMasterAccountUid.equals("PASTE_YOUR_KEYMASTER_ACCOUNT_UID_HERE")) {
            System.err.println("CRITICAL CONFIGURATION ERROR: myKeyMasterAccountUid is not set.");
            System.err.println("Please open this file and replace the placeholder with your actual KeyMaster User ID.");
            return;
        }
        KeyMasterAuthService service = new KeyMasterAuthService();
        String testLicenseKey = "YOUR-TEST-KEY-FROM-KEYMASTER";
        String testUsername = "TestJavaUser123";
        String testAppVersion = "1.0.0";
        String testHwid = HwidGenerator.getHardwareId();
        System.out.println("\nUsing MasterUserId: " + myKeyMasterAccountUid + ", HWID: " + testHwid);
        System.out.println("Authenticating key: " + testLicenseKey + " for App v" + testAppVersion + "...");
        AuthModels.AuthRequest authReq = new AuthModels.AuthRequest(
            myKeyMasterAccountUid, testLicenseKey, testHwid, testAppVersion, testUsername
        );
        AuthModels.AuthResponse result = service.authenticateLicense(authReq);
        System.out.println("\n--- Authentication Result ---");
        System.out.println(result);
        if (result != null && result.success) {
            System.out.println("Application can proceed!");
        } else {
            System.out.println("Application access denied.");
        }
    }
}

