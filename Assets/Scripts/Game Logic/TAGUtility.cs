using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TAG.utility
{
    [System.Serializable]
    public struct DifficultyPresetParameters{
        public float startingDiffCoff;
        public float difficultyScalingFactor;
        public float minCostCoff;
        public int minCostCeiling;
        public int startingIncome;
    }

    [System.Serializable]
    public enum SPAWN_TYPE {
        SHOOTING_TOWER, FENCE_TOWER,SUPPORT_TOWER, PICKUP 
    }

    [System.Serializable]
    public enum ORIENTATION_TYPE{
        NONE, TOWARD_PLAYER_START, RANDOM
    }

    [System.Serializable]
    public enum FORMATION_TYPE {
        SINGLE, DUO, PARTIAL_SHIELDED, FULL_SHIELDED, ARC
    }

    [System.Serializable]
    public struct PathNode{
        public Vector2 pos;
        public Vector2 dirFromPrev; 
        public float lengthFromPrev;
        public float cumulativeLength;
        public GameObject visualFromPrev;
    }

    [System.Serializable]
    public enum G_STATE { PLANNING_PHASE, EXECUTION_PHASE, UPGRADE_PHASE, LOSE_STATE };

    public static class PathUtility{

        public static Vector3 PositionAlongPath(float progress, List<PathNode> nodes){

            Vector3 foundPos = Vector3.zero;

            progress = Mathf.Clamp(progress, 0f, nodes[nodes.Count-1].cumulativeLength);

            for(int i=1; i<nodes.Count; i++){
                if(progress - nodes[i].cumulativeLength < 0f){
                    float progressToNode = (progress - nodes[i-1].cumulativeLength)/nodes[i].lengthFromPrev;
                    foundPos = Vector2.Lerp(nodes[i-1].pos, nodes[i].pos, progressToNode);

                    // Calculate rotaiton and pass it in z
                    float rot = Vector2.SignedAngle(Vector2.right, nodes[i].dirFromPrev);

                    foundPos.z = rot; //Mathf.Rad2Deg*Mathf.Atan(nodes[i].dirFromPrev.y/nodes[i].dirFromPrev.x);
                    return foundPos;
                }
            }

            return foundPos;
        }
    }

    public static class MathUtility {

        public static float EaseOutExp(float v1, float v2, float t){
        t = Mathf.Clamp(t, 0, 1);

        float progress = 1-Mathf.Pow(2,-10*t);;

        return v1*(1-progress)+v2*progress;
        }

        public static float SquareRootEaseOut(float v1, float v2, float t){
        t = Mathf.Clamp(t, 0, 1);

        float progress = Mathf.Sqrt(t);

        return v1*(1-progress)+v2*progress;
        }

        /// A sawthooth function going from 0 to 1
        public static float Sawtooth(float x, float period){
            return (x-0.5f)/period-Mathf.Floor(0.5f+(x-0.5f)/period)+0.5f;
        }
    }


    public static class EnemyUtility {
        public static Vector2[] GenerateCirclePositions(Vector2 center, float radius, int n, float angleOffset=0f){
            if(n<=0){
                Debug.LogError("Invalid amount of positions: " + n.ToString());
                return null;
            }
            else{
                Vector2[] positions = new Vector2[n];
                float segmentAngle = 2*Mathf.PI/n;
                for(int i=0; i<n; i++){
                    float angle = segmentAngle*i + angleOffset;
                    positions[i] = center + radius*(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
                }

                return positions;
            }
        }

        public static Vector2[] GenerateCircleArcPositions(Vector2 center, float radius, int n, float arcAngle=180f, float angleOffset=0f){
            if(n<=1){
                Debug.LogError("Invalid amount of positions: " + n.ToString());
                return null;
            }

            if(arcAngle<0f | arcAngle > 360f){
                Debug.LogError("Invalid amount of arc angle: " + arcAngle.ToString());
                return null;
            }
            
            float arcStart = angleOffset-arcAngle/2;
            float segmentAngle = arcAngle/(n-1);

            arcStart     *= Mathf.Deg2Rad;
            segmentAngle *= Mathf.Deg2Rad;

            Vector2[] output = new Vector2[n];
            for(int i=0; i<n; i++){
                float angle = segmentAngle*i + arcStart;
                output[i] = center + radius*(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
            }

            return output;
            
        }
    }

}