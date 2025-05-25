// Java AuthModels.java
// (full code, see integration guide for details)
package com.example.keymasterclient;
import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.annotation.JsonInclude;
public class AuthModels {
    @JsonInclude(JsonInclude.Include.NON_NULL)
    public static class AuthRequest {
        @JsonProperty("masterUserId")
        public String masterUserId;
        @JsonProperty("licenseKey")
        public String licenseKey;
        @JsonProperty("hwid")
        public String hwid;
        @JsonProperty("appVersion")
        public String appVersion;
        @JsonProperty("username")
        public String username;
        public AuthRequest(String masterUserId, String licenseKey, String hwid, String appVersion, String username) {
            this.masterUserId = masterUserId;
            this.licenseKey = licenseKey.toUpperCase();
            this.hwid = hwid;
            this.appVersion = appVersion;
            this.username = username;
        }
        public AuthRequest() {}
    }
    public static class AuthResponse {
        @JsonProperty("success")
        public boolean success;
        @JsonProperty("message")
        public String message;
        @JsonProperty("errorCode")
        public String errorCode;
        @JsonProperty("userStatus")
        public String userStatus;
        public AuthResponse() {}
        @Override
        public String toString() {
            return "AuthResponse{" +
                   "success=" + success +
                   ", message='" + (message != null ? message : "N/A") + '\'' +
                   ", errorCode='" + (errorCode != null ? errorCode : "N/A") + '\'' +
                   ", userStatus='" + (userStatus != null ? userStatus : "N/A") + '\'' +
                   '}';
        }
    }
}

