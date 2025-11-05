# GBS2XB

Hi! This is a sample project that will allow you to release your game made with GB Studio to the Microsoft Store, including PC, Xbox One and Xbox Series consoles.

## Sample - My game Dorotea 

You can try the results of this template by downloading my game Dorotea from the Microsoft Store. 

- Dorotea on Microsoft Store (Uses this project at its core): https://apps.microsoft.com/store/detail/9N2J4VXC7WXJ?cid=DevShareMCLPCS
- Dorotea on Itch.io (Uses nw.js for the PC version): https://pasquiindustry.itch.io/dorotea

Note that the version on Itch.io uses nw.js for the PC Version.

Also I'm using a custom launcher for the language selector. This launcher integrates with this project and launches two different instances of binjgb with two different ROMs

## Important notes and known Issues

__This is an experimental project. Some aspects are still not entirely defined and there may be some breaking issues. I'll also try to make a more detailed tutorial in the following days / weeks.__

Here are some notes and known issues

- On Xbox One, the app seems to fail to start. This is hevaily monitored.
- The B button on the Xbox Gamepad cannot be currently assigned, since it will close the game. In the future we can add a popup that informs the user that the game will close
- The game may still get inputs when a popup is opened on Xbox
- I advice you to read the documentation about making UWP apps and their certification process. This guide will assume that you have some basic knowledge of WebView2, WinUi2 / UWP, XAML and C#
- This guide was made with GB Studio 4.1.3 and the Windows 11 SDK 26100 (UWP / WinUi2, .NET 9). Some things may change in the future.
- The game can be installed on Hololens, but the input should not work.
- There aren't controls for closing the game or changing some settings, including the volume.

## Introduction guide

1. From GB Studio, make your game and save the project.
2. From the menu bar, click on Game > Export as > Export Web.
3. Take the contents of the folder and copy-paste to the Game folder of this project / solution. Be sure that there's an index.html file directly inside the Game folder.
4. Select all the files inside the Game folder manually and set the property "Copy to Output Directory" to "Copy if newer". Unfortunately Visual Studio 2022 doesn't seem to do this automatically. Be sure to double check that all the files have this property! Do not select the folders or the property won't appear, select just the files.

### Update the project
Here are some parameters and things to do to customize your project
- Rename the project with what name you prefer. If unsure, you can also try to replicate the app code in a new blank UWP Project.
- Update the parameters in Package.appxmanifest. The Display Name should be the same as the identity on the Microsoft Store
- Update the mandatory Assets. You can't release an app on the Microsoft Store with the default images
- In MainPage.xaml.cs, function MainPage_Loaded -> replace the strings "mygbstudio.game" with a different value. This is the virtual address that the WebView will navigate to
- In MainPage.xaml.cs, function SendKeyToWebView -> double check the key bindings, especially if you've changed them in GB Studio

## F.A.Q.

### Why we use a Web View and not a native emulator?
At the moment I wanted to make a simpler project that can be easily updated and can use the same files required by the Itch.io web player