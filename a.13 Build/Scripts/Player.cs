using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class Player : MonoBehaviour
{

    // Basic stuff
    public PlayerMovement movement;
    public Inventory inventory;
    public UIManager UI;
    public CombatManager combat;
    public Animator Anim;





    // Player actions
    public List<GameAction> innateActions;
    public List<GameAction> actionsToProcessAtEndOfTurn;
    public List<GameAction> incidentalActions;
    
    //Actions happeneing to player
    public List<GameAction> actionsHappeningToPlayer;
    public List<GameAction> actionsThatThePlayerCanDoBecauseExternalStuff;

  

    // Info bars stored here
    
    // Deals with mind strength, happiness, etc
    public int sanity;
    public int defaultSanity = 100;
    public Vector2Int minMaxSanity = new Vector2Int(0, 100);
    public float sanityIncreaseChance = .5f;
    
    //TODO impliment insultation, heat loss/gain
    //Physical temp of player
    public int temperature;
    public int defaultTemp = 5;
    public Vector2Int minMaxTemp = new Vector2Int(0, 10);
    // Normal Temp will be 3 to 8
    // bar will turn dark blue when reaches 2 
    // Bar will turn light blue when reaches 1
    // baby blue when 0, frozen (player frozen will have movement range decreased by half and only *2 damage taken mult)
    // orange when 9 
    // Red when 10, on fire 
    // Blocks will have a couple different temp states: 
    // Freezing cold (-2 at turn start from normal player temp, -4 from player 10 temp, -3 from 9 temp )
    // Cold (-1 from normal player temp) 
    // Normal (0)
    // Warm (+1 from normal)
    // Hot (+2 from normal, +4 when player freezing )

    public List<int> inRangeOfHowManyFires = new List<int>();
    
    //Hours of sleep had 
    public int sleep;
    public int defaultSleep = 8;
    public Vector2Int minmaxSleep = new Vector2Int(0, 8);

    // Hunger
    public int hunger;
    public int defaultHunger = 0;
    public Vector2Int minMaxHunger = new Vector2Int(0, 10);

    // Combat 
    public InventoryItem fist;
    public InventoryItem selectedWeapon;
    public InventoryItem selectedAbilty;
    public InventoryItem ultimate;

    public int playerDamage;
    public float playerRange;

    // cooldowns in hours 
    public int abilityDamage;
    public float abilityRange;
    public int abilitycooldown;

    public int ultimateDamage;
    public float ultimateRange;
    public int ulimateCooldown;

    public bool inAbilityCoooldown = false;
    public int AcoolDownRemaining;
    
    public bool inUltimateCooldown = false;
    public int UcoolDownRemaining;

    public bool playerHasBasicAttackedThisTurn;
    public bool playerHasAbilitiesThisTurn;
    public bool playerHasUltimatedThisTurn;
    
    // health
    public int health;
    public int maxHealth;

    // Equipment and stats
    public List<InventoryItem> equipmentWorn = new List<InventoryItem>();
    public int currentArmour;
    public int magicResistance;

    public List<FreeEntity.StatusType> statuss = new List <FreeEntity.StatusType> { FreeEntity.StatusType.Nominal };
    public List<int> statusTimes = new List<int> { 0 };

    // particles
    public ParticleSystem firePunch;


    // Unlockables and transformations
    public Accomplishments playerAccomplishments;

    public Transformation currentTransfomation;
    public Transformation[] possibleTransformations;


    // Start is called before the first frame update
    void Start()
    {
        // Set the player as player
        GameAction.SetUp();

        // set starting stats
        sleep = defaultSleep;
        sanity = defaultSanity;
        temperature = defaultTemp;
        health = maxHealth;


        // Set acheviements
        playerAccomplishments = new Accomplishments();
        playerAccomplishments.acheivements = Resources.LoadAll<Acheivement>("Acheivements");

        // If null, human
        currentTransfomation = null;

        possibleTransformations = Resources.LoadAll<Transformation>("Transformations");
    }

    // Update is called once per frame
    void Update()
    {
        
        // try to do the innate actions available - like moving, chopping, mining
        foreach (GameAction GA in innateActions)
        {
            GameAction.PossiblyAttemptGameAction(GA);
        }

        bool blockingActionFound = false;

        // Find any blocking actions -- take only first in list to display UI(thats why its called a blocking action)
        foreach (GameAction GA in incidentalActions)
        {
            if (GA.isBlockingAction)
            {
                blockingActionFound = true;
                GameAction.prefomringBlockingAction = GA;
                break;
            }
        }

        // If not blocking action found, do actions that the player can do because of like holding an item or such and set as blocking action
        if (!blockingActionFound)
        {
            
            foreach (GameAction GA in actionsThatThePlayerCanDoBecauseExternalStuff)
            {
                if (GA.isBlockingAction)
                {
                    blockingActionFound = true;
                    GameAction.prefomringBlockingAction = GA;
                    break;
                }
            }

            if (!blockingActionFound)
            {
                GameAction.prefomringBlockingAction = null;
            }
        }


        // Do incidental actions
        int g = 0;
        while (g< incidentalActions.Count)
        {
            GameAction.PossiblyAttemptGameAction(incidentalActions[g]);
            
            UI.UpdateHotBar();
            g++;
        }
        
        // Do the external actions 
        foreach (GameAction GA in actionsThatThePlayerCanDoBecauseExternalStuff)
        {
            GameAction.PossiblyAttemptGameAction(GA);
        }
        
    }

    public int TakeDamage(int amount, DamageType type)
    {


        Debug.Log("Player took damage of type: " + type.ToString());
        int ret = 0;
        if (type != DamageType.Innate)
        {


            if (statuss.Contains(FreeEntity.StatusType.Frozen))
            {
                amount *= 2;
            }

            if (type == DamageType.Magic)
            {
                // Rounding to nearest int of whatever percent the resistance is, and subtracting it.  If you have a magic resitance of 1, thats 10%, so subtract 10% of origional value, leaving 90%, rounded up.
                // If damage is 2, subtract 1
                // If damage is 10, subtract 1

                amount -= Mathf.CeilToInt(((float)amount) * (((float)magicResistance) / 10f));
            }



            int halfAmount = amount / 2;

            if (halfAmount > currentArmour)
            {
                halfAmount = currentArmour;
            }

            int armourbroken = 0;
            int i = 0;

            while (i < halfAmount)
            {
                if (Random.value > .5f)
                {
                    armourbroken++;
                }
                i++;
            }

            int currentEquipment = 0;
            while (armourbroken > 0)
            {
                InventoryItem equipmentWithArmour = equipmentWorn[currentEquipment];
                if (equipmentWithArmour.extraDataInt <= 0)
                {
                    currentEquipment++;
                }
                else
                {
                    equipmentWithArmour.extraDataInt--;
                    currentArmour--;
                    armourbroken--;
                }
            }
            // Figure out armour deductions later 
            int remainingAmount = amount - halfAmount;
            health -= remainingAmount;
            ret = remainingAmount;
            Accomplishments.UpdateAccomplishment(Accomplishments.AccomplishmentName.DamageTaken, remainingAmount);

            if (health <= 0)
            {
                Die();
            }
            return ret;
        }
        else
        {
            health -= amount;
            
        if (health <= 0)
        {
            Die(); 
        }
            return ret;
        }
       
   
    }


    public void ChangeStatus(FreeEntity.StatusType newStatus)
    {

        if (newStatus == FreeEntity.StatusType.Nominal)
        {
            statuss = new List<FreeEntity.StatusType> { FreeEntity.StatusType.Nominal };
            return;
        }

        // If contains nominal, then is all fine. Clear list and add status
        if (statuss.Contains(FreeEntity.StatusType.Nominal))
        {
            statuss.Clear();
            statuss = new List<FreeEntity.StatusType> { newStatus };
            statusTimes[0] = FreeEntity.statusStartTimes[(int)newStatus];
        }
        else
        { // Then entity already has a status effect

            // Already present
            if (statuss.Contains(newStatus))
            {
                int j = statuss.IndexOf(newStatus);
                statusTimes[j] += FreeEntity.statusStartTimes[(int)newStatus];
            }
            else
            {
                statuss.Add(newStatus);
                int j = statuss.Count;
                statusTimes.Add(FreeEntity.statusStartTimes[(int)newStatus]);

            }
        }
    }

    public void Die()
    {
        Debug.LogWarning("Player died!!!");
        Anim.SetBool("IsDead", true);
        Accomplishments.UpdateAccomplishment(Accomplishments.AccomplishmentName.TimesDied, 1);

    }


    public void AlterTemp(Tile.TileTemp standingTemp)
    {

      
        int tempToAdd = 0;

        //Player temp ranges
        if (temperature>=3 && temperature <= 7)
        {
            if (standingTemp== Tile.TileTemp.FreezingCold)
            {
                tempToAdd = -2;
            } else if (standingTemp== Tile.TileTemp.Cold)
            {
                tempToAdd = -1;

            }
            else if (standingTemp == Tile.TileTemp.Normal)
            {
                // Slowly normalize

                if (temperature<5&& Random.value < .5f)
                {
                    tempToAdd = 1;
                }
                else if (temperature > 5 && Random.value < .5f)
                {
                    tempToAdd = -1;
                }
                else
                {
                    tempToAdd = 0;
                }
                
                   
                
               
            }
            else if (standingTemp== Tile.TileTemp.Warm)
            {
                tempToAdd = 1;
            }
            else if (standingTemp == Tile.TileTemp.Hot)
            {
                tempToAdd = 2;
            }
        } else if (temperature == 2)
        {
            if (standingTemp == Tile.TileTemp.FreezingCold)
            {
                tempToAdd = -2;
            }
            else if (standingTemp == Tile.TileTemp.Cold)
            {
                tempToAdd = -1;
            }
            else if (standingTemp == Tile.TileTemp.Normal)
            {
                tempToAdd = 1;
            }
            else if (standingTemp == Tile.TileTemp.Warm)
            {
                tempToAdd = 2;
            }
            else if (standingTemp == Tile.TileTemp.Hot)
            {
                tempToAdd = 3;
            }
        }
        else if (temperature == 1)
        {
            if (standingTemp == Tile.TileTemp.FreezingCold)
            {
                tempToAdd = -1;
            }
            else if (standingTemp == Tile.TileTemp.Cold)
            {
                tempToAdd = -1;
            }
            else if (standingTemp == Tile.TileTemp.Normal)
            {
                tempToAdd = 1;
            }
            else if (standingTemp == Tile.TileTemp.Warm)
            {
                tempToAdd = 2;
            }
            else if (standingTemp == Tile.TileTemp.Hot)
            {
                tempToAdd = 3;
            }
        }
        else if (temperature == 0)
        {
            if (standingTemp == Tile.TileTemp.FreezingCold)
            {
                tempToAdd = 0;
            }
            else if (standingTemp == Tile.TileTemp.Cold)
            {
                tempToAdd = 0;
            }
            else if (standingTemp == Tile.TileTemp.Normal)
            {
                tempToAdd = 1;
            }
            else if (standingTemp == Tile.TileTemp.Warm)
            {
                tempToAdd = 3;
            }
            else if (standingTemp == Tile.TileTemp.Hot)
            {
                tempToAdd = 4;
            }
        }
        else if (temperature == 8)
        {
            if (standingTemp == Tile.TileTemp.FreezingCold)
            {
                tempToAdd = -3;
            }
            else if (standingTemp == Tile.TileTemp.Cold)
            {
                tempToAdd = -2;
            }
            else if (standingTemp == Tile.TileTemp.Normal)
            {
                tempToAdd = -1;
            }
            else if (standingTemp == Tile.TileTemp.Warm)
            {
                tempToAdd = 1;
            }
            else if (standingTemp == Tile.TileTemp.Hot)
            {
                tempToAdd = 1;
            }
        }
        else if (temperature == 9)
        {
            if (standingTemp == Tile.TileTemp.FreezingCold)
            {
                tempToAdd = -3;
            }
            else if (standingTemp == Tile.TileTemp.Cold)
            {
                tempToAdd = -2;
            }
            else if (standingTemp == Tile.TileTemp.Normal)
            {
                tempToAdd = -1;
            }
            else if (standingTemp == Tile.TileTemp.Warm)
            {
                tempToAdd = 1;
            }
            else if (standingTemp == Tile.TileTemp.Hot)
            {
                tempToAdd = 1;
            }
        }
        else if (temperature == 10)
        {
            if (standingTemp == Tile.TileTemp.FreezingCold)
            {
                tempToAdd = -4;
            }
            else if (standingTemp == Tile.TileTemp.Cold)
            {
                tempToAdd = -3;
            }
            else if (standingTemp == Tile.TileTemp.Normal)
            {
                tempToAdd = -1;
            }
            else if (standingTemp == Tile.TileTemp.Warm)
            {
                tempToAdd = 0;
            }
            else if (standingTemp == Tile.TileTemp.Hot)
            {
                tempToAdd = 0;
            }
        }

    
        temperature += tempToAdd;
        temperature = Math.Clamp(temperature, minMaxTemp.x, minMaxTemp.y);

        if (temperature == 0)
        {
            TakeDamage(1, DamageType.Innate);
            ChangeStatus(FreeEntity.StatusType.Frozen);
        } else if (temperature == 10)
        {
            ChangeStatus(FreeEntity.StatusType.OnFire);

        }
       
    }

    public void ChangeSanity(int dS)
    {
         sanity += dS;
        sanity = Math.Clamp(sanity, minMaxSanity.x, minMaxSanity.y);
    }

    public void ChangeSleep(int dH)
    {
        sleep += dH;
        sleep = Math.Clamp(sleep, minmaxSleep.x, minmaxSleep.y);
        
    }

    public void ChangeHunger(int dH)
    {
        hunger += dH;
        hunger = Math.Clamp(hunger, minMaxHunger.x, minMaxHunger.y);

    }

}

