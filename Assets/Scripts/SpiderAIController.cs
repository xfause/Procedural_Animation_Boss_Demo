using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAIController : MonoBehaviour
{
    public CreatureController SpiderObject;
    public GameObject Player;

    [Header("Health")]
    public GameObject spiderHealthBar;
    public float initHealth = 100f;
    public float health = 100f;
    private float initHealthBarWidth = 0f;
    private RectTransform healthRectTransform;

    [Header("Attack")]
    public float attackRange = 5f;

    [Header("Charge")]
    public bool canCharge = true;
    public bool isCharging = false;
    public float lastChargeTime = 0f;
    public float chargeCoolDown = 5f;
    public float chargeRange = 35f;
    public float chargeDuration = 3f;
    public float chargeWalkSpeed = 8f;
    public float chargeRotateSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        healthRectTransform = spiderHealthBar.transform as RectTransform;
        initHealthBarWidth = healthRectTransform.rect.width;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 distVect = new Vector3(SpiderObject.transform.position.x - Player.transform.position.x, 0f, SpiderObject.transform.position.z - Player.transform.position.z);
        float distance = distVect.magnitude;
        if (distance < attackRange && isCharging == false)
        {
            Attack();
        }
        if (distance >= attackRange && isCharging == false)
        {
            Move();
        }
        if (distance > chargeRange || isCharging == true)
        {
            Charge();
        }
    }

    public void Move()
    {
        Vector3 targetOnSpiderPlane = new Vector3(Player.transform.position.x, SpiderObject.transform.position.y, Player.transform.position.z);
        SpiderObject.MoveToPos(targetOnSpiderPlane, Player);
    }

    public void Attack()
    {
        // SpiderObject.transform.position += new Vector3(0, 0, 8);

    }

    public void Charge()
    {
        Vector3 targetOnSpiderPlane = new Vector3(Player.transform.position.x, SpiderObject.transform.position.y, Player.transform.position.z);
        // start charge
        if (canCharge == true)
        {
            if (Time.time - lastChargeTime > chargeCoolDown)
            {
                Debug.Log("start charge");
                canCharge = false;
                isCharging = true;
                lastChargeTime = Time.time;
                SpiderObject.ChargeToPos(targetOnSpiderPlane, Player, chargeWalkSpeed, chargeRotateSpeed, chargeDuration);
            }
        }

        if (canCharge == false)
        {
            // charging
            if (Time.time - lastChargeTime < chargeDuration)
            {
                Debug.Log("charging");
                SpiderObject.ChargeToPos(targetOnSpiderPlane, Player, chargeWalkSpeed, chargeRotateSpeed, chargeDuration);
            }
            // end charge
            if (Time.time - lastChargeTime > chargeDuration)
            {
                Debug.Log("end charge");
                isCharging = false;
                lastChargeTime = Time.time;
                canCharge = true;
            }
        }
    }

    public void lifeChange(float change)
    {
        health += change;
        if (health <=0) {
            Dead();
        }
        float newWidth = initHealthBarWidth * health / initHealth;
        healthRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
    }

    public void Dead(){
        // todo
        Debug.Log("Spider dead");
    }
}
