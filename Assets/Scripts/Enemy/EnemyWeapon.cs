using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public int attackDamage = 20;
    public float attackRange = 1f;
    public LayerMask playerLayers;

    public Transform attackPoint;

    public void Attack()
    {
        Collider2D colInfo = Physics2D.OverlapCircle(attackPoint.position, attackRange, playerLayers);
        Debug.Log("colInfo " + colInfo);
        if (colInfo != null)
        {
            colInfo.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
     {
        if (attackPoint == null)
        {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

}
