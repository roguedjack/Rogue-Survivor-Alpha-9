========================
STATS MODDING QUICK HELP
by roguedjack
========================

1. OVERVIEW
2. MODDING...
	2.1 ACTORS.
	2.2 ITEMS.
	2.3 SKILLS
	

1. OVERVIEW
===========	

DISCLAIMER
----------
Please keep in mind that modding is limited. 
The design originally didn't support data files modding and it was added as a bonus.
So expect limitations and incosistencies if you try to make it do what it is not designed to do.
Remember to make a copy of the original data files before editing.
In addition to reading this mini-manual, you'll probably have to do some guessing to see what works and what doesn't.


CSV FORMAT
-----------
CSV files are easily editable in Open Office or Microsoft Office, or even a simple text editor.
** Remember to properly format the cells to their values type in Office **
CSV has the advantage of being easily editable, lightweight and readable by anyone.
On the other hand, it is very rigid and not extensible. 
For this reason, new versions of the game that make significant changes will likely break mod compability because of a field format change.


CONVENTIONS
-----------
The game csv parser is very simple and will go insane, or at best spout exceptions, if you give it a wrong format.
- The first line is skipped by the game, expecting it to be the table header.
- Some headers fields describe what the value means when it is not obvious.
- Line ordering is not important, the game read the IDs field to recognize entities. A missing ID will cause an exception.
- C-style \" combo inside of quoted strings will confuse the parser. Don't do it.
- Most numeric values are integers and are quantities, duration in hours or number of points.
- Some numeric values are floats, with are always dotted (eg: 1.0 for 1). No dot means no float wanted there.


2. MODDING...
=============

You should know and understand the game pretty well before attempting modding, since not everything is explicited in the data files.
You can mod only some stats and not change abilities or add new entities.
This is because the game engine relies on a number of assumptions about some actors or items.
In other words, the RS game engine is specifically designed and coded to handle RS gameplay in the way the game author envision it.
RS is not a game built on a generic engine, the game and the engine go hand by hand.
This is a strong design decision that won't change.
Modding RS is meant to balance the game the way you like it (stats mod), or change the look and feel of the game (graphics + stats mod).
The game will not handle well (engine or interface) values that varies too much from the default ones.


2.1 ACTORS
----------
Some stats are not revelant or used by the game at all, depending on the actor.
You can't change the abilities of actors by changing values that are not used.
Remember the stats do not include the effects of skills.

STA		
Unused by undeads.
99 is a placeholder value and does not mean infinite stamina.

AUDIO
Unused by undeads.

SMELL
Unused by livings. Unused by Skeleton AI.
Threshold as a percentage of the maximum scent strength.
Undeads, both AIs and PC, won't "see" scents that are too weak for them to smell.
0 effectively "blinds" an undead to all scents.
100 makes him able to smell even the weakest of scents.


2.2 ITEMS
-------------
There are some hardcoded contraints for some specific items, and changing some values will have no effect.

EXPLOSIVES
"Blast" columns with number greater than "Radius" are ignored.
You can change the radius of explosives.
The hardcoded maximum radius accepted by the game is the number of "Blast" columns.
You can't extend the maximum radius by adding "Blast" columns.

FOOD ITEMS
Perishable food items cannot be made stackable, and vice-versa, they are harcoded that way.
"BestBefore" column is ignored for non-perishable items.
"StackingLimit" column is ignored for perishable items.

MEDECINE ITEMS
The engine allows for medecine items to have multi-effects and negative values.
Eg: you might want the medikit to both heal and recover stamina and the green pills to penalize sleep.

The vanilla items have no meds which make have these properties, but you can mod them to.
The engine and AIs will make use of multi-effects and negative values properly.

RANGED WEAPONS
The type of ammo associated to each ranged weapon is harcoded.


2.3 SKILLS
----------
Value fields semantic depends entirely on the skill.
All fields accept float values, but some will be cast to integers anyway.
You cannot change the basic effects of skills, only rebalance them.
Changing probabilities, percentage and stats increase skill is safe.
Changing distances or inventory slots number can lead to game inconsistencies or UI glitches.
If you are unsure what value does what, make a change and look at the skill description in game.


HAPPY MODDING!

----
roguedjack