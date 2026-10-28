# Label hook

A training aid for the screen bot in `attl8d/conquer-bot`. While the game server runs, it appends one JSON line per map that has players on it, every 100 ms, listing every role within 20 cells of a player: where it really is, its HP and what it is. The bot's recorder copies these lines next to its screen recording, so the detector can be trained on labels nobody has to draw by hand. The bot never reads this while it plays.

It only reads game state; it changes nothing in the game.

## Turning it on and off

It is on by default and writes to `C:\CO\bot-data\truth\server-truth.jsonl`. To change that, add an optional section to `Comet.Game.config`:

```json
"LabelHook": {
  "Enabled": true,
  "File": "C:\\CO\\bot-data\\truth\\server-truth.jsonl",
  "IntervalMs": 100
}
```

or pass it on the command line: `Comet.Game.exe --LabelHook:Enabled=false`.

At about 30 roles on screen it writes roughly 100 KB a second while someone is logged in, and nothing while the server is empty. The file only grows; delete it when the server is stopped if it gets big (the recorder copies what it needs into each session).

## Line format (version 1)

```json
{"t":1761676800123456700,"v":1,"kind":"snap","map":1002,"map_doc":1002,"users":[1000001],
 "roles":[
   {"id":1000001,"cls":"player","name":"Attila","mesh":1003,"x":438,"y":377,"dir":6,"lvl":15,"hp":420,"max_hp":420,"alive":true},
   {"id":400012,"cls":"monster","name":"Pheasant","mesh":101,"x":445,"y":370,"dir":2,"type":1,"lvl":1,"hp":33,"max_hp":33,"alive":true},
   {"id":700045,"cls":"item","name":"Arrow","mesh":1050000,"x":440,"y":376,"dir":0}
 ]}
```

| Field | Meaning |
| --- | --- |
| `t` | Unix time in nanoseconds from the PC clock, the same clock the recorder stamps frames with |
| `map`, `map_doc` | Map id, and the map file it uses (they differ for copies of a map) |
| `users` | Ids of the players on this map; one of them is the character being recorded |
| `cls` | `player`, `monster`, `guard`, `pet`, `npc`, `dyna_npc` (attackable NPCs such as guild poles) or `item` (loot on the floor) |
| `x`, `y` | Map cell. During a jump the server already has the landing cell while the client is still showing the jump |
| `dir` | Facing, 0 = south-west, clockwise to 7 = south |
| `hp`, `max_hp`, `alive`, `lvl`, `type` | Where they apply; `type` is the monster type id from `cq_monstertype` |

The code is `src/Comet.Game/World/Threading/LabelHook.cs`; it is started from `Kernel.StartupAsync`.
