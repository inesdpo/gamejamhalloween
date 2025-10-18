using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent ai;
    public List<Transform> destinations;
    public Animator aiAnim;
    public float walkSpeed, chaseSpeed, minIdleTime, maxIdleTime, idleTime, sightDistance, catchDistance, chaseTime, minChaseTime, maxChaseTime, jumscareTime;
    public bool walking, chasing;
    public Transform player;

    private Transform currentDest;
    private Vector3 dest;
    private int randNum, randNum2;
    public int destinationAmount;
    public Vector3 raycastOffet;
    public string deathScene;

    void Start()
    {
        walking = true;
        randNum = Random.Range(0, destinationAmount);
        currentDest = destinations[randNum];
    }

    void Update()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + raycastOffet, direction, out hit, sightDistance))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                walking = false;
                StopCoroutine("stayIdle");
                StopCoroutine("chaseRoutine");
                StartCoroutine("chaseRoutine");
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("idle");
                aiAnim.SetTrigger("sprint");
                chasing = true;
            }
        }

        if (chasing == true)
        {
            dest = player.position;

            ai.destination = dest;
            ai.speed = chaseSpeed;

            if (ai.remainingDistance <= catchDistance)
            {
                player.gameObject.SetActive(false);
                aiAnim.ResetTrigger("sprint");
                aiAnim.SetTrigger("jumpscare");
                StartCoroutine(deathRoutine());
                chasing = false;
            }
        }

        if (walking)
        {
            dest = currentDest.position;
            ai.destination = dest;
            ai.speed = walkSpeed;

            if (ai.remainingDistance <= ai.stoppingDistance)
            {
                randNum2 = Random.Range(0, 2);
                if (randNum2 == 0)
                {
                    randNum = Random.Range(0, destinationAmount);
                    currentDest = destinations[randNum];
                }
                if (randNum2 == 1)
                {
                    aiAnim.ResetTrigger("walk");
                    aiAnim.SetTrigger("iddle");
                    ai.speed = 0;
                    StartCoroutine("stayIddle");
                    walking = false;
                }
            }
        }

        IEnumerator StayIdle()
        {
            idleTime = Random.Range(minIdleTime, maxIdleTime);
            yield return new WaitForSeconds(idleTime);
            walking = true;
            randNum = Random.Range(0, destinationAmount);
            currentDest = destinations[randNum];

            aiAnim.ResetTrigger("idle");
            aiAnim.SetTrigger("walk");
        }

        IEnumerator chaseRoutine()
        {
            chaseTime = Random.Range(minChaseTime, maxChaseTime);
            yield return new WaitForSeconds(chaseTime);
            walking = true;
            chasing = false;
            randNum = Random.Range(0, destinationAmount);
            currentDest = destinations[randNum];

            aiAnim.ResetTrigger("sprint");
            aiAnim.SetTrigger("walk");
        }
        IEnumerator deathRoutine()
        {
            yield return new WaitForSeconds(jumscareTime);
            SceneManager.LoadScene(deathScene);
        }
    }
}
