# 🛡️ StringGuard for Unity

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](https://unity.com/)
[![Release](https://img.shields.io/github/v/release/uitarboy001/StringGuard-Unity)](https://github.com/uitarboy001/StringGuard-Unity/releases)
[![Web Validator](https://img.shields.io/badge/Web_Validator-Live-emerald.svg)](https://string-guard-lemon.vercel.app/)

> **Catch missing tokens and UI overflow in RAM before broken translations crash your Unity build.**

A lightweight, zero-setup Unity Editor gatekeeper and linter that validates Google Sheets localization data directly in memory before writing to disk. Features 100% validation parity with the standalone [StringGuard Web Validator](https://string-guard-lemon.vercel.app/).

---

## 🎬 Demo

![StringGuard Demo](showcase.gif)

---

## ⚠️ The Problem
When collaborating with external translators or community contributors via Google Sheets
- Translators accidentally wipe or alter code tokens like `{0}`, `{playerName}`, `%s`, or `%d`.
- When passed into C# `string.Format()`, missing tokens trigger a **runtime FormatException and crash the game**.
- Translated text (e.g., German/Russian/Thai) silently overflows UI elements.
- Google Sheets CDN caches CSV downloads for 30–120 seconds, causing developers to pull outdated files even after a fix was submitted.

---

## 🚀 Features

- **🛡️ In-RAM Gatekeeper:** Validates CSV data completely in memory. Blocks saving to disk if syntax issues exist, keeping your game builds 100% crash-free.
- **⚡ Real-Time Cache-Busting:** Automatically injects dynamic timestamp parameters (`&t=timestamp`) into Google Sheets export URLs to bypass CDN caching for instant, near real-time synchronization.
- **📑 RFC 4180 Compliant Parser:** Correctly parses commas inside double-quoted text (`"Hello, adventurer!"`), quotes, and special symbols without corrupting row data.
- **🔍 1:1 Validation Parity with Web:** Shares identical linting rules with the [Web Validator](https://string-guard-lemon.vercel.app/), ensuring translators and developers always see matching results.
- **🔤 Token & Format String Integrity:** Tracks curly bracket tokens (`{name}`, `{0}`) and standard printf format specifiers (`%s`, `%d`, `%f`).
- **📏 UI Overflow Detection:** Flags text exceeding the `MaxChars` limit with exact length vs. limit counters.
- **✨ Clean, Modern Editor UI:** Clean inline status cards and non-intrusive floating toasts (`ShowNotification`) instead of disruptive Windows modal dialogs.
- **🔗 Translator Cross-Link:** One-click button inside the Unity Editor to copy the Web Validator URL directly to your clipboard for your localization team.

---

## 🔄 End-to-End Workflow

```text

[Google Sheets]
       │
       ├──► Translators validate on Web (Zero install, Client-side only)
       │    └─► [https://string-guard-lemon.vercel.app/](https://string-guard-lemon.vercel.app/)
       │
       └──► Developers sync in Unity (Tools > StringGuard > Sync Strings)
            └─► In-RAM Gatekeeper passes ──► Saved to Assets/Localization/strings.csv
---

## 📦 Installation

### Option 1: Unity Package Manager via Git URL (Recommended)
1. In Unity, open **Window > Package Manager**.
2. Click the **+** icon in the top-left corner and select **Add package from git URL...**
3. Enter:
   ```text
   https://github.com/uitarboy001/StringGuard-Unity.git https://github.com/uitarboy001/StringGuard-Unity.git
