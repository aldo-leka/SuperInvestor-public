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
            "STRIPE_API_KEY": "",
            "STRIPE_PRICE_ID": "",
            "STRIPE_WEBHOOK_SECRET": "",
            "CONNECTION_STRING": "",
            "SENDER_EMAIL_ADDRESS": "",
            "SMTP_HOST": "",
            "SMTP_PORT": "",
            "SMTP_USERNAME": "",
            "SMTP_PASSWORD": "",
            "SMTP_USE_SSL": ""
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

### **Email Configuration**

SuperInvestor uses SMTP to send account confirmation and password reset emails. Configure the following environment variables:

**For Gmail:**
1. Set `SENDER_EMAIL_ADDRESS` to your Gmail address
2. Set `SMTP_HOST` to `smtp.gmail.com`
3. Set `SMTP_PORT` to `587`
4. Set `SMTP_USERNAME` to your Gmail address
5. Set `SMTP_USE_SSL` to `true`
6. Set `SMTP_PASSWORD` to a **Gmail App Password** (not your regular password)

**How to generate a Gmail App Password:**
- Visit: https://myaccount.google.com/apppasswords
- Select "Mail" for the app type
- Select "Other (Custom name)" and enter "SuperInvestor"
- Copy the 16-character password and use it as `SMTP_PASSWORD`

**Note:** You must have 2-Step Verification enabled on your Google Account to generate app passwords.

**For other email providers:**
- Use your provider's SMTP settings (host, port, SSL settings)
- Check your email provider's documentation for SMTP configuration

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
