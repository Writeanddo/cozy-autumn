using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pool;

public class GridCell : MonoBehaviour
{
    public int index;
    public SpriteRenderer renderer;

    public GameObject bk;

    public CellInfo cellInfo;

   public  CounterDown countDownObject;
    public string type;

    public string equipment = null;
    public SpriteRenderer equipRenderer;
    public GameObject ice;

    public Text hp;
    public GameObject newRuleAlert;
    public CanvasGroup descriptionCanvas;
    public bool isFreezed = false;

    public Text amountLabel;
    public int amount;
    public void updateHp(int x)
    {
        hp.text = x.ToString();
    }

    public void freeze()
    {
        isFreezed = true;
        if (!ice)
        {
            Debug.Log("??");
            return;
        }
        ice.SetActive(true);
    }
    public void thaw()
    {
        isFreezed = false;

        if (!ice)
        {
            Debug.Log("??");
            return;
        }
        ice.SetActive(false);
    }


    public  void init(string _type,int i,int _amount)
    {
        CellManager.Instance.showCell(_type);
        if (hp)
        {

            hp.text = "";
        }
        type = _type;
        cellInfo = CellManager.Instance.getInfo(type);
        renderer.sprite = Resources.Load<Sprite>("cell/" + type);
        index = i;
        bk.SetActive(cellInfo.isCell());

        if (cellInfo.isEnemy())
        {

            gameObject. AddComponent<EnemyCell>().init(type);
            bk.GetComponent<SpriteRenderer>().color = Color.red;
        }
        if(cellInfo.type == "fire")
        {
            gameObject.AddComponent<FireCell>();
            bk.GetComponent<SpriteRenderer>().color = new Color(1,0.5f, 0.5f);
        }

        if (_amount > 0)
        {
            amount = _amount;
        }
        else
        {
            if (cellInfo.categoryValue > 0)
            {
                amount = cellInfo.categoryValue;
            }
            else
            {
                amount = 1;
            }
        }
        updateAmount();
    }

    public void updateAmount()
    {
        if(amountLabel == null)
        {
            return;
        }
        if (amount > 1)
        {
            amountLabel.text = amount.ToString();
        }
        else
        {
            amountLabel.text = "";
        }
    }
    public void addAmount()
    {
        amount += 1;
        updateAmount();
    }
    public void decreaseAmount()
    {
        amount -= 1;
        updateAmount();
    }

    public void equip(string e)
    {
        equipment = e;
        equipRenderer.sprite = Resources.Load<Sprite>("cell/" + equipment);

        equipRenderer.transform.DOPunchScale(equipRenderer.transform.localScale*2, GridController.Instance.animTime);

        SFXManager.Instance.play("equipweapon");
    }

    public bool hasEquipment()
    {
        return equipment != null && equipment!="";
    }


    private void OnDestroy()
    {
        transform.DOKill();
        descriptionCanvas.DOKill();
    }
    public void unequip(Transform targetTransform)
    {
        if (equipment == null)
        {
            Debug.LogError("nothing to unequip");
        }

        var go = Instantiate(Resources.Load<GameObject>("flyingObject"), equipRenderer.transform.position, Quaternion.identity);
        go.transform.localScale = equipRenderer.transform.localScale;
        go.GetComponent<FlyingObject>().init(equipRenderer.sprite, targetTransform.position);

        equipment = null;
        equipRenderer.sprite = Resources.Load<Sprite>("cell/" + "empty");

    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        EventPool.OptIn("moveAStep", step);
    }

    public virtual void step()
    {
        var rule = GridController.Instance.combineResult(this);
        if (rule!=null && !RuleManager.Instance.visitedList.Contains(rule))
        {
            newRuleAlert.SetActive(true);
            descriptionCanvas.transform.Find("attackWhenNext").gameObject.SetActive(true);
            HintCell.generateHintText(descriptionCanvas.transform.Find("attackWhenNext"), "Move it now to unlock new rule!");
            descriptionCanvas.transform.Find("attackPerRound").gameObject.SetActive(false);
            descriptionCanvas.transform.Find("notNut").gameObject.SetActive(false);
            descriptionCanvas.transform.Find("Move").gameObject.SetActive(false);
            descriptionCanvas.transform.Find("Move2").gameObject.SetActive(false);
        }
        else
        {

            newRuleAlert.SetActive(false);
        }
    }

    bool isMouseDown = false;
    float mouseDownTime;
    float longPressTime = 0.3f;
    public virtual void OnMouseUp()
    {

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (GetComponent<Collider2D>().OverlapPoint(mousePosition))
        {
            if (Time.time - mouseDownTime > longPressTime)
            {

                //GridController.Instance.exploreCell(this);
                GridController.Instance.moveCell(this,true);

                ResourceManager.Instance.consumeResource("nut", 1, transform.position);
            }
            else
            {

                GridController.Instance.moveCell(this,false);
            }
        }







        isMouseDown = false;

        ExploreHoldText.text = "";
        progressBar.fillAmount = 0;
    }

    public Image progressBar;
    public Text ExploreHoldText;

    
    private void OnMouseOver()
    {
        Debug.Log("just over");
        if (isMouseDown)
        {
            
            progressBar.fillAmount = (Time.time - mouseDownTime) / longPressTime;
            if (progressBar.fillAmount >= 1)
            {
                progressBar.color = Color.black;
            }
            else
            {

                progressBar.color = Color.green;
            }
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (!GetComponent<Collider2D>().OverlapPoint(mousePosition))
            {

                ExploreHoldText.text = "Cancel";
               // progressBar.fillAmount = 0;
                return;
            }

            //update progress bar
        }
    }

    public virtual void OnMouseDown()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {

            Collider2D colliderHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (colliderHit != GetComponent<Collider2D>())
            {
                return;
            }

            //if (GetComponent<PlayerCell>())
            //{
            //    ResourceManager.Instance.consumeResource("nut", 1);
            //}
            if (isFreezed)
            {

                FindObjectOfType<AchievementManager>().ShowAchievement("freezeMove");
                failedToMove();
                return;
            }
            isMouseDown = true;
            mouseDownTime = Time.time;
            ExploreHoldText.text = "Move";
            //index = GridController.Instance.moveCellToEmpty(this);
            //  if(index == -1)
            //  {
            //      Destroy(gameObject);
            //  }
        }
    }
    public void failedToMove()
    {
        transform.DOKill();
        transform.DOShakePosition(0.3f, 0.3f, 20);

        SFXManager.Instance.play("negative");
    }

    private void OnMouseEnter()
    {
        if (newRuleAlert.active)
        {

            DOTween.To(() => descriptionCanvas.alpha, x => descriptionCanvas.alpha = x, 1, 0.3f);
        }

        //explainPanel.SetActive(true);

    }

    private void OnMouseExit()
    {
        DOTween.To(() => descriptionCanvas.alpha, x => descriptionCanvas.alpha = x, 0, 0.3f);

        //explainPanel.SetActive(false);
    }
}