public enum DamageType
{
    Normal,
    Fire,
    Ice,
    Electrical,
    Nature,
    Magic, 
    DarkMagic,
    Innate // Avoids armour 
    
}


public class Accomplishments
{

    public static List<string> internalNames = new List<string>() { "BlocksMoved", "OrcsSlain", "OrcsSlainAsSkele", "CerberusSlain", "CerberusSlainAsOrc", "WitchesSlain", "SagesSlain", "SagesSlainAsWitch", "SkeletonsSlain",
        "BlocksMined", "TreesChopped", "DamageTaken", "HoursSlept", "DaySurvived", "ItemsCrafted", "DaysAsTransformation", "TimesTransformed", "TimesOpenedGame", "TimesDied" };

    public static List<int> timesHappened = new List<int>( new int[internalNames.Count]);

    public static void UpdateAccomplishment(AccomplishmentName N, int count)
    {
        int numberList = (int)N;
        timesHappened[numberList]+=count;

      
    }

    public void UnlockAcheivements()
    {
        foreach (Acheivement A in acheivements)
        {
            // Unlock achivements here
        }


    }

    public Acheivement[] acheivements;


    public enum AccomplishmentName
    {
       BlocksMoved,
       OrcsSlain,
       OrcsSlainAsSkele,
       CerberusSlain,
       CerberusSlainAsOrc,
       WitchesSlain,
       SagesSlain,
       SagesSlainAsWitch,
       SkeletonsSlain,
       BlocksMined,
       TreesChopped,
       DamageTaken,
       HoursSlept,
       DaySurvived,
       ItemsCrafted,
       DaysAsTransformation,
       TimesTransformed,
       TimesOpenedGame,
       TimesDied




    }
}