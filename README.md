# GBS2XB

Hi! This is a sample project that will allow you to release your games made with GB Studio onto the Microsoft Store, including PC, Xbox One and Xbox Series consoles.

> Note: this is an experimental project. Some aspects are still not defined. I made this project tailored to my games

## Requirements and what I used

I made with with Visual Studio 2022 Community on Windows, .NET 9.
If you use the Visual Studio 2022 Installer, you should enable the WinUi tools, Windows 11 SDK 26100

## Introduction guide

- I advice you to read the documentation about making UWP apps and their certification process. This guide will assume that you have some basic knowledge about WebView2, WinUi2 / UWP
- This guide was made with GB Studio 4.1.3 in mind. Some things may change in the future.

### Export from GB Studio

1. Make your game and save
2. From the menu bar, click on Game > Export as > Export Web.
3. Take the contents of the folder and copy-paste to the Game folder of this project. 