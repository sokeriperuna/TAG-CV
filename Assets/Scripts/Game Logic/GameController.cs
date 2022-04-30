using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TAG.utility;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using FMODUnity;
using UnityEditor;


[System.Serializable]
public struct SpecialUpgrade{
    public EntityUpgrade upgrade;
    public int weight;
}

[RequireComponent(typeof(FMODUnity.StudioBankLoader))]
[RequireComponent(typeof(FMODUnity.StudioEventEmitter))]
public class GameController : MonoBehaviour
{
    [SerializeField]
    private EventReference createFlareSfx;

    [SerializeField]
    private EventReference removeFlareSfx;

    protected StudioEventEmitter sfxEmitter;

    [Header("Difficulty Parameters")]
    public DifficultyPreset difficulty;
    private float diffCoff;
    private int currentCurrency;

    [Header("Start Stats")]
    public int initialTowers = 3;
    public int startingFlares = 2;
    public float speed = 5f;
    
    public float safePlayerRadius = 7.5f;
    public float viewGrowthFactor = 0.1f;

    private int maxFlares;
    private int currentFlares;
    private int upgradeResources;

    private Camera mainCamera;
    [Header("Prefabs")]
    public GameObject unit;
    public GameObject resourceRewardInfo;
    public GameObject resourcePickup;

    [Header("Upgrades")]
    public EntityUpgrade[] constantUpgrades;
    public SpecialUpgrade[] specialUpgrades;

    [Header("Miscellaneous")]
    public Transform startingNode;
    public List<Transform> pEntities = new List<Transform>();

    public static G_STATE currentPhase;

    [HideInInspector]public G_STATE gameState;

    public float objectSpacing = 1.5f;

    public float nodeVisualThickness = 1f; // figure out better place to define this variable

    public GUIController GUIController;
    public Transform UITopLeft;
    public Transform playAreaTopLeft;
    public Transform playAreaBottomRight;

    private CursorVisualController cursorVisual;

    private List<PathNode> nodePath;
    private List<Transform> nodeTransforms = new List<Transform>();

    private GameObject pathNodePrefab;
    private GameObject pathNodeVisualPrefab;

    private EntityUpgrade[] upcomingUpgrades;
    private bool[] upgradesPurchased;

    [SerializeField] private List<EntityUpgrade> upgradeHistory;

    private int round = 0;
    private int score = 0;

    private const float endPhaseButtonDelay = 0.65f;
    private float endPhaseButtonRefresh = 0f;

    private MusicController musicController;


    private void Awake() {

        #if UNITY_EDITOR
        string[] names = AssetDatabase.FindAssets("scanline");
        int i=0;
        foreach(string n in names){
            Debug.Log((i++).ToString() + ": " + n);
        }
        #endif

        musicController = GetComponent<MusicController>();
        cursorVisual = GetComponent<CursorVisualController>();

        mainCamera = Camera.main;

        TransformsToUnits();
        pathNodePrefab = Resources.Load<GameObject>("PathNode");
        pathNodeVisualPrefab = Resources.Load<GameObject>("PathNodeVisual");

        
        InitializeGame();
    

        GUIController.chosenUpgrade += OnChooseUpgrade;
        GUIController.endPhaseButton += OnEndPhaseButton;

        GUIController.newGameButton += RestartGame;
        GUIController.quitButton += Quit;
        
        Entity.entityDeath += OnEntityDeath;

        ResourcePickup.resourcePickup += OnResourcePickup;
    }

    private void Start(){
        /// We wait a frame to make sure the camera has initialized properly as the Towers' spawning is dependent on the camera's position.
        /// Using Awake and Start didn't work so this is an adequate workaround
        StartCoroutine(WaitAFrameAfterStart());
    }

    private IEnumerator WaitAFrameAfterStart(){
        yield return new WaitForEndOfFrame();
        StartPlanningPhase();
    } 

    private void OnDestroy() {
        GUIController.chosenUpgrade  -= OnChooseUpgrade;
        GUIController.endPhaseButton -= OnEndPhaseButton;

        GUIController.newGameButton -= RestartGame;
        GUIController.quitButton -= Quit;

        Entity.entityDeath -= OnEntityDeath;

        ResourcePickup.resourcePickup -= OnResourcePickup;
    }


    private void Update() {

        #if UNITY_EDITOR // Cheat code for debugging
        if(Input.GetKey(KeyCode.U) | Input.GetKeyDown(KeyCode.U))
            SceneManager.LoadScene("main");
        #endif

        // Check for inputs, act accordingly
        if(gameState == G_STATE.PLANNING_PHASE)
        {
            ProcessInput();
        }

        bool spaceInput = Input.GetKey(KeyCode.Space) | Input.GetKeyDown(KeyCode.Space);
        if(gameState == G_STATE.PLANNING_PHASE & spaceInput){
            EndPlanningPhase();
            return;
        }
    }

    /// Phases flow into each other. Each End[NAME1]Phase method triggers another Start[NAME2]Phase at the end.
    private void StartPlanningPhase() {
        musicController.EnterPlanningPhase();
        if(round != 0){
            GrowCameraPosition();
        }

        // Diff director actions
        UpdateDifficultyCoff();
        DifficultyDirectorIncome();
        SpawnNewTowers();

        gameState = G_STATE.PLANNING_PHASE;
        currentPhase = gameState;

        GUIController.UpdatePhaseText(gameState);
        GUIController.UpdateWaveText(round+1); // Game Controller counts rounds from 0 for math purposes, player counts up from 1.
    }

    private void EndPlanningPhase(){
        cursorVisual.SetCursorTo(CURSOR_TYPE.NORMAL);

        if(pEntities.Count == 0 | pEntities == null){
            Debug.LogError("no player units found!");
            EndGame();
            return;
        }
        StartExecutionPhase();
    }

    private void StartExecutionPhase(){
        musicController.EnterActionPhase();
        
        gameState = G_STATE.EXECUTION_PHASE;
        currentPhase = gameState;

        GUIController.UpdatePhaseText(gameState);

        StartCoroutine(IExecutionPhase());
    }

