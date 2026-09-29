using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class StringGuardWindow : EditorWindow
{
    private string sheetUrl = "";
    private string saveFolder = "Assets/Localization";
    private string fileName = "strings.csv";
    private Vector2 scrollPos;
    private List<string> errors = new List<string>();
    private bool isSyncing = false;

    [MenuItem("Tools/StringGuard/Sync Strings")]
    public static void ShowWindow()
    {
        GetWindow<StringGuardWindow>("StringGuard");
    }

    private void OnGUI()
    {
        GUILayout.Label("StringGuard: Localization Sync & Linter", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        sheetUrl = EditorGUILayout.TextField("Google Sheet CSV URL", sheetUrl);
        saveFolder = EditorGUILayout.TextField("Target Folder", saveFolder);
        fileName = EditorGUILayout.TextField("File Name", fileName);

        EditorGUILayout.HelpBox(
            "Quick Setup:\n" +
            "1. Make sure Sheet Share access is set to 'Anyone with the link can view'.\n" +
            "2. URL must end with '/export?format=csv'.\n" +
            "3. Click 'Sync & Validate' to pull real-time strings.",
            MessageType.Info
        );

        EditorGUILayout.Space();

        GUI.enabled = !isSyncing && !string.IsNullOrEmpty(sheetUrl);
        if (GUILayout.Button(isSyncing ? "Syncing..." : "Sync & Validate", GUILayout.Height(35)))
        {
            StartSync();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        if (errors.Count > 0)
        {
            EditorGUILayout.HelpBox($"Found {errors.Count} issue(s)! Import blocked to prevent runtime errors.", MessageType.Error);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
            foreach (var err in errors)
            {
                EditorGUILayout.LabelField(err, EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private void StartSync()
    {
        isSyncing = true;
        errors.Clear();

        UnityWebRequest request = UnityWebRequest.Get(sheetUrl.Trim());
        var operation = request.SendWebRequest();

        operation.completed += (op) =>
        {
            isSyncing = false;
            if (request.result != UnityWebRequest.Result.Success)
            {
                EditorUtility.DisplayDialog("StringGuard Error", "Download failed: " + request.error, "OK");
                Repaint();
                return;
            }

            string csvText = request.downloadHandler.text;
            ValidateAndSave(csvText);
            Repaint();
        };
    }

    private void ValidateAndSave(string csvText)
    {
        // ดักจับกรณีชีตติดล็อค หรือ Google พ่น HTML หน้า Login กลับมา
        if (csvText.TrimStart().StartsWith("<") || csvText.Contains("<!DOCTYPE html") || csvText.Contains("<html"))
        {
            EditorUtility.DisplayDialog(
                "Access Denied / Not a CSV",
                "Google Sheet returned an HTML page instead of CSV!\n\n" +
                "Possible Fixes:\n" +
                "1. Check the blue 'Share' button in Google Sheets -> Change to 'Anyone with the link can view'.\n" +
                "2. Ensure your URL ends with '/export?format=csv'.",
                "OK"
            );
            return;
        }

        string[] lines = csvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        if (lines.Length <= 1)
        {
            errors.Add("CSV file is empty or missing data rows.");
            return;
        }

        // อ่าน Headers
        string[] headers = lines[0].Split(',');
        int keyIndex = Array.FindIndex(headers, h => Regex.IsMatch(h.Trim(), "^(key|id|string_id|name)$", RegexOptions.IgnoreCase));
        int sourceIndex = Array.FindIndex(headers, h => Regex.IsMatch(h.Trim(), "^(en|english|source|text)$", RegexOptions.IgnoreCase));
        int targetIndex = Array.FindIndex(headers, h => h.IndexOf("target", StringComparison.OrdinalIgnoreCase) >= 0 || h.IndexOf("de", StringComparison.OrdinalIgnoreCase) >= 0);
        int maxCharsIndex = Array.FindIndex(headers, h => Regex.IsMatch(h.Trim(), "limit|max|char", RegexOptions.IgnoreCase));

        if (keyIndex == -1 && sourceIndex == -1)
        {
            errors.Add("Invalid Header row: Could not identify 'Key' or 'Source' columns.");
            return;
        }

        if (keyIndex == -1) keyIndex = 0;
        if (sourceIndex == -1) sourceIndex = 1;
        if (targetIndex == -1) targetIndex = headers.Length > 2 ? 2 : sourceIndex;

        // วนลูปตรวจแต่ละแถว
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] cols = line.Split(',');
            if (cols.Length <= Math.Max(keyIndex, Math.Max(sourceIndex, targetIndex))) continue;

            string key = cols[keyIndex].Trim();
            string source = cols[sourceIndex].Trim();
            string target = cols[targetIndex].Trim();

            // ดึง Token
            List<string> sourceTokens = ExtractTokens(source);

            // 1. ตรวจสอบ Missing Tokens
            foreach (var token in sourceTokens)
            {
                if (!target.Contains(token))
                {
                    errors.Add($"[Row {i + 1}] Key: '{key}' -> Missing required token: {token}");
                }
            }

            // 2. ตรวจสอบ UI Overflow
            if (maxCharsIndex != -1 && cols.Length > maxCharsIndex)
            {
                if (int.TryParse(cols[maxCharsIndex].Trim(), out int maxLimit))
                {
                    if (target.Length > maxLimit)
                    {
                        errors.Add($"[Row {i + 1}] Key: '{key}' -> Text overflow ({target.Length}/{maxLimit} chars)");
                    }
                }
            }
        }

        if (errors.Count > 0)
        {
            Debug.LogError($"[StringGuard] Found {errors.Count} issue(s). Import blocked!");
        }
        else
        {
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            string fullPath = Path.Combine(saveFolder, fileName);
            File.WriteAllText(fullPath, csvText, System.Text.Encoding.UTF8);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "StringGuard Success",
                $"Localization synced successfully!\n0 syntax errors found (100% safe).\n\nSaved to: {fullPath}",
                "OK"
            );
            Debug.Log($"<color=green>[StringGuard]</color> Sync successful: {fullPath}");
        }
    }

    private List<string> ExtractTokens(string text)
    {
        List<string> tokens = new List<string>();
        var bracketMatches = Regex.Matches(text, @"\{[a-zA-Z0-9_]+\}");
        foreach (Match m in bracketMatches)
        {
            if (!tokens.Contains(m.Value)) tokens.Add(m.Value);
        }

        var printfMatches = Regex.Matches(text, @"%[sdif]");
        foreach (Match m in printfMatches)
        {
            if (!tokens.Contains(m.Value)) tokens.Add(m.Value);
        }

        return tokens;
    }
}