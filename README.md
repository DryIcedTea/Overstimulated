# Overstimulated! All you Can Eat
## A [Buttplug.io](https://buttplug.io/) mod for Overcooked! All You Can Eat

This mod allows you to connect your Buttplug.io compatible toys to Overcooked! All You Can Eat!

## FEATURES
* Vibrations on using the cutting board
* Vibrations on washing dishes
* Vibrations on dashing
* Vibrations on death (lasts until you respawn)
* Vibrations on new/completed/falied order
    * Seperate vibrations for new, completed and failed orders
* Vibrations on using the fire extinguisher/water gun

## INSTALLATION
1. Install [Intiface Central](https://intiface.com/central/)
   - This is what the mod uses to communicate with your toys.
2. Install [BepInEx 6](https://builds.bepinex.dev/projects/bepinex_be)
   - **IMPORTANT: Make sure you're not downloading BepInEx 5! The mod will not work with BepInEx 5!**
   - **On the list of builds, select the one that says BepInEx Unity (IL2CPP) for Windows (x64) games (or mac)**
   - [Install guide](https://docs.bepinex.dev/master/articles/user_guide/installation/unity_il2cpp.html) (You want x64, not x86)
   - If you're having problems with this step, feel free to contact me on twitter, bluesky or discord!
3. Install the Overstimulated! All You Can Eat Mod from the releases page and place the "OvercookedBP folder" in your BepInEx/plugins folder.
4. Launch the game and enjoy! (the config file shows up after you launch the game for the first time)

## Config
* The game config can be found in the BepInEx/config/ folder found in your game directory.
* The config file is called "DryIcedMatcha.OvercookedBP.cfg"
  * To edit which player triggers vibrations, see the multiplayer section below.

## MULTIPLAYER
Overstimulated! is compatible with multiplayer but there are some things to keep in mind:
* By default, only Player 1 will trigger vibrations.
* You can change this by pressing the "INSERT" button in game to open a config menu
  * From there, you can press 1-4 to toggle which players can trigger vibrations. Press INSERT to close the menu when you're done.
  * This is reset to default when you close the game, but it's quick to change so I hope that's okay.
* If you're playing online multipleyer, only local players will trigger vibrations.
    * This means that only players on your computer will trigger vibrations, not players on other computers, regardless of config setting. (I doubt this will be a big issue, but feel free to contact me if this is a big problem)
      * This is a limitation of how the mod checks what player is doing what, but the mod should register you as player 1 by default, so you should be able to trigger vibrations without changing any settings.

## FAQ
### What toys does this work with?
Check Out [https://iostindex.com/](https://iostindex.com/).
### Can I use this in multiplayer?
Yes! See the multiplayer section above for details.
### Does it support multiple toys at once?
Yes!
### Will you add or change X feature?
Maybe. Please let me know what you want to see added/changed!

## Feedback
If you have any feedback, feel free to open an issue, [tweet](https://twitter.com/DryIcedMatcha) at me, tag me on [bluesky](https://bsky.app/profile/dryicedmatcha.bsky.social) or message me on discord. You'll find me in the [buttplug.io discord](https://discord.buttplug.io/)!
You're most likely going to get a quicker response on Discord.

## Potentially known issues (**please** let me know if you encounter any of these)
* Vibrations might sometimes fail to trigger or get stuck on until you trigger another vibration.
  * Hitting the "dash" button should fix it for now. I'm looking into why this is happpening.
  * Please let me know if you run into this so I know that it's not just a problem on my end!
* Vibrations can get interrupted if another vibration fires while one is already going.
  * This will be fixed for the full release. 