    void UpdateDifficultyCoff(){
        diffCoff = difficulty.parameters.startingDiffCoff
        *Mathf.Pow(difficulty.parameters.difficultyScalingFactor, round);
    }

    void DifficultyDirectorIncome(){
        Debug.Log("DD Income: " + Mathf.RoundToInt(difficulty.parameters.startingIncome*diffCoff).ToString());
        currentCurrency += Mathf.RoundToInt(difficulty.parameters.startingIncome*diffCoff);
    }
    
    private void SpawnNewTowers(){

        int initialCurrency = currentCurrency;
        int minCostDynamic = Mathf.RoundToInt(initialCurrency * difficulty.parameters.minCostCoff);
        int minCostCeil = difficulty.parameters.minCostCeiling;

        int purchasesMade = 0;

        if(difficulty == null){
            Debug.LogError("No difficulty preset found!");
            return;
        }
        else{

            const int pickupCap = 3;
            int spawnedPickups = 0;


            int exit=0;

            /// We iterate a finite number of times in order to generate new towers spawns.
            /// The finite loop is a safeguard against crashing
            while(exit < 1000){
                ++exit;

                // Check termination condition (we have too litle money to spend anymore)
                if(currentCurrency <= 0){
                    return;
                }

                SpawnFormation formation = ChooseRandomWeightedFormationFrom(difficulty.formations);

                
                SpawnElement core = ChooseRandomWeightedSpawnElementFrom(difficulty.coreElements);
                SpawnElement satellite = ChooseRandomWeightedSpawnElementFrom(difficulty.satelliteElements);

                if(spawnedPickups <= 0)
                {
                    foreach(SpawnElement coreE in difficulty.coreElements)
                        if(coreE.type == SPAWN_TYPE.PICKUP){
                            core = coreE;
                            break;
                        }
                }

                satellite = ChooseRandomWeightedSpawnElementFrom(difficulty.satelliteElements); /// Is this necessary? I don't remember... {Comment by Esko}

                /// CHECK EXCEPTION RULES

                if(core.cannotSpawnAlone & formation.type == FORMATION_TYPE.SINGLE)
                {
                    --exit;
                    continue;
                }

                if(core.type == SPAWN_TYPE.PICKUP & spawnedPickups >= pickupCap){
                    --exit;
                    continue;
                }

                if(formation.numberOfSatellites <=2 & satellite.type == SPAWN_TYPE.FENCE_TOWER){
                    --exit;
                    continue;
                }

                if(core.type == SPAWN_TYPE.FENCE_TOWER & formation.type == FORMATION_TYPE.ARC & formation.numberOfSatellites <=2){
                    --exit;
                    continue;
                }

                
                float finalMultiplier = formation.costMultiplier*core.costMultiplier*satellite.costMultiplier; 
                int finalCost = Mathf.RoundToInt(finalMultiplier*(core.cost+satellite.cost*formation.numberOfSatellites));

                // Check if cost is too expensive
                if(finalCost > currentCurrency)
                    continue;
                
                //Check if cost is too cheap. (this leads to expensive towers being favored over time)
                if(finalCost <= Mathf.Clamp(minCostDynamic, 0, minCostCeil)){
                    continue;
                }

                // Try to find a suitable position
                Vector2 outputPos;
                if(!FindSuitablePosition(formation, out outputPos)){
                    continue; // No suitable pos found, we go back to try
                }

                float orientationAngle = GetOrientation(formation, outputPos);

                Vector2[] fPos = GetFormationPositions(formation, outputPos, orientationAngle);

                // Check if any positions generated are outside play

                if(fPos == null){
                    Debug.LogError("NULL FORMATIONS POSITIONS\n"+"FORMATION TYPE: " + formation.type.ToString());
                    continue;
                }

                // Check if any positions generated are outside play
                bool posOutsidePlayArea = false;

                foreach(Vector2 p in fPos){
                    if(!IsInPlayArea(p, 0.7f) | IsInSafeZone(p)){
                        posOutsidePlayArea = true;
                        continue;
                    }
                }

                if(posOutsidePlayArea){
                    --exit;
                    continue; // We reject formations with positions outside the play area
                }


                if(core.type == SPAWN_TYPE.PICKUP)
                    spawnedPickups++;

                SpawnFormationObjects(formation, fPos, core, satellite);

                Debug.Log("Type: "+formation.type.ToString() + '\n' + "Core: " + core.type.ToString() + '\n' + "Satellite: " + satellite.type.ToString());
                Debug.Log("Core: "+core.prefab.name + '\n' +"Satellite: " + satellite.prefab.name);

                // Deduct suitable cost from currency                
                currentCurrency -= finalCost;
                purchasesMade++;

                if(difficulty.parameters.minCostCoff <=0f){
                    break;
                }
            }
            if(exit>=1000)
                Debug.Log("Spawning iteration limit reached.");
        }

        if(purchasesMade == 0){
            Debug.LogError("Round: " + round.ToString() + "\nNo purchases made.");
            return;
        }

        int av = (initialCurrency - currentCurrency)/purchasesMade; // Calculate average difficulty director purchase cost (DEBUG/BALANCE PURPOSES)
        Debug.Log("Round: " + round.ToString() + "\nAVERAGE COST: " + av.ToString());
    }

    private bool FindSuitablePosition(SpawnFormation f, out Vector2 position){
        if(f == null){
            Debug.LogError("NULL SPAWN FORMATION.");
            position = Vector2.zero;
            return false;
        }

        position = Vector2.zero;

        switch(f.type){
                case FORMATION_TYPE.SINGLE:
                position = RandomUnsaturatedPosition();
                return true;

                case FORMATION_TYPE.FULL_SHIELDED:
                case FORMATION_TYPE.PARTIAL_SHIELDED:
                case FORMATION_TYPE.ARC:
                position = RandomUnsaturatedPosition();
                RaycastHit2D[] hits = Physics2D.CircleCastAll(position, f.arcRadius+0.5f, Vector2.zero);
                foreach(RaycastHit2D hit in hits){
                    if(hit.collider.CompareTag("Tower") | hit.collider.CompareTag("Pickup"))
                        return false;
                }
                return true;                    

                default:

                Debug.LogError("UNKNOWN OR UNIMPLEMENTED FORMATION TYPE: " + f.type.ToString());

                return false;
            }
    }

