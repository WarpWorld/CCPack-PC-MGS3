using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ConnectorLib;
using ConnectorLib.Inject.VersionProfiles;
using ConnectorLib.Memory;
using CrowdControl.Common;
using Log = CrowdControl.Common.Log;
using ConnectorType = CrowdControl.Common.ConnectorType;
using AddressChain = ConnectorLib.Inject.AddressChaining.AddressChain;
using System.Diagnostics.CodeAnalysis;

namespace CrowdControl.Games.Packs.MGS3;

[SuppressMessage("ReSharper", "StringLiteralTypo")]
public class MGS3 : InjectEffectPack
{
    public override Game Game { get; } = new("METAL GEAR SOLID3", "MGS3", "PC", ConnectorType.PCConnector);

    public MGS3(UserRecord player, Func<CrowdControlBlock, bool> responseHandler, Action<object> statusUpdateHandler)
        : base(player, responseHandler, statusUpdateHandler)
    {
        VersionProfiles = [new("METAL GEAR SOLID3", InitGame, DeinitGame)];
    }

    #region AddressChains

    // Weapon Ammo and Item Base Addresses points to NONE Weapon and NONE Item
    private AddressChain baseWeaponAddress;
    private AddressChain baseItemAddress;
    
    // Snake's Animations
    private AddressChain snakeQuickSleep;
    private AddressChain snakePukeFire;
    private AddressChain snakeBunnyHop;
    private AddressChain snakeFreeze;
    private AddressChain snakeYcoordinate;

    // Snake's Stats
    private AddressChain snakeStamina;
    private AddressChain snakeCurrentEquippedWeapon;
    private AddressChain snakeCurrentEquippedItem;
    private AddressChain snakeCurrentCamo;
    private AddressChain snakeCurrentFacePaint;
    private AddressChain snakeCommonCold;
    private AddressChain snakePoison;
    private AddressChain snakeFoodPoisoning;
    private AddressChain snakeHasLeeches;
    private AddressChain snakeDamageMultiplierInstructions;
    private AddressChain snakeDamageMultiplierValue;

    // Game State
    private AddressChain isPausedOrMenu;
    private AddressChain alertStatus;

    // HUD and Filters
    private AddressChain hudPartiallyRemoved;
    private AddressChain hudFullyRemoved;
    private AddressChain fieldOfView;
    private AddressChain lightNearSnake;
    private AddressChain mapColour;
    private AddressChain skyColour;
    private AddressChain skyValue;

    // Guard Health, Sleep & Stun Statues
    // Lethal Damage
    private AddressChain guardWpNadeDamage;
    private AddressChain guardShotgunDamage;
    private AddressChain guardM63Damage;
    private AddressChain guardKnifeForkDamage;
    private AddressChain guardMostWeaponsDamage;
    private AddressChain guardExplosionDamage;
    private AddressChain guardThroatSlitDamage;

    // Sleep Damage
    private AddressChain guardZzzDrain;
    private AddressChain guardSleepStatus1;
    private AddressChain guardSleepStatus2;
    private AddressChain guardZzzWeaponsDamage;

    // Stun Damage
    private AddressChain guardCqcSlamVeryEasytoHardDifficulty;
    private AddressChain guardCqcSlamExtremeDifficulty;
    private AddressChain guardRollDamage;
    private AddressChain guardStunGrenadeDamage;
    private AddressChain guardPunchDamage;

    #endregion

    #region [De]init

    private void InitGame()
    {

        Connector.PointerFormat = PointerFormat.Absolute64LE;

        /* Made a class to use offsets from the base address then add 80 bytes
           (0x50) to get to the next weapon to cut down on overall code */
        baseWeaponAddress = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D2C16C");
        baseItemAddress = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D2EA5C");

        // Snake Animations to test
        snakeQuickSleep = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E14C3B");
        snakePukeFire = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E14C3C");
        snakeBunnyHop = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E14C48");
        snakeFreeze = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E14C4C");
        snakeYcoordinate = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E14BF0=>+134");

        // Snake Stats
        snakeStamina = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+ACBE18=>+A4A");
        snakeCommonCold = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+ACBE18=>+688");
        snakePoison = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+ACBE18=>+696");
        snakeFoodPoisoning = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+ACBE18=>+6A4");
        snakeHasLeeches = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+ACBE18=>+6B2");
        snakeCurrentEquippedWeapon = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+ACBE18=>+5D4");
        snakeCurrentEquippedItem = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+ACBE18=>+5D6");
        snakeCurrentCamo = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+ACBE18=>+67E"); // Exceeding 31 will crash the game
        snakeCurrentFacePaint = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+ACBE18=>+67F"); // Exceeding 22 will crash the game
        snakeDamageMultiplierInstructions = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+335AE9");
        snakeDamageMultiplierValue = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+335AEB");


        // Game State
        isPausedOrMenu = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D76E9C");
        // 16 = Alert, 32 = Caution, 0 = No Alert
        alertStatus = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D84F38");


        // HUD and Filters
        hudPartiallyRemoved = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D558DD");
        hudFullyRemoved = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D558DC");
        fieldOfView = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+AE737");
        lightNearSnake = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D5432D");
        mapColour = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D54324");
        skyColour = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D54320");
        skyValue = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D5431C");

        // Guard Health, Sleep & Stun Statues
        // Lethal Damage
        guardWpNadeDamage = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1BC456");
        guardShotgunDamage = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1CECFD");
        guardM63Damage = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1CED3C");
        guardKnifeForkDamage = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1CEE81");
        guardMostWeaponsDamage = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1CF40F");
        guardExplosionDamage = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1CF505");
        guardThroatSlitDamage = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1F2EAD");

        // Sleep Damage
        guardZzzDrain = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1BDC46");
        guardSleepStatus1 = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1CF3B2");
        guardSleepStatus2 = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1CF35E");
        guardZzzWeaponsDamage = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D1941");

        // Stun Damage
        guardCqcSlamVeryEasytoHardDifficulty = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1BC3E7");
        guardCqcSlamExtremeDifficulty = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1BC3F2");
        guardRollDamage = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1CF002");
        guardStunGrenadeDamage = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1CEDB2");
        guardPunchDamage = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1CF636");
    }

    private void DeinitGame()
    {
    }

    #endregion

    #region Weapon and Items Class

    public abstract class GameObject
    {
        public string Name { get; set; }
    }

    public abstract class WeaponItemManager : GameObject
    {
        public int Index { get; set; }
        public bool HasAmmo { get; set; }
        public bool HasClip { get; set; }
        public bool HasSuppressor { get; set; }

        protected WeaponItemManager(string name, int index)
        {
            Name = name;
            Index = index;
        }
    }

    public class Weapon : WeaponItemManager
    {
        public Weapon(string name, int index, bool hasAmmo = false, bool hasClip = false, bool hasSuppressor = false)
            : base(name, index)
        {
            HasAmmo = hasAmmo;
            HasClip = hasClip;
            HasSuppressor = hasSuppressor;
        }

        /* I made this method to avoid having to make a new effect to for each weapon and each property
           (ammo/max ammo/clip/max clip/suppressor) of the weapon as it might be overwhleming to the user */
        public AddressChain GetPropertyAddress(AddressChain baseWeaponAddress, int propertyOffset)
        {
            int totalOffset = (WeaponAddresses.WeaponOffset * Index) + propertyOffset;
            return baseWeaponAddress.Offset(totalOffset);
        }
    }

    public class Item : WeaponItemManager
    {
        public Item(string name, int index)
            : base(name, index)
        {
        }

        public AddressChain GetPropertyAddress(AddressChain baseItemAddress, int propertyOffset)
        {
            int totalOffset = (ItemAddresses.ItemOffset * Index) + propertyOffset;
            return baseItemAddress.Offset(totalOffset);
        }
    }

    public static class WeaponAddresses
    {
        public const int WeaponOffset = 0x50;
        public const int CurrentAmmoOffset = 0x0;
        public const int MaxAmmoOffset = 0x2;
        public const int ClipOffset = 0x4;
        public const int MaxClipOffset = 0x6;
        public const int SuppressorToggleOffset = 0x10;
    }

    public static class ItemAddresses
    {
        public const int ItemOffset = 0x50;
        public const int CurrentCapacityOffset = 0x0;
        public const int MaxCapacityOffset = 0x2;
    }


