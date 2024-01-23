# Osmy

[English](README.en-US.md) | 日本語

SPDX ドキュメントを用いたソフトウェアの管理を行うためのツール

## ツールの構成

Osmy はサーバープログラムと CLI/GUI クライアントから構成されます．

### サーバー

- ソフトウェアの情報（表示名，SPDX ドキュメントの情報など）を保持
- 管理対象ソフトウェアの脆弱性診断とチェックサムの検証を定期的に実行

### CLI/GUI クライアント

- 管理するソフトウェアの追加，削除，情報の確認
- 診断結果の確認

<img src="doc/tool-overview.svg">

## 実行方法

サーバーおよびクライアントの実行には.NET 8 以降をインストールする必要があります．
また，サーバーでは，SPDX ドキュメントのフォーマット変換のために，[spdx/tools-java](https://github.com/spdx/tools-java)を利用しているため，
Java 11 以上がインストールされ，`java`コマンドにパスが通っている必要があります．

### サーバー（Osmy.Server）

> [!NOTE]
> Linux で実行する場合は，`/var/run/Osmy`，`/var/lib/Osmy`および`/etc/Osmy`以下に書き込みを行うため，root 権限が必要です．

```PowerShell
# Ubuntu
# root権限が必要
sudo ./Osmy.Server

# Windows
./Osmy.Server.exe
```

脆弱性診断およびファイルのチェックサム検証の実行間隔などの設定ファイルについては，
[サーバーの設定](doc/server-setting.md)を参照してください．

### GUI クライアント（Osmy.Gui）

> [!WARNING]
> Linux で実行する場合は，使用ライブラリの不具合（<https://github.com/rioil/Osmy/issues/20>）を回避するためにラテン文字ロケールで起動する必要があります．

```PowerShell
# Ubuntu
LC_ALL=C ./Osmy.Gui

# Windows
./Osmy.Gui
```

### CLI クライアント（Osmy.Cli）

以下のように引数なしで実行するとヘルプが表示されます．

```PowerShell
# Ubuntu
./Osmy.Cli

# Windows
./Osmy.Cli
```

## ライセンス

[LICENSE.md](LICENSE.md)

```Text
SPDX-License-Identifier: MIT
PackageLicenseDeclared: MIT
```