    /// Returns signed angle
    private float GetOrientation(SpawnFormation f, Vector2 fPos){
        if(f == null){
            Debug.LogError("NULL SPAWN FORMATION.");
            return 0f;
        }

        switch(f.orientation){
            
            case ORIENTATION_TYPE.NONE:
            return 0f;  

            case ORIENTATION_TYPE.RANDOM:
            return Random.Range(-180f, 180f);

            case ORIENTATION_TYPE.TOWARD_PLAYER_START:

            Vector2 pPos = nodeTransforms[0].position;
            Vector2 dir = (pPos-fPos);
            float outputAngle = Vector2.SignedAngle(Vector2.right, dir);

            //Debug.Log("pPos: " + pPos.ToString() + "\nfPos: " + fPos.ToString());
            //Debug.Log("Angle: " + outputAngle.ToString() + "\ndir: " + dir.normalized.ToString());

            return outputAngle;

            default:
                Debug.LogError("UNKNOWN OR UNIMPLEMENTED ORIENTATION TYPE: " + f.type.ToString());

            return 0f;
        }
    }

    private Vector2[] GetFormationPositions(SpawnFormation f, Vector2 pos, float orientationAngle){
        if(f == null){
            Debug.LogError("NULL SPAWN FORMATION.");
            return null;
        }

        /// First index of the output array is the core tower.
        /// The rest are satellites
        List<Vector2> output = new List<Vector2>();
        Vector2[] satPos;

        switch(f.type){
            case FORMATION_TYPE.SINGLE:
                output.Add(pos);
                return output.ToArray();

            case FORMATION_TYPE.FULL_SHIELDED:
                output.Add(pos);
                satPos = EnemyUtility.GenerateCirclePositions(pos, f.arcRadius, f.numberOfSatellites, orientationAngle);
                foreach(Vector2 p in satPos)
                    output.Add(p);
                return output.ToArray();

            case FORMATION_TYPE.PARTIAL_SHIELDED:
                output.Add(pos);
                satPos = EnemyUtility.GenerateCircleArcPositions(pos, f.arcRadius, f.numberOfSatellites, f.arcLength, orientationAngle);
                foreach(Vector2 p in satPos)
                    output.Add(p);
                return output.ToArray();

            case FORMATION_TYPE.ARC:
                if(f.numberOfSatellites%2 == 1){
                    Debug.LogError("Odd number of satellites for ARC formation.");
                    return null;
                }

                satPos = EnemyUtility.GenerateCircleArcPositions(pos, f.arcRadius, f.numberOfSatellites+1, f.arcLength, orientationAngle);
                int corePosIndex = f.numberOfSatellites/2; // Formula derived for getting middle index
                output.Add(satPos[corePosIndex]);
                for(int i = 0; i<satPos.Length; i++){
                    if(i != corePosIndex)
                        output.Add(satPos[i]);
                }

                return output.ToArray();
            default:
                Debug.LogError("UNKNOWN OR UNIMPLEMENTED FORMATION TYPE: " + f.type.ToString());
            return null;
        }
    }

    private void SpawnFormationObjects(SpawnFormation f, Vector2[] positions, SpawnElement core, SpawnElement sat){
        if(f == null){
            Debug.LogError("NULL SPAWN FORMATION.");
            return;
        }

        GameObject coreObj;
        GameObject[] satellites;
        switch(f.type){
            case FORMATION_TYPE.FULL_SHIELDED:
                coreObj = Instantiate(core.prefab, positions[0], Quaternion.identity);
                if(core.type == SPAWN_TYPE.PICKUP){
                    ResourcePickup p = coreObj.GetComponent<ResourcePickup>();
                    p.SetResourceReward(CalculateResourcePickupReward());
                }

                satellites = new GameObject[positions.Length-1];

                for(int i=1; i<positions.Length;i++){
                    satellites[i-1] = Instantiate(sat.prefab, positions[i], Quaternion.identity);
                }

                if(sat.type == SPAWN_TYPE.FENCE_TOWER)
                {
                    int l = satellites.Length;
                    for(int i=0; i<l; i++){
                        if((i%2)==0)
                        {
                            FenceTower ft1;
                            FenceTower ft2;
                            if(i<(l-1))
                            {
                                ft1 = satellites[i].GetComponent<FenceTower>();
                                ft2 = satellites[i+1].GetComponent<FenceTower>();
                            }
                            else
                            {
                                ft1 = satellites[i].GetComponent<FenceTower>();
                                ft2 = satellites[0].GetComponent<FenceTower>();
                            }
                            if(ft1 == null | ft2 == null){
                                Debug.LogError("ERROR CONNECTING FENCE TOWERS");
                                continue;
                            }

                            ft1.AddConnectedTower(ft2 as Tower);
                        }
                    }
                }
                break;

            case FORMATION_TYPE.SINGLE:
                    coreObj = Instantiate(core.prefab, positions[0], Quaternion.identity);
                    if(core.type == SPAWN_TYPE.PICKUP){
                        ResourcePickup p = coreObj.GetComponent<ResourcePickup>();
                        p.SetResourceReward(CalculateResourcePickupReward());
                    }
                break;

            case FORMATION_TYPE.PARTIAL_SHIELDED:
                coreObj = Instantiate(core.prefab, positions[0], Quaternion.identity);
                if(core.type == SPAWN_TYPE.PICKUP){
                    ResourcePickup p = coreObj.GetComponent<ResourcePickup>();
                    p.SetResourceReward(CalculateResourcePickupReward());
                }

                satellites = new GameObject[positions.Length-1];
                for(int i=1; i<positions.Length;i++){
                    satellites[i-1] = Instantiate(sat.prefab, positions[i], Quaternion.identity);
                }

                if(sat.type == SPAWN_TYPE.FENCE_TOWER)
                {
                    int l = satellites.Length-1;
                    for(int i=0; i<l; i++){
                        FenceTower ft1 = satellites[i].GetComponent<FenceTower>();
                        FenceTower ft2 = satellites[i+1].GetComponent<FenceTower>();
                        
                        if(ft1 == null | ft2 == null){
                            Debug.LogError("ERROR CONNECTING FENCE TOWERS");
                            continue;
                        }

                        ft1.AddConnectedTower(ft2 as Tower);
                    }
                }
                break;

            case FORMATION_TYPE.ARC:
                coreObj = Instantiate(core.prefab, positions[0], Quaternion.identity);
                if(core.type == SPAWN_TYPE.PICKUP){
                    ResourcePickup p = coreObj.GetComponent<ResourcePickup>();
                    p.SetResourceReward(CalculateResourcePickupReward());
                }

                satellites = new GameObject[positions.Length-1];
                for(int i=1; i<positions.Length;i++){
                    satellites[i-1] = Instantiate(sat.prefab, positions[i], Quaternion.identity);
                }

                if(sat.type == SPAWN_TYPE.FENCE_TOWER)
                {
                    int half = satellites.Length/2;
                    int l = satellites.Length-1;
                    for(int i=0; i<l; i++){
                        half--;
                        if(half == 0){
                            continue; // We're right before the middle, so we don't connect.
                        }

                        FenceTower ft1 = satellites[i].GetComponent<FenceTower>();
                        FenceTower ft2 = satellites[i+1].GetComponent<FenceTower>();
                        
                        if(ft1 == null | ft2 == null){
                            Debug.LogError("ERROR CONNECTING FENCE TOWERS");
                            continue;
                        }

                        ft1.AddConnectedTower(ft2 as Tower);
                    }
                }
                break;

            default:                
                Debug.LogError("UNKNOWN OR UNIMPLEMENTED FORMATION TYPE: " + f.type.ToString());
                break;
        }
    }

