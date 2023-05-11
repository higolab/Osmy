# Osmy

## ソリューションの構成

| プロジェクト名 | 説明                                                                                                                        |
| -------------- | --------------------------------------------------------------------------------------------------------------------------- |
| Osmy.Server    | SPDX ドキュメントの管理を行うサーバープログラム．動作中は，脆弱性診断およびファイルのチェックサム検証を定期的に実行します． |
| Osmy.Cli       | サーバーに保存された SPDX ドキュメントの管理を行う CLI クライアント                                                         |
| Osmy.Gui       | サーバーに保存された SPDX ドキュメントの管理を行う GUI クライアント                                                         |
| Osmy.Api       | Osmy.Cli と Osmy.Gui で利用するサーバーとの通信 API クライアント                                                            |
| Osmy.Core      | 複数のプロジェクトで共通して利用するクラス等を定義するコアライブラリ                                                        |

## 実行方法

プログラムの実行には.NET 6 以降をインストールする必要があります．
.NET のインストール方法は，<https://learn.microsoft.com/ja-jp/dotnet/core/install/>を参照してください．
このツールにはインストーラー等は存在しないため，ビルド済みのバイナリを任意の場所に配置して実行できます．

### サーバー（Osmy.Server）

Linux で実行する場合は，`/var/run/Osmy`および`/var/lib/Osmy`以下に書き込みを行うため，root 権限が必要です．
また，SPDXドキュメントのフォーマット変換のために，[spdx/tools-java](https://github.com/spdx/tools-java)

```powershell
# Ubuntu
# root権限が必要
sudo ./Osmy.Server

# Windows
./Osmy.Server.exe
```

脆弱性診断およびファイルのチェックサム検証の実行間隔などの設定ファイルは，
`%PROGRAMDATA%\Osmy\Settings`（Windows），`/etc/Osmy/`（Linux）
以下に初回起動時に作成されます．
設定の反映には，プログラムの再起動が必要です．

### GUI クライアント（Osmy.Gui）

Linux で実行する場合は，使用ライブラリの不具合（<https://github.com/rioil/Osmy/issues/20>）を回避するためにラテン文字ロケールで起動する必要があります．

```powershell
# Ubuntu
LC_ALL=C ./Osmy.Gui

# Windows
./Osmy.Gui
```

### Osmy.Cli

以下のように引数なしで実行するとヘルプが表示されます．

```powershell
# Ubuntu
./Osmy.Cli

# Windows
./Osmy.Cli
```

## ビルド方法

通常のC#プログラムとしてビルド可能です．
クローンしたソリューションファイルをVisual Studioで開いてビルドを行うか，
dotnetコマンドを用いてビルドしてください．
