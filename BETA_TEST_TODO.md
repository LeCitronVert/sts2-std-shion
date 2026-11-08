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
- [ ] Losing HP from any source (enemy hit, self damage, Wounds…) with Thorn Bracelet gives exactly 2 Blood per HP-loss event, whatever the amount lost.
- [ ] Fully blocked hits don't give Blood.
- [ ] Blood stays between turns.

**Blood Bend X**
- [ ] Consumes ALL Blood. X = the Blood consumed (0 if you had none).
- [ ] The "(X+1)" upgrades add 1 on top of the consumed Blood.
- [ ] Thorn Bracelet gives +1 Blood after **every** Blood Bend card, even with X = 0.
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

- [ ] **Thorn Bracelet** — Whenever you lose HP, gain 2 Blood. Whenever you play a Blood Bend card, gain 1 Blood after you consumed your Blood.
- [ ] **Thorn Bracers** — Whenever you play a Blood Bend card, lose 1 HP after it resolves. Whenever you lose HP, gain 1 Blood.
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
- [ ] Seels' 5 incomplete rows: the Nimble Blood Bend, "Choose 1 of 2 colorless", "Draw X cards…", and the Vulnerable→Strength power. (Final Showdown: added in section 15.)
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

## 9. Team Blast duplicates

- [ ] With 2+ copies of the same Team Blast, a player only gets **one** Ethereal copy of it per turn once the gauge is full (the upgraded one if any copy is upgraded). Different Team Blasts still each give a copy.
- [ ] Each duplicate (copies after the first, per player and per card) makes the **whole team** fill the gauge 20% faster: 2 copies = +20%, 3 copies = +40%, and two players each with 2 copies = +40%.
- [ ] Gauge hover tip says "Filling X% faster" when there's a bonus.
- [ ] Upgraded and unupgraded copies of the same card count as duplicates.

## 10. Crit lines

- [ ] The old "Crit!!" bubble is gone. Crits on characters with no lines (Fry, Seels, Slal) only show the damage number.
- [ ] Ironclad, Silent, Defect, Regent, Necrobinder and Shion say a random crit line about **10%** of the time per crit that deals unblocked damage.
- [ ] While Shion is **Berserk**, she uses her red Berserk lines instead of her normal ones.
- [ ] Text effects look right: shake on "WEAK!!", "MORE", "YES!!"; jitter on "BLEED"; bold italic on the second "HARDER!!!". The long Defect hex line fits in the bubble.
- [ ] Multiplayer: every player sees the same line at the same time.

## 11. Berserk hurry-up ping

- [ ] Co-op: when Shion is **Berserk** and alive, her end-turn "hurry up" ping says "You're such a tease..." in red, not "Everything okay?".
- [ ] Out of Berserk, or dead, she uses her normal alive/dead lines.
- [ ] The other players see the Berserk line too.

## 12. Shion VFX

- [ ] Images are new: run a Godot export (Publish) once so they get imported, or the effects won't load.
- [ ] **Come What May**: Shion plays her Cast animation, with full screen white speedlines behind her (bursting from the bottom center of the screen) for about 0.9s. The lines look like they're running: frames swap and mirror at random, zoom outward, and dashes race outward along the lines.
- [ ] **Spirit Burst** autoplays: yellow lightning (cycling 3 frames) crosses the whole screen horizontally behind Shion, at her body's height, while each card autoplays. Chained autoplays keep the same lightning without flickering; it fades out after the last one.
- [ ] The lightning always spans the whole screen width, including in fullscreen / other resolutions and during camera shake.
- [ ] Hits from cards Spirit Burst autoplayed show a flurry of golden slashes on each target, instead of Hellraiser's VFX.
- [ ] **Energy Attacks**: when the ki blast lands, an explosion (flash, golden ring, fireball, smoke) plays on the target, on top of the existing ki burst. Check it isn't too much on multi-hit cards (Energy Volley).
- [ ] Multiplayer: every player sees these effects.

## 13. Come What May rework

- [ ] New image (`images/ui/stars/shion_stars_icon.png`): run a Godot export (Publish) once so it gets imported, or Shion's star counter keeps the vanilla star.
- [ ] Playing it gives **9 Stars**. Text: "Gain 9 ★".
- [ ] At the start of each turn: lose 1 HP, gain 1 Energy, **1 Star**, and draw 1 card. Two stacks → 2 of each (HP loss scales too).
- [ ] Power tooltip shows the Star gain with the stack count.
- [ ] As **Shion**, the star counter (next to the energy orb) shows the blue icon instead of the gold star, with no rotating star layers. It still dims at 0 Stars and flashes when gaining Stars.
- [ ] As another character (e.g. Regent, or anyone via Thorn Ring), the star counter is unchanged.
- [ ] Transform distribution (hidden from the player): over a few plays, roughly 40% Colorless, 20% vanilla characters, 30% this mod's other characters (Slal, Seels, Fry, Shion if you aren't Shion), 10% other mods' characters. Never your own character's cards.
- [ ] With no other character mods installed, the "other mods" 10% is dropped and the rest scales up (≈44/22/33). No crash.
- [ ] Multiplayer: the transformed cards match on every client (no desync).

**Decisions I made**
- HP loss stays and scales with stacks (confirmed).
- The blue icon is only on Shion's star counter. Card/power text keeps the vanilla star glyph (confirmed).
- "9 Stars" is written as "9 ★" rather than 9 star glyphs in a row.
- The weights are per transformed card: each card first rolls a category, then a card from it. Empty categories are skipped.
- "Your own character" is excluded from every category, so a non-Shion player using Come What May can get Shion cards in the 30% bucket.

## 14. "First X times each turn" countdown

- [ ] The power icon number shows how many triggers are **left this turn**, goes down by 1 on each trigger, and goes back to full at the start of your turn.
- [ ] **Apple Juice Stocks**: goes down only on Slimed played from Hand (not auto-played ones).
- [ ] **Slide Form** / **Kickstart Event**: goes down on Novel cards.
- [ ] **Bumper Form** / **Squeaky Squirrel Form**: goes down on Powers. Echo Form-style repeats count once.
- [ ] **Sword Form**: goes down on Attacks.
- [ ] **Octopus Form**: goes down once per enemy affected (an AoE card can use several at once).
- [ ] **Box of Nails Form**: goes down when you're hit by an attack. It refills at the start of **each** side's turn (unchanged behavior), so it's full when the enemy turn starts.
- [ ] **Sparrow Form**: unchanged (it already counted down).
- [ ] Stacking a power mid-turn raises the number by the added amount.
- [ ] The hover text still shows the full X ("The first 2 Attacks..."), not the remaining count.

**Decisions I made**
- The countdown is only on the icon. The description keeps the full amount.
- At 0 the icon shows 0 (same as Sparrow Form already did); the power isn't hidden.

## 15. Seels card art + Final Showdown

- [ ] Re-export the `.pck` with MegaDot (93 new images: `card_portraits/seels_*.png` + `big/` versions; MegaDot will generate their `.import` files), then build and launch.
- [ ] Every Seels card shows its art (no fallback `card.png`), both in hand and in the big inspect view.
- [ ] **Blood Prison** and **Gambling Addiction** (Ancient) use the tall ancient frame art. Check Blood Prison's crop: the source is wide, so only the center is kept.
- [ ] **Final Showdown** (Rare Skill, cost 1 ⚠️): applies 3 (5) Vulnerable, **then** 10 Artifact to the enemy.
- [ ] You gain Strength and Demise equal to the enemy's **total** Vulnerable (including Vulnerable they already had).
- [ ] Draws 3 (5) cards.
- [ ] Preview: when aiming at an enemy, the card shows "(Gain X Strength and Demise)". It shows only the enemy's current Vulnerable if they already have Artifact (the new Vulnerable gets blocked).
- [ ] Demise ticks on you at end of turn.
- [ ] Final Showdown appears in Seels' card rewards.

**Decisions I made**
- The 10 Artifact goes on the enemy (confirmed), after the Vulnerable, so it doesn't block it.
- Cost 1 (confirmed), still flagged `//todo : choose cost`.
- Strength/Demise reads the enemy's total Vulnerable after applying (confirmed). Nothing is gained if it's 0.
- Uses the base game's `DemisePower` (the one from Powdered Demise).

## 16. Seels in-combat previews

- [ ] Build and launch (code + localization only, no `.pck` re-export needed).
- [ ] **Strike First**: while aiming, shows "(Deal N damage M times)". N doubles when you're Vulnerable and again when the enemy intends to attack; M = your energy (+1 upgraded). With no target it shows only "(Hits M times)".
- [ ] **Blood Cage** / **Blood Prison**: "(Apply N Vulnerable)" = your Blood (+1 upgraded); Prison+ says "twice".
- [ ] **Blood Bath**: "(Gain N Block)" = (Blood (+1)) × Block, with Dexterity applied to each gain.
- [ ] **Bloody Execution**: the preview now includes your Blood ("Deal N damage" instead of "N + X").
- [ ] **Exsanguination**: "(Deal N damage)" = your Blood, tripled when the condition is met (matches the gold glow).
- [ ] **Payback Time** (multiplayer): the preview no longer breaks. Shows "(Deal N damage M times)", M = your Blood. Upgraded: N already counts your Blood Bend heal.
- [ ] **Scarlet Shadow**: "(Gain N Blood)" = Blood × 2 (3).
- [ ] **Spectacular Break-In** / **Pin Them Down**: "(Add N Slashes)" = Blood (+1).
- [ ] **Funny Dance**: "(Gain X Vulnerable and N Block)" for this turn's Block.
- [ ] **Summon Wrath**: "(Lose X HP and get N Slashes)"; upgraded: "(Get N Slashes)". N is capped by the Slashes in your Draw + Discard piles.
- [ ] **High Reward**: "(Hits 1/3 times)" depending on whether you're Vulnerable.
- [ ] **Ultra Kill**: "(Lose N HP)" = 6 − current cost (changes when Erratic rerolls it, never below 0).
- [ ] **Slash**: "(Hits N times)" only shows when Untelegraph Attacks makes it hit more than once.
- [ ] **Go Crazy**: "(Add N Slashes)" = free Hand slots (this card doesn't count).
- [ ] **Echoes of Pain**: "(Shuffle N Wounds)" and "(Draw N cards)".
- [ ] **Hematoma**: "(Draw N cards)" = Wounds in Hand + the one it adds.
- [ ] **Pain Chock**: the preview now counts the Wound it adds first (it used to show 1 less than it dealt).
- [ ] **Reckless Charge** / **Pain Chock** / **Hematoma** with a full Hand: the added Wound isn't counted if it can't fit.
- [ ] **Big Target Sign** (multiplayer): aiming at an ally shows "(Gain N Vulnerable)".
- [ ] **Do This All Day**: "(Move N Slashes)".
- [ ] **Improvised Acupuncture**: "(Draw N cards)" grows with the HP loss each play.

**Decisions I made**
- Blood Bend X is now resolved in previews (current Blood), the same way X-cost cards show current energy.
- No previews on Powers, or where the number depends on random draws or on how much damage gets through (Belittle, Body Strike, Palm Strike, Private Joke, Running Joke, Copy Kitty).
- An Opening and Never Learn have no number to show (they already glow gold when their condition is met), so they didn't get preview text.

## Team Blast: Rest Site reset + Elite/Boss multipliers

- [ ] Fill the gauge partway, enter a **Rest Site**: the gauge is back to 0 in the next combat.
- [ ] Same in multiplayer: every player's gauge reads 0 after the Rest Site (no desync).
- [ ] The gauge tooltip now says it empties at Rest Sites.
- [ ] **Elite** and **Boss** fights fill the gauge exactly like normal fights for now (`EliteMultiplier` / `BossMultiplier` = 1x in `TeamBlastSingleton`).

**Decisions I made**
- The reset happens on entering the Rest Site, whichever option is picked.
- The Elite/Boss multipliers stack with the per-Team-Blast charge bonus (they multiply each other). They aren't in the tooltip while they're 1x.

## 17. Shion flavour VFX

- [ ] Re-export the `.pck` with MegaDot (13 new scenes + scripts and 2 shaders in `scenes/fx/`; MegaDot will generate their `.uid` files), then build and launch. If any effect is missing, check MegaDot's output for GDScript errors first: I couldn't run the scripts.
- [ ] **Berserk Form**: Nightmare's smoky vignette, tinted crimson, fades in around the screen edges while Shion has Berserk Form, and stays. It fades out when the power goes away and is gone after combat.
- [ ] Berserk Form (multiplayer): only the player whose Shion has it sees the vignette.
- [ ] **Team Blast**: confetti falls from the top of the screen on every player's screen, then clears (about 5s max).
- [ ] **Borrow Technique**: Spirit Burst's lightning crosses the screen once (one loop), then fades.
- [ ] **Codependency**: salmon-pink glow behind the creature that has it, and a pink light on their body (players).
- [ ] **Come What May**: tears fall from both sides of Shion's head now and then while she has the power. Only on Shion. ⚠️ Eye positions are guessed from her bounds; tweak in `CreatureFxController.RefreshPowerEffects`.
- [ ] **Competitive Spirit**: a red 💢 pops on Shion's head each time it gives Strength.
- [ ] **Pout**: same 💢 on Shion when played.
- [ ] **Crunch** (Galvanised branch): Fur Coat's bite on the enemy.
- [ ] **Defensive**: a pale honeycomb barrier in front of Shion while she has Defensive. Cells pop in from the center, shimmer, and scatter when it ends.
- [ ] **Mighty Guard**: the same barrier, azure, with thicker edges and a second inner layer. Both can show at once.
- [ ] **Dragon Roar**: purple shockwave arcs from Shion's mouth towards the enemies, plus a medium screen shake.
- [ ] **Dragonsong**: white notes with a black outline orbit Shion, one per point of reduction (= min(unique buffs, Dragonsong)). Shapes are random (quarter, eighth, beamed, sixteenth). The count follows buffs gained/lost.
- [ ] Dragonsong: all notes flash red when a hit's damage is reduced.
- [ ] **Dragon Pulse**: translucent purple wind streaks and a ring gust out of Shion.
- [ ] **Dragon Smash**: Shion strikes once (no damage), the enemy flies to an upper corner, then bounces off screen edges, spinning fast. Each collision = one damage instance with a hit VFX and a small shake. The last hit slams it back in its place, upright.
- [ ] Dragon Smash: X = 1 → launch, then straight back down for the only hit. Enemy dies mid-flight → it snaps back home afterwards. Health bar/intent stay in place (only the body flies).
- [ ] Dragon Smash in Instant fast mode: no flying; the hits play normally.
- [ ] **Eleventh Hour**: Shion turns black and white and animates at 20% speed while she has it; back to normal when it ends.
- [ ] **Extreme Speed**: same on the enemy (works on Spine enemies).
- [ ] **Energy Bomb**: when the blasts land, for a split second the background goes white and every creature is a white silhouette with a black outline, then a white flash fades. ⚠️ Check the white backdrop is behind the creatures (it goes in `BackCombatVfxContainer`).
- [ ] **Energy Bind**: purple fiery aura flares on the enemy, holds ~1.6s, then sinks and flickers out.
- [ ] **Energy Burst**: same, on Shion.
- [ ] **Energy Volley**: more hits = faster blasts and animation (+15% per extra hit, up to 3x). Each hit pushes the enemy back a little; it slides back at the end.
- [ ] **Flow Charge**: orange particles drift up around Shion while she has the power. Berserk Flow Charge: orange fiery aura (like Energy Burst) when played.
- [ ] **Improvise**: a light bulb pops above Shion's head, lights up, shakes and explodes into shards.
- [ ] **Lurking Intensity**: red particles drift up around Shion while she has the power.
- [ ] **Nap**: 4 Zs rise from Shion's head and float off screen. Her animations are 20% slower until the last Z is gone.
- [ ] **Eepy**: Zs keep rising and she stays 20% slower until her next turn starts.
- [ ] Slowdowns stack sensibly: Eleventh Hour + Nap → the slower one (20%) wins; multi-hit attacks still speed up each loop.
- [ ] **Monster Spell**: 50% chance Shion says a line when played, depending on the enemy it was copied from. Berserk → only the red Berserk lines.
- [ ] Monster Spell lines: try one enemy per group (e.g. Nibbit, Leaf Slime, Bowlbug, Axebot, The Lost, a Ruby Raider, a Cultist, and an unlisted one like a Gremlin → "Other"). `[jitter]`/`[shake]` render.
- [ ] Monster Spell (multiplayer): every client shows the same line or none, no desync.

**Decisions I made**
- Dragon Roar: Piercing Wail has no VFX in the base game (only the Cast animation), so it's a programmatic purple shockwave + screen shake (confirmed).
- Black and white is on the affected creature's body only, not the whole screen (confirmed).
- Lurking Intensity / Flow Charge particles last as long as the power (confirmed).
- Berserk Form's vignette is local only, like Nightmare (confirmed). Nightmare's hands VFX aren't used, only the edge vignette.
- Energy Bind / Energy Burst / Berserk Flow Charge reuse Shion's aura spritesheets: Fell Dragon's (purple) and the Crit/Vigor one (orange).
- Defensive's barrier is pale lavender (no colour was given); Mighty Guard's is azure.
- Dragon Smash's bounce spots are random (cosmetic RNG, not the run's). The original blunt hit VFX now plays at each collision instead of at the start of each hit.
- Energy Bomb's impact frame plays once per attack, not once per enemy hit.
- Monster Spell: while Berserk, only Berserk lines; otherwise only normal lines (same as the crit lines). Typos in the sheet fixed ("(jiter)…(/juter)", the stray "(" after "under your skin!"). Kaiser Crab = its Crusher and Rocket halves. The sheet listed Stabbot twice; I put **Zapbot** in the Machine group (`//todo : check Zapbot's group`). Unlisted enemies (Gremlins, Byrdpip, Architect, Wriggler, Torch Head Amalgam, The Adversary…) use "Other".
- Monster Spell always uses the same two shared RNG rolls, even when the line is skipped, so clients in different languages can't desync.