    private SpawnFormation ChooseRandomFormation(){
        int index = Random.Range(0, difficulty.formations.Length);
        return difficulty.formations[index];
    }

    private SpawnFormation ChooseRandomWeightedFormationFrom(SpawnFormation[] fArray){
        int sum = 0;
        foreach(SpawnFormation sf in fArray){
            sum += sf.weight;
        }

        int r = Random.Range(1, sum+1);
        int currentCeiling = 0;
        for(int i=0; i<fArray.Length; i++)
        {
            currentCeiling += fArray[i].weight;
            if(r<=currentCeiling)
                return fArray[i];
        }

        Debug.Log("Error picking random weighted spawn formation!");
        return null;
    }

    private SpawnElement ChoosenRandomSpawnElementFrom(SpawnElement[] seArray){
        int index = Random.Range(0, seArray.Length);
        return seArray[index];
    }

    private SpawnElement ChooseRandomWeightedSpawnElementFrom(SpawnElement[] seArray){
        int sum = 0;
        foreach(SpawnElement se in seArray){
            sum += se.weight;
        }

        int r = Random.Range(1, sum+1);
        int currentCeiling = 0;
        for(int i=0; i<seArray.Length; i++)
        {
            currentCeiling += seArray[i].weight;
            if(r<=currentCeiling)
                return seArray[i];
        }

        Debug.Log("Error picking random weighted spawning element!");
        return new SpawnElement();
    }

    private void EndExecutionPhase(){
        round ++;
        RemoveAllNodesExceptStart(); // We remove all nodes after the execution phase.
        CleanUpPlayerEntityList();
        HealToMaxAll();
        StartUpgradePhase();
    }

    private void StartUpgradePhase(){
        musicController.EnterUpgradePhase();
        gameState = G_STATE.UPGRADE_PHASE;
        currentPhase = gameState;

        GUIController.ShowUpgradePanels(true);
        GUIController.UpdatePhaseText(gameState);
        GUIController.UpdateFlareCounter(maxFlares, maxFlares);

        upcomingUpgrades = GenerateUpcomingSpecialUpgrades();

        UpdateUpgradeUI();
    }

    private void EndUpgradePhase(){
        GUIController.ShowUpgradePanels(false);
        GUIController.UpdateFlareCounter(currentFlares, maxFlares);
        StartPlanningPhase();
    }

    void OnEndPhaseButton(){

        if(Time.time < endPhaseButtonRefresh){
            Debug.Log("Tried to press 'end phase' button too early.");
            return;
        }
        else{
            endPhaseButtonRefresh = Time.time + endPhaseButtonDelay;

            if(gameState == G_STATE.PLANNING_PHASE){
                EndPlanningPhase();
            }

            if(gameState == G_STATE.UPGRADE_PHASE){
                EndUpgradePhase();
            }
        }
    }


    void UpdateUpgradeUI(){
        for(int i=0; i<constantUpgrades.Length; i++){
            //upcomingUpgrades[i] = debugExampleUpgrade;
            int uCost = CalculateUpgradeCost(constantUpgrades[i]);
            GUIController.DisplayConstantUpgrade(constantUpgrades[i].UIData, i, uCost);
        }

        for(int i=0; i<upcomingUpgrades.Length; i++){
            //upcomingUpgrades[i] = debugExampleUpgrade;
            int uCost = CalculateUpgradeCost(upcomingUpgrades[i]);
            GUIController.DisplaySpecialUpgrade(upcomingUpgrades[i].UIData, i, uCost, upgradesPurchased[i]);
        }
    }

    void UpdateConvoyInfoUI(){
        GameObject sampleUnit = pEntities[0].gameObject; 
        Entity e = sampleUnit.GetComponent<Entity>();

        //Debug.Log("Firerate: " + e.GetGunFirerate().ToString());

        GUIController.UpdateConvoyInfo(pEntities.Count, e.hitpoints, e.GetGunDamage(), e.GetGunFirerate(), this.speed);
    }