    public static class MGS3UsableObjects
    {
        public static readonly Weapon NoneWeapon = new Weapon("None Weapon", 0);
        public static readonly Weapon SurvivalKnife = new Weapon("Survival Knife", 1);
        public static readonly Weapon Fork = new Weapon("Fork", 2);
        public static readonly Weapon CigSpray = new Weapon("Cig Spray", 3, hasAmmo: true);
        public static readonly Weapon Handkerchief = new Weapon("Handkerchief", 4, hasAmmo: true);
        public static readonly Weapon MK22 = new Weapon("MK22", 5, hasAmmo: true, hasClip: true, hasSuppressor: true);
        public static readonly Weapon M1911A1 = new Weapon("M1911A1", 6, hasAmmo: true, hasClip: true, hasSuppressor: true);
        public static readonly Weapon EzGun = new Weapon("EZ Gun", 7);
        public static readonly Weapon SAA = new Weapon("SAA", 8, hasAmmo: true, hasClip: true);
        public static readonly Weapon Patriot = new Weapon("Patriot", 9);
        public static readonly Weapon Scorpion = new Weapon("Scorpion", 10, hasAmmo: true, hasClip: true);
        public static readonly Weapon XM16E1 = new Weapon("XM16E1", 11, hasAmmo: true, hasClip: true, hasSuppressor: true);
        public static readonly Weapon AK47 = new Weapon("AK47", 12, hasAmmo: true, hasClip: true);
        public static readonly Weapon M63 = new Weapon("M63", 13, hasAmmo: true, hasClip: true);
        public static readonly Weapon M37 = new Weapon("M37", 14, hasAmmo: true, hasClip: true);
        public static readonly Weapon SVD = new Weapon("SVD", 15, hasAmmo: true, hasClip: true);
        public static readonly Weapon MosinNagant = new Weapon("Mosin-Nagant", 16, hasAmmo: true, hasClip: true);
        public static readonly Weapon RPG7 = new Weapon("RPG-7", 17, hasAmmo: true, hasClip: true);
        public static readonly Weapon Torch = new Weapon("Torch", 18);
        public static readonly Weapon Grenade = new Weapon("Grenade", 19, hasAmmo: true);
        public static readonly Weapon WpGrenade = new Weapon("WP Grenade", 20, hasAmmo: true);
        public static readonly Weapon StunGrenade = new Weapon("Stun Grenade", 21, hasAmmo: true);
        public static readonly Weapon ChaffGrenade = new Weapon("Chaff Grenade", 22, hasAmmo: true);
        public static readonly Weapon SmokeGrenade = new Weapon("Smoke Grenade", 23, hasAmmo: true);
        public static readonly Weapon EmptyMagazine = new Weapon("Empty Magazine", 24, hasAmmo: true);
        public static readonly Weapon TNT = new Weapon("TNT", 25, hasAmmo: true);
        public static readonly Weapon C3 = new Weapon("C3", 26, hasAmmo: true);
        public static readonly Weapon Claymore = new Weapon("Claymore", 27, hasAmmo: true);
        public static readonly Weapon Book = new Weapon("Book", 28, hasAmmo: true);
        public static readonly Weapon Mousetrap = new Weapon("Mousetrap", 29, hasAmmo: true);
        public static readonly Weapon DirectionalMic = new Weapon("Directional Microphone", 30);

        public static readonly Dictionary<int, Weapon> AllWeapons = new Dictionary<int, Weapon>
        {
            { NoneWeapon.Index, NoneWeapon },
            { SurvivalKnife.Index, SurvivalKnife },
            { Fork.Index, Fork },
            { CigSpray.Index, CigSpray },
            { Handkerchief.Index, Handkerchief },
            { MK22.Index, MK22 },
            { M1911A1.Index, M1911A1 },
            { EzGun.Index, EzGun },
            { SAA.Index, SAA },
            { Patriot.Index, Patriot },
            { Scorpion.Index, Scorpion },
            { XM16E1.Index, XM16E1 },
            { AK47.Index, AK47 },
            { M63.Index, M63 },
            { M37.Index, M37 },
            { SVD.Index, SVD },
            { MosinNagant.Index, MosinNagant },
            { RPG7.Index, RPG7 },
            { Torch.Index, Torch },
            { Grenade.Index, Grenade },
            { WpGrenade.Index, WpGrenade },
            { StunGrenade.Index, StunGrenade },
            { ChaffGrenade.Index, ChaffGrenade },
            { SmokeGrenade.Index, SmokeGrenade },
            { EmptyMagazine.Index, EmptyMagazine },
            { TNT.Index, TNT },
            { C3.Index, C3 },
            { Claymore.Index, Claymore },
            { Book.Index, Book },
            { Mousetrap.Index, Mousetrap },
            { DirectionalMic.Index, DirectionalMic }
        };

        public static readonly Item NoneItem = new Item("None Item", 0);
        public static readonly Item LifeMedicine = new Item("Life Medicine", 1);
        public static readonly Item Pentazemin = new Item("Pentazemin", 2);
        public static readonly Item FakeDeathPill = new Item("Fake Death Pill", 3);
        public static readonly Item RevivalPill = new Item("Revival Pill", 4);
        public static readonly Item Cigar = new Item("Cigar", 5);
        public static readonly Item Binoculars = new Item("Binoculars", 6);
        public static readonly Item ThermalGoggles = new Item("Thermal Goggles", 7);
        public static readonly Item NightVisionGoggles = new Item("Night Vision Goggles", 8);
        public static readonly Item Camera = new Item("Camera", 9);
        public static readonly Item MotionDetector = new Item("Motion Detector", 10);
        public static readonly Item ActiveSonar = new Item("Active Sonar", 11);
        public static readonly Item MineDetector = new Item("Mine Detector", 12);
        public static readonly Item AntiPersonnelSensor = new Item("Anti Personnel Sensor", 13);
        public static readonly Item CBoxA = new Item("CBox A", 14);
        public static readonly Item CBoxB = new Item("CBox B", 15);
        public static readonly Item CBoxC = new Item("CBox C", 16);
        public static readonly Item CBoxD = new Item("CBox D", 17);
        public static readonly Item CrocCap = new Item("Croc Cap", 18);
        public static readonly Item KeyA = new Item("Key A", 19);
        public static readonly Item KeyB = new Item("Key B", 20);
        public static readonly Item KeyC = new Item("Key C", 21);
        public static readonly Item Bandana = new Item("Bandana", 22);
        public static readonly Item StealthCamo = new Item("Stealth Camo", 23);
        public static readonly Item BugJuice = new Item("Bug Juice", 24);
        public static readonly Item MonkeyMask = new Item("Monkey Mask", 25);
        public static readonly Item Serum = new Item("Serum", 26);
        public static readonly Item Antidote = new Item("Antidote", 27);
        public static readonly Item ColdMedicine = new Item("Cold Medicine", 28);
        public static readonly Item DigestiveMedicine = new Item("Digestive Medicine", 29);
        public static readonly Item Ointment = new Item("Ointment", 30);
        public static readonly Item Splint = new Item("Splint", 31);
        public static readonly Item Disinfectant = new Item("Disinfectant", 32);
        public static readonly Item Styptic = new Item("Styptic", 33);
        public static readonly Item Bandage = new Item("Bandage", 34);
        public static readonly Item SutureKit = new Item("Suture Kit", 35);
        // This Knife is to be used for medical purposes but removing the knife as a weapon makes it disappear here too
        public static readonly Item Knife = new Item("Knife", 36);
        public static readonly Item Battery = new Item("Battery", 37);
        // These are for suppressor quantities but it being on/off is determined by the weapon attribute in the weapon class
        public static readonly Item M1911A1Suppressor = new Item("M1911A1 Suppressor", 38);
        public static readonly Item MK22Suppressor = new Item("MK22 Suppressor", 39);
        public static readonly Item XM16E1Suppressor = new Item("XM16E1 Suppressor", 40);
        // 0 for unacquired and 1 for acquired we check for this before changing the camo as
        // equippping an unacquired camo has a chance to crash the game
        public static readonly Item OliveDrab = new Item("Olive Drab", 41);
        public static readonly Item TigerStripe = new Item("Tiger Stripe", 42);
        public static readonly Item Leaf = new Item("Leaf", 43);
        public static readonly Item TreeBark = new Item("Tree Bark", 44);
        public static readonly Item ChocoChip = new Item("Choco Chip", 45);
        public static readonly Item Splitter = new Item("Splitter", 46);
        public static readonly Item Raindrop = new Item("Raindrop", 47);
        public static readonly Item Squares = new Item("Squares", 48);
        public static readonly Item Water = new Item("Water", 49);
        public static readonly Item Black = new Item("Black", 50);
        public static readonly Item Snow = new Item("Snow", 51);
        public static readonly Item Naked = new Item("Naked", 52);
        public static readonly Item SneakingSuit = new Item("Sneaking Suit", 53);
        public static readonly Item Scientist = new Item("Scientist", 54);
        public static readonly Item Officer = new Item("Officer", 55);
        public static readonly Item Maintenance = new Item("Maintenance", 56);
        public static readonly Item Tuxedo = new Item("Tuxedo", 57);
        public static readonly Item HornetStripe = new Item("Hornet Stripe", 58);
        public static readonly Item Spider = new Item("Spider", 59);
        public static readonly Item Moss = new Item("Moss", 60);
        public static readonly Item Fire = new Item("Fire", 61);
        public static readonly Item Spirit = new Item("Spirit", 62);
        public static readonly Item ColdWar = new Item("Cold War", 63);
        public static readonly Item Snake = new Item("Snake", 64);
        public static readonly Item GaKo = new Item("Ga-Ko", 65);
        public static readonly Item DesertTiger = new Item("Desert Tiger", 66);
        public static readonly Item DPM = new Item("DPM", 67);
        public static readonly Item Flecktarn = new Item("Flecktarn", 68);
        public static readonly Item Auscam = new Item("Auscam", 69);
        public static readonly Item Animals = new Item("Animals", 70);
        public static readonly Item Fly = new Item("Fly", 71);
        public static readonly Item BananaCamo = new Item("Banana Camo", 72);
        public static readonly Item Downloaded = new Item("Downloaded", 73);
        public static readonly Item NoPaint = new Item("No Paint", 74);
        public static readonly Item Woodland = new Item("Woodland", 75);
        public static readonly Item BlackFacePaint = new Item("Black", 76);
        public static readonly Item WaterFacePaint = new Item("Water", 77);
        public static readonly Item DesertFacePaint = new Item("Desert", 78);
        public static readonly Item SplitterFacePaint = new Item("Splitter", 79);
        public static readonly Item SnowFacePaint = new Item("Snow", 80);
        public static readonly Item Kabuki = new Item("Kabuki", 81);
        public static readonly Item Zombie = new Item("Zombie", 82);
        public static readonly Item Oyama = new Item("Oyama", 83);
        public static readonly Item Mask = new Item("Mask", 84);
        public static readonly Item GreenFacePaint = new Item("Green", 85);
        public static readonly Item BrownFacePaint = new Item("Brown", 86);
        public static readonly Item Infinity = new Item("Infinity", 87);
        public static readonly Item SovietUnion = new Item("Soviet Union", 88);
        public static readonly Item UK = new Item("UK", 89);
        public static readonly Item France = new Item("France", 90);
        public static readonly Item Germany = new Item("Germany", 91);
        public static readonly Item Italy = new Item("Italy", 92);
        public static readonly Item Spain = new Item("Spain", 93);
        public static readonly Item Sweden = new Item("Sweden", 94);
        public static readonly Item Japan = new Item("Japan", 95);
        public static readonly Item USA = new Item("USA", 96);

