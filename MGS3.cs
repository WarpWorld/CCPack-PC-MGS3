using System.Diagnostics.CodeAnalysis;
using System.Text;
using ConnectorLib.Memory;
using CrowdControl.Common;
using AddressChain = ConnectorLib.Memory.AddressChain<ConnectorLib.Inject.InjectConnector>;
using ConnectorType = CrowdControl.Common.ConnectorType;
using Log = CrowdControl.Common.Log;

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
    private AddressChain boxCrouch;
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
    private AddressChain camoIndexInstructions;
    private AddressChain camoIndexValue;
    // Game State
    private AddressChain isPausedOrMenu;
    private AddressChain alertStatus;
    private AddressChain mapStringAddress;

    // HUD and Filters
    private AddressChain hudPartiallyRemoved;
    private AddressChain hudFullyRemoved;
    private AddressChain fieldOfView;
    private AddressChain pissFilter;
    private AddressChain pissFilterDensity;
    private AddressChain lightNearSnake;
    private AddressChain mapColour;
    private AddressChain skyColour;
    private AddressChain skyValue;
    private AddressChain distanceVisibility;

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

        /* Made a class to use offsets from the base address then add 80 bytes (0x50) to get to the next weapon to cut down on overall code */
        baseWeaponAddress = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D2C16C");
        baseItemAddress = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D2EA5C");

        // Snake Animations to test
        snakeQuickSleep = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E14C3B");
        snakePukeFire = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E14C3C");
        snakeBunnyHop = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E14C48");
        snakeFreeze = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E1F9DD");
        snakeYcoordinate = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E14BF0=>+134");
        boxCrouch = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E14C46");

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

        camoIndexInstructions = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+357907"); // 89 44 2B 24 = normal 90 90 90 90 allows camoIndexValue to be changed
        camoIndexValue = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1E14C24"); // -1000 for -100% camo 1000 for 100% camo 4 byte value

        // Game State
        isPausedOrMenu = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D76E9C");
        mapStringAddress = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+ACBE18=>+24");


        // 16 = Alert, 32 = Caution, 0 = No Alert
        alertStatus = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D84F38");


        // HUD and Filters
        hudPartiallyRemoved = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D558DD");
        hudFullyRemoved = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D558DC");
        fieldOfView = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+AE733");

        pissFilter = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D5435B");
        pissFilterDensity = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D54330");
        lightNearSnake = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D5432D");
        mapColour = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D54324");
        skyColour = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D54320");
        skyValue = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D5431C");
        distanceVisibility = AddressChain.Parse(Connector, "\"METAL GEAR SOLID3.exe\"+1D54316");


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
        public static readonly Weapon CigSpray = new Weapon("Cig Spray", 3, true);
        public static readonly Weapon Handkerchief = new Weapon("Handkerchief", 4, true);
        public static readonly Weapon MK22 = new Weapon("MK22", 5, true, true, true);
        public static readonly Weapon M1911A1 = new Weapon("M1911A1", 6, true, true, true);
        public static readonly Weapon EzGun = new Weapon("EZ Gun", 7);
        public static readonly Weapon SAA = new Weapon("SAA", 8, true, true);
        public static readonly Weapon Patriot = new Weapon("Patriot", 9);
        public static readonly Weapon Scorpion = new Weapon("Scorpion", 10, true, true);
        public static readonly Weapon XM16E1 = new Weapon("XM16E1", 11, true, true, true);
        public static readonly Weapon AK47 = new Weapon("AK47", 12, true, true);
        public static readonly Weapon M63 = new Weapon("M63", 13, true, true);
        public static readonly Weapon M37 = new Weapon("M37", 14, true, true);
        public static readonly Weapon SVD = new Weapon("SVD", 15, true, true);
        public static readonly Weapon MosinNagant = new Weapon("Mosin-Nagant", 16, true, true);
        public static readonly Weapon RPG7 = new Weapon("RPG-7", 17, true, true);
        public static readonly Weapon Torch = new Weapon("Torch", 18);
        public static readonly Weapon Grenade = new Weapon("Grenade", 19, true);
        public static readonly Weapon WpGrenade = new Weapon("WP Grenade", 20, true);
        public static readonly Weapon StunGrenade = new Weapon("Stun Grenade", 21, true);
        public static readonly Weapon ChaffGrenade = new Weapon("Chaff Grenade", 22, true);
        public static readonly Weapon SmokeGrenade = new Weapon("Smoke Grenade", 23, true);
        public static readonly Weapon EmptyMagazine = new Weapon("Empty Magazine", 24, true);
        public static readonly Weapon TNT = new Weapon("TNT", 25, true);
        public static readonly Weapon C3 = new Weapon("C3", 26, true);
        public static readonly Weapon Claymore = new Weapon("Claymore", 27, true);
        public static readonly Weapon Book = new Weapon("Book", 28, true);
        public static readonly Weapon Mousetrap = new Weapon("Mousetrap", 29, true);
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
        public static readonly Item GakoCamo = new Item("GakoCamo", 65);
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
            { GakoCamo.Index, GakoCamo },
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
        GakoCamo = 24,
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
        Mask = 10, // Causes crashes when forced on or off will need an exception like if mask is on then don't change
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

    private string GetString(AddressChain addr, int maxLength)
    {
        if (maxLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "maxLength must be positive");

        byte[] data = addr.GetBytes(maxLength);
        if (data == null || data.Length == 0)
        {
            return string.Empty;
        }

        int nullIndex = Array.IndexOf(data, (byte)0);
        if (nullIndex >= 0)
        {
            return Encoding.ASCII.GetString(data, 0, nullIndex);
        }
        else
        {
            return Encoding.ASCII.GetString(data, 0, data.Length);
        }
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

    // Find out if M1911A1, MK22, or XM16E1 has a suppressor
    private bool IsWeaponSuppressed(Weapon weapon)
    {
        if (weapon == null || !weapon.HasSuppressor)
        {
            Log.Message($"{weapon?.Name ?? "Unknown Weapon"} does not have a suppressor.");
            return false;
        }

        var suppressorAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.SuppressorToggleOffset);
        return Get8(suppressorAddress) == 1;
    }

    private void ToggleWeaponSuppressor(Weapon weapon)
    {
        if (weapon == null || !weapon.HasSuppressor)
        {
            Log.Message($"{weapon?.Name ?? "Unknown Weapon"} does not have a suppressor.");
            return;
        }

        var suppressorAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.SuppressorToggleOffset);
        byte suppressorValue = Get8(suppressorAddress);
        byte newSuppressorValue = suppressorValue == 0 ? (byte)16 : (byte)0;
        Set8(suppressorAddress, newSuppressorValue);
        Log.Message($"{weapon.Name} suppressor toggled. Value: {suppressorValue} -> {newSuppressorValue}");
    }

    // Helper methods for suppressor control:
    private void ForceWeaponSuppressorOff(Weapon weapon)
    {
        if (weapon == null || !weapon.HasSuppressor)
            return;

        var suppressorAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.SuppressorToggleOffset);
        byte currentValue = Get8(suppressorAddress);
        // If not already off, force it off.
        if (currentValue != 0)
        {
            Set8(suppressorAddress, 0);
            Log.Message($"{weapon.Name} suppressor forced off (was {currentValue}).");
        }
    }

    private void ForceWeaponSuppressorOn(Weapon weapon)
    {
        if (weapon == null || !weapon.HasSuppressor)
            return;

        var suppressorAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.SuppressorToggleOffset);
        // Here we assume that 16 is the “on” value for these weapons.
        Set8(suppressorAddress, 16);
        Log.Message($"{weapon.Name} suppressor forced on.");
    }


    #endregion

    #region Items

    private short GetItemValue(Item item)
    {
        try
        {
            AddressChain currentAddress = item.GetPropertyAddress(baseItemAddress, ItemAddresses.CurrentCapacityOffset);
            return Get16(currentAddress);
        }
        catch (Exception ex)
        {
            Log.Error($"Error reading current value for {item.Name}: {ex.Message}");
            return -1;
        }
    }

    private short GetItemMaxCapacity(Item item)
    {
        try
        {
            AddressChain maxAddress = item.GetPropertyAddress(baseItemAddress, ItemAddresses.MaxCapacityOffset);
            return Get16(maxAddress);
        }
        catch (Exception ex)
        {
            Log.Error($"Error reading max capacity for {item.Name}: {ex.Message}");
            return -1;
        }
    }

    private void SetItemValue(Item item, short newValue)
    {
        AddressChain currentAddress = item.GetPropertyAddress(baseItemAddress, ItemAddresses.CurrentCapacityOffset);
        Set16(currentAddress, newValue);
        Log.Message($"{item.Name} value set to {newValue}");
    }

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

    private void SetSnakeCamoIndexInstructionToNormal()
    {
        try
        {
            byte[] camoIndexInstruction = new byte[] { 0x89, 0x44, 0x2B, 0x24 };
            SetArray(camoIndexInstructions, camoIndexInstruction);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Camo Index Instruction: {ex.Message}");
        }
    }

    private void SetSnakeCamoIndexInstructionToWritable()
    {
        try
        {
            byte[] camoIndexInstruction = new byte[] { 0x90, 0x90, 0x90, 0x90 };
            SetArray(camoIndexInstructions, camoIndexInstruction);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Camo Index Instruction: {ex.Message}");
        }
    }

    private void SetSnakeCamoIndexValue(int value)
    {
        try
        {
            int originalValue = Get32(camoIndexValue);
            Set32(camoIndexValue, value);
            int newValue = Get32(camoIndexValue);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Snake's Camo Index Value: {ex.Message}");
        }
    }

    private bool IsCamoIndexInstructionNormal()
    {
        try
        {
            byte[] normalInstruction = new byte[] { 0x89, 0x44, 0x2B, 0x24 };
            byte[] currentInstruction = GetArray<byte>(camoIndexInstructions, 4);
            return currentInstruction.SequenceEqual(normalInstruction);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while checking Camo Index Instruction: {ex.Message}");
            return false;
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

    private void SetTimeOfDayValues(
        byte pissFilterValue,
        byte[] pissDensityValues,
        byte[] lightNearSnakeValues,
        byte[] mapColourValues,
        byte[] skyColourValues,
        byte skyValueValue,
        byte[] distanceVisibilityValues)
    {
        Set8(pissFilter, pissFilterValue);
        SetArray(pissFilterDensity, pissDensityValues);
        SetArray(lightNearSnake, lightNearSnakeValues);
        SetArray(mapColour, mapColourValues);
        SetArray(skyColour, skyColourValues);
        Set8(skyValue, skyValueValue);
        SetArray(distanceVisibility, distanceVisibilityValues);
    }

    public void SetToDayMode()
    {
        // Define your day-mode values (example values – adjust as needed)
        byte dayPissFilter = 0x42;
        byte[] dayPissDensity = new byte[] { 0x00, 0x5E, 0x72, 0x64 };
        byte[] dayLightNearSnake = new byte[] { 0x40, 0x9C, 0xC5, 0x00 };
        byte[] dayMapColour = new byte[] { 0x00, 0x5E, 0x72, 0x64 };
        byte[] daySkyColour = new byte[] { 0x60, 0xA5, 0xFF, 00 };
        byte daySkyValue = 0x0F; // 0F essentially removes the skybox textures need a colour to sub out for this to make things look better
        byte[] dayDistanceVisibility = new byte[] { 0xA0, 0x49 };

        SetTimeOfDayValues(
            dayPissFilter,
            dayPissDensity,
            dayLightNearSnake,
            dayMapColour,
            daySkyColour,
            daySkyValue,
            dayDistanceVisibility);
    }

    public void SetToNightMode()
    {
        // Define your night-mode values (example values – adjust as needed)
        byte nightPissFilter = 0x41;  // Use same or different value as needed
        byte[] nightPissDensity = new byte[] { 0x00, 0x24, 0x74, 0x46 }; // Example values
        byte[] nightLightNearSnake = new byte[] { 0x00, 0x24, 0x74, 0x46 };
        byte[] nightMapColour = new byte[] { 0x00, 0x03, 0x0A, 0x1B };
        byte[] nightSkyColour = new byte[] { 0x00, 0x03, 0x0A, 0x1B };
        byte nightSkyValue = 0x0F;
        byte[] nightDistanceVisibility = new byte[] { 0x3F, 0x47 };

        SetTimeOfDayValues(
            nightPissFilter,
            nightPissDensity,
            nightLightNearSnake,
            nightMapColour,
            nightSkyColour,
            nightSkyValue,
            nightDistanceVisibility);
    }

    public void SetToFoggyMode()
    {
        // Define your foggy-mode values (example values – adjust as needed)
        byte foggyPissFilter = 0x00;
        byte[] foggyPissDensity = new byte[] { 0x00, 0x48, 0x9C, 0x46 };
        byte[] foggyLightNearSnake = new byte[] { 0x00, 0x48, 0x9C, 0x46 };
        byte[] foggyMapColour = new byte[] { 0x22, 0x2C, 0x2C, 0x00 };
        byte[] foggySkyColour = new byte[] { 0x22, 0x2C, 0x2C, 0x00 };
        byte foggySkyValue = 0x0F;
        byte[] foggyDistanceVisibility = new byte[] { 0xA0, 0x49 };

        SetTimeOfDayValues(
            foggyPissFilter,
            foggyPissDensity,
            foggyLightNearSnake,
            foggyMapColour,
            foggySkyColour,
            foggySkyValue,
            foggyDistanceVisibility);
    }

    public void SetToMuddyFogMode()
    {
        byte muddyFogPissFilter = 0x00;
        byte[] muddyFogPissDensity = new byte[] { 0x46, 0x48, 0x9C, 0x46 };
        byte[] muddyFogLightNearSnake = new byte[] { 0x00, 0x48, 0x9C, 0x46 };
        byte[] muddyFogMapColour = new byte[] { 0x32, 0x2A, 0x10, 0x00 };
        byte[] muddyFogSkyColour = new byte[] { 0x32, 0x2A, 0x10, 0x00 };
        byte muddyFogSkyValue = 0x0F;
        byte[] muddyFogDistanceVisibility = new byte[] { 0xA0, 0x49 };
        SetTimeOfDayValues(
            muddyFogPissFilter,
            muddyFogPissDensity,
            muddyFogLightNearSnake,
            muddyFogMapColour,
            muddyFogSkyColour,
            muddyFogSkyValue,
            muddyFogDistanceVisibility);
    }

    public void SetToRedMistMode()
    {
        byte redMistPissFilter = 0x00;
        byte[] redMistPissDensity = new byte[] { 0x46, 0x48, 0x9C, 0x46 };
        byte[] redMistLightNearSnake = new byte[] { 0x00, 0x48, 0x9C, 0x46 };
        byte[] redMistMapColour = new byte[] { 0x60, 0x00, 0x00, 0x00 };
        byte[] redMistSkyColour = new byte[] { 0x60, 0x00, 0x00, 0x00 };
        byte redMistSkyValue = 0x0F;
        byte[] redMistDistanceVisibility = new byte[] { 0xA0, 0x49 };
        SetTimeOfDayValues(
            redMistPissFilter,
            redMistPissDensity,
            redMistLightNearSnake,
            redMistMapColour,
            redMistSkyColour,
            redMistSkyValue,
            redMistDistanceVisibility);
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

    private void SetZoomInFOV()
    {
        try
        {
            SetArray(fieldOfView, new byte[] { 0xCD, 0xCC, 0xCC, 0x3E });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Zoom In FOV: {ex.Message}");
        }
    }

    private void SetZoomOutFOV()
    {
        try
        {
            SetArray(fieldOfView, new byte[] { 0x00, 0x00, 0x20, 0x40 });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Zoom Out FOV: {ex.Message}");
        }
    }

    private void SetNormalFOV()
    {
        try
        {
            SetArray(fieldOfView, new byte[] { 0x00, 0x00, 0x80, 0x3F });
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while setting Normal FOV: {ex.Message}");
        }
    }

    private bool IsNormalFOV()
    {
        try
        {
            byte[] normalFOV = new byte[] { 0x00, 0x00, 0x80, 0x3F };
            byte[] currentFOV = GetArray<byte>(fieldOfView, 4);
            return currentFOV.SequenceEqual(normalFOV);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while checking Normal FOV: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Camo and Face Paint

    private void SwapUniform(SnakesUniformCamo uniform)
    {
        if (IsInCutscene())
        {
            throw new Exception("Cannot swap uniform during a cutscene or menu.");
        }
        // Write the new uniform value.
        Set8(snakeCurrentCamo, (byte)uniform);
        Log.Message($"Uniform swapped to {uniform}");
    }

    private void SwapFacePaint(SnakesFacePaint facePaint)
    {
        if (IsInCutscene())
        {
            throw new Exception("Cannot swap face paint during a cutscene or menu.");
        }
        byte currentFacePaint = Get8(snakeCurrentFacePaint);
        if (currentFacePaint == (byte)SnakesFacePaint.Mask)
        {
            throw new Exception("Cannot change face paint while mask is equipped.");
        }
        Set8(snakeCurrentFacePaint, (byte)facePaint);
        Log.Message($"Face paint swapped to {facePaint}");
    }

    // Check if Mask is equipped
    private bool IsMaskEquipped()
    {
        byte currentFacePaint = Get8(snakeCurrentFacePaint);
        return currentFacePaint == (byte)SnakesFacePaint.Mask;
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
            Log.Message("Attempting to make Snake puke while being set on fire.");
            Set8(snakeFreeze, 1);
            Set8(snakePukeFire, 255);
            Log.Message("Snake is puking and on fire.");
            Thread.Sleep(1500);
            Set8(snakeFreeze, 0);
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
            Set8(snakeFreeze, 1);
            Set8(snakePukeFire, 1);
            Thread.Sleep(1500);
            Set8(snakeFreeze, 0);
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
            Set8(snakeFreeze, 1);
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

    private void MakeSnakeBoxCrouch()
    {
        try
        {
            Set8(boxCrouch, 49);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while making Snake box crouch: {ex.Message}");
        }
    }

    private void UndoSnakeBoxCrouch()
    {
        try
        {
            Set8(boxCrouch, 0);
        }
        catch (Exception ex)
        {
            Log.Error($"An error occurred while making Snake box stand: {ex.Message}");
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

    private bool IsInCutscene()
    {
        try
        {
            string currentMap = GetString(mapStringAddress, 64).Trim().ToLowerInvariant();
            var cutsceneMaps = new List<string> { "kyle_op", "title", "theater", "ending" };

            // Cutscene have _0 or _1 at the end of the map name only hud related effects should be allowed during cutscenes
            if (cutsceneMaps.Contains(currentMap) || currentMap.EndsWith("_0") || currentMap.EndsWith("_1"))
            {
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Log.Error("Error in IsInCutscene: " + ex.Message);
            return true;
        }
    }

    private bool IsMedicalItemEffectsAllowed()
    {
        try
        {
            string currentMap = GetString(mapStringAddress, 64).Trim().ToLowerInvariant();
            var medicalItemMaps = new List<string> { "v001a", "v003a", "v004a", "v005a", "v006a", "v006b", "v007a" };

            if (medicalItemMaps.Contains(currentMap))
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Error in IsMedicalItemEffectsAllowed: " + ex.Message);
            return false;
        }
    }

    /*s161a, // Groznyj Grad - 1st Part of bike chase
    s162a, // Groznyj Grad Runway South - 2nd Part of bike chase
    s163a, // Groznyj Grad Runway - 3rd part of bike chase
    s163b, // Groznyj Grad Runway - 4th path of chase with shagohod only
    s171a, // Groznyj Grad Rail Bridge - Shooting the C3
    s171b, // Groznyj Grad Rail Bridge - Fighting the Shagohod on and off the bike
    s181a, // Groznyj Grad Rail Bridge North - 1st Escape after beating Volgin*/
    private bool IsSleepAllowedOnCurrentMap()
    {
        try
        {
            string currentMap = GetString(mapStringAddress, 64).Trim().ToLowerInvariant();
            var noSleepMaps = new List<string> { "v001a", "v003a", "v004a", "v005a", "v006a", "v006b", "v007a", "s012a", "s066a", "s161a", "s162a", "s163a", "s163b", "s171a", "s171b", "s181a" };

            if (noSleepMaps.Contains(currentMap))
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Error in IsSleepAllowedOnCurrentMap: " + ex.Message);
            return false;
        }
    }

    private bool IsBunnyHopAllowedOnCurrentMap()
    {
        try
        {
            string currentMap = GetString(mapStringAddress, 64).Trim().ToLowerInvariant();
            var noBunnyHopMaps = new List<string> { "v007a", "s066a", "s113a", "s161a", "s162a", "s163a", "s163b", "s171a", "s171b", "s181a" };

            if (noBunnyHopMaps.Contains(currentMap))
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Error in IsBunnyHopAllowedOnCurrentMap: " + ex.Message);
            return false;
        }
    }

    private bool IsAlertAllowedOnCurrentMap()
    {
        try
        {
            string currentMap = GetString(mapStringAddress, 64).Trim().ToLowerInvariant();

            var noAlertMaps = new List<string> { "v001a", "s002a", "v003a", "v007a", "v006b", "s003a", "s012a", "s023a", "s031a", "s032a", "s032b", "s033a", "s051a", "s051b", "s066a", "s113a", "s152a", "s161a", "s162a", "s163a", "s163b", "s171a", "s171b", "s181a" };

            if (noAlertMaps.Contains(currentMap))
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error("Error in IsAlertMapAllowed: " + ex.Message);
            return false;
        }
    }

    #endregion

    #endregion

    #region Crowd Control Effects - Pricing, Descriptions, Etc

    public override EffectList Effects => new List<Effect>
    {
    
    #region Weapon and Item Effects

    new ("Subtract Ammo", "subtractAmmo")
        {
        Price = 2,
        Quantity = 50,
        Description = "Removes a chunk of Snake's ammunition supply",
        Category = "Weapons"
        },

    new ("Add Ammo", "addAmmo")
        {
        Price = 2,
        Quantity = 50,
        Description = "Grants additional ammunition to Snake",
        Category = "Weapons"
        },

    new ("Empty Snake's Weapon Clip", "emptyCurrentWeaponClip")
        {
        Price = 80,
        Duration = 8,
        Description = "Forces Snake to reload over and over for 8 seconds",
        Category = "Weapons"
        },

    new ("Unequip Snake's Weapon", "setSnakeCurrentWeaponToNone")
        {
        Price = 30,
        Duration = 4,
        Description = "Leaves Snake defenseless by unequipping his current weapon",
        Category = "Weapons"
        },

    new ("Remove Current Suppressor", "removeCurrentSuppressor")
        {
        Price = 30,
        Duration = 8,
        Description = "Removes the suppressor from Snake's current weapon for a short time. This will also stop him from using a different suppressor on a suppressed weapon",
        Category = "Weapons"
        },

    #endregion

    #region Alert Status Effects

    new ("Set Alert Status", "setAlertStatus")
        {
        Price = 120,
        Description = "Triggers an alert status, sending the enemies to attack Snake",
        Category = "Alert Status"
        },

    new ("Set Evasion Status", "setEvasionStatus")
        {
        Price = 60,
        Description = "Puts the guards into evasion mode, where guards actively search for Snake",
        Category = "Alert Status"
        },

    new ("Set Caution Status", "setCautionStatus")
        {
        Price = 30,
        Description = "Puts the guards into caution mode with heightened awareness",
        Category = "Alert Status"
        },

    #endregion

    #region Camo - Face Paint Swap

    new ("Swap to No", "swapToNoFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Removes Snake's face paint, leaving his face bare",
        Category = "Camo - Face Paint"
        },

    new ("Swap to Woodland", "swapToWoodlandFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies woodland face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Black", "swapToBlackFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies black face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Water", "swapToWaterFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies water face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Desert", "swapToDesertFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies desert face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Splitter", "swapToSplitterFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies splitter face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Snow", "swapToSnowFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies snow face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Kabuki", "swapToKabukiFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies kabuki face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Zombie", "swapToZombieFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies zombie face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Oyama", "swapToOyamaFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies oyama face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Green", "swapToGreenFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies green face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Brown", "swapToBrownFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies brown face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Soviet Union", "swapToSovietUnionFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies Soviet Union face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to UK", "swapToUKFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies UK face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to France", "swapToFranceFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies France face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Germany", "swapToGermanyFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies Germany face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Italy", "swapToItalyFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies Italy face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Spain", "swapToSpainFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies Spain face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Sweden", "swapToSwedenFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies Sweden face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to Japan", "swapToJapanFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies Japan face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    new ("Swap to USA", "swapToUSAFacePaint")
        {
        Note = "Face Paint",
        Price = 30,
        Description = "Applies USA face paint to Snake's face",
        Category = "Camo - Face Paint",
        Image = "face_pain"
        },

    #endregion

    #region Camo - Uniform Swap

    new ("Swap to Olive Drab", "swapToOliveDrab")
        {
        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Olive Drab",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to Tiger Stripe", "swapToTigerStripe")
        {
        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Tiger Stripe",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to Leaf", "swapToLeaf")
        {
        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Leaf",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to Tree Bark", "swapToTreeBark")
        {
        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Tree Bark",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to Choco Chip", "swapToChocoChip")
        {

        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Choco Chip",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to Splitter", "swapToSplitter")
        {

        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Splitter",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to Raindrop", "swapToRaindrop")
        {

Note = "Uniform",
         Price = 30,
            Description = "Changes Snake's uniform to Raindrop",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Squares", "swapToSquares")
        {

Note = "Uniform",
         Price = 30,
            Description = "Changes Snake's uniform to Squares",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Water", "swapToWater")
        {

Note = "Uniform",
         Price = 30,
            Description = "Changes Snake's uniform to Water",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Black", "swapToBlack")
        {

Note = "Uniform",
         Price = 30,
            Description = "Changes Snake's uniform to Black",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Snow", "swapToSnow")
        {

Note = "Uniform", 
        Price = 30,
            Description = "Changes Snake's uniform to Snow",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Naked", "swapToNaked")
        {

Note = "Uniform",
         Price = 30,
            Description = "Changes Snake's uniform to Naked",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Sneaking Suit", "swapToSneakingSuit")
        {

Note = "Uniform",
         Price = 30,
            Description = "Changes Snake's uniform to Sneaking Suit",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Hornet Stripe", "swapToHornetStripe")
        {

Note = "Uniform", 
        Price = 30,
            Description = "Changes Snake's uniform to Hornet Stripe",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Spider", "swapToSpider")
        {

Note = "Uniform", 
        Price = 30,
            Description = "Changes Snake's uniform to Spider",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Moss", "swapToMoss")
        {

Note = "Uniform",
         Price = 30,
            Description = "Changes Snake's uniform to Moss",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Fire", "swapToFire")
        {

Note = "Uniform",
         Price = 30,
            Description = "Changes Snake's uniform to Fire",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Spirit", "swapToSpirit")
        {

Note = "Uniform",
         Price = 30,
            Description = "Changes Snake's uniform to Spirit",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Cold War", "swapToColdWar")
        {

Note = "Uniform",
         Price = 30,
            Description = "Changes Snake's uniform to Cold War",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Snake", "swapToSnake")
        {

Note = "Uniform", 
        Price = 30,
            Description = "Changes Snake's uniform to Snake",
            Category = "Camo - Uniform",
            Image = "camo_uniform"
        },

    new ("Swap to Ga-Ko", "swapToGaKo")
        {

        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Ga-Ko",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to Desert Tiger", "swapToDesertTiger")
        {

        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Desert Tiger",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to DPM", "swapToDPM")
        {

        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to DPM",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to Flecktarn", "swapToFlecktarn")
        {

        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Flecktarn",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to Auscam", "swapToAuscam")
        {

        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Auscam",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to Animals", "swapToAnimals")
        {

        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Animals",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    new ("Swap to Fly", "swapToFly")
        {

        Note = "Uniform",
        Price = 30,
        Description = "Changes Snake's uniform to Fly",
        Category = "Camo - Uniform",
        Image = "camo_uniform"
        },

    #endregion

    #region Add/Remove Camos

    new ("Add Olive Drab", "giveOliveDrab")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Olive Drab camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Olive Drab", "removeOliveDrab")
        {
        Price = 60,
        Description = "Removes Olive Drab camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Tiger Stripe", "giveTigerStripe")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Tiger Stripe camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Tiger Stripe", "removeTigerStripe")
        {
        Price = 60,
        Description = "Removes Tiger Stripe camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Leaf", "giveLeaf")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Leaf camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Leaf", "removeLeaf")
        {
        Price = 60,
        Description = "Removes Leaf camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Tree Bark", "giveTreeBark")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Tree Bark camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Tree Bark", "removeTreeBark")
        {
        Price = 60,
        Description = "Removes Tree Bark camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Choco Chip", "giveChocoChip")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Choco Chip camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Choco Chip", "removeChocoChip")
        {
        Price = 60,
        Description = "Removes Choco Chip camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Splitter", "giveSplitter")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Splitter camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Splitter", "removeSplitter")
        {
        Price = 60,
        Description = "Removes Splitter camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Raindrop", "giveRaindrop")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Raindrop camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Raindrop", "removeRaindrop")
        {
        Price = 60,
        Description = "Removes Raindrop camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Squares", "giveSquares")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Squares camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Squares", "removeSquares")
        {
        Price = 60,
        Description = "Removes Squares camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Water", "giveWater")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Water camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Water", "removeWater")
        {
        Price = 60,
        Description = "Removes Water camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Black", "giveBlack")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Black camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Black", "removeBlack")
        {
        Price = 60,
        Description = "Removes Black camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Snow", "giveSnow")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Snow camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Snow", "removeSnow")
        {
        Price = 60,
        Description = "Removes Snow camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Sneaking Suit", "giveSneakingSuit")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Sneaking Suit camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Sneaking Suit", "removeSneakingSuit")
        {
        Price = 60,
        Description = "Removes Sneaking Suit camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Hornet Stripe", "giveHornetStripe")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Hornet Stripe camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Hornet Stripe", "removeHornetStripe")
        {
        Price = 60,
        Description = "Removes Hornet Stripe camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Spider", "giveSpider")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Spider camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Spider", "removeSpider")
        {
        Price = 60,
        Description = "Removes Spider camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Moss", "giveMoss")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Moss camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Moss", "removeMoss")
        {
        Price = 60,
        Description = "Removes Moss camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Fire", "giveFire")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Fire camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Fire", "removeFire")
        {
        Price = 60,
        Description = "Removes Fire camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Spirit", "giveSpirit")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Spirit camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Spirit", "removeSpirit")
        {
        Price = 60,
        Description = "Removes Spirit camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Cold War", "giveColdWar")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Cold War camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Cold War", "removeColdWar")
        {
        Price = 60,
        Description = "Removes Cold War camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Snake", "giveSnake")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Snake camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Snake", "removeSnake")
        {
        Price = 60,
        Description = "Removes Snake camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Ga-Ko", "giveGako")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Ga-Ko camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Ga-Ko", "removeGako")
        {
        Price = 60,
        Description = "Removes Ga-Ko camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Desert Tiger", "giveDesertTiger")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Desert Tiger camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Desert Tiger", "removeDesertTiger")
        {
        Price = 60,
        Description = "Removes Desert Tiger camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add DPM", "giveDPM")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds DPM camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove DPM", "removeDPM")
        {
        Price = 60,
        Description = "Removes DPM camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Flecktarn", "giveFlecktarn")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Flecktarn camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Flecktarn", "removeFlecktarn")
        {
        Price = 60,
        Description = "Removes Flecktarn camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Auscam", "giveAuscam")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Auscam camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Auscam", "removeAuscam")
        {
        Price = 60,
        Description = "Removes Auscam camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Animals", "giveAnimals")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Animals camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Animals", "removeAnimals")
        {
        Price = 60,
        Description = "Removes Animals camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Fly", "giveFly")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Fly camo to Snake's inventory",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Fly", "removeFly")
        {
        Price = 60,
        Description = "Removes Fly camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    new ("Add Banana", "giveBanana")
        {
        Note = "Uniform",
        Price = 60,
        Description = "Adds Banana camo to Snake's inventory, might appear without textures if mod is not installed",
        Category = "Camo - Uniform - Add",
        Image = "camo_give"
        },

    new ("Remove Banana", "removeBanana")
        {
        Price = 60,
        Description = "Removes Banana camo from Snake's inventory",
        Category = "Camo - Uniform - Remove",
        Image = "camo_remove"
        },

    #endregion

    #region HUD Effects

    new ("Remove Partial HUD", "removePartialHUD")
        {
        Price = 30,
        Duration = 60,
        Description = "Removes parts of the on-screen HUD for a limited time",
        Category = "Visual Effects"
        },

    new ("Remove Full HUD", "removeFullHUD")
        {
        Price = 30,
        Duration = 60,
        Description = "Completely hides the on-screen HUD for a limited time",
        Category = "Visual Effects"
        },

    new ("Set to Day Mode", "setToDayMode")
        {
        Price = 20,
        Description = "Changes the game visuals to daytime lighting",
        Category = "Visual Effects"
        },

    new ("Set to Night Mode", "setToNightMode")
        {
        Price = 20,
        Description = "Changes the game visuals to nighttime lighting",
        Category = "Visual Effects"
        },

    new ("Set to Foggy Mode", "setToFoggyMode")
        {
        Price = 20,
        Description = "Changes the game visuals to foggy weather",
        Category = "Visual Effects"
        },

    new ("Set to Muddy Fog Mode", "setToMuddyFogMode")
        {
        Price = 20,
        Description = "Changes the game visuals to muddy fog weather",
        Category = "Visual Effects"
        },

    new ("Set to Red Mist Mode", "setToRedMistMode")
        {
        Price = 20,
        Description = "Changes the game visuals to red mist weather",
        Category = "Visual Effects"
        },

    new ("Zoom Camera In", "zoomInFOV")
        {
        Price = 60,
        Duration = 30,
        Description = "Zooms the camera in to give a closer view of the action, which will probably also annoy the Streamer which is a bonus",
        Category = "Visual Effects"
        },

    new ("Zoom Camera Out", "zoomOutFOV")
        {
        Price = 60,
        Duration = 30,
        Description = "Zooms the camera out to give a wider view of the action, which will probably also annoy the Streamer which is a bonus",
        Category = "Visual Effects"
        },

    
    #endregion

    #region Items

    new ("Give Life Med", "giveLifeMedicine")
        {
        Price = 150,
        Description = "Gives Snake a Life Med to restore health",
        Category = "Items - Add",
        Image = "give_item"
        },

    new ("Remove Life Med", "removeLifeMedicine")
        {
        Price = 150,
        Description = "Removes a Life Medicine from Snake's inventory",
        Category = "Items - Remove",
        Image = "remove_item"
        },

    new ("Give Scope", "giveScope")
        {
        Price = 20,
        Description = "Gives Snake a binoculars to scout the area",
        Category = "Items - Add",
        Image = "give_item"
        },

    new ("Remove Scope", "removeScope")
        {
        Price = 20,
        Description = "No more long range scouting for Snake",
        Category = "Items - Remove",
        Image = "remove_item"
        },

    new ("Give Thermal Goggles", "giveThermalGoggles")
        {
        Price = 60,
        Description = "Gives Snake thermal goggles to see in the dark",
        Category = "Items - Add",
        Image = "give_item"
        },

    new ("Remove Thermal Goggles", "removeThermalGoggles")
        {
        Price = 60,
        Description = "Take away Snake's thermal goggles which will stop him from tracking heat signatures",
        Category = "Items - Remove",
        Image = "remove_item"
        },

    new ("Give Night Vision Goggles", "giveNightVisionGoggles")
        {
        Price = 60,
        Description = "Gives Snake NVGs to see in the dark",
        Category = "Items - Add",
        Image = "give_item"
        },

    new ("Remove Night Vision Goggles", "removeNightVisionGoggles")
        {
        Price = 60,
        Description = "Take away Snake's NVGs which will stop him from seeing in the dark. Pairs well with the effect to make it night time.",
        Category = "Items - Remove",
        Image = "remove_item"
        },

    new ("Give Motion Detector", "giveMotionDetector")
        {
        Price = 30,
        Description = "Gives Snake a motion detector to track enemy and animal movement",
        Category = "Items - Add",
        Image = "give_item"
        },

    new ("Remove Motion Detector", "removeMotionDetector")
        {
        Price = 30,
        Description = "Take away Snake's motion detector which will stop him from tracking enemy and animal movement",
        Category = "Items - Remove",
        Image = "remove_item"
        },

    new ("Give Sonar", "giveSonar")
        {
        Price = 30,
        Description = "Gives Snake a sonar to detect enemy and animal positions",
        Category = "Items - Add",
        Image = "give_item"
        },

    new ("Remove Sonar", "removeSonar")
        {
        Price = 30,
        Description = "Take away Snake's sonar which will stop him from detecting enemy and animal positions",
        Category = "Items - Remove",
        Image = "remove_item"
        },

    new ("Give Anti-Personnel Sensor", "giveAntiPersonnelSensor")
        {
        Price = 30,
        Description = "Gives Snake an anti-personnel sensor to detect enemy movement",
        Category = "Items - Add",
        Image = "give_item"
        },

    new ("Remove Anti-Personnel Sensor", "removeAntiPersonnelSensor")
        {
        Price = 30,
        Description = "Take away Snake's anti-personnel sensor which will stop him from detecting enemy movement",
        Category = "Items - Remove",
        Image = "remove_item"
        },

    new ("Give Antidote", "giveAntidote")
        {
        Price = 30,
        Description = "Gives Snake an antidote to cure certain poisons",
        Category = "Items (Medical) - Add",
        Image = "give_item_medical"
        },

    new ("Remove Antidote", "removeAntidote")
        {
        Price = 30,
        Description = "Removes an antidote from Snake's inventory",
        Category = "Items (Medical) - Remove",
        Image = "remove_item_medical"
        },

    new ("Give C Med", "giveCMed")
        {
        Price = 30,
        Description = "Gives Snake a C Med to cure colds",
        Category = "Items (Medical) - Add",
        Image = "give_item_medical"
        },

    new ("Remove C Med", "removeCMed")
        {
        Price = 30,
        Description = "Removes a C Med from Snake's inventory, the common cold is a mystery hope he doesn't catch it.",
        Category = "Items (Medical) - Remove",
        Image = "remove_item_medical"
        },

    new ("Give D Med", "giveDMed")
        {
        Price = 30,
        Description = "Gives Snake a D Med to cure Snake's stomach issues",
        Category = "Items (Medical) - Add",
        Image = "give_item_medical"
        },

    new ("Remove D Med", "removeDMed")
        {
        Price = 30,
        Description = "Removes a D Med from Snake's inventory, hope his stomach doesn't get upset somehow.",
        Category = "Items (Medical) - Remove",
        Image = "remove_item_medical"
        },

    new ("Give Serum", "giveSerum")
        {
        Price = 50,
        Description = "Gives Snake a serum to cure poison",
        Category = "Items (Medical) - Add",
        Image = "give_item_medical"
        },

    new ("Remove Serum", "removeSerum")
        {
        Price = 50,
        Description = "Removes a serum from Snake's inventory, sure would suck if he got poisoned.",
        Category = "Items (Medical) - Remove",
        Image = "remove_item_medical"
        },

    new ("Give Bandage", "giveBandage")
        {
        Price = 60,
        Description = "Gives Snake a bandage to stop bleeding",
        Category = "Items (Medical) - Add",
        Image = "give_item_medical"
        },

    new ("Remove Bandage", "removeBandage")
        {
        Price = 60,
        Description = "Removes a bandage from Snake's inventory, hope he doesn't get hurt.",
        Category = "Items (Medical) - Remove",
        Image = "remove_item_medical"
        },

    new ("Give Disinfectant", "giveDisinfectant")
        {
        Price = 30,
        Description = "Gives Snake a disinfectant to clean wounds",
        Category = "Items (Medical) - Add",
        Image = "give_item_medical"
        },

    new ("Remove Disinfectant", "removeDisinfectant")
        {
        Price = 30,
        Description = "Removes a disinfectant from Snake's inventory, hope he doesn't have to worry about an infection.",
        Category = "Items (Medical) - Remove",
        Image = "remove_item_medical"
        },

    new ("Give Ointment", "giveOintment")
        {
        Price = 30,
        Description = "Gives Snake an ointment to heal burns",
        Category = "Items (Medical) - Add",
        Image = "give_item_medical"
        },

    new ("Remove Ointment", "removeOintment")
        {
        Price = 30,
        Description = "Removes an ointment from Snake's inventory, getting burnt would not be ideal for Snake.",
        Category = "Items (Medical) - Remove",
        Image = "remove_item_medical"
        },

    new ("Give Splint", "giveSplint")
        {
        Price = 30,
        Description = "Gives Snake a splint to fix broken bones",
        Category = "Items (Medical) - Add",
        Image = "give_item_medical"
        },

    new ("Remove Splint", "removeSplint")
        {
        Price = 30,
        Description = "Removes a splint from Snake's inventory, what are the odds he gets thrown off a bridge again breaking all his bones?",
        Category = "Items (Medical) - Remove",
        Image = "remove_item_medical"
        },

    new ("Give Styptic", "giveStyptic")
        {
        Price = 30,
        Description = "Gives Snake a styptic to stop bleeding",
        Category = "Items (Medical) - Add",
        Image = "give_item_medical"
        },

    new ("Remove Styptic", "removeStyptic")
        {
        Price = 30,
        Description = "Removes a styptic from Snake's inventory, he probably doesn't need those.",
        Category = "Items (Medical) - Remove",
        Image = "remove_item_medical"
        },

    new ("Give Suture Kit", "giveSutureKit")
        {
        Price = 30,
        Description = "Gives Snake a suture kit to stitch up his cuts",
        Category = "Items (Medical) - Add",
        Image = "give_item_medical"
        },

    new ("Remove Suture Kit", "removeSutureKit")
        {
        Price = 30,
        Description = "Removes a suture kit from Snake's inventory, he's a CQC expert he probably won't get stabbed.",
        Category = "Items (Medical) - Remove",
        Image = "remove_item_medical"
        },



    #endregion

    #region Snake's Stat Related Effects

    new ("Set Snake Stamina to 0", "setSnakeStamina")
        {
        Price = 400,
        Description = "Drains Snake's stamina completely",
        Category = "Snake's Stats"
        },

    new ("Set Snake Max Stamina", "setSnakeMaxStamina")
        {
        Price = 400,
        Description = "Fully restores Snake's stamina bar",
        Category = "Snake's Stats"
        },

    new ("Snake gets Common Cold", "snakeHasTheCommonCold")
        {
        Price = 20,
        Description = "Inflicts Snake with a cold, causing sneezes to alert enemies",
        Category = "Snake's Stats"
        },

    new ("Poison Snake", "snakeIsPoisoned")
        {
        Price = 150,
        Description = "Poisons Snake, slowly draining his health",
        Category = "Snake's Stats"
        },

    new ("Snake has Food Poisoning", "snakeHasFoodPoisoning")
        {
        Price = 40,
        Description = "Gives Snake food poisoning, causing frequent nausea",
        Category = "Snake's Stats"
        },

    new ("Snake has Leeches", "snakeHasLeeches")
        {
        Price = 40,
        Description = "Attaches leeches to Snake, draining stamina until removed",
        Category = "Snake's Stats"
        },

    new ("Snake x2 Damage Multiplier", "setSnakeDamageX2")
        {
        Price = 80,
        Duration = 30,
        Description = "Doubles the damage Snake takes for a limited time",
        Category = "Snake's Stats"
        },

    new ("Snake x3 Damage Multiplier", "setSnakeDamageX3")
        {
        Price = 150,
        Duration = 30,
        Description = "Triples the damage Snake takes for a limited time",
        Category = "Snake's Stats"
        },

    new ("Snake x4 Damage Multiplier", "setSnakeDamageX4")
        {
        Price = 250,
        Duration = 30,
        Description = "Quadruples the damage Snake takes for a limited time",
        Category = "Snake's Stats"
        },

    new ("Snake x5 Damage Multiplier", "setSnakeDamageX5")
        {
        Price = 350,
        Duration = 30,
        Description = "Quintuples the damage Snake takes for a limited time",
        Category = "Snake's Stats"
        },

    new ("Set Snake's Camo Index to -100%", "setSnakeCamoIndexNegative")
        {
        Price = 150,
        Duration = 60,
        Description = "Sets Snake's camo index to -100 for a limited time",
        Category = "Snake's Stats"
        },

    new ("Set Snake's Camo Index to 100%", "setSnakeCamoIndexPositive")
        {
        Price = 150,
        Duration = 60,
        Description = "Sets Snake's camo index to 100 for a limited time",
        Category = "Snake's Stats"
        },

    new ("Set Snake's Camo Index to 50%", "setSnakeCamoIndexFifty")
        {
        Price = 80,
        Duration = 60,
        Description = "Sets Snake's camo index to 50 for a limited time",
        Category = "Snake's Stats"
        },

    new ("Set Snake's Camo Index to -50%", "setSnakeCamoIndexNegativeFifty")
        {
        Price = 80,
        Duration = 60,
        Description = "Sets Snake's camo index to -50 for a limited time",
        Category = "Snake's Stats"
        },

    new ("Set Snake's Camo Index to 0%", "setSnakeCamoIndexZero")
        {
        Price = 40,
        Duration = 60,
        Description = "Sets Snake's camo index to 0 for a limited time",
        Category = "Snake's Stats"
        },

    #endregion

    #region Snake's Animation Effects

    new ("Snake Nap Time", "makeSnakeQuickSleep")
        {
        Price = 80,
        Description = "Puts Snake to sleep instantly",
        Category = "Snake's Animations"
        },

    new ("Snake Pukes and gets set on Fire", "makeSnakePukeFire")
        {
        Price = 400,
        Description = "Causes Snake to vomit explosively and catch fire",
        Category = "Snake's Animations"
        },

    new ("Snake Pukes", "makeSnakePuke")
        {
        Price = 150,
        Description = "Causes Snake to vomit",
        Category = "Snake's Animations"
        },

    new ("Set Snake on Fire", "setSnakeOnFire")
        {
        Price = 250,
        Description = "Sets Snake on fire, causing him to take damage over time",
        Category = "Snake's Animations"
        },

    new ("Snake Bunny Hop", "makeSnakeBunnyHop")
        {
        Price = 80,
        Duration = 10,
        Description = "Makes Snake repeatedly jump like a bunny for a short time",
        Category = "Snake's Animations"
        },

    new ("Snake Freeze in Place", "makeSnakeFreeze")
        {
        Price = 80,
        Duration = 5,
        Description = "Immobilizes Snake completely for a short duration",
        Category = "Snake's Animations"
        },

    new ("Make Snake Jump", "makeSnakeJump")
        {
        Price = 150,
        Description = "Forces Snake to jump unexpectedly",
        Category = "Snake's Animations"
        },

    new ("Make Snake Crouch", "makeSnakeCrouch")
        {
        Price = 80,
        Duration = 5,
        Description = "Forces Snake to crouch unexpectedly",
        Category = "Snake's Animations"
        },

    #endregion

    #region Guard Stats

    new ("Guards are Almost Invincible", "setGuardStatsAlmostInvincible")
        {
        Price = 250,
        Duration = 40,
        Description = "Guards become almost invincible to lethal, sleep, and stun damage",
        Category = "Guard Stats"
        },

    new ("Guards become Very Strong", "setGuardStatsVeryStrong")
        {
        Price = 150,
        Duration = 40,
        Description = "Guards become very strong against lethal, sleep, and stun damage",
        Category = "Guard Stats"
        },

    new ("Guards become Very Weak", "setGuardStatsVeryWeak")
        {
        Price = 150,
        Duration = 40,
        Description = "Guards become very weak against lethal, sleep, and stun damage",
        Category = "Guard Stats"
        },

    new ("Guards can be One Shot", "setGuardStatsOneShot")
        {
        Price = 250,
        Duration = 40,
        Description = "Guards become one shot by lethal, sleep, and stun damage",
        Category = "Guard Stats"
        },

    #endregion

    };

    #endregion

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
        var codeParams = FinalCode(request).Split('_');
        switch (codeParams[0])
        {

            #region Weapons

            case "subtractAmmo":
                {
                    if (!int.TryParse(codeParams[1], out int quantity))
                    {
                        Respond(request, EffectStatus.FailTemporary, StandardErrors.CannotParseNumber, codeParams[1]);
                        break;
                    }

                    if (IsInCutscene())
                    {
                        DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                        break;
                    }

                    TryEffect(request,
                        () => true,
                        () => TrySubtractAmmoFromCurrentWeapon((short)quantity),
                        () => Connector.SendMessage($"{request.DisplayViewer} subtracted {quantity} ammo from {GetCurrentEquippedWeapon()?.Name ?? "Unknown Weapon"}."),
                        retryOnFail: false);
                    break;
                }

            case "addAmmo":
                {
                    if (!int.TryParse(codeParams[1], out int quantity) || IsInCutscene())
                    {
                        Respond(request, EffectStatus.FailTemporary, StandardErrors.CannotParseNumber, codeParams[1]);
                        break;
                    }

                    TryEffect(request,
                        () => true,
                        () => TryAddAmmoToCurrentWeapon((short)quantity),
                        () => Connector.SendMessage($"{request.DisplayViewer} added {quantity} ammo to {GetCurrentEquippedWeapon()?.Name ?? "Unknown Weapon"}."),
                        retryOnFail: false);
                    break;
                }

            case "emptyCurrentWeaponClip":
                {
                    var emptyClipDuration = request.Duration;

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
                        {
                            Connector.SendMessage("Emptying Snake's weapon clip effect has ended.");
                        });

                    break;
                }

            case "setSnakeCurrentWeaponToNone":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var unequipSnakeWeapon = request.Duration;
                var unequipSnakeWeaponAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} unequipped Snake's current weapon for {unequipSnakeWeapon.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                () =>
                    {
                        SetSnakeCurrentWeaponToNone();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                false);
                unequipSnakeWeaponAct.WhenCompleted.Then
                    (_ =>
                {
                    Connector.SendMessage("Snake's weapon has been re-equipped.");
                });
                break;

            case "removeCurrentSuppressor":
                {
                    Weapon currentWeapon = GetCurrentEquippedWeapon();
                    if (IsInCutscene())
                    {
                        DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                        return;
                    }

                    if (currentWeapon != MGS3UsableObjects.M1911A1 &&
                        currentWeapon != MGS3UsableObjects.MK22 &&
                        currentWeapon != MGS3UsableObjects.XM16E1)
                    {
                        Respond(request, EffectStatus.FailTemporary, StandardErrors.PrerequisiteNotFound, "A weapon with a suppressor");
                        return;
                    }

                    RepeatAction(
                        request,
                        () => true,
                        () => true,
                        TimeSpan.Zero,
                        () => IsReady(request),
                        TimeSpan.FromMilliseconds(100),
                        () =>
                        {
                            Weapon weapon = GetCurrentEquippedWeapon();
                            if (weapon is { HasSuppressor: true } &&
                                (weapon == MGS3UsableObjects.M1911A1 ||
                                 weapon == MGS3UsableObjects.MK22 ||
                                 weapon == MGS3UsableObjects.XM16E1))
                            {
                                AddressChain suppressorAddress = weapon.GetPropertyAddress(baseWeaponAddress, WeaponAddresses.SuppressorToggleOffset);
                                // Force the suppressor off.
                                Set8(suppressorAddress, 0);
                            }
                            return true;
                        },
                        TimeSpan.FromMilliseconds(100),
                        false
                    ).WhenCompleted.Then(_ =>
                    {
                        Connector.SendMessage("Suppressors can be equipped again.");
                    });

                    break;
                }

            #endregion

            #region Alert Status

            case "setAlertStatus":
                {
                    // Combine the two checks: if we are in a cutscene OR the map doesn't allow alerts, disallow the effect.
                    if (IsInCutscene() || !IsAlertAllowedOnCurrentMap())
                    {
                        DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                        return;
                    }

                    TryEffect(request,
                        () => true,
                        () =>
                        {
                            SetAlertStatus();
                            return true;
                        },
                        () => Connector.SendMessage($"{request.DisplayViewer} set the game to Alert Status."));
                    break;
                }

            case "setEvasionStatus":
                if (IsInCutscene() || !IsAlertAllowedOnCurrentMap())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
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
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Evasion Status."));
                break;

            case "setCautionStatus":
                if (IsInCutscene() || !IsAlertAllowedOnCurrentMap())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetCautionStatus();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Caution Status."));
                break;

            #endregion

            #region HUD and Filters

            case "removePartialHUD":
                var removePartialHUDDuration = request.Duration;
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
                var removeFullHUDDuration = request.Duration;
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
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Day Mode."));
                break;

            case "setToNightMode":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetToNightMode();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Night Mode."));
                break;

            case "setToFoggyMode":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetToFoggyMode();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Foggy Mode."));
                break;

            case "setToMuddyFogMode":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetToMuddyFogMode();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Muddy Fog Mode."));
                break;

            case "setToRedMistMode":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetToRedMistMode();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set the game to Red Mist Mode."));
                break;

            case "zoomInFOV":
                if (!IsNormalFOV() || IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var zoomInFOVDuration = request.Duration;
                var zoomInFOVAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} zoomed the camera in for {zoomInFOVDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetZoomInFOV();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                zoomInFOVAct.WhenCompleted.Then
                    (_ =>
                {
                    SetNormalFOV();
                    Connector.SendMessage("Camera zoom effect has ended.");
                });
                break;

            // Zoom Camera Out
            case "zoomOutFOV":
                if (!IsNormalFOV() || IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var zoomOutFOVDuration = request.Duration;
                var zoomOutFOVAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} zoomed the camera out for {zoomOutFOVDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetZoomOutFOV();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                zoomOutFOVAct.WhenCompleted.Then

                    (_ =>
                {
                    SetNormalFOV();
                    Connector.SendMessage("Camera zoom effect has ended.");
                });
                break;


            #endregion

            #region Camo - Face Paint

            case "swapToNoFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.NoPaint);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Snake's face paint."));
                break;


            case "swapToWoodlandFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Woodland);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Woodland."));
                break;

            case "swapToBlackFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Black);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Black."));
                break;

            case "swapToWaterFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Water);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Water."));
                break;

            case "swapToDesertFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Desert);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Desert."));
                break;

            case "swapToSplitterFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Splitter);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Splitter."));
                break;

            case "swapToSnowFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Snow);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Snow."));
                break;

            case "swapToKabukiFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Kabuki);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Kabuki."));
                break;

            case "swapToZombieFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Zombie);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Zombie."));
                break;

            case "swapToOyamaFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Oyama);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Oyama."));
                break;

            case "swapToGreenFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Green);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Green."));
                break;

            case "swapToBrownFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Brown);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Brown."));
                break;

            case "swapToSovietUnionFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.SovietUnion);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Soviet Union."));
                break;

            case "swapToUKFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.UK);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to UK."));
                break;

            case "swapToFranceFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.France);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to France."));
                break;

            case "swapToSpainFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Spain);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Spain."));
                break;

            case "swapToSwedenFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Sweden);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Sweden."));
                break;

            case "swapToItalyFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Italy);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Italy."));
                break;

            case "swapToGermanyFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Germany);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Germany."));
                break;

            case "swapToJapanFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.Japan);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to Japan."));
                break;


            case "swapToUSAFacePaint":
                if (IsInCutscene() || IsMaskEquipped())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.WrongMode);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapFacePaint(SnakesFacePaint.USA);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's face paint to USA."));
                break;



            #endregion

            #region Camo - Uniform

            case "swapToOliveDrab":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.OliveDrab);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Olive Drab."));
                break;

            case "swapToTigerStripe":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.TigerStripe);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Tiger Stripe."));
                break;

            case "swapToLeaf":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Leaf);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Leaf."));
                break;

            case "swapToTreeBark":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.TreeBark);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Tree Bark."));
                break;

            case "swapToChocoChip":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.ChocoChip);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Choco Chip."));
                break;

            case "swapToSplitter":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Splitter);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Splitter."));
                break;

            case "swapToRaindrop":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Raindrop);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Raindrop."));
                break;

            case "swapToSquares":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Squares);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Squares."));
                break;

            case "swapToWater":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Water);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Water."));
                break;

            case "swapToBlack":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Black);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Black."));
                break;

            case "swapToSnow":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Snow);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Snow."));
                break;

            case "swapToNaked":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Naked);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Naked."));
                break;

            case "swapToSneakingSuit":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.SneakingSuit);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Sneaking Suit."));
                break;



            case "swapToHornetStripe":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.HornetStripe);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Hornet Stripe."));
                break;

            case "swapToSpider":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Spider);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Spider."));
                break;

            case "swapToMoss":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Moss);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Moss."));
                break;

            case "swapToFire":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Fire);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Fire."));
                break;

            case "swapToSpirit":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Spirit);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Spirit."));
                break;

            case "swapToColdWar":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.ColdWar);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Cold War."));
                break;

            case "swapToSnake":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Snake);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Snake."));
                break;

            case "swapToGaKo":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.GakoCamo);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Ga-Ko."));
                break;

            case "swapToDesertTiger":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.DesertTiger);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Desert Tiger."));
                break;

            case "swapToDPM":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.DPM);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to DPM."));
                break;

            case "swapToFlecktarn":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Flecktarn);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Flecktarn."));
                break;

            case "swapToAuscam":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Auscam);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Auscam."));
                break;

            case "swapToAnimals":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Animals);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Animals."));
                break;

            case "swapToFly":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SwapUniform(SnakesUniformCamo.Fly);
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} swapped Snake's uniform to Fly."));
                break;

            #endregion

            #region Camo - Uniform Add/Remove

            case "giveOliveDrab":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.OliveDrab) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.OliveDrab);
                        SetItemValue(MGS3UsableObjects.OliveDrab, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Olive Drab."));
                break;

            case "removeOliveDrab":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.OliveDrab) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.OliveDrab);
                        SetItemValue(MGS3UsableObjects.OliveDrab, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Olive Drab from Snake."));
                break;

            case "giveTigerStripe":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.TigerStripe) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.TigerStripe);
                        SetItemValue(MGS3UsableObjects.TigerStripe, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Tiger Stripe."));
                break;

            case "removeTigerStripe":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.TigerStripe) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.TigerStripe);
                        SetItemValue(MGS3UsableObjects.TigerStripe, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Tiger Stripe from Snake."));
                break;

            case "giveLeaf":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Leaf) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Leaf);
                        SetItemValue(MGS3UsableObjects.Leaf, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Leaf."));
                break;


            case "removeLeaf":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Leaf) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Leaf);
                        SetItemValue(MGS3UsableObjects.Leaf, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Leaf from Snake."));
                break;

            case "giveTreeBark":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.TreeBark) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.TreeBark);
                        SetItemValue(MGS3UsableObjects.TreeBark, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Tree Bark."));
                break;

            case "removeTreeBark":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.TreeBark) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.TreeBark);
                        SetItemValue(MGS3UsableObjects.TreeBark, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Tree Bark from Snake."));
                break;

            case "giveChocoChip":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.ChocoChip) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.ChocoChip);
                        SetItemValue(MGS3UsableObjects.ChocoChip, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Choco Chip."));
                break;

            case "removeChocoChip":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.ChocoChip) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.ChocoChip);
                        SetItemValue(MGS3UsableObjects.ChocoChip, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Choco Chip from Snake."));
                break;

            case "giveSplitter":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Splitter) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Splitter);
                        SetItemValue(MGS3UsableObjects.Splitter, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Splitter."));
                break;

            case "removeSplitter":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Splitter) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Splitter);
                        SetItemValue(MGS3UsableObjects.Splitter, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Splitter from Snake."));
                break;

            case "giveRaindrop":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Raindrop) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Raindrop);
                        SetItemValue(MGS3UsableObjects.Raindrop, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Raindrop."));
                break;

            case "removeRaindrop":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Raindrop) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Raindrop);
                        SetItemValue(MGS3UsableObjects.Raindrop, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Raindrop from Snake."));
                break;

            case "giveSquares":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Squares) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Squares);
                        SetItemValue(MGS3UsableObjects.Squares, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Squares."));
                break;

            case "removeSquares":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Squares) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Squares);
                        SetItemValue(MGS3UsableObjects.Squares, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Squares from Snake."));
                break;

            case "giveWater":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Water) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Water);
                        SetItemValue(MGS3UsableObjects.Water, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Water."));
                break;

            case "removeWater":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Water) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Water);
                        SetItemValue(MGS3UsableObjects.Water, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Water from Snake."));
                break;

            case "giveBlack":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Black) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Black);
                        SetItemValue(MGS3UsableObjects.Black, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Black."));
                break;

            case "removeBlack":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Black) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Black);
                        SetItemValue(MGS3UsableObjects.Black, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Black from Snake."));
                break;

            case "giveSnow":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Snow) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Snow);
                        SetItemValue(MGS3UsableObjects.Snow, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Snow."));
                break;

            case "removeSnow":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Snow) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Snow);
                        SetItemValue(MGS3UsableObjects.Snow, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Snow from Snake."));
                break;

            case "giveSneakingSuit":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.SneakingSuit) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.SneakingSuit);
                        SetItemValue(MGS3UsableObjects.SneakingSuit, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Sneaking Suit."));
                break;

            case "removeSneakingSuit":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.SneakingSuit) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.SneakingSuit);
                        SetItemValue(MGS3UsableObjects.SneakingSuit, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Sneaking Suit from Snake."));
                break;

            case "giveHornetStripe":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.HornetStripe) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.HornetStripe);
                        SetItemValue(MGS3UsableObjects.HornetStripe, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Hornet Stripe."));
                break;

            case "removeHornetStripe":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.HornetStripe) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.HornetStripe);
                        SetItemValue(MGS3UsableObjects.HornetStripe, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Hornet Stripe from Snake."));
                break;

            case "giveSpider":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Spider) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Spider);
                        SetItemValue(MGS3UsableObjects.Spider, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Spider."));
                break;

            case "removeSpider":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Spider) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Spider);
                        SetItemValue(MGS3UsableObjects.Spider, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Spider from Snake."));
                break;

            case "giveMoss":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Moss) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Moss);
                        SetItemValue(MGS3UsableObjects.Moss, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Moss."));
                break;

            case "removeMoss":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Moss) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Moss);
                        SetItemValue(MGS3UsableObjects.Moss, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Moss from Snake."));
                break;

            case "giveFire":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Fire) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Fire);
                        SetItemValue(MGS3UsableObjects.Fire, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Fire."));
                break;

            case "removeFire":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Fire) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Fire);
                        SetItemValue(MGS3UsableObjects.Fire, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Fire from Snake."));
                break;

            case "giveSpirit":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Spirit) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Spirit);
                        SetItemValue(MGS3UsableObjects.Spirit, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Spirit."));
                break;

            case "removeSpirit":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Spirit) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Spirit);
                        SetItemValue(MGS3UsableObjects.Spirit, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Spirit from Snake."));
                break;

            case "giveColdWar":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.ColdWar) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.ColdWar);
                        SetItemValue(MGS3UsableObjects.ColdWar, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Cold War."));
                break;

            case "removeColdWar":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.ColdWar) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.ColdWar);
                        SetItemValue(MGS3UsableObjects.ColdWar, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Cold War from Snake."));
                break;

            case "giveSnake":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Snake) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Snake);
                        SetItemValue(MGS3UsableObjects.Snake, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Snake."));
                break;

            case "removeSnake":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Snake) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Snake);
                        SetItemValue(MGS3UsableObjects.Snake, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Snake from Snake."));
                break;

            case "giveGako":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.GakoCamo) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.GakoCamo);
                        SetItemValue(MGS3UsableObjects.GakoCamo, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Gako."));
                break;

            case "removeGako":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.GakoCamo) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.GakoCamo);
                        SetItemValue(MGS3UsableObjects.GakoCamo, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Gako from Snake."));
                break;

            case "giveDesertTiger":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.DesertTiger) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.DesertTiger);
                        SetItemValue(MGS3UsableObjects.DesertTiger, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Desert Tiger."));
                break;

            case "removeDesertTiger":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.DesertTiger) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.DesertTiger);
                        SetItemValue(MGS3UsableObjects.DesertTiger, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Desert Tiger from Snake."));
                break;

            case "giveDPM":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.DPM) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.DPM);
                        SetItemValue(MGS3UsableObjects.DPM, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake DPM."));
                break;

            case "removeDPM":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.DPM) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.DPM);
                        SetItemValue(MGS3UsableObjects.DPM, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed DPM from Snake."));
                break;

            case "giveFlecktarn":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Flecktarn) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Flecktarn);
                        SetItemValue(MGS3UsableObjects.Flecktarn, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Flecktarn."));
                break;

            case "removeFlecktarn":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Flecktarn) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Flecktarn);
                        SetItemValue(MGS3UsableObjects.Flecktarn, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Flecktarn from Snake."));
                break;

            case "giveAuscam":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Auscam) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Auscam);
                        SetItemValue(MGS3UsableObjects.Auscam, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Auscam."));
                break;

            case "removeAuscam":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Auscam) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Auscam);
                        SetItemValue(MGS3UsableObjects.Auscam, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Auscam from Snake."));
                break;

            case "giveAnimals":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Animals) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Animals);
                        SetItemValue(MGS3UsableObjects.Animals, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Animals."));
                break;

            case "removeAnimals":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Animals) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Animals);
                        SetItemValue(MGS3UsableObjects.Animals, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Animals from Snake."));
                break;

            case "giveFly":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Fly) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Fly);
                        SetItemValue(MGS3UsableObjects.Fly, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Fly."));
                break;

            case "removeFly":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Fly) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Fly);
                        SetItemValue(MGS3UsableObjects.Fly, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Fly from Snake."));
                break;

            case "giveBanana":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.BananaCamo) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.BananaCamo);
                        SetItemValue(MGS3UsableObjects.BananaCamo, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake Banana."));
                break;

            case "removeBanana":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.BananaCamo) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.BananaCamo);
                        SetItemValue(MGS3UsableObjects.BananaCamo, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed Banana camo from Snake."));
                break;

            #endregion

            #region Items

            case "giveLifeMedicine":
                if (IsInCutscene() || (GetItemMaxCapacity(MGS3UsableObjects.LifeMedicine) <= GetItemValue(MGS3UsableObjects.LifeMedicine)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.LifeMedicine);
                        SetItemValue(MGS3UsableObjects.LifeMedicine, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a life med. Snake now has {GetItemValue(MGS3UsableObjects.LifeMedicine)} life med(s)."));
                break;


            case "removeLifeMedicine":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.LifeMedicine) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.LifeMedicine);
                        SetItemValue(MGS3UsableObjects.LifeMedicine, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a life medicine from Snake, he now has {GetItemValue(MGS3UsableObjects.LifeMedicine)} life med(s)."));
                break;

            case "giveScope":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Binoculars) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Binoculars);
                        SetItemValue(MGS3UsableObjects.Binoculars, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a scope."));
                break;

            case "removeScope":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.Binoculars) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Binoculars);
                        SetItemValue(MGS3UsableObjects.Binoculars, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a cigar from Snake, guess he is quitting smoking early."));
                break;

            case "giveThermalGoggles":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.ThermalGoggles) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.ThermalGoggles);
                        SetItemValue(MGS3UsableObjects.ThermalGoggles, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake thermal goggles."));
                break;

            case "removeThermalGoggles":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.ThermalGoggles) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.ThermalGoggles);
                        SetItemValue(MGS3UsableObjects.ThermalGoggles, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed thermal goggles from Snake."));
                break;

            case "giveNightVisionGoggles":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.NightVisionGoggles) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.NightVisionGoggles);
                        SetItemValue(MGS3UsableObjects.NightVisionGoggles, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake night vision goggles."));
                break;

            case "removeNightVisionGoggles":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.NightVisionGoggles) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.NightVisionGoggles);
                        SetItemValue(MGS3UsableObjects.NightVisionGoggles, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed night vision goggles from Snake."));
                break;

            case "giveMotionDetector":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.MotionDetector) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.MotionDetector);
                        SetItemValue(MGS3UsableObjects.MotionDetector, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a motion detector."));
                break;

            case "removeMotionDetector":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.MotionDetector) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.MotionDetector);
                        SetItemValue(MGS3UsableObjects.MotionDetector, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a motion detector from Snake."));
                break;

            case "giveSonar":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.ActiveSonar) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.ActiveSonar);
                        SetItemValue(MGS3UsableObjects.ActiveSonar, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a sonar."));
                break;

            case "removeSonar":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.ActiveSonar) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.ActiveSonar);
                        SetItemValue(MGS3UsableObjects.ActiveSonar, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a sonar from Snake."));
                break;

            case "giveAntiPersonnelSensor":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.AntiPersonnelSensor) == 1))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.AntiPersonnelSensor);
                        SetItemValue(MGS3UsableObjects.AntiPersonnelSensor, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake an anti-personnel sensor."));
                break;

            case "removeAntiPersonnelSensor":
                if (IsInCutscene() || (GetItemValue(MGS3UsableObjects.AntiPersonnelSensor) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.AntiPersonnelSensor);
                        SetItemValue(MGS3UsableObjects.AntiPersonnelSensor, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed an anti-personnel sensor from Snake."));
                break;

            case "giveAntidote":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MGS3UsableObjects.Antidote) <= GetItemValue(MGS3UsableObjects.Antidote)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Antidote);
                        SetItemValue(MGS3UsableObjects.Antidote, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake an antidote. Snake now has {GetItemValue(MGS3UsableObjects.Antidote)} antidote(s)."));
                break;

            case "removeAntidote":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MGS3UsableObjects.Antidote) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Antidote);
                        SetItemValue(MGS3UsableObjects.Antidote, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed an antidote from Snake, he now has {GetItemValue(MGS3UsableObjects.Antidote)} antidote(s)."));
                break;

            case "giveCMed":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MGS3UsableObjects.ColdMedicine) <= GetItemValue(MGS3UsableObjects.ColdMedicine)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.ColdMedicine);
                        SetItemValue(MGS3UsableObjects.ColdMedicine, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a C Med. Snake now has {GetItemValue(MGS3UsableObjects.ColdMedicine)} C Med(s)."));
                break;

            case "removeCMed":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MGS3UsableObjects.ColdMedicine) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.ColdMedicine);
                        SetItemValue(MGS3UsableObjects.ColdMedicine, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a C Med from Snake, he now has {GetItemValue(MGS3UsableObjects.ColdMedicine)} C Med(s)."));
                break;


            case "giveDMed":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MGS3UsableObjects.DigestiveMedicine) <= GetItemValue(MGS3UsableObjects.DigestiveMedicine)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.DigestiveMedicine);
                        SetItemValue(MGS3UsableObjects.DigestiveMedicine, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a D Med. Snake now has {GetItemValue(MGS3UsableObjects.DigestiveMedicine)} D Med(s)."));
                break;

            case "removeDMed":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MGS3UsableObjects.DigestiveMedicine) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.DigestiveMedicine);
                        SetItemValue(MGS3UsableObjects.DigestiveMedicine, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a D Med from Snake, he now has {GetItemValue(MGS3UsableObjects.DigestiveMedicine)} D Med(s)."));
                break;

            case "giveSerum":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MGS3UsableObjects.Serum) <= GetItemValue(MGS3UsableObjects.Serum)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Serum);
                        SetItemValue(MGS3UsableObjects.Serum, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a serum. Snake now has {GetItemValue(MGS3UsableObjects.Serum)} serum(s)."));
                break;

            case "removeSerum":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MGS3UsableObjects.Serum) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Serum);
                        SetItemValue(MGS3UsableObjects.Serum, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a serum from Snake, he now has {GetItemValue(MGS3UsableObjects.Serum)} serum(s)."));
                break;

            case "giveBandage":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MGS3UsableObjects.Bandage) <= GetItemValue(MGS3UsableObjects.Bandage)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Bandage);
                        SetItemValue(MGS3UsableObjects.Bandage, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a bandage. Snake now has {GetItemValue(MGS3UsableObjects.Bandage)} bandage(s)."));
                break;

            case "removeBandage":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MGS3UsableObjects.Bandage) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Bandage);
                        SetItemValue(MGS3UsableObjects.Bandage, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a bandage from Snake, he now has {GetItemValue(MGS3UsableObjects.Bandage)} bandage(s)."));
                break;

            case "giveDisinfectant":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MGS3UsableObjects.Disinfectant) <= GetItemValue(MGS3UsableObjects.Disinfectant)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Disinfectant);
                        SetItemValue(MGS3UsableObjects.Disinfectant, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a disinfectant. Snake now has {GetItemValue(MGS3UsableObjects.Disinfectant)} disinfectant(s)."));
                break;

            case "removeDisinfectant":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MGS3UsableObjects.Disinfectant) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Disinfectant);
                        SetItemValue(MGS3UsableObjects.Disinfectant, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a disinfectant from Snake, he now has {GetItemValue(MGS3UsableObjects.Disinfectant)} disinfectant(s)."));
                break;

            case "giveOintment":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MGS3UsableObjects.Ointment) <= GetItemValue(MGS3UsableObjects.Ointment)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Ointment);
                        SetItemValue(MGS3UsableObjects.Ointment, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake an ointment. Snake now has {GetItemValue(MGS3UsableObjects.Ointment)} ointment(s)."));
                break;

            case "removeOintment":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MGS3UsableObjects.Ointment) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Ointment);
                        SetItemValue(MGS3UsableObjects.Ointment, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed an ointment from Snake, he now has {GetItemValue(MGS3UsableObjects.Ointment)} ointment(s)."));
                break;

            case "giveSplint":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MGS3UsableObjects.Splint) <= GetItemValue(MGS3UsableObjects.Splint)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Splint);
                        SetItemValue(MGS3UsableObjects.Splint, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a splint. Snake now has {GetItemValue(MGS3UsableObjects.Splint)} splint(s)."));
                break;

            case "removeSplint":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MGS3UsableObjects.Splint) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Splint);
                        SetItemValue(MGS3UsableObjects.Splint, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a splint from Snake, he now has {GetItemValue(MGS3UsableObjects.Splint)} splint(s)."));
                break;

            case "giveStyptic":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MGS3UsableObjects.Styptic) <= GetItemValue(MGS3UsableObjects.Styptic)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Styptic);
                        SetItemValue(MGS3UsableObjects.Styptic, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a styptic. Snake now has {GetItemValue(MGS3UsableObjects.Styptic)} styptic(s)."));
                break;

            case "removeStyptic":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MGS3UsableObjects.Styptic) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.Styptic);
                        SetItemValue(MGS3UsableObjects.Styptic, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a styptic from Snake, he now has {GetItemValue(MGS3UsableObjects.Styptic)} styptic(s)."));
                break;

            case "giveSutureKit":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemMaxCapacity(MGS3UsableObjects.SutureKit) <= GetItemValue(MGS3UsableObjects.SutureKit)))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.SutureKit);
                        SetItemValue(MGS3UsableObjects.SutureKit, (short)(currentValue + 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake a suture kit. Snake now has {GetItemValue(MGS3UsableObjects.SutureKit)} suture kit(s)."));
                break;

            case "removeSutureKit":
                if (IsInCutscene() || !IsMedicalItemEffectsAllowed() || (GetItemValue(MGS3UsableObjects.SutureKit) == 0))
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        short currentValue = GetItemValue(MGS3UsableObjects.SutureKit);
                        SetItemValue(MGS3UsableObjects.SutureKit, (short)(currentValue - 1));
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} removed a suture kit from Snake, he now has {GetItemValue(MGS3UsableObjects.SutureKit)} suture kit(s)."));
                break;

            #endregion

            #region Snake's Stats

            case "setSnakeStamina":
                {
                    if (IsInCutscene())
                    {
                        DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                        return;
                    }
                    TryEffect(request,
                        () => true,
                        () =>
                        {
                            SetSnakeStamina();
                            return true;
                        },
                        () => Connector.SendMessage($"{request.DisplayViewer} set Snake's stamina to 0."));
                    break;
                }

            case "setSnakeMaxStamina":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetSnakeMaxStamina();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's Stamina to 30000."));
                break;

            case "makeSnakeJump":
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        IncreaseSnakeYCoordBy2000();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake jump."));
                break;

            case "snakeHasTheCommonCold":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeHasTheCommonCold();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake the common cold."));
                break;

            case "snakeIsPoisoned":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeIsPoisoned();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} poisoned Snake."));
                break;

            case "snakeHasFoodPoisoning":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeHasFoodPoisoning();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake food poisoning."));
                break;

            case "snakeHasLeeches":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SnakeHasLeeches();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} gave Snake leeches."));
                break;

            case "setSnakeDamageX2":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var damageX2Duration = request.Duration;
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
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var damageX3Duration = request.Duration;
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
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var damageX4Duration = request.Duration;
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
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var damageX5Duration = request.Duration;
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

            case "setSnakeCamoIndexNegative":
                if (IsInCutscene() || !IsCamoIndexInstructionNormal())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var camoIndexNegativeDuration = request.Duration;
                var camoIndexNegativeAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's camo index to -1000 for {camoIndexNegativeDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeCamoIndexInstructionToWritable();
                        SetSnakeCamoIndexValue(-1000);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                camoIndexNegativeAct.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeCamoIndexValue(0);
                    SetSnakeCamoIndexInstructionToNormal();
                    Connector.SendMessage("Snake's camo index is back to normal.");
                });
                break;

            case "setSnakeCamoIndexPositive":
                if (IsInCutscene() || !IsCamoIndexInstructionNormal())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var camoIndexPositiveDuration = request.Duration;
                var camoIndexPositiveAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's camo index to 1000 for {camoIndexPositiveDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeCamoIndexInstructionToWritable();
                        SetSnakeCamoIndexValue(1000);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                camoIndexPositiveAct.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeCamoIndexValue(0);
                    SetSnakeCamoIndexInstructionToNormal();
                    Connector.SendMessage("Snake's camo index is back to normal.");
                });
                break;

            case "setSnakeCamoIndexZero":
                if (IsInCutscene() || !IsCamoIndexInstructionNormal())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var camoIndexZeroDuration = request.Duration;
                var camoIndexZeroAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's camo index to 0 for {camoIndexZeroDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeCamoIndexInstructionToWritable();
                        SetSnakeCamoIndexValue(0);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                camoIndexZeroAct.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeCamoIndexValue(0);
                    SetSnakeCamoIndexInstructionToNormal();
                    Connector.SendMessage("Snake's camo index is back to normal.");
                });
                break;

            case "setSnakeCamoIndexFifty":
                if (IsInCutscene() || !IsCamoIndexInstructionNormal())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var camoIndexFiftyDuration = request.Duration;
                var camoIndexFiftyAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's camo index to 50 for {camoIndexFiftyDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeCamoIndexInstructionToWritable();
                        SetSnakeCamoIndexValue(500);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                camoIndexFiftyAct.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeCamoIndexValue(0);
                    SetSnakeCamoIndexInstructionToNormal();
                    Connector.SendMessage("Snake's camo index is back to normal.");
                });
                break;

            case "setSnakeCamoIndexNegativeFifty":
                if (IsInCutscene() || !IsCamoIndexInstructionNormal())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var camoIndexNegativeFiftyDuration = request.Duration;
                var camoIndexNegativeFiftyAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake's camo index to -50 for {camoIndexNegativeFiftyDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        SetSnakeCamoIndexInstructionToWritable();
                        SetSnakeCamoIndexValue(-500);
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);
                camoIndexNegativeFiftyAct.WhenCompleted.Then
                    (_ =>
                {
                    SetSnakeCamoIndexValue(0);
                    SetSnakeCamoIndexInstructionToNormal();
                    Connector.SendMessage("Snake's camo index is back to normal.");
                });
                break;


            #endregion

            #region Snake's Animations

            case "makeSnakeQuickSleep":
                if (IsInCutscene() || !IsSleepAllowedOnCurrentMap())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        MakeSnakeQuickSleep();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake quick sleep."));
                break;

            case "makeSnakePukeFire":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        MakeSnakePukeFire();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake puke fire."));
                break;

            case "makeSnakePuke":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        MakeSnakePuke();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake puke."));
                break;

            case "setSnakeOnFire":
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                TryEffect(request,
                    () => true,
                    () =>
                    {
                        SetSnakeOnFire();
                        return true;
                    },
                    () => Connector.SendMessage($"{request.DisplayViewer} set Snake on fire."));
                break;

            case "makeSnakeBunnyHop":
                if (IsInCutscene() || !IsBunnyHopAllowedOnCurrentMap())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var bunnyHopDuration = request.Duration;

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
                if (IsInCutscene())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var freezeDuration = request.Duration;

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

            case "makeSnakeCrouch":
                if (IsInCutscene() || !IsSleepAllowedOnCurrentMap())
                {
                    DelayEffect(request, StandardErrors.BadGameState, GameState.Cutscene);
                    return;
                }
                var crouchDuration = request.Duration;

                var crouchAct = RepeatAction(request,
                    () => true,
                    () => Connector.SendMessage($"{request.DisplayViewer} made Snake crouch for {crouchDuration.TotalSeconds} seconds."),
                    TimeSpan.Zero,
                    () => IsReady(request),
                    TimeSpan.FromMilliseconds(500),
                    () =>
                    {
                        MakeSnakeBoxCrouch();
                        return true;
                    },
                    TimeSpan.FromMilliseconds(500),
                    false);

                crouchAct.WhenCompleted.Then
                    (_ =>
                {
                    UndoSnakeBoxCrouch();
                    Connector.SendMessage("Snake is no longer crouching.");
                });
                break;

            #endregion

            #region Guard Stats

            case "setGuardStatsAlmostInvincible":
                var almostInvincibleDuration = request.Duration;

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
                var veryStrongDuration = request.Duration;

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
                var veryWeakDuration = request.Duration;

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
                var oneShotDuration = request.Duration;

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

            default:
                Respond(request, EffectStatus.FailPermanent, StandardErrors.UnknownEffect, request);
                break;

                #endregion
        }
    }
}