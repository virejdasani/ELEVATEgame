# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.0.3] - 2020-07-13
### Fixed
- Documentation matches now the version 2.0.1 and newer.

## [2.0.2] - 2020-06-18
### Changed
- [Project] Updated LICENSE
- [Project] Updated README

### Fixed
- [Bugfix] Fixed: WebGL Publisher still states that WebGL Module is not installed after installing the module. I need to relaunch WebGL Publisher.

## [2.0.1] - 2020-06-09
### Added
- [Feature] Added support for Unity 2020
- [Feature] A popup for changing build target is now displayed if the user tries to build a WebGL game while the project is using a different build target
- [Feature] Before building and sharing, the package now detects if the WebGL module is installed, and asks the user to install it if it can't be found.
- [Feature] The "Upload" tab has been improved: you can now manage up to 10 recent builds, of which information such as unity version, build date and size is displayed
- [Feature] Added a button to delete a build (Upload tab)
- [Feature] Added the possibility to remove a build from the list without deleting its the files (Upload tab)
- [Feature] Added a button to share a specific build (Upload tab)
- [Feature] Added a button to open a build's folder (Upload tab)
- [Feature] The user can now locate an existing build through a dropdown menu in the Upload tab
- [Feature] the Upload tab now provides a button that allows users to build the game again in a different location, preserving the old build
- [Feature] Added an Introductive window for users that open the Share package for the first time of their project's lifetime
- [Feature] You can now open the Build Settings window by clicking a button in the Upload tab
- [Feature] If the user tries to build without any active scene added to build settings, the BuildSettings window opens for him.
- [Feature] Clicking "Create a build" automatically opens the build dialog if the project settings (platform, scenes) are correct
- [Feature] Added an "InstallWebGL" tab that tells the user how to install WebGL module if they don't have it
- [Feature] The "NotLoggedIn" tab now automatically pops-up the login tab if the user presses "Login"
- [Editor] One can now get the first-time instructions back by going to Edit > Preferences > My Settings > Publish WebGL Game > Show first-time instructions.

### Changed
- [UI] All tabs have been redesigned to improve clarity and user experience
- [UI] Added light/dark editor theme support
- [UI] Updated the link related to Installing WebGL
- [UI] Update terminology from "Share" to "Publish"
- [UI] Moved the menu item that opens the window from "Windows/Share WebGL" to "Publish/WebGL Project"

- [Optimization] Hugely reduced the amount of requests sent for login, progress and upload checks
- [Optimization] Reduced memory garbage

- [Bugfix] Fixed game title being unset sometimes
- [Bugfix] Fixed: Editor crashes when using "Auto switch + build" feature
- [Bugfix] Fixed build not uploading in case of missing GUID
- [Bugfix] Broken builds due to missing GUID.txt are now correctly recognized
- [Bugfix] Fixed a bug where the Share process would break if the user manually deleted some core build files.
- [Bugfix] Fixed: As soon as I try to add the third build, the oldest build is removed from the list even if the new build is invalid
- [Bugfix] Non MicroGames projects now produce correct dependency file after build
- [Bugfix] Fixed missing default thumbnail preventing sharing
- [Bugfix] Fixed broken upload due to missing thumbnail file
- [Bugfix] Fixed: It is possible to add the same build path several times using the "Locate Build" button (NEW-322)
- [Bugfix] Fixed missing icon causing UI not loading in non-MG projects

### Removed
- [Removed] It is no more possible to setup the game's title and thumbnail in the upload tab

## [1.2.0] - 2020-03-19
### Changed
- Converted the package to use UIElements for the UI, dropping the UIWidgets dependency completely.
### Added
- The window reacts on the user's login and logout dynamically.
- Build output directory is now remembered even if the window is closed or the project is restarted.
Therefore, no new build is needed if the project is restarted without any changes.
- "Finish" button to go from the Success state to the Upload state.
- Editor Coroutines package as a dependency.
### Fixed
- All the states that have a thumbnail image now display the last valid thumbnail.
- User is now redirected to the Login state if the user signs out and then switches to another state.
- The name of the project is now remembered if you encounter an error while selecting the thumbnail.
- The game title was not set in certain cases.
- When the window is closed, the system stops checking if the user is logged in.

## [1.1.0] - 2020-03-16
### Changed
 - Raised the required Unity version to 2019.3.
### Fixed
- Null reference exception when `applicationIdentifier` not set in `ProjectSettings.asset`. Use `productName` as a fallback.

## [1.0.10] - 2020-02-24
### Added
 - Support for authorized URLs (Unity Connect auto-login).
 - Automatically log in the user when the URL for the shared game is clicked.
 - Open the shared game automatically in the web browser when the upload is completed.

## [1.0.9] - 2019-12-13
### Changed
- Upload metadata: use `applicationIdentifier` in `ProjectSettings.asset` for retrieving the Microgames's name.
### Fixed
- Upload metadata: fix obtaining the project's dependency packages for Unity 2019.2 and newer.

## [1.0.8] - 2019-12-10
### Changed
- Use `Application.companyName` (instead of `identifier`) for retrieving the Microgames's name for upload metadata.

## [1.0.7] - 2019-11-28
### Changed
- Updated UIWidgets dependency.

## [1.0.6] - 2019-11-14
### Changed
- Use *Share* as the window title.
- Minor wording adjustments.
### Added
- Save and load the state of the window upon assembly reloads.
- Add upload metadata (used Unity version & dependencies) to the WebGL build folder.

## [1.0.5] - 2019-08-14
- Send `buildGUID` to Connect backend.

## [1.0.4] - 2019-05-27
- Remove unused package files.
- Updated UIWidgets dependency.

## [1.0.3] - 2019-05-27
- Add WebGL game .zip size limit check (max. limit is 100 MB).

## [1.0.2] - 2019-05-22
- Update package name & description.
- Remove QAReport from the package.

## [1.0.1] - 2019-05-22
- Check the thumbnail image size.
    
## [1.0.0] - 2019-05-20
This is the first release the Unity package *Share WebGL Game*.
