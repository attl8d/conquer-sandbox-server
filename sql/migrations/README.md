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
| `20261028_03_remove_2_0_shops.sql` | Removes the 2.0-only shops (CP sellers, VIP, mount, mall and event shops); backs the rows up first |
| `20261028_04_remove_2_0_shops_undo.sql` | Undo for the above, restores from the backup tables |
