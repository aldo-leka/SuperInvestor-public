# SuperInvestor

SuperInvestor is a note-taking application for investors, built with ASP.NET Core Blazor. It allows users to integrate financial filings from the SEC, take structured notes, and organize research efficiently.

## ✨ Features
- 📄 **Integrated SEC Filing Search** – Fetch and display financial reports from the SEC API.
- 📝 **Rich-Text Note-Taking** – Annotate filings and save structured notes.
- 🔍 **Search & Filter Notes** – Quickly find notes by stock ticker, date, or keywords.
- 🌍 **Blazor-Powered UI** – Smooth and interactive interface built with Blazor.
- 🗄️ **Secure Data Storage** – Persist notes locally or through database integration.

---

## 🚀 Getting Started

### **Prerequisites**
- .NET 9.0 SDK
- Node.js and npm

### **Setup**
1. **Clone the repository**  
   ```sh
   git clone https://github.com/aldo-leka/SuperInvestor-public.git
   ```
2. **Navigate to project root**  
   ```sh
   cd SuperInvestor-public
   ```
3. **Set up configuration**  
   - Add the configuration file at `Properties/launchSettings.json` for local development. Sample content:
   ```
   {
      "profiles": {
         "http": {
            "commandName": "Project",
            "launchBrowser": true,
            "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Development",
            "GOOGLE_CLIENT_ID": "",
            "GOOGLE_CLIENT_SECRET": "",
            "TURNSTILE_SITE_KEY": "",
            "TURNSTILE_SECRET_KEY": "",
            "RESEND_API_KEY": "",
            "RESEND_SENDER_EMAIL": "",
            "RESEND_SENDER_NAME": "",
            "STRIPE_API_KEY": "",
            "STRIPE_PRICE_ID": "",
            "STRIPE_WEBHOOK_SECRET": "",
            "CONNECTION_STRING": ""
            },
            "dotnetRunMessages": true,
            "applicationUrl": "http://localhost:5036"
         },
         "https": {
            "commandName": "Project",
            "launchBrowser": true,
            "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "dotnetRunMessages": true,
            "applicationUrl": "https://localhost:7083;http://localhost:5036"
         },
         "IIS Express": {
            "commandName": "IISExpress",
            "launchBrowser": true,
            "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Development"
            }
         }
      },
      "$schema": "http://json.schemastore.org/launchsettings.json",
      "iisSettings": {
         "windowsAuthentication": false,
         "anonymousAuthentication": true,
         "iisExpress": {
            "applicationUrl": "http://localhost:28945",
            "sslPort": 44367
         }
      }
   }
   ```

### **Installing JavaScript Dependencies**
```sh
npm install
```
To watch for changes and rebuild automatically:
```sh
npm run watch
```

### **Running the Application**
```sh
dotnet run
```
Then, open your browser and go to:  
**🔗 https://localhost:7083** (or the port specified in the console output)

---

## 📁 Project Structure
```
SuperInvestor-public/
│── wwwroot/javascript/   # JavaScript files
│── Components/           # Blazor components
│── Services/             # C# service classes
│── wwwroot/dist/         # Bundled JavaScript
```

---

## 🤝 Contributing
Want to improve SuperInvestor? Feel free to **fork the repo and submit a pull request!**

---

## 📜 License
This project is licensed under the **MIT License**.

---

## 📬 Contact
For questions or feature requests, reach out via **GitHub Issues**.
