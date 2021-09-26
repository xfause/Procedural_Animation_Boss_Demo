using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackedDetector : MonoBehaviour
{
    public float initHealth = 100f;
    public float health = 100f;
    private Animator _animator;
    private PlayerMovementInputController _movement;

    public GameObject playerHealthBar;
    private float initHealthBarWidth = 0f;
    private RectTransform healthRectTransform;


    // Start is called before the first frame update
    void Start()
    {
        healthRectTransform = playerHealthBar.transform as RectTransform;
        initHealthBarWidth = healthRectTransform.rect.width;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<PlayerMovementInputController>();
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Spider_head" || collider.tag == "Spider_leg")
        {
            health -= 10f;
            float newWidth = initHealthBarWidth * health / initHealth;
            healthRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);

            if (health <= 0)
            {
                _animator.SetTrigger("IsDead");
                Dead();
            }
            Debug.Log("player health= " + health);
            if (_movement.aimValue == 1f)
            {
                _movement.aimValue = 0f;
                _animator.SetBool("IsAiming", false);
                _animator.Play("GetHit");
            }
            else
            {
                _animator.Play("GetHit");

            }
        }
    }
    // 接触结束
    void OnTriggerExit(Collider collider)
    {
        // Debug.Log("接触结束");
    }

    // 接触持续中
    void OnTriggerStay(Collider collider)
    {
        // Debug.Log("接触持续中");
    }

    public void Dead()
    {
        //todo;
        Debug.Log("Player dead");
    }
}
