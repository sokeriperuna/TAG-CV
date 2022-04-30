using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TAG.utility;

[System.Serializable]
public struct UpgradePanel
{
    public UnityEngine.UI.Image upgradeIcon;
    public TMP_Text upgradeName;
    public TMP_Text upgradeDescription; 
    public TMP_Text costText;
}

public class GUIController : MonoBehaviour
    {

    // These names absolutely suck but I have no idea how to name them better
    public delegate void GUIButtonDelegate();
    public event GUIButtonDelegate endPhaseButton;

    public delegate void GUIUpgradeDelegate(int i);
    public event GUIUpgradeDelegate chosenUpgrade;

    public event GUIButtonDelegate quitButton;
    public event GUIButtonDelegate newGameButton;

    [Header("Gameplay UI")]
    public TMP_Text waveText;
    [HideInInspector]
    public TMP_Text phaseText;
    [HideInInspector]
    public TMP_Text scoreText;
    public TMP_Text resourceText;
    public TMP_Text flareText;

    [Header("Unit Info UI")]
    public TMP_Text unitCountText;
    public TMP_Text hitpointsText;
    public TMP_Text damageText;
    public TMP_Text firerateText;
    public TMP_Text speedText;

    [Header("Upgrade UI")]
    public GameObject UpgradePanelsParent;

    [SerializeField]
    public UpgradePanel[] constantUpgradePanels;

    [SerializeField]
    public UpgradePanel[] specialUpgradePanels;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverInfo;

    /// buttons call this function with their index to communicate which upgrade was chosen
    public void ChooseUpgrade(int upgradeIndex){
        Debug.Log("Index : " + upgradeIndex.ToString() + " chosen!");
        if(chosenUpgrade != null)
            chosenUpgrade(upgradeIndex);
    }

    public void EndPhaseButton(){
        if(endPhaseButton != null)
            endPhaseButton();
    }

    public void QuitButton(){
        if(quitButton != null)
            quitButton();
    }

    public void NewGameButton(){
        if(newGameButton != null)
            newGameButton();
    }

    public void UpdateFlareCounter(int currentFlares, int maxFlares){
        flareText.text = ": " + currentFlares.ToString() + '/' + maxFlares.ToString();
    }

    public void DisplayConstantUpgrade(UpgradeUI UI, int index, int cost = 0){
        if(index >= constantUpgradePanels.Length | index < 0){
            Debug.LogError("Upgrade panel " + index.ToString() + " does not exist.");
        }
        else{
            // set visuals for upgrade panel
            constantUpgradePanels[index].upgradeName.text        = UI.upgradeName;
            constantUpgradePanels[index].costText.text = "COST: " + cost.ToString();
        }
    }

    
    // Currently hard-coded to display 3 upgrades max
    public void DisplaySpecialUpgrade(UpgradeUI UI, int index, int cost = 0, bool purchased=true){
        if(index >= constantUpgradePanels.Length | index < 0){
            Debug.LogError("Upgrade panel " + index.ToString() + " does not exist.");
        }
        else{
            // set visuals for upgrade panel
            specialUpgradePanels[index].upgradeIcon.sprite      = UI.upgradeIcon;
            specialUpgradePanels[index].upgradeName.text        = UI.upgradeName;
            specialUpgradePanels[index].upgradeDescription.text = UI.upgradeDescription;

            if(!purchased)
                specialUpgradePanels[index].costText.text = "COST: " + cost.ToString();
            else
                specialUpgradePanels[index].costText.text = "SOLD";
        }
    }

    public void UpdatePhaseText(G_STATE state){
        if(phaseText==null)
            return;

        string stateName = "";
        switch(state){

            case G_STATE.PLANNING_PHASE:
            stateName = "planning phase";
            break;

            case G_STATE.EXECUTION_PHASE:
            stateName = "execution phase";
            break;

            case G_STATE.UPGRADE_PHASE:
            stateName = "upgrade phase";
            break;

            default:
            break;
        }

        phaseText.text = stateName;
    }

    public void UpdateConvoyInfo(int ships, int hitpoints, int damage, float firerate, float speed){
        unitCountText.text = "SHP: "+ ships.ToString();
        hitpointsText.text = "HIT: "+ hitpoints.ToString();
        damageText.text    = "DMG: "+ damage.ToString();
        firerateText.text  = "RTE: "+ firerate.ToString();
        speedText.text     = "SPD: "+ speed.ToString();

    }

    public void UpdateWaveText(int waveNumber){
        waveText.text = "WAVE:<color=#66D955>" + waveNumber.ToString();
    }

    public void UpdateScoreText(int newScore){
        if(scoreText == null)
            return;
        scoreText.text = "score: " + newScore.ToString();
    }

    public void UpdateResourceText(int newResourceCount){
        resourceText.text = ": " + newResourceCount.ToString();
    }

    public void ShowUpgradePanels(bool b) {
        UpgradePanelsParent.SetActive(b);
    }

    public void ShowGameOverPanel(bool b) {
        gameOverPanel.SetActive(b);
    }

    public void SetGameOverInfo(int wave, int score){
        if(gameOverInfo == null)
            return;
        gameOverInfo.text = "Wave: "  +  wave.ToString() + '\n'
                          + "Score: " + score.ToString();
    }
}
