using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    string bulletOwner; //Var to verify who shot the bullet
    int bulletDirection; //Var to check which direction the bullet should fly based on the owner's gun orientation
    float bulletSpeed; //Var to decide how fast the bullet will travel

    void Start()
    {
        //Start different thread to destroy bullet in the eventuality that the bullet wont hit anything for too long
        StartCoroutine(AutoDestroy());
    }

    // Update is called once per frame
    void Update()
    {
        //Move bullet based on the setted values
        this.transform.position += new Vector3(bulletSpeed * bulletDirection, 0, 0);
    }

    //Set bullet values
    public void SetBullet(string owner, int direction, float speed)
    {
        bulletOwner = owner;
        bulletDirection = direction;
        bulletSpeed = speed;
    }

    //Check bullet collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If player shot the bullet and the bullet hit an enemy play enemy animation
        if(bulletOwner.Equals("Player") && collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Animator>().SetBool("EnemyDead", true);
            Destroy(this.gameObject);
        }

        //If enemy shot the bullet and the bullet hit the player play animation
        if (bulletOwner.Equals("Enemy") && collision.gameObject.CompareTag("Player"))
        {
            //Player will dodge bullet if is ducking
            if (!collision.gameObject.GetComponent<Animator>().GetBool("IsDucking"))
            {
                collision.gameObject.GetComponent<Animator>().SetBool("IsDead", true);
                Destroy(this.gameObject);
            }
        }
    }

    //Method to destroy bullet after a short period of time if no collision occur
    IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(5f);
        Destroy(this.gameObject);
    }
}
