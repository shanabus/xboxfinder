# Xbox Finder App
Checks Best Buy for an Xbox Series X using Selenium and Chrome Driver


## Parameters

Parameter  | Default  | Type     | Notes
---------- | -------- | -------- | ------------------------
retries    | 24       | _int_    | How many times to retry
sleep      | 180000   | _int_    | Milliseconds to sleep between retries
sound      | true     | _bool_   | Play success/fail sound after checking
series     | X        | _string_ | Which xbox to look for (X | S)
email      | true     | _bool_   | Send an email report? Requires email address in appsettings and an API key in user-secrets
