
# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.6.1]
### Added
- Able to subscribe for game switch events
- Tool to extract files from inside skinned folders and convert them into separate skinned files

## [0.6.0]
### Added
- Prevent skinning assets already inside skinned folder
- Added shaders to supported types
- Added icon overlay in project window on top of skinned assets

### Fixed
- Fixed issue where renaming an asset exclusively skinned to one game would fail
- Fixed issue where copying assets from skins to unity would fail if an asset parent folder did not exist

## [0.5.0]
### Added
- New improved Auto save feature supporting internal and external skins
- Ability to Save/Discard specific skins
- Ability to skin/remove asset exclusively for a game
- Overall improved GUI

### Fixed
- Fixed issue where skinning operation would fail if project path contained space characters
- Fixed issue when skinning where original asset was not being removed from repo

## [0.4.3]
### Removed
- Auto save feature as it was causing devs to not be able to revert changes, save is now a manual operation.

## [0.4.2]
### Fixed
- Fixed issue where project in batchmode would timeout while performing game switch operation

## [0.4.1]

### Added
- Update GUI to be tab based
- Support for skinning mono scripts
- Support for switching game through command line (support for jenkins)

### Fixed
- Fixed issue where addressables would not load due to shapeshifter attempting to run while entering playmode

## [0.4.0]

### Warning
This version requires adding a "/" to end of any git ignore path related to a folder. 

### Fixed
- Fixed issue where opening project for the first time would fail to restore any skinned folder

### Added
- Validating path safety to prevent any possible edge case where a copy/delete operation would target the root folder or any other dangerous target path.

## [0.3.1]
### Fixed
- Fixed issue where opening skin folders externally would create DS_Store files that fooled shapeshifter 
into thinking there was still assets to be restored into the project
- Fixed external skinning not adding files to git ignore

## [0.3.0]
### Added
- Drop To Replace: Drag external files on top of selected asset skin preview to quickly replace it
- Skin assets on project window by Right Clicking -> ShapeShifter -> Skin Selected Assets
- Pre Merge Check Tool to anticipate merge conflicts

### Fixed
- Fixed issue where an infinite save loop would occur when having selected a nested prefab while in prefab mode with Auto Save enabled.
- Fixed issue when clicking "Remove All Skins" twice
- Fixed Scroll not working on asset skinner section when multiple assets are being previewed

