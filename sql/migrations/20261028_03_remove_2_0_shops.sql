-- Switch off the shops Classic Conquer doesn't have.
--
-- Classic's client knows 22 NPC shops, all ordinary silver shops. The 5187
-- sandbox has 2.0-era extras on top: Conquer Point sellers, the VIP shop,
-- the steed (mount) retailer, an event steward, extra town shopkeepers and
-- the client-opened mall shops. This removes those NPCs and their stock.
-- The 22 shared shops are kept as they are (their stock differs a little;
-- arrows and potions match).
--
-- Before deleting, the affected rows are copied into two backup tables, so
-- 20261028_04_remove_2_0_shops_undo.sql can put them back.
-- Restart the Game server afterwards.

START TRANSACTION;

CREATE TABLE IF NOT EXISTS `bak_20261028_cq_npc` LIKE `cq_npc`;
CREATE TABLE IF NOT EXISTS `bak_20261028_cq_goods` LIKE `cq_goods`;

-- NPCs: GreatMerchant + CostumerChen (CP sellers, Market), VIPSHOP and
-- ThanksgivingSteward (map 5000, the hidden NPC map), MountRetailer (riding), and the 2.0
-- Armorer/StoreKeeper extras in Desert City, Phoenix Castle, Bird Island
-- and Ape City (Classic has no shops with these ids).
INSERT IGNORE INTO `bak_20261028_cq_npc`
  SELECT * FROM `cq_npc`
  WHERE `id` IN (432, 433, 2888, 4352, 4353, 4354, 4355, 4356, 4357, 4358, 4359, 5517, 7484);

-- Stock of those shops, plus the shops the client opens on its own
-- (3983, 5793, 6000, 6001, 6004: mall and event shops with no NPC).
INSERT IGNORE INTO `bak_20261028_cq_goods`
  SELECT * FROM `cq_goods`
  WHERE `ownerid` IN (432, 433, 2888, 4352, 4353, 4354, 4355, 4356, 4357, 4358, 4359, 5517, 7484,
                      3983, 5793, 6000, 6001, 6004);

DELETE FROM `cq_goods` WHERE `id` IN (SELECT `id` FROM `bak_20261028_cq_goods`);
DELETE FROM `cq_npc` WHERE `id` IN (SELECT `id` FROM `bak_20261028_cq_npc`);

COMMIT;

-- Safety net: Conquer Point prices are not used by any shop that is left.
SELECT COUNT(*) AS `cp_goods_left` FROM `cq_goods` WHERE `moneytype` <> 0;
