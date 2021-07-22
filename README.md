# Celestial
Coordinate overlay for Sea of Thieves

# How it works
The Celestial UI acts as a DLL injector and overlay. When you press the inject button it will inject an Astro.dll into the Sea of Thieves process, which will then bind to a localhost socket on port 8000 and broadcast out the player's XYZ coordinates. The coordinates are displayed in an overlay that tracks the position of the game on screen. Hitting CTRL+F will save the current location and start a new row.

![Demo](https://github.com/andreidorin13/Celestial/blob/main/sample.gif "Demo")

### Note: The pointers on the stack to get the player location change every patch and need to be updated.