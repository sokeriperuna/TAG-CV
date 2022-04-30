using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CursorData {
    public CURSOR_TYPE type;
    public Sprite sprite;
}

// Old deprecated struct
/*
[System.Serializable]
public struct CursorData {
    public CURSOR_TYPE type;
    public Sprite tex;
    public Vector2 hotspot;
    public CursorMode mode;
}
*/

public enum CURSOR_TYPE{
    NORMAL, HIGHLIGHTING, UNAVAILABLE
}



public class CursorVisualController : MonoBehaviour
{
    public GameObject cursorObject;
    public CursorData[] cursors;

    private Camera cam;
    private SpriteRenderer cursorSpriteRenderer;

    private void Awake() {
        cursorSpriteRenderer = cursorObject.GetComponent<SpriteRenderer>();
        Cursor.visible = false;
        cam = Camera.main;
    }

    private void LateUpdate() {
        Vector2 mousePosInWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        cursorObject.transform.position = mousePosInWorld;
    }

    private void Start() {
        SetCursorTo(CURSOR_TYPE.NORMAL);
    }

    public void SetCursorTo(CURSOR_TYPE newType){
        foreach(CursorData c in cursors)
            if(c.type == newType) // Search for first instance of desired type of cursor and then apply it.
            {
                cursorSpriteRenderer.sprite = c.sprite;
                return;
            }
    }
}
