The FixPrefabLink.cs was a simple tool I wrote to fix a mistake where I had accidentally edited positions of lots of lights in play mode. I had copied the lights when I realized that I was in fact in play mode and that my changes would be discarded otherwise, but all of the prefab links to the light prefab had been broken upon pasting the lights outside of play mode.

So this script assumes that the input objects have the same structure as a input prefab and simply restores their prefab link.
