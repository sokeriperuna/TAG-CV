using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DeprecatedUtility
{
    /*
    private void SpawnTowersAtRandom(int n) {
        
        //var verticalSize   = (float)mainCamera.orthographicSize * 2.0f;
        //var horizontalSize = verticalSize * Screen.width / Screen.height;
        for (int i = 0; i<n; i++){
            Vector2 pos = RandomUnsaturatedPosition();//new Vector2(Random.Range(-horizontalSize/2+1,horizontalSize/2-1),Random.Range(-verticalSize/2+1,verticalSize/2-1));
            if(IsInSafeZone(pos) || !IsInPlayArea(pos)){
                i--;
            }
            else
                SpawnTowerAt(pos);
        }
    }*/

    /*private int SpawnTowersAtRandomCircle(float radius, float enemyFrequency){
        var verticalSize   = (float)mainCamera.orthographicSize * 2.0f;
        var horizontalSize = verticalSize * Screen.width / Screen.height;
        Vector3 center = RandomUnsaturatedPosition();

        int enemyCount = Mathf.RoundToInt(2f*Mathf.PI*radius/enemyFrequency);

        //Vector3 center = new Vector3(Random.Range(-10f, 10f), Random.Range(-5f, 5f), 0f);
        //Vector2[] positions = EnemyUtility.GenerateCirclePositions(center, Random.Range(Mathf.Max(2.5f,n/4), Mathf.Max(7.5f,n/2)), n, Random.Range(0, 2*Mathf.PI));
        Vector2[] positions = EnemyUtility.GenerateCirclePositions(center, radius, enemyCount, Random.Range(0, 2*Mathf.PI));

        List<Vector2> posList = positions.ToList<Vector2>();

        Vector2 c = mainCamera.transform.position;
        //posList.RemoveAll(n => !(n.x > c.x-horizontalSize/2+1.25f && n.x < c.x+horizontalSize/2-1 && n.y < c.y+verticalSize/2-1.25f && n.y > c.y-verticalSize/2-1.25f && !IsInSafeZone(n)));
        posList.RemoveAll(n => !(IsInPlayArea(n) && !IsInSafeZone(n)));
        foreach(var p in posList){
            if(!IsOverlapingMapElement(p, 1f))
                SpawnTowerAt(p);
        }

        return posList.Count;
    }*/
    
    /*
    // NOTE: I added additional randomness to this function. BR. Esko
    private int SpawnTowersAtRandomLine(int n) {

        List<Vector2> positions  = new List<Vector2>();
        var verticalSize   = (float)mainCamera.orthographicSize * 2.0f;
        var horizontalSize = verticalSize * Screen.width / Screen.height;

        var r = Random.Range(2f, 4f);
        //var pos1 = randomPosition();
        //var pos2 = randomPosition();

        var pos1 = RandomUnsaturatedPosition();
        var pos2 = RandomUnsaturatedPosition();

        Debug.DrawLine(pos1, pos2, Color.white,2f);


        var unit12 = (pos2-pos1).normalized;
        var length12 = (pos2-pos1).magnitude;
        if(length12/n < 2.5f){
            length12 = 2.5f * n;
        }
        
        var norm12 = new Vector2(unit12.y, -unit12.x)*r;

        for (int i = 0; i<n; i++){
            var multiplier = Mathf.Pow(-1, i) * Random.Range(0.5f, 1f);
            positions.Add(pos1+unit12*length12*i*(1f/n)*Random.Range(0.7f, 1f) + norm12*multiplier);
        }
        //Debug.Log("B: " + positions.Count.ToString());
        Vector2 c = mainCamera.transform.position;
        positions.RemoveAll(n => !(IsInPlayArea(n) && !IsInSafeZone(n)));

        foreach (Vector2 p in positions){
            if(!IsOverlapingMapElement(p, 1f))
                SpawnTowerAt(p);
        }


        //Debug.Log("A: " + positions.Count.ToString());

        return positions.Count;
    }*/

    /*
    towerList.RemoveAll( t => t == null);
    foreach(var t in towerList){
        t.currentTarget = null;
        t.GetComponentInChildren<Gun>().AddCooldown();
    }
    */
}
