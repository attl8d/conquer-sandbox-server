# Sandbox migrations

SQL scripts that change the bundled 5187 data so the sandbox plays more like
the target server. Run them in filename order against the game database after
the base dumps (`comet.game.sql`, `comet.game.data.sql`) are loaded, for example:

```
mysql -u root -p comet < sql/migrations/20261028_01_classic_monster_hp.sql
```

Each change has an `_undo.sql` partner that puts the original values back.
Restart the Game and AI servers afterwards so the new data is loaded.

| File | What it does |
| --- | --- |
| `20261028_01_classic_monster_hp.sql` | Raises HP of 111 monster types to the target server's values (mostly 1.5x) |
| `20261028_02_classic_monster_hp_undo.sql` | Undo for the above |
