using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourceRewardInfo : MonoBehaviour
{
    float lifetime = 1f; 
    private TMP_Text resourceText;

    private void Awake() {
        resourceText = GetComponent<TMP_Text>();
    }

    public void DisplayResourceAmount(int r){
        resourceText.text = '+' + r.ToString();
        StartCoroutine(DisplayText());
    }

    IEnumerator DisplayText(){

        // animate stuff in the future?
        yield return new WaitForSeconds(lifetime);
        Destroy(this.gameObject);
    }
}
