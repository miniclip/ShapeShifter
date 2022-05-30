# Shape Shifter 🌗

<p align="center">
<img alt="Version" src="https://img.shields.io/github/v/tag/miniclip/shapeshifter?label=version" />
<a href="https://github.com/miniclip/shapeshifter/issues" target="_blank">
<img alt="GitHub issues" src ="https://img.shields.io/github/issues-raw/miniclip/shapeshifter" />
</a>
<a href="https://github.com/miniclip/shapeshifter/pulls" target="_blank">
<img alt="GitHub pull requests" src ="https://img.shields.io/github/issues-pr-raw/miniclip/shapeshifter" />
</a>
<img alt="GitHub last commit" src ="https://img.shields.io/github/last-commit/miniclip/shapeshifter" />
</p>
<p align="center">
<a href="https://www.codacy.com/gh/miniclip/ShapeShifter/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=miniclip/ShapeShifter&amp;utm_campaign=Badge_Grade"><img src="https://app.codacy.com/project/badge/Grade/2ecd3052e8204654ab5a4e2fc5d5329a"/></a>
<a href="https://github.com/miniclip/shapeshifter/blob/master/LICENSE.md" target="_blank">
<img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-blue.svg" />
</a>
</p>

## What is ShapeShifter?

ShapeShifter is a novel approach to skinning and switching between different games inside the same Unity project.

By switching your skinned assets with alternate versions while maintaining its original GUID, this tool allows for
multiple versions of the same assets to coexist without ever losing references.

It supports the following asset types:

- AnimationClip
- AnimatorController
- Folders
- GameObject
- MonoScript
- Prefabs
- SceneAsset
- ScriptableObject
- Shader
- Texture2D
- TextAsset

### Caution!

- This is an experimental package with no official release yet. Take into account that it might bring unexpected
  problems into your project. **Please make sure to backup your work before integrating ShapeShifter.**

## Preview

Here you can see some examples of what ShapeShifter is able to do:

<details>
  <summary>Skinning a sprite</summary>

- After skinning a sprite, you'll be able to change the texture for each configured game version. 
- After that, you can
  simply do a Game Switch operation and ShapeShifter will take care of replacing all skinned assets with the target game
  version you've selected.
- You can see the Game Switch changes happen in real time, even on a scene currently using that sprite!

![Step 1](/Documentation~/Examples/Sprite-01-SkinSprite.gif)
![Step 2](/Documentation~/Examples/Sprite-02-ReplaceVersionB.gif)
![Step 3](/Documentation~/Examples/Sprite-03-SwitchSprites.gif)
![Step 4](/Documentation~/Examples/Sprite-04-SwitchInsideScene.gif)

</details>

<br>

<details>
  <summary>Skinning a prefab</summary>

- With ShapeShifter you'll be able to experiment different designs while using the same prefab and maintaining all its
  references all the time.
- Notice that each game version will replace, not only the sprite from the previous example, but the whole prefab layout
  as well.

![Step 1](/Documentation~/Examples/Prefab-SkinAndChangePrefab.gif)

</details>

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
