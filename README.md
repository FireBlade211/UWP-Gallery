<p align="center">
  <img src="/Assets/StoreLogo.scale-400.png" alt="UWP Gallery Logo" />
</p>
<h1 align="center">UWP Gallery</h1>
<p align="center">Companion app for <a href="https://learn.microsoft.com/windows/uwp">UWP</a> controls & APIs</p>
This app demonstrates the controls and APIs available in UWP, showcasing UWP functionality through interactive samples and code snippets. Each control page shows the markup (XAML) and code-behind (C#) used to create each sample!

# 👏 Contribute to the UWP Gallery
Any samples or documentation improvements you'd like to see? Feel free to file an issue, or even better, create a PR with the change you'd like to see!

# ✉️ License
**UWP Gallery** is licensed under the MIT License. See the [LICENSE](LICENSE) file for more information.

# 🚀 Getting Started
## 1. Building
> [!NOTE]
> The **UWP Gallery** requires Windows 10 or later to execute.

> [!TIP]
> You can also download the **UWP Gallery** from the [Releases](https://github.com/FireBlade211/UWP-Gallery/releases) page instead of building it manually. See [Installation](README.md#2-installation) for more info.

### 1. Clone the repository
Inside the command line, run:
```pwsh
git clone https://github.com/FireBlade211/UWP-Gallery.git
```

### 2. Open `UWP Gallery.sln` and build!
Open the **UWP Gallery** solution in Visual Studio and click **Build -> Build Solution**!

## 2. Installation
Instead of building the project yourself, you can install the app from the [Releases](https://github.com/FireBlade211/UWP-Gallery/releases) page.

### 1. Download the ZIP file
Download the latest ZIP file from the [Releases](https://github.com/FireBlade211/UWP-Gallery/releases) and extract it.

### 2. Install the Certificate
To be able to install the app, you need to install the certificate:
1. Open the extracted folder.
2. Find the *.cer* file and double-click on it.
3. In the **Certificate** dialog, click **Install Certificate**.
4. In the **Certificate Import Wizard**, select *Local Machine* and press Next.
5. Authenticate with **User Account Control** (*UAC*) to continue.
6. On the next page, select **Place all certificates in the following store**, and in the *Certificate store* box, click **Browse**, find **Trusted Root Certification Authorities**, and press OK. Then, press Next.
7. Finally, hit Finish, close the message box that shows up, and close out of the **Certificate** dialog.

### 3. Install the Gallery
Run the **.msixbundle** file inside the extracted folder and press **Install**.

# ℹ️ Further information
To learn more about UWP, go to the [UWP Documentation](https://learn.microsoft.com/windows/uwp).