        public static readonly Dictionary<int, Item> AllItems = new Dictionary<int, Item>
        {
            { NoneItem.Index, NoneItem },
            { LifeMedicine.Index, LifeMedicine },
            { Pentazemin.Index, Pentazemin },
            { FakeDeathPill.Index, FakeDeathPill },
            { RevivalPill.Index, RevivalPill },
            { Cigar.Index, Cigar },
            { Binoculars.Index, Binoculars },
            { ThermalGoggles.Index, ThermalGoggles },
            { NightVisionGoggles.Index, NightVisionGoggles },
            { Camera.Index, Camera },
            { MotionDetector.Index, MotionDetector },
            { ActiveSonar.Index, ActiveSonar },
            { MineDetector.Index, MineDetector },
            { AntiPersonnelSensor.Index, AntiPersonnelSensor },
            { CBoxA.Index, CBoxA },
            { CBoxB.Index, CBoxB },
            { CBoxC.Index, CBoxC },
            { CBoxD.Index, CBoxD },
            { CrocCap.Index, CrocCap },
            { KeyA.Index, KeyA },
            { KeyB.Index, KeyB },
            { KeyC.Index, KeyC },
            { Bandana.Index, Bandana },
            { StealthCamo.Index, StealthCamo },
            { BugJuice.Index, BugJuice },
            { MonkeyMask.Index, MonkeyMask },
            { Serum.Index, Serum },
            { Antidote.Index, Antidote },
            { ColdMedicine.Index, ColdMedicine },
            { DigestiveMedicine.Index, DigestiveMedicine },
            { Ointment.Index, Ointment },
            { Splint.Index, Splint },
            { Disinfectant.Index, Disinfectant },
            { Styptic.Index, Styptic },
            { Bandage.Index, Bandage },
            { SutureKit.Index, SutureKit },
            { Knife.Index, Knife },
            { Battery.Index, Battery },
            { M1911A1Suppressor.Index, M1911A1Suppressor },
            { MK22Suppressor.Index, MK22Suppressor },
            { XM16E1Suppressor.Index, XM16E1Suppressor },
            { OliveDrab.Index, OliveDrab },
            { TigerStripe.Index, TigerStripe },
            { Leaf.Index, Leaf },
            { TreeBark.Index, TreeBark },
            { ChocoChip.Index, ChocoChip },
            { Splitter.Index, Splitter },
            { Raindrop.Index, Raindrop },
            { Squares.Index, Squares },
            { Water.Index, Water },
            { Black.Index, Black },
            { Snow.Index, Snow },
            { Naked.Index, Naked },
            { SneakingSuit.Index, SneakingSuit },
            { Scientist.Index, Scientist },
            { Officer.Index, Officer },
            { Maintenance.Index, Maintenance },
            { Tuxedo.Index, Tuxedo },
            { HornetStripe.Index, HornetStripe },
            { Spider.Index, Spider },
            { Moss.Index, Moss },
            { Fire.Index, Fire },
            { Spirit.Index, Spirit },
            { ColdWar.Index, ColdWar },
            { Snake.Index, Snake },
            { GaKo.Index, GaKo },
            { DesertTiger.Index, DesertTiger },
            { DPM.Index, DPM },
            { Flecktarn.Index, Flecktarn },
            { Auscam.Index, Auscam },
            { Animals.Index, Animals },
            { Fly.Index, Fly },
            { BananaCamo.Index, BananaCamo },
            { Downloaded.Index, Downloaded },
            { NoPaint.Index, NoPaint },
            { Woodland.Index, Woodland },
            { BlackFacePaint.Index, BlackFacePaint },
            { WaterFacePaint.Index, WaterFacePaint },
            { DesertFacePaint.Index, DesertFacePaint },
            { SplitterFacePaint.Index, SplitterFacePaint },
            { SnowFacePaint.Index, SnowFacePaint },
            { Kabuki.Index, Kabuki },
            { Zombie.Index, Zombie },
            { Oyama.Index, Oyama },
            { Mask.Index, Mask },
            { GreenFacePaint.Index, GreenFacePaint },
            { BrownFacePaint.Index, BrownFacePaint },
            { Infinity.Index, Infinity },
            { SovietUnion.Index, SovietUnion },
            { UK.Index, UK },
            { France.Index, France },
            { Germany.Index, Germany },
            { Italy.Index, Italy },
            { Spain.Index, Spain },
            { Sweden.Index, Sweden },
            { Japan.Index, Japan },
            { USA.Index, USA }
        };
    }

    #endregion

    #region Enums

    /* Uniform and Face paint redeclared here as the number they use in the class for if it's
       acquired or not is different than what is for when equipped by Snake ingame This version 
       doesn't utilize any of these or Facepaint, as I was worried about cluttering with 60+ effects 
       that only revolve around camo but it will in an update after testing */
    public enum SnakesUniformCamo
    {
        OliveDrab = 0,
        TigerStripe = 1,
        Leaf = 2,
        TreeBark = 3,
        ChocoChip = 4,
        Splitter = 5,
        Raindrop = 6,
        Squares = 7,
        Water = 8,
        Black = 9,
        Snow = 10,
        Naked = 11,
        SneakingSuit = 12,
        Scientist = 13,
        Officer = 14,
        Maintenance = 15,
        Tuxedo = 16,
        HornetStripe = 17,
        Spider = 18,
        Moss = 19,
        Fire = 20,
        Spirit = 21,
        ColdWar = 22,
        Snake = 23,
        GaKo = 24,
        DesertTiger = 25,
        DPM = 26,
        Flecktarn = 27,
        Auscam = 28,
        Animals = 29,
        Fly = 30,
        BananaCamo = 31
    }

