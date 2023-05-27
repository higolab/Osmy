# Osmy

## ツールの構成

このツールは，SPDXドキュメントの情報を保持し，脆弱性診断とチェックサムの検証を定期的に実行するサーバープログラムと，サーバープログラムと通信して管理するSPDXドキュメントの追加，削除，情報の確認などを行うクライアントプログラムからなります．
クライアントプログラムには，コマンドラインで実行するCLIクライアントと，画面上で操作を行うGUIクライアントが存在します．

## ソリューションの構成

| プロジェクト名 | 説明                                                                                                                        |
| -------------- | --------------------------------------------------------------------------------------------------------------------------- |
| Osmy.Server    | SPDX ドキュメントの管理を行うサーバープログラム．動作中は，脆弱性診断およびファイルのチェックサムの検証を定期的に実行します． |
| Osmy.Cli       | サーバーに保存された SPDX ドキュメントの管理を行う CLI クライアント                                                         |
| Osmy.Gui       | サーバーに保存された SPDX ドキュメントの管理を行う GUI クライアント                                                         |
| Osmy.Api       | Osmy.Cli と Osmy.Gui で利用するサーバーとの通信 API クライアント                                                            |
| Osmy.Core      | 複数のプロジェクトで共通して利用するクラス等を定義するコアライブラリ                                                        |

## 実行方法

サーバーおよびクライアントの実行には.NET 6 以降をインストールする必要があります．
.NET のインストール方法は，<https://learn.microsoft.com/ja-jp/dotnet/core/install/>を参照してください．
また，サーバーでは，SPDXドキュメントのフォーマット変換のために，[spdx/tools-java](https://github.com/spdx/tools-java)を利用しているため，
Java 11以上がインストールされ，`java`コマンドにパスが通っている必要があります．

なお，このツールにはインストーラー等は存在せず，ビルドしたバイナリを任意の場所に配置して実行できます．

### サーバー（Osmy.Server）

Linux で実行する場合は，`/var/run/Osmy`，`/var/lib/Osmy`および`/etc/Osmy`以下に書き込みを行うため，root 権限が必要です．

```powershell
# Ubuntu
# root権限が必要
sudo ./Osmy.Server

# Windows
./Osmy.Server.exe
```

脆弱性診断およびファイルのチェックサム検証の実行間隔などの設定ファイルについては，
[サーバーの設定](doc/server-setting.md)を参照してください．

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

ソリューションファイルをVisual Studioで開いてビルドを行うか，
[PowerShellスクリプト](build/createRelease.ps1)を実行することでビルドできます．

dotnetコマンドを直接実行する場合は以下のように引数を与えます．

```powershell
# Windows
dotnet publish <プロジェクトのパス> -o <出力先ディレクトリ> -c Release -r win-x64 -p:PublishReadyToRun=true --no-self-contained

# Linux
dotnet publish <プロジェクトのパス> -o <出力先ディレクトリ> -c Release -r linux-x64 -p:PublishReadyToRun=true --no-self-contained
```