## 18. Slal: Change Pending B

- [ ] Re-export the `.pck` with MegaDot (`scenes/fx/shion_auras.gd` changed), then build and launch.

**Random / Memory / Confusion**
- [ ] **Random**: Confusion applied by Slal's cards gets +current Random (1–10) on **each** application. Hover a Confusion card with e.g. 4 Random: `{Confusion}` shows the boosted value in green.
- [ ] Random's tooltip shows the current bonus ("increased by X").
- [ ] Confusion from **powers** is NOT boosted: Pinwheel Form, Pirate Parrot Form, Random Blast, Dangerous Ideas, Conclusion. Random Blast's card text doesn't show the bonus either.
- [ ] **Memory** still adds its Confusion on Flashback plays, on top of Random. Its text is unchanged.
- [ ] Computed previews include Random (and Memory on Flashback plays): Frontflip and a Half, Crypted Lore, Slime Hug, Elaborate Flourish, Cartoony Dodge.
- [ ] Multi-application cards: Share Secrets (twice), False-Con Punch (each hit), Elaborate Flourish (each repeat), Cartoony Dodge (once per Flashback) all get the bonus each time.

**Keywords**
- [ ] New hover tips **Shapeshift+** and **Free Shapeshift+** (upgraded Goofball Token Power) appear on Distraction+ and Emergency Shapeshift+.

**Cards**
- [ ] **Chaotic Clone**: gain 6 Block; plays a random Slimed; **only if it did**, Mimic 2 / Shapeshift choice. Upgraded text: "...or Shapeshift and upgrade that card."
- [ ] Chaotic Clone / **Plot Twist** played by **Flashback**: no choice screen, always Shapeshift (upgraded token if the card is upgraded). Also with Continuity Thread.
- [ ] **Burlesque Wind-Up**: Always Novel instead of Exhaust. Its blue aura is ~30% bigger and 30% transparent (the light on the body is unchanged).
- [ ] **Emergency Shapeshift**: first two sentences on one line; upgraded says "Free Shapeshift+".
- [ ] **Distraction**: Shapeshift; glows gold while you're Weak, and the token is then free this turn. Upgraded: Shapeshift+ / Free Shapeshift+.
- [ ] **Plot Twist**: Self-target, gain 1 Flashback, Mimic 2 or Shapeshift. No Exhaust, no Confusion anymore. Flashback can now play it.
- [ ] **Proof Reading**: draw 1 (2) cards even with 0 Flashback, then gain 1 energy per Flashback, lose all Flashback. Keeps Always Novel and Exhaust (upgrade no longer removes Exhaust).
- [ ] **Frontflip and a Half**: gain 2 Weak, now Exhausts.
- [ ] **Wait a Second**: X Weak to ALL enemies (preview shows X), gain Flashback = different cards in Hand (upgraded: all cards), preview excludes itself. No more self Weak / Retain Hand.
- [ ] **Wall of Text**: no longer Always Novel; upgraded = triple Block (keeps Exhaust).
- [ ] **Cartoony Dodge**: Always Novel, targets ALL enemies, gain 8 Block, apply 4 Confusion to ALL enemies per Flashback (preview = total per enemy). No longer loses Flashback.
- [ ] **Funny Gag**: Bludgeon's heavy blunt hit VFX + heavy attack sound.
- [ ] **Favourite Bucket**: gain 2 Weak, 14 (18) Block, 3 Free Status, ignore Vulnerable until next turn. No Slimed effects anymore.
- [ ] **Fresh Idea**: always choose the card to Transform; upgraded gains Retain.
- [ ] **Enthusiastic Wave**: gives Fresh Idea+ (not Steady Fresh Idea); upgraded 2 to ALL players.
- [ ] **Surprise Shapeshift**: now a **Skill** (check the frame/portrait looks fine), apply 2 (3) Confusion, gain 1 Flashback, Shapeshift (twice). No damage.
- [ ] **Share Secrets**: apply 7 Confusion twice.
- [ ] **Crypted Lore**: Stun threshold 40.
- [ ] **False-Con Punch**: gain 2 Weak, then 2 Confusion per different card in Hand.
- [ ] **Sarcophagus Form**: whenever you gain Weak, gain 3 (5) Plating now and 1 energy next turn. No more damage reduction. Plating gives Block at end of turn and decays.
- [ ] **Slime Strikes**: Slimy's Slimed is added before the hits; 1 hit + 1 per Slimed in Hand (min 2 with room in Hand). Preview "Hits X times". Upgrade: 5 damage only.
- [ ] **Slime It Off**: text now "Gain X Free Status" with its hover tip.
- [ ] **Pinwheel Form**: 2 (1) cost. Each Shapeshift/Mimic → 4 Confusion to ALL enemies + gain the created card's energy/star cost (unmodified, so Free Shapeshift still refunds 1; upgraded tokens cost 0 → nothing).
- [ ] Pinwheel Form: Emergency Shapeshift with 3 cards = 3 triggers; Surprise Shapeshift+ = 2; skipped Mimic still applies Confusion but gives no energy. Hat Tip / Break Continuity Shapeshifts also trigger it.
- [ ] Pinwheel Form + a Mimic'd Regent card with a star cost: gain the stars.
- [ ] **Puzzling Tip**: also gain 2 Weak (at the end).
- [ ] **Pinball Move**: already fixed in d4f630c (one multi-hit attack, Vigor on every hit). Just re-check.

**Decisions I made**
- Random adds its current amount per application; Memory keeps its bonus too (confirmed). "Directly applied by cards" = the Confusion has the owner's card as source; powers now pass no card source (Pirate Parrot Form was changed for this).
- Kept the name Cartoony Dodge (confirmed). Surprise Shapeshift → Skill (confirmed). Plot Twist loses Exhaust, Proof Reading keeps Always Novel (confirmed).
- "Free Shapeshift+" in the sheet didn't say "upgraded"; I made it upgraded, like Shapeshift+.
- "Upgrade that card" = the offered cards are upgraded (same as before, only the wording changed).
- Wall of Text: upgrade is now triple Block instead of removing Exhaust, so both versions Exhaust. Slime Strikes: upgrade is now +1 damage only (no more shuffle into the Draw Pile).
- Fresh Idea: "Transform a card" read as a choice (the old base version picked randomly).
- Favourite Bucket: the sheet says "This turn, ignore your Vulnerable", but Vulnerable only matters on the enemy turn, so it still lasts until your next turn and the text says so. No upgrade given → kept +4 Block.
- Cartoony Dodge: one 4-Confusion application per Flashback (so Random applies each time). Its upgrade (-1 cost, `//todo : upgrade not specified`) is unchanged.
- Pinwheel Form: X-cost created cards give nothing. Removed the old "spread to all enemies" mechanic and the `IsApplyingToAllEnemies` flag that only it used.
- Burlesque aura: "bigger" = 1.3x (`AURA_SCALES` in `shion_auras.gd`), 70% opacity on the flames only.
- Funny Gag: Bludgeon's hit VFX and sound, without the Ironclad heavy attacker animation.
- Puzzling Tip: the Weak comes after the Colorless card ("Also").

## 19. Seels Team-Ups, vanilla & generic Team Blasts, Power Core

- [ ] Re-export the `.pck` with MegaDot (17 new card portraits + `big/` versions, `relics/power_core.png` + `big/` + a placeholder outline; MegaDot will generate their `.import` files), then build and launch.
- [ ] Shion's Team-Ups still work exactly as before (their base class was split into `TeamUpCard` → `ShionTeamUpCard` / `SeelsTeamUpCard`): play one, the partner gets the Ethereal copy, the "Team-Up: (Class)" line and the partner text are right.

