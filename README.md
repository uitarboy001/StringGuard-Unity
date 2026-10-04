# 🛡️ StringGuard for Unity

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity.com/)
[![Release](https://img.shields.io/github/v/release/uitarboy001/StringGuard-Unity)](https://github.com/uitarboy001/StringGuard-Unity/releases)

> **Catch missing tokens and UI overflow in RAM before broken translations crash your Unity build.**

A lightweight, zero-setup Unity Editor gatekeeper that validates Google Sheets localization data directly in memory before writing to disk.

---

## 🎬 Demo

![StringGuard Demo](showcase.gif)

https://github.com/user-attachments/assets/85b55453-59d1-4a98-8311-0cd7d06dee48

---

## ⚠️ The Problem
When collaborating with external translators or community contributors via Google Sheets:
- Translators accidentally wipe code tokens like `{0}`, `{playerName}`, or `%s`.
- When passed into C# `string.Format()`, this triggers a **FormatException and crashes the game at runtime**.
- Translated text (e.g. German/Russian) exceeds UI layouts without anyone noticing until release day.

---

## 🚀 Features

- **RAM Gatekeeper:** Validates data completely in memory. If any errors exist, it blocks file saving to keep your project safe.
- **Token Integrity Linter:** Automatically matches tokens against the source language using Regex.
- **UI Overflow Check:** Flags text exceeding the `MaxChars` limit with exact counts.
- **Zero Cloud Console Setup:** No Google Cloud Console, service accounts, or OAuth credentials needed. Just paste a view-only share link.
- **One-Click Sync:** Automatically exports clean CSV data to `Assets/Localization/strings.csv` and calls `AssetDatabase.Refresh()`.

---

## 📦 Installation

### Option 1: Unity Package Manager via Git URL (Recommended)
1. In Unity, open **Window > Package Manager**.
2. Click the **+** icon in the top-left corner and select **Add package from git URL...**
3. Enter:
### Option 2: Unity Package (.unitypackage)
Download the latest `StringGuard.unitypackage` from the [Releases](https://github.com/uitarboy001/StringGuard-Unity/releases) page and import it into your project.

---

## 🛠️ Quick Start

1. Open **Tools > StringGuard > Sync Strings** from the Unity top menu.
2. Paste your Google Sheet URL (ensure the sheet is set to *Anyone with the link can view*).
3. Set your target folder and file name (defaults to `Assets/Localization/strings.csv`).
4. Click **Sync & Validate**.
