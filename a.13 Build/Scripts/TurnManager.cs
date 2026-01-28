using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TurnManager : MonoBehaviour
{


    public Player player;

    public EnviromentManager environment;

    public CameraFollow cameraFollow;


    public static int numberOfHoursPassed = 0;
    
    [FormerlySerializedAs("turnCounter")] public TMP_Text dayCounter;
    public TMP_Text hourCounter;
    

    public static int minTimeIncriment = 1;
    
    public static int dayNightCyclePeriod = 24;

    public static float cyclePhaseOffset = .5f;

    public static int numberOfHoursToPass = 0;

    public CombatManager combatMan;
    
    
    public void Start()
    {
        ProcessChangeOfTurn();
    }

    public void ProcessChangeOfTurn()
    {

        // Players actions


        int i = 0;
        int origionalLength = player.actionsToProcessAtEndOfTurn.Count;
        numberOfHoursToPass = 0;
        while (i < origionalLength)
        {
            GameAction toDo = player.actionsToProcessAtEndOfTurn[0];
            numberOfHoursToPass += toDo.actionPointHourCost;
            GameAction.CompleteGameAction(toDo);

            player.actionsToProcessAtEndOfTurn.Remove(toDo);
            i++;
        }


        // Set player flags

        player.playerHasAbilitiesThisTurn = false;
        player.playerHasUltimatedThisTurn = false;
        player.playerHasBasicAttackedThisTurn = false;

        player.AcoolDownRemaining--;
        if (player.AcoolDownRemaining <= 0)
        {
            player.inAbilityCoooldown = false;
        }

        player.UcoolDownRemaining--;
        if (player.UcoolDownRemaining <= 0)
        {
            player.inUltimateCooldown = false;
        }

        // Change player temp

        Tile.TileTemp temp = Tile.TileTemp.Normal;
        if (player.inRangeOfHowManyFires.Count > 0)
        {
            // Maybe move fire calculation into here 

            if (player.inRangeOfHowManyFires.Count == 1)
            {
                temp = Tile.TileTemp.Warm;
            } else if (player.inRangeOfHowManyFires.Count >= 2)
            {
                temp = Tile.TileTemp.Hot;
            }

            player.inRangeOfHowManyFires.Clear();
        }
        else
        {
            if (Tile.DoesTileExist(player.movement.playerPos + Vector3Int.down))
            {
                Tile playerStandingOn = Tile.tilesInWorld.Find(x => x.tilePos == player.movement.playerPos + Vector3Int.down);

              
                //gets cold at night
                if (environment.dayNightCyclePercentage > .25f && environment.dayNightCyclePercentage < .75f)
                {
                    if (playerStandingOn.type == Tile.TileType.Sand)
                    {
                        temp = Tile.TileTemp.Cold;
                    }
                    else if (playerStandingOn.type == Tile.TileType.Water)
                    {
                        temp = Tile.TileTemp.FreezingCold;
                    }

                }
                else
                {
                    temp = playerStandingOn.temp;
                }
            }
        }
        player.AlterTemp(temp);


        // Do player status stuff 

        if (player.statuss.Contains(FreeEntity.StatusType.OnFire))
        {
            player.TakeDamage(1, DamageType.Innate);
        }
        if (player.statuss.Contains(FreeEntity.StatusType.Poisoned))
        {
            player.TakeDamage((int)((float)player.maxHealth * .1f), DamageType.Innate);
        }

        if (!player.statuss.Contains(FreeEntity.StatusType.Nominal))
        {
            int h = 0;

            while (h < player.statusTimes.Count)
            {

                player.statusTimes[h]--;

                if (player.statusTimes[h] <= 0)
                {
                    player.statuss.RemoveAt(h);
                    player.statusTimes.RemoveAt(h);
                    if (player.statuss.Count == 0)
                    {
                        player.statuss.Add(FreeEntity.StatusType.Nominal);
                        player.statusTimes.Add(0);
                        break;
                    }
                    continue;
                }

                h++;
            }

        }


        // Work with player stat increases/decreases

        // Sleep 
        if (player.sleep> player.minmaxSleep.x)
        { 
            // Maybe only do at night
            player.ChangeSleep(-1);
        } else if (player.sleep== player.minmaxSleep.x)
        {
            // At minamum, decrease sanity
            player.ChangeSanity(-1);
            
        }

        // Hunger
        if (player.hunger< player.minMaxHunger.y)
        {
            player.ChangeHunger(1);
        } else if (player.hunger== player.minMaxHunger.y)
        {
            player.TakeDamage(1, DamageType.Innate);
        }
        
        // Sanity 

        if (UnityEngine.Random.value < player.sanityIncreaseChance)
        {
            // chance to increase sanity per turn
            player.ChangeSanity(1);
        } else if (player.sanity== player.minMaxSanity.x)
        {
            player.ChangeHunger(1);
            player.ChangeSleep(-1);
            player.TakeDamage(1, DamageType.Innate);
        }

      

        
        // Now do Ai actions
        
        // Gets all AI things
        FreeEntity[] entities = GameObject.FindObjectsOfType<FreeEntity>();

        foreach (FreeEntity E in entities)
        {
            // Also processes status
            E.ProcessTurn();
        }
        
        // Now process AI actions
        
        foreach (FreeEntity E in entities)
        {
            foreach (GameAction A in E.actionsToDoAtEndOfTurn)
            {
                if (E.currentStatus.Contains(FreeEntity.StatusType.Stunned) || E.currentStatus.Contains( FreeEntity.StatusType.Frozen))
                {
                    if (A.type != ActionType.Attack && A.type != ActionType.Movement)
                    {
                        GameAction.CompleteGameAction(A);
                    }
                }
                else
                {
                    if ( E.currentStatus.Contains( FreeEntity.StatusType.Trapped))
                    {
                        if (A.type != ActionType.Movement)
                        {
                            GameAction.CompleteGameAction(A);
                        }
                    }
                    else
                    {
                        GameAction.CompleteGameAction(A);  
                    }
                }
            }
        }
        
        // Now spawn Ai maybe
        
        // At night
        if ((numberOfHoursPassed) % dayNightCyclePeriod == 0)
        {
            combatMan.SpawnEnemies();
            Accomplishments.UpdateAccomplishment(Accomplishments.AccomplishmentName.DaySurvived, 1);

        }


        numberOfHoursPassed += Mathf.Max(1, numberOfHoursToPass);
        
        dayCounter.text = (Mathf.Floor((float)numberOfHoursPassed/(float)dayNightCyclePeriod)).ToString();

        int hour = ((numberOfHoursPassed) % dayNightCyclePeriod);

        // Night ui

        if (hour == 20)
        {
            hourCounter.text = "Night!";
        }
        else
        {
            if (hour < 6 || hour > 20)
            {
                if (hour > 20)
                {
                    hourCounter.text = ((24 - hour) + 6).ToString() + " Turns Till Day";
                }
                else if (hour < 6)
                {
                    hourCounter.text = (6 - hour).ToString() + " Turns Till Day";
                }

            }
            else
            {
                hourCounter.text = (20 - hour).ToString() + " Turns Till Night";
            }
        }



        environment.HandleDayNightCycle();
        player.movement.StartTurn();


        //cameraFollow.StartResetToTarget();
    }


}
