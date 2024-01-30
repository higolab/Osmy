# Server Settings

The configuration files are created in
`%PROGRAMDATA%\Osmy\Setting`（Windows）or `/etc/Osmy/`（Linux）
when you run Osmy for the first time.

To apply changes, you have to restart the server.

## General Settings `general.json`

```json
{
  "VulnerabilityScanInterval": "1.01:02:03",
  "ChecksumVerificationInterval": "12:34:56"
}
```

| Setting Item                 | Description                                                                                | Default Value  |
| ---------------------------- | ------------------------------------------------------------------------------------------ | -------------- |
| VulnerabilityScanInterval    | Interval of vulnerability assessments<br/>Format: (d.)hh:mm:ss                             | `"1.00:00:00"` |
| ChecksumVerificationInterval | Interval of file integrity verifications (checksum verifications)<br/>Format: (d.)hh:mm:ss | `"1.00:00:00"` |

## Notification Settings `notification.json`

```json
{
  "Email": {
    "IsEnabled": false,
    "Host": "mail.example.com",
    "Port": 587,
    "Username": "hoge",
    "Password": "password",
    "To": ["hoge@example.com", "fuga@example.com"],
    "Cc": ["foo@example.com"],
    "Bcc": ["bar@example.com"]
  }
}
```

| Setting Item | Description                              | Default Value |
| ------------ | ---------------------------------------- | ------------- |
| Email        | Email notification settings              |               |
| ├ IsEnabled  | Whether Osmy sends notification by email | `false`       |
| ├ Host       | Mail server host name                    | `""`          |
| ├ Port       | Mail server port number                  | `0`           |
| ├ Username   | Username                                 | `""`          |
| ├ Password   | Password                                 | `""`          |
| ├ To         | Email destination (To)                   | `[]`          |
| ├ Cc         | Email destination (Cc)                   | `[]`          |
| └ Bcc        | Email destination (Bcc)                  | `[]`          |
