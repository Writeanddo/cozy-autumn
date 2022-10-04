using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMotherBear : Boss
{
    enum Stage { none, prepare, attacked }
    Stage stage;
    int prepareRound = 3;
    int attackRound = 2;
    List<int> dangeousIndices;

    public GameObject bossCell;
    EnemyCell enemyCell;
    public Vector3 originPosition;

    int saveBabyRequirement = 3;

    // Start is called before the first frame update
    void Start()
    {
        GridController.Instance.boss = this;
        enemyCell = bossCell.GetComponent<EnemyCell>();
        enemyCell.init("motherBear");
        originPosition = bossCell.transform.position;
        GetComponentInChildren<CounterDown>(true).gameObject.SetActive(false);

        // if saved 3 baby, let pass
        var allEnemies = GameObject.FindObjectsOfType<EnemyCell>();
        int babyCount = 0;
        foreach (var enemy in allEnemies)
        {

        }
        if (babyCount >= saveBabyRequirement)
        {
            happyEnd();
        }
    }

    public override void getKilled()
    {
        killEnd();
    }
    void happyEnd()
    {


        FindObjectOfType<AchievementManager>().ShowAchievement("happyBear");
        PopupManager.Instance.showEvent("You saved 3 baby bears and mom bear is happy, so she let you pass. Poison Forest unlocked.", () => { GameManager.Instance.restartGame(); }, "Restart");
        SFXManager.Instance.play("win");
    }

    void killEnd()
    {


        PopupManager.Instance.showEvent("You killed the mom bear and passed the forest. Grass Land unlocked", () => { GameManager.Instance.restartGame(); }, "Restart");
        SFXManager.Instance.play("win");
    }

    // Update is called once per frame
    void Update()
    {

    }

    float animTime = 0.3f;

    public override IEnumerator onNextStep()
    {
        yield return null;
        switch (stage)
        {
            case Stage.none:
                stage = Stage.prepare;
                //select dangeous area
                dangeousIndices = new List<int>() { 0, 1, 3, 4 };
                GridController.Instance.showDangerousCell("motherBear", dangeousIndices);
                count = prepareRound;
                break;
            case Stage.prepare:
                count--;
                if (count == 0)
                {
                    //start attack
                    //move boss grid to the center of dangerous cell
                    var position = GridController.Instance.getCenterOfCells(dangeousIndices);
                    bossCell.transform.DOMove(position, animTime);
                    yield return new WaitForSeconds(animTime);
                    //bossCell.transform.position = position;
                    bossCell.transform.Find("front").localScale *= 2;
                    bossCell.GetComponent<BoxCollider2D>().size *= 2;
                    //get attack of traps
                    foreach (var i in dangeousIndices)
                    {

                        yield return StartCoroutine(GridController.Instance.triggerTrapOnCell(i, enemyCell));
                    }

                    if (dangeousIndices.Contains(GridController.Instance.playerCellIndex))
                    {
                        //do attack on character

                        yield return StartCoroutine(GridController.Instance.attackAndMovePlayer(dangeousIndices, enemyCell));
                    }
                    else
                    {

                        // get attack from character
                        if (isNextToPlayer())
                        {

                            StartCoroutine(enemyCell.activeAttack(false, false));
                        }
                    }


                    GridController.Instance.hideDangerousCell("motherBear", dangeousIndices);
                    count = attackRound;
                    stage = Stage.attacked;
                }
                break;
            case Stage.attacked:
                count--;
                if (count == 0)
                {
                    //move back

                    bossCell.transform.Find("front").localScale /= 2;
                    bossCell.GetComponent<BoxCollider2D>().size /= 2;

                    bossCell.transform.DOMove(originPosition, animTime);
                    yield return new WaitForSeconds(animTime);

                    stage = Stage.none;
                }
                else
                {
                    if (isNextToPlayer())
                    {

                        StartCoroutine(enemyCell.activeAttack());
                    }
                }
                break;
        }

        if (count != 0)
        {
            GetComponentInChildren<CounterDown>(true).gameObject.SetActive(true);
            GetComponentInChildren<CounterDown>(true).initCount(count);
        }
        else
        {
            GetComponentInChildren<CounterDown>(true).gameObject.SetActive(false);

        }
    }

    bool isNextToPlayer()
    {
        foreach (var i in dangeousIndices)
        {
            if (GridController.Instance.isPlayerAround(i))
            {
                return true;
            }
        }
        return false;
    }
}