**Seels Team-Ups** (2 players: Seels + the named class; only offered when that class is in the run)
- [ ] **Scratchin' Pals** (anyone, 1): 2 (3) Slashes into Seels' and the partner's Hand, Draw Pile and Discard Pile. Exhausts.
- [ ] **Blowout Blast** (Shion): Seels' card costs 0, Shion's copy costs **X**. When Shion plays it: Seels Blood Bends, Shion deals 14 damage (Seels' X + Shion's X) times. Upgraded: +1 to each X. Previews: Seels' card shows "Your X", Shion's copy shows the total hits. Exhausts.
- [ ] **Goopy Blood** (Slal, 2 → 1): Slal creating a Slimed → Seels +1 Strength. Seels creating a Wound → Slal +2 Random. Enemy-added Slimed/Wounds don't count.
- [ ] **Mismatched Magic** (Fry, 1): Fry spends Stars → Seels loses 1 Blood, heals 2 (nothing without Blood). Seels Blood Bends → Fry loses 3 Stars, gains 10 Bad Faith (nothing under 3 Stars).
- [ ] **Stormblood** (Ironclad, 3 → 2): each attack hit by Seels or the Ironclad on a Vulnerable enemy → Seels +1 Blood. Ironclad's Attacks deal + Seels' Blood (check the damage preview on Ironclad's cards).
- [ ] **Bloody Daggers** (Silent, 2, upgrade Innate): Shivs the Silent creates show a "Blood Bend X" line + hover tip and deal +X (X = Silent's Blood, shown in the damage preview); after the Shiv is played the Silent Blood Bends (heals X), and Seels gains X Blood. The Silent gains 1 Blood per Skill played.
- [ ] **Royal Mage** (Regent, 2): Seels and the Regent each get a red **Blood Inklet** (Inklet model, facing the enemies) with 10% (20%) of their own Max HP and 1 Slippery. Check: tint, facing, position next to its owner. Hovering it shows its HP bar, powers and hover tips (on a teammate's Inklet the bar only shows on hover, like their Osty).
- [ ] Blood Inklet takes the owner's **unblocked attack** damage, before Osty (test with a Necrobinder ally: give Seels an Osty via Osty's Bestie, then Royal Mage). Playing Royal Mage again adds Max HP + Slippery to the living Inklet; once it dies, a new one is summoned.
- [ ] **Osty's Bestie** (Necrobinder, 3, upgrade Retain): the Necrobinder's Osty Max HP doubles, then Seels Summons his own Osty with that much Max HP. Check a non-Necrobinder Osty looks and behaves fine (size, position, taking hits). Nothing happens if the Necrobinder's Osty is dead.
- [ ] **Bloody Blitz** (Defect, 2 → 1): Seels and the Defect draw 3, those cost 1 less this turn, then the drawn ones that don't cost 0 (and X-cost ones) are discarded.

**Team Blasts** (start set aside, come from the gauge like the others)
- [ ] They show up in rewards for their character (Ironclad, Silent, Regent, Necrobinder, Defect) and the generic ones as Colorless Rares, in multiplayer only.
- [ ] **Bitter End** (Ironclad): 1 (2) Vulnerable to ALL enemies; per enemy, every player gets 1 (2) random Attacks from their Draw Pile into their Hand, and their highest cost Attack costs 0 this turn.
- [ ] **Bladestorm** (Silent): every player's Hand fills up with Shivs (Shivs+ when upgraded).
- [ ] **Uncontested Reign** (Regent): Forge 10 + ALL players' Strength (upgraded: everyone's, enemies included; negative Strength lowers it), preview shows the total. Then the Sovereign Blade is played on the target, wherever it was (Hand, Draw, Discard). With no Sovereign Blade yet, Forge creates one first.
- [ ] **Legion of Death** (Necrobinder): the target gains Doom equal to the unblocked attack damage it takes, this turn (upgraded: this turn and next). The debuff shows on the enemy and disappears on time.
- [ ] **Happy Fleet** (Defect): the other players (upgraded: everyone, Defect included) gain as many Orb Slots as the Defect has and Channel copies of its Orbs. Check non-Defects' orb UI.
- [ ] **Cacophony**: ALL players +3 (5) Energy and +3 (5) Stars.
- [ ] **Good Pals**: ALL players +10 (20) Block, heal 10.
- [ ] **Dragon Cheer** (Shion's Team Blast) no longer gives Block. Base text: "ALL players gain 20% Critical Hit chance, and 10% more while Berserk." Upgraded text starts with the Regen line.
- [ ] **All Out Attack**: 30 (50) damage, with every other player's Strength/Vigor/relic damage modifiers (and Shion's crit chance) applied. Check the damage preview.

**Relics**
- [ ] **Friendship Bracelet** now also adds a random Curse to your Deck (even if you skip the card).
- [ ] **Power Core** (new Neow relic, multiplayer): choose 1 of 3 Team Blasts: your character's own (if it has one) + generic ones; no other character's. Then a random Curse. Shion's own is Dragon Cheer, Slal's Random Blast, Seels' Payback Time, Fry's Mecha Expansion Pack.
- [ ] Neow: Friendship Bracelet and Power Core share her first slot (1 in 8; when both are possible, one at random). Both now use Neow's "cursed" closing line.

**Decisions I made**
- Blood Inklet HP uses each owner's own Max HP (confirmed). Bloody Daggers' Blood Bend X = +X damage (confirmed). Power Core shares Bracelet's slot (confirmed). Uncontested Reign rewritten as you asked.
- Blowout Blast: the partner's copy becomes X-cost when it's created; Seels Blood Bends when the partner plays it, and the partner deals the damage.
- Bloody Daggers: "gain the Blood they used" = **Seels** gains it (same wording as "gain 1 Strength" on Goopy Blood). The Shiv's bonus uses the Silent's Blood at hit time, and the Blood Bend happens right after the Shiv resolves.
- Stormblood: Blood is gained per **hit** on a Vulnerable enemy (like Hyper Offense's per-hit crits); the bonus damage only applies to the partner's Attack cards.
- Mismatched Magic: once per Star spend (not per Star); with several stacks it converts up to that many times. No upgrade in the sheet → none (`//todo : upgrade not specified`).
- Bitter End: the "free" Attack is per enemy (1 per enemy, even when upgraded) and skips Attacks that already cost 0; Attacks are picked at random from the Draw Pile.
- Bloody Blitz: X-cost cards don't get the reduction and count as "not 0", so they're discarded.
- Legion of Death: counts unblocked attack damage only; it wears off at the end of the players' turn.
- Power Core can't be skipped (you pay the Curse either way). Friendship Bracelet keeps its skip.
- The new powers have no icons yet (default power icon); they're listed in `MISSING_IMAGES.tsv`. Power Core's outline is a copy of its icon for now.

## Team Blast: lock fix (Random Blast duplicates)

- [ ] With 2+ Random Blasts in the deck, fill the gauge: only **one** Random Blast copy in hand per turn, and never a second one while a copy is still in Hand/Draw/Discard (e.g. Retained).
- [ ] Play Random Blast: every other Team Blast (yours and the other players', in Hand, Draw and Discard) is **Exhausted** right away.
- [ ] Gauge orange (locked): no Team Blast shows up again for the rest of the combat, even through clones/gifts (Wall of Text, Reverse Beam, Random Portal…).

## 20. Slal: Change Pending C

- [ ] Build and launch. No scene or `.pck` changes in this pass, so no re-export is needed.

**Free Shapeshift removed / no more Colorless**
- [ ] No card, power or hover tip says "Free Shapeshift" or "Free Shapeshift+" any more. Goofball Token Powers are never made free by Slal's effects.
- [ ] **Emergency Shapeshift**: Exhaust up to 3 cards, then **Shapeshift** (upgraded: Shapeshift+) once per card. The Goofball powers are no longer free, and they cost 0 anyway.
- [ ] Slal never creates Colorless cards: Mimic, Plot Twist, Puzzling Tip, Genie Form wishes, etc. only offer character cards. Mood Shift no longer gives a power.
- [ ] Colorless cards still show up in Slal's shop like they do for everyone else.

**Memory / Creative Flow**
- [ ] **Memory** text: "At the start of your turn, gain X Flashback. When you gain 1 Memory, also gain 1 Flashback." plus the old Confusion line, which is kept. The mechanics are unchanged.
- [ ] **Creative Flow**: all cards (and created ones) gain Novel. At combat start, gain **1 Memory**, which also gives 1 Flashback right away. With turn 1's start-of-turn Flashback, that's 2 Flashback on turn 1. No more starting Random, and no Random on Novel plays.

**Cards**
- [ ] **Blooper**: also gain 1 Weak (hover tip present).
- [ ] **Proof Reading**: draw until you draw a Novel card (shuffles the Discard Pile in if needed; stops when the Hand is full or there are no cards left). Then gain 1 energy per Flashback and lose all Flashback. Base: Exhaust and **not** Always Novel. Upgraded: no Exhaust, gains Always Novel.
- [ ] **All Goofball / Creativity Unbound Forms cost 0**: Bumper, Ventilator, Squeaky Squirrel, Sword, Octopus, Box of Nails, Television, Sparrow, Genie, Walking Encyclopedia, Pinwheel.
- [ ] **Upgraded Forms double their stacks**: Squeaky Squirrel 2 times, Television 2 cards, Sparrow 2 cards, Genie 2 wishes (2 cards right away on play), Pinwheel 8 Confusion, Walking Encyclopedia 2 Random. Check the green upgrade preview.
- [ ] **Bumper Form**: upgrade is now "first **2** times" (it used to be -1 cost).
- [ ] **Ventilator Form**: "The first 1 (2) time(s) you play a Power each turn, ALL enemies lose 1 Strength this turn." The power counts down its triggers each turn and resets next turn.
- [ ] **Walking Encyclopedia Form** and **Pinwheel Form** are now Tokens: gone from Slal's card rewards and shops, and never random-generated. They come from Creativity Unbound and Flo's Brew.
- [ ] **Creativity Unbound**: Always Novel, no Exhaust. Adds 1 random power out of Genie, Pinwheel, Sparrow, Television and Walking Encyclopedia Form (all 5 hover tips). Not the same one twice in a row. Not made free (they're 0-cost).
- [ ] **Flo's Brew** can still offer the 5 Creativity Unbound Forms.
- [ ] **Distraction**: Shapeshift. If you're Weak, Shapeshift again (2 different Goofball powers). Upgraded: both are Shapeshift+. It glows gold while you're Weak.
- [ ] **Hat Tip**: whenever you play a Shapeshift/Mimic card, **Shapeshift** (once per stack).
- [ ] **Break Continuity**: whenever you play a Novel card, **Shapeshift** (once per stack), gain 1 Frail and 1 Disintegration.
- [ ] **Puzzling Tip**: gain 2 Weak, apply 5 Confusion (+Random), choose to Mimic 2 or Shapeshift. Upgraded: the offered cards are upgraded. Played by Flashback: always Shapeshifts. It now triggers Hat Tip.
- [ ] **Slime Rain**: +2 damage per Slimed created (it was +1). Upgraded: **10** base damage, still +2 per Slimed. Check the in-hand preview and that the bonus survives downgrade.
- [ ] **Mood Shift**: only "Put a Power from your Draw Pile into your Hand". Exhaust (upgraded: no Exhaust).

**Decisions I made**
- Creative Flow loses its Random parts (confirmed). Memory keeps its Confusion bonus (confirmed). Ventilator's base is "first 1 time" (confirmed). Slime Rain+ = 10 base damage (confirmed). Creativity Unbound loses Exhaust (confirmed). Emergency Shapeshift → plain Shapeshift (confirmed). Proof Reading reshuffles (confirmed).
- Squeaky Squirrel, Television, Sparrow, Genie, Pinwheel and Walking Encyclopedia Forms: the upgrade now doubles their stacks, replacing the old -1 cost (confirmed).
- Distraction checks Weak before the first Shapeshift.
- The Mood Shift **power** was deleted, along with its loc and its `MISSING_IMAGES.tsv` row. The card stays.
- Flo's Brew now uses the shared Creativity Unbound list, so Pinwheel and Walking Encyclopedia stay pickable after becoming Tokens.
- Page 2 of the sheet ("Ideas, ne pas implémenter encore") was not implemented.

## 21. Seels: Change Pending D

- [ ] Build and launch. No scene or `.pck` changes in this pass, so no re-export is needed.

**New status: Gloat X**
- [ ] **Gloat X**: if you take unblocked attack damage, gain X energy and draw X more cards next turn (Energy Next Turn + Draw Cards Next Turn). It triggers once and is then removed.
- [ ] Gloat that never triggers wears off when the enemy turn ends. Fully blocked hits, non-attack damage and self HP loss don't trigger it.
- [ ] The Gloat hover tip appears on Concede First Blow, Lure, Palm Strike, Play on Words and Smug Look.

**Cards**
- [ ] **Aggravating Demeanor**: whenever you apply Vulnerable (to an enemy), gain that much **Vigor next turn**, multiplied by the stacks: 2 copies = twice as much. Self-Vulnerable doesn't count. Upgraded: 0-cost.
- [ ] **Beast Form**: gain 6 (9) Strength. Still a Form Power, cost 3.
- [ ] **Belittle**: 7 damage ×3 (×4) to random enemies. After the attack, each enemy that lost HP loses 2 (3) Strength this turn, once per enemy even if hit several times. Vigor boosts the hits.
- [ ] **Big Target Sign**: X-cost, no longer multiplayer-only, hits ALL enemies. They lose 2X Strength this turn and you gain 4X Tainted (base game). Check the in-combat preview. Upgraded: Retain.
- [ ] **Concede First Blow**: Gloat 2 (3). Exhaust.
- [ ] **Final Showdown**: no more 10 Artifact. Apply 3 (5) Vulnerable, gain Strength + Demise equal to the enemy's Vulnerable, draw 3 (5).
- [ ] **Funny Dance**: now targets an enemy and applies X Vulnerable to it. Block X times now and next turn are unchanged.
- [ ] **High Reward**: hits 3 times if you lost HP this turn (glows gold). Check that HP loss from Blood/self-damage counts.
- [ ] **Illusion of Risk**: 0-cost. Gamble 2 (3), draw 2 (3), then return to your Hand costing 1 more this turn. Make sure the +1 survives the move back to the Hand and resets next turn.
- [ ] **Joke and Run**: 2-cost Power. Gain 1 (2) Dexterity, gain 2 energy, lose 3 Strength this turn.
- [ ] **Laugh It Off**: whenever you lose HP, add 1 (2) Slash(es) into your Hand.
- [ ] **Lure**: 2-cost (upgraded 1-cost). Gain 1 Intangible, Gloat 3. Exhaust. No more Retain upgrade.
- [ ] **Never Learn**: checks **Blood** > HP (glows gold when true). Upgraded: Retain.
- [ ] **Palm Strike**: deal 10, Gloat 1.
- [ ] **Play on Words**: 1-cost. Gloat 1 (2), apply 1 (2) Vulnerable to ALL enemies.
- [ ] **Questionable Strategy**: 2-cost. Fill your Hand with Wounds (preview shows how many), gain 20 (28) Block.
- [ ] **Savagery**: whenever you play an Attack, gain 2 Strength this turn. Upgraded: 0-cost.
- [ ] **Smug Look**: 1-cost (upgraded 0). Gloat 2, then this turn, every card played deals 2 to a random enemy. The "for 2 turns" upgrade is gone.
- [ ] **Strike First**: 0-cost (no longer X). Deal 5 (7). If the target intends to attack, gain 3 (4) Strength this turn. It glows gold if any enemy intends to attack.
- [ ] **Threatening Promise**: no more "repeat if you are Vulnerable". One Perfect Fit Slash (Slash+) per Vulnerable on the enemy.
- [ ] **Tornado Stance**: now 2-cost. The rest is unchanged.
- [ ] **Splash Armor**: multiplayer only (gone from solo rewards).
- [ ] **Untelegraph Attacks**: singleplayer only (gone from MP rewards).
- [ ] **Blood Bath**: also adds X Slashes into the Discard Pile. X is the Blood consumed (the upgrade's +1 only affects the Block).
- [ ] **Bloody Execution**: Blood Bend X, then add 2 Slashes (Slashes+) enchanted **Sharp X** to the Hand and 2 to the Draw Pile. If X = 0, the Slashes have no Sharp. It no longer deals damage and needs no target.
- [ ] **Reckless Charge**: Wounding. 9 (11) damage per Wound in your Hand, plus that many Slashes into the Discard Pile.
- [ ] **Pain Chock**: Wounding. 7 (9) damage to ALL, +3 (+4) per Wound in your Hand (the preview counts its own Wound). No more Replay upgrade.
- [ ] **Open Wounds**: Wounding, draw 1, **Replay 1 (2)**. Check that Replay shows in hand **and** in the card library/compendium, and survives save/load.
- [ ] **Break Continuity (Slal)**: the upgrade now gives Retain instead of Innate.

**Visuals**
- [ ] **Seels card frame** is now a deep, saturated gold (about `#d4ab17`; was `#c8a85a`, too gray, and before that `#f1c232`, too bright), and so are the small deck-list card icons. Check that it stands out against gray and isn't glaring. The shader numbers are estimated, so the in-game colour may be slightly off.
- [ ] **Gloat** power has no icon yet (added to `MISSING_IMAGES.tsv`). The new temporary Strength powers (Belittle, Big Target Sign, Joke and Run, Savagery, Strike First) use the wrapper's default display, like Humiliate.

**Decisions I made**
- Gloat = "if you take unblocked attack damage, gain X energy and draw X cards next turn" (confirmed). It triggers once, lasts through the enemy turn and then wears off.
- Aggravating Demeanor: stacks multiply the Vigor (confirmed). Following the Vigor rule (VigorNextTurn for hooks), the Vigor arrives as **Vigor next turn**, because Vulnerable can be applied mid-attack. Only Vulnerable applied to *other* creatures counts.
- Tainted = base-game TaintedPower (confirmed). Sharp = base-game Sharp enchantment (confirmed).
- Kept the current values the sheet doesn't mention: Joke and Run stays a Power, Final Showdown keeps 5 Vulnerable / 5 draw upgraded, Laugh It Off upgrade = 2 Slashes, Big Target Sign upgrade = Retain, Never Learn upgrade = Retain, Palm Strike upgrade = -1 cost, Threatening Promise upgrade = +1 Vulnerable / Slash+, Tornado Stance still upgrades the Slashes.
- Play on Words: the upgrade raises both Gloat and Vulnerable by 1, like the old version raised both of its values.
- Bloody Execution stays an Attack card (no type change in the sheet), but targets no one.
- Deleted the now-unused **Beast Form**, **Open Wounds** and **Illusion of Risk** powers, with their loc and their `MISSING_IMAGES.tsv` rows. The cards stay.
- Page 2 of the sheet ("Ideas, ne pas implémenter encore") was not implemented.

## Companions: Abby & Edge outside of combat

Shown for any player who owns the relic (Abby = Abby the Absol, Edge = Edge the Chao). Positions are constants at the top of `Visuals/CompanionVisuals.cs`; Edge's bobbing and proc time are exports in `scenes/companions/edge_chao.gd`.

**Edge**
- [ ] **Combat**: floats gently at Shion's top left, first Sonic Chao sprite (with his ponpon).
- [ ] **Edge the Chao procs** (Vigor spent): switches to the fourth sprite with a "!" in place of his ponpon for about 1.2 s, then goes back. Several procs in a row keep the "!" up.
- [ ] Edge disappears when Shion dies.
- [ ] **Merchant**: second sprite, flipped (looking away from Shion), "?" in place of the ponpon. The "?" itself must not be mirrored.
- [ ] **Rest site**: last sprite, heart in place of the ponpon, a bit left of and lower than his combat spot. Check he actually sits on the log, in all 3 acts.
- [ ] Edge still floats up and down at the merchant and the rest site.
- [ ] Pixel art stays crisp (no blur).

**Abby**
- [ ] **Merchant**: just in front of Shion, drawn over her legs.
- [ ] **Rest site**: lying by Shion's side, drawn over her lower body.
- [ ] **Multiplayer**: companions follow their owner, including the darker back row at the merchant and the mirrored rest site spots (2nd and 4th players).

## 22. Seels: Epochs

**Setup**
- [ ] Open the project in MegaDot so it imports the 16 new images in `images/timeline/seels/` (8 portraits + 8 `_packed` thumbnails), then re-export the pck.
- [ ] On the first main menu load, the log line `[Epochs] Validated N epochs: 0 problem(s)` shows no error (missing keys, art, or two epochs on the same slot).

**Unlocks** (each epoch appears on the timeline once Seels is unlocked)
- [ ] **Finally!**: play a run with Slal. Unlocks Seels (same epoch as before, so saves that already had Seels keep him). Column of Grand Strategy.
- [ ] **Bo-riiiing!**: beat Act 1 with Seels. Column of Shion's New Challenges.
- [ ] **Hold On...**: defeat 15 Elites with Seels. Column of Darv.
- [ ] **Wait For Me, Guys!**: beat Act 2 with Seels. Column of Life and Death.
- [ ] **Well, This Blows.**: beat Act 3 with Seels. Column of The Daily (7th row, the column scrolls).
- [ ] **Ragtag Pals**: defeat 15 Bosses with Seels. Column of Shion's If One Doesn't Cut it...
- [ ] **Condensed Might**: win a run as Seels holding the Red Chaos Emerald (solo or combo). Same column as Ragtag Pals.
- [ ] **Roll With The Punches**: win a run on Ascension 1 with Seels. A new column after If One Doesn't Cut it... (the base game's empty "The End" era, year 3,000).

**Visuals**
- [ ] The timeline thumbnails are center crops of the art. Check that none cuts off the important part (I made them automatically).
- [ ] Text effects: `(shake)` became `[shake]`, like the combat messages. Check "UNACCEPTABLY BORING" and "I love it." shake, and the jitter words jitter.
- [ ] The new "The End" column shows its icon, name and year, and the timeline line reaches it.

**Decisions I made**
- No epoch unlocks anything yet. Each one has a placeholder `unlockText` "TODO" in `epochs.json`. When you add Cards/Relics/Potions (3 of one kind) to an epoch, delete its `unlockText` line.
- Kept the class `SeelsUnlockEpoch` for Finally! (ID unchanged) so it doesn't re-lock Seels for testers.
- Chapter order follows the sheet's row order (Chapter # was empty).
- "(purple and yellow)we" is written as a purple "w" and a gold "e". "(azure)" uses `[aqua]`.
- Condensed Might = win as Seels with the Red Emerald (confirmed). Red is shared with the Ironclad, but only Seels' runs count.

## 23. Change Pending E (Seels, Spirit Burst, Cosmo cards, Frontflip and a Half, Blood)

- [ ] Build and launch. No scene, image or `.pck` changes in this pass, so no re-export is needed.

**Wounds**
- [ ] **Eye Opening Realization**: Wounding. ALL your Wounds (in every pile, at the time you play it) lose Unplayable and cost **1**. Wounds in your Hand get Swift 3 (4). A Wound created later is still Unplayable.
- [ ] **Jolting Pain**: same, with Adroit 7 (9).
- [ ] **Burning Pain**: same, but the Wounds cost **0**, and the ones in your Hand get Sown.
- [ ] Playing one of these after another sets the Wound cost again (Burning Pain then Jolting Pain = Wounds cost 1).
- [ ] Echoes of Pain copies keep the cost of the Wound they copy.

**Seels cards**
- [ ] **Big Numbers**: 4-cost (3). At the start of each turn, gain 2 energy (on top of your normal energy, not wiped by the reset). Erratic max cost +1. Two copies = 4 energy and +2. No more Gamble.
- [ ] **Funny Dance**: X-cost. Apply X Vulnerable, Gloat X. Exhaust. Upgraded: no Exhaust. Check the in-combat preview.
- [ ] **Body Strike**: Wounding (a Wound in your Hand first). 14 damage. If the enemy lost HP, 1 more Wound in your Discard Pile (just 1, no longer 1 per HP).
- [ ] **Open Wounds**: X-cost. Gain X energy, draw X (X+1) cards, add X Wounds to your Discard Pile. No more Wounding or Replay. Check the preview.
- [ ] **Bloody Execution**: is now a Skill.
- [ ] **Parry Attempt**: one Block gain of 6 + 4 × this card's cost (7 + 5×). Gain Blur equal to the cost. Check with Erratic costs (0-cost = 6 Block, no Blur).
- [ ] **Wink**: also applies Weak equal to this card's cost to ALL enemies (0 at base cost, so it needs Erratic).
- [ ] **Exsanguination**: 1-cost. Blood Bend X, deal X + X × Attacks played this turn (itself included) × this card's cost. Exhaust. Upgraded: no Exhaust. Example: 5 Blood, 2nd Attack of the turn, cost 1 = 5 + 5×2×1 = 15. Check the preview shows the Attack count and the damage.
- [ ] **Aggravating Demeanor**: when your Gloat triggers (you take unblocked attack damage), immediately gain Blur equal to the Gloat energy and 5 (8) Block. Two copies = 10 Block, same Blur. No more Vulnerable → Vigor. Upgrade is now +3 Block, not -1 cost.
- [ ] **Joke and Run**: Gloat 2. Gain 1 (2) Dexterity and 1 Blur. No more energy or Strength loss.
- [ ] **Gambler's Luck**: draws during your turn give 3 (5) × the card's cost as Block. The starting Hand draw doesn't count anymore. No more Draw on play.
- [ ] **Laugh it Off**: at the end of your turn, Gloat 1 (2). It adds to Gloat played during the turn.
- [ ] **Card Dealing**: the damage now happens first, then the draw and Gamble.
- [ ] **Hematoma**: discards ALL Status cards in your Hand (not just Wounds) and draws as many.
- [ ] **Live With The Pain**: 2-cost (1). Counts ALL your Wounds, Exhausted ones included, upgraded or not.
- [ ] **Summon Wrath**: 0-cost (no longer X). Lose HP equal to its cost, add 2 Slashes (Slashes+) to your Hand, then it costs 1 more this combat and is shuffled into your Draw Pile (not discarded). Play it several times in one combat: 0, 1, 2… HP.
- [ ] **Big Target Sign**: multiplayer-only again. It must not show up in single player rewards.
- [ ] **Sweat and Tears**: every 4 Attacks you play, gain 1 (2) Blood. Each copy is a separate power icon with its own countdown, like Orbit. Replays don't count.
- [ ] **Echoes of Pain**: 1-cost (0).
- [ ] **Massive Defend**: also gain 1 Blur.
- [ ] **Shed The Pain**: 2-cost (1). Exhausts ALL your Wounds (enchanted or not). Heals 1 HP per *enchanted* Wound Exhausted. Check the preview.

**Other characters**
- [ ] **Spirit Burst**: no more auto-play. While active, your Strikes and Energy cards cost 0, and every Energy card you play draws 1 card. The countdown and the No Block / No Heal at the end still work.
- [ ] Spirit Burst visuals: Strikes and Energy cards played during it still show the lightning and the Spirit Burst strike effect (it used to be on auto-played cards). Check it looks fine on played cards.
- [ ] **Frontflip and a Half** (Slal): is now a Skill.
- [ ] **Cosmo's Kindness / Determination / Guidance**: all three are Skills (Guidance was a Power) and Exhaust.

**Blood**
- [ ] **Unspent Blood carries over**: win a combat with some Blood, and you get it back at the start of your first turn of the next combat.
- [ ] Spent Blood (Blood Bend) doesn't carry over. 0 Blood at the end = 0 next combat.
- [ ] Save and quit between two combats, then continue: the Blood still comes back.
- [ ] **Red Fang**: the Blood you get back doesn't count again for its counter.
- [ ] **Multiplayer**: each player gets their own Blood back. No desync on the first turn.

**Decisions I made**
- Thorn Bracers already does "gain 1 Blood after you consumed your Blood" when you play a Blood Bend card. No change.
- "Make ALL your Wounds playable" = every Wound you have when you play the card, in every pile. Wounds created later stay Unplayable (confirmed).
- Exsanguination counts itself as an Attack played this turn (confirmed). Only the first play of a series counts (not replays), like Pinball Move.
- Gambler's Luck no longer draws on play (confirmed).
- Blood carry-over is on (confirmed). It's saved when you win, so it doesn't apply to losing a fight.
- Kept the current values the sheet doesn't mention: Body Strike upgrade = -1 cost, Laugh it Off upgrade = Gloat +1, Joke and Run upgrade = +1 Dexterity, Sweat and Tears upgrade = +1 Blood, Bloody Execution / Frontflip keep their costs and upgrades.
- Big Numbers: each copy gives 2 energy per turn.
- Aggravating Demeanor: the Block stacks, but the Blur is always the Gloat energy (not multiplied by copies).
- Card Dealing counts the enemies before the damage, so an enemy it kills still gives you a card.
- Summon Wrath is shuffled into a random spot of the Draw Pile.
- Parry Attempt gives the 6 Block and the extra Block as one Block gain, so Dexterity only applies once.
- Spirit Burst: X-cost Strikes or Energy cards aren't made free (that would break their X). Energy cards draw on every play, replays included.
- Deleted the unused **Live With The Pain+** and **Joke and Run** Strength-down powers, with their loc. Also deleted Spirit Burst's "tired" message (it was only for the auto-play cap).
- Page 2 "Ideas (ne pas implémenter encore)" was not implemented.

## Random Blast: flying icons

- [ ] While Random Blast is active, every card played (by anyone) throws a random buff/debuff icon out of Slal, spinning, towards the enemies (between straight ahead and ~35° up), until it leaves the screen.
- [ ] The Random Blast play itself doesn't throw one (same as the Confusion proc).
- [ ] Icons vary (any power of the game, including modded ones), and each player sees their own random icons (visual only, no desync).

## Team Blast intro

- [ ] Playing any Team Blast (vanilla, Colorless, Shion/Seels/Fry/Slal) first makes a gentle light burst out of every living player of the team, expanding and fading out.
- [ ] Light colors: blue for Silent / Defect / Slal, red for Ironclad / Fry / Shion, yellow for Regent / Seels / Necrobinder (other characters: soft white).
- [ ] 0.5s later the Dramatic Entrance full-screen effect plays, right as the lights finish fading, then the Team Blast resolves.
- [ ] Only plays once per Team Blast (not on replays), on every client, and is skipped with Instant fast mode.

## 24. Seels: Ancients & party interactions (+ Shion ↔ Seels)

**Setup**
- [ ] Re-export the .pck (ancients.json changed) and rebuild. Let MegaDot generate the `.uid` for the new `SeelsPartyDialogues.cs`.
- [ ] Note: the build currently fails on `Variables/PatchUpVar.cs(43)` (`DynamicVar._owner` is protected). The error was already on HEAD before this pass.

**Seels with ancients** (one-time dialogues show on Seels' 1st, 2nd, 5th, 8th, 11th… visit, like Shion's; repeating ones join the random pool once that visit is reached)
- [ ] Neow: first visit "come closer…" → "Wow, big fish!"; repeatables "rise… annoying mage", "Big oof…", "This time, for sure!"; one-time "It happened twice..." and "i kept the blood you collected…".
- [ ] Orobas: "Blonde thing!!" intro; repeatables (blood spells, "Ooh, the shinies!", marvelling narration).
- [ ] Pael: hemomancer intro with the mini-Pael narration; repeatables (dragon blood).
- [ ] Tezcatara: "chip off the old block" intro; repeatable "crimson lake"; one-time pyromancy line.
- [ ] Nonupeipe: "What is this stench?" intro; repeatables.
- [ ] Tanx: "SMELL OF A TRUE WARRIOR" intro; repeatables, including the playfight.
- [ ] Vakuu: "Imagine seeing someone already familiar…" intro; repeatable "pathetic"; one-time "I'm done taking your contracts" speech (7 lines, last one wavy).
- [ ] Darv: "[purple]punk[/purple]" intro; repeatables (steal from the Architect, "nice junk").
- [ ] Line speakers look right: narration lines sit on the side of whoever the sheet says (Tezcatara's cackle on Tezcatara's side, Seels' narrations on Seels').

**Seels party dialogues at Neow (multiplayer)**
- [ ] Ironclad, Silent, Defect, Regent, Necrobinder: the first one-time dialogue plays first, then the repeatables join. Necrobinder's 2nd one-time dialogue ("Always amazing how friendship…") only shows once the first one has been seen.
- [ ] Silent's "bonks Seels' head" and Defect's "Loading 'Dreamer Manifest'…" lines show on the partner's side, with the partner's icon and colour.
- [ ] STS1 (Ironclad + Silent + Defect): "Hell, yeah! NOW, it's a party!"
- [ ] STS2 (Regent + Necrobinder): Regent opens, Necrobinder says "Hard pass.", and the right speaker shows on every line.
- [ ] Any Dreamer (Slal / Fry / Shion): the 3 repeatable lines.
- [ ] Seels + Seels: the player/partner lines alternate correctly.
- [ ] Seels + Shion: Shion opens ("Oh, damn!…"), then the postcard shout shakes.

**Shion party dialogues with Seels (new group)**
- [ ] Shion + Seels: Seels opens with "Hey, girl!"; repeatables "Race ya to the top!", "good company… folly", "insane bloodlust", "came home more often", "[gold]he[/gold] nags me".
- [ ] The second one-time dialogue ("Tsk.... this is gonna take some time") only shows after the first one has been seen, and ends on Shion's italic "(side-eye)".
- [ ] Shion's old Dreamers group still shows with Seels too (both groups are in the random pool).

**Decisions I made**
- Default visit spacing for one-time ancient dialogues (confirmed). "It happened twice..." shows on Seels' 8th Neow visit.
- Button labels the sheet doesn't give are contextual (confirmed): Approach, Rejoice, Reconsider, Answer, Show off, Greet, Playfight, Stay silent, Listen. Everything else is "Continue". Party dialogues use the existing "Continue".
- Tag mapping: (wavey) → [sine], (shake) → [jitter], (italic) → [i], (purple)/(red) → same colour, (yellow) → [gold] (there's no yellow tag). Narration uses the same small italic style as Shion's. All of Pael's lines use [thinky_dots], like his other lines.
- Typo fixes: "trinks" → "trinkets", "vigoriously" → "vigorously", "TRANSDIMENSONAL" → "TRANSDIMENSIONAL", "welp" → "whelp", and added a period to Nonupeipe's "…reward might. Come.".
- The sheet has no Architect lines and no "other modded character" group for Seels, so the Architect TODO placeholders stay and nothing was added for modded characters.

## 25. Change Pending F (Seels rework, Maim, Slal bits)

**Setup**
- [ ] Rebuild, re-export the .pck (new/changed card portraits) and let MegaDot generate the `.uid` files for: `SeelsMutilate.cs`, `MaimHelper.cs`, `MaimDescriptionPatch.cs`, `MaimSingleton.cs`, `BurningPain.cs`, `EchoesOfPain.cs`, `ScarletShadow.cs`, `NeverLearnStrength.cs`, plus the `.import` for `seels_mutilate.png`.

**Maim X (new keyword)**
- [ ] Cards with Maim show "Maim X." at the top of their text, under Wounding/Retain lines, with a hover tip.
- [ ] Every hit that makes an enemy lose HP: that enemy loses X Strength this turn (shown as a "Maim" debuff) and you gain X Blood. Belittle 3 hits on enemies with no Block = 3 triggers.
- [ ] Blocked hits (no HP lost) don't trigger it. A hit that kills still gives the Blood.
- [ ] Non-attack HP loss counts: Steal Blood and Big Target Sign ("ALL enemies lose 2 HP") trigger Maim per enemy.
- [ ] Maim added to a card (Go Crazy, Steal Blood) shows in green and stacks with the card's own Maim. Copies of that card keep it.

**Blood Bend on replays**
- [ ] A replayed Blood Bend card (Echoes of Pain, Tornado Stance's Replay, etc.) reuses the X of its first play, doesn't consume Blood gained in between, heals X again and triggers Burning Pain / Splash Armor / Recall Blood / Thorn Bracers again.

**Seels cards**
- [ ] **Mutilate** (new, Basic, 3 cost): Maim 2, deal 27 damage, Gloat 1. Upgraded: 33 damage, Gloat 2. It's in the starter deck (10 cards now).
- [ ] **Scarlet Shadow**: after playing it, at the end of combat you Blood Bend (heal), so that Blood isn't carried over to the next fight.
- [ ] **Exsanguination**: Blood Bend X, deal 8 + 5X damage, doubled against a Vulnerable enemy. The aimed damage preview shows the doubling. Exhaust (upgrade: no Exhaust).
- [ ] **Bloody Execution**: now an Attack. Maim 2, 12 damage + 4 per Vulnerable on the target (upgrade 15 / 5). The aimed preview includes the bonus.
- [ ] **Funny Dance**: 2X Vulnerable to ALL enemies, Gloat X.
- [ ] **Body Strike**: Maim 1 added.
- [ ] **Summon Wrath**: lose 2 HP, put 2 random Attacks from Draw Pile into Hand (upgraded: upgrades them), HP loss +1 each play this combat. It now goes to the Discard Pile and its cost stays 0.
- [ ] **Ultra Kill**, **Strike First**: Maim 3 / Maim 1 added.
- [ ] **Burning Pain**: now a 2-cost Power. Blood Bending with 2+ Blood gives 2 (3) Strength.
- [ ] **Echoes of Pain**: the next card with Maim is played one extra time (High Reward counts even without the HP-loss condition). Exhaust. 1 cost (0 upgraded).
- [ ] **Go Crazy**: adds Maim 1 to ALL your Slashes (not the Exhausted ones).
- [ ] **Never Learn**: Max HP check first, then Blood Bend X, gain X Strength this turn (with preview).
- [ ] **Scratch**: 1 damage, +2 per play (upgraded 2 / +3).
- [ ] **Steal Blood**: Maim 2, enemy loses 2 HP; if it did, choose an Attack in Hand to get Maim 1 + Upgrade (upgraded: ALL Attacks in Hand, no choice).
- [ ] **High Reward**: if you lost HP this turn, hits 3 times with Maim 3. Otherwise no Maim.
- [ ] **Big Target Sign**: Maim 2, ALL enemies lose 2 HP (also at X = 0), gain 4X Tainted.
- [ ] **Copy Kitty**: 2 cost (1 upgraded). Plays a copy of every card the ally played this turn (in order, replays not counted), and the copies are Exhausted.
- [ ] **Toss A Coin** (multiplayer): the targeted ally gets their own selection screen, picks any number of cards to Gamble (0 allowed), then draws 1. Upgrade: Retain. **Check for desyncs / soft-locks here: it's another player choosing during Seels' card.**
- [ ] **Brutal Teachings**: now an Attack. Maim 2, 20 damage, +10 damage each play this combat, Gloat 2.
- [ ] **Tornado Stance**: now Ancient (new tall portrait), Retain. Adds Replay to Slashes + Vulnerable per Slash. At the start of each turn while in Hand, adds 1 (2) Slash into the Discard Pile.
- [ ] **Splash Armor**: X Block twice + 1 (2) Blur to ALL players.
- [ ] **Savagery**: upgrade is now 3 Strength instead of 0 cost.
- [ ] **Show Off**: also triggers when a Wound is Exhausted.
- [ ] **Weird Flex**: whenever you play a Wound, gain 1 (2) Strength and 1 (2) Dexterity (permanent).
- [ ] **Invigorating Pain**: played Wounds are Exhausted, give 3 (5) Vigor and draw 1.
- [ ] **Hematoma**: plays ALL Status cards in Hand (Wounds/Burns included, even Unplayable ones), then draws that many. The Wounds go back to being Unplayable afterwards.
- [ ] **Combo Meter**: now a 1-cost Ethereal Skill (0 upgraded). Stacks the Combo Meter power, then goes back into the Draw Pile.
- [ ] **Shed The Pain**: Blood (instead of Heal) per enchanted Wound, then Gloat 2.
- [ ] **Blood Punch**: Maim 1, +5 damage per play, goes into the Draw Pile (on top when upgraded).
- [ ] **Blood Cage**: now a Skill. Blood Bend X, 4 Block X+1 times, X (X+1) Vulnerable.
- [ ] **Blood Prison**: now a Skill that still deals 14 (20) normal damage to ALL enemies + X (X+1) Vulnerable (twice upgraded).
- [ ] **Blood Bath**: 6 Block X (X+1) times + 2 Blur, no more Slashes.
- [ ] **Gambling Addiction**: now 0-cost Rare (normal portrait). Ethereal, gain 1 energy, lose 3 (1) HP. Every Gamble pulls it from Draw/Discard into your Hand (never from the Exhaust Pile). When it's Exhausted (Ethereal included), deals 5 x Gambles this combat to ALL enemies.
- [ ] **Belittle**: Maim 2 (3), 7 damage x3 (x4), no more Strength part.
- [ ] **Recall Blood**: 2 cost (1 upgraded).
- [ ] **Pin Them Down**: Gloat 2, 1 Inky Slash into Hand + 2 per energy cost into Draw Pile (Slash+ when upgraded). Try it with an Erratic cost.
- [ ] **Spectacular Break-In**: Wounding, 1 Glass 2 Slash (Slash+) into Hand per Wound in Hand (counts the new Wound).

**Other**
- [ ] **Sarcophagus Form** (Slal): Confusion you apply to a Weak enemy gets +6 (+9). It shows in card previews and can't loop.
- [ ] **Slime Potion**: Transform any number of Hand cards into Slimed (0 allowed), gain 6 Free Status.
- [ ] **Thorn Bracelet**: after a card that Blood Bent finishes resolving, lose 1 HP (which gives 1 Blood back).
- [ ] **Fresh Lemonade**: Gamble any number, draw that many, heal per energy cost drawn.
- [ ] **Megaphone**: every Gloat applied to you (cards, Laugh It Off...) applies 1 Vulnerable to ALL enemies.
- [ ] **Lottery Ticket**: every play of an Erratic card gives 1 energy (no more start-of-combat choice).

**Thorn Bracelet / Thorn Bracers swap**
- [ ] Seels starts with Thorn Bracelet: lose HP → 2 Blood, each Blood Bend card → +1 Blood after consuming.
- [ ] Upgrading it (Touch of Orobas) gives Thorn Bracers: lose 1 HP after each Blood Bend card resolves, lose HP → 1 Blood.

**Visuals**
- [ ] Mutilate portrait (Seras punch crop) looks right, small + big.
- [ ] Tornado Stance tall Ancient crop (face centered), Gambling Addiction recropped to the normal format.
- [ ] New powers without icons (Burning Pain, Echoes of Pain, Scarlet Shadow) added to MISSING_IMAGES.tsv. Removed rows for the deleted Brutal Teachings / Gambling Addiction / Steal Blood powers.

**Decisions I made**
- Maim triggers once per hit (confirmed). Blood Bend replays reuse X and count as full Blood Bends (confirmed). Page 3 skipped (confirmed). Gambling Addiction upgrade = lose 1 HP (confirmed). Mutilate added to the starter deck (confirmed). Blood Prison deals normal damage as a Skill (confirmed). Summon Wrath drops the cost increase and the return into the Draw Pile (confirmed). Toss A Coin: the ally chooses (confirmed).
- The "Maim X." line is always at the top of the card. On Blood Punch it shows before "Lose 1 HP" (the sheet has it after).
- High Reward always shows the Maim hover tip, but its Maim only applies while it's played with the HP-loss condition.
- Kept the current values the sheet doesn't give: Echoes of Pain keeps Exhaust; Go Crazy keeps 2 cost and has no upgrade (flagged `//todo : upgrade not specified`); Bloody Execution / Brutal Teachings keep their costs; Blood Cage keeps the X+1 Vulnerable upgrade; Splash Armor/Combo Meter keep their stacking powers.
- Scarlet Shadow's end-of-combat Blood Bend happens before Blood carry-over is saved, so that Blood is healed, not carried.
- Exsanguination's x2 and Bloody Execution's Vulnerable bonus are damage modifiers, so they show in the aimed preview and stack with Vulnerable's own 50%.
- Hematoma lifts Unplayable only for the forced play. Invigorating Pain sends played Wounds to the Exhaust Pile.
- Thorn Bracers (upgraded Thorn Bracelet) doesn't get the new HP loss.
- Lottery Ticket and Thorn Bracelet trigger on every play (replays included), like the Spirit Burst decision.
- Sarcophagus Form is an additive bonus to Confusion applied (no second application, so no infinite loop).
- Deleted the now-unused powers with their loc: Brutal Teachings, Brutal Teachings Dexterity, Gambling Addiction, Steal Blood, Big Target Sign Strength Down, Weird Flex Strength. Belittle Strength Down was renamed to Maim Strength Down.
- Removed `GambleRandomly` (Toss A Coin was its only user) and added `GambleAnyNumber`.

## Character naming: "Name, the Class"

- [ ] Character select shows Shion, the Bruiser / Slal, the Goofball / Seels, the Reckless / Fry, the Commander.
- [ ] Team-Up lines read "Team-Up: Bruiser / Goofball / Reckless / Commander".
- [ ] Colorful Philosophers offers "Bruiser / Goofball / Reckless / Commander cards" (unchanged).
- [ ] Orobas's Sea Glass for one of our characters says "See 15 cards from The Bruiser" (etc.), and still uses its custom title (Tough Glass...).
- [ ] Custom-run card modifiers are "Bruiser Cards", "Goofball Cards", "Reckless Cards", "Commander Cards".
- [ ] Timeline story columns are titled Bruiser / Goofball / Reckless / Commander. Epoch conditions say "with the Bruiser" etc.; unlock pop-ups use the full name.
- [ ] Shion Berserk Form says "random Bruiser and Colorless Attacks".
- [ ] Run history death text and singleplayer power owner names show the full name ("Shion, the Bruiser was consumed by...").
- [ ] Ancient and other dialogue text is unchanged.

**Decisions I made**
- The run history can't show only the first name without code: the game uses the same `title` key for character select and run history. It shows the full name.
- Sea Glass names a character from `title`, so a small patch (`SeaGlassClassNamePatch`) swaps in `SEA_GLASS.(Character).character` from relics.json ("The Bruiser", matching the base game's "The Ironclad").
- Epoch conditions follow the base game's "with the [red]Ironclad[/red]" style. Slal's epoch name color changed from gold to aqua to match his other text.

## Mutilate trashed

- [ ] Seels starts with 9 cards: 4 Strike, 4 Defend, Blood Cage. No Mutilate.
- [ ] Mutilate never shows up in card rewards, shops or random card generation.
- [ ] Maim still works on every other Maim card and its debuff is still titled "Maim" (the power keeps Mutilate as its internal source card).

**Decisions I made**
- Trashed, not deleted (confirmed): class, loc and portraits are kept, moved to the Token pool like Anchor Throw.
- Starter deck goes down to 9 cards, nothing replaces it (confirmed).

## 26. Change Pending G (Seels numbers & Gloat pass, Slal bits)

**Setup**
- [ ] Rebuild, re-export the .pck and let MegaDot regenerate the `.import` files for the renamed `seels_running_jokester.png` (normal + `big/`). `SeelsRunningJokester.cs.uid` keeps the old uid.

**Seels**
- [ ] Gambling Addiction and Belittle already matched the sheet, so nothing changed.
- [ ] Exsanguination: 1 cost. Blood Bend X. Deal 4 + 3X damage, doubled against Vulnerable. Exhaust (upgrade removes Exhaust). The in-combat preview matches.
- [ ] Blood Cage: Skill. Blood Bend X. Gain 4 Block **once** (no longer X+1 times). Apply X Vulnerable (X+1 upgraded). The preview only shows the Vulnerable amount now.
- [ ] Never Learn: 3 cost. Gains 4 Max HP if Blood > HP, then Blood Bend X and gain X **permanent** Strength (it stays at end of turn). Exhaust, Retain when upgraded.
- [ ] Bloody Execution: 3 cost. Maim 2. 20 damage + 3 per Vulnerable (upgraded: 25 + 4).
- [ ] Body Strike: Wounding. 14 damage. Gloat 1. No Maim and no extra Wound in the Discard Pile any more. Upgrade still -1 cost.
- [ ] Steal Blood: Maim 2. The enemy loses 2 HP, then **always** (even if blocked or the HP loss is prevented) a random Attack in Hand gets Maim 1 and is upgraded, with no selection screen. Upgraded: ALL Attacks in Hand.
- [ ] Running Joke is now "Running Jokester" (class/ID renamed, portrait still shows). Draws until a non-Attack, applies that much Vulnerable, then Gloat 1. Slal's Running Joke is unchanged.
- [ ] Spectacular Break-in: Wounding. Adds 1 Glass 2 Slash (Slash+) to Hand, then 1 more **on top of the Draw Pile** for each Wound in Hand. The preview shows the Draw Pile count.
- [ ] Improvised Acupuncture: starts at lose 2 HP / draw 2 (upgraded: lose 1 HP / draw 2). Still +1 HP loss each play this combat.
- [ ] Blood Mixing: 2 cost. Per enemy: gain 1 Max HP, then lose 1 HP. ALL enemies lose 1 HP and 1 Strength. Exhaust, Innate when upgraded. The preview shows the Max HP and HP totals.
- [ ] Scratch: 8 damage, all Scratches +1 (upgraded: 11, +2). Copies still go into the Draw Pile, and buffs survive downgrade.
- [ ] Concede First Blow: 1 cost. Innate. Gloat 3. Exhaust. Upgrade adds Retain (no longer +1 Gloat).
- [ ] Smug Look and Pin Them Down: Gloat 1.
- [ ] Brutal Teachings: no Gloat any more (text, hover tip and effect).

**Slal**
- [ ] Pinball Move: Slal plays her attack animation, flies spinning to the top of the screen, then for each hit she bounces off a screen edge (not before the first hit) and slams into that hit's random enemy (blunt VFX + small shake). She flies back home at the end. Check with 0 hits (nothing happens), 1 hit, many hits, a target dying mid-sequence, and fast mode.
- [ ] Slime Form: 2 cost (1 upgraded).

**Decisions I made**
- Never Learn's Strength is permanent (confirmed). The now-unused `NeverLearnStrength` power and its loc are deleted.
- Steal Blood: no "if it did" condition, and the upgrade still hits ALL Attacks (confirmed). Picks use `CombatCardSelection` RNG. The unused selection prompt loc is removed.
- Improvised Acupuncture upgrade = starts at 1 HP loss **and** keeps the +1 draw (confirmed).
- Pinball Move: Slal is the ball (confirmed). Each hit's target is read through a `WithHitVfxNode` callback that adds no node.
- "Joke" > "Jokester" read as renaming Seels' card to "Running Jokester", so it no longer shares Slal's card name. Gloat 1 goes at the end of the text.
- Kept the current values the sheet doesn't give: Blood Cage keeps the X+1 Vulnerable upgrade, Body Strike keeps -1 cost, Spectacular Break-in upgrades its Slashes, Scratch keeps 0 cost, Blood Mixing keeps Innate on upgrade.
- The "Ideas (ne pas implémenter encore)" rows were skipped.

## Character select transitions

Start a run with each character (normal speed, not Instant fast mode) and watch the wipe into the run. Also check multiplayer, Custom Run and Daily, which use the same transition.
- [ ] Shion: punch impact on the left (flash, shockwave, impact frame, cracks, shake), then on the right, a short beat, then three dragon claw gashes sweep right → left and rip open until the screen is black.
- [ ] Slal: azure blue slime pours from the top with round drips, drags the screen down, and leaves darkness behind it.
- [ ] Seels: 12 slashes from all directions, each with a quick white blade streak, lay black strokes over the screen (later ones wider). Each slash bleeds 3 black droplets that run down, until everything is covered.
- [ ] Fry: a blurry hand shadow sharpens as it lands, grabs and shoves the screen back, crumples it into its fist, then pulls away downward.
- [ ] Each one ends fully black, with no flash of the old screen before the run fades in.
- [ ] The transition shows the character select screen as you left it (UI, panels, character art), not just the blurred backdrop behind it.
- [ ] Sounds: Shion plays Punch Construct's double punch, Slal plays Slimed Berserker's slime, Seels plays the Silent wipe, Fry plays Osty's attack. Check that each one fits the 0.8s transition and isn't too loud next to the music.
- [ ] A vanilla character's transition still looks and sounds the same.

**Decisions I made**
- Hooked up through BaseLib's `CustomCharacterSelectTransitionPath`, with one shader material per character in `ShionTheBruiser/materials/transitions/`. The effects are procedural shaders (`ShionTheBruiser/shaders/transitions/`), so they need no art.
- The game plays the transition in a fixed 0.8s and fades its own black overlay in on top (quadratic ease-in), so the effects are front-loaded. Phase timings are shader uniforms if they need retuning.
- Godot copies the screen only once per frame, for the first shader that reads it, and the menus' blurred backdrops read it first. `Patches/CharacterTransitionScreenCapturePatch.cs` adds a `BackBufferCopy` right before the transition rect, visible only during our fade-outs, so the transition captures the full current screen.
- Sounds: vanilla only has `wipe_ironclad` and `wipe_silent` (Defect, Necrobinder and Regent all reuse Ironclad's), so the old placeholder `wipe_defect` and `wipe_regent` played nothing. Seels gets Silent's. The others use enemy/companion sounds that match their effect better: Punch Construct double punch (Shion), Slimed Berserker slime (Slal), Osty attack (Fry).

## Character select skin

Open the character select screen (singleplayer, then multiplayer) and click through every character, then do it again with a gamepad.
- [ ] Shion, Slal, Seels and Fry: the info panel sits on a plain semi-transparent dark backing with rounded corners. The name is white with a black outline (no checkered plate), and "THE BRUISER" / "THE GOOFBALL" / "THE RECKLESS" / "THE COMMANDER" sits above it in gold on a dark chip. It's easy to read.
- [ ] The name is just the first name ("Shion", not "Shion, the Bruiser"). Check it also reads right elsewhere: compendium, run history, multiplayer lobby, Sea Glass (still "The Bruiser").
- [ ] HP and Gold sit in a column at the right of the name (HP on top), on black chips with a gold line, centred on the name area. They don't overlap the name or the class name, and there's a small gap before the description.
- [ ] The starting relic icon has no badge behind it and is vertically centred on its name + description, with faint gold lines above and below its row.
- [ ] Shion shows 6 playstyle coins (Berserk, Galvanised, Defensive, Dragon, Energy, Critical) and Slal 4 (Slimy, Memory, Weak, Forms), with the right icons, in rows of 3. Seels and Fry show none yet.
- [ ] Under the relic row, the backing narrows to the width of the coin grid, and neither the coins nor the backing reach the ascension selector or its text.
- [ ] Hovering a coin highlights it and shows a tooltip above the coin grid (gold title, thin line, then the description). It goes away when the mouse leaves, and never covers the ascension selector or the character buttons.
- [ ] The frame fits the panel for every character (long descriptions and the coin row included) and doesn't cover the character buttons at the bottom.
- [ ] Gamepad: up from any character button goes to the first coin of the bottom row, up/down moves between rows, left/right runs through the coins with the tooltip following, down from the bottom row goes back to the selected character. Pressing confirm on a coin embarks normally and doesn't leave focus stuck.
- [ ] After pressing Embark in multiplayer (waiting for others), the coins can't be focused or hovered. Unready brings them back.
- [ ] A locked character keeps the frame and plate but shows no class name and no coins.
- [ ] Switching to a vanilla character or Random gives back the vanilla panel exactly (gold name, fuzzy dark backdrop, HP/Gold row, relic icon position). Back out and re-enter the screen: still clean.
- [ ] The character buttons and the banner at the bottom are unchanged.

**Decisions I made**
- Built on the D mockup, then toned down: the red checkered frame became a plain semi-transparent backing, and the checkered name plate, tooltip title and relic badge were dropped. The class name in D was dark text straight on the gold checkers, so it now sits on a dark chip in gold text, for contrast.
- Everything is drawn in code on top of the vanilla panel (`Patches/CharacterSelectSkinPatch.cs`) and undone on the next selection, so no art is needed.
- Uses the game's own fonts. The pixel font from the mockup (Silkscreen) isn't bundled; ask if you want it.
- The tooltip is our own (styled like the mockup), not the game's hover tip.
- New strings: `(character).className`, `(character).playstyle.(KEY).title/description` in characters.json, `SHIONTHEBRUISER-CHARACTER_SELECT.playstyles` in main_menu_ui.json. `title` and `titleObject` are now just the first name. Strings that only name the class ("Bruiser Cards", Sea Glass) and the epoch unlock texts that spell out "Shion, the Bruiser" were left as they are.
- Seels and Fry get the skin but no playstyle row until their playstyles are given (`Playstyles` in their character class).

## 27. Change Pending H (Seels card pass, Go Crazy, Gamble tag, Slal Slime cards)

**Setup**
- [ ] Rebuild, re-export the .pck (cards.json and powers.json changed) and let MegaDot generate the `.uid` files for `SeelsFinalRush.cs`, `GoCrazyPatches.cs`, `BeastForm.cs`, `GoCrazy.cs` and `NeverLearn.cs`. `BeastFormStrength.cs.uid` is the old `SavageryStrength` uid, renamed.

**Seels**
- [ ] Beast Form: 3 cost Power, Ethereal. Whenever you play an Attack, gain 2 (3) Strength this turn (the Strength wears off at end of turn).
- [ ] Savagery: 2 cost. The first time (2 times upgraded) you play an Attack each turn, a random card in your Hand becomes free this turn. The power's number counts down the uses left this turn and refills next turn. It picks cards that cost something first (not X-cost); with an empty Hand nothing happens but the use is spent.
- [ ] Blind Assault: 2 cost (1 upgraded). Makes plain Corrupted Slashes (no longer Slashes+ when upgraded).
- [ ] Blood Bath: Blood Bend X, gain 6X Block (6 + 6X upgraded) in **one** gain, so Dexterity applies once. Gain 2 Blur. Check the in-combat preview matches.
- [ ] Blood Cage: now Exhausts.
- [ ] Blood Prison: Blood Bend X. Gain 4 Block and X (X+1) Plating. Apply X (X+1) Vulnerable to ALL enemies. Exhaust. Plating gives Block at end of turn and goes down by 1 each turn.
- [ ] Blood Punch: Lose 3 (2) HP. Deal 7 damage, +3 for each time you lost HP this combat (this card's own HP loss counts). Goes back into the Draw Pile at a random spot. Preview damage = what it deals. Check with Buffer/Intangible blocking the HP loss.
- [ ] Burning Pain: Uncommon Skill, 2 cost (1 upgraded), Wounding. The next Blood Bend X gives X Strength and uses the charge, even with 0 Blood. Two copies = next 2 Blood Bends.
- [ ] Combo Meter: upgrade now draws 1 card (cost stays 1).
- [ ] Final Rush (new Rare, 2 cost): 2 damage to a random enemy per Slash in the Exhaust Pile, then adds Finishing Explosion to your Hand. Exhaust (upgrade removes Exhaust). Shows up in rewards; has no art (listed in MISSING_IMAGES).
- [ ] Finishing Explosion: now a 0-cost token (not in rewards). 4 damage to ALL enemies + 2 (4 upgraded) per Slash in the Exhaust Pile. Exhaust.
- [ ] Gambling Addiction: 1-cost Event card (no longer in rewards), Exhaust, Eternal, no upgrade. Deals damage = the energy it was played for (0 if free) to a random enemy, once per Erratic card you have. If it's in your Hand at end of turn, Gamble your whole Hand, Gambling Addiction included. Try two copies in Hand.
- [ ] Go Crazy: Ancient 3-cost Power (Innate upgraded). Once per turn, a card you can't afford is playable: you spend all your energy and lose HP equal to the missing energy. Check: the card glows as playable, the counter goes to 0 after one use and refills next turn, X-cost cards and star costs aren't affected, two copies give 2 uses, and it works in multiplayer for the right player.
- [ ] Magical Overload: enchanted Slashes created by Blind Assault, Press the Attack, Pin Them Down, Spectacular Break-in and Threatening Promise now count as modified cards (they didn't before).
- [ ] Never Learn: 3 cost Power (Retain upgraded). When Gloat's energy arrives at the start of your turn, gain that much Strength (×2 with two copies). No longer Blood Bends or gives Max HP.
- [ ] Private Joke: wording now "deal 3 damage to a random enemy".
- [ ] Scarlet Shadow: Double (Triple) your Blood, no Blood Bend on play. Still Blood Bends at end of combat and Exhausts. Preview shows the Blood gained.
- [ ] Sweat and Tears: Whenever you Blood Bend (even with 0 Blood, and replays count), gain 1 (2) energy. Copies stack into one power now.
- [ ] Tornado Stance: Rare (no longer Ancient).
- [ ] U-Turn: also discards drawn X-cost cards and Unplayable cards (Curses, Statuses), like Scrape.
- [ ] Blood Bend + Chemical X: Blood Bend X gets +2 (Blood Bath, Blood Cage, Blood Prison, etc. and their previews), and Burning Pain counts it too. The heal stays at the Blood actually consumed. Chemical X doesn't flash for these.
- [ ] Gamble tag added to Card Dealing, Wink, Cliff Dropper, Dice Blessing, Excited Wait, Illusion of Risk and Toss A Coin (nothing uses it yet).
- [ ] Big Numbers: the power's number now shows the highest cost an Erratic card can reach (3 + stacks).

**Slal**
- [ ] Slime Hammer: Very Slimy. 10 damage + 2 per Slimy card you have. Exhaust (upgrade removes Exhaust instead of raising the bonus).
- [ ] Slime Form: whenever you play a Slimy or Very Slimy card, gain 4 Block (no longer on creating Statuses). Cost unchanged (2, 1 upgraded).

**Decisions I made**
- Finishing Explosion upgrade = per-Slash bonus 2 → 4; Gambling Addiction damage = its own cost per Erratic card; Go Crazy HP = missing energy; Burning Pain charge is used up even at 0 Blood (all confirmed).
- Savagery's old effect moved to Beast Form: the power is now `BeastForm`, its temporary Strength `BeastFormStrength` (renamed from `SavageryStrength`, loc key renamed too). `Savagery` is the new free-card power.
- Kept current values the sheet doesn't give: Burning Pain stays 2 cost, Savagery upgrade +1 trigger, Sweat and Tears upgrade +1 energy, Never Learn 3 cost with Retain upgrade, Scarlet Shadow 0 cost, Slime Hammer 2 cost, Blood Prison 0 cost.
- Blind Assault: the sheet's upgrade "1-cost" replaces the old Slashes+ upgrade.
- Blood Prison: X+1 applies to both Plating and Vulnerable. It no longer deals damage.
- Gambling Addiction: Gambles itself too, so it keeps coming back on top of your Draw Pile (Erratic). Damage ignores Strength. `//todo : upgrade not specified`, so it can't be upgraded.
- Go Crazy is done with two Harmony patches (`GoCrazyPatches.cs`): one makes the card affordable, the other notes the missing energy before it's spent; the power takes the HP once the energy is spent.
- Never Learn's Strength comes at the start of your turn, together with Gloat's energy (not when Gloat triggers).
- Cursed Dice doesn't exist yet, so only the tag was added.
- New power icons not made: Beast Form, Savagery (new effect), Go Crazy, Never Learn added/updated in MISSING_IMAGES.tsv, plus the Final Rush portrait.
- The "Ideas (ne pas implémenter encore)" rows were skipped.

## 28. Star Rod (Shion's Tanx relic)
- [ ] Tanx offers Star Rod to Shion about 3 times in 10, in place of one of his 3 weapons. Shows in the Relic Collection and the `ancient` console command.
- [ ] Icon, outline and big icon look right (made from star_rod.png: trimmed, fitted to 256×256, white silhouette outline).
- [ ] Galvanised Attacks (Energy Ball, Energy Jolt, Energy Wave, Energy Volley, Flood, Break Down) apply one random effect after resolving: 1 Weak, 1 Vulnerable, 6 Confusion, 6 Doom or 3 Poison. One roll per play, given to every enemy the Attack hit that's still alive.
- [ ] Doesn't trigger on non-Galvanised plays, under Spirit Break / Defensive, or from Energy Strike ("if not Galvanised"). Fell Dragon makes it always trigger.
- [ ] Shield Breaker replays each roll separately when Galvanised.
- [ ] Confusion from Star Rod isn't boosted by Slal's Random (no card source).
- [ ] MP: no desync when it triggers (roll uses the Niche RNG, only in AfterCardPlayed).

**Decisions I made**
- "Galvanised Attack" = an Attack whose own Galvanised effect triggered on that play. Shield Breaker's Galvanised 9 doesn't make every Attack count. Cards now pass themselves to `ShionMechanicsHelper.TriggerGalvanised(card, threshold)` so the relic knows.
- Effects are applied to the enemies the Attack hit, after it resolves (so the debuff doesn't boost that Attack's own damage).

## 29. Relic List pass (missing relics + flavour texts)
- [ ] Rebuild and re-export the pck (new relics + relics.json changes).
- [ ] Cursed Dice: grant it with the console. Gambling Addiction is added to your Deck when you pick it up.
- [ ] Cursed Dice: every combat card reward shows 1 extra card, a Gamble card from Seels' pool (Wink, Card Dealing, Dice Blessing...), and it doesn't repeat a card already in the reward. Also works when a non-Seels character holds it. No extra card on non-combat rewards (events, shops).
- [ ] Whimsical Hat: grant it with the console. 2 Dazed are shuffled into your Draw Pile at the start of combat.
- [ ] Whimsical Hat: at the end of your turn, a random Ethereal card in your Hand is played for free. Nothing happens if there isn't one. Dazed (Unplayable) is never picked.
- [ ] MP: no desync from Whimsical Hat's pick (Shuffle RNG, end-of-turn only) or Cursed Dice's rewards.
- [ ] Neither relic drops from chests, shops or elites (Event rarity, EventRelicPool). Both show in the Relic Collection.
- [ ] Flavour texts now show for every Slal and Seels relic (Creative Spark through Crystal Garland). Creative Spark's flavour is no longer a copy of its effect.
- [ ] Both new relics use the placeholder icon (rows added to MISSING_IMAGES.tsv).

**Decisions I made**
- "The Gambler" event doesn't exist in the mod or the base game, so Cursed Dice is in the event pool and can't be found yet. Same for Whimsical Hat, which has no source listed (both confirmed).
- Whimsical Hat's autoplay is free, like Stampede (confirmed). "Daze" = the base game's Dazed.
- Cursed Dice's extra card follows Lasting Candy, but on every combat reward: Common/Uncommon/Rare Seels cards with the Gamble tag, using the reward's normal rarity odds (confirmed).
- Flavour texts were copied as written, except "suprisingly" → "surprisingly" (Genie's Advice) and a stray space in "apples !" (Apple Sauce). Keyboard Key's missing letters ("hrder", "t some point", "hve") look like a joke about a broken key, so I left them.
- Not touched: the 6 Fry rows with no name or effect, the reviewer notes (shop relic suggestions, "remove or Boss" on Training Manual, Shell Bell / Souvenir Picture remarks), and effect-text differences. All other relics on the sheet already existed and their flavours already matched.

## 30. Special Events pass (Ninja Trial, Team Battle, The Gambler, Monster Soul)
- [ ] Open the project in MegaDot so the 4 new images in `ShionTheBruiser/images/events/` get imported, then rebuild and re-export the pck.
- [ ] Console tip: `event` with the event id (`SHIONTHEBRUISER-THE_GAMBLER`, `SHIONTHEBRUISER-MONSTER_SOUL`, `SHIONTHEBRUISER-TEAM_BATTLE`, vanilla `TRIAL`) to jump straight in.

**The Trial: Ninja (new defendant in the vanilla event)**
- [ ] Accepting the Trial rolls the Ninja about 1 time in 4. Merchant, Noble and Nondescript still show up, equally often.
- [ ] Visuals: the vanilla "trial started" portrait, with the panini drawn where vanilla's defendants stand. Check size and position (`NinjaTrialPatch.CreateDefendant`: position 568,700, scale 0.6).
- [ ] Innocent: Doubt is added to your Deck, then you pick 1 card to Upgrade. The result page starts with "IT IS DECIDED" (vanilla adds it).
- [ ] Guilty: the option flashes as damaging. You take 8 damage (hit sound plays), then get 2 relics among Kusarigama / Nunchaku / Kunai / Shuriken you don't own. With 3 of them owned: the last one + 1 truly random. With all 4: 2 truly random.
- [ ] Reject → Accept still leads to the trials (Ninja included).

**Team Battle (multiplayer only, Hive or Glory)**
- [ ] Never shows in solo, nor in Act 1. Shows the vanilla "collaborative event, vote" line.
- [ ] Fight them as a team: everyone gets a card reward of Team Blast cards (their own class's + generic Colorless ones), and Clumsy.
- [ ] Fight your counterpart one on one: everyone gets a Rare card reward from their own pool, and Injury.
- [ ] Fight your teammate's counterpart: only offered when the team has at least 2 different classes. Everyone gets 2 card rewards from their teammates' classes (mixed when there are several), never from their own class.
- [ ] MP: same 2 or 3 options on every client, no desync after the vote.

**The Gambler (any act, after Seels' Asc 2 is unlocked)**
- [ ] Only appears once Seels' "Roll With The Punches" epoch is revealed (by anyone in the run), and only when every player has 120+ Gold.
- [ ] Do not play: pick a card, it's Transformed into Shame (Eternal cards can't be picked). Leaves the event.
- [ ] Play once: pays 40 Gold, rolls a new curse. The first roll never gives Shame; no curse repeats until all 13 have shown, then it starts over. First and later plays have different text; the man's line names the curse.
- [ ] After a roll, "Do not play" transforms into the rolled curse. When Great Fortunes is rolled, it becomes "Celebrate": adds Great Fortunes, then leaves.
- [ ] Below 40 Gold, "Play once" turns into the locked "Broke" option.
- [ ] Play until you win: lose ALL your Gold, get the Cursed Dice (and its Gambling Addiction). Leaves the event.
- [ ] Portrait looks right (Eggman on the left, text readable on the right).

**Great Fortunes (new Seels curse)**
- [ ] Curse frame. Unplayable, Innate, Sly, Exhaust, Retain, Eternal. Starts each combat in your Hand and stays there.
- [ ] Discarding it auto-plays it (Sly), which Exhausts it: +150 Gold.
- [ ] Exhausting it any other way: +150 Gold. Transforming it in combat: +150 Gold.
- [ ] Can't be removed or transformed out of your Deck (Eternal). Never rolled by random-curse effects (Neow's Bones, Sere Talon, Cursed Run).
- [ ] MP: no desync from the gold (paid inside the transform/exhaust command).

**Monster Soul (any act, after Shion's Asc 2 is unlocked)**
- [ ] Only appears once Shion's "If One Doesn't Cut It..." epoch is revealed (by anyone in the run).
- [ ] Hold onto the Crystal: lose 10 Max HP (option flashes red at 10 Max HP or less), Monstermancy added to your Deck.
- [ ] Smash the Crystal: heal to full, Regret added to your Deck.
- [ ] The outcome text changes for Shion / another Dreamer (Slal, Fry, Seels) / a vanilla character.

**Decisions I made**
- "Asc 2" gates = Asc 2 unlocked = the existing "win a run in Ascension 1" epochs (confirmed). In MP it counts if anyone in the run has it, like vanilla's event unlocks.
- Great Fortunes is Unplayable like every curse (your list had no cost). Sly + Exhaust is what pays out when discarded. It sits in the vanilla curse pool (for the curse frame) but opts out of random curse generation. Text: "If this card is Transformed or Exhausted, gain 150 Gold."
- Team Blast reward = cards with the Team Blast keyword (confirmed). Rare reward = your own class's pool. No real combat happens in Team Battle (the text tells how the fight went).
- Gambler's "Do not play": you pick the card (confirmed). Its outcome text says the current curse ("Doubt for the quitter it is, then…") instead of always "Shame".
- Events use one portrait each, as in vanilla: your images placed on the left of a dark 2560×1200 backdrop (the event screen's size), with a coloured glow. The Ninja uses vanilla's Trial portrait and the panini as the defendant sprite. `CustomVfxPath` in BaseLib does nothing, so no event uses it.
- Text: colour tags mapped (shake → jitter, wavey → sine). Small fixes: "developped" → "developed", "upperhand" → "upper hand", "Low and behold" → "Lo and behold", "weakness lie" → "weaknesses lie", "rel to leave" → "relieved to leave", "a dice" kept. "(Collaborative choice.)" dropped since vanilla shows its own vote line.
- Author Trial skipped, as asked.

## Master Artisan Cleat rework

- [ ] At the start of each combat (turn 1, before the opening draw), 1 random Rare Colourless card is added to your Hand.
- [ ] That card costs 0 for the whole combat, including after being shuffled back and redrawn.
- [ ] No more pickup card-selection screen when obtaining the relic.

**Decisions I made**
- Rarity unchanged (Rare). X-cost cards (if any Rare Colourless has one) are not made free, matching the mod's other "free" helpers.

## 31. Cosmo, Mother of (re)Animation (new ancient + Warm Embrace epoch)
- [ ] Open the project in MegaDot so the new images in `ShionTheBruiser/images/ancients/` and the scene `ShionTheBruiser/scenes/ancients/cosmo.tscn` get imported, then rebuild and re-export the pck.
- [ ] Console tip: `ancient SHIONTHEBRUISER-COSMO` (or `event SHIONTHEBRUISER-COSMO`) to jump straight in.

**Warm Embrace (epoch)**
- [ ] Its slot appears in the Invitation7 column (row 5, under Fry's), opened by Shion's "The Call". On a save where The Call is already revealed, the slot should still show up after a restart (the save fixer adds missing child slots).
- [ ] Obtained at the end of any run once 2 Chaos Emeralds are unlocked in the save. Text, unlock info ("Unlock 2 Chaos Emeralds") and unlock text ("Cosmo will sometimes welcome you…") look right. The story tab reads "Dreamers". The portrait is the placeholder for now.

**When Cosmo shows up (instead of Neow)**
- [ ] Never before Warm Embrace is revealed, never in custom/daily runs with modifiers, never in Acts 2 or 3.
- [ ] Solo: shows up every run after a run where that character held a Chaos Emerald (win or loss), and only then. Neow comes back after a run without one.
- [ ] MP: shows up when anyone in the party has Warm Embrace, every other run (Cosmo, Neow, Cosmo…), following the host's profile. Every player sees the same ancient. No desync.
- [ ] Like Neow, she heals you to full on entering (heal VFX plays). The map's start node and the run history show her icon.

**Visuals**
- [ ] Warm backdrop with Cosmo on the left (scene: Figure at 156,100, 768×1020). Check her size and position next to the dialogue and options, at 16:9 and at a wider/narrower window. Nothing should overlap the option buttons.
- [ ] Her dialogue bubbles are leaf green and use her icon. The name banner reads "Cosmo, Mother of (re)Animation".
- [ ] The source art has the artist's credit ("Cosmo the Seedrian from Sonic X, fanart by AlexiSonicKST") under her feet. Decide whether to keep it.

**Options (3 relics)**
- [ ] Solo: 1 cursed relic (last) + 2 non-cursed. Never a "MP only" relic.
- [ ] MP: 1 "MP only" relic + 1 cursed relic (unless the MP one is cursed: Power Core, Chaos Ring, Poem of Gratitude) + non-cursed for the rest. Never a "Solo only" relic.
- [ ] Festive Ribbons and Secret Key never show up together. Picking a cursed relic shows the "Be careful with that one" closing text.
- [ ] Power Core and Friendship Bracelet no longer appear at Neow. Master Emerald Shard Pendant no longer drops as a Shion Rare relic (only from Cosmo).

**Relics**
- [ ] Power Core / Friendship Bracelet (MP): unchanged effects (pick a Team Blast / Team-Up, then a random Curse).
- [ ] Master Emerald Shard Pendant: Lull still works at rest sites, for any character.
- [ ] Sailing Ship (MP): gaining Max HP (e.g. a Strawberry) gives every other living player the same; losing Max HP takes it from them. Two players with a Sailing Ship: no infinite loop (mirrored changes don't mirror again).
- [ ] Checkpoint (solo): from any later map point of the act, the last Rest Site you visited also lights up as travelable. Going back uses it up (relic greys out); the map then continues from that Rest Site. Check the save/reload after going back, and that it doesn't break the map (spec note: "tell me if it completely breaks everything").
- [ ] Comfy Cape: Rest heals 30% + 20% of Max HP. Being Mended by someone also gets the +20%.
- [ ] Super Bulldozurator Puzzlitowersonic: after Smithing, a random other upgradable card is also Upgraded (shown).
- [ ] Peace Promise (cursed): on pickup, 3 basic Strikes are Transformed (screen shows them). Each Attack you play gives "Peace Promise" −3 Strength until end of turn (stacks per Attack, all restored at end of turn).
- [ ] Chaos Ring (MP, cursed): only offered if you unlocked (in your save) the emerald of another player's character that you don't hold. On pickup: that Chaos Emerald + 2 of that character's basic Defends. Check every client agrees on whether it's offered (no desync).
- [ ] Leaf Robe (MP): after you Mend someone, you heal 15% of your Max HP.
- [ ] Invisible Notebook (cursed): all your cards (hand, deck, rewards, shop) show no description. At 0 HP once: survive and heal to 50% Max HP, then the relic is spent.
- [ ] Force Cursor: opening a Treasure chest also gives a 3-card reward. Check in MP: only the owner gets it.
- [ ] Immortal Rose: your next 3 card rewards each have one card Enchanted with Sown (counter 3 → 0, then greyed out).
- [ ] Lucky Glove: your card rewards show the Reroll button until you use it once. The rerolled cards are Upgraded. After that, no more Reroll buttons (even on other rewards of the same screen, once reopened).
- [ ] Lemonade Bottles (MP): the other players get 100 Gold. Pop Corn (MP): the other players get 5 Max HP.
- [ ] Poem of Gratitude (MP, cursed): every player (you included) gets a random relic. Your first Elite combat afterwards gives you no relic (the others still get theirs).
- [ ] Planet Egg: choose 1 of 3 starting cards of other unlocked characters (no Strikes/Defends), e.g. Bash, Neutralize, Zap…
- [ ] Festive Ribbons (solo, cursed): all Defends Upgraded on pickup. The first Treasure chest is empty, the next ones are normal.
- [ ] Souvenir Seed: every player gets one random basic card Enchanted with Sown.
- [ ] Watering Can: the first Smith of an unenchanted card Enchants it with Sown (relic then greys out). Smithing an already-enchanted card doesn't use it.
- [ ] Santa Hat: the next Curse that would be added to your Deck isn't (relic flashes and greys out). Test with Power Core's Curse or an event Curse.
- [ ] Flo's Fridge: a random potion on pickup. On entering each Boss room: +1 potion slot (kept for the run) and a random potion.
- [ ] Secret Key (solo): after the mid-act Treasure chest, the other chests of that row are travelable once. Taking one uses the relic.

**Dialogue (eng only)**
- [ ] First ever visit (any character): the 6-line intro (2 narration lines, then "Coucou !"…).
- [ ] Any character: repeatable lines ("You can't give up now!" / "{character name}! Stay determined!", "I believe in you!", "If only there was a peaceful way…"). Solo after holding an emerald: "I can't bring you back the emerald you found last time…".
- [ ] Shion: 15 dialogues from the sheet. Seels: 14. One-time ones play at visits 1, 2, 5, 8… (default spacing). Repeatable ones join the pool from their slot.
- [ ] Conditional ones (Solo / MP without other Dreamers / MP with all the other Dreamers / MP with another mod's character) play once their condition holds, even after their slot. One-time ones play only once (saved in `shionthebruiser_cosmo.json`).

**Decisions I made**
- Spawn (your answer): solo = had an emerald last run with that character; MP = alternate with Neow per the host's profile, if anyone has Warm Embrace. Emerald unlocks and the host's "Cosmo next" flag are added to the unlock data every lobby already sends, so all clients decide the same thing without new network messages. These extras aren't in the run save: a reloaded MP run sees them as empty (on every client alike). That only matters if you reload while Cosmo's options are still up.
- Warm Embrace's slot is opened by Shion's "The Call" (the Dreamers' story starts with the first Dreamer). Row 5 of Invitation7, the first free row in Condensed Might's column.
- Option order: regular gifts first, then the MP-only one, the cursed one last (like Neow). The filler relics never include a second MP-only relic. "Friendship Bracelet" isn't in the cursed pool on the sheet, so it gets a cursed option next to it even though it adds a Curse.
- Chaos Ring gate (your answer): you must have unlocked the emerald of a partner's character and not hold it. The emerald is the solo one. The 2 Defends are the first basic Defend of that character's starting deck.
- Checkpoint and Secret Key add extra travelable map points. Checkpoint goes back to the last Rest Site of the current act. Secret Key opens the unvisited chests of the same row.
- Lucky Glove reuses the vanilla Reroll button (the Driftwood one). One use for the whole run.
- Flo's Fridge: the Boss-room slot is permanent (your answer). It triggers on entering the Boss room.
- Comfy Cape: +20% of Max HP, rounded down. Leaf Robe: 15%, rounded down (min 1).
- Peace Promise's −3 Strength is a BaseLib temporary-down power (default temp-down icon), so no new power icon.
- Conditional dialogues (your answer): a conditional one-time dialogue waits for its condition but not for its visit slot. It still waits for the unconditional one-time dialogues before it.
- Names: "Super Bulldozurator Puzzlitowersonic" is the sheet's name; its note says the full name is still to be fetched. "Master Shard Pendant" keeps its existing title, "Master Emerald Shard Pendant".
- Text: "{Class}" in Cosmo's line = the character's name. Small fixes: "revives you.." → "...", "english" → "English", "here's some something" → "here's something", "pretty stronger" → "pretty strong", "that one version of Fry I meant" → "I met", "Don't sweat it" got a period. I wrote the event page texts (opening, closing, cursed closing) and the button labels; the sheet didn't have them.
- Cosmo's art: your image with a soft warm halo, on a generated warm backdrop (amber glow, light rays, floating motes, darker on the right for readability). Map icon (160px) and dialogue/run-history icon (128px) come from your dialog icon, with white outlines.
- Not done: the epoch sheet's second, untitled row ("Unlock seven Chaos Emeralds") has no title or effect yet. Force Cursor's "the cursor changes to open chests" note is cosmetic and skipped.

## Event-only cards (Monstermancy, Gambling Addiction)

- [ ] **Monstermancy** never appears in card rewards, shops, random generation or transforms. You only get it from Monster Soul ("Hold onto the Crystal").
- [ ] **Gambling Addiction** never appears in card rewards (Cursed Dice's extra Gamble card included). You only get it from Cursed Dice (The Gambler event).
- [ ] Monstermancy still shows Shion's purple frame, with the Event rarity banner.

## 32. Change Pending I-1 pass (Seels rework, Blood display, Cosmo's Rulebook, Slal tweaks)
- [ ] Open the project in MegaDot so `seels_hack.png` (both sizes) and `scenes/fx/blood_wisps.tscn` get imported, then rebuild and re-export the pck.

**Blood**
- [ ] Blood is capped by missing HP. At full HP, Thorn Bracelet / Maim / Shed the Pain give no Blood. With 5 missing HP and 3 Blood, gaining 4 Blood only gives 2.
- [ ] Healing removes the Blood above the cap (heal potion, Red Fang, Reaper-style heals…). Blood Bend still heals X normally (its Blood is spent first).
- [ ] Carried-over Blood is capped when it comes back at the start of the next combat (e.g. after resting at a campfire).
- [ ] No Blood icon in the power row. Instead, a dark scarlet segment fills the missing-HP part of the health bar right after the current HP, and the amount shows in scarlet just right of the bar. Also on the small MP health bars. Hovering Seels still shows Blood's hover tip.
- [ ] Scarlet wisps circle Seels: one per Blood up to 10, then they grow with more Blood (up to 2.5x at 35+). They fade out when Blood is spent.
- [ ] Red Fang only counts Blood actually gained (not the part lost to the cap).
- [ ] Maim on the killing blow still grants Blood (fixed in commit 4f433c9 "Seels: Maim: Earn blood if lethal"). Check this build is after it. Remember the cap: no Blood at full HP.

**Seels cards**
- [ ] Beast Form+: costs 2. Strength stays 2.
- [ ] Blood Bath: Exhaust.
- [ ] Eye Opening Realization: costs 0.
- [ ] Blood Drop: Starter rarity. Seels' starting deck is now 4 Strike, 4 Defend, Blood Cage, Blood Drop. It no longer shows up in rewards.
- [ ] Cool Bandages (Common): gain 6 (8) Block, next 3 (5) Wounds are free and make ALL enemies lose 4 HP. Playing it twice stacks the Wounds count (3 + 3 = 6).
- [ ] Hack (new, Uncommon, 0 cost, targets an enemy): add 1 (2) Slash into your Hand, Blood Bend X, the enemy loses X Strength and you gain X Strength this turn. Exhaust. Preview shows X in combat. New art.
- [ ] Hematoma (follow-up rework): gain 7 (9) Block, then pick any number of cards in your Hand (0 is fine). Each is Transformed into a Wound enchanted with Swift 1. Those Wounds are playable and cost 0 until the end of the turn, then go back to Unplayable. Other Wounds are unaffected. Two Hematomas in one turn both work. No Wounding anymore.
- [ ] Gambling Addiction+: costs 0. Note: its damage equals the energy it was played for, so at 0 cost it deals 0 unless Erratic/Gamble raises its cost.
- [ ] Bloody Execution (Rare, no Maim anymore): only playable when it would kill. It only glows when at least one enemy is killable, and you can only aim at enemies it would kill (damage after Strength/Vulnerable/bonus, minus the enemy's Block, ≥ its HP). Upgrade: 25 damage, +4 per Vulnerable.
- [ ] Shed the Pain: Uncommon.
- [ ] Do This All Day: puts a copy of ALL Slashes from the Exhaust Pile into the Draw Pile (originals stay exhausted). Upgraded: the copies are upgraded.
- [ ] Strike First (Common): 5 (6) damage, 2 (3) Strength this turn if the enemy intends to attack.
- [ ] Summon Wrath: after playing it, your next Attack this turn has its base damage multiplied by the number of identical cards you have (all piles, upgraded/enchanted copies included, the played card counts) and costs that many HP when played. Test the 3 sheet examples:
  - [ ] Finishing Explosion with 1 copy → lose 1 HP, normal damage.
  - [ ] Strike with 4 Strikes total (one Erratic) → 6 × 4 = 24 damage, lose 4 HP.
  - [ ] Glass 2 Slash+ with 1 Strength, 6 Slashes in total → 7 × 6 = 42 + 1 = 43 damage, lose 6 HP.
- [ ] Summon Wrath: while active, every Attack in your Hand shows its multiplied damage and a "Summon Wrath: lose X HP." line. It wears off at the end of your turn. Multi-hit Attacks multiply every hit; the HP is lost once, before the Attack resolves. Upgraded: no Exhaust.

**Cosmo's Rulebook / Cosmo's Plea**
- [ ] Cosmo's Rulebook is in Cosmo's non-cursed pool (solo and MP). Upon pickup, a Cosmo's Plea is added to your Deck (with the card preview).
- [ ] Cosmo's Plea (Ancient, 1 cost): gain 10 (14) Block, ALL Attacks in your Hand go on top of your Draw Pile. Exhaust. Upgraded: Retain. You get a thought bubble: "Maybe violence is not the issue here." Works for every character.

**Slal**
- [ ] Octopus Form: no Dexterity anymore.
- [ ] Volatile Form: triggers on Tokens and Statuses only (no more Curses). A Token that Exhausts itself when played triggers once, not twice. A Token/Status Exhausted by something else (or by Ethereal) still triggers.
- [ ] Plot Twist: Exhaust. Burst of Laughter: Exhaust.
- [ ] Running Joke: no longer Slimy (no Slimed added when played).

**Decisions I made**
- Your answers: full Blood rework; Blood Drop added to the starting deck (10 cards); Rulebook is just part of Cosmo's non-cursed pool; Bloody Execution drops Maim, Volatile Form drops Curses, Hematoma plays the Wounds in your Hand only, Beast Form+ only costs 2.
- Blood: I read "Main cards" as Maim cards and left "gained by losing HP" to Thorn Bracelet, as before. Blood gained above the cap is simply lost.
- Blood health-bar look: a flat dark scarlet segment plus the number right of the bar. Wisps are drawn in code (no art): glowing red flames with short trails. Past 10 Blood they grow by 6% per Blood, up to 2.5x.
- Bloody Execution's "would kill" check uses the previewed damage minus Block. It ignores crits and HP-loss effects like Intangible, because some of those roll RNG and can't be used for a preview.
- Summon Wrath: the multiplier applies after the card's enchantment and before Strength, which matches the sheet's 43 example. Playing Summon Wrath twice affects your next 2 Attacks (one each), not one Attack twice. Free extra plays of the same Attack (e.g. Echo Form-style replays) keep the multiplier but the HP is only lost once.
- Cool Bandages: the HP loss no longer upgrades (always 4), so stacking an upgraded and a normal copy just adds the Wounds counts.
- Cosmo's Rulebook flavor text is your sheet note, lightly reworded. The art is still missing (MISSING_IMAGES.tsv), as are Cosmo's Plea's art and the Summon Wrath power icon. Hack's two Strength powers use BaseLib's default temporary icons.
- Cosmo's Plea's line is a thought bubble (your note), using the game's ThinkCmd, so it is purely visual.
- Hematoma follow-up: your new text has no Wounding, so I removed it. The upgrade still gives +2 Block. A Wound you pick is still replaced by a new Swift 1 Wound. The "free this turn" part is a hidden power that ends at end of turn, so nothing on the cards has to be undone.
- Not done: the "Ideas (ne pas implémenter encore)" section.

## Seels playstyles

- [ ] Seels' character select panel shows 5 playstyle coins, in rows of 2: Blood Bend (Blood icon), Gamble (Gambler's Luck icon), Slash (Combo Meter icon), Wounds (Bleed Instructions icon), Gloat (Gloat icon).
- [ ] Hovering each coin shows its title and description as given. The panel backing grows to fit the 3 rows. Fry still shows no playstyles.

**Decisions I made**
- Icons are the big icons of the matching Seels powers. Keys in characters.json: BLOOD_BEND, GAMBLE, SLASH, WOUNDS, GLOAT.
