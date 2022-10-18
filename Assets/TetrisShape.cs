using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TetrisShape : MonoBehaviour
{

    List<Vector2> tetrisShape;
    List<GameObject> tetrises = new List<GameObject>();

    List<Vector2> tetrisShapeAfterRotation;
    int rotateTime = 0;
    bool isDragging = false;
    bool isUnlocked = false;
    Vector3 dragOriginalPosition;

    Vector2 currentFinalPosition;


    public void getReady()
    {
        isUnlocked = true;
    }

    public void init(List<Vector2> shape)
    {
        tetrisShape = new List<Vector2>(shape);

        tetrisShapeAfterRotation = new List<Vector2>(shape);
        for (int i = 0; i < tetrisShape.Count; i++)
        {
            var index = tetrisShape[i];
            var card = DeckManager.Instance.drawCard(false);
            var go = GridGeneration.Instance.generateCell(index+(Vector2)transform.position, card);
            go.transform.parent = transform;
            tetrises.Add(go);
            go.GetComponent<GridCell>().collider.enabled = false;
        }
    }



    // Start is called before the first frame update
    void Start()
    {

    }
    void clearGeneratedCombineResult()
    {

        foreach (var c in generatedCombineResult)
        {
            Destroy(c);
        }
        generatedCombineResult.Clear();
    }
    List<GameObject> generatedCombineResult = new List<GameObject>();
    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (isDragging)
            {
                var mousePosition = new Vector3(mousePos.x, mousePos.y, dragOriginalPosition.z);
                var newFinalPosition = new Vector2(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y));
                if (currentFinalPosition != newFinalPosition)
                {
                    currentFinalPosition = newFinalPosition;
                    transform.position = currentFinalPosition;
                    if (generatedCombineResult.Count != 0)
                    {
                        clearGeneratedCombineResult();
                    }
                    bool canPlaceCell = canPlace();
                    if (canPlaceCell)
                    {
                        //show how nearby cells would update
                        
                        StartCoroutine( GridGeneration.Instance.calculateCombinedResult(allCells(), generatedCombineResult));
                    }
                }

            }
        }
        

        if (Input.GetMouseButtonDown(1) && isUnlocked)
        {
            rotate90Degree();
            if (generatedCombineResult.Count != 0)
            {
                clearGeneratedCombineResult();
            }
            bool canPlaceCell = canPlace();
            if (canPlaceCell)
            {
                //show how nearby cells would update

                StartCoroutine(GridGeneration.Instance.calculateCombinedResult(allCells(), generatedCombineResult));
            }
        }
        //if (Input.GetMouseButtonDown(0))
        //{
        //    if (canPlaceCell)
        //    {

        //        tryPlace();
        //    }
        //}
    }

    List<GridCell> allCells()
    {
        List<GridCell> res = new List<GridCell>();
        foreach(var c in tetrises)
        {
            res.Add(c.GetComponent<GridCell>());

        }
        return res;
    }

    private void OnMouseDown()
    {

        if (!GridGeneration.Instance.canMoveCell())
        {
            return;
        }
        if (!isUnlocked)
        {
            return;
        }
        isDragging = true;
        dragOriginalPosition = transform.position;
    }
    private void OnMouseUp()
    {

        if (!isUnlocked)
        {
            return;
        }
        bool canPlaceCell = canPlace();

        if (canPlaceCell)
        {
            tryPlace();
            isUnlocked = false;
        }
        else
        {
            isDragging = false;

            transform.position = dragOriginalPosition;
            clearColor();
        }
    }


    Color getColor(bool isValid, bool isEnemy)
    {
        if(!isUnlocked || !isDragging)
        {

            return Color.white;
        }
        if (isValid)
        {

            return Color.white;
        }
        else
        {
            return Color.red;

        }
    }

    void clearColor()
    {
        foreach(var t in tetrises)
        {
            t.GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>().color =Color.white;
        }
    }

    bool canPlace()
    {
        bool res = true;
        bool surroundExistedCell = false;
        bool outOfBorder = false;
        for (int i = 0; i < tetrisShape.Count; i++)
        {
            var index = tetrisShapeAfterRotation[i] + currentFinalPosition;
            tetrises[i].GetComponent<GridCell>().index = index;

            if (Mathf.Abs(index.x )> GridGeneration.Instance.gridSize || Mathf.Abs( index.y) > GridGeneration.Instance.gridSize)
            {
                outOfBorder = true;

                tetrises[i].GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>().color = getColor(false, true);
                continue;
            }

            surroundExistedCell |= GridGeneration.Instance.isNextToOccupiedCells(index);

            if (GridGeneration.Instance.isOccupied(index))
            {
                res = false;
                tetrises[i].GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>().color = getColor(false,true);
            }
            else
            {
                if(tetrises[i] == null || tetrises[i].GetComponent<GridCell>() == null || tetrises[i].GetComponent<GridCell>().cellInfo == null || tetrises[i].GetComponent<GridCell>().bk == null || tetrises[i].GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>() == null)
                {
                    Debug.LogError("???");
                }
                tetrises[i].GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>().color = getColor(true, tetrises[i].GetComponent<GridCell>().cellInfo.isEnemy());
            }
        }

        if (!surroundExistedCell)
        {
            for (int i = 0; i < tetrisShape.Count; i++)
            {
                tetrises[i].GetComponent<GridCell>().bk.GetComponent<SpriteRenderer>().color = getColor(false, true);
            }
        }



        return res && surroundExistedCell && !outOfBorder;
    }

    void tryPlace()
    {
        clearGeneratedCombineResult();
        for (int i = 0; i < tetrisShape.Count; i++)
        {
            var index = tetrisShape[i];
            var go = tetrises[i];
            go.GetComponent<GridCell>().index = tetrisShapeAfterRotation[i] +currentFinalPosition;
            go.transform.localPosition = tetrisShapeAfterRotation[i];
            go.transform.parent = null;
        }


        GridGeneration.Instance.placeCells(tetrises);
        Destroy(gameObject);
        
    }
    void rotate90Degree()
    {
        rotateTime++;
        rotateTime %= 4;
        for (int i = 0; i < tetrisShape.Count; i++)
        {
            var index = tetrisShape[i];
            var go = tetrises[i];
            tetrisShapeAfterRotation[i] = RotatePointAroundPoint(index, Vector2.zero, rotateTime * 90);

            tetrises[i].GetComponent<GridCell>().index = index;
            go.transform.DOLocalMove(tetrisShapeAfterRotation[i], 0.3f);

            //go.transform.localPosition = RotatePointAroundPoint(index, Vector2.zero, rotateTime * 90);

        }
    }

    Vector3 RotatePointAroundPoint(Vector3 point1, Vector3 point2, float angle)
    {
        angle *= Mathf.Deg2Rad;
        var x = Mathf.Cos(angle) * (point1.x - point2.x) - Mathf.Sin(angle) * (point1.y - point2.y) + point2.x;
        var y = Mathf.Sin(angle) * (point1.x - point2.x) + Mathf.Cos(angle) * (point1.y - point2.y) + point2.y;
        return Vector3Int.RoundToInt( new Vector3 (x, y));
    }


}