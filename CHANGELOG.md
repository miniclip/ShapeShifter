# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
- Fix Scroll not working on asset skinner section when multiple assets are being previewed

