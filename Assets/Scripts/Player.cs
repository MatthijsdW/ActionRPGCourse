using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public int curHp;
    public int maxHp;

    public float moveSpeed;
    public float jumpForce;

    public float attackRange;
    public int damage;
    private bool isAttacking;

    public Rigidbody rig;
    public Animator anim;

    private void Update()
    {
        Move();

        if (Input.GetKeyDown(KeyCode.Space))
            Jump();

        if (Input.GetMouseButtonDown(0) && !isAttacking)
            Attack();

        if (!isAttacking)
            UpdateAnimator();
    }

    private void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 direction = transform.right * x + transform.forward * z;
        direction *= moveSpeed;
        direction.y = rig.velocity.y;

        rig.velocity = direction;
    }

    private void UpdateAnimator()
    {
        anim.SetBool("MovingForwards", false);
        anim.SetBool("MovingBackwards", false);
        anim.SetBool("MovingLeft", false);
        anim.SetBool("MovingRight", false);

        Vector3 localVel = transform.InverseTransformDirection(rig.velocity);

        if (localVel.z > 0.1f)
            anim.SetBool("MovingForwards", true);
        else if (localVel.z < -0.1f)
            anim.SetBool("MovingBackwards", true);
        else if (localVel.x > 0.1f)
            anim.SetBool("MovingRight", true);
        else if (localVel.x < -0.1f)
            anim.SetBool("MovingLeft", true);
    }

    private void Jump()
    {
        if (!CanJump())
            return;

        rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private bool CanJump()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.01f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 0.1f))
        {
            return hit.collider != null;
        }

        return false;
    }

    public void TakeDamage(int damageToTake)
    {
        curHp -= damageToTake;

        HealthBarUI.instance.UpdateFill(curHp, maxHp);

        if (curHp <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void Attack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack");

        Invoke("TryDamage", 0.7f);
        Invoke("DisableIsAttacking", 1.5f);
    }

    private void TryDamage()
    {
        Ray ray = new Ray(transform.position + transform.forward, transform.forward);
        RaycastHit[] hits = Physics.SphereCastAll(ray, attackRange, 1 << 8);

        foreach(RaycastHit hit in hits)
        {
            hit.collider.GetComponent<Enemy>()?.TakeDamage(damage);
        }
    }

    private void DisableIsAttacking()
    {
        isAttacking = false;
    }
}
