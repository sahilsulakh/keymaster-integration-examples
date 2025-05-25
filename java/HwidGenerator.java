// Java HwidGenerator.java
// (full code, see integration guide for details)
package com.example.keymasterclient;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.UUID;
public class HwidGenerator {
    private static String hwid = null;
    private static final String APP_NAME_FOR_PATH = "YourJavaSoftwareName";
    private static Path getHwidFilePath() {
        String userHome = System.getProperty("user.home");
        String appDirName = APP_NAME_FOR_PATH.toLowerCase().replaceAll("[^a-zA-Z0-9.-]", "_");
        String os = System.getProperty("os.name").toLowerCase();
        Path baseAppDataPath;
        if (os.contains("win")) {
            String appData = System.getenv("APPDATA");
            baseAppDataPath = Paths.get(appData != null ? appData : Paths.get(userHome, "AppData", "Roaming").toString());
        } else if (os.contains("mac")) {
            baseAppDataPath = Paths.get(userHome, "Library", "Application Support");
        } else {
            String xdgConfigHome = System.getenv("XDG_CONFIG_HOME");
            baseAppDataPath = Paths.get(xdgConfigHome != null ? xdgConfigHome : Paths.get(userHome, ".config").toString());
        }
        return baseAppDataPath.resolve(appDirName).resolve("settings").resolve("hwid.dat");
    }
    public static synchronized String getHardwareId() {
        if (hwid != null && !hwid.isEmpty()) {
            return hwid;
        }
        Path hwidFilePath = getHwidFilePath();
        try {
            if (Files.exists(hwidFilePath)) {
                hwid = new String(Files.readAllBytes(hwidFilePath)).trim();
                if (hwid != null && !hwid.isEmpty()) {
                    return hwid;
                }
            }
        } catch (IOException e) {
            System.err.println("Error reading HWID file: " + e.getMessage());
        }
        hwid = UUID.randomUUID().toString().toUpperCase();
        try {
            Files.createDirectories(hwidFilePath.getParent());
            Files.write(hwidFilePath, hwid.getBytes());
        } catch (IOException e) {
            System.err.println("Error writing HWID file: " + e.getMessage());
        }
        return hwid;
    }
}
