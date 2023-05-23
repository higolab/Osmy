# サーバーの設定

脆弱性診断およびファイルのチェックサム検証の実行間隔などの設定ファイルが，
`%PROGRAMDATA%\Osmy\Settings`（Windows），`/etc/Osmy/`（Linux）
以下に初回起動時に作成されます．

設定の反映には，サーバーの再起動が必要です．

## 一般設定 `Common.json`

```json
{
  // 脆弱性診断の実行間隔
  // (d.)hh:mm:ssの形式で指定します
  // この例では実行間隔が1日1時間2分3秒に設定されています
  "VulnerabilityScanInterval": "1.01:02:03",

  // チェックサムの検証の実行間隔
  // (d.)hh:mm:ssの形式で指定します
  // この例では実行間隔が12時間34分56秒に設定されています
  "ChecksumVerificationInterval": "12:34:56"
}
```

## 通知設定 `Notification.json`

```json
{
  // メール通知設定
  "Email": {
    // メール通知を行うか
    // ture:行う false:行わない
    "IsEnabled": false,

    // メールサーバーのホスト名
    "Host": "mail.example.com",

    // メールサーバーのポート番号
    "Port": 587,

    // ユーザー名
    "Username": "hoge",

    // パスワード
    "Password": "password",

    // メールの宛先
    "To": [ "hoge@example.com", "fuga@example.com" ],
    // メールの宛先（Cc）
    "Cc": [ "foo@example.com" ],
    // メールの宛先（Bcc）
    "Bcc": [ "bar@example.com" ]
  }
}
```
