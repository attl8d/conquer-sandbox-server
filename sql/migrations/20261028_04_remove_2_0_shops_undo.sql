-- Undo 20261028_03_remove_2_0_shops.sql: put the removed NPCs and stock back.

START TRANSACTION;

INSERT IGNORE INTO `cq_npc` SELECT * FROM `bak_20261028_cq_npc`;
INSERT IGNORE INTO `cq_goods` SELECT * FROM `bak_20261028_cq_goods`;

COMMIT;

-- The backup tables can be dropped once you're happy:
-- DROP TABLE `bak_20261028_cq_npc`, `bak_20261028_cq_goods`;
