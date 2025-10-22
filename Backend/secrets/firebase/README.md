## Firebase Service Account Setup

1. Ask the project owner for the real `serviceAccount.json`.
2. Place it in this folder (next to this README).
3. Set the environment variable `GOOGLE_APPLICATION_CREDENTIALS` to the absolute path of the file.
   - Local PowerShell example:

     ```powershell
     setx GOOGLE_APPLICATION_CREDENTIALS "C:\path\to\repo\Backend\secrets\firebase\serviceAccount.json"
     $env:GOOGLE_APPLICATION_CREDENTIALS = "C:\path\to\repo\Backend\secrets\firebase\serviceAccount.json"
     ```

4. For Docker/CI, mount the file into the container and set the same environment variable.
5. Never commit the real key to Git; only keep `serviceAccount.example.json` as a template.
