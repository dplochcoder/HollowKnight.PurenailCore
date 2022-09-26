# PurenailCore

Common functionality for Purenail's mods

## ICUtil.PriorityEvents

Additional RandomizerMod.PriorityEvents for specific override functions.

MoreDoors and DarknessRandomizer use this to ensure they make SceneManager changes in a consistent order on scene load. This is necessary to ensure doors interop with darkness changes correctly.

## ModUtil.VersionUtil

Utility class for calculating the version string for a given assembly. Uses the SHA1 of the dll, mod 997.

## RandoUtil

### InfectionWallLogicFix

Makes the transition between Ancestral Mound and False Knight permanently out-of-logic in non-RoomRando, due to possible infection.

MoreDoors and DarknessRandomizer apply this fix due to the False Door and potential dark-crossroads respectively, since it may erroneously put False Knight in logic otherwise when it shouldn't be.

### LogicReplacer

An efficient, generic Logic rewriter that takes a map of substitutions.
