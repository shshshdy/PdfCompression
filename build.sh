#!/bin/bash
BAS_PATH=$(cd `dirname $0`; pwd)
APP_NAME="$BAS_PATH/output/osx-x64/PDF压缩.app"
PUBLISH_OUTPUT_DIRECTORY="$BAS_PATH/publish/osx-x64"
INFO_PLIST="$BAS_PATH/mac/Info.plist"
ICON_FILE="$BAS_PATH/mac/logo.icns"

if [ -d "$APP_NAME" ]
then
    rm -rf "$APP_NAME"
fi

dotnet publish -c Release -r osx-x64 -o "$PUBLISH_OUTPUT_DIRECTORY"



mkdir "$BAS_PATH/output"
rm -rf "$BAS_PATH/output/osx-x64"
mkdir "$BAS_PATH/output/osx-x64"

mkdir "$APP_NAME"
mkdir "$APP_NAME/Contents"
mkdir "$APP_NAME/Contents/MacOS"
mkdir "$APP_NAME/Contents/Resources"

cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$ICON_FILE" "$APP_NAME/Contents/Resources/"
cp -a "$PUBLISH_OUTPUT_DIRECTORY/." "$APP_NAME/Contents/MacOS"