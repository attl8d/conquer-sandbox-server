-- Raise monster HP on the sandbox to Classic Conquer's values.
--
-- Source: Classic's client monster table (maxLife), matched to cq_monstertype
-- by name and level. Only raises HP; rows where the sandbox is already higher
-- are listed at the bottom and left alone.
--
-- Run once against the game database, then restart the Game and AI servers
-- (monsters read their HP from cq_monstertype when they spawn).
-- Undo: 20261028_02_classic_monster_hp_undo.sql

START TRANSACTION;

UPDATE `cq_monstertype` SET `life` = 183 WHERE `id` = 3 AND `life` = 122; -- Robin L12: 122 -> 183
UPDATE `cq_monstertype` SET `life` = 303 WHERE `id` = 4 AND `life` = 202; -- Apparition L17: 202 -> 303
UPDATE `cq_monstertype` SET `life` = 548 WHERE `id` = 5 AND `life` = 366; -- Poltergeist L22: 366 -> 548
UPDATE `cq_monstertype` SET `life` = 667 WHERE `id` = 6 AND `life` = 445; -- WingedSnake L27: 445 -> 667
UPDATE `cq_monstertype` SET `life` = 817 WHERE `id` = 7 AND `life` = 545; -- Bandit L32: 545 -> 817
UPDATE `cq_monstertype` SET `life` = 999 WHERE `id` = 8 AND `life` = 666; -- Ratling L37: 666 -> 999
UPDATE `cq_monstertype` SET `life` = 1828 WHERE `id` = 9 AND `life` = 1219; -- FireSpirit L42: 1219 -> 1828
UPDATE `cq_monstertype` SET `life` = 2199 WHERE `id` = 10 AND `life` = 1467; -- Macaque L47: 1467 -> 2199
UPDATE `cq_monstertype` SET `life` = 2643 WHERE `id` = 11 AND `life` = 1763; -- GiantApe L52: 1763 -> 2643
UPDATE `cq_monstertype` SET `life` = 3184 WHERE `id` = 12 AND `life` = 2124; -- ThunderApe L57: 2124 -> 3184
UPDATE `cq_monstertype` SET `life` = 4771 WHERE `id` = 13 AND `life` = 3182; -- Snakeman L62: 3182 -> 4771
UPDATE `cq_monstertype` SET `life` = 5760 WHERE `id` = 14 AND `life` = 3842; -- SandMonster L67: 3842 -> 5760
UPDATE `cq_monstertype` SET `life` = 7358 WHERE `id` = 15 AND `life` = 4908; -- HillMonster L72: 4908 -> 7358
UPDATE `cq_monstertype` SET `life` = 8459 WHERE `id` = 16 AND `life` = 5642; -- RockMonster L77: 5642 -> 8459
UPDATE `cq_monstertype` SET `life` = 11673 WHERE `id` = 17 AND `life` = 7786; -- BladeGhost L82: 7786 -> 11673
UPDATE `cq_monstertype` SET `life` = 13422 WHERE `id` = 18 AND `life` = 8952; -- Birdman L87: 8952 -> 13422
UPDATE `cq_monstertype` SET `life` = 15432 WHERE `id` = 19 AND `life` = 10293; -- HawKing L92: 10293 -> 15432
UPDATE `cq_monstertype` SET `life` = 20697 WHERE `id` = 55 AND `life` = 13805; -- BanditL97 L97: 13805 -> 20697
UPDATE `cq_monstertype` SET `life` = 1096 WHERE `id` = 64 AND `life` = 731; -- HeavyGhostL23 L23: 731 -> 1096
UPDATE `cq_monstertype` SET `life` = 1334 WHERE `id` = 65 AND `life` = 890; -- WingedSnakeL28 L28: 890 -> 1334
UPDATE `cq_monstertype` SET `life` = 2451 WHERE `id` = 66 AND `life` = 1635; -- BanditL33 L33: 1635 -> 2451
UPDATE `cq_monstertype` SET `life` = 2997 WHERE `id` = 67 AND `life` = 1999; -- FireRatL38 L38: 1999 -> 2997
UPDATE `cq_monstertype` SET `life` = 5484 WHERE `id` = 68 AND `life` = 3658; -- FireSpiritL43 L43: 3658 -> 5484
UPDATE `cq_monstertype` SET `life` = 6597 WHERE `id` = 69 AND `life` = 4400; -- MacaqueL48 L48: 4400 -> 6597
UPDATE `cq_monstertype` SET `life` = 7929 WHERE `id` = 70 AND `life` = 5289; -- GiantApeL53 L53: 5289 -> 7929
UPDATE `cq_monstertype` SET `life` = 9552 WHERE `id` = 71 AND `life` = 6371; -- ThunderApeL58 L58: 6371 -> 9552
UPDATE `cq_monstertype` SET `life` = 14313 WHERE `id` = 72 AND `life` = 9547; -- SnakemanL63 L63: 9547 -> 14313
UPDATE `cq_monstertype` SET `life` = 17280 WHERE `id` = 73 AND `life` = 11526; -- SandMonsterL68 L68: 11526 -> 17280
UPDATE `cq_monstertype` SET `life` = 22074 WHERE `id` = 74 AND `life` = 14723; -- HillMonsterL73 L73: 14723 -> 22074
UPDATE `cq_monstertype` SET `life` = 16918 WHERE `id` = 75 AND `life` = 11284; -- RockMonsterL78 L78: 11284 -> 16918
UPDATE `cq_monstertype` SET `life` = 23346 WHERE `id` = 76 AND `life` = 15572; -- BladeGhostL83 L83: 15572 -> 23346
UPDATE `cq_monstertype` SET `life` = 26844 WHERE `id` = 77 AND `life` = 17905; -- BirdmanL88 L88: 17905 -> 26844
UPDATE `cq_monstertype` SET `life` = 30864 WHERE `id` = 78 AND `life` = 20586; -- HawkL93 L93: 20586 -> 30864
UPDATE `cq_monstertype` SET `life` = 41394 WHERE `id` = 79 AND `life` = 27610; -- BanditL98 L98: 27610 -> 41394
UPDATE `cq_monstertype` SET `life` = 40000 WHERE `id` = 84 AND `life` = 26680; -- Banditti L98: 26680 -> 40000
UPDATE `cq_monstertype` SET `life` = 817 WHERE `id` = 3031 AND `life` = 545; -- Cateran L32: 545 -> 817
UPDATE `cq_monstertype` SET `life` = 999 WHERE `id` = 3032 AND `life` = 666; -- PoisonousRat L37: 666 -> 999
UPDATE `cq_monstertype` SET `life` = 8459 WHERE `id` = 3033 AND `life` = 5642; -- StoneBandit L77: 5642 -> 8459
UPDATE `cq_monstertype` SET `life` = 4771 WHERE `id` = 3050 AND `life` = 3182; -- HeresySnakeman L62: 3182 -> 4771
UPDATE `cq_monstertype` SET `life` = 6902 WHERE `id` = 5045 AND `life` = 3451; -- UndeadSoldier L120: 3451 -> 6902
UPDATE `cq_monstertype` SET `life` = 6902 WHERE `id` = 5046 AND `life` = 3451; -- Phantom L120: 3451 -> 6902
UPDATE `cq_monstertype` SET `life` = 10353 WHERE `id` = 5049 AND `life` = 5176; -- UndeadSpearman L121: 5176 -> 10353
UPDATE `cq_monstertype` SET `life` = 10272 WHERE `id` = 5050 AND `life` = 5136; -- Revenant L121: 5136 -> 10272
UPDATE `cq_monstertype` SET `life` = 8217 WHERE `id` = 5051 AND `life` = 4108; -- Eidolon L123: 4108 -> 8217
UPDATE `cq_monstertype` SET `life` = 20673 WHERE `id` = 6011 AND `life` = 13789; -- Ghost L82: 13789 -> 20673
UPDATE `cq_monstertype` SET `life` = 1100 WHERE `id` = 8100 AND `life` = 734; -- HumanMessenger L22: 734 -> 1100
UPDATE `cq_monstertype` SET `life` = 1200 WHERE `id` = 8101 AND `life` = 800; -- WingedSnakeMsgr L27: 800 -> 1200
UPDATE `cq_monstertype` SET `life` = 1600 WHERE `id` = 8102 AND `life` = 1067; -- BanditMessenger L32: 1067 -> 1600
UPDATE `cq_monstertype` SET `life` = 2000 WHERE `id` = 8103 AND `life` = 1334; -- RatMessenger L37: 1334 -> 2000
UPDATE `cq_monstertype` SET `life` = 4400 WHERE `id` = 8105 AND `life` = 2935; -- MonkeyMessenger L47: 2935 -> 4400
UPDATE `cq_monstertype` SET `life` = 5300 WHERE `id` = 8106 AND `life` = 3535; -- GiantApeMsgr L52: 3535 -> 5300
UPDATE `cq_monstertype` SET `life` = 6300 WHERE `id` = 8107 AND `life` = 4202; -- ThunderApeMsgr L57: 4202 -> 6300
UPDATE `cq_monstertype` SET `life` = 9500 WHERE `id` = 8108 AND `life` = 6337; -- SnakemanMsgr L62: 6337 -> 9500
UPDATE `cq_monstertype` SET `life` = 11000 WHERE `id` = 8109 AND `life` = 7337; -- SandMonsterMsgr L67: 7337 -> 11000
UPDATE `cq_monstertype` SET `life` = 14000 WHERE `id` = 8110 AND `life` = 9338; -- HillMonsterMsgr L72: 9338 -> 14000
UPDATE `cq_monstertype` SET `life` = 17000 WHERE `id` = 8111 AND `life` = 11339; -- RockMonsterMsgr L77: 11339 -> 17000
UPDATE `cq_monstertype` SET `life` = 22000 WHERE `id` = 8112 AND `life` = 14674; -- BladeGhostMsgr L82: 14674 -> 22000
UPDATE `cq_monstertype` SET `life` = 26000 WHERE `id` = 8113 AND `life` = 17342; -- BirdmanMsgr L87: 17342 -> 26000
UPDATE `cq_monstertype` SET `life` = 30000 WHERE `id` = 8114 AND `life` = 20010; -- HawkMessenger L92: 20010 -> 30000
UPDATE `cq_monstertype` SET `life` = 40000 WHERE `id` = 8115 AND `life` = 26680; -- RobberMessenger L97: 26680 -> 40000
UPDATE `cq_monstertype` SET `life` = 1100 WHERE `id` = 8200 AND `life` = 734; -- HumanAide L22: 734 -> 1100
UPDATE `cq_monstertype` SET `life` = 1200 WHERE `id` = 8201 AND `life` = 800; -- WingedSnakeAide L27: 800 -> 1200
UPDATE `cq_monstertype` SET `life` = 1600 WHERE `id` = 8202 AND `life` = 1067; -- BanditAide L32: 1067 -> 1600
UPDATE `cq_monstertype` SET `life` = 2000 WHERE `id` = 8203 AND `life` = 1334; -- RatAide L37: 1334 -> 2000
UPDATE `cq_monstertype` SET `life` = 4400 WHERE `id` = 8205 AND `life` = 2935; -- MonkeyAide L47: 2935 -> 4400
UPDATE `cq_monstertype` SET `life` = 5300 WHERE `id` = 8206 AND `life` = 3535; -- GiantApeAide L52: 3535 -> 5300
UPDATE `cq_monstertype` SET `life` = 6300 WHERE `id` = 8207 AND `life` = 4202; -- ThunderApeAide L57: 4202 -> 6300
UPDATE `cq_monstertype` SET `life` = 9500 WHERE `id` = 8208 AND `life` = 6337; -- SnakemanAide L62: 6337 -> 9500
UPDATE `cq_monstertype` SET `life` = 11000 WHERE `id` = 8209 AND `life` = 7337; -- SandMonsterAide L67: 7337 -> 11000
UPDATE `cq_monstertype` SET `life` = 14000 WHERE `id` = 8210 AND `life` = 9338; -- HillMonsterAide L72: 9338 -> 14000
UPDATE `cq_monstertype` SET `life` = 17000 WHERE `id` = 8211 AND `life` = 11339; -- RockMonsterAide L77: 11339 -> 17000
UPDATE `cq_monstertype` SET `life` = 22000 WHERE `id` = 8212 AND `life` = 14674; -- BladeGhostAide L82: 14674 -> 22000
UPDATE `cq_monstertype` SET `life` = 26000 WHERE `id` = 8213 AND `life` = 17342; -- BirdmanAide L87: 17342 -> 26000
UPDATE `cq_monstertype` SET `life` = 30000 WHERE `id` = 8214 AND `life` = 20010; -- HawkAide L92: 20010 -> 30000
UPDATE `cq_monstertype` SET `life` = 40000 WHERE `id` = 8215 AND `life` = 26680; -- RobberAide L97: 26680 -> 40000
UPDATE `cq_monstertype` SET `life` = 30000 WHERE `id` = 8300 AND `life` = 1334; -- GhostKing L22: 1334 -> 30000
UPDATE `cq_monstertype` SET `life` = 40000 WHERE `id` = 8301 AND `life` = 26680; -- WingedSnakeKing L27: 26680 -> 40000
UPDATE `cq_monstertype` SET `life` = 45000 WHERE `id` = 8302 AND `life` = 30015; -- BanditKing L32: 30015 -> 45000
UPDATE `cq_monstertype` SET `life` = 45000 WHERE `id` = 8303 AND `life` = 30015; -- RatKing L37: 30015 -> 45000
UPDATE `cq_monstertype` SET `life` = 45000 WHERE `id` = 8305 AND `life` = 30015; -- MonkeyKing L47: 30015 -> 45000
UPDATE `cq_monstertype` SET `life` = 45000 WHERE `id` = 8306 AND `life` = 30015; -- GiantApeKing L52: 30015 -> 45000
UPDATE `cq_monstertype` SET `life` = 45000 WHERE `id` = 8307 AND `life` = 30015; -- ThunderApeKing L57: 30015 -> 45000
UPDATE `cq_monstertype` SET `life` = 46000 WHERE `id` = 8308 AND `life` = 30682; -- SnakemanKing L62: 30682 -> 46000
UPDATE `cq_monstertype` SET `life` = 47000 WHERE `id` = 8309 AND `life` = 31349; -- SandMonsterKing L67: 31349 -> 47000
UPDATE `cq_monstertype` SET `life` = 47000 WHERE `id` = 8310 AND `life` = 31349; -- HillMonsterKing L72: 31349 -> 47000
UPDATE `cq_monstertype` SET `life` = 48000 WHERE `id` = 8311 AND `life` = 32016; -- RockMonsterKing L77: 32016 -> 48000
UPDATE `cq_monstertype` SET `life` = 50000 WHERE `id` = 8312 AND `life` = 33350; -- BladeGhostKing L82: 33350 -> 50000
UPDATE `cq_monstertype` SET `life` = 60460 WHERE `id` = 8313 AND `life` = 40327; -- BirdmanKing L87: 40327 -> 60460
UPDATE `cq_monstertype` SET `life` = 65535 WHERE `id` = 8314 AND `life` = 43712; -- HawkBoss L92: 43712 -> 65535
UPDATE `cq_monstertype` SET `life` = 65535 WHERE `id` = 8315 AND `life` = 43712; -- RobberKing L97: 43712 -> 65535
UPDATE `cq_monstertype` SET `life` = 183 WHERE `id` = 4835 AND `life` = 122; -- Robin L12: 122 -> 183
UPDATE `cq_monstertype` SET `life` = 303 WHERE `id` = 4836 AND `life` = 202; -- Apparition L17: 202 -> 303
UPDATE `cq_monstertype` SET `life` = 667 WHERE `id` = 4838 AND `life` = 445; -- WingedSnake L27: 445 -> 667
UPDATE `cq_monstertype` SET `life` = 817 WHERE `id` = 4839 AND `life` = 545; -- Bandit L32: 545 -> 817
UPDATE `cq_monstertype` SET `life` = 999 WHERE `id` = 4840 AND `life` = 666; -- Ratling L37: 666 -> 999
UPDATE `cq_monstertype` SET `life` = 1828 WHERE `id` = 4841 AND `life` = 1219; -- FireSpirit L42: 1219 -> 1828
UPDATE `cq_monstertype` SET `life` = 2199 WHERE `id` = 4842 AND `life` = 1467; -- Macaque L47: 1467 -> 2199
UPDATE `cq_monstertype` SET `life` = 2643 WHERE `id` = 4843 AND `life` = 1763; -- GiantApe L52: 1763 -> 2643
UPDATE `cq_monstertype` SET `life` = 3184 WHERE `id` = 4844 AND `life` = 2124; -- ThunderApe L57: 2124 -> 3184
UPDATE `cq_monstertype` SET `life` = 4771 WHERE `id` = 4845 AND `life` = 3182; -- Snakeman L62: 3182 -> 4771
UPDATE `cq_monstertype` SET `life` = 5760 WHERE `id` = 4846 AND `life` = 3842; -- SandMonster L67: 3842 -> 5760
UPDATE `cq_monstertype` SET `life` = 7358 WHERE `id` = 4847 AND `life` = 4908; -- HillMonster L72: 4908 -> 7358
UPDATE `cq_monstertype` SET `life` = 8459 WHERE `id` = 4848 AND `life` = 5642; -- RockMonster L77: 5642 -> 8459
UPDATE `cq_monstertype` SET `life` = 11673 WHERE `id` = 4849 AND `life` = 7786; -- BladeGhost L82: 7786 -> 11673
UPDATE `cq_monstertype` SET `life` = 13422 WHERE `id` = 4850 AND `life` = 8952; -- Birdman L87: 8952 -> 13422
UPDATE `cq_monstertype` SET `life` = 26844 WHERE `id` = 4852 AND `life` = 17905; -- BirdmanL88 L88: 17905 -> 26844
UPDATE `cq_monstertype` SET `life` = 30864 WHERE `id` = 4853 AND `life` = 20586; -- HawkL93 L93: 20586 -> 30864
UPDATE `cq_monstertype` SET `life` = 41394 WHERE `id` = 4854 AND `life` = 27610; -- BanditL98 L98: 27610 -> 41394
UPDATE `cq_monstertype` SET `life` = 7929 WHERE `id` = 4870 AND `life` = 5289; -- GiantApeL53 L53: 5289 -> 7929
UPDATE `cq_monstertype` SET `life` = 22074 WHERE `id` = 4871 AND `life` = 14723; -- HillMonsterL73 L73: 14723 -> 22074
UPDATE `cq_monstertype` SET `life` = 20697 WHERE `id` = 4874 AND `life` = 13805; -- BanditL97 L97: 13805 -> 20697