    public enum SnakesFacePaint
    {
        NoPaint = 0,
        Woodland = 1,
        Black = 2,
        Water = 3,
        Desert = 4,
        Splitter = 5,
        Snow = 6,
        Kabuki = 7,
        Zombie = 8,
        Oyama = 9,
        Mask = 10,
        Green = 11,
        Brown = 12,
        Infinity = 13,
        SovietUnion = 14,
        UK = 15,
        France = 16,
        Germany = 17,
        Italy = 18,
        Spain = 19,
        Sweden = 20,
        Japan = 21,
        USA = 22
    }

    public enum AlertModes
    {
        Normal = 0,
        Alert = 16,
        Caution = 32,
        Evasion = 128
    }

    #endregion

    #region Memory Getters and Setters

    byte Get8(AddressChain addr)
    {
        return addr.GetByte();
    }

    void Set8(AddressChain addr, byte val)
    {
        addr.SetByte(val);
    }

    short Get16(AddressChain addr)
    {
        return BitConverter.ToInt16(addr.GetBytes(2), 0);
    }

    void Set16(AddressChain addr, short val)
    {
        addr.SetBytes(BitConverter.GetBytes(val));
    }

    int Get32(AddressChain addr)
    {
        return BitConverter.ToInt32(addr.GetBytes(4), 0);
    }

    void Set32(AddressChain addr, int val)
    {
        addr.SetBytes(BitConverter.GetBytes(val));
    }

    float GetFloat(AddressChain addr)
    {
        if (addr.TryGetBytes(4, out byte[] bytes))
        {
            return BitConverter.ToSingle(bytes, 0);
        }
        else
        {
            throw new Exception("Failed to read float value.");
        }
    }

    void SetFloat(AddressChain addr, float val)
    {
        byte[] bytes = BitConverter.GetBytes(val);
        addr.SetBytes(bytes);
    }

    T[] GetArray<T>(AddressChain addr, int count) where T : struct
    {
        int typeSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
        int totalSize = typeSize * count;
        byte[] bytes = addr.GetBytes(totalSize);

        T[] array = new T[count];
        Buffer.BlockCopy(bytes, 0, array, 0, totalSize);
        return array;
    }

    void SetArray<T>(AddressChain addr, T[] values) where T : struct
    {
        int typeSize = System.Runtime.InteropServices.Marshal.SizeOf<T>();
        int totalSize = typeSize * values.Length;
        byte[] bytes = new byte[totalSize];
        Buffer.BlockCopy(values, 0, bytes, 0, totalSize);
        addr.SetBytes(bytes);
    }

    public static short SetSpecificBits(short currentValue, int startBit, int endBit, int valueToSet)
    {
        int maskLength = endBit - startBit + 1;
        int mask = ((1 << maskLength) - 1) << startBit;
        return (short)((currentValue & ~mask) | ((valueToSet << startBit) & mask));
    }

    #endregion

    #region Effect Helpers

    #region Weapons

    private Weapon GetCurrentEquippedWeapon()
    {
        byte weaponId = Get8(snakeCurrentEquippedWeapon);
        if (MGS3UsableObjects.AllWeapons.TryGetValue(weaponId, out Weapon weapon))
        {
            return weapon;
        }
        else
        {
            Log.Error($"Unknown weapon ID: {weaponId}");
            return null;
        }
    }

    private void SetSnakeCurrentWeaponToNone()
    {
        try
        {
            Log.Message("Attempting to set Snake's Current Weapon to None.");
            byte originalWeapon = Get8(snakeCurrentEquippedWeapon);
            Set8(snakeCurrentEquippedWeapon, (byte)MGS3UsableObjects.NoneWeapon.Index);
            byte newWeapon = Get8(snakeCurrentEquippedWeapon);
            Log.Message($"Original Weapon was {originalWeapon}, new Weapon is {newWeapon}.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Current Weapon: {ex.Message}");
        }
    }

