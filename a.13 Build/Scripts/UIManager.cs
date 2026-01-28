using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{


    public GameObject InventoryPanel;

    public GameObject InvenBoxProto;

    public Transform startPosAndParent;

    public float invenWidth;

    public float invenHeight;

    public int invenSpaceCountX;
    public int invenSpaceCountY;

    public Player player;

    public GameObject[] inventorySlots;

    public GameObject selector;
    
    public GameObject craftingPanel;

    public Color canCraft;
    public Color cannotCraft;

    public Color canCraftB;
    public Color cannotCraftB;
    
    public GameObject craftRPProto;

    public GameObject craftPanelInside;
    public float verticalDist;
    public Transform protoCraftParent;

    public CombatManager combatManage;

    public Button combatButton;

    public GameObject localInventoryInteractScreen;

    public Transform localInventoryItemsStartPos;

    public Transform localInventoryItemsParent;

    public Vector2 localInventoryItemPadding;

    public GameObject localInventoryPrototype;

    public int localInventoryColsNum;

    public BlockingActionHolder[] blockingActionUI;

 
    public Slider sleepS;
    public Slider healthS;
    public Slider sanityS;
    public Slider tempS;
    public Slider hungerS;


    public GameObject playerCombatInfoPanel;

    public Button weaponSelectButtonToChangeSprite;
    public TMP_Text weaponDamagetext;
    public TMP_Text weaponnametext;
    public TMP_Text weaponRangeText;
    public TMP_Text weapondDescriptionText;

    public GameObject nextTurnButton;
    
    public GameObject weapondSelectProto;
    public GameObject abilitySelectproto;
    public Transform weapondSelectParent;
    public GameObject weaponSelectScroll;
    public float weaponSelectSpacing = 150;

    public GameObject combatOptions;
    public bool didShowCombatUI = false;

    public GameObject selectedEnemy;


    public TMP_Text abilityDamagetext;
    public TMP_Text abilityRangetext;
    public TMP_Text abilityDescrip;
    public TMP_Text abilitynameText;
    public TMP_Text abilityCooldowntxt;
    
    public TMP_Text ultimateDamagetext;
    public TMP_Text ultimateRangetext;
    public TMP_Text ultimateDescrip;
    public TMP_Text ultimatenameText;
    public TMP_Text ultimatecooldownTxt;

    public GameObject equipmentInterface;
    public GameObject abilityInterface;

    public GameObject selectEquipmentScroll;
    public GameObject equipmentSelectProto;
    public GameObject equipmentSelectparent;
    public float equipmentSelectSpacing;

    public TMP_Text helmetName;
    public TMP_Text helmetArmurVal;
    public TMP_Text chestplateName;
    public TMP_Text chestPlateVal;
    public TMP_Text helmetDescrip;
    public TMP_Text chestDescript;
    public TMP_Text MRTxt;

    public GameObject scrapbookProto;
    public GameObject scrapbookParent;
    public float scrapbookSpacing;
    public GameObject scrapbookMoreInfo;

    public GameObject scrapbookPanel;
    public GameObject inventoryPanel;

    public bool doingAdvancedCrafting;

    public Color redBar;
    public Color orangeBar;
    public Color greenbar;
    public Color blueBar;

    public Image tempBackground;
    public Image healthBackground;
    public Image sleepBackground;
    public Image sanityBackground;
    public Image hungerbackground;

    public GameObject fireParticleSystem;
    public GameObject iceParticleSystem;
    public GameObject poisonParticleSystem;
    public GameObject trappedPS;
    public GameObject stunnedPS;

    public GameObject[] statusParticles;

    public TMP_Text armourRating;
    public TMP_Text playerDamage;
    public TMP_Text playerDamageType;

    public TMP_Text playerMoveRange;
    public TMP_Text playerAttackRange;
    public TMP_Text playerStatuses;
    public TMP_Text playerPos;

    public GameObject transformationPanel;
    public TMP_Text[] transformationStrings;
    public GameObject transformationButton;
   


    // Start is called before the first frame update
    void Start()
    {
        SelectHotBarItem(0);
        SetWeaponInfo();
        statusParticles = new GameObject[] { null, fireParticleSystem, iceParticleSystem, stunnedPS, poisonParticleSystem, null, trappedPS };

    }
    

    // Update is called once per frame
    void Update()
    {

        // Ui info

        UpdateInfoUI();

        SetCombatButton();

        // Blocking action UI 

        // Blocking actions have a UI associated with them
        if (GameAction.prefomringBlockingAction != null)
        {

            if (Input.GetKeyDown(KeyCode.Escape))
            {
              
                // Exit blocking UI
                player.actionsThatThePlayerCanDoBecauseExternalStuff.Remove(player.actionsThatThePlayerCanDoBecauseExternalStuff.Find(x => x.ACTIONID == GameAction.prefomringBlockingAction.ACTIONID));
                player.incidentalActions.Remove(player.incidentalActions.Find(x => x.ACTIONID == GameAction.prefomringBlockingAction.ACTIONID));
                GameAction.prefomringBlockingAction = null;
           
            }
            else
            {
                 int i = 0;

                while (i < blockingActionUI.Length)
                {
                    bool isThisAction = GameAction.prefomringBlockingAction.ACTIONID == blockingActionUI[i].action.ACTIONID;
                    blockingActionUI[i].associatedUI.SetActive(isThisAction);

                    if (blockingActionUI[i].associatedUI.transform.gameObject.name=="Sleeping" && player.currentTransfomation!=null)
                    {
                        blockingActionUI[i].associatedUI.transform.Find("ExtraSleepButton").gameObject.SetActive(true);
                    }
                    i++;
                }
            }
           
        }
        else
        {
            int i = 0;
            while (i < blockingActionUI.Length)
            {
                blockingActionUI[i].associatedUI.SetActive(false);

                if (blockingActionUI[i].associatedUI.transform.gameObject.name == "Sleeping")
                {
                    blockingActionUI[i].associatedUI.transform.Find("ExtraSleepButton").gameObject.SetActive(false);
                }
                i++;
            }
        }

        didShowCombatUI = false;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (InventoryPanel.activeSelf)
            {
                CloseBackpack();
            }
            else
            {
                OpenBackpack();
            }
        }
    }

    public void OpenTransformationPanel()
    {

        transformationPanel.SetActive(true);


        // Go through and unlock each transformation string

        int i = 0;
        foreach (Transformation T in player.possibleTransformations)
        {
            transformationStrings[i].text = T.unlocked ? T.Name: "?";

            i++;
        }
    }

    public void CloseTransformationPanel()
    {
        transformationPanel.SetActive(false);

    }

    public void TRANSFORM(int which)
    {

        // transforms the human player into a transformation

        Transformation totrans = player.possibleTransformations[which];

        if (!totrans.unlocked || player.currentTransfomation!=null)
        {
            return;
        }

        player.currentTransfomation = totrans;

        player.ultimate = player.currentTransfomation.ultimateAbility;

        foreach(InventoryItem A in player.currentTransfomation.abilities)
        {
            player.inventory.abilities.Add(A);

        }

        RemoveAllEquipment();


        // And now change the model
        player.transform.Find("PlayerBody").Find("Transformations").GetChild(0).gameObject.SetActive(false);
        player.transform.Find("PlayerBody").Find("Transformations").GetChild(which + 1).gameObject.SetActive(true);
        CloseTransformationPanel();
    }

    public void DoLongSleep()
    {
        player.actionsThatThePlayerCanDoBecauseExternalStuff.Find(x => x.ACTIONID == 5).extraDataV = Vector3.one;
    }

    public void WindowedMode()
    {
        Screen.fullScreen = !Screen.fullScreen;

    }


    public void SelectChestplateToUse()
    {
        
        if (player.currentTransfomation != null)
        {
            return;

        }

        selectEquipmentScroll.SetActive(true);


        List<InventoryItem> onlyChests = player.inventory.items.FindAll(x => x.type == InventoryItemType.EquipableArmour && x.tags.Contains(InventoryTag.Chestplate));
        List<InventoryItem> onlyChestssUnqiue = onlyChests.Distinct(new InventoryItemComparer()).ToList();


        if (onlyChestssUnqiue.Count == 0)
        {
            selectEquipmentScroll.SetActive(false);
        }
        
        int i = 0;

        while (i < onlyChestssUnqiue.Count)
        {
            GameObject protoClone = Instantiate(equipmentSelectProto, equipmentSelectparent.transform.position + Vector3.down * equipmentSelectSpacing*i, Quaternion.identity, equipmentSelectparent.transform);
            protoClone.transform.Find("Name").gameObject.GetComponent<TMP_Text>().text = onlyChestssUnqiue[i].Name;
            protoClone.name = i.ToString();
            
            i++;
        }

        
    }
    
    public void SelectThisEquipmentToUse(GameObject me)
    {
        string nameOfWeapon = me.transform.Find("Name").GetComponent<TMP_Text>().text;

        List<InventoryItem> onlyHelmets =player.inventory.items.FindAll(x => x.type == InventoryItemType.EquipableArmour && x.tags.Contains(InventoryTag.Helmet));

        List<InventoryItem> onlyHelemntsUnqiue = onlyHelmets.Distinct(new InventoryItemComparer()).ToList();
        InventoryItem chosen = onlyHelemntsUnqiue.Find(x => x.Name == nameOfWeapon);

        if (chosen == null)
        {
            // is a chestplate instead
            List<InventoryItem> onlyChests =player.inventory.items.FindAll(x => x.type == InventoryItemType.EquipableArmour && x.tags.Contains(InventoryTag.Chestplate));
            List<InventoryItem> onlyChestssUnqiue = onlyChests.Distinct(new InventoryItemComparer()).ToList();
            chosen = onlyChestssUnqiue.Find(x => x.Name == nameOfWeapon);
             
             // Remove current chest
             InventoryItem ChestWorn = player.equipmentWorn.Find(x => x.tags.Contains(InventoryTag.Chestplate));
        
             if (ChestWorn != null)
             {
                 player.equipmentWorn.Remove(ChestWorn);
            
                 // prevent infinite armour glitch
                 player.currentArmour -= ChestWorn.extraDataInt;
                 player.magicResistance -= GetmagicRes(ChestWorn.extraData);
             } 
        }
        else
        {
            // Remove current helmet
            InventoryItem helmetWorn = player.equipmentWorn.Find(x => x.tags.Contains(InventoryTag.Helmet));
        
            if (helmetWorn != null)
            {
                player.equipmentWorn.Remove(helmetWorn);
            
                // prevent infinite armour glitch
                player.currentArmour -= helmetWorn.extraDataInt;
                player.magicResistance -= GetmagicRes(helmetWorn.extraData);
            } 

        }
        
        if (player.currentTransfomation == null)
        {

            // Add new helmet and add its armour val
            player.equipmentWorn.Add(chosen);
            player.currentArmour += chosen.extraDataInt;

            // Calc magic resistance 

            player.magicResistance += GetmagicRes(chosen.extraData);

        }
            
   


        foreach (Transform T in equipmentSelectparent.transform)
        {
            Destroy(T.gameObject);
        }
        
        selectEquipmentScroll.SetActive(false);

        PopulateEquipmentUI();
        ShowHideCombatInfoMenu();
        ShowHideCombatInfoMenu();

    }

    public void RemoveAllEquipment()
    {

        foreach(InventoryItem EQ in player.equipmentWorn)
        {
            player.currentArmour -= EQ.extraDataInt;
            player.magicResistance -= GetmagicRes(EQ.extraData);
        }
        player.equipmentWorn.Clear();

    }



    int GetmagicRes(string s)
    {
       

        int Dindex = s.IndexOf("MR");

        if (Dindex != -1)
        {
            string justRes = s.Substring(Dindex + 4, 1);
           return int.Parse(justRes);
        }

        return 0;
    }

    public void SelectHelmetToUse()
    {


        if (player.currentTransfomation != null)
        {
            return;

        }
        selectEquipmentScroll.SetActive(true);


        List<InventoryItem> onlyHelmets = player.inventory.items.FindAll(x => x.type == InventoryItemType.EquipableArmour && x.tags.Contains(InventoryTag.Helmet));

        if (onlyHelmets.Count == 0)
        {
            selectEquipmentScroll.SetActive(false);
        }

       List<InventoryItem> onlyHelemntsUnqiue =  onlyHelmets.Distinct(new InventoryItemComparer()).ToList();
        
        int i = 0;
        while (i < onlyHelemntsUnqiue.Count)
        {
            GameObject protoClone = Instantiate(equipmentSelectProto, equipmentSelectparent.transform.position + Vector3.down * equipmentSelectSpacing*i, Quaternion.identity, equipmentSelectparent.transform);
            protoClone.transform.Find("Name").gameObject.GetComponent<TMP_Text>().text = onlyHelemntsUnqiue[i].Name;
            protoClone.name = i.ToString();
            
            i++;
        }

        
    }

    public void PopulateEquipmentUI()
    {
        InventoryItem helmetWorn = player.equipmentWorn.Find(x => x.tags.Contains(InventoryTag.Helmet));

        if (helmetWorn != null)
        {
            helmetName.text = helmetWorn.Name;
            helmetArmurVal.text = helmetWorn.extraDataInt.ToString();
            helmetDescrip.text = helmetWorn.Description;
        }
        else
        {
            helmetName.text = "--";
            helmetArmurVal.text = "--";
            helmetDescrip.text = "--";
        }
        
        InventoryItem chestplateWorn = player.equipmentWorn.Find(x => x.tags.Contains(InventoryTag.Chestplate));
        
        if (chestplateWorn != null)
        {
            chestplateName.text = chestplateWorn.Name;
            chestPlateVal.text = chestplateWorn.extraDataInt.ToString();
            chestDescript.text = chestplateWorn.Description;
        }
        else
        {
            chestplateName.text = "--";
            chestPlateVal.text = "--";
            chestDescript.text = "--";
        }
    }
    
    public void SwitchMenuToAbilityInterface()
    {
        abilityInterface.SetActive(true);
        equipmentInterface.SetActive(false);
        ShowHideCombatInfoMenu();
        ShowHideCombatInfoMenu();

    }
    
    public void SwitchMenuToEquipmentInterface()
    {
        abilityInterface.SetActive(false);
        equipmentInterface.SetActive(true);
        PopulateEquipmentUI();
    }

    public void SelectWeaponToUse()
    {
        weaponSelectScroll.SetActive(true);


        List<InventoryItem> onlyWeapons = player.inventory.items.FindAll(x => x.type == InventoryItemType.Weapon);
        onlyWeapons.Add(player.fist);
        List<InventoryItem> onlyWepsUnqiue = onlyWeapons.Distinct(new InventoryItemComparer()).ToList();
        int i = 0;

        while (i < onlyWepsUnqiue.Count)
        {
            GameObject protoClone = Instantiate(weapondSelectProto, weapondSelectParent.transform.position + Vector3.down * weaponSelectSpacing*i, Quaternion.identity, weapondSelectParent.transform);
            protoClone.transform.Find("Name").gameObject.GetComponent<TMP_Text>().text = onlyWepsUnqiue[i].Name;
            protoClone.name = i.ToString();
            
            i++;
        }

       
    }

    public void SelectThisWeapon(GameObject me)
    {
        string nameOfWeapon = me.transform.Find("Name").GetComponent<TMP_Text>().text;

        List<InventoryItem> onlyWeapons = player.inventory.items.FindAll(x => x.type == InventoryItemType.Weapon);
        List<InventoryItem> onlyWepsUnqiue = onlyWeapons.Distinct(new InventoryItemComparer()).ToList();
        InventoryItem chosen = onlyWepsUnqiue.Find(x => x.Name == nameOfWeapon);

        player.selectedWeapon = chosen;
        SetWeaponInfo();

        foreach (Transform T in weapondSelectParent)
        {
            Destroy(T.gameObject);
        }
        
        weaponSelectScroll.SetActive(false);

        ShowHideCombatInfoMenu();
        ShowHideCombatInfoMenu();

    }

    public void SelectAbiltyToUse()
    {
        weaponSelectScroll.SetActive(true);


        List<InventoryItem> onlyWeapons = player.inventory.abilities.FindAll(x => x.type == InventoryItemType.Ability);
        List<InventoryItem> onlyWepsUnqiue = onlyWeapons.Distinct(new InventoryItemComparer()).ToList();


        if (onlyWepsUnqiue.Count == 0)
        {
            weaponSelectScroll.SetActive(false);
        }
        
        int i = 0;

        while (i < onlyWepsUnqiue.Count)
        {
            GameObject protoClone = Instantiate(abilitySelectproto, weapondSelectParent.transform.position + Vector3.down * weaponSelectSpacing*i, Quaternion.identity, weapondSelectParent.transform);
            protoClone.transform.Find("Name").gameObject.GetComponent<TMP_Text>().text = onlyWepsUnqiue[i].Name;
            protoClone.name = i.ToString();
            
            i++;
        }
        
    }

    public void SelectThisAbility(GameObject me )
    {
        
        string nameOfWeapon = me.transform.Find("Name").GetComponent<TMP_Text>().text;

        List<InventoryItem> onlyWeapons = player.inventory.abilities.FindAll(x => x.type == InventoryItemType.Ability);
        List<InventoryItem> onlyWepsUnqiue = onlyWeapons.Distinct(new InventoryItemComparer()).ToList();
        InventoryItem chosen = onlyWepsUnqiue.Find(x => x.Name == nameOfWeapon);

        player.selectedAbilty = chosen;
        SetWeaponInfo();

        foreach (Transform T in weapondSelectParent)
        {
            Destroy(T.gameObject);
        }
        
        weaponSelectScroll.SetActive(false);

        ShowHideCombatInfoMenu();
        ShowHideCombatInfoMenu();
    }

    public void SelectUltimateAttack()
    {
        // Allows player to do a ultimate attack omn the selected enemy
        if (selectedEnemy!= null)
        {
           combatManage.PlayerUltimateAttack(selectedEnemy);
            
            
            combatOptions.SetActive(false);
            selectedEnemy.transform.GetComponentInChildren<cakeslice.Outline>().enabled = false;
            selectedEnemy = null;
        }
        
    }
    
    public void SelectAbiltyToAttack()
    {
        // Allows player to do an abilty attack omn the selected enemy
        if (selectedEnemy!= null)
        {
            combatManage.PlayerAbiltyAttack(selectedEnemy);
            combatOptions.SetActive(false);
            selectedEnemy.transform.GetComponentInChildren<cakeslice.Outline>().enabled = false;
            selectedEnemy = null;
        }
    }
    public void SelectBasicAttack()
    {
        // Allows player to do a basic attack omn the selected enemy
        if (selectedEnemy!= null)
        {
            combatManage.PlayerBasicAttack(selectedEnemy);
            combatOptions.SetActive(false);
            selectedEnemy.transform.GetComponentInChildren<cakeslice.Outline>().enabled = false;
            selectedEnemy = null;
        }
    }

    public void ShowCombatActions(Vector3 worldPos, GameObject enem)
    {
       // Opens the panel for combat actions when clicking on an enemy
        //combatOptions.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(worldPos) ;


        float dist = Vector3.Distance(player.transform.position, worldPos);
        if (player.selectedWeapon.tags.Contains(InventoryTag.RangedWeapon))
        {
            combatOptions.transform.Find("Basic Attack").gameObject.GetComponent<Button>().interactable =  dist< player.playerRange && dist> combatManage.melleRange;
        }
        else
        {
            combatOptions.transform.Find("Basic Attack").gameObject.GetComponent<Button>().interactable = dist < player.playerRange;
        }
        

        combatOptions.transform.Find("Ability").gameObject.GetComponent<Button>().interactable = (player.selectedAbilty != null && !player.inAbilityCoooldown && Vector3.Distance(player.transform.position, worldPos) < player.abilityRange);
        
        combatOptions.transform.Find("Ultimate").gameObject.GetComponent<Button>().interactable = Vector3.Distance(player.transform.position, worldPos) < player.ultimateRange && !player.inUltimateCooldown;
        combatOptions.transform.Find("EnemyName").gameObject.GetComponent<TMP_Text>().text = enem.GetComponent<FreeEntity>().GetType().ToString();
        combatOptions.transform.Find("Health").gameObject.GetComponent<TMP_Text>().text = enem.GetComponent<FreeEntity>().health.ToString();

        combatOptions.SetActive(true);
        didShowCombatUI = true;

        if (selectedEnemy != null)
        {
            selectedEnemy.transform.GetComponentInChildren<cakeslice.Outline>().enabled = false;
        }

        selectedEnemy = enem;
    
        selectedEnemy.transform.GetComponentInChildren<cakeslice.Outline>().enabled = true;

    }
    
    
    public void SetWeaponInfo()
    {

        // Sets the weapon info for combat menu and background
        
        // Will always have fists
        
        if (player.selectedWeapon == null)
        {
            player.selectedWeapon = player.fist;
        }

        string extraInfo = player.selectedWeapon.extraData;
        int startingIndexOfDamage = player.selectedWeapon.extraData.IndexOf("DAMAGE:");

        string stringStartingWithDamageNum = extraInfo.Substring(startingIndexOfDamage+7, extraInfo.Length - startingIndexOfDamage-7);

        int nextIndexOfSemiColon = stringStartingWithDamageNum.IndexOf(";");
        
        string onlyDamageNum = stringStartingWithDamageNum.Substring(0,nextIndexOfSemiColon );
        
        
        string withoutDamageNum = stringStartingWithDamageNum.Substring(nextIndexOfSemiColon+1,stringStartingWithDamageNum.Length - nextIndexOfSemiColon-1 );

        int startOfRange = withoutDamageNum.IndexOf("RANGE:");
        
        string rangeNum = withoutDamageNum.Substring(startOfRange + 6,withoutDamageNum.Length -startOfRange -6-1  );

        player.playerDamage = int.Parse(onlyDamageNum);
        player.playerRange = float.Parse(rangeNum);

        armourRating.text = player.currentArmour.ToString();
        playerDamage.text = onlyDamageNum;
        DamageType T = DamageType.Normal;
        InventoryItem WP = player.selectedWeapon;

        if (WP.tags.Contains(InventoryTag.FireDamage))
        {
            T = DamageType.Fire;
        }
        else if (WP.tags.Contains(InventoryTag.IceDamage))
        {
            T = DamageType.Ice;
        }
        else if (WP.tags.Contains(InventoryTag.NatureDamage))
        {
            T = DamageType.Nature;
        }
        else if (WP.tags.Contains(InventoryTag.ElectricalDamage))
        {
            T = DamageType.Electrical;
        }
        else if (WP.tags.Contains(InventoryTag.DarkMagicDamage))
        {
            T = DamageType.DarkMagic;
        }
        else if (WP.tags.Contains(InventoryTag.MagicDamage))
        {
            T = DamageType.Magic;
        }
        playerDamageType.text = T.ToSafeString();

      
      //  weaponDamagetext.text = player.playerDamage.ToString();
        
        // Now ability info 

        if (player.selectedAbilty == null)
        {
            // if no abilty equipped
            
           // abilityDamagetext.text= "--";
           // abilityRangetext.text = "--";
          //  abilityDescrip.text = "--";
          //  abilitynameText.text = "NA";
           // abilityCooldowntxt.text = "--";
        }
        else
        {
             extraInfo = player.selectedAbilty.extraData;
             startingIndexOfDamage = extraInfo.IndexOf("DAMAGE:");

             stringStartingWithDamageNum = extraInfo.Substring(startingIndexOfDamage+7, extraInfo.Length - startingIndexOfDamage-7);

             nextIndexOfSemiColon = stringStartingWithDamageNum.IndexOf(";");
        
             onlyDamageNum = stringStartingWithDamageNum.Substring(0,nextIndexOfSemiColon );
        
        
             withoutDamageNum = stringStartingWithDamageNum.Substring(nextIndexOfSemiColon+1,stringStartingWithDamageNum.Length - nextIndexOfSemiColon-1 );

             startOfRange = withoutDamageNum.IndexOf("RANGE:");
        
             rangeNum = withoutDamageNum.Substring(startOfRange + 6,withoutDamageNum.Length -startOfRange -6-1  );

          
             player.abilityDamage = int.Parse(onlyDamageNum);
             player.abilityRange = float.Parse(rangeNum);
             player.abilitycooldown = player.selectedAbilty.cooldown;

           //  abilityDamagetext.text = player.abilityDamage.ToString();
          //  abilityRangetext.text = player.abilityRange.ToString();
          //  abilityDescrip.text = player.selectedAbilty.Description;
           // abilitynameText.text = player.selectedAbilty.Name;
           // abilityCooldowntxt.text = player.abilitycooldown.ToString();
        }
        
        
        // Now ultimate info
        
        // Will always have the singular ultimate allowd equipped
        
        extraInfo = player.ultimate.extraData;
        startingIndexOfDamage = extraInfo.IndexOf("DAMAGE:");

        stringStartingWithDamageNum = extraInfo.Substring(startingIndexOfDamage+7, extraInfo.Length - startingIndexOfDamage-7);

        nextIndexOfSemiColon = stringStartingWithDamageNum.IndexOf(";");
        
        onlyDamageNum = stringStartingWithDamageNum.Substring(0,nextIndexOfSemiColon );
        
        
        withoutDamageNum = stringStartingWithDamageNum.Substring(nextIndexOfSemiColon+1,stringStartingWithDamageNum.Length - nextIndexOfSemiColon-1 );

        startOfRange = withoutDamageNum.IndexOf("RANGE:");
        
        rangeNum = withoutDamageNum.Substring(startOfRange + 6,withoutDamageNum.Length -startOfRange -6-1  );

        player.ultimateDamage = int.Parse(onlyDamageNum);
        player.ultimateRange = float.Parse(rangeNum);
        player.ulimateCooldown = player.ultimate.cooldown;

        // Additional stats page

        playerAttackRange.text = player.playerRange.ToString();
        playerMoveRange.text = player.movement.actionRange.ToString();
        playerPos.text = player.movement.playerPos.x.ToString() + "," + player.movement.playerPos.z.ToString();

        if (player.statuss.Count ==1)
        {
            playerStatuses.text = player.statuss[0].ToString();
        }
        else
        {
            playerStatuses.text = "";
            foreach (FreeEntity.StatusType S in player.statuss)
            {
                playerStatuses.text = playerStatuses.text + ", " + S.ToString();
            }
        }


    }

    public void ShowHideCombatInfoMenu()
    {
        if (!weaponSelectScroll.activeSelf)
        {
            playerCombatInfoPanel.SetActive(!playerCombatInfoPanel.activeSelf);
        }
        SetWeaponInfo();

        bool anytransforsUnlocked = false;

        foreach (Transformation T in player.possibleTransformations)
        {
            anytransforsUnlocked = T.unlocked;
            if (anytransforsUnlocked)
            {
                break;
            }
        }

        transformationButton.SetActive(anytransforsUnlocked);


    }

    public void UpdateInfoUI()
    {

        // Updates the info text and turns off the combat marker if the player clicks anywhere else 
      
        sleepS.value = player.sleep;
        sanityS.value = player.sanity;
        healthS.value = player.health;
        hungerS.value = player.hunger;
        tempS.value = player.temperature;

        sleepS.minValue = player.minmaxSleep.x;
        sleepS.maxValue = player.minmaxSleep.y;

        sanityS.minValue = player.minMaxSanity.x;
        sanityS.maxValue = player.minMaxSanity.y;

        hungerS.minValue = player.minMaxHunger.x;
        hungerS.maxValue = player.minMaxHunger.y;

        healthS.minValue = 0;
        healthS.maxValue = player.maxHealth;

        tempS.minValue = player.minMaxTemp.x;
        tempS.maxValue = player.minMaxTemp.y;

        // Change colouring

        if (player.sanity >= 7)
        {

            sanityBackground.color = greenbar;
        } else if (player.sanity >= 3)
        {
            sanityBackground.color = orangeBar;
        }
        else
        {
            sanityBackground.color = redBar;
        }

        if (player.hunger <=4)
        {

            hungerbackground.color = greenbar;        
        }
        else if (player.hunger<=8)
        {
            hungerbackground.color = orangeBar;
        }
        else
        {
            hungerbackground.color = redBar;
        }

        if (player.temperature >= 4 && player.temperature<=7)
        {
            tempBackground.color = greenbar;
        }
        else if (player.temperature < 4)
        {
            tempBackground.color = blueBar;
        }
        else
        {
            tempBackground.color = redBar;
        }


        if (player.sleep >=5)
        {
            sleepBackground.color = greenbar;
        }
        else if (player.sleep >=3)
        {
            sleepBackground.color = orangeBar;
        }
        else
        {
            sleepBackground.color = redBar;
        }

        if ( ((float)player.health/ (float)player.maxHealth)>.7f )
        {
            healthBackground.color = greenbar;
        }
        else if (((float)player.health / (float)player.maxHealth) > .3f)
        {
            healthBackground.color = orangeBar;
        }
        else
        {
            healthBackground.color = redBar;
        }


        if (combatOptions.activeSelf && !didShowCombatUI && (Input.GetMouseButtonDown(0)|| Input.GetMouseButtonDown(1)))
        {
            List<RaycastResult> res = (GetEventSystemRaycastResults());

            bool isOverCombatMarker = false;
            foreach (RaycastResult R in res)
            {
                if (R.gameObject.CompareTag("CombatUIInterface"))
                {
                    isOverCombatMarker = true;
                }
            }

            if (!isOverCombatMarker)
            {
                combatOptions.SetActive(false);
                selectedEnemy = null;
            }
            
        }
    }
    
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
    

    public void EXITGAME()
    {
        Application.Quit();
    }

    public void ShowLocalInventoryInteractScreen(LocalInventory inven)
    {
        // Shows the local (non player, ex chest) inventory screen to grab items
        localInventoryInteractScreen.SetActive(true);

        localInventoryInteractScreen.transform.Find("SourceName").GetComponent<TMP_Text>().text = inven.inventoryName;

        int i = 0;

        while (i < inven.containedItems.Count)
        {
            GameObject newItem = Instantiate(localInventoryPrototype, localInventoryItemsStartPos.position + Vector3.right * localInventoryItemPadding.x * (i%localInventoryColsNum) + Vector3.down * localInventoryItemPadding.y * ((float)Math.Floor( (float)i /(float)localInventoryColsNum)), localInventoryPrototype.transform.rotation);
            newItem.transform.SetParent(localInventoryItemsParent, true);
            newItem.transform.Find("Name").GetComponent<TMP_Text>().text = inven.containedItems[i].Name;
            newItem.name = inven.containedItems[i].uniqueID.ToString();
            newItem.transform.Find("ChestID").GetComponent<TMP_Text>().text = inven.sourceID.ToString();
            i++;
        }
    }

    public void CloseLocalInventoryInteractScreen()
    {


        foreach (Transform T in localInventoryItemsParent)
        {
            AddItemFromLocalInventoryToPlayerInventory(T.gameObject);
            Destroy(T.gameObject);
        }
        localInventoryInteractScreen.SetActive(false);
        
    }

    public void AddItemFromLocalInventoryToPlayerInventory(GameObject G)
    {

        GameObject source = GameObject.Find(G.transform.Find("ChestID").GetComponent<TMP_Text>().text);
        int itemID = int.Parse(G.name);

        if (source.GetComponent<Chest>())
        {
            List<InventoryItem> items = source.GetComponent<Chest>().items.containedItems;

            int i = 0;

            while (i < items.Count)
            {
                if (items[i].uniqueID == itemID)
                {
                    player.inventory.AddItemToInven(items[i], 1);
                    items.Remove(items[i]);
                    return;
                }
                i++;
            }
        }
      
    }
    
    public void SetCombatButton()
    {
        combatButton.gameObject.GetComponent<Image>().color = combatManage.playerIsInCombat ? Color.red : Color.green;
        //nextTurnButton.SetActive(combatManage.playerIsInCombat);
    }
    public void DebugChangeCombat()
    {
        combatManage.playerIsInCombat = !combatManage.playerIsInCombat;
    }

    public void OpenSmeltingPanel()
    {
        // Opens the smelting panel for use   
        
        craftingPanel.SetActive(true);

        GameObject first = null;
        int i = 0;
        
        foreach (CraftingReciepe RP in player.inventory.Reciepes)
        {

            if (RP.output.item.tags.Contains(InventoryTag.RequiresSmelting))
            {
                GameObject temp = Instantiate(craftRPProto, protoCraftParent, true);
                temp.transform.position = protoCraftParent.transform.position + Vector3.down * i * verticalDist;

                temp.transform.Find("Name").gameObject.GetComponent<TMP_Text>().text = RP.output.item.Name;

                if (player.inventory.CanCraft(RP))
                {
                    temp.GetComponent<Image>().color = canCraft;
                    temp.transform.Find("Craft").gameObject.GetComponent<Image>().color = canCraftB;
                    temp.transform.Find("Craft").gameObject.GetComponent<Button>().interactable=true;
                }
                else
                {
                    temp.GetComponent<Image>().color = cannotCraft;
                    // Button for crafting
                    temp.transform.Find("Craft").gameObject.GetComponent<Image>().color = cannotCraftB;
                    temp.transform.Find("Craft").gameObject.GetComponent<Button>().interactable=false;
                }
                
                temp.name = RP.output.item.ID.ToString();

                if (i == 0)
                {
                    first = temp;
                }
                i++;
            }
        }

        SelectRP(first);
        
    }
    public void OpenCraftingPanel(bool isTable)
    {
        // Opens the player crafting panel for use 
        
        craftingPanel.SetActive(true);
        doingAdvancedCrafting = isTable;

        GameObject first = null;
        int i = 0;
        
        foreach (CraftingReciepe RP in player.inventory.Reciepes)
        {

            if (isTable)
            {
                GameObject temp = Instantiate(craftRPProto, protoCraftParent, true);
                temp.transform.position = protoCraftParent.transform.position + Vector3.down * i * verticalDist;

                temp.transform.Find("Name").gameObject.GetComponent<TMP_Text>().text = RP.output.item.Name;

                if (player.inventory.CanCraft(RP))
                {
                    temp.GetComponent<Image>().color = canCraft;
                    temp.transform.Find("Craft").gameObject.GetComponent<Image>().color = canCraftB;
                    temp.transform.Find("Craft").gameObject.GetComponent<Button>().interactable = true;
                }
                else
                {
                    temp.GetComponent<Image>().color = cannotCraft;
                    // Button for crafting
                    temp.transform.Find("Craft").gameObject.GetComponent<Image>().color = cannotCraftB;
                    temp.transform.Find("Craft").gameObject.GetComponent<Button>().interactable = false;
                }

                temp.name = RP.output.item.ID.ToString();

                if (i == 0)
                {
                    first = temp;
                }
                i++;
            }
            else
            {
                if (RP.output.item.tags.Contains(InventoryTag.BasicItem))
                {
                    GameObject temp = Instantiate(craftRPProto, protoCraftParent, true);
                    temp.transform.position = protoCraftParent.transform.position + Vector3.down * i * verticalDist;

                    temp.transform.Find("Name").gameObject.GetComponent<TMP_Text>().text = RP.output.item.Name;

                    if (player.inventory.CanCraft(RP))
                    {
                        temp.GetComponent<Image>().color = canCraft;
                        temp.transform.Find("Craft").gameObject.GetComponent<Image>().color = canCraftB;
                        temp.transform.Find("Craft").gameObject.GetComponent<Button>().interactable = true;
                    }
                    else
                    {
                        temp.GetComponent<Image>().color = cannotCraft;
                        // Button for crafting
                        temp.transform.Find("Craft").gameObject.GetComponent<Image>().color = cannotCraftB;
                        temp.transform.Find("Craft").gameObject.GetComponent<Button>().interactable = false;
                    }

                    temp.name = RP.output.item.ID.ToString();

                    if (i == 0)
                    {
                        first = temp;
                    }
                    i++;
                }
            }

         
            
      
        }

        SelectRP(first);
    }

    public void SelectRP(GameObject G)
    {
        // Selects a recipe for use in crafting
        int ID = int.Parse(G.name);

        CraftingReciepe RP = null;
        foreach (CraftingReciepe RP1 in player.inventory.Reciepes)
        {
            if (RP1.output.item.ID == ID)
            {
                RP = RP1;
                break;
            }
        }

        if (RP == null)
        {
            Debug.LogError("Null item!!!:"+ G.name);
            return;
        }
        
        craftPanelInside.transform.Find("Name").gameObject.GetComponent<TMP_Text>().text = RP.output.item.Name;
        craftPanelInside.transform.Find("Description").gameObject.GetComponent<TMP_Text>().text = RP.output.item.Description;

        string recpie = RP.input[0].item.Name + " ( " + RP.input[0].count.ToString() + " )";
        
        int i = 1;
        while (i < RP.input.Count)
        {
            recpie += " | " + RP.input[i].item.Name + " ( " + RP.input[i].count.ToString() + " )";
            i++;
        }
        
        craftPanelInside.transform.Find("RP").gameObject.GetComponent<TMP_Text>().text = recpie;


        if (player.inventory.CanCraft(RP))
        {
          
            G.transform.Find("Craft").gameObject.GetComponent<Image>().color = canCraftB;
            G.transform.Find("Craft").gameObject.GetComponent<Button>().interactable=true;
        }
        else
        {
            G.transform.Find("Craft").gameObject.GetComponent<Image>().color = cannotCraftB;
            G.transform.Find("Craft").gameObject.GetComponent<Button>().interactable=false;
        }
        
    }

    public void CraftButtonPressed(GameObject G)
    {
        // Player presses crafting button to craft
        
        int ID = int.Parse(G.name);

        CraftingReciepe RP = null;
        foreach (CraftingReciepe RP1 in player.inventory.Reciepes)
        {
            if (RP1.output.item.ID == ID)
            {
                RP = RP1;
                break;
            }
        }

        if (RP == null)
        {
            Debug.LogError("Null item!!!:"+ G.name);
            return;
        }

        if (player.inventory.CraftItem(RP))
        {
            if (RP.output.item.tags.Contains(InventoryTag.RequiresSmelting))
            {
                CloseSmeltingPanel();
                OpenSmeltingPanel();
            }
            else if (doingAdvancedCrafting)
            {
                CloseCraftingPanel();
                OpenCraftingPanel(true);
            }
            else
            {
                CloseCraftingPanel();
                OpenCraftingPanel(false);
            }
          
           
        }

    }

    public void CloseCraftingPanel()
    {
        craftingPanel.SetActive(false);
        doingAdvancedCrafting = false;
        foreach (Transform T in protoCraftParent)
        {
            Destroy(T.gameObject);
        }
    }

    public void CloseSmeltingPanel()
    {
        
        craftingPanel.SetActive(false);

        foreach (Transform T in protoCraftParent)
        {
            Destroy(T.gameObject);
        }
    }

    public void OpenBackpack()
    {
        InventoryPanel.SetActive(true);
        GoToInventoryFromScrapbook();
       
        
    }

    public void GoToScrapbookfromInventory()
    {
        CloseInven();
        scrapbookPanel.SetActive(true);
        // Do everything to populate scrollbar
        
        
    }
    public void GoToInventoryFromScrapbook()
    {
        scrapbookPanel.SetActive(false);
        foreach (Transform T in scrapbookParent.transform)
        {
            Destroy(T.gameObject);
        }
        
        OpenInven();
    }

    public void CloseBackpack()
    {
        CloseInven();
        InventoryPanel.SetActive(false);
    }
    
    public void OpenInven()
    {
        
        // Player opens inventory 


        inventoryPanel.SetActive(true);
        List<int> countsUItems;

        List<InventoryItem> unquieItems = player.inventory.FindAllUnquieItems(out countsUItems);
        int i = 0;
        int I = 1;

        while (i < unquieItems.Count)
        {
            if (i >= invenSpaceCountY*I)
            {
                I++;
            }
            
            GameObject G = Instantiate(InvenBoxProto, startPosAndParent.position + Vector3.right *invenWidth * (i % invenSpaceCountX) + Vector3.down*(I-1)*invenHeight, InvenBoxProto.transform.rotation);
            G.transform.SetParent(startPosAndParent);
            G.transform.Find("ItemName").gameObject.GetComponent<TMP_Text>().text = unquieItems[i].Name;
            G.transform.Find("ItemNumber").Find("Number").GetComponent<TMP_Text>().text = countsUItems[i].ToString();

            i++;
        }
    }

    public void CloseInven()
    {

        foreach (Transform T in startPosAndParent)
        {
            Destroy(T.gameObject);
        }
        inventoryPanel.SetActive(false);
    }

    public void SelectHotBarItem(int number)
    {
        player.inventory.hotbarSlot = number;
        player.inventory.UpdatedInventory(false);
        
        UpdateHotBar();
        
    }

    public void UpdateHotBar()
    {
        // Updates the hotbar UI 
        
        selector.transform.position = inventorySlots[player.inventory.hotbarSlot].transform.position;

        int i = 0;

        while (i < inventorySlots.Length)
        {
            string displayName;
            
            if (player.inventory.hotbar[i]!=null && player.inventory.hotbar[i].item != null && player.inventory.hotbar[i].count!=0)
            {
                displayName = player.inventory.hotbar[i].item.Name;
            }
            else
            {
                displayName = "";
            }

            inventorySlots[i].transform.Find("Select").Find("Text").GetComponent<TMP_Text>().text = displayName;
            i++;
        }
        
    }
}

[System.Serializable]
public struct BlockingActionHolder
{
    public GameAction action;
    public GameObject associatedUI;

}