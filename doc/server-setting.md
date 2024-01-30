# サーバーの設定

脆弱性診断およびファイルのチェックサム検証の実行間隔などの設定ファイルが，
`%PROGRAMDATA%\Osmy\Setting`（Windows），`/etc/Osmy/`（Linux）
以下に初回起動時に作成されます．

設定の反映には，サーバーの再起動が必要です．

## 一般設定 `general.json`

```json
{
  "VulnerabilityScanInterval": "1.01:02:03",
  "ChecksumVerificationInterval": "12:34:56"
}
```

| 項目                         | 説明                                                       | デフォルト値   |
| ---------------------------- | ---------------------------------------------------------- | -------------- |
| VulnerabilityScanInterval    | 脆弱性診断の実行間隔<br/>(d.)hh:mm:ss の形式で指定         | `"1.00:00:00"` |
| ChecksumVerificationInterval | チェックサムの検証の実行間隔<br/>(d.)hh:mm:ss の形式で指定 | `"1.00:00:00"` |

## 通知設定 `notification.json`

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

| 項目        | 説明                                           | デフォルト値 |
| ----------- | ---------------------------------------------- | ------------ |
| Email       | メール通知設定                                 |              |
| ├ IsEnabled | メール通知を行うか（ture:行う false:行わない） | `false`      |
| ├ Host      | メールサーバーのホスト名                       | `""`         |
| ├ Port      | メールサーバーのポート番号                     | `0`          |
| ├ Username  | ユーザー名                                     | `""`         |
| ├ Password  | パスワード                                     | `""`         |
| ├ To        | メールの宛先                                   | `[]`         |
| ├ Cc        | メールの宛先（Cc）                             | `[]`         |
| └ Bcc       | メールの宛先（Bcc）                            | `[]`         |
