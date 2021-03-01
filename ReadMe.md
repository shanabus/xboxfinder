# Xbox Finder App
Checks Best Buy for an Xbox Series X using Selenium and Chrome Driver

## Sending Email
You will need to create a FREE SendGrid account, authenticate a Sender email address and use it in the app settings in place of mine.  Create a SendGrid API Key and set it:

```
dotnet user-secrets set "SendGrid.ApiKey" "YOUR_KEY_HERE"
```

## Parameters
To set parameters, pass them with the run command: 

```
dotnet run series=S sound=false
```

Parameter  | Default  | Type     | Notes
---------- | -------- | -------- | ------------------------
retries    | 24       | _int_    | How many times to retry
sleep      | 180000   | _int_    | Milliseconds to sleep between retries
sound      | true     | _bool_   | Play success/fail sound after checking
series     | X        | _string_ | Which xbox to look for (X | S)
email      | true     | _bool_   | Send an email report? Requires email address in appsettings and an API key in user-secrets 
   
  
## Terms & Conditions
This application is for entertainment purposes only and its author is not liable for use which conflicts with the terms & conditions of the sites you use it to check availability for.