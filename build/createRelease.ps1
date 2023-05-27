# ビルドするランタイム
$runtimes = @(
  "win-x64",
  "linux-x64"
)

# ビルド対象プロジェクト
$projects = @(
  "Osmy.Server",
  "Osmy.Cli",
  "Osmy.Gui"
)

$software = "Osmy"
$buildConfiguration = "Release"
$publishFolder = "publish"
$mainProject = $projects[0]

$rootPath = Split-Path $PSScriptRoot -Parent
$publishPath = "$rootPath\$publishFolder"

$stopWatch = New-Object -TypeName System.Diagnostics.Stopwatch
$stopWatch.Start()

# バージョンを取得
$projectXml = [Xml] (Get-Content "$rootPath\$mainProject\$mainProject.csproj")
$version = "$($projectXml.Project.PropertyGroup.VersionPrefix)$($projectXml.Project.PropertyGroup.VersionSuffix)".Trim();
if ([string]::IsNullOrWhiteSpace($version)) {
  Write-Error "Can not detect the $software version."
  Write-Error "Make sure that $mainProject\$mainProject.csproj exists, and VersionPrefix and VersionSuffix are defined."
  exit
}

Write-Output "============================================================"
Write-Output "Building Osmy v$version..."
Write-Output "============================================================"
Write-Output ""

# ランタイム毎にビルド
foreach ($runtime in $runtimes) {
  Write-Output "------------------------------------------------------------"
  Write-Output "Building target: $runtime"
  Write-Output "------------------------------------------------------------"

  $runtimeStopWatch = New-Object -TypeName System.Diagnostics.Stopwatch
  $runtimeStopWatch.Start()

  $publishName = "${software}_${runtime}_v$version"
  $runtimeDir = "$publishPath\$publishName"

  # 各プロジェクトをビルド
  foreach ($project in $projects) {
    dotnet publish "$rootPath\$project" -o "$runtimeDir\$project" -c $buildConfiguration -r $runtime -p:PublishReadyToRun=true --no-self-contained
  }

  # ビルドしたバイナリをzip圧縮
  $zipName = "$publishName.zip"
  $zipPath = "$publishPath\$zipName"
  Compress-Archive -Path $runtimeDir\* -DestinationPath $zipPath -Force
  Write-Output "Zipped to $zipPath"

  $runtimeStopWatch.Stop()
  Write-Output "------------------------------------------------------------"
  Write-Output "$runtime build finished in $($runtimeStopWatch.Elapsed)"
  Write-Output "------------------------------------------------------------"
  Write-Output ""
}

$stopWatch.Stop()
Write-Output "============================================================"
Write-Output "Build finished in $($stopWatch.Elapsed)"
Write-Output "============================================================"
