# 1.3.1
- Simulations now copy your inventory in real time instead of when summoned
- Simulations will now prefer to stay inside the radius of Iterate when it is active
- Attacks from simulations with a proc coefficient higher than 0 but less than 1 are upgraded to 1
- Dismantle's targeting reticle now only locks control if it hits an object instead of also when M1 is released (you can pause it and then move it again as long as it hasnt detonated yet)
- Dismantle damage nerfed from 700% to 560%
- Data UI now has smoothing for both the number and the visual
- Data gained above the cap can now be stored as reserve, up to 75% of maximum. Reserve data can not be used directly by skills, but will be drained to replenish missing data when out of combat and danger
- Vrab now has a shadow ???
- Fixed Vrab's description having outdated info (talked about Deconstruct being ground-only, had a tip for Refresh instead of Iterate)

# 1.3.0
- Deconstruct now forces simulations to target the hit enemy
- Deconstruct now slows targets
- Fixed Deconstruct not being tagged as primary skill damage
- Allies copied from Simulate now follow you to the next stage if kept alive
- Bosses can now only be simulated to a max of 1 across all boss types. Trying to simulate a different boss will kill the old one.
- False Son and Mithrix can no longer be copied (False Son copies Halcyonite and Mithrix copies a random lunar chimera)
- Solus Wing's copy is now the Alloy Hunter instead of Solus Control Unit
- Solus Heart now copies the Alloy Hunter instead of spawning a dormant pre-fight Solus Heart that does nothing
- Simulate no longer copies allies
- Simulations now receive double the bonus from overload for stats except regen
- Simulations have a different visual that is much less fucking blinding
- Added an alternate primary, Dismantle (yes it has the same icon as Deconstruct for now idc enough to make a shitty icon for it today)

# 1.2.0
- Refresh utility skill is now behind a config and disabled by default
- Replaced utility skill with Iterate
- Overload is no longer applied to enemies
- Simulated allies under the effects of Overload now regenerate 5% HP/s instead of decaying
- Overload no longer gives movement speed
- Overload attack speed and damage buffs raised to 30%
- Simulated allies now teleport after you when far away
- Deconstruct data generation buffed from 7% -> 10% per second

# 1.1.4
- Fixed issue where Vrab's presence in multiplayer would lag the shit out of the game only in certain modpacks somehow ???
- Simulate no longer copies Transcendence

# 1.1.3
- Fixed all bosses being untargetable for Simulate (im dumb and was checking the ungrabbable flag to catch solus wing canisters and that included all bosses)

# 1.1.2
- Deconstruct now massively prioritizes enemies over allies unless the ally is at critical health (under 30% max hp)
- Pelagic Drift no longer makes you keep bouncing off the ground if you have space still held down when landing

# 1.1.1
- Reverted targeting angle change

# 1.1.0
- Deconstruct now heals allies at a rate of 5% max health per second in exchange for 7% data per second
- Deconstruct is now properly only disabled when drifting, not just for being airborne in general
- Deconstruct no longer spawns the beam vfx if the channel would be cancelled immediately for any reason (prevents vfx spam in some situations)
- Analyze bolt damage reduced to 600% (was 800%)
- Analyze data gain reduced to 5% (was 7%)
- Fixed Analyze not giving data from converted projectiles when used by a client
- Simulated allies no longer inherit healing items
- Simulate no longer fails on enemies with a base max health greater than 2,400
- Simulate no longer goes on cooldown if it fails to copy the target (missing data, uncopyable target)
- Simulate is now capped at 3 same-enemy clones for standard enemies, and 1 same-enemy clone for bosses (there is no hard cap to total number of clones, just the amount of each enemy type you may have)
- Locking onto an enemy now shows an indicator of how much data is required to simulate them. Attempting to simulate a target without the required data no longer creates the ERR! indicator.
- Attempting to simulate Solus Wing, Solus Heart, or Voidling will now summon a Solus Control Unit (wing, heart) or Void Devastator (voidling) instead
- Pelagic Drift no longer consumes data for the first 1 second of flight. Free-flight timer resets upon touching the ground again.
- Pelagic Drift now consumes 7% data per second (down from 10%)
- Pelagic Drift now has a much quicker ascent speed for a brief moment after beginning flight
- Refresh no longer consumes data
- Refresh's overload attack speed buff reduced to 25% (was 50%)
- Refresh's overload now increases damage by 25%
- Vrab's shell no longer gets fucked by lobby and certain stage lighting

# 1.0.3
- fixed for AC

# 1.0.2
- fixed vrab being untargetable by certain things and breaking ally ai targeting (this fixes the halcyon shrine bug)
- simulated enemies now immediately drop a dead target instead of attacking their corpse for a few seconds
- simulating a flying enemy now spawns it in the air
- simulated enemies no longer shoot at you when you're more than 50m away from them
- analyze now captures projectiles way more reliably
- analyze no longer captures analysis bolts

# 1.0.1
- fixed m2, util, and special activating without delay if you had extra charges
- improved targeting of deconstruct
- pelagic drift now only consumes data 0.7s after leaving the ground for the first time (you still get a boosted initial jump but simply jumping no longer consumes data)

# 1.0.0
- release