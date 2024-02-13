# As of the Arms Race Update Magic Bullet was fixed and therefore this plugin is no longer being updated.

# MagicBulletFix
CounterStrikeSharp plugin to block magic bullet in CS2

# Configuration
In the MagicBulletFix.json file you can change these plugin options:

**ChatMessage** changes the message output when magic bullet is detected

**FixMethod** lets you change between the behavior when detected:
* 0 allows the damage
* 1 ignores the damage completely
* 2 reflects the damage onto the person who fired the bullet
* 3 is the same as 2 but does not let the person die (remains at 1hp)

**ReflectScale** changes the scaling of the damage from the magic bullet. Scale 1 is 100% of the original damage, 0.1 is 10% of the original damage. On FixMethod 0 this value is not used.

# Note
If using reflect, the user may get banned for suiciding too many times.
