# LokoMobileApp

## Install deps
- dev environment setup (macOS)
  - install visual studio code
    - install .net maui extension
    - install .net sdk 10
    - install .net maui sdk
  - install xcode
    - install latest iOS simulator
  - install microsoft jdk 17
  - install android studio
    - install android sdk

## Build guide
```
# ensure .NET and MAUI workloads installed
dotnet --info
dotnet workload install maui

# from repo root
cd "Loko MAUI App/LokoMaui"

# restore and build solution
dotnet restore "LokoMaui.sln"
#dotnet build "LokoMaui.sln" -c Debug

# use InstallAndroidDependencies to build
dotnet build -t:InstallAndroidDependencies -f:net10.0-android -p:AndroidSdkDirectory="~/Library/Android/sdk" -p:AcceptAndroidSDKLicenses=True

# publish for Android (example)
dotnet publish -f net10.0-android -c Release -o ./publish/android
```