    private bool TrySubtractAmmoFromCurrentWeapon(short amount)
    {
        try
        {
            Weapon weapon = GetCurrentEquippedWeapon();
            if (weapon == null || !weapon.HasAmmo)
            {
                Log.Message($"{weapon?.Name ?? "Unknown Weapon"} does not use ammo.");
                return false;
            }

            var ammoAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.CurrentAmmoOffset);
            short currentAmmo = Get16(ammoAddress);

            if (currentAmmo <= 0)
            {
                Log.Message($"{weapon.Name} has no ammo to subtract.");
                return false;
            }

            short newAmmo = (short)Math.Max(currentAmmo - amount, 0);

            if (newAmmo == currentAmmo)
            {
                Log.Message($"{weapon.Name} ammo cannot be reduced further.");
                return false;
            }

            Set16(ammoAddress, newAmmo);
            Log.Message($"Subtracted {amount} ammo from {weapon.Name}. Ammo: {currentAmmo} -> {newAmmo}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while subtracting ammo: {ex.Message}");
            return false;
        }
    }

    private bool TryAddAmmoToCurrentWeapon(short amount)
    {
        try
        {
            Weapon weapon = GetCurrentEquippedWeapon();
            if (weapon == null || !weapon.HasAmmo)
            {
                Log.Message($"{weapon?.Name ?? "Unknown Weapon"} does not use ammo.");
                return false;
            }

            var ammoAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.CurrentAmmoOffset);
            short currentAmmo = Get16(ammoAddress);
            short maxAmmo = Get16(weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.MaxAmmoOffset));

            if (currentAmmo >= maxAmmo)
            {
                Log.Message($"{weapon.Name} ammo is already full.");
                return false;
            }

            short newAmmo = (short)Math.Min(currentAmmo + amount, maxAmmo);

            if (newAmmo == currentAmmo)
            {
                Log.Message($"{weapon.Name} ammo cannot be increased further.");
                return false;
            }

            Set16(ammoAddress, newAmmo);
            Log.Message($"Added {amount} ammo to {weapon.Name}. Ammo: {currentAmmo} -> {newAmmo}");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while adding ammo: {ex.Message}");
            return false;
        }
    }

    private bool TryEmptyCurrentWeaponClip()
    {
        try
        {
            Weapon weapon = GetCurrentEquippedWeapon();
            if (weapon == null || !weapon.HasClip)
            {
                Log.Message($"{weapon?.Name ?? "Unknown Weapon"} does not have a clip.");
                return false;
            }

            var clipAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.ClipOffset);
            short currentClip = Get16(clipAddress);

            if (currentClip <= 0)
            {
                Log.Message($"{weapon.Name} clip is already empty.");
                return false;
            }

            Set16(clipAddress, 0);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while emptying clip: {ex.Message}");
            return false;
        }
    }

    private void EmptySnakeClipInLoop()
    {
        try
        {
            Weapon weapon = GetCurrentEquippedWeapon();
            if (weapon == null || !weapon.HasClip)
            {
                Log.Message($"{weapon?.Name ?? "Unknown Weapon"} does not have a clip.");
                return;
            }

            var clipAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.ClipOffset);
            Set16(clipAddress, 0);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while emptying clip: {ex.Message}");
        }
    }

    #endregion

    #region Items

    /* Will add to items after testing of current effects. Basically the same logic as weapons,
       but there's less interesting effects when it comes to items */

    #endregion

    #region Snake's Stats

    private void SetSnakeStamina()
    {
        try
        {
            Log.Message("Attempting to set Snake's Stamina to 0.");

            short originalStamina = Get16(snakeStamina);
            Set16(snakeStamina, 0);
            short newStamina = Get16(snakeStamina);

            Log.Message($"Original Stamina was {originalStamina}, new Stamina is {newStamina}.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Stamina: {ex.Message}");
        }
    }

    private void SetSnakeMaxStamina()
    {
        try
        {
            Log.Message($"Attempting to set Snake's Stamina to 30000.");
            short originalStamina = Get16(snakeStamina);
            Set16(snakeStamina, 30000);
            short newStamina = Get16(snakeStamina);
            Log.Message($"Original Stamina was {originalStamina}, new Stamina is {newStamina}.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Stamina: {ex.Message}");
        }
    }

    private void IncreaseSnakeYCoordBy2000()
    {
        try
        {
            Log.Message($"Attempting to increase Snake's Y coordinate by 2000.");
            float originalYCoord = GetFloat(snakeYcoordinate);
            SetFloat(snakeYcoordinate, originalYCoord + 2000);
            float newYCoord = GetFloat(snakeYcoordinate);
            Log.Message($"Original Y coordinate was {originalYCoord}, new Y coordinate is {newYCoord}.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while increasing Snake's Y coordinate: {ex.Message}");
        }
    }

    private void SnakeHasTheCommonCold()
    {
        try
        {
            Log.Message("Attempting to give Snake the common cold.");
            byte[] coldArray = new byte[] { 0, 0, 100, 0, 0, 0, 0, 0, 12, 4, 0, 0, 44, 1 };
            SetArray(snakeCommonCold, coldArray);
            Log.Message("Snake has the common cold.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while giving Snake the common cold: {ex.Message}");
        }
    }

    private void SnakeIsPoisoned()
    {
        try
        {
            Log.Message("Attempting to poison Snake.");
            byte[] poisonArray = new byte[] { 0, 0, 100, 0, 0, 0, 0, 0, 10, 2, 0, 0, 44, 1 };
            SetArray(snakePoison, poisonArray);
            Log.Message("Snake is poisoned.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while poisoning Snake: {ex.Message}");
        }
    }

    private void SnakeHasFoodPoisoning()
    {
        try
        {
            Log.Message("Attempting to give Snake food poisoning.");
            byte[] foodPoisoningArray = new byte[] { 10, 0, 100, 0, 10, 0, 0, 0, 13, 1, 0, 0, 43, 1 };
            SetArray(snakeFoodPoisoning, foodPoisoningArray);
            Log.Message("Snake has food poisoning.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while giving Snake food poisoning: {ex.Message}");
        }
    }

    private void SnakeHasLeeches()
    {
        try
        {
            Log.Message("Attempting to give Snake leeches.");
            byte[] leechesArray = new byte[] { 171, 255, 117, 255, 119, 0, 253, 127, 7, 0, 0, 0, 44, 1 };
            SetArray(snakeHasLeeches, leechesArray);
            Log.Message("Snake has leeches.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while giving Snake leeches: {ex.Message}");
        }
    }

    private void SetSnakeDamageMultiplierInstruction()
    {
        try
        {
            byte[] damageMultiplierInstruction = new byte[] { 0x66, 0xBD, 0x01, 0x00, 0x66, 0x0F, 0xAF, 0xCD, 0x90 };
            SetArray(snakeDamageMultiplierInstructions, damageMultiplierInstruction);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Damage Multiplier Instruction: {ex.Message}");
        }
    }

    private void SetSnakeDamageMultiplierValue(int value)
    {
        try
        {
            short originalValue = Get16(snakeDamageMultiplierValue);
            Set16(snakeDamageMultiplierValue, (short)value);
            short newValue = Get16(snakeDamageMultiplierValue);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Damage Multiplier Value: {ex.Message}");
        }
    }

    #endregion

    #region Guard Stats
    /* For all types of stats the default methods are used to
       restore the values after a timer runs out for an effect */

    #region Lethal Damage

    private void SetGuardLethalDamageInvincible()
    {
        try
        {
            SetArray(guardWpNadeDamage, new byte[] { 0xE8, 0x03, 0x00, 0x00 });
            SetArray(guardShotgunDamage, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Set16(guardM63Damage, 0);
            SetArray(guardKnifeForkDamage, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Set16(guardMostWeaponsDamage, 0);
            Set16(guardExplosionDamage, 0);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Lethal Damage to Invincible: {ex.Message}");
        }
    }

    private void SetGuardLethalDamageVeryStrong()
    {
        try
        {
            SetArray(guardWpNadeDamage, new byte[] { 0xFA, 0x00, 0x00, 0x00 });
            SetArray(guardShotgunDamage, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Set16(guardM63Damage, 100);
            SetArray(guardKnifeForkDamage, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Set16(guardMostWeaponsDamage, 100);
            Set16(guardExplosionDamage, 100);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Lethal Damage to Very Strong: {ex.Message}");
        }
    }

    private void SetGuardLethalDamageDefault()
    {
        try
        {
            SetArray(guardWpNadeDamage, new byte[] { 0x00, 0x00, 0x00, 0x00 });
            SetArray(guardShotgunDamage, new byte[] { 0x89, 0x8E, 0x38, 0x01, 0x00, 0x00 });
            Set16(guardM63Damage, 1000);
            SetArray(guardKnifeForkDamage, new byte[] { 0x29, 0x86, 0x38, 0x01, 0x00, 0x00 });
            Set16(guardMostWeaponsDamage, 1000);
            Set16(guardExplosionDamage, 1000);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Lethal Damage to Default: {ex.Message}");
        }
    }

    private void SetGuardLethalDamageVeryWeak()
    {
        try
        {
            SetArray(guardWpNadeDamage, new byte[] { 0x00, 0x00, 0x00, 0x00 });
            SetArray(guardShotgunDamage, new byte[] { 0x89, 0x8E, 0x38, 0x01, 0x00, 0x00 });
            Set16(guardM63Damage, 2500);
            SetArray(guardKnifeForkDamage, new byte[] { 0x29, 0x86, 0x38, 0x01, 0x00, 0x00 });
            Set16(guardMostWeaponsDamage, 2500);
            Set16(guardExplosionDamage, 2500);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Lethal Damage to Very Weak: {ex.Message}");
        }
    }

    private void SetGuardLethalDamageOneshot()
    {
        try
        {
            SetArray(guardWpNadeDamage, new byte[] { 0x00, 0x00, 0x00, 0x00 });
            SetArray(guardShotgunDamage, new byte[] { 0x89, 0x8E, 0x38, 0x01, 0x00, 0x00 });
            Set16(guardM63Damage, 30000);
            SetArray(guardKnifeForkDamage, new byte[] { 0x29, 0x86, 0x38, 0x01, 0x00, 0x00 });
            Set16(guardMostWeaponsDamage, 30000);
            Set16(guardExplosionDamage, 30000);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Lethal Damage to Oneshot: {ex.Message}");
        }
    }

    #endregion

    #region Sleep Damage

    private void SetGuardSleepDamageAlmostInvincible()
    {
        try
        {
            SetArray(guardZzzDrain, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            SetArray(guardSleepStatus1, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Set16(guardSleepStatus2, 0);
            Set16(guardZzzWeaponsDamage, 0);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Sleep Damage to Invincible: {ex.Message}");
        }
    }

    private void SetGuardSleepDamageVeryStrong()
    {
        try
        {
            SetArray(guardZzzDrain, new byte[] { 0x89, 0x87, 0x48, 0x01, 0x00, 0x00 });
            SetArray(guardSleepStatus1, new byte[] { 0x89, 0x86, 0x48, 0x01, 0x00, 0x00 });
            Set16(guardSleepStatus2, 1000);
            Set16(guardZzzWeaponsDamage, 1000);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Sleep Damage to Very Strong: {ex.Message}");
        }
    }

    private void SetGuardSleepDamageDefault()
    {
        try
        {
            SetArray(guardZzzDrain, new byte[] { 0x89, 0x87, 0x48, 0x01, 0x00, 0x00 });
            SetArray(guardSleepStatus1, new byte[] { 0x89, 0x86, 0x48, 0x01, 0x00, 0x00 });
            Set16(guardSleepStatus2, 4000);
            Set16(guardZzzWeaponsDamage, 4000);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Sleep Damage to Default: {ex.Message}");
        }
    }

    private void SetGuardSleepDamageVeryWeak()
    {
        try
        {
            SetArray(guardZzzDrain, new byte[] { 0x89, 0x87, 0x48, 0x01, 0x00, 0x00 });
            SetArray(guardSleepStatus1, new byte[] { 0x89, 0x86, 0x48, 0x01, 0x00, 0x00 });
            Set16(guardSleepStatus2, 8000);
            Set16(guardZzzWeaponsDamage, 8000);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Sleep Damage to Very Weak: {ex.Message}");
        }
    }

    private void SetGuardSleepDamageOneshot()
    {
        try
        {
            SetArray(guardZzzDrain, new byte[] { 0x89, 0x87, 0x48, 0x01, 0x00, 0x00 });
            SetArray(guardSleepStatus1, new byte[] { 0x89, 0x86, 0x48, 0x01, 0x00, 0x00 });
            Set16(guardSleepStatus2, 30000);
            Set16(guardZzzWeaponsDamage, 30000);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Sleep Damage to Oneshot: {ex.Message}");
        }
    }

    #endregion

    #region Stun Damage

    private void SetGuardStunAlmostDamageInvincible()
    {
        try
        {
            Set32(guardCqcSlamVeryEasytoHardDifficulty, 90000);
            Set32(guardCqcSlamExtremeDifficulty, 90000);
            SetArray(guardStunGrenadeDamage, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            SetArray(guardRollDamage, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Set8(guardPunchDamage, 0);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Stun Damage to Almost Invincible: {ex.Message}");
        }
    }

    private void SetGuardStunDamageVeryStrong()
    {
        try
        {
            Set32(guardCqcSlamVeryEasytoHardDifficulty, -1600);
            Set32(guardCqcSlamExtremeDifficulty, -1600);
            SetArray(guardStunGrenadeDamage, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Set8(guardPunchDamage, 232);
            SetArray(guardRollDamage, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Stun Damage to Very Strong: {ex.Message}");
        }
    }

    private void SetGuardStunDamageDefault()
    {
        try
        {
            Set32(guardCqcSlamVeryEasytoHardDifficulty, -90000);
            Set32(guardCqcSlamExtremeDifficulty, -36000);
            SetArray(guardStunGrenadeDamage, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
            Set8(guardPunchDamage, 1);
            SetArray(guardRollDamage, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Stun Damage to Default: {ex.Message}");
        }
    }

    private void SetGuardStunDamageVeryWeak()
    {
        try
        {
            Set32(guardCqcSlamVeryEasytoHardDifficulty, -99999);
            Set32(guardCqcSlamExtremeDifficulty, -99999);
            SetArray(guardStunGrenadeDamage, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
            Set8(guardPunchDamage, 4);
            SetArray(guardRollDamage, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Stun Damage to Very Weak: {ex.Message}");
        }
    }

    private void SetGuardStunDamageOneshot()
    {
        try
        {
            Set32(guardCqcSlamVeryEasytoHardDifficulty, -99999);
            Set32(guardCqcSlamExtremeDifficulty, -99999);
            SetArray(guardStunGrenadeDamage, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
            Set8(guardPunchDamage, 10);
            SetArray(guardRollDamage, new byte[] { 0x29, 0x86, 0x40, 0x01, 0x00, 0x00 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Guard Stun Damage to One Shot: {ex.Message}");
        }
    }

    #endregion

    #endregion

    #region Alert Status

    private void SetAlertStatus()
    {
        try
        {
            Log.Message("Forcing Alert status...");
            Set8(alertStatus, (byte)AlertModes.Alert);
            Log.Message("Alert status forced.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error forcing Alert: {ex.Message}");
        }
    }

    private void SetEvasionStatus()
    {
        try
        {
            short current = Get16(alertStatus);
            short cleared = SetSpecificBits(current, 6, 15, 400);
            Set16(alertStatus, cleared);

            short evasionValue = Get16(alertStatus);
            short newValue = SetSpecificBits(evasionValue, 5, 14, 596);
            Set16(alertStatus, newValue);

            Log.Message($"Successfully forced Evasion. Old short: {current}, new short: {newValue}.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while forcing Evasion: {ex.Message}");
        }
    }

    private void SetCautionStatus()
    {
        try
        {
            Log.Message("Forcing Caution status...");
            Set8(alertStatus, (byte)AlertModes.Caution);
            Log.Message("Caution status forced.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error forcing Caution: {ex.Message}");
        }
    }

    private void ClearAlertStatuses()
    {
        try
        {
            short currentValue = Get16(alertStatus);
            short modifiedValue = SetSpecificBits(currentValue, 6, 15, 400);
            Set16(alertStatus, modifiedValue);

            Log.Message("Cleared evasion/caution bits.");
        }
        catch (Exception ex)
        {
            Log.Error($"Error clearing evasion/caution: {ex.Message}");
        }
    }

    #endregion

    #region Hud and Filters

    private void SetTimeOfDayValues(byte[] lightNearSnakeValues, byte[] mapColourValues, byte[] skyColourValues, byte skyValueValue)
    {
        SetArray(lightNearSnake, lightNearSnakeValues);
        SetArray(mapColour, mapColourValues);
        SetArray(skyColour, skyColourValues);
        Set8(skyValue, skyValueValue);
    }

    public void SetToDayMode()
    {
        SetTimeOfDayValues(
            new byte[] { 0x40, 0x9C, 0xC5 },       // Light Near Snake
            new byte[] { 0x00, 0x5E, 0x72, 0x64 }, // Map Colour
            new byte[] { 0x00, 0xFF, 0xFF, 0x44 }, // Sky Colour
            0x05);                                 // Sky Value
    }

    public void SetToNightMode()
    {
        SetTimeOfDayValues(
            new byte[] { 0x40, 0x1C, 0xC8 },
            new byte[] { 0x00, 0x1B, 0x0A, 0x03 },
            new byte[] { 0x00, 0x1F, 0x13, 0x09 },
            0x0F);
    }

    public void RemovePartialHUD()
    {
        Set8(hudPartiallyRemoved, 1);
    }

    public void RestorePartialHUD()
    {
        Set8(hudPartiallyRemoved, 0);
    }

    public void RemoveFullHUD()
    {
        Set8(hudFullyRemoved, 0);
    }

    public void RestoreFullHUD()
    {
        Set8(hudFullyRemoved, 1);
    }

    #endregion

    #region Snake's Animations

    private void MakeSnakeQuickSleep()
    {
        try
        {
            Log.Message("Attempting to make Snake quick sleep.");
            Set8(snakeQuickSleep, 2);
            Log.Message("Snake is quick sleeping.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while making Snake quick sleep: {ex.Message}");
        }
    }

    private void MakeSnakePukeFire()
    {
        try
        {
            Log.Message("Attempting to make Snake puke fire.");
            Set8(snakePukeFire, 255);
            Log.Message("Snake is puking fire.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while making Snake puke fire: {ex.Message}");
        }
    }

    private void MakeSnakePuke()
    {
        try
        {
            Log.Message("Attempting to make Snake puke.");
            Set8(snakePukeFire, 1);
            Log.Message("Snake is puking.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while making Snake puke: {ex.Message}");
        }
    }

    private void SetSnakeOnFire()
    {
        try
        {
            Log.Message("Attempting to set Snake on fire.");
            Set8(snakePukeFire, 8);
            Log.Message("Snake is on fire.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake on fire: {ex.Message}");
        }
    }

    private void MakeSnakeBunnyHop()
    {
        try
        {
            Set8(snakeBunnyHop, 3);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while making Snake bunny hop: {ex.Message}");
        }
    }

    private void MakeSnakeFreeze()
    {
        try
        {
            Set8(snakeFreeze, 9);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while making Snake freeze: {ex.Message}");
        }
    }

    private void UnfreezeSnake()
    {
        try
        {
            Log.Message("Attempting to unfreeze Snake.");
            Set8(snakeFreeze, 0);
            Log.Message("Snake is no longer frozen.");
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while unfreezing Snake: {ex.Message}");
        }
    }

    #endregion

    #region Game State Tracking

    private bool IsReady(EffectRequest request)
    {
        try
        {
            byte gameState = Get8(isPausedOrMenu);
            if (gameState == 1)
            {
                Log.Message("Game is paused or on the radio.");
                return false;
            }

            else if (gameState == 4)
            {
                Log.Message("Game is in the weapon/item selection menu.");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while checking game state: {ex.Message}");
            return false;
        }
    }

    #endregion

    #endregion

    #region Crowd Control Effects
    public override EffectList Effects => new List<Effect>

    {
    // Weapon and Item Effects
    new ("Subtract Ammo", "subtractAmmo") 
        { Price = 1, Quantity = 50, Description = "Removes a chunk of Snake's ammunition supply", Category = "Weapons" },

    new ("Add Ammo", "addAmmo") 
        { Price = 1, Quantity = 50, Description = "Grants additional ammunition to Snake", Category = "Weapons" },

    new ("Empty Snake's Weapon Clip", "emptyCurrentWeaponClip") 
        { Price = 50, Duration = 8, Description = "Forces Snake to reload over and over for 8 seconds", Category = "Weapons" },

    new ("Unequip Snake's Weapon", "setSnakeCurrentWeaponToNone") 
        { Price = 20, Description = "Leaves Snake defenseless by unequipping his current weapon", Category = "Weapons" },

    // Alert Status Effects
    new ("Set Alert Status", "setAlertStatus") 
        { Price = 80, Description = "Triggers an alert status, sending the enemies to attack Snake", Category = "Alert Status" },

    new ("Set Evasion Status", "setEvasionStatus") 
        { Price = 40, Description = "Puts the guards into evasion mode, where guards actively search for Snake", Category = "Alert Status" },

    new ("Set Caution Status", "setCautionStatus") 
        { Price = 20, Description = "Puts the guards into caution mode with heightened awareness", Category = "Alert Status" },

    // HUD Effects
    new ("Remove Partial HUD", "removePartialHUD") 
        { Price = 20, Duration = 60, Description = "Removes parts of the on-screen HUD for a limited time", Category = "HUD" },

    new ("Remove Full HUD", "removeFullHUD") 
        { Price = 20, Duration = 60, Description = "Completely hides the on-screen HUD for a limited time", Category = "HUD" },

    new ("Set to Day Mode", "setToDayMode") 
        { Price = 5, Description = "Changes the game visuals to simulate daytime lighting", Category = "Visual Effects" },

    new ("Set to Night Mode", "setToNightMode") 
        { Price = 5, Description = "Changes the game visuals to simulate nighttime lighting", Category = "Visual Effects" },

    // Snake's Stat Related Effects
    new ("Set Snake Stamina to 0", "setSnakeStamina") 
        { Price = 250, Description = "Drains Snake's stamina completely", Category = "Snake's Stats" },

    new ("Set Snake Max Stamina", "setSnakeMaxStamina") 
        { Price = 250, Description = "Fully restores Snake's stamina bar", Category = "Snake's Stats" },

    new ("Snake gets Common Cold", "snakeHasTheCommonCold") 
        { Price = 10, Description = "Inflicts Snake with a cold, causing sneezes to alert enemies", Category = "Snake's Stats" },

    new ("Poison Snake", "snakeIsPoisoned") 
        { Price = 100, Description = "Poisons Snake, slowly draining his health", Category = "Snake's Stats" },

    new ("Snake has Food Poisoning", "snakeHasFoodPoisoning") 
        { Price = 25, Description = "Gives Snake food poisoning, causing frequent nausea", Category = "Snake's Stats" },

    new ("Snake has Leeches", "snakeHasLeeches") 
        { Price = 25, Description = "Attaches leeches to Snake, draining stamina until removed", Category = "Snake's Stats" },

    new ("Snake x2 Damage Multiplier", "setSnakeDamageX2") 
        { Price = 50, Duration = 30, Description = "Doubles the damage Snake takes for a limited time", Category = "Snake's Stats" },

    new ("Snake x3 Damage Multiplier", "setSnakeDamageX3") 
        { Price = 100, Duration = 30, Description = "Triples the damage Snake takes for a limited time", Category = "Snake's Stats" },

    new ("Snake x4 Damage Multiplier", "setSnakeDamageX4") 
        { Price = 150, Duration = 30, Description = "Quadruples the damage Snake takes for a limited time", Category = "Snake's Stats" },

    new ("Snake x5 Damage Multiplier", "setSnakeDamageX5") 
        { Price = 200, Duration = 30, Description = "Quintuples the damage Snake takes for a limited time", Category = "Snake's Stats" },

    // Snake's Animation Effects
    new ("Snake Nap Time", "makeSnakeQuickSleep") 
        { Price = 100, Description = "Puts Snake to sleep instantly", Category = "Snake's Animations" },

    new ("Snake Pukes and gets set on Fire", "makeSnakePukeFire") 
        { Price = 250, Description = "Causes Snake to vomit explosively and catch fire", Category = "Snake's Animations" },

    new ("Snake Pukes", "makeSnakePuke") 
        { Price = 100, Description = "Causes Snake to vomit", Category = "Snake's Animations" },

    new ("Set Snake on Fire", "setSnakeOnFire") 
        { Price = 150, Description = "Sets Snake on fire, causing him to take damage over time", Category = "Snake's Animations" },

    new ("Snake Bunny Hop", "makeSnakeBunnyHop") 
        { Price = 50, Duration = 3, Description = "Makes Snake repeatedly jump like a bunny for a short time", Category = "Snake's Animations" },

    new ("Freeze Snake in Place", "makeSnakeFreeze") 
        { Price = 50, Description = "Immobilizes Snake completely for a short duration", Category = "Snake's Animations" },

    new ("Make Snake Jump", "makeSnakeJump")
        { Price = 100, Description = "Forces Snake to jump unexpectedly", Category = "Snake's Animations" },

    // Guard Stats
    new ("Guards are Almost Invincible", "setGuardStatsAlmostInvincible")
        { Price = 150, Duration = 40, Description = "Guards become almost invincible to lethal, sleep, and stun damage", Category = "Guard Stats" },

    new ("Guards become Very Strong", "setGuardStatsVeryStrong")
        { Price = 100, Duration = 40, Description = "Guards become very strong against lethal, sleep, and stun damage", Category = "Guard Stats" },

    new ("Guards become Very Weak", "setGuardStatsVeryWeak")
        { Price = 100, Duration = 40, Description = "Guards become very weak against lethal, sleep, and stun damage", Category = "Guard Stats" },

    new ("Guards can be One Shot", "setGuardStatsOneShot")
        { Price = 150, Duration = 40, Description = "Guards become one shot by lethal, sleep, and stun damage", Category = "Guard Stats" },

    };

    protected override GameState GetGameState()
    {
        try
        {
            if (!isPausedOrMenu.TryGetInt(out int v)) return GameState.WrongMode;
            // Checks if game is paused or radio call so we can delay effects
            if (v == 1) return GameState.WrongMode;
            return GameState.Ready;
        }
        catch { return GameState.Unknown; }
    }

    protected override void StartEffect(EffectRequest request)
    {
        if (!IsReady(request))
        {
            DelayEffect(request);
            return;
        }

        var codeParams = FinalCode(request).Split('_');
        switch (codeParams[0])
        {

            #region Weapons
            case "subtractAmmo":
                {
                    if (!int.TryParse(codeParams[1], out int quantity))
                    {
                        Respond(request, EffectStatus.FailTemporary, "Invalid quantity");
                        break;
                    }

                    TryEffect(request,
                        () => true,
                        () => TrySubtractAmmoFromCurrentWeapon((short)quantity),
                        () => Connector.SendMessage($"{request.DisplayViewer} subtracted {quantity} ammo from {GetCurrentEquippedWeapon()?.Name ?? "Unknown Weapon"}."), null, false);
                    break;
                }

            case "addAmmo":
                {
                    if (!int.TryParse(codeParams[1], out int quantity))
                    {
                        Respond(request, EffectStatus.FailTemporary, "Invalid quantity");
                        break;
                    }

                    TryEffect(request,
                        () => true,
                        () => TryAddAmmoToCurrentWeapon((short)quantity),
                        () => Connector.SendMessage($"{request.DisplayViewer} added {quantity} ammo to {GetCurrentEquippedWeapon()?.Name ?? "Unknown Weapon"}."), 
                        null, false);
                    break;
                }

            case "emptyCurrentWeaponClip":
                {
                    var emptyClipDuration = request.Duration = TimeSpan.FromSeconds(8);

                    var emptyClipAct = RepeatAction(request,
                        () => true,
                        () => Connector.SendMessage($"{request.DisplayViewer} is emptying Snake's weapon clip for {emptyClipDuration.TotalSeconds} seconds."),
                        TimeSpan.Zero,
                        () => IsReady(request),
                        TimeSpan.FromMilliseconds(100),
                        () =>
                        {
                            Weapon currentWeapon = GetCurrentEquippedWeapon();
                            if (currentWeapon != null && currentWeapon.HasClip)
                            {
                                EmptySnakeClipInLoop();
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        },
                        TimeSpan.FromMilliseconds(100), false);

                    emptyClipAct.WhenCompleted.Then
                        (_ =>
                        { Connector.SendMessage("Emptying Snake's weapon clip effect has ended."); 
                        });

                    break;
                }

            case "setSnakeCurrentWeaponToNone":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetSnakeCurrentWeaponToNone();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} unequipped Snake's weapon."),
                    null, true);
                break;

            #endregion

            #region Alert Status

            case "setAlertStatus":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetAlertStatus();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Alert Status."),
                    null, true);
                break;

            case "setEvasionStatus":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetCautionStatus();
                        /* This 5 seconds gives time for reinforcements to be called which 
                         makes for a better evasion status of guards searching for Snake */
                        Task.Delay(5000).Wait();
                        SetEvasionStatus();
                        Task.Delay(1000).Wait();
                        SetAlertStatus();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Evasion Status."),
                    null, true);
                break;

            case "setCautionStatus":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetCautionStatus();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Caution Status."),
                    null, true);
                break;

            #endregion

            #region HUD and Filters

            case "removePartialHUD":
                var removePartialHUDDuration = request.Duration = TimeSpan.FromSeconds(60);
                var removePartialHUDAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} removed the partial HUD for {removePartialHUDDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                () =>
                    {
                        RemovePartialHUD();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                false);
                removePartialHUDAct.WhenCompleted.Then
                    (_ =>
                {
                    RestorePartialHUD();
                    Connector.SendMessage("Partial HUD has been restored.");
                });
                break;

            case "removeFullHUD":
                var removeFullHUDDuration = request.Duration = TimeSpan.FromSeconds(60);
                var removeFullHUDAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} removed the full HUD for {removeFullHUDDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                () =>
                    {
                        RemoveFullHUD();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                false);
                removeFullHUDAct.WhenCompleted.Then
                    (_ =>
                {
                    RestoreFullHUD();
                    Connector.SendMessage("Full HUD has been restored.");
                });
                break;

            case "setToDayMode":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetToDayMode();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Day Mode."),
                    null, true);
                break;

            case "setToNightMode":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetToNightMode();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Night Mode."),
                    null, true);
                break;

            #endregion

            #region Snake's Stats

            case "setSnakeStamina":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetSnakeStamina();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's Stamina to 0."),
                    null, true);
                break;


            case "setSnakeMaxStamina":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetSnakeMaxStamina();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's Stamina to 30000."),
                    null, true);
                break;

            case "makeSnakeJump":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        IncreaseSnakeYCoordBy2000();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake jump."),
                    null, true);
                break;

            case "snakeHasTheCommonCold":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeHasTheCommonCold();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake the common cold."),
                    null, true);
                break;


            case "snakeIsPoisoned":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeIsPoisoned();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} poisoned Snake."),
                    null, true);
                break;

            case "snakeHasFoodPoisoning":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeHasFoodPoisoning();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake food poisoning."),
                    null, true);
                break;

            case "snakeHasLeeches":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeHasLeeches();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake leeches."),
                    null, true);
                break;

            case "setSnakeDamageX2":
                var damageX2Duration = request.Duration = TimeSpan.FromSeconds(60);
                var damageX2Act = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's damage multiplier to x2 for {damageX2Duration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeDamageMultiplierInstruction();
                        SetSnakeDamageMultiplierValue(2);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                damageX2Act.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeDamageMultiplierValue(1);
                    Connector.SendMessage("Snake's damage multiplier is back to x1.");
                });
                break;

            case "setSnakeDamageX3":
                var damageX3Duration = request.Duration = TimeSpan.FromSeconds(60);
                var damageX3Act = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's damage multiplier to x3 for {damageX3Duration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeDamageMultiplierInstruction();
                        SetSnakeDamageMultiplierValue(3);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                damageX3Act.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeDamageMultiplierValue(1);
                    Connector.SendMessage("Snake's damage multiplier is back to x1.");
                });
                break;

            case "setSnakeDamageX4":
                var damageX4Duration = request.Duration = TimeSpan.FromSeconds(60);
                var damageX4Act = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's damage multiplier to x4 for {damageX4Duration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeDamageMultiplierInstruction();
                        SetSnakeDamageMultiplierValue(4);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                damageX4Act.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeDamageMultiplierValue(1);
                    Connector.SendMessage("Snake's damage multiplier is back to x1.");
                });
                break;

            case "setSnakeDamageX5":
                var damageX5Duration = request.Duration = TimeSpan.FromSeconds(60);
                var damageX5Act = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's damage multiplier to x5 for {damageX5Duration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeDamageMultiplierInstruction();
                        SetSnakeDamageMultiplierValue(5);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                damageX5Act.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeDamageMultiplierValue(1);
                    Connector.SendMessage("Snake's damage multiplier is back to x1.");
                });
                break;

            #endregion

            #region Snake's Animations

            case "makeSnakeQuickSleep":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        MakeSnakeQuickSleep();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake quick sleep."),
                    null, true);
                break;

            case "makeSnakePukeFire":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        MakeSnakePukeFire();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake puke fire."),
                    null, true);
                break;

            case "makeSnakePuke":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        MakeSnakePuke();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake puke."),
                    null, true);
                break;

            case "setSnakeOnFire":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetSnakeOnFire();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake on fire."),
                    null, true);
                break;

            case "makeSnakeBunnyHop":
                var bunnyHopDuration = request.Duration = TimeSpan.FromSeconds(3);

                var bunnyHopAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake bunny hop for {bunnyHopDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        MakeSnakeBunnyHop();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);

                bunnyHopAct.WhenCompleted.Then
                    (_ =>
                {
                    Connector.SendMessage("Snake is no longer Bunny Hopping.");
                });
                break;

            case "makeSnakeFreeze":
                var freezeDuration = request.Duration = TimeSpan.FromSeconds(3);

                var freezeAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} froze Snake for {freezeDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        MakeSnakeFreeze();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);

                freezeAct.WhenCompleted.Then
                    (_ =>
                {
                    UnfreezeSnake();
                    Connector.SendMessage("Snake is no longer frozen.");
                });
                break;

                #endregion

            #region Guard Stats

            case "setGuardStatsAlmostInvincible":
                var almostInvincibleDuration = request.Duration = TimeSpan.FromSeconds(40);

                var almostInvincibleAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set the guards to be almost invincible for {almostInvincibleDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetGuardLethalDamageInvincible();
                        SetGuardSleepDamageAlmostInvincible();
                        SetGuardStunAlmostDamageInvincible();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);

                almostInvincibleAct.WhenCompleted.Then
                (_ =>
                {
                    SetGuardLethalDamageDefault();
                    SetGuardSleepDamageDefault();
                    SetGuardStunDamageDefault();
                    Connector.SendMessage("Guard stats are back to default.");
                });

                break;

            case "setGuardStatsVeryStrong":
                var veryStrongDuration = request.Duration = TimeSpan.FromSeconds(40);

                var veryStrongAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set the guards to be very strong for {veryStrongDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetGuardLethalDamageVeryStrong();
                        SetGuardSleepDamageVeryStrong();
                        SetGuardStunDamageVeryStrong();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                veryStrongAct.WhenCompleted.Then
                (_ =>
                {
                    SetGuardLethalDamageDefault();
                    SetGuardSleepDamageDefault();
                    SetGuardStunDamageDefault();
                    Connector.SendMessage("Guard stats are back to default.");
                });

                break;

            case "setGuardStatsVeryWeak":
                var veryWeakDuration = request.Duration = TimeSpan.FromSeconds(40);

                var veryWeakAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set the guards to be very weak for {veryWeakDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetGuardLethalDamageVeryWeak();
                        SetGuardSleepDamageVeryWeak();
                        SetGuardStunDamageVeryWeak();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                veryWeakAct.WhenCompleted.Then
                (_ =>
                {
                    SetGuardLethalDamageDefault();
                    SetGuardSleepDamageDefault();
                    SetGuardStunDamageDefault();
                    Connector.SendMessage("Guard stats are back to default.");
                });
                break;

            case "setGuardStatsOneShot":
                var oneShotDuration = request.Duration = TimeSpan.FromSeconds(40);

                var oneShotAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set the guards to be one shot for {oneShotDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetGuardLethalDamageOneshot();
                        SetGuardSleepDamageOneshot();
                        SetGuardStunDamageOneshot();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                oneShotAct.WhenCompleted.Then
                (_ =>
                {
                    SetGuardLethalDamageDefault();
                    SetGuardSleepDamageDefault();
                    SetGuardStunDamageDefault();
                    Connector.SendMessage("Guard stats are back to default.");
                });
                break;

                #endregion
        }

    }
}
#endregion