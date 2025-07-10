# ProjektKiosk
These hold the project files for the project (Projekt Kiosk)
To be able to open and build the project through webGL you will have to install the webGL platform and install the Unity version: 2022.3.58f1

/\/\/\/\ PROJECTKIOSK TIPS & KNOW-HOWS /\/\/\/\

-Entire dialogue/Narrative is handled by the DialogueManager gameobject and script(s) attached
-ItemManager gameobj & attached scripts handle picking up items and any item interaction
-SoundManager is obvious

/\ TO CREATE/ADD/EDIT NPCS: /\
To add NPC:
-Inside DialogueManager inspector add a new customer.
-To that customer, add lines (of dialogue)
-To add responses (that the player can choose from) on a line of dialogue, add responses in the inspector
 of the line of dialogue(element) where you want the responses to appear.
-In the MainCanvas gameobj, add your NPC sprite and the same scripts the existing NPCs have on them.
(to edit what npc wants to buy edit Requested Items in NPC Request script inspector on any npc)
-Set the sprite gameobj to inactive (if it isn't the 1st npc you want to appear. First NPC should always be
set to active, by default it's hoss lerman)

To edit order of NPCs: 
-In the DialogueManager gameobj edit order of customers in the inspector,
if for fast testing you need an npc to be the first one to come, set their sprite to active and 
Hoss(or 1st customer) to inactive, and make sure your selected NPC is first in the DialogueManager order.

/\ DialogueManager inspector Tick Boxes and what they do /\

If a dialogue line has responses, under a response line you have:
-Response txt (obvious)
-Next line index: the number you input is the number of the element (dialogue line) where you will be taken if
you click that response
-Return after response: Redundant, (was used in early stages of the game, feel free to delete)
-Activate continue after choice: Redundant
-Is make sale button: Clicking the response will check if player has placed items found in NPCRequest
in the checkout. Completes sale if yes, gives error sound and NPC repeats request if no.

Rest of tick boxes (found under any normal dialogue line):
-Ask for ID: Spawns ID of customer after line
-Show go back button: redundant
-Go back target: redundant
-Grant Name/DOB/Address/Issuer/Picture gives specified info on the scanner after dialogue line
-Disable continue button: For that line, disables the "continue" button that automatically 
spawns after any dialogue line (continue button when pressed takes you to the next element/
line of dialogue in the inspector)
-End conversation here: immediatly ends conversation and displays a score screen
-Score screen index: choose which score screen to display (end conversation here must
be ticked for this to do something, score screens are also added in the inspector of
DialogueManager at the bottom)

/\ TO EDIT/ADD/REMOVE ITEMS/WARES /\

-Go into the ItemScript to edit or add item(s) & their values
-On any sprite, edit tag & layer to "pickupitem" & add ItemScript & ItemHover 
scripts, then choose the item you want that sprite to be from the ItemScript inspector
-Leave slot index in ItemScript inspector as is.


<> Congrats! You now should know what the core scripts do and how to use them. There's a bunch of smaller scripts in the game as well that all do smaller tasks, but those are simple enough. Good Luck! <>