# Beta test checklist: Seels (The Reckless) & Fry (The Commander)

Branch: `feature/seelmiles-fry`. Everything compiles (`dotnet build`, 0 errors, and every localization key is present). **Nothing has been tested in game yet**, so this list is the first real playtest.

How to use it: tick a box once the item behaves as described. If it doesn't, write a short note under it. Items marked **⚠️ todo** are values I picked myself because the spec didn't give them (they're flagged with `//todo : ...` in the code too).

---

## 0. Setup & smoke test

- [ ] Re-export the `.pck` with Godot (new files: `localization/eng/enchantments.json` and `localization/eng/card_selection.json`), then build and launch.
- [ ] **Seels The Reckless** and **Fry The Commander** appear on character select (placeholder visuals: Silent for Seels, Regent for Fry).
- [ ] Both characters are male (he/him) in the texts.
- [ ] Seels: 75 HP, 3 energy, light blue (#add8e6) name, card backs and map drawing.
- [ ] Fry: 80 HP, 3 energy, red (#cc0000) name, card backs and map drawing.
- [ ] Card back colors look right (HSV in `SeelsTheRecklessCardPool.cs` / `FryTheCommanderCardPool.cs`; the shader may need tweaking like it did for Slal).
- [ ] Every new card, power, relic and potion shows the fallback art (`card.png` / `power.png` / `relic.png`). Real art can go in with the usual file names (e.g. `card_portraits/seels_blood_cage.png`, `powers/blood.png`, `relics/thorn_bracelet.png`).
- [ ] The Architect fight starts without missing-dialogue errors (the lines are placeholder `TODO` text).
- [ ] Fry's character-select description reads "todo: Write Character-select description".
- [ ] Hover tips work for the new keywords: Wounding, Erratic, Blood Bend X, Gamble X, Cooldown X, Request X, Refuse X, The Valkyrs.
- [ ] Card rewards and shops only offer each character's own non-Token cards. Multiplayer-only cards don't appear in single player.
- [ ] Ancient cards (Blood Prison, Gambling Addiction, Apocalypse Stroll, A Wizard Did It) are offered the same way as Shion's and Slal's.
- [ ] Neow's "upgrade starter relic" option (Touch of Orobas) turns Thorn Bracelet into Thorn Bracers and Fry's starter relic (TODO) into its upgraded version.

---

## 1. Seels The Reckless

Starting deck: 4 Strike, 4 Defend, 1 Blood Cage. Starter relic: Thorn Bracelet.

### 1.1 Core mechanics

**Blood**
- [ ] Losing HP from any source (enemy hit, self damage, Wounds…) with Thorn Bracelet gives exactly 1 Blood per HP-loss event, whatever the amount lost.
- [ ] Fully blocked hits don't give Blood.
- [ ] Blood stays between turns.

**Blood Bend X**
- [ ] Consumes ALL Blood. X = the Blood consumed (0 if you had none).
- [ ] The "(X+1)" upgrades add 1 on top of the consumed Blood.
- [ ] Thorn Bracers gives +1 Blood after **every** Blood Bend card, even with X = 0.
- [ ] Splash Armor and Recall Blood do nothing when X = 0.
- [ ] Cerberus' Tip is used up by the **first** Blood Bend of the combat, even if X = 0 (literal reading of the spec).

**Gamble X**
- [ ] You choose min(X, cards in hand) cards. Statuses and Curses can be chosen.
- [ ] The chosen cards gain **Erratic** (for this combat only, not in the deck) and go on **top** of the Draw Pile.
- [ ] Cliff Dropper draws after a Gamble.
- [ ] Toss A Coin (multiplayer): the *other* player's cards are picked **at random** (no remote choice screen).

**Erratic**
- [ ] When drawn, the cost becomes a random value from 0 to 3. Big Numbers raises the max by 1 per stack.
- [ ] Erratic removes Unplayable: a Gambled Wound becomes playable and costs its random value.
- [ ] X-cost cards with Erratic keep their X cost.
- [ ] The randomized cost stays after the card leaves the hand, until the next draw re-rolls it.
- [ ] Spider Tea, Gambling Addiction and Excited Wait see the *randomized* cost.

**Wounding / Wounds**
- [ ] "Wounding" adds the Wound to your hand **before** the card's effect (so Pain Chock and Reckless Charge count it).
- [ ] Wounds that "lose Unplayable" (Jolting Pain, Burning Pain, Eye Opening Realization) can be played for 0 and trigger their enchantment (Adroit = Block, Sown = energy, Swift = draw).
- [ ] Enchanting a Wound that already has a *different* enchantment does nothing to that Wound.

**Slash (token)**
- [ ] 0 cost, 3 (5) damage to a random enemy, draw 1, Exhaust.
- [ ] With Untelegraph Attacks: hits = energy cost + 1 (a 0-cost Slash hits once, an Erratic 3-cost Slash hits 4 times).

**Glass X (new enchantment)**
- [ ] +X damage on the card's hits, and applies 1 Vulnerable once per play (to the target, or a random enemy for random-target Slashes).
- [ ] Shows in the enchantment slot with its tooltip.

**Magical Overload tracking**
- [ ] Each card gets counted once when it gains Erratic or an enchantment from Seels' effects (Gamble, Hold Breath, Lottery Ticket, Jolting/Burning Pain, Eye Opening Realization, Tornado Stance, Press the Attack…).

### 1.2 Cards

Values are base (upgraded). "X" is the Blood Bend / energy X.

#### Starter

- [ ] **Blood Cage** — Attack, 0 energy  
  Blood Bend X. Deal 4 damage. Apply X (upgraded: X+1) Vulnerable.
- [ ] **Strike** — Attack, 1 energy  
  Deal 6 (9) damage.
- [ ] **Defend** — Skill, 1 energy  
  Gain 5 (8) Block.

#### Ancient

- [ ] **Blood Prison** — Attack, 0 energy  
  Blood Bend X. Deal 6 damage to ALL enemies. Apply X (upgraded: X+1) Vulnerable to ALL enemies (upgraded: twice).
- [ ] **Gambling Addiction** — Skill, 2 (1) energy — Exhaust  
  Gamble 1. The next time you draw an Erratic card, gain Intangible equal to its randomized energy cost.

#### Common

- [ ] **An Opening** — Attack, 0 energy  
  Deal 3 (4) damage. If the enemy does not intend to attack, apply 1 (2) Vulnerable.
- [ ] **Belittle** — Attack, 2 energy  
  Deal 7 damage to a random enemy 3 (4) times. Gain 1 Vulnerable each time an enemy loses HP this way.
- [ ] **Bloody Execution** — Attack, 2 energy  
  Blood Bend X. Deal X damage. Deal 2 (3) additional damage for ALL your cards containing "Blood".
- [ ] **Card Dealing** — Attack, 1 energy  
  Draw a card for each enemy. Gamble that many cards. Deal 3 (6) damage to ALL enemies.
- [ ] **Drop Kick** — Attack, 1 energy — Wounding  
  Deal 5 damage. Draw 2 (3) cards.
- [ ] **Exsanguination** — Attack, 2 energy  
  Blood Bend X. Deal X damage. Triple X if it is equal to (upgraded: or more than)the number of other cards in your Hand.
- [ ] **Pain Chock** — Attack, 1 energy — Wounding  
  Deal damage to ALL enemies equal to the number of Wounds in your Hand.
- [ ] **Potentially Massive Blow** — Attack, 1 energy  
  Deal 10 (12) damage times the energy cost of this card. Double this damage if its cost is 2 or more.
- [ ] **Private Joke** — Attack, 0 energy  
  Draw 1 (2) cards. If you draw an Attack, a random enemy takes 3 damage. (upgraded: A random enemy takes 3 damage for each Attack drawn this way.)
- [ ] **Reckless Charge** — Attack, 1 energy — Wounding  
  Deal 5 (7) damage for each Wound in your Hand.
- [ ] **Violent Rush** — Attack, 2 energy  
  Deal 7 damage to ALL enemies. Add a Slash (upgraded: Slash+) into your Hand, Draw Pile and Discard Pile.
- [ ] **Blood Bath** — Skill, 2 energy  
  Blood Bend X. Gain 5 Block X (upgraded: X+1) times.
- [ ] **Blood Drop** — Skill, 0 energy — Wounding  
  Lose 2 HP. Gain 1 (2) energy.
- [ ] **Bypass** — Skill, 2 energy  
  Gain 11 Block. Add a Slash (upgraded: Slash+) into your Hand and Draw Pile.
- [ ] **Funny Dance** — Skill, X energy  
  Gain X Vulnerable. Gain 6 (8) Block X times. Next turn, gain 6 (8) Block X times.
- [ ] **Hematoma** — Skill, 1 energy — Wounding  
  Gain 7 (9) Block. Discard your Wounds and draw as many cards.
- [ ] **Improvised Acupuncture** — Skill, 0 energy  
  Lose 1 HP. Draw as many cards (upgraded: +1). Increase this card's HP loss by 1 this combat.
- [ ] **Jolting Pain** — Skill, 0 energy — Wounding  
  ALL Wounds in your Hand lose Unplayable and are enchanted with Adroit 4 (5).
- [ ] **Parry Attempt** — Skill, 1 energy  
  Gain 8 (10) Block times the energy cost of this card. Double this Block if its cost is 2 or more.
- [ ] **Wink** — Skill, 0 energy  
  Gain 4 (6) Block. Gamble 1.

#### Uncommon

- [ ] **Blood Punch** — Attack, 1 energy  
  Lose 1 HP. Deal 7 damage. This combat, increase this card's damage by 3 and HP loss by 1. Shuffle this card into your Draw Pile. (upgraded: Put this card on top of your Draw Pile.)
- [ ] **Body Strike** — Attack, 1 (0) energy  
  Deal 8 damage. Add a Wound into your Discard Pile for each HP the enemy lost.
- [ ] **High Reward** — Attack, 1 energy  
  Deal 10 (12) damage. If you are Vulnerable, hits 3 times.
- [ ] **Magical Overload** — Attack, 1 energy  
  Deal 5 (7) damage to a random enemy for each card you modified with a Keyword or an Enchantment this combat.
- [ ] **Palm Strike** — Attack, 1 (0) energy  
  Deal 10 damage. Gain Vulnerable equal to the damage Blocked.
- [ ] **Scratch** — Attack, 0 energy  
  Deal 3 (4) damage. Increase the damage of ALL Scratch cards by 1 (2) this combat. Add a copy of this card into your Draw Pile.
- [ ] **Steal Blood** — Attack, 1 energy  
  Lose 3 (2) HP. Deal 9 (11) damage. Whenever this enemy loses HP this turn, gain 1 Blood.
- [ ] **Strike First** — Attack, X energy — ⚠️ todo: upgrade not specified  
  Deal 1 damage, plus additional damage equal to the enemy's Vulnerable, X (upgraded: X+1) times. Double if you are Vulnerable. Double if the enemy intends to attack.
- [ ] **Aggravating Demeanor** — Power, 1 energy  
  Whenever you gain Vulnerable, gain Block next turn equal to your Vulnerable, up to 5 (10). Gain 2 (4) Vulnerable.
- [ ] **Bleed Instructions** — Power, 1 energy  
  If you lose HP at least once during your turn, draw 1 (2) additional cards at the start of your next turn.
- [ ] **Combo Meter** — Power, 2 (1) energy  
  Add a copy of the first Slash you play each turn into your Hand.
- [ ] **Dice Blessing** — Power, 2 (1) energy  
  At the start of your turn, draw 1 additional card and Gamble 1.
- [ ] **Invigorating Pain** — Power, 1 energy  
  Whenever you play a Wound, gain 3 (5) Vigor.
- [ ] **Joke and Run** — Power, 1 energy  
  Gain 3 Vulnerable. Gain 1 (2) Dexterity. Heal HP equal to your Vulnerable.
- [ ] **Laugh It Off** — Power, 1 energy  
  Whenever you gain Vulnerable, draw 1 (2) cards.
- [ ] **Savagery** — Power, 1 energy  
  You deal 10% more damage to Vulnerable enemies for each of your first 5 (8) Vulnerable.
- [ ] **Weird Flex** — Power, 1 energy  
  Whenever you draw a Wound, gain 2 (3) Strength until the end of your turn.
- [ ] **Big Target Sign** — Skill, 0 energy — +Retain when upgraded — multiplayer only  
  Gain Vulnerable equal to another player's. They lose their Vulnerable.
- [ ] **Blind Assault** — Skill, 1 energy  
  Transform 2 cards in your Hand into Corrupted Slashes (upgraded: Slashes+).
- [ ] **Concede First Blow** — Skill, 0 energy  
  Gain 2 Vulnerable. Next turn, gain 2 (3) energy and draw 2 additional cards.
- [ ] **Cool Bandages** — Skill, 1 energy  
  The next 3 Wounds are free to play and make ALL enemies lose 4 (6) HP when played.
- [ ] **Copy Kitty** — Skill, 1 (0) energy — multiplayer only  
  Play a copy of a random card another player played this turn.
- [ ] **Excited Wait** — Skill, 1 (0) energy  
  Gamble 1. The next time you draw an Erratic card, gain energy equal to its randomized energy cost.
- [ ] **Eye Opening Realization** — Skill, 1 energy — Wounding  
  ALL Wounds in your Hand lose Unplayable and are enchanted with Swift 2 (3).
- [ ] **Hold Breath** — Skill, 0 energy — Exhaust  
  Add Erratic to ALL cards in your Hand. Shuffle ALL your cards into your Draw Pile. Draw 3 (5) cards.
- [ ] **Humiliate** — Skill, 1 energy — Exhaust  
  Apply 2 (3) Vulnerable. This turn, the enemy loses Strength equal to their Vulnerable.
- [ ] **Illusion of Risk** — Skill, 1 energy  
  Double your Vulnerable. For the next 2 (3) turns, if you are Vulnerable, you take 75% reduced damage from Vulnerable enemies.
- [ ] **Open Wounds** — Skill, 0 energy  
  Draw 2 (3) cards. The next 2 times you lose HP, add a Wound into your Hand.
- [ ] **Pin Them Down** — Skill, 1 energy  
  Blood Bend X. Add X (upgraded: X+1) Inky Slashes into your Draw Pile.
- [ ] **Play on Words** — Skill, 0 energy  
  Gain 1 (2) Vulnerable. Apply 1 (2) Vulnerable to ALL enemies.
- [ ] **Press the Attack** — Skill, 1 energy  
  Add a Momentum 5 Slash (upgraded: Slash+) into your Hand. It loses Exhaust and gains Erratic.
- [ ] **Questionable Strategy** — Skill, 0 energy  
  Gain 5 Vulnerable. Gain Block equal to your Vulnerable (upgraded: twice).
- [ ] **Running Joke** — Skill, 1 (0) energy  
  Draw cards until you draw a non-Attack card. Apply Vulnerable equal to the number of cards drawn this way.
- [ ] **Smug Look** — Skill, 0 energy  
  Gain 2 Vulnerable. This turn (upgraded: for 2 turns), whenever you play a card, deal 2 damage to a random enemy.
- [ ] **Spectacular Break-in** — Skill, 1 energy  
  Blood Bend X. Add X (upgraded: X+1) Glass 2 Slashes into your Draw Pile.
- [ ] **Summon Wrath** — Skill, X energy  
  Lose X (upgraded: 1) HP. Put X random Slashes from your Draw or Discard Piles into your Hand and Upgrade them.
- [ ] **Toss A Coin** — Skill, 0 energy — multiplayer only  
  Another player Gambles 1 (2) random cards.
- [ ] **U-Turn** — Skill, 2 energy  
  Gain 6 (8) Block. Draw 4 (5) cards. Discard ALL cards drawn this way that do not cost 0.

#### Rare

- [ ] **Ace of Spade** — Attack, 1 energy — Retain, +Innate when upgraded  
  While this card is in your Hand, it grants you its energy cost as Strength and Dexterity. Deal 1 damage.
- [ ] **Finishing Explosion** — Attack, 2 energy  
  Deal 3 (4) damage to a random enemy for each Slash in your Exhaust Pile.
- [ ] **Payback Time** — Attack, 0 energy — TeamBlast — multiplayer only  
  Blood Bend X. Deal damage equal to the missing HP of ALL other players X times (upgraded: ALL players, including you).
- [ ] **Ultra Kill** — Attack, 6 energy  
  Lose 6 HP minus the energy cost of this card. Deal 30 (35) damage 2 times.
- [ ] **Beast Form** — Power, 3 energy  
  At the start of your turn, if you have more Vulnerable than Strength, gain 2 (3) Strength.
- [ ] **Big Numbers** — Power, 3 (2) energy  
  Gamble 2. Increase the maximum energy cost an Erratic card can have by 1.
- [ ] **Brutal Teachings** — Power, 2 (1) energy  
  Whenever you lose HP, gain 2 Dexterity until the end of your turn.
- [ ] **Cliff Dropper** — Power, 1 energy — +Innate when upgraded  
  Whenever you Gamble, draw 1 card.
- [ ] **Dark Powers** — Power, 2 (1) energy  
  The first time you play a Slash each turn, gain 1 energy and apply 1 Vulnerable to a random enemy.
- [ ] **Gambler's Luck** — Power, 1 energy  
  Whenever you draw a card during your turn, gain its energy cost as Block. Draw 1 (2) cards.
- [ ] **Live With The Pain** — Power, 1 energy  
  At the start of your turn, lose 2 HP and gain Block equal to the number of your non-Exhausted Wounds (upgraded: Wounds, including the Exhausted ones).
- [ ] **Recall Blood** — Power, 3 (2) energy  
  Whenever you Blood Bend X, ALL enemies lose X HP, and you gain X Vigor next turn.
- [ ] **Show Off** — Power, 2 energy — Wounding  
  Whenever a Wound is put into your Discard Pile, gain 2 (3) Block.
- [ ] **Splash Armor** — Power, 2 energy  
  Whenever you Blood Bend X, ALL players gain X Block, and Block is not removed at the start of their next turn (upgraded: next 2 turns).
- [ ] **Sweat and Tears** — Power, 2 energy  
  Every 4 energy you spend, gain 1 (2) Blood.
- [ ] **Untelegraph Attacks** — Power, 2 energy  
  Add 2 Slashes (upgraded: Slashes+) into your Hand. Gamble 2. ALL Slashes now hit a number of times equal to their energy cost +1.
- [ ] **Blood Mixing** — Skill, 1 energy — Exhaust, +Innate when upgraded  
  Lose 1 HP for each enemy. ALL enemies lose 1 HP and 1 Strength.
- [ ] **Burning Pain** — Skill, 1 (0) energy — Wounding  
  ALL Wounds in your Hand lose Unplayable and are enchanted with Sown.
- [ ] **Do This All Day** — Skill, 2 energy  
  Put ALL Slashes from your Exhaust Pile into your Discard Pile (upgraded: Draw Pile).
- [ ] **Echoes of Pain** — Skill, 2 (1) energy — Exhaust — ⚠️ todo: upgrade not specified  
  Shuffle a copy of ALL your enchanted Wounds into your Draw Pile. Draw cards until your Hand is full.
- [ ] **Go Crazy** — Skill, 2 energy  
  Fill your Hand with Slashes (upgraded: Slashes+).
- [ ] **Lure** — Skill, 1 energy — Exhaust, +Retain when upgraded  
  Gain 1 Intangible. Gain 5 Vulnerable.
- [ ] **Massive Defend** — Skill, 4 energy — Exhaust, +Retain when upgraded  
  Gain 40 Block.
- [ ] **Never Learn** — Skill, 1 energy — Exhaust, +Retain when upgraded  
  If you have more Vulnerable than HP, gain 4 Max HP.
- [ ] **Scarlet Shadow** — Skill, 0 energy — Exhaust  
  Blood Bend X. Gain X Blood 2 (3) times.
- [ ] **Shed The Pain** — Skill, 3 (2) energy — Exhaust  
  Exhaust ALL your enchanted Wounds. Heal 1 HP per card Exhausted this way.
- [ ] **Threatening Promise** — Skill, 1 energy  
  Apply 2 (3) Vulnerable. Add a Perfect Fit Slash (upgraded: Slash+) into your Discard Pile for each Vulnerable on the enemy. Repeat if you are Vulnerable.
- [ ] **Tornado Stance** — Skill, 1 energy  
  Add Replay to ALL your Slashes (upgraded: and Upgrade them). Gain 1 Vulnerable for each of your Slashes. Doesn't include the Exhausted ones.

#### Token

- [ ] **Slash** — Attack, 0 energy — Exhaust  
  Deal 3 (5) damage to a random enemy. Draw 1 cards.

### 1.3 Powers (in-combat status icons)

- [ ] **Blood** — You have X Blood, consumed by Blood Bend cards.
- [ ] **Vigor Next Turn** — At the start of your next turn, gain X Vigor.
- [ ] **Smug Look** — For the next X turns, whenever you play a card, deal 2 damage to a random enemy.
- [ ] **Splash Armor** — Whenever you Blood Bend X, ALL players gain X Block and X Blur.
- [ ] **Gambling Addiction** — The next X times you draw an Erratic card, gain Intangible equal to its randomized energy cost.
- [ ] **Excited Wait** — The next X times you draw an Erratic card, gain energy equal to its randomized energy cost.
- [ ] **Recall Blood** — Whenever you Blood Bend X, ALL enemies lose X HP, and you gain X Vigor next turn.
- [ ] **Show Off** — Whenever a Wound is put into your Discard Pile, gain X Block.
- [ ] **Beast Form** — At the start of your turn, if you have more Vulnerable than Strength, gain X Strength.
- [ ] **Brutal Teachings** — Whenever you lose HP, gain X Dexterity until the end of your turn.
- [ ] **Brutal Teachings** — Gains X Dexterity until the end of the turn.
- [ ] **Sweat and Tears** — Every 4 energy you spend, gain X Blood.
- [ ] **Live With The Pain** — At the start of your turn, X times: lose 2 HP and gain Block equal to the number of your non-Exhausted Wounds.
- [ ] **Live With The Pain+** — At the start of your turn, X times: lose 2 HP and gain Block equal to the number of your Wounds, including the Exhausted ones.
- [ ] **Cliff Dropper** — Whenever you Gamble, draw X cards.
- [ ] **Gambler's Luck** — Whenever you draw a card during your turn, gain its energy cost as Block.
- [ ] **Untelegraph Attacks** — ALL Slashes hit a number of times equal to their energy cost +X.
- [ ] **Big Numbers** — Erratic cards can cost up to X more energy.
- [ ] **Dark Powers** — The first X times you play a Slash each turn, gain 1 energy and apply 1 Vulnerable to a random enemy.
- [ ] **Invigorating Pain** — Whenever you play a Wound, gain X Vigor.
- [ ] **Aggravating Demeanor** — Whenever you gain Vulnerable, gain Block next turn equal to your Vulnerable, up to X.
- [ ] **Dice Blessing** — At the start of your turn, draw X additional cards and Gamble X.
- [ ] **Savagery** — You deal 10% more damage to Vulnerable enemies for each of your first X Vulnerable.
- [ ] **Combo Meter** — Add a copy of the first X Slashes you play each turn into your Hand.
- [ ] **Weird Flex** — Whenever you draw a Wound, gain X Strength until the end of your turn.
- [ ] **Weird Flex** — Gains X Strength until the end of the turn.
- [ ] **Laugh It Off** — Whenever you gain Vulnerable, draw X cards.
- [ ] **Bleed Instructions** — If you lose HP during your turn, draw X additional cards at the start of your next turn.
- [ ] **Illusion of Risk** — For the next X turns, if you are Vulnerable, you take 75% reduced damage from Vulnerable enemies.
- [ ] **Cool Bandages** — Your next X Wounds are free to play and make ALL enemies lose HpLoss HP when played.
- [ ] **Open Wounds** — The next X times you lose HP, add a Wound into your Hand.
- [ ] **Steal Blood** — Whenever this enemy loses HP this turn, the attacker gains X Blood.
- [ ] **Humiliate** — Loses X Strength until the end of the turn.

### 1.4 Relics

- [ ] **Thorn Bracelet** — Whenever you lose HP, gain 1 Blood.
- [ ] **Thorn Bracers** — Whenever you lose HP, gain 2 Blood. Whenever you play a Blood Bend card, gain 1 Blood after you consumed your Blood.
- [ ] **Red Fang** — Every 10 Blood you gain, heal 1 HP.
- [ ] **Megaphone** — While Vulnerable, you have 2 additional Dexterity.
- [ ] **Lottery Ticket** — At the start of each combat, choose up to 3 cards in your Draw Pile, add Erratic to them and randomize their energy cost. Gain 1 Vulnerable for each card modified.
- [ ] **Cerberus' Tip** — The first time you Blood Bend X each combat, heal X HP.
- [ ] **Driving License** — Whenever you create a card, shuffle a copy of that card into your Discard Pile.
- [ ] **Spider Tea** — The first time each turn you draw a card that costs 3 energy or more, gain 1 energy.
- [ ] **Crystal Garland** — At the start of each Elite or Boss fight, procure a Blood Potion.

- [ ] Red Fang's counter carries over between combats and survives save & quit (it's a `[SavedProperty]`).
- [ ] Lottery Ticket's choice screen opens before your first hand is drawn.

### 1.5 Potions

- [ ] **Fresh Lemonade** — Gamble 3. Draw 3 cards. Heal HP equal to the energy cost of the cards drawn this way.
- [ ] **Empty Bottle** — Add 2 upgraded Glass 2 Slashes into your Hand.
- [ ] **Elixir of Ferality** — Put ALL your Exhausted Slashes on top of your Draw Pile.

- [ ] Fresh Lemonade never shows up from random potion generation in combat (like Fruit Juice).

---

## 2. Fry The Commander

Starting deck: 4 Strike, 4 Defend, Refuge in Audacity, Ruin Walk. Starter relic: TODO (`FryRelicTodo1`). The Star counter is always visible, like the Regent's.

### 2.1 Core mechanics

**Stars**
- [ ] The starter relic gives Stars equal to your unspent energy when you end your turn. The upgraded relic gives +1, and 3 Stars at the start of each combat.
- [ ] Cards with a Star cost show it and spend Stars like the Regent's cards. Artistic License – Physics is X-Star.

**Bad Faith**
- [ ] Each round, the first X HP you would lose are absorbed. You gain the same amount of **Damage Debt** instead. The icon shows the remaining capacity.
- [ ] Capacity resets at the start of your turn.
- [ ] Works on any HP loss (enemy attacks, self damage such as Ramming Always Works or Implausible Deniability).

**Damage Debt**
- [ ] At the **end of your turn**, you lose HP equal to the debt, then it's removed.
- [ ] Paying the debt is **not** absorbed by Bad Faith again (no infinite loop).
- [ ] The orange forecast segment on the health bar shows the upcoming loss. It's hidden while Refuse is active.

**Refuse X**
- [ ] While Refuse is active, the end-of-turn Damage Debt payment is skipped and the debt keeps growing.
- [ ] Refuse ticks down by 1 at the end of each of your turns (so "Refuse 1" played this turn skips only this turn's payment).

**Cooldown X** (all Cooldown cards also have Retain)
- [ ] Played normally: the effect happens, you pay energy/Stars, and the card **returns to your Hand**.
- [ ] During the next X turns it costs 0 energy and 0 Stars. Playing it then sends it to the Discard Pile with **no effect**, and clears the cooldown.
- [ ] The cooldown goes down by 1 at the start of each of your turns. The card text shows "On cooldown for N more turns".
- [ ] TODO (`FryTodo10`) can discard Cooldown cards.

**Request X / Requested**
- [ ] Playing a Request card creates its own **Requested** counter (counters never merge). The hover shows the source card.
- [ ] The counter goes down at the start of each of your turns, and the effect triggers when it reaches 0 (Request 1 → next turn).
- [ ] "Decrease ALL Requested counters" (Just in Time, Call on Me, Scotty Time, TODO 14) triggers counters that reach 0 immediately.
- [ ] Removing the Earpiece and Discarded, Not Delivered remove counters **without** triggering them.
- [ ] Summon Bigger Fish lowers the Request of the first N Request cards each combat by 1 (Request 1 → immediate).
- [ ] TODO 15 (Dex per counter), TODO 17 (energy when a counter reaches 0) and Improvised Armour all interact correctly.

**The Valkyrs**
- [ ] Valkyr damage ignores **your** modifiers (Strength, Vigor, Bloody Handprint, Strike bonuses…) but still uses the **target's** (Vulnerable…). There's no summoned creature.
- [ ] Valkyr Block ignores Dexterity / Frail.
- [ ] TODO (`FryTodo19`)'s mark doubles Valkyr damage on that enemy for 1 turn.

### 2.2 Cards

Values are base (upgraded). "TODO" cards are the spec's unnamed rows (`FryTodoN` in code).

#### Starter

- [ ] **Ruin Walk** — Attack, 0 energy + 2 Stars — ⚠️ todo: upgrade not specified  
  Cooldown 2. Deal 12 (16) damage to ALL enemies.
- [ ] **Strike** — Attack, 1 energy  
  Deal 6 (9) damage.
- [ ] **Defend** — Skill, 1 energy  
  Gain 5 (8) Block.
- [ ] **Refuge in Audacity** — Skill, 1 energy — ⚠️ todo: upgrade not specified  
  Gain 6 (9) Bad Faith. Lose half of your Damage Debt.

#### Ancient

- [ ] **Apocalypse Stroll** — Attack, 0 energy + 2 Stars — ⚠️ todo: upgrade not specified  
  Cooldown 1. Deal 20 (26) damage. Deal 12 (16) damage to ALL enemies.
- [ ] **A Wizard Did It** — Power, 3 energy  
  Lose 3 (2) Max HP. At the start of each turn, gain 1 fewer 1 energy and play the top card of your Draw Pile.

#### Common

- [ ] **Desperation Attack** — Attack, 1 (0) energy — ⚠️ todo: choose cost; upgrade not specified  
  Deal damage equal to your Damage Debt. Triple if your Damage Debt is larger than or equal to your HP.
- [ ] **Fireballs** — Attack, 0 energy + 1 Stars  
  Cooldown 1. Deal 14 (17) damage. Deal 3 (6) damage to ALL other enemies.
- [ ] **Here We Go Again!** — Attack, 1 energy  
  Deal 8 (10) damage. Next turn, gain 1 (2) energy. Put this card on top of your Draw Pile.
- [ ] **Just in Time** — Attack, 0 energy — ⚠️ todo: choose cost; upgrade not specified  
  Deal 3 (6) damage. Decrease ALL your Requested counters by 1.
- [ ] **Ramming Always Works** — Attack, 1 energy — Exhaust, −Exhaust when upgraded  
  Remove the enemy's Block. Deal 2 damage. Apply 3 Vulnerable. Lose 2 HP.
- [ ] **Razor-Sharp Hand** — Attack, 1 energy — ⚠️ todo: choose cost; upgrade not specified  
  Deal 9 (12) damage. Lose Damage Debt equal to the unblocked damage dealt.
- [ ] **TODO (`FryTodo19`)** — Attack, 0 energy — Valkyrs — ⚠️ todo: choose cost; upgrade not specified  
  Deal 3 (6) damage. If this deals unblocked damage, The Valkyrs deal double damage to this enemy this turn.
- [ ] **TODO (`FryTodo21`)** — Attack, 2 energy  
  Deal 12 (14) damage. Apply 2 (3) Vulnerable.
- [ ] **TODO (`FryTodo22`)** — Attack, 2 energy  
  Deal 14 (16) damage. Apply 2 (3) Weak.
- [ ] **TODO (`FryTodo23`)** — Attack, 1 energy — ⚠️ todo: type not specified  
  Deal 8 (10) damage. Gain 1 (2) Dexterity this turn.
- [ ] **Blunt "No"** — Skill, 1 (0) energy  
  Refuse 1.
- [ ] **Boots of Toughness** — Skill, 1 energy  
  Gain 10 Block. Gain 3 (5) Bad Faith. Lose 2 (4) Damage Debt.
- [ ] **Call on Me** — Skill, 0 energy — ⚠️ todo: choose cost; upgrade not specified  
  Gain 4 (7) Block. If the enemy intends to attack, decrease ALL your Requested counters by 1.
- [ ] **Crosshair Aware** — Skill, 0 energy + 1 Stars  
  If the enemy intends to attack, gain 10 (13) Block.
- [ ] **Fake Texting** — Skill, 0 energy — Exhaust — ⚠️ todo: choose cost  
  Apply 2 Weak. (upgraded: Request 5. Add nothing into your Hand.)
- [ ] **Gratuitous English** — Skill, 1 energy — Request  
  Request 2 (1). Choose 2 cards in your Draw Pile to put into your Hand.
- [ ] **Implausible Deniability** — Skill, 1 (0) energy — ⚠️ todo: choose cost; upgrade not specified  
  Lose half of your Damage Debt. Take damage equal to your Damage Debt.
- [ ] **TODO (`FryTodo10`)** — Skill, 0 energy — ⚠️ todo: upgrade not specified  
  Gain 4 (6) Block. Discard any number of cards with Cooldown. Draw that many cards.
- [ ] **TODO (`FryTodo12`)** — Skill, 1 energy — Request, Valkyrs — ⚠️ todo: choose cost; upgrade not specified  
  Request 1. The Valkyrs deal 12 (16) damage to ALL enemies.
- [ ] **TODO (`FryTodo13`)** — Skill, 1 energy — Request, Valkyrs — ⚠️ todo: choose cost  
  Request 2. The Valkyrs grant 10 (12) Block and 10 (12) Block next turn.
- [ ] **TODO (`FryTodo24`)** — Skill, 1 energy — ⚠️ todo: type not specified; upgrade not specified  
  Gain 6 (9) Block. Next turn, gain 1 energy.
- [ ] **TODO (`FryTodo25`)** — Skill, 1 energy — ⚠️ todo: type not specified; upgrade not specified  
  Gain 6 (9) Block. Gain 4 Bad Faith.
- [ ] **TODO (`FryTodo26`)** — Skill, 1 energy — ⚠️ todo: type not specified; upgrade not specified  
  Gain 8 (11) Block. Discard 1 card.
- [ ] **TODO (`FryTodo6`)** — Skill, 1 (0) energy — Retain — ⚠️ todo: choose cost; type and rarity not specified; upgrade not specified  
  Remove the enemy's Block. Lose Damage Debt equal to the amount of Block removed.
- [ ] **Talking Is a Free Action** — Skill, 0 energy + 1 Stars  
  Choose 1 (2) cards in your Draw Pile to put on top of your Draw Pile.

#### Uncommon

- [ ] **Claw Strike** — Attack, 1 energy  
  Deal 8 (10) damage. Increase the damage of ALL Claw cards by 2 (3) this combat.
- [ ] **Getting the Boot** — Attack, 1 energy — Exhaust  
  Deal 8 (11) damage. The enemy loses 5 (7) Strength this turn.
- [ ] **Meteor-Summoning Attack** — Attack, 0 energy + 3 Stars  
  Cooldown 3. Deal 32 (42) damage.
- [ ] **No Kill like Overkill** — Attack, 0 energy + 4 Stars  
  Deal 42 (52) damage. The enemy loses 2 (3) Strength and 2 (3) Dexterity.
- [ ] **Nuke** — Attack, 3 energy — ⚠️ todo: upgrade not specified  
  Deal 30 (38) damage. Deal 1 damage to ALL other enemies.
- [ ] **Blatant Lies** — Power, 1 energy  
  At the start of your turn, gain 2 (3) Bad Faith.
- [ ] **Bloody Handprint** — Power, 1 energy  
  You deal 5 (10)% more damage per Damage Debt you have.
- [ ] **Improvised Armour** — Power, 1 (0) energy  
  The first Request card you play each turn, gain 10 Block.
- [ ] **Meditation Powerup** — Power, 1 energy — ⚠️ todo: choose cost  
  Whenever you Retain a card, gain 1 (2) Vigor.
- [ ] **Recycled Animation** — Power, 2 (1) energy  
  The first time you spend Stars each turn, gain 1 energy.
- [ ] **TODO (`FryTodo11`)** — Power, 1 energy — +Innate when upgraded  
  Whenever a card with Retain is put into your Discard Pile, draw 1 card and gain 4 Block.
- [ ] **TODO (`FryTodo15`)** — Power, 1 energy — ⚠️ todo: choose cost  
  Each of your Requested counters gives you 2 (3) Dexterity until it reaches 0.
- [ ] **Actually a Doombot** — Skill, 1 energy — multiplayer only  
  Another player gains Block equal to their HP (upgraded: Max HP).
- [ ] **Attack Pattern Alpha** — Skill, X energy — Request, Valkyrs  
  Request 2. The Valkyrs deal 9 (12) damage to a random enemy X times.
- [ ] **Bottomless Magazines** — Skill, 1 (0) energy — Request — ⚠️ todo: choose cost; upgrade not specified  
  Request 1. Draw cards until your Hand is full.
- [ ] **Calling Your Attacks** — Skill, 1 energy  
  Gain 9 (12) Vigor. Lose 1 Strength.
- [ ] **Chekhov's Gun** — Skill, 1 (0) energy — ⚠️ todo: upgrade not specified  
  Add Retain and Exhaust to a card in your Hand. Next turn, gain 1 energy.
- [ ] **Discarded, Not Delivered** — Skill, 0 energy  
  Remove a random Requested counter. Gain 2 (3) Dexterity.
- [ ] **Door Slam of Rage** — Skill, 1 energy  
  Gain 1 Vigor for each Attack in your Discard Pile. Add The Door Slams You (upgraded: The Door Slams You+) into your Hand.
- [ ] **Jet Pack** — Skill, 0 energy + 1 Stars — Retain  
  Gain 16 (20) Block. Add 3 Burns into your Draw Pile. Transform this card into Jettison Jetpack Attack and return it to your Hand.
- [ ] **Kinda Busy Here** — Skill, 1 energy — ⚠️ todo: choose cost; upgrade not specified  
  Gain 5 (7) Block for each Requested counter you have.
- [ ] **Mana Shield** — Skill, 0 energy + 2 Stars  
  Cooldown 2. Gain 15 (20) Block. Lose 1 Dexterity.
- [ ] **Perpetual-Motion Monster** — Skill, 0 energy + 2 Stars  
  Cooldown 2. Gain 3 (4) energy.
- [ ] **Punch Parry** — Skill, 1 energy  
  Lose 10 (20) Strength this turn. The enemy loses 10 (20) Strength this turn.
- [ ] **Retcon** — Skill, 2 (1) energy + 2 Stars — ⚠️ todo: upgrade not specified  
  Cooldown 3. Remove ALL your Damage Debt.
- [ ] **Scotty Time** — Skill, 1 energy — Request — ⚠️ todo: choose cost; upgrade not specified  
  Gain 10 (13) Block. Request 1. Decrease ALL your Requested counters by 1.
- [ ] **Spikes of Villainy** — Skill, 1 energy  
  Gain 1 Thorns. Gain 1 (2) Strength. Next turn, gain 1 energy.
- [ ] **Spin to Deflect Stuff** — Skill, 0 energy  
  Lose 3 (2) Strength this turn. Discard your Hand. Gain 4 (6) Block for each card discarded. Draw 4 cards.
- [ ] **Sucking-In Lines** — Skill, 1 (0) energy  
  Gain Stars equal to the enemy's Strength.
- [ ] **TODO (`FryTodo14`)** — Skill, 0 energy — +Retain when upgraded — ⚠️ todo: choose cost; upgrade not specified  
  Reduce ALL your Requested counters by 1.
- [ ] **TODO (`FryTodo18`)** — Skill, 0 energy — +Innate when upgraded  
  Exhaust a card. Choose 2 cards in your Draw Pile to put on top of your Draw Pile.
- [ ] **TODO (`FryTodo2`)** — Skill, 0 energy — multiplayer only  
  Another player puts up to 2 (3) random cards from their Hand on top of their Draw Pile.
- [ ] **TODO (`FryTodo20`)** — Skill, 1 (0) energy — Exhaust  
  If the enemy intends to attack, gain Bad Faith equal to its full attack power.
- [ ] **TODO (`FryTodo3`)** — Skill, 0 energy + 1 (0) Stars  
  ALL players gain 1 energy and gain 1 (2) energy next turn.
- [ ] **TODO (`FryTodo5`)** — Skill, 1 energy  
  Exhaust 1 (2) cards in your Hand. Gain Stars equal to their energy cost.
- [ ] **TODO (`FryTodo7`)** — Skill, 1 energy  
  Gain 15 (18) Block. Lose 1 Dexterity.
- [ ] **Tactical Rock–Paper–Scissors** — Skill, 0 energy — Exhaust  
  If the enemy intends to attack, gain 1 (2) Dexterity. If it intends to defend, gain 1 (2) Strength. If it intends neither, gain 1 (2) energy next turn.

#### Rare

- [ ] **Break Meter** — Attack, 0 energy + 5 Stars  
  Deal 52 (62) damage. Apply 3 Vulnerable.
- [ ] **Infinity +1 Sword** — Attack, 0 energy + 9 Stars  
  Can only be played if you have 9 or more Attacks in your Discard Pile. Deal 52 (63) damage 2 times.
- [ ] **Magic Missile Storm** — Attack, 1 energy + 2 Stars — ⚠️ todo: upgrade not specified  
  Cooldown 1. Deal 8 (11) damage. After being played, this card is free to play until it is discarded.
- [ ] **You Nuke 'Em** — Attack, 1 energy + 3 Stars — ⚠️ todo: upgrade not specified  
  Cooldown 1. Deal 20 (26) damage to ALL enemies.
- [ ] **Decoy Trial** — Power, 2 (1) energy — multiplayer only  
  Whenever you gain Bad Faith, Refuse or lose Damage Debt from a card, ALL other players benefit from it too.
- [ ] **Hyper-Destructive Bouncing Ball** — Power, 2 (1) energy — ⚠️ todo: upgrade not specified  
  At the start of your turn, deal 3 damage to a random enemy 1 time and increase the number of hits by 2.
- [ ] **Rule of Cool** — Power, 1 energy  
  Whenever you attack an enemy and it has less HP than (upgraded: twice)the amount of energy and Stars you spent this turn, it dies.
- [ ] **Summon Bigger Fish** — Power, 1 (0) energy  
  The first time a Request card is played each combat, decrease its Request by 1.
- [ ] **TODO (`FryTodo1`)** — Power, 3 (2) energy — multiplayer only  
  Whenever any other player ends their turn, gain Stars equal to their remaining energy, and ALL players gain the same amount of Block next turn.
- [ ] **TODO (`FryTodo17`)** — Power, 2 (1) energy  
  Whenever one of your Requested counters reaches 0, gain 1 energy.
- [ ] **TODO (`FryTodo4`)** — Power, 2 energy  
  Whenever an enemy breaks your Block, gain 1 (2) Strength.
- [ ] **TODO (`FryTodo8`)** — Power, 2 (1) energy  
  If you play 4 or less cards in a turn, gain 1 energy next turn.
- [ ] **Ultimate Form** — Power, 3 energy  
  Refuse 2 (3). Gain 40 Bad Faith. After 2 (3) turns, lose ALL Bad Faith.
- [ ] **Artistic License – Physics** — Skill, 0 energy + X Stars — Exhaust  
  Gain X (upgraded: X+1) 1 energy 2 times.
- [ ] **BFS** — Skill, 0 energy + 3 Stars  
  Cooldown 2. Forge 15 (20). Put Sovereign Blade from ANYWHERE into your Hand.
- [ ] **Bare-Handed Blade Block** — Skill, 2 energy — Exhaust, −Exhaust when upgraded  
  Gain Block equal to your HP. Block is not removed at the start of your next turn.
- [ ] **Blocking Stops All Damage** — Skill, 2 energy — Exhaust  
  If you have Block, you can't lose HP until your next turn. Lose 5 (3) Dexterity.
- [ ] **Cape Swish** — Skill, X energy — Request  
  Request X. Deal 8 (10) damage to ALL enemies and gain 4 (5) Strength this turn. Repeat X times.
- [ ] **Casting A Shadow** — Skill, 1 energy — ⚠️ todo: upgrade not specified  
  Cooldown 3 (2). Channel 1 Dark.
- [ ] **Expendable Clone** — Skill, 0 energy + 2 Stars — Request  
  Request 3 (2). Remove your Damage Debt. Lose ALL Weak, Frail and Vulnerable.
- [ ] **Friendly Ghost** — Skill, 1 energy + 2 Stars  
  Cooldown 2. Summon 14 (18).
- [ ] **Heal Thyself** — Skill, 1 (0) energy + 2 Stars  
  Heal 3 HP for each card in your Hand. Discard your Hand.
- [ ] **Last Chance Hit Point** — Skill, 2 energy — Exhaust, +Retain when upgraded — ⚠️ todo: choose cost  
  Until your next turn, gain Hardened Shell equal to your HP minus 1. Refuse 1.
- [ ] **Mecha Expansion Pack** — Skill, 0 energy — TeamBlast, Request — multiplayer only, ⚠️ todo: Shrink amount not specified  
  Request 2 (1). Apply 3 Shrink to ALL enemies. ALL players gain 7 Strength and 7 Dexterity for 3 turns.
- [ ] **Mid-Season Upgrade** — Skill, 0 energy + 2 Stars — ⚠️ todo: upgrade not specified  
  Cooldown 3 (2). Double your Strength.
- [ ] **Punch Catch** — Skill, 2 energy  
  Gain 20 (30) Block. If the enemy intends to attack, it gains Imbalanced for 1 turn.
- [ ] **Removing the Earpiece** — Skill, 0 energy — Exhaust  
  Gain 2 (3) Strength per Requested counter. Remove ALL your Requested counters.
- [ ] **TODO (`FryTodo16`)** — Skill, 2 energy — Request, Valkyrs — ⚠️ todo: choose cost; upgrade not specified  
  Request 3. ALL enemies lose 5 Strength this turn. The Valkyrs deal 33 (40) damage to ALL enemies.
- [ ] **TODO (`FryTodo9`)** — Skill, 3 (2) energy — Exhaust  
  Refuse 1. Take an extra turn after this one. End your turn.
- [ ] **What Were You Thinking?** — Skill, 1 energy — Request, Valkyrs — ⚠️ todo: choose cost; upgrade not specified  
  Request 5, minus 1 for each 10 Damage Debt you have. The Valkyrs deal 20 (26) damage to ALL enemies. Apply 3 Weak to ALL enemies.

#### Token

- [ ] **Jettison Jetpack Attack** — Attack, 0 energy — Retain, Exhaust — ⚠️ todo: upgrade not specified  
  Deal 15 (20) damage. If you keep this card in your Hand for 2 turns, transform it into Jet Pack.
- [ ] **The Door Slams You** — Attack, 0 energy — Ethereal, Exhaust  
  Deal 6 (10) damage. Deal additional damage equal to your Vigor.
- [ ] **All for Nothing** — Skill, 0 energy — Ethereal, Exhaust  
  Does nothing.

### 2.3 Powers (in-combat status icons)

- [ ] **Bad Faith** — This turn, the next X HP you should lose are not lost. You gain Damage Debt instead.
- [ ] **Damage Debt** — At the end of your turn, lose X HP, then this is removed.
- [ ] **Refuse** — For the next X turns, your Damage Debt does not trigger.
- [ ] **Requested** — In X turns, the requested effect triggers.
- [ ] **Decoy Trial** — Whenever you gain Bad Faith, Refuse or lose Damage Debt from a card, ALL other players benefit from it too.
- [ ] **Summon Bigger Fish** — The next X times a Request card is played this combat, decrease its Request by 1.
- [ ] **A Wizard Did It** — At the start of each turn, gain X fewer energy and play the top X cards of your Draw Pile.
- [ ] **Ultimate Form** — In X turns, lose ALL Bad Faith.
- [ ] **Mecha Expansion Pack** — Grants 7 Strength and 7 Dexterity for X more turns.
- [ ] **TODO (`FRY_TODO1_POWER`)** — Whenever any other player ends their turn, gain Stars equal to their remaining energy, and ALL players gain the same amount of Block next turn.
- [ ] **TODO (`FRY_TODO4_POWER`)** — Whenever an enemy breaks your Block, gain X Strength.
- [ ] **Recycled Animation** — The first X times you spend Stars each turn, gain 1 energy.
- [ ] **Bloody Handprint** — You deal X% more damage per Damage Debt you have.
- [ ] **Blatant Lies** — At the start of your turn, gain X Bad Faith.
- [ ] **Meditation Powerup** — Whenever you Retain a card, gain X Vigor.
- [ ] **TODO (`FRY_TODO8_POWER`)** — If you play 4 or less cards in a turn, gain X energy next turn.
- [ ] **Extra Turn** — Take X extra turns after this one.
- [ ] **TODO (`FRY_TODO11_POWER`)** — Whenever a card with Retain is put into your Discard Pile, draw X cards and gain Block Block per card.
- [ ] **TODO (`FRY_TODO15_POWER`)** — Each of your Requested counters gives you X Dexterity.
- [ ] **TODO (`FRY_TODO17_POWER`)** — Whenever one of your Requested counters reaches 0, gain X energy.
- [ ] **Improvised Armour** — The first X Request cards you play each turn, gain 10 Block.
- [ ] **Rule of Cool** — Whenever you attack an enemy with less HP than X (energy and Stars spent this turn times the number of stacks), it dies.
- [ ] **Hyper-Destructive Bouncing Ball** — At the start of your turn, deal 3 damage to a random enemy X times, then increase the number of hits by 2.
- [ ] **Valkyr Mark** — For X turns, the Valkyrs deal double damage to this creature.
- [ ] **Fleeting Shell** — Until your next turn, you can't lose more than X more HP this turn.
- [ ] **Punch Parry** — Loses X Strength until the end of the turn.
- [ ] **Getting the Boot** — Loses X Strength until the end of the turn.
- [ ] **Spin to Deflect Stuff** — Loses X Strength until the end of the turn.
- [ ] **TODO (`FRY_TODO16_STRENGTH_DOWN`)** — Loses X Strength until the end of the turn.
- [ ] **Cape Swish** — Gains X Strength until the end of the turn.
- [ ] **TODO (`FRY_TODO23_DEXTERITY`)** — Gains X Dexterity until the end of the turn.

### 2.4 Relics

- [ ] **TODO (`FryRelicTodo1`)** — Whenever you end your turn, gain Stars equal to your remaining energy.
- [ ] **TODO (`FryRelicTodo2`)** — Whenever you end your turn, gain Stars equal to your remaining energy +1. Start each combat with 3 Stars.
- [ ] **TODO (`FryRelicTodo3`)** — The first time you gain Damage Debt each combat, Refuse 1.

- [ ] TODO (`FryRelicTodo3`) only triggers once per combat (it glows until it has).

---

## 3. Decisions I made that you may want to change

Spec interpretations:
- [ ] **Seels' deck** is 4 Strike, 4 Defend, 1 Blood Cage (9 cards).
- [ ] **Blood** only comes from relics and cards, never automatically.
- [ ] **Bad Faith** absorbs *all* HP loss, not only attack damage. **Damage Debt** is a Buff (so Artifact can't block it).
- [ ] **Strike First**: the spec gives it an X cost but never uses X. I made it **hit X times** (X+1 upgraded).
- [ ] **Pain Chock**: name kept as written. Typos fixed: "Concede First Blow", "Questionable Strategy", "U-Turn".
- [ ] **Toss A Coin** and **TODO 2**: the other player's cards are picked at random (no remote choice screen).
- [ ] **Copy Kitty** plays a fresh copy of the card (upgraded if the original was). Targeted cards pick a random target.
- [ ] **Smug Look** damage doesn't stack when you play it twice (the duration does).
- [ ] **Live With The Pain**: each stack makes you lose 2 HP separately. The upgraded card applies a separate "Live With The Pain+" power that counts Exhausted Wounds.
- [ ] **Last Chance Hit Point** and **Blocking Stops All Damage**: the Hardened Shell only lasts **until your next turn** (custom "Fleeting Shell" power, which also allows a cap of 0).
- [ ] **Ultimate Form** removes Bad Faith at the start of your turn, 2 (3) turns later.
- [ ] **Mecha Expansion Pack**: Shrink amount 3 ⚠️, plus +7 Strength/Dexterity that is removed after 3 turns.
- [ ] **Friendly Ghost** summons Osty for Fry. **Casting A Shadow** adds an orb slot if Fry has none (base game behaviour).
- [ ] **Claw Strike** also buffs the Defect's base-game Claw (via reflection).
- [ ] **TODO 9** (extra turn) uses the base game's extra-turn hook, the same one Pael's Eye uses.
- [ ] **Ultimate Form** carries the Slal "Form" tag, so Genie's Advice reduces its cost.

Content I skipped (incomplete in the spec):
- [ ] Seels' 5 incomplete rows: the Nimble Blood Bend, "Choose 1 of 2 colorless", "Draw X cards…", Final Showdown, and the Vulnerable→Strength power.
- [ ] Fry: "No Endor Holocaust" and "Slap-On-The-Wrist" (no effect), the 5 relics with no effect, and all Fry potions.

## 4. Placeholders to fill in later

- [ ] Real names for every "TODO" card and relic (search for `FryTodo` / `FryRelicTodo`).
- [ ] Every `//todo : choose cost`, `//todo : upgrade not specified` and `//todo : type not specified` in `ShionTheBruiserCode/Cards/` (listed as ⚠️ above).
- [ ] Art: character visuals and scenes, card portraits, power, relic and potion icons, energy counter, map marker, character-select portraits.
- [ ] Architect dialogue lines (`ancients.json`) and Fry's character-select description (`characters.json`).

---


## 5. Slal: "To change-2" pass

- [ ] Re-export the `.pck` with MegaDot (new scene: `scenes/fx/memory_bulbs.tscn` + `memory_bulbs.gd`; MegaDot will generate their `.uid` files), then build and launch.

### 5.1 Cards

- [ ] **Draft Phase** no longer has Always Novel.
- [ ] **Favourite Bucket**: 1 cost. Slimy. Gain 11 Block, gain 2 Weak, ignore your Vulnerable until your next turn. Exhaust. Upgrade: loses Exhaust (no more Retain or double Block).
- [ ] **Logic Shift**: 0 cost, no target. Always Novel. Choose a card in your Discard Pile to put into your Hand, then discard 1 card. Exhaust. Upgrade: loses Exhaust. Works with an empty Discard Pile and with an empty Hand.
- [ ] **Sparrow Form**: the first X non-free cards you create each turn become free to play. The cost stays at 0 until the card is played, even if it was created in the Draw/Discard Pile.
- [ ] Sparrow Form's icon counts down the triggers left this turn and resets at the start of your turn. Playing a second Sparrow Form raises it.
- [ ] Sparrow Form ignores 0-cost, X-cost and Unplayable cards. It also ignores cards from **Splash**, **Genie Form** and **Creativity Unbound**, which are created already free.
- [ ] **Creativity Unbound**: Always Novel (kept Exhaust and its cost/upgrade). Adds Genie, Television or Sparrow Form, free this turn. Those three are **no longer Goofball Token Powers** (Surprise Shapeshift, Hat Tip etc. never create them).
- [ ] **Trash Disposal** and **Enthusiastic Wave** have Always Novel (Trash Disposal still draws 1).
- [ ] **Instant Mattress**: another player gains 4 Block and ignores Vulnerable until your next turn, and you gain 4 Block. Upgraded: targets ALL players, 6 Block each, all ignore Vulnerable. The old damage redirect and Dexterity are gone.
- [ ] **Ignore Vulnerable** (new power): an enemy attack on a Vulnerable player with it does normal damage, also with Paper Phrog / Cruelty / Debilitate. Removed at the start of your next turn.
- [ ] **Slime Form**: upgrade now reduces the cost to 1 (Block per Slimed stays 3).
- [ ] **Share Secrets** is a Skill: apply 15 Confusion, add a Fresh Idea (+ if upgraded) to Draw Pile, Hand and Discard Pile. No damage.
- [ ] **Surprise Shapeshift** applies Confusion *before* dealing damage, so it can execute with its own hit.
- [ ] **Emerald Power: Cyan**: 4 cost, upgrade reduces it by 2 (no more Retain). Still gets 1 cheaper when drawn.
- [ ] Epoch 3 (**Mimickery**) unlocks **Absorption** instead of **Cartoonish Mimickery**. Cartoonish Mimickery is available from the start.

### 5.2 Keywords

- [ ] **Novel** tooltip: "When played, gain 1 Random. Identical cards lose Novel this fight, including future copies. Can't be played by Memory."
- [ ] **Always Novel** tooltip: "Novel. This keyword is not removed after being played."
- [ ] **Memory** text says non-Rare, and Memory no longer plays **Rare** cards (still skips Novel, Unplayable and Exhaust cards). Also with Continuity Thread's choice screen.

### 5.3 Visuals

- [ ] **Confusion execute**: when an attack doesn't kill a Confused enemy but Confusion executes it, a green puff of smoke appears and the enemy flies right and slightly up, spinning clockwise (almost one full turn) until it leaves the screen. There's no normal death animation.
- [ ] Spin-off: spins around its own center (not its feet). Works for big/small enemies and flipped ones. The death sound still plays, and the combat ends normally if it was the last enemy.
- [ ] Spin-off: enemies that revive or prevent death (e.g. multi-phase bosses) don't fly away. Attacks that kill without the Confusion execute play the normal death.
- [ ] **Memory bulbs**: one light bulb per stack of Memory (incl. Memory this turn) over the head of any player who has it. Colors cycle red, green, blue, yellow, purple, white.
- [ ] Bulbs orbit slowly and never go below the neck. Back ones are smaller and darker. They grow in/shrink out when Memory changes and disappear on death.
- [ ] During a Memory proc, the bulbs hop one after the other, one per card played. Television Form replays don't make them hop.
- [ ] Bulbs are positioned well on Slal and on other characters who got Memory (e.g. Tabula Rasa). Size/orbit values are exported in `memory_bulbs.gd` if they need tuning.

### 5.4 Decisions I made

- [ ] "Ignore Vulnerable this turn" lasts **until your next turn** (so it covers the enemy turn), as a new **Ignore Vulnerable** buff.
- [ ] Sparrow Form makes cards free **until played** (not only this turn). The spec just says "free to play".
- [ ] The old **Instant Mattress**, **Instant Mattress Dexterity**, **Favourite Bucket** and **Logic Shift Strength Down** powers stay in the code but nothing applies them anymore.

## 6. Crunch rework

- [ ] **Crunch** at 3+ Vigor (Galvanised 3): applies 1 Weak and 1 Vulnerable (2 each upgraded). It doesn't consume Vigor and goes to the discard pile (no more return to Hand).
- [ ] At 9+ Vigor (Galvanised 9): also applies 1 Debilitate (2 upgraded).
- [ ] Under 3 Vigor: Exhausts and draws 1 card, with no debuffs. It glows gold at 3+ Vigor and red otherwise.
- [ ] Spirit Break / Defensive turn Crunch off (Exhaust + draw). Fell Dragon always gives the full Galvanised 9 effect.
- [ ] A Galvanised Crunch locks Defensive cards for the turn, like other Galvanised triggers.
- [ ] Critical Draw 10% still works.
- [ ] **Shield Breaker**: the Vulnerable-on-Attack effect now triggers at **Galvanised 9** (was 15). Card and power text say 9.

## 7. Slal: "Change Pending" pass

- [ ] Re-export the `.pck` with MegaDot (renamed images: `powers/flashback.png`, `card_portraits/slal_cartoonish_mimicry.png` + `big/` versions; edited `memory_bulbs.gd` and `shion_auras.gd`; new `Powers/SlalFirstPlaysEachTurnPower.cs` needs its `.uid`), then build and launch.
- [ ] Drop a bowling sound at `ShionTheBruiser/audio/sfx/confusion_bowling.mp3` (nothing plays until it exists).

### 7.1 Memory / Flashback rework

- [ ] **Flashback** (was Short-Term Memory, renamed class/ID/icon): at the end of your turn, plays X random non-Rare cards without Exhaust (still skips Novel/Unplayable) from the Discard Pile, then is removed.
- [ ] **Memory**: at the start of your turn, gain X Flashback. Gaining Memory also gives that much Flashback right away (not written on cards).
- [ ] Cards played by Flashback apply X more Confusion, X = your **Memory** (no bonus with Flashback alone).
- [ ] Random reaching 10 still gives 1 Memory (so +1 Flashback right away too).
- [ ] **Novel** tooltip: "Can't be played by Flashback". **Continuity Thread** picks the cards Flashback plays (still Exhausts them).
- [ ] Bulbs now count **Flashback**, are twice as big, and keep their size when Slal shrinks with HP (only the orbit follows the body). They vanish at end of turn and come back with Memory's Flashback.

### 7.2 Cards

- [ ] **Anglerfish Form**: no longer stacks. A second copy only gives its Slippery; Weak → always 1 Vulnerable.
- [ ] **Anvil Gag**: Gain 1 Memory, deal 14 (20) damage, Weak = Random, +4 (+5) damage per Flashback gain this combat. Retain upgrade removed. The damage shown in Hand includes the Flashback from its own Memory.
- [ ] **Apple Juice Stocks**: first Slimed from Hand each turn grants 1 energy; no extra play anymore.
- [ ] **Bell Memory**: 6 damage twice, gain 1 Flashback.
- [ ] **Bold Announcement**: applies 6 Confusion **before** the 1 damage to ALL (can execute), gain 1 Flashback per enemy.
- [ ] **Break Continuity**: now a 2-cost **Power** (Ancient). Whenever you play a Novel card: Free Shapeshift once, gain 1 Frail and 1 Disintegration (base game power). Upgrade: Innate. Old effect gone.
- [ ] **Bumper Form**: the first time you play a Power each turn, a random card in Hand that costs energy costs 1 less this turn. Upgrade still -1 cost.
- [ ] **Burst of Laughter**: Gain 1 Weak, gain 1 (2) energy. No more double-Confusion effect.
- [ ] **Cartoonish Mimicry** (renamed class/ID/portrait): Exhaust a card in Hand. Costs 0 / X / Unplayable → 1 Slippery, else Mimic its cost (skippable). Keeps Exhaust; upgrade still removes it.
- [ ] **Cartoony Dodge**: Always Novel, targets an enemy: 6 Confusion, 4 Block per Flashback, lose all Flashback. Upgrade -1 cost still flagged `//todo : upgrade not specified`.
- [ ] **Chaotic Clone**: play a random Slimed from Hand (if any), then choose to Mimic 2 or Shapeshift (always). No Block anymore.
- [ ] **Continuity Thread**: text only (Flashback).
- [ ] **Dangerous Ideas**: 1 cost. Whenever you create a card, a random enemy gains 3 (5) Confusion (no more Innate).
- [ ] **Distraction**: Shapeshift. Exhaust. (Upgraded token when upgraded.)
- [ ] **Emergency Shapeshift**: Exhaust up to 3 cards in Hand, Free Shapeshift for each. Keeps Exhaust and the upgraded-tokens upgrade.
- [ ] **Favourite Bucket**: 2 cost. 10 (14) Block + 3 (4) per Slimed in Hand, then plays those Slimed, then Ignore Vulnerable until your next turn. No Slimy, no Weak, no Exhaust.
- [ ] **Funny Gag**: +4 damage per Flashback gain.
- [ ] **Hat Tip**: whenever you play a card with Shapeshift or Mimic, Free Shapeshift once.
- [ ] **Kickstart Event**: the first 3 (5) Novel cards each turn grant 1 energy. Stays 2 cost when upgraded.
- [ ] **Logic Shift**: 1 cost, upgrade → 0 cost (keeps Exhaust now).
- [ ] **Plot Twist**: 3 Confusion, choose to Mimic 2 or Shapeshift. Keeps Exhaust.
- [ ] **Proof Reading**: 0 cost. Gain 1 energy and draw 1 per Flashback, lose all Flashback. Exhaust (upgrade: no Exhaust).
- [ ] **Slide Form**: 2 cost (1 upgraded). The first 2 Novel cards each turn draw 1 card.
- [ ] **Slime Appetite**: 1 cost (0 upgraded). Start of turn: add 1 Slimed to Hand.
- [ ] **Slime Form**: 1 cost (0 upgraded). Whenever you create a Status card, gain 4 Block.
- [ ] **Slime Rain**: damage shown in Hand includes the 3 Slimed its Very Slimy makes before the hit.
- [ ] **Sparrow Form**: text now says "free to play this combat" (behaviour was already like that).
- [ ] **Squeaky Squirrel Form**: the first time you play a Power each turn, draw 1 card. Upgrade → 0 cost. Still says "Pouic!".
- [ ] **Surprise Shapeshift**: 5 Confusion, 5 damage, Shapeshift (into Hand now, not Draw Pile). Upgrade keeps +1/+1 and Shapeshifts twice.
- [ ] **Wait a Second**: Gain 1 Weak and X (X+1) Flashback. Retain Hand. Exhaust.

### 7.3 Keywords, relic, text

- [ ] New hover tips: **Shapeshift**, **Free Shapeshift**, **Mimic X**, **Goofball Token Powers** (lists the 6 forms). Shapeshift gives different tokens when it gives several.
- [ ] **Champagne**: needs a Status, a Common, an Uncommon and a Rare each turn for its 1 Slippery.
- [ ] **Aroma of Chaos** line: cyan (`[aqua]`) and wavy.

### 7.4 Visuals and sounds

- [ ] **Confusion execute** plays the bowling sound (once the file is there).
- [ ] **Slime Hammer**: Bludgeon's bonk + heavy attack sound.
- [ ] **Dice Shotgun**: Dagger Spray's flurry, impacts and sound.
- [ ] **Slime Strikes**: Gunk Up's goop + its blunt sound. **Slime Hug**: Gunk Up's goop on the target (no sound).
- [ ] **Burlesque Wind-Up**: Shion's blue flames (Spirit Break's) on whoever plays it, until the turn ends with no Vigor or until the Vigor it gave is used up.

### 7.5 Decisions I made

- [ ] Hat Tip only triggers on cards tagged Shapeshift/Mimic: Distraction, Surprise Shapeshift, Plot Twist, Chaotic Clone, Cartoonish Mimicry and Emergency Shapeshift (not Break Continuity or Hat Tip itself).
- [ ] "Choose to Mimic 2 or Shapeshift" stays a single screen: 2 other-character cards + 1 Goofball Token Power (skippable).
- [ ] Favourite Bucket's "This turn, ignore your Vulnerable" uses the existing Ignore Vulnerable buff (lasts until your next turn).
- [ ] "First X per turn" powers (Bumper, Squeaky Squirrel, Kickstart, Slide, Apple Juice) count each card once, even if it's played several times (Echo Form…). Their icon shows X, not a countdown.
- [ ] Bold Announcement's Flashback counts as one Flashback gain for Funny Gag / Anvil Gag, whatever the number of enemies.
- [ ] Old **Burst of Laughter**, **Blank Mind** (Memory Blocked) and **Slal On Power Played** base code is kept but nothing uses it anymore.

### 7.6 Left out

- [ ] **Sunken Treasure** "écrit en wavey": the mod has no Slal text for that event. Tell me which line.
- [ ] **Elaborate Flourish** (suggested illustration): no image was in the spec.
- [ ] The **Mimickery** epoch keeps its name and ID (renaming the ID would reset its unlock in saves). Rename only its title if wanted.

## 8. Grey Chaos Emerald rework

- [ ] **Grey Chaos Emerald** (and the combo Chaos Emeralds): after each fight, one random card in your Deck gets **Glam**, with the enchant animation. Hovering the relic shows Glam's tooltip.
- [ ] Only cards Glam can go on are picked: no Status/Curse, no Unplayable, and no card that already has an Enchantment.
- [ ] The old effect is gone: no card choice at the start of combat, and no card removed from your Deck after combat.