    // This applies to common upgrades.
    void OnChooseUpgrade(int upgradeIndex){
        //Debug.Log("U: " + upgradeIndex.ToString());

        EntityUpgrade chosenUpgrade;
        if(upgradeIndex<constantUpgrades.Length){
            chosenUpgrade=constantUpgrades[upgradeIndex];
        }
        else{
            chosenUpgrade = upcomingUpgrades[upgradeIndex-constantUpgrades.Length];
        }

        int cost =  CalculateUpgradeCost(chosenUpgrade);
        if(upgradeResources-cost<0){
            FMODUnity.RuntimeManager.PlayOneShot("event:/UI_sounds/Unvalid_click");
            return;
        }

        if(chosenUpgrade==null){
            Debug.LogError("Chosen upgrade is null!");
            return;
        }
        else
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/UI_sounds/Purchase_sound");
            for(int i=0; i<upcomingUpgrades.Length; i++){
                if(upcomingUpgrades[i] == chosenUpgrade && upgradesPurchased[i] == true){
                    return;
                }
            }

            upgradeResources -= cost;
            for(int i=0; i<upcomingUpgrades.Length; i++){
                if(upcomingUpgrades[i] == chosenUpgrade){
                    upgradesPurchased[i] = true;
                    break;
                }
            }



            Debug.Log((chosenUpgrade.type).ToString());

            ApplyExternalUpgrade(chosenUpgrade);
            foreach(Transform pEntity in pEntities)
                pEntity.GetComponent<Entity>().Upgrade(chosenUpgrade); // Upgrade each player entity


            Debug.Log((chosenUpgrade.type).ToString());
            upgradeHistory.Add(chosenUpgrade);
            Debug.Log(chosenUpgrade.name + " picked!");
            

            
            GUIController.UpdateResourceText(upgradeResources);
            GUIController.UpdateFlareCounter(currentFlares, maxFlares);
            UpdateConvoyInfoUI();
            UpdateUpgradeUI();
        }
    }

    void OnResourcePickup(int rCount){
        upgradeResources += rCount;

        if(upgradeResources < 0){
            upgradeResources = 0;
        }

        GUIController.UpdateResourceText(upgradeResources);
    }

    void OnEntityDeath(Entity e){
        if(e is Tower){
            score++;
            GUIController.UpdateScoreText(score);

            if((e as Tower).deathResourceReward>0){
                upgradeResources += (e as Tower).deathResourceReward;
                GUIController.UpdateResourceText(upgradeResources);

                GameObject rInfo = Instantiate(resourceRewardInfo, e.transform.position, Quaternion.identity);
                rInfo.GetComponent<ResourceRewardInfo>().DisplayResourceAmount((e as Tower).deathResourceReward);
            }
        }

        if(e is Attacker)
        {
            // Check if all the player units are dead
            int pCount=0; // how many player units are alive?
            for(int j=0; j<pEntities.Count; j++){
                if(pEntities[j] != null)
                    if(pEntities[j].gameObject != null)
                    pCount++;
            }
            pCount--; // remove the unit that just died

            GUIController.UpdateConvoyInfo(pCount, e.hitpoints, e.GetGunDamage(), e.GetGunFirerate(), this.speed);


            // No player references means the whole convoy has been destroyed
            if(pCount<=0){
                EndGame();
            }
        }
    }


    EntityUpgrade[] GenerateUpcomingSpecialUpgrades(){

        EntityUpgrade[] upgrades = new EntityUpgrade[2];
        do{
            upgrades[0]=PickRandomWeightedUpgrade();
            upgrades[1]=PickRandomWeightedUpgrade();
        }while(upgrades[0]==upgrades[1]&&specialUpgrades.Length>2);

        upgradesPurchased = new bool[2];
        for(int i=0; i<upgradesPurchased.Length; i++){
            upgradesPurchased[i] = false;
        }

        return upgrades;
    }

    int CalculateResourcePickupReward(){
        int reward=0;
        reward = Mathf.RoundToInt(round*0.7f+1)*10;

        return reward;
    }

    int CalculateUpgradeCost(EntityUpgrade u){

        float cost = 0;


        int existingUpgrades = 0; 
        switch(u.type){
            case UPGRADE_TYPE.BASIC:
            foreach(EntityUpgrade instance in upgradeHistory){
                if(u == instance)
                    existingUpgrades++;
            }
            break;

            case UPGRADE_TYPE.SPECIAL:
            foreach(EntityUpgrade instance in upgradeHistory){
                if(instance.type == UPGRADE_TYPE.SPECIAL)
                    existingUpgrades++;
            }
            break;

            default:
            Debug.LogError("UNKOWN/UNIMPLEMENTED UPGRADE TYPE.");
            break;
        }


        // New integrated cost scaling
        cost = u.startingCost*(u.costGain*Mathf.Pow(existingUpgrades, 2)+1);
        return 5*Mathf.RoundToInt(cost/5f);

        // Exponential cost gain
        //cost = u.startingCost*Mathf.Pow(u.costGain, existingUpgrades);

        // Integrated scaling
        //return Mathf.RoundToInt(1+Mathf.Pow(i, 2)/2f+3*i)*u.startingCost;
        
        // Geometric cost gain: starting cost*costGain^(n-1)
        //return Mathf.RoundToInt((u.startingCost*Mathf.Pow(2f, (float)(i))));

        // Exponential Alt.
        //return Mathf.RoundToInt((u.startingCost*Mathf.Pow(u.costGain, (float)(i)))/5f)*5;

        // Linear increase in cost over purchases.
        //return u.costGain*i+u.startingCost; 
    }

    EntityUpgrade PickRandomWeightedUpgrade(){
        int sum = 0;
        foreach(SpecialUpgrade su in specialUpgrades){
            sum += su.weight;
        }

        int r = Random.Range(1, sum+1);
        int currentCeiling = 0;
        for(int i=0; i<specialUpgrades.Length; i++)
        {
            currentCeiling += specialUpgrades[i].weight;
            if(r<=currentCeiling)
                return specialUpgrades[i].upgrade;
        }

        return new EntityUpgrade();
    }

    private bool IsInSafeZone(Vector3 p){
        return ((Vector2)nodeTransforms[0].position-(Vector2)p).SqrMagnitude()<Mathf.Pow(safePlayerRadius,2);
    }

    private bool IsOverlapingMapElement(Vector2 position, float radius){
        RaycastHit2D[] hits = Physics2D.CircleCastAll(position, radius, Vector2.zero);
        foreach(RaycastHit2D hit in hits){
            if(hit.collider.CompareTag("Tower") || hit.collider.CompareTag("Pickup") ||hit.collider.CompareTag("Fence"))
                return true;
        }
        
        return false;
    }

    Vector2 GetRandomPosInCameraView(float borderMargin=0f){

        Vector2 playAreaSize = GetPlayAreaSize();
        var verticalSize   = playAreaSize.y;
        var horizontalSize = playAreaSize.x;

        return  GetPlayAreaCenter() + 
            new Vector2(Random.Range(-horizontalSize/2+borderMargin, horizontalSize/2-borderMargin),
                        Random.Range(-verticalSize  /2+borderMargin,   verticalSize/2-borderMargin));
    }


    // Try to get a position with the least amount of towers around
    private Vector2 RandomUnsaturatedPosition(bool unsaturated=true, int randomSamples=40){

        Vector2 unsaturatedPos = new Vector2();
        int saturation = unsaturated ? int.MaxValue : -1;

        int i = 0;
        do{
            Vector2 pos = GetRandomPosInCameraView(1.5f);
            if(!IsInSafeZone(pos)){
                Collider2D[] collliders = Physics2D.OverlapBoxAll(pos, new Vector2(7.5f, 7.5f), 0f);
                int s = 0;
                foreach(Collider2D coll in collliders)
                    if(coll.CompareTag("Tower")){
                        s++;
                    }

                if(unsaturated?(s<saturation):(s>saturation)){
                    saturation=s;
                    unsaturatedPos = pos;
                }
                
                if(saturation==0 && unsaturated)
                    return unsaturatedPos;

                i++;
                }
            } while(i<randomSamples);

            //Debug.Log("saturation: " + saturation.ToString());
            return unsaturatedPos;
    }

    IEnumerator IExecutionPhase() {
 
        float progress = 0f;
        float maxProgress = nodePath[nodePath.Count-1].cumulativeLength + (pEntities.Count-1)*objectSpacing;
        float length = nodePath[nodePath.Count-1].cumulativeLength;

        while(progress < maxProgress) {
            progress += Time.fixedDeltaTime*speed;
            
            for(int i=0; i<pEntities.Count; i++) {
                if(pEntities[i]== null )
                    continue;

                // Config position
                Vector3 newPos = PathUtility.PositionAlongPath(progress-i*objectSpacing, nodePath);

                // Config rotation
                Quaternion newRot = new Quaternion();
                newRot.eulerAngles = new Vector3(0f, 0f, newPos.z);
                pEntities[i].rotation = newRot;
                newPos.z=0f;

                var entityProgress = Mathf.Clamp(progress-i*objectSpacing, 0f, length);
                if(entityProgress > 0 && entityProgress < length && !pEntities[i].gameObject.activeSelf) {
                    pEntities[i].gameObject.SetActive(true);
                }
                if(entityProgress >= length && pEntities[i].gameObject.activeSelf && pEntities[i] != null) {
                    pEntities[i].gameObject.SetActive(false);
                }
                pEntities[i].position = newPos;
            }

            yield return new WaitForFixedUpdate();
        }
        if(gameState != G_STATE.LOSE_STATE)
            EndExecutionPhase();
    }

    void InitializeGame(){
        score = round = upgradeResources = 0;
        currentFlares = maxFlares = startingFlares;

        upgradeHistory = new List<EntityUpgrade>();

        if(nodeTransforms.Count > 0) // Make sure nodeTransforms is clear.
            nodeTransforms.RemoveRange(0, nodeTransforms.Count);
        
        nodeTransforms.Add(startingNode);
        nodePath = GeneratePathNodes(nodeTransforms);

        // Config GUI
        GUIController.ShowUpgradePanels(false);
        GUIController.UpdateScoreText(score);
        GUIController.UpdateResourceText(upgradeResources);
        GUIController.UpdateFlareCounter(currentFlares, maxFlares);

        GUIController.ShowGameOverPanel(false);

        UpdateConvoyInfoUI();
        PositionCamera();

        Vector2 pos;
        do{
            pos = RandomUnsaturatedPosition(true, 40);
        }
        while(IsOverlapingMapElement(pos, 1f));
    }

    void EndGame(){
        Debug.Log("Game over!");
        gameState = G_STATE.LOSE_STATE;

        GUIController.ShowUpgradePanels(false);
        GUIController.ShowGameOverPanel(true);

        GUIController.SetGameOverInfo(round, score);
    }

    void CleanUpPlayerEntityList(){
        var aliveEntities = new List<Transform>();
        foreach (var e in pEntities) {
            if(e != null)
                aliveEntities.Add(e);
        }
        pEntities = aliveEntities;
    }

    void HealToMaxAll() {
        foreach (var e in pEntities) {
            e.gameObject.GetComponent<Entity>().HealToMax();
        }
    }

    void ApplyExternalUpgrade(EntityUpgrade u){
        // Implement flare boost
        this.speed += u.speedBoost;
        if(speed<=0f)
            speed = 0.5f;

        for(int i=0; i<u.unitBoost; i++){
            if(u.unitPrefab != null)
                AddUnitToConvoy(u.unitPrefab);
        }

        maxFlares += u.flareBoost;
        if(maxFlares<=0)
            maxFlares = 1;

        currentFlares = maxFlares;


        GUIController.UpdateFlareCounter(currentFlares, maxFlares);

        //Debug.Log (u.UIData.upgradeName + " applied.");
    }

    void AddUnitToConvoy(GameObject unitPrefab){
        GameObject a = Instantiate(unitPrefab, startingNode.position, Quaternion.identity);
        a.SetActive(false);
        pEntities.Add(a.transform);
        
        Entity e = a.GetComponent<Entity>();
        
        // When adding a new unit we retroactively apply all past upgrades to it
        foreach(EntityUpgrade u in upgradeHistory)
            e.Upgrade(u);

    }

    void RemoveUnitFromConvoy(){
        Destroy(pEntities[pEntities.Count-1].gameObject);
        pEntities.RemoveAt(pEntities.Count-1);
    }
    
    private void CreateNode(Vector2 pos){

        if(currentFlares <= 0){
            return;
        }
        else
        {
            // SFX
            RuntimeManager.PlayOneShot(createFlareSfx,Vector3.zero);

            currentFlares--;
            GUIController.UpdateFlareCounter(currentFlares, maxFlares);

            GameObject nodeObject = Instantiate(pathNodePrefab, pos, Quaternion.identity);
            Transform newNode = nodeObject.transform;
            nodeTransforms.Add(newNode);
            nodePath = GeneratePathNodes(nodeTransforms);
        }

    }

    private void RemoveNode(Transform nodeForRemoval){

        // SFX
        RuntimeManager.PlayOneShot(removeFlareSfx, Vector3.zero);

        currentFlares++;
        GUIController.UpdateFlareCounter(currentFlares, maxFlares);

        int deletionIndex = nodeTransforms.IndexOf(nodeForRemoval);
        if(deletionIndex==0) // We can't remove the starting node.
            return;

        nodeTransforms.RemoveAt(deletionIndex);
        GameObject.Destroy(nodeForRemoval.gameObject);

        nodePath = GeneratePathNodes(nodeTransforms); // Convert GeneratePathNodes to return List<PathNode>
    }

    private void RemoveAllNodesExceptStart(){

        if(nodeTransforms.Count == 0){
            Debug.LogError("No nodes to remove!");
            return;
        }

        Transform start = nodeTransforms[0];

        for(int i=1; i<nodeTransforms.Count; i++)
            GameObject.Destroy(nodeTransforms[i].gameObject);
        
        nodeTransforms = new List<Transform>();
        nodeTransforms.Add(start);

        // Update node path.
        nodePath = GeneratePathNodes(nodeTransforms);

        currentFlares = maxFlares;
    }


    void TransformsToUnits() {
        for(int i = 0; i < pEntities.Count; i++)
        {
            var t = pEntities[i].position;
            Destroy(pEntities[i].gameObject);
            pEntities[i] = Instantiate(unit,t, Quaternion.identity).transform;
            pEntities[i].gameObject.SetActive(false);
        }
    }

    /// First transform in array is assumed to be the start. Last connection is made from the last node to the start.
    /// srcT is an array of transforms that we use to generate the Path Nodes
    private List<PathNode> GeneratePathNodes(List<Transform> srcT){

        PathNode[] newPath = new PathNode[srcT.Count+1];

        foreach(Transform t in srcT){
            ClearPathVisual(t);
        }


        // Generate first node
        {
            newPath[0].pos = srcT[0].position;
            newPath[0].dirFromPrev = Vector2.zero;
            newPath[0].lengthFromPrev   = 0;
            newPath[0].cumulativeLength = 0;
        }

        // Generate inbetween nodeTransforms
        for(int i=1; i<srcT.Count; i++){
            newPath[i].pos = srcT[i].position;
            newPath[i].dirFromPrev = (srcT[i].position - srcT[i-1].position).normalized;
            newPath[i].lengthFromPrev = (srcT[i].position - srcT[i-1].position).magnitude;
            newPath[i].cumulativeLength += (newPath[i-1].cumulativeLength+newPath[i].lengthFromPrev);

            GeneratePathVisual(srcT[i-1], srcT[i]);
            }

        // Generate last node
        {
            int last = srcT.Count; // last index
            newPath[last].pos = newPath[0].pos;
            newPath[last].dirFromPrev = (srcT[0].position - srcT[last-1].position).normalized;
            newPath[last].lengthFromPrev = (srcT[0].position - srcT[last-1].position).magnitude;
            newPath[last].cumulativeLength += (newPath[last-1].cumulativeLength+newPath[last].lengthFromPrev);

            if(srcT.Count>2) // We avoid akward incidents with overlapping path visuals between 2 nodes
                GeneratePathVisual(srcT[last-1], srcT[0]);
        }

        List<PathNode> newPathList = new List<PathNode>(newPath);

        return newPathList; // We work with arrays then return a list.
    }

    private void GeneratePathVisual (Transform start, Transform end) {
        GameObject visual = Instantiate(pathNodeVisualPrefab, end);
        visual.GetComponent<PathNodeVisualController>().GeneratePathQuad(start.position, end.position, nodeVisualThickness);
    }

    private void ClearPathVisual(Transform node){
        foreach(Transform child in node.GetComponentsInChildren<Transform>())
            if(child.CompareTag("PathNodeVisual"))
                Destroy(child.gameObject);
    }

    private void ProcessInput() {

        #if UNITY_EDITOR
        // Debug function
        if(Input.GetKeyDown(KeyCode.O))
            {
                GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
                foreach(GameObject t in towers)
                {
                    t.GetComponent<Entity>().Die();
                }
                GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
                foreach(GameObject p in pickups)
                {
                    Destroy(p);
                }

                EndExecutionPhase();
                EndUpgradePhase();
            }

        if(Input.GetKey(KeyCode.R) || Input.GetKeyDown(KeyCode.R))
            {
                upgradeResources += 1000000;
                GUIController.UpdateResourceText(upgradeResources);
            }
            
        #endif

        ProcessMouseInput();
    }


    private void ProcessMouseInput(){

        // We first check if we're not hoeving over any UI elements

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        
        Vector2 mPosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);


        //Debug.Log("Mouse is in playarea: " + IsInPlayArea(mPosInWorld).ToString());

        Collider2D result = Physics2D.OverlapPoint(mPosInWorld);
        if(result == null)
            cursorVisual.SetCursorTo(CURSOR_TYPE.NORMAL);
        else
            switch(result.tag)
            {
                case "Tower":
                cursorVisual.SetCursorTo(CURSOR_TYPE.UNAVAILABLE);
                break;

                case "Pickup":
                cursorVisual.SetCursorTo(CURSOR_TYPE.UNAVAILABLE);
                break;

                case "Node":
                cursorVisual.SetCursorTo(CURSOR_TYPE.HIGHLIGHTING);
                break;

                default:
                cursorVisual.SetCursorTo(CURSOR_TYPE.NORMAL);
                break;
            }

        foreach(RaycastResult r in results){
            if(r.gameObject.GetComponent<UnityEngine.UI.Button>() != null){
                cursorVisual.SetCursorTo(CURSOR_TYPE.HIGHLIGHTING);
                return; // Were howering over a button, so we don't process mouse input
            }
        }

        if(!IsInPlayArea(mPosInWorld)){
            return;
        }


        // 0=left, 1=right, 2=middle
        for(int b=0; b<2; b++)
        {
            if(Input.GetMouseButton(b) && !Input.GetMouseButtonDown(b))
                {
                    ProcessMouseDrag(b);
                    return;
                }

            if(Input.GetMouseButtonDown(b))
                {
                    ProcessMouseDown(b);
                    return;
                }

            if(Input.GetMouseButtonUp(b))
                {
                    ProcessMouseUp(b);
                    return;
                }
        }
    }

    private void ProcessMouseDown(int buttonIndex){
        Cursor.visible = false;

        //Debug.Log("Mouse Down: " + buttonIndex.ToString());

        Vector2 mPosInWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        switch(buttonIndex)
        {
            case 0: // Left-mouse
            Collider2D result = Physics2D.OverlapPoint(mPosInWorld); /// Implement better solution later

            if(result == null & gameState == G_STATE.PLANNING_PHASE)
                CreateNode(mPosInWorld);
            else if(result.CompareTag("Node") & gameState == G_STATE.PLANNING_PHASE && result.transform != nodeTransforms[0])
                RemoveNode(result.transform); // Remove node
            else
                FMODUnity.RuntimeManager.PlayOneShot("event:/UI_sounds/Unvalid_click");
            break;

            case 1:


            break;

            case 2:

            break;

            default:

            break;
        }
    }

    private void ProcessMouseDrag(int buttonIndex){

        //Debug.Log("Dragging: " + buttonIndex.ToString());
        switch(buttonIndex)
        {
            case 0:

            break;

            case 1:

            break;

            case 2:

            break;

            default:

            break;
        }
    }

    private void ProcessMouseUp(int buttonIndex){

        //Debug.Log("Mouse Up: " + buttonIndex.ToString());

        switch(buttonIndex)
        {
            case 0:

            break;

            case 1:

            break;

            case 2:

            break;

            default:

            break;
        }
    }

    void GrowCameraPosition(){
        mainCamera.orthographicSize += viewGrowthFactor;
        PositionCamera();
    }

    // We position the came so that the play area is in the center and GUI overlay is on the side
    void PositionCamera(){
        Vector2 camSizeInWorld = GetCamSizeInWorld();
        Vector2 playAreaSize   = GetPlayAreaSize();

        Vector3 newCamPos = nodeTransforms[0].position + new Vector3(Mathf.Abs(camSizeInWorld.x-playAreaSize.x)/-2f, playAreaSize.y/2f, 0f);

        mainCamera.transform.position = new Vector3(newCamPos.x, newCamPos.y, mainCamera.transform.position.z);
    }

    Vector2 GetCamSizeInWorld(){
       return new Vector2(Mathf.Abs(UITopLeft.position.x-playAreaBottomRight.position.x), Mathf.Abs(UITopLeft.position.y-playAreaBottomRight.position.y));
    }

    Vector2 GetPlayAreaSize(){
        return new Vector2(Mathf.Abs(playAreaTopLeft.position.x-playAreaBottomRight.position.x), Mathf.Abs(playAreaTopLeft.position.y-playAreaBottomRight.position.y));
    }

    Vector2 GetPlayAreaCenter(){
        return ((Vector2)nodeTransforms[0].position+Vector2.up*0.5f*GetPlayAreaSize().y);
    }

    bool IsInPlayArea(Vector3 pos, float padding = 0f){


        Vector2 playAreaTL = playAreaTopLeft.position;
        Vector2 playAreaBR = playAreaBottomRight.position;
        bool output = ((playAreaTL.x+padding)<pos.x) && (pos.x<(playAreaBR.x-padding)) &&
                      ((playAreaTL.y-padding)>pos.y) && (pos.y>(playAreaBR.y+padding));

        return output;
    }

    private void RestartGame(){
        musicController.StopMusic();
        SceneManager.LoadScene("Main");
    }

    private void Quit(){
        musicController.StopMusic();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
        #elif UNITY_STANDALONE
            Application.Quit();
        #endif
    }

    private void DebugPlayAreaPos(Vector2 position){
        Debug.Log("Inside play area: " + IsInPlayArea(position, 0.7f).ToString());
        Debug.Log("Is in forbidden zone: " + (!IsInPlayArea(position, 0.7f) | IsInSafeZone(position)).ToString());
        bool condition =  (!IsInPlayArea(position, 0.7f) | IsInSafeZone(position));

        Color topLeft = condition ? Color.blue : Color.red;
        Color bottomRight = condition ? Color.magenta : Color.green;

        Debug.DrawLine(playAreaTopLeft.position, playAreaBottomRight.position, Color.yellow);
        Debug.DrawLine(playAreaTopLeft.position,position, topLeft);
        Debug.DrawLine(playAreaBottomRight.position,position, bottomRight);
    }

    
    private void DrawScreenBorders() {
        var verticalSize   = (float)mainCamera.orthographicSize*2.0f;
        var horizontalSize = verticalSize * Screen.width / Screen.height;
        var rect = new Rect(-horizontalSize/2, -verticalSize/2, horizontalSize, verticalSize);

        Vector3 cPos = mainCamera.transform.position;

        Debug.DrawLine(new Vector3(rect.x, rect.y) + cPos, new Vector3(rect.x + rect.width, rect.y ) + cPos,Color.green);
        Debug.DrawLine(new Vector3(rect.x, rect.y) + cPos, new Vector3(rect.x , rect.y + rect.height) + cPos, Color.red);
        Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y + rect.height) + cPos, new Vector3(rect.x + rect.width, rect.y) + cPos, Color.green);
        Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y + rect.height) + cPos, new Vector3(rect.x, rect.y + rect.height) + cPos, Color.red);
        
    }


}
