# Shutdown Timer

A modern Windows shutdown timer application. Schedule automatic shutdown or restart operations with an intuitive interface.

https://github.com/user-attachments/assets/83472298-3b93-495e-bd7f-71966dd55189

## ✨ Features

- **⏰ Flexible Timer Modes**
  - Duration-based timer (hours, minutes, seconds)
  - Specific time scheduling
  - Real-time countdown and scheduled time display

- **🎯 System Actions**
  - Shutdown
  - Restart
  - Force execution option (bypasses application save prompts)

- **🌐 Multi-language Support**
  - Japanese
  - English

- **💾 Smart Features**
  - Dark/Light theme support
  - Automatic schedule save/restore

## 🚀 Installation

### Option 1: Portable (ZIP)

1. Visit the [Releases](https://github.com/yashikota/shutdown-timer/releases) page
2. Download `shutdown-timer-{VERSION}-win-{ARCHITECTURE}.zip`
3. Extract the ZIP file and run `shutdown-timer.exe`

### Option 2: MSIX Package

> [!CAUTION]
> This app uses a self-signed certificate. To install the MSIX package, you need to add this certificate to your "Trusted Root Certification Authorities".
>
> **Please understand what this means:**
> - You are trusting a certificate created by this project's developer
> - Apps signed with this certificate will be trusted by your system
> - Only proceed if you trust this project and have reviewed the source code
> - The certificate only needs to be installed once

1. Visit the [Releases](https://github.com/yashikota/shutdown-timer/releases) page
2. Download `signing-cert.cer` and install the certificate:
   - Double-click `signing-cert.cer`
   - Click "Install Certificate"
   - Select "Local Machine" → Next
   - Select "Place all certificates in the following store" → Browse
   - Select "Trusted Root Certification Authorities" → OK → Next → Finish
3. Download `shutdown-timer-{VERSION}-win-{ARCHITECTURE}.msix`
4. Double-click the `.msix` file to install

## 🎮 How to Use

### Basic Usage

1. **Select Timer Mode**
   - **Duration**: Set timer for specific hours, minutes, and seconds
   - **Specific Time**: Schedule for exact time

2. **Configure Settings**
   - Choose action type (Shutdown/Restart)
   - Enable force execution if needed

3. **Start Timer**
   - Click "Start" to begin countdown
   - Monitor remaining time and scheduled time in the status area
   - Cancel anytime with "Cancel" button