COMMIT;

-- 111 rows raised.
--
-- Left alone (sandbox HP already higher than Classic's client value):
--   (The 100x rows are boss versions on the sandbox; Classic has the same art at normal HP.)
--   30 SmartMonkey L50: sandbox 219900, Classic 2199
--   31 RoaringApe L55: sandbox 264300, Classic 2643
--   32 SnowApe L60: sandbox 318400, Classic 3184
--   33 CrazySnake L65: sandbox 477100, Classic 4771
--   34 SandDevil L70: sandbox 576000, Classic 5760
--   35 MountMonster L75: sandbox 735800, Classic 7358
--   37 BladeMonster L85: sandbox 1167300, Classic 11673
--   38 BirdKing L90: sandbox 1342200, Classic 13422
--   39 SilverHawk L95: sandbox 1543200, Classic 15432
--   40 Robber L100: sandbox 2069700, Classic 20697
--   90 FlyingBat L105: sandbox 2447200, Classic 24472
--   91 BloodyKing L110: sandbox 2814000, Classic 28140
--   92 BullDevil L115: sandbox 3505000, Classic 35050
--   93 GreenDevil L120: sandbox 6046000, Classic 60460
--   2034 NightDevil L120: sandbox 100000, Classic 60460
--   4857 BullMonsterL113 L113: sandbox 99295, Classic 65535